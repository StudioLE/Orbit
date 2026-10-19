//! Problem in a Compose project that does not stop a deploy.

use crate::prelude::*;

/// Problem in a Compose project that does not stop a deploy.
#[derive(Clone, Debug, Eq, PartialEq, Serialize)]
pub enum SettingsWarning {
    /// A service `environment` key has no value, so it is not read from `.sops.env`.
    BareName {
        /// Service name.
        service: String,
        /// Env var name.
        name: String,
    },
    /// A top-level secret reads an env var, so it is not read from `.sops.env`.
    EnvironmentSecret {
        /// Top-level secret name.
        secret: String,
        /// Env var name.
        variable: String,
    },
    /// A service `environment` key is replaced by `x-orbit.environment`.
    Replaced {
        /// Service name.
        service: String,
        /// Env var name.
        name: String,
    },
}

impl Display for SettingsWarning {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> fmt::Result {
        match self {
            Self::BareName { service, name } => {
                write!(formatter, "Service `{service}` sets `{name}` with no value")
            }
            Self::EnvironmentSecret { secret, variable } => write!(
                formatter,
                "Secret `{secret}` reads `{variable}` from your shell"
            ),
            Self::Replaced { service, name } => write!(
                formatter,
                "Service `{service}` sets `{name}` in both `environment` and `x-orbit.environment`; using `x-orbit.environment`"
            ),
        }
    }
}
