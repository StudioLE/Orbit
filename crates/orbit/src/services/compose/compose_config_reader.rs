//! Trait and adapter for reading the Compose project with `docker compose config`.

use crate::prelude::*;
use serde_json::from_slice as json_from_slice;
use std::process::Command;

/// Read the Compose project in the [`WorkingDirectory`].
#[cfg_attr(test, mockall::automock)]
pub trait ComposeConfigReader: Send + Sync {
    /// Load the Compose project in the [`WorkingDirectory`] as JSON without interpolation.
    fn read(&self) -> Result<Value, Report<ComposeConfigError>>;
}

/// [`ComposeConfigReader`] that runs the `docker` binary.
#[derive(FromServices)]
pub struct ComposeConfigReaderAdapter {
    directory: Arc<WorkingDirectory>,
}

impl ComposeConfigReader for ComposeConfigReaderAdapter {
    /// Load the Compose project in the [`WorkingDirectory`] as JSON without interpolation.
    ///
    /// - Inherits the environment, so `COMPOSE_FILE` and `COMPOSE_PROJECT_NAME` apply
    fn read(&self) -> Result<Value, Report<ComposeConfigError>> {
        let directory = self.directory.path();
        let output = Command::new("docker")
            .args(["compose", "config", "--no-interpolate", "--format", "json"])
            .current_dir(directory)
            .output()
            .change_context(ComposeConfigError::Spawn)
            .attach_path(directory)?
            .ok_or_report(ComposeConfigError::Config)
            .attach_path(directory)?;
        json_from_slice(&output.stdout)
            .change_context(ComposeConfigError::Parse)
            .attach_path(directory)
    }
}

/// Errors returned by [`ComposeConfigReader`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum ComposeConfigError {
    /// Unable to start docker.
    #[error("Unable to start docker")]
    Spawn,
    /// `docker compose config` exited with an error.
    #[error("Unable to load the Compose project")]
    Config,
    /// `docker compose config` printed invalid JSON.
    #[error("Unable to parse the Compose project")]
    Parse,
}
