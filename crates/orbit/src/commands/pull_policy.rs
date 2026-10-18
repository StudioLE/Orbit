//! Image pull policy shared by the up and run subcommands.

use crate::prelude::*;
use usage_rs::ValueEnum;

/// When Compose pulls images before starting containers.
#[derive(Clone, Copy, Debug, Display, Eq, PartialEq, ValueEnum)]
#[strum(serialize_all = "lowercase")]
pub enum PullPolicy {
    /// Always pull.
    Always,
    /// Pull only missing images.
    Missing,
    /// Never pull.
    Never,
}
