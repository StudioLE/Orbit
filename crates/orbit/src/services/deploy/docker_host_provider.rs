//! Resolve the Docker host to deploy to.

use crate::prelude::*;

/// Resolve the Docker host from `--host` or `x-orbit.host`.
#[derive(FromServices)]
pub struct DockerHostProvider {
    cli: Arc<CliArgs>,
}

impl DockerHostProvider {
    /// Resolve the Docker host to deploy to.
    ///
    /// - Prefers `--host` over top-level `x-orbit.host`
    /// - Logs both at `info` when they differ
    pub fn get(&self, settings: &OrbitSettings) -> Result<String, Report<DockerHostError>> {
        match (&self.cli.host, &settings.host) {
            (Some(host), Some(stack_host)) => {
                if host != stack_host {
                    info!(%host, %stack_host, "Overriding `x-orbit.host` with `--host`");
                }
                Ok(host.clone())
            }
            (Some(host), None) | (None, Some(host)) => Ok(host.clone()),
            (None, None) => Err(Report::new(DockerHostError::Missing)),
        }
    }
}

/// Errors returned by [`DockerHostProvider`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum DockerHostError {
    /// Neither `--host` nor `x-orbit.host` is set.
    #[error("No Docker host: set top-level `x-orbit.host` or pass `--host`")]
    Missing,
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`DockerHostProvider::get`] uses `x-orbit.host` without `--host`.
    #[test]
    fn docker_host_provider_get() {
        // Arrange
        let services = ServiceBuilder::new().with_test_services().build();
        let provider = services.expect::<DockerHostProvider>();
        let settings = OrbitSettings {
            host: Some("ssh://stack-host".to_owned()),
            ..OrbitSettings::default()
        };
        // Act
        let host = provider.get(&settings);
        // Assert
        assert_eq!(host.expect("host should resolve"), "ssh://stack-host");
    }

    /// [`DockerHostProvider::get`] prefers `--host` over `x-orbit.host`.
    #[test]
    fn docker_host_provider_get_override() {
        // Arrange
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_instance(CliArgs {
                host: Some("ssh://other".to_owned()),
                ..CliArgs::fixture()
            })
            .build();
        let provider = services.expect::<DockerHostProvider>();
        let settings = OrbitSettings {
            host: Some("ssh://stack-host".to_owned()),
            ..OrbitSettings::default()
        };
        // Act
        let host = provider.get(&settings);
        // Assert
        assert_eq!(host.expect("host should resolve"), "ssh://other");
    }

    /// [`DockerHostProvider::get`] uses `--host` without `x-orbit.host`.
    #[test]
    fn docker_host_provider_get_flag_only() {
        // Arrange
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_instance(CliArgs {
                host: Some("ssh://other".to_owned()),
                ..CliArgs::fixture()
            })
            .build();
        let provider = services.expect::<DockerHostProvider>();
        // Act
        let host = provider.get(&OrbitSettings::default());
        // Assert
        assert_eq!(host.expect("host should resolve"), "ssh://other");
    }

    /// [`DockerHostProvider::get`] fails without `--host` or `x-orbit.host`, naming `x-orbit.host`.
    #[test]
    fn docker_host_provider_get_missing() {
        // Arrange
        let services = ServiceBuilder::new().with_test_services().build();
        let provider = services.expect::<DockerHostProvider>();
        // Act
        let report = provider
            .get(&OrbitSettings::default())
            .expect_err("host should be missing");
        // Assert
        assert_eq!(report.current_context(), &DockerHostError::Missing);
        assert!(report.to_string().contains("x-orbit.host"));
    }
}
