//! Load a [`Stack`] from the working directory.

use crate::prelude::*;

/// Load a [`Stack`] from the working directory with `docker compose config`.
#[derive(FromServices)]
pub struct StackProvider {
    directory: Arc<WorkingDirectory>,
    compose_config: Arc<dyn ComposeConfigReader>,
}

impl StackProvider {
    /// Create a [`Stack`] from `docker compose config` in the [`WorkingDirectory`].
    ///
    /// - Reads the project JSON without interpolation
    /// - Takes the project name from top-level `name`
    /// - Parses `x-orbit` into [`OrbitSettings`], logging each warning at `warn`
    pub fn get(&self) -> Result<Stack, Report<StackError>> {
        let directory = self.directory.path();
        let project = self
            .compose_config
            .read()
            .change_context(StackError::Load)?;
        let name = project
            .get("name")
            .and_then(Value::as_str)
            .ok_or_else(|| Report::new(StackError::Name))?
            .to_owned();
        let stack = Stack::new(name, directory.to_path_buf(), project)
            .change_context(StackError::Settings)?;
        trace!(name = %stack.name, settings = ?stack.settings, "Loaded stack");
        for warning in &stack.settings.warnings {
            warn!("{warning}");
        }
        Ok(stack)
    }
}

/// Errors returned by [`StackProvider`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum StackError {
    /// Unable to load the Compose project.
    #[error("Unable to load the stack")]
    Load,
    /// The project has no name.
    #[error("Compose project has no name")]
    Name,
    /// Unable to read Orbit settings.
    #[error("Unable to read Orbit settings")]
    Settings,
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`StackProvider::get`] loads the project in the directory.
    #[test]
    fn stack_provider_get() {
        // Arrange
        let expected = Stack::mock();
        let mut compose_config = MockComposeConfigReader::new();
        compose_config
            .expect_read()
            .times(1)
            .returning(|| Ok(Stack::mock_project()));
        let provider = get_provider(compose_config, &Stack::mock_directory());
        // Act
        let stack = provider.get().expect("stack should load");
        // Assert
        assert_eq!(stack, expected);
    }

    /// [`StackProvider::get`] fails when Compose cannot load the project.
    #[test]
    fn stack_provider_get_compose_error() {
        // Arrange
        let mut compose_config = MockComposeConfigReader::new();
        compose_config
            .expect_read()
            .returning(|| Err(Report::new(ComposeConfigError::Config)));
        let provider = get_provider(compose_config, Path::new("/stack"));
        // Act
        let report = provider.get().expect_err("load should fail");
        // Assert
        assert_eq!(report.current_context(), &StackError::Load);
    }

    fn get_provider(
        compose_config: MockComposeConfigReader,
        directory: &Path,
    ) -> Arc<StackProvider> {
        ServiceBuilder::new()
            .with_mock_services(
                compose_config,
                MockComposeRunner::new(),
                MockSopsClient::new(),
            )
            .with_instance(WorkingDirectory::new(directory.to_path_buf()))
            .build()
            .expect::<StackProvider>()
    }
}
