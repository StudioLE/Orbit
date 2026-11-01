//! Parsed CLI arguments registered in the service container.

use crate::prelude::*;
use usage_rs::Cli as UsageCli;

/// Parsed CLI arguments.
#[derive(Debug, UsageCli)]
#[usage(bin = "orbit", about, version, unknown_flags = "error")]
pub struct CliArgs {
    /// Set the log level.
    #[usage(long, global, env = "ORBIT_LOG_LEVEL", value_enum)]
    pub log_level: Option<LogLevel>,
    /// Docker host to deploy to.
    ///
    /// Default: `x-orbit.host` in `docker-compose.yml`
    #[usage(long)]
    pub host: Option<String>,
    /// Subcommand to execute.
    #[usage(subcommand)]
    pub command: Subcommand,
}

impl FromServices for CliArgs {
    type Error = Infallible;

    fn from_services(_: &ServiceProvider) -> Result<Self, Report<Self::Error>>
    where
        Self: Sized,
    {
        Ok(CliArgs::parse())
    }
}

#[cfg(test)]
mod fixtures {
    use super::*;

    impl CliArgs {
        /// Create a [`CliArgs`] fixture for `up` without flags.
        pub fn fixture() -> Self {
            Self {
                log_level: None,
                host: None,
                command: Subcommand::Up(UpArgs::default()),
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use usage_rs::test::{Page, argv, help, parse};

    /// [`CliArgs::parse_from`] binds a log level, a host and an up subcommand with every flag.
    #[test]
    fn cli_args_parse_from() {
        // Arrange
        let input = argv([
            "--log-level",
            "debug",
            "--host",
            "ssh://example-host",
            "up",
            "-d",
            "--force-recreate",
            "--pull",
            "always",
            "--build",
            "--no-build",
            "--no-deps",
            "--remove-orphans",
            "--wait",
            "--wait-timeout",
            "60",
            "-t",
            "5",
            "web",
            "app",
        ]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        assert_eq!(cli.log_level, Some(LogLevel::Debug));
        assert_eq!(cli.host.as_deref(), Some("ssh://example-host"));
        let expected = UpArgs {
            detach: true,
            force_recreate: true,
            pull: Some(PullPolicy::Always),
            build: true,
            no_build: true,
            no_deps: true,
            remove_orphans: true,
            wait: true,
            wait_timeout: Some(60),
            timeout: Some(5),
            services: vec!["web".to_owned(), "app".to_owned()],
        };
        assert_eq!(cli.command, Subcommand::Up(expected));
    }

    /// [`CliArgs::parse_from`] binds an up subcommand without flags or services.
    #[test]
    fn cli_args_parse_from_up_bare() {
        // Arrange
        let input = argv(["up"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        assert_eq!(cli.command, Subcommand::Up(UpArgs::default()));
    }

    /// [`CliArgs::parse_from`] passes arguments after the service to the container verbatim.
    #[test]
    fn cli_args_parse_from_run() {
        // Arrange
        let input = argv([
            "run", "--rm", "--pull", "never", "app", "hello", "--loud", "-x",
        ]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        let expected = RunArgs {
            rm: true,
            pull: Some(PullPolicy::Never),
            args: vec!["hello".to_owned(), "--loud".to_owned(), "-x".to_owned()],
            ..RunArgs::fixture()
        };
        assert_eq!(cli.command, Subcommand::Run(expected));
    }

    /// [`CliArgs::parse_from`] collects every `-e` and `--env` given before the service.
    #[test]
    fn cli_args_parse_from_run_env() {
        // Arrange
        let input = argv(["run", "-e", "NAME=a", "--env", "TAG=b=c", "app"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        let expected = RunArgs {
            env: vec!["NAME=a".to_owned(), "TAG=b=c".to_owned()],
            ..RunArgs::fixture()
        };
        assert_eq!(cli.command, Subcommand::Run(expected));
    }

    /// [`CliArgs::parse_from`] binds every run flag given before the service.
    #[test]
    fn cli_args_parse_from_run_flags() {
        // Arrange
        let input = argv([
            "run",
            "--build",
            "--no-deps",
            "-d",
            "-T",
            "--entrypoint",
            "sh",
            "-u",
            "1000",
            "-w",
            "/data",
            "--name",
            "job",
            "-P",
            "app",
        ]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        let expected = RunArgs {
            build: true,
            no_deps: true,
            detach: true,
            no_tty: true,
            entrypoint: Some("sh".to_owned()),
            user: Some("1000".to_owned()),
            workdir: Some("/data".to_owned()),
            name: Some("job".to_owned()),
            service_ports: true,
            ..RunArgs::fixture()
        };
        assert_eq!(cli.command, Subcommand::Run(expected));
    }

    /// [`CliArgs::parse_from`] keeps flags after the service that `run` also declares.
    #[test]
    fn cli_args_parse_from_run_declared_flag_after_service() {
        // Arrange
        let input = argv(["run", "app", "--rm", "--host", "x"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        assert_eq!(cli.host, None);
        let expected = RunArgs {
            args: vec!["--rm".to_owned(), "--host".to_owned(), "x".to_owned()],
            ..RunArgs::fixture()
        };
        assert_eq!(cli.command, Subcommand::Run(expected));
    }

    /// [`CliArgs::parse_from`] binds the global `--log-level` after a subcommand.
    #[test]
    fn cli_args_parse_from_log_level_after_subcommand() {
        // Arrange
        let input = argv(["up", "--log-level", "debug"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        assert_eq!(cli.log_level, Some(LogLevel::Debug));
    }

    /// [`CliArgs::parse_from`] passes the global `--log-level` after the service to the container.
    #[test]
    fn cli_args_parse_from_run_log_level_after_service() {
        // Arrange
        let input = argv(["run", "app", "--log-level", "debug"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let cli = parse(spec, &words, CliArgs::parse_from).expect("the command line should parse");
        // Assert
        assert_eq!(cli.log_level, None);
        let expected = RunArgs {
            args: vec!["--log-level".to_owned(), "debug".to_owned()],
            ..RunArgs::fixture()
        };
        assert_eq!(cli.command, Subcommand::Run(expected));
    }

    /// [`CliArgs::parse_from`] rejects an undeclared flag before the service.
    #[test]
    fn cli_args_parse_from_run_unknown_flag() {
        for flag in ["--cap-add", "-v"] {
            // Arrange
            let input = argv(["run", flag, "sh", "app"]);
            let words = input.words();
            let spec = CliArgs::spec();
            // Act
            let message = parse(spec, &words, CliArgs::parse_from)
                .expect_err("an undeclared flag should not parse");
            // Assert
            assert!(message.contains(flag), "{message}");
        }
    }

    /// [`CliArgs::parse_from`] rejects an undeclared flag on up.
    #[test]
    fn cli_args_parse_from_up_unknown_flag() {
        // Arrange
        let input = argv(["up", "--scale", "web=2"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let message =
            parse(spec, &words, CliArgs::parse_from).expect_err("`--scale` is not a flag of `up`");
        // Assert
        assert!(message.contains("--scale"), "{message}");
    }

    /// [`CliArgs::parse_from`] rejects `--host` after the subcommand.
    #[test]
    fn cli_args_parse_from_host_after_subcommand() {
        // Arrange
        let input = argv(["up", "--host", "ssh://example-host"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let message =
            parse(spec, &words, CliArgs::parse_from).expect_err("`--host` is not a flag of `up`");
        // Assert
        assert!(message.contains("--host"), "{message}");
    }

    /// [`CliArgs::parse_from`] rejects a pull policy outside the declared choices.
    #[test]
    fn cli_args_parse_from_invalid_pull() {
        // Arrange
        let input = argv(["up", "--pull", "sometimes"]);
        let words = input.words();
        let spec = CliArgs::spec();
        // Act
        let message =
            parse(spec, &words, CliArgs::parse_from).expect_err("`sometimes` is not a policy");
        // Assert
        assert!(message.contains("invalid value 'sometimes'"), "{message}");
    }

    /// The root help page.
    ///
    /// - Replaces the version with a placeholder, as CI sets it at build time
    #[test]
    fn cli_args_help() {
        let spec = CliArgs::spec();
        let output = help(spec, &[], Page::Long).replace(env!("CARGO_PKG_VERSION"), "x.y.z");
        assert_snapshot!(output);
    }

    /// The help page for the up subcommand.
    #[test]
    fn cli_args_help_up() {
        let spec = CliArgs::spec();
        let output = help(spec, &["up"], Page::Long);
        assert_snapshot!(output);
    }

    /// The help page for the run subcommand.
    #[test]
    fn cli_args_help_run() {
        let spec = CliArgs::spec();
        let output = help(spec, &["run"], Page::Long);
        assert_snapshot!(output);
    }

    /// The usage spec printed by `orbit __usage_spec__`.
    ///
    /// - Replaces the version with a placeholder, as CI sets it at build time
    #[test]
    fn cli_args_to_kdl() {
        let output = CliArgs::to_kdl().replace(env!("CARGO_PKG_VERSION"), "x.y.z");
        assert_snapshot!(output);
    }
}
