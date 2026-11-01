//! Arguments for the up subcommand.

use crate::prelude::*;

/// Parsed arguments for the up subcommand.
#[derive(Args, Clone, Debug, Default, Eq, PartialEq)]
#[expect(clippy::struct_excessive_bools, reason = "one bool per Compose flag")]
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
    /// Build images before starting.
    #[usage(long)]
    pub build: bool,
    /// Don't build images, even if the service's pull policy says to.
    #[usage(long)]
    pub no_build: bool,
    /// Don't start dependencies of the given services.
    #[usage(long)]
    pub no_deps: bool,
    /// Remove containers for services not in the Compose file.
    #[usage(long)]
    pub remove_orphans: bool,
    /// Wait for services to be running or healthy, then detach.
    #[usage(long)]
    pub wait: bool,
    /// Maximum seconds to wait for services to be running or healthy.
    #[usage(long, value_name = "SECONDS")]
    pub wait_timeout: Option<u32>,
    /// Seconds to wait for containers to stop before killing them.
    #[usage(short, long, value_name = "SECONDS")]
    pub timeout: Option<u32>,
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
        if self.build {
            arguments.push("--build".to_owned());
        }
        if self.no_build {
            arguments.push("--no-build".to_owned());
        }
        if self.no_deps {
            arguments.push("--no-deps".to_owned());
        }
        if self.remove_orphans {
            arguments.push("--remove-orphans".to_owned());
        }
        if self.wait {
            arguments.push("--wait".to_owned());
        }
        if let Some(wait_timeout) = self.wait_timeout {
            arguments.push("--wait-timeout".to_owned());
            arguments.push(wait_timeout.to_string());
        }
        if let Some(timeout) = self.timeout {
            arguments.push("--timeout".to_owned());
            arguments.push(timeout.to_string());
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
            build: true,
            no_build: true,
            no_deps: true,
            remove_orphans: true,
            wait: true,
            wait_timeout: Some(60),
            timeout: Some(5),
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
            "--build",
            "--no-build",
            "--no-deps",
            "--remove-orphans",
            "--wait",
            "--wait-timeout",
            "60",
            "--timeout",
            "5",
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
