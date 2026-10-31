//! Config file read from disk.

use crate::prelude::*;

/// Config file read from disk.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct LoadedConfig {
    /// Path in the container.
    pub target: String,
    /// File content as written on disk.
    pub content: String,
    /// File mode in the container.
    pub mode: Option<ConfigMode>,
}
