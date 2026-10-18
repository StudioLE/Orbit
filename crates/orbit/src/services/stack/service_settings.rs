//! What Orbit reads from one Compose service.

use crate::prelude::*;

/// What Orbit reads from one Compose service.
#[derive(Clone, Debug, Default, Eq, PartialEq, Serialize)]
pub struct ServiceSettings {
    /// Env vars from `x-orbit.environment`, in declared order.
    pub environment: Vec<SecretEntry>,
    /// Files from `x-orbit.secrets`, in declared order.
    pub secrets: Vec<SecretEntry>,
    /// Files from `x-orbit.configs`, in declared order.
    pub configs: Vec<ConfigFile>,
}
