//! Handler shared by the up and run subcommands.

use crate::prelude::*;

/// Prepare the stack and run a Compose subcommand on the Docker host.
#[derive(FromServices)]
pub struct ComposeHandler {
    deployments: Arc<DeploymentProvider>,
    runner: Arc<dyn ComposeRunner>,
}

impl ComposeHandler {
    /// Run Compose with `arguments` for the stack in the working directory.
    pub fn execute(&self, arguments: Vec<String>) -> Result<(), Report<ComposeHandlerError>> {
        trace!(?arguments, "Execute");
        let deployment = self
            .deployments
            .get()
            .change_context(ComposeHandlerError::Prepare)?;
        let invocation = deployment.to_invocation(arguments);
        self.runner
            .execute(invocation)
            .change_context(ComposeHandlerError::Compose)
    }
}

/// Errors returned by [`ComposeHandler`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum ComposeHandlerError {
    /// Unable to prepare the stack.
    #[error("Unable to prepare the stack")]
    Prepare,
    /// Compose failed.
    #[error("Compose failed")]
    Compose,
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`ComposeHandler::execute`] runs Compose with the arguments on the stack's host.
    #[test]
    fn compose_handler_execute() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_instance(WorkingDirectory::new(directory.path().to_path_buf()))
            .build();
        let handler = services.expect::<ComposeHandler>();
        let arguments = ["up", "--detach", "web"].map(str::to_owned).to_vec();
        // Act
        let output = handler.execute(arguments);
        // Assert
        assert!(output.is_ok(), "{output:?}");
        let calls = services.expect::<ComposeRunnerSpy>().calls();
        assert_eq!(calls.len(), 1);
        let invocation = calls.first().expect("Compose should run");
        let subcommand: Vec<&str> = invocation
            .arguments
            .iter()
            .map(String::as_str)
            .skip_while(|argument| *argument != "up")
            .collect();
        assert_eq!(subcommand, ["up", "--detach", "web"]);
        assert_eq!(invocation.host, "ssh://example-host");
        assert_eq!(invocation.secrets, Secrets::fixture());
    }

    /// [`ComposeHandler::execute`] does not run Compose when preparation fails.
    #[test]
    fn compose_handler_execute_prepare_error() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let mut project = Stack::fixture_project();
        project.expect_object_mut("").remove("x-orbit");
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_instance(ComposeConfigReaderStub {
                project: Ok(project),
            })
            .with_instance(WorkingDirectory::new(directory.path().to_path_buf()))
            .build();
        let handler = services.expect::<ComposeHandler>();
        // Act
        let report = handler
            .execute(vec!["up".to_owned()])
            .expect_err("missing host should fail");
        // Assert
        assert_eq!(report.current_context(), &ComposeHandlerError::Prepare);
        let calls = services.expect::<ComposeRunnerSpy>().calls();
        assert_eq!(calls, Vec::new());
    }
}
