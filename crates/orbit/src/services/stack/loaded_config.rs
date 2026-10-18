//! Config file read from disk.

/// Config file read from disk.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct LoadedConfig {
    /// Path in the container.
    pub target: String,
    /// File content as written on disk.
    pub content: String,
}
