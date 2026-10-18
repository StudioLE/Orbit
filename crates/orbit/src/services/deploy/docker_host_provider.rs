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
        let provider = get_provider(None);
        // Act
        let host = provider.get(&mock_settings(Some("ssh://stack-host")));
        // Assert
        assert_eq!(host.expect("host should resolve"), "ssh://stack-host");
    }

    /// [`DockerHostProvider::get`] prefers `--host` over `x-orbit.host`.
    #[test]
    fn docker_host_provider_get_override() {
        // Arrange
        let provider = get_provider(Some("ssh://other"));
        // Act
        let host = provider.get(&mock_settings(Some("ssh://stack-host")));
        // Assert
        assert_eq!(host.expect("host should resolve"), "ssh://other");
    }

    /// [`DockerHostProvider::get`] uses `--host` without `x-orbit.host`.
    #[test]
    fn docker_host_provider_get_flag_only() {
        // Arrange
        let provider = get_provider(Some("ssh://other"));
        // Act
        let host = provider.get(&mock_settings(None));
        // Assert
        assert_eq!(host.expect("host should resolve"), "ssh://other");
    }

    /// [`DockerHostProvider::get`] fails without `--host` or `x-orbit.host`, naming `x-orbit.host`.
    #[test]
    fn docker_host_provider_get_missing() {
        // Arrange
        let provider = get_provider(None);
        // Act
        let report = provider
            .get(&mock_settings(None))
            .expect_err("host should be missing");
        // Assert
        assert_eq!(report.current_context(), &DockerHostError::Missing);
        assert!(report.to_string().contains("x-orbit.host"));
    }

    fn get_provider(host: Option<&str>) -> Arc<DockerHostProvider> {
        let cli = CliArgs {
            host: host.map(str::to_owned),
            ..CliArgs::mock()
        };
        ServiceBuilder::new()
            .with_mock_services(
                MockComposeConfigReader::new(),
                MockComposeRunner::new(),
                MockSopsClient::new(),
            )
            .with_instance(cli)
            .build()
            .expect::<DockerHostProvider>()
    }

    fn mock_settings(host: Option<&str>) -> OrbitSettings {
        OrbitSettings {
            host: host.map(str::to_owned),
            ..OrbitSettings::default()
        }
    }
}
