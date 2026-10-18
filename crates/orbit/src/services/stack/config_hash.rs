//! Hash of a service's configs, set as a label so content changes recreate the container.

use crate::prelude::*;
use sha2::{Digest, Sha256};

/// Label key holding the [`ConfigHash`].
pub const CONFIG_HASH_LABEL: &str = "orbit.config-hash";

/// Lowercase hex SHA-256 of a service's configs.
///
/// - Hashes target, `0x00`, content, `0x00` for each config in declared order
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ConfigHash(Vec<u8>);

impl ConfigHash {
    /// Create a [`ConfigHash`] from configs in declared order.
    pub fn new(configs: &[LoadedConfig]) -> Self {
        let mut hasher = Sha256::new();
        for config in configs {
            hasher.update(config.target.as_bytes());
            hasher.update([0]);
            hasher.update(config.content.as_bytes());
            hasher.update([0]);
        }
        Self(hasher.finalize().to_vec())
    }
}

impl Display for ConfigHash {
    fn fmt(&self, f: &mut Formatter<'_>) -> FmtResult {
        for byte in &self.0 {
            write!(f, "{byte:02x}")?;
        }
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`ConfigHash::new`] hashes target and content separated by NUL bytes.
    ///
    /// - Expected value from `printf '/etc/a\0x\0' | sha256sum`
    #[test]
    fn config_hash_new() {
        let hash = ConfigHash::new(&[mock("/etc/a", "x")]);
        assert_eq!(
            hash.to_string(),
            "68bc2c933e58ee763cf97ec8bf9651208261b540bbb38230ca8f3f96f9c94d51"
        );
    }

    /// [`ConfigHash::new`] changes when content moves between target and content.
    #[test]
    fn config_hash_new_separator() {
        let left = ConfigHash::new(&[mock("/etc/ab", "c")]);
        let right = ConfigHash::new(&[mock("/etc/a", "bc")]);
        assert_ne!(left, right);
    }

    /// [`ConfigHash::new`] changes when config order changes.
    #[test]
    fn config_hash_new_order() {
        let a = mock("/etc/a", "1");
        let b = mock("/etc/b", "2");
        let forward = ConfigHash::new(&[a.clone(), b.clone()]);
        let reverse = ConfigHash::new(&[b, a]);
        assert_ne!(forward, reverse);
    }

    fn mock(target: &str, content: &str) -> LoadedConfig {
        LoadedConfig {
            target: target.to_owned(),
            content: content.to_owned(),
        }
    }
}
