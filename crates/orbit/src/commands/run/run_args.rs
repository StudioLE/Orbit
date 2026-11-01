//! Arguments for the run subcommand.

use crate::prelude::*;

/// Parsed arguments for the run subcommand.
#[derive(Args, Clone, Debug, Eq, PartialEq)]
#[expect(clippy::struct_excessive_bools, reason = "one bool per Compose flag")]
pub struct RunArgs {
    /// Remove the container when it exits.
    #[usage(long)]
    pub rm: bool,
    /// Pull images before starting.
    #[usage(long, value_enum)]
    pub pull: Option<PullPolicy>,
    /// Set an environment variable in the container.
    #[usage(short, long, var, value_name = "KEY=VALUE")]
    pub env: Vec<String>,
    /// Build the image before starting.
    #[usage(long)]
    pub build: bool,
    /// Don't start the service's dependencies.
    #[usage(long)]
    pub no_deps: bool,
    /// Run the container in the background and print its ID.
    #[usage(short, long)]
    pub detach: bool,
    /// Don't allocate a pseudo-TTY.
    #[usage(short = 'T', long)]
    pub no_tty: bool,
    /// Override the entrypoint of the image.
    #[usage(long)]
    pub entrypoint: Option<String>,
    /// Run as this user name or UID.
    #[usage(short, long)]
    pub user: Option<String>,
    /// Working directory inside the container.
    #[usage(short, long, value_name = "PATH")]
    pub workdir: Option<String>,
    /// Name the container.
    #[usage(long)]
    pub name: Option<String>,
    /// Publish the service's ports to the host.
    #[usage(short = 'P', long)]
    pub service_ports: bool,
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
        for env in &self.env {
            arguments.push("--env".to_owned());
            arguments.push(env.clone());
        }
        if self.build {
            arguments.push("--build".to_owned());
        }
        if self.no_deps {
            arguments.push("--no-deps".to_owned());
        }
        if self.detach {
            arguments.push("--detach".to_owned());
        }
        if self.no_tty {
            arguments.push("--no-tty".to_owned());
        }
        if let Some(entrypoint) = &self.entrypoint {
            arguments.push("--entrypoint".to_owned());
            arguments.push(entrypoint.clone());
        }
        if let Some(user) = &self.user {
            arguments.push("--user".to_owned());
            arguments.push(user.clone());
        }
        if let Some(workdir) = &self.workdir {
            arguments.push("--workdir".to_owned());
            arguments.push(workdir.clone());
        }
        if let Some(name) = &self.name {
            arguments.push("--name".to_owned());
            arguments.push(name.clone());
        }
        if self.service_ports {
            arguments.push("--service-ports".to_owned());
        }
        arguments.push(self.service.clone());
        arguments.extend(self.args.iter().cloned());
        arguments
    }
}

#[cfg(test)]
mod fixtures {
    use super::*;

    impl RunArgs {
        /// Create a [`RunArgs`] fixture for the `app` service without flags or arguments.
        pub fn fixture() -> Self {
            Self {
                rm: false,
                pull: None,
                env: Vec::new(),
                build: false,
                no_deps: false,
                detach: false,
                no_tty: false,
                entrypoint: None,
                user: None,
                workdir: None,
                name: None,
                service_ports: false,
                service: "app".to_owned(),
                args: Vec::new(),
            }
        }
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
            env: vec!["NAME=a".to_owned(), "TAG=b".to_owned()],
            build: true,
            no_deps: true,
            detach: true,
            no_tty: true,
            entrypoint: Some("sh".to_owned()),
            user: Some("1000".to_owned()),
            workdir: Some("/data".to_owned()),
            name: Some("job".to_owned()),
            service_ports: true,
            service: "app".to_owned(),
            args: vec!["echo".to_owned(), "--rm".to_owned()],
        };
        // Act
        let arguments = args.to_compose_arguments();
        // Assert
        let expected = [
            "run",
            "--rm",
            "--pull",
            "never",
            "--env",
            "NAME=a",
            "--env",
            "TAG=b",
            "--build",
            "--no-deps",
            "--detach",
            "--no-tty",
            "--entrypoint",
            "sh",
            "--user",
            "1000",
            "--workdir",
            "/data",
            "--name",
            "job",
            "--service-ports",
            "app",
            "echo",
            "--rm",
        ];
        assert_eq!(arguments, expected);
    }
}
