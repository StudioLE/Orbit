//! Value from `.sops.env` Orbit gives a service.

use crate::prelude::*;

/// Value from `.sops.env` Orbit gives a service, from `x-orbit.environment` or `x-orbit.secrets`.
#[derive(Clone, Debug, Deserialize, Eq, PartialEq, Serialize)]
#[serde(deny_unknown_fields)]
pub struct SecretEntry {
    /// Key in `.sops.env`.
    pub source: String,
    /// Env var name, or file path in the container.
    pub target: String,
}
