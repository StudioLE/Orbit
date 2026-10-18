//! Arguments and environment for one Docker Compose run.

use crate::prelude::*;

/// Arguments and environment for one `docker compose` run.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ComposeInvocation {
    /// Arguments after `docker compose`.
    pub arguments: Vec<String>,
    /// Docker host URL set as `DOCKER_HOST`.
    pub host: String,
    /// Secrets set as environment variables.
    pub secrets: Secrets,
}
