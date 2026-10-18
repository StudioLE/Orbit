//! Subcommand variants and dispatch handler.

use crate::prelude::*;
use usage_rs::Subcommands as UsageSubcommands;

/// Available CLI subcommands.
#[derive(Clone, Debug, Eq, PartialEq, UsageSubcommands)]
pub enum Subcommand {
    /// Create and start containers on the Docker host.
    Up(UpArgs),
    /// Run a one-off command in a new container on the Docker host.
    Run(RunArgs),
}

/// Dispatch the selected [`Subcommand`] to the [`ComposeHandler`].
#[derive(FromServices)]
pub struct SubcommandHandler {
    cli: Arc<CliArgs>,
    handler: Arc<ComposeHandler>,
}

impl SubcommandHandler {
    /// Execute the selected subcommand.
    pub fn run(&self) -> Result<(), StructuredError> {
        let arguments = match &self.cli.command {
            Subcommand::Up(args) => args.to_compose_arguments(),
            Subcommand::Run(args) => args.to_compose_arguments(),
        };
        Ok(self.handler.execute(arguments)?)
    }
}
