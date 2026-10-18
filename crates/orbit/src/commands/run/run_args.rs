//! Arguments for the run subcommand.

use crate::prelude::*;

/// Parsed arguments for the run subcommand.
#[derive(Args, Clone, Debug, Eq, PartialEq)]
pub struct RunArgs {
    /// Remove the container when it exits.
    #[usage(long)]
    pub rm: bool,
    /// Pull images before starting.
    #[usage(long, value_enum)]
    pub pull: Option<PullPolicy>,
    /// Service to run.
    #[usage(arg, double_dash = "automatic")]
    pub service: String,
    /// Arguments passed to the container verbatim.
    #[usage(arg)]
    pub args: Vec<String>,
}

impl RunArgs {
    /// Arguments for `docker compose`, starting with the `run` subcommand.
    pub fn to_compose_arguments(&self) -> Vec<String> {
        let mut arguments = vec!["run".to_owned()];
        if self.rm {
            arguments.push("--rm".to_owned());
        }
        if let Some(pull) = self.pull {
            arguments.push("--pull".to_owned());
            arguments.push(pull.to_string());
        }
        arguments.push(self.service.clone());
        arguments.extend(self.args.iter().cloned());
        arguments
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`RunArgs::to_compose_arguments`] maps flags before the service and its arguments.
    #[test]
    fn run_args_to_compose_arguments() {
        // Arrange
        let args = RunArgs {
            rm: true,
            pull: Some(PullPolicy::Never),
            service: "app".to_owned(),
            args: vec!["echo".to_owned(), "--rm".to_owned()],
        };
        // Act
        let arguments = args.to_compose_arguments();
        // Assert
        let expected = ["run", "--rm", "--pull", "never", "app", "echo", "--rm"];
        assert_eq!(arguments, expected);
    }
}
