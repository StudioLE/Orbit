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
        let directory = Stack::mock_temp_directory();
        let mut compose_config = MockComposeConfigReader::new();
        compose_config
            .expect_read()
            .returning(|| Ok(Stack::mock_project()));
        let mut runner = MockComposeRunner::new();
        runner
            .expect_execute()
            .withf(|invocation| {
                let expected = ["up", "--detach", "web"];
                let subcommand: Vec<&str> = invocation
                    .arguments
                    .iter()
                    .map(String::as_str)
                    .skip_while(|argument| *argument != "up")
                    .collect();
                invocation.host == "ssh://example-host"
                    && invocation.secrets.contains("API_TOKEN")
                    && subcommand == expected
            })
            .times(1)
            .returning(|_| Ok(()));
        let handler = get_handler(compose_config, runner, directory.path());
        let arguments = ["up", "--detach", "web"].map(str::to_owned).to_vec();
        // Act
        let output = handler.execute(arguments);
        // Assert
        assert!(output.is_ok(), "{output:?}");
    }

    /// [`ComposeHandler::execute`] does not run Compose when preparation fails.
    #[test]
    fn compose_handler_execute_prepare_error() {
        // Arrange
        let directory = Stack::mock_temp_directory();
        let mut compose_config = MockComposeConfigReader::new();
        compose_config.expect_read().returning(|| {
            let mut project = Stack::mock_project();
            project.expect_object_mut("").remove("x-orbit");
            Ok(project)
        });
        let mut runner = MockComposeRunner::new();
        runner.expect_execute().never();
        let handler = get_handler(compose_config, runner, directory.path());
        // Act
        let report = handler
            .execute(vec!["up".to_owned()])
            .expect_err("missing host should fail");
        // Assert
        assert_eq!(report.current_context(), &ComposeHandlerError::Prepare);
    }

    fn get_handler(
        compose_config: MockComposeConfigReader,
        runner: MockComposeRunner,
        directory: &Path,
    ) -> Arc<ComposeHandler> {
        let mut sops = MockSopsClient::new();
        sops.expect_decrypt().returning(|_| {
            Ok(Secrets::from_str("API_TOKEN=a\nMAIL_PASSWORD=b\nTLS_KEY=c")
                .expect("dotenv should parse"))
        });
        ServiceBuilder::new()
            .with_mock_services(compose_config, runner, sops)
            .with_instance(WorkingDirectory::new(directory.to_path_buf()))
            .build()
            .expect::<ComposeHandler>()
    }
}
