//! Turn the stack into a [`Deployment`] for a Compose run.

mod deployment;
mod deployment_provider;
mod docker_host_provider;

pub use deployment::*;
pub use deployment_provider::*;
pub use docker_host_provider::*;
