//! Arguments for the up subcommand.

use crate::prelude::*;

/// Parsed arguments for the up subcommand.
#[derive(Args, Clone, Debug, Default, Eq, PartialEq)]
pub struct UpArgs {
    /// Run containers in the background.
    #[usage(short, long)]
    pub detach: bool,
    /// Recreate containers even if their configuration is unchanged.
    #[usage(long)]
    pub force_recreate: bool,
    /// Pull images before starting.
    #[usage(long, value_enum)]
    pub pull: Option<PullPolicy>,
    /// Services to start.
    ///
    /// Default: every service
    #[usage(arg)]
    pub services: Vec<String>,
}

impl UpArgs {
    /// Arguments for `docker compose`, starting with the `up` subcommand.
    pub fn to_compose_arguments(&self) -> Vec<String> {
        let mut arguments = vec!["up".to_owned()];
        if self.detach {
            arguments.push("--detach".to_owned());
        }
        if self.force_recreate {
            arguments.push("--force-recreate".to_owned());
        }
        if let Some(pull) = self.pull {
            arguments.push("--pull".to_owned());
            arguments.push(pull.to_string());
        }
        arguments.extend(self.services.iter().cloned());
        arguments
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`UpArgs::to_compose_arguments`] maps every flag before the services.
    #[test]
    fn up_args_to_compose_arguments() {
        // Arrange
        let args = UpArgs {
            detach: true,
            force_recreate: true,
            pull: Some(PullPolicy::Missing),
            services: vec!["web".to_owned(), "app".to_owned()],
        };
        // Act
        let arguments = args.to_compose_arguments();
        // Assert
        let expected = [
            "up",
            "--detach",
            "--force-recreate",
            "--pull",
            "missing",
            "web",
            "app",
        ];
        assert_eq!(arguments, expected);
    }

    /// [`UpArgs::to_compose_arguments`] returns only `up` without flags or services.
    #[test]
    fn up_args_to_compose_arguments_bare() {
        // Arrange
        let args = UpArgs::default();
        // Act
        let arguments = args.to_compose_arguments();
        // Assert
        assert_eq!(arguments, ["up"]);
    }
}
