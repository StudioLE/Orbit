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
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
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
        // Arrange
        let config = LoadedConfig {
            target: "/etc/a".to_owned(),
            content: "x".to_owned(),
            mode: None,
        };
        // Act
        let hash = ConfigHash::new(&[config]);
        // Assert
        assert_eq!(
            hash.to_string(),
            "68bc2c933e58ee763cf97ec8bf9651208261b540bbb38230ca8f3f96f9c94d51"
        );
    }

    /// [`ConfigHash::new`] changes when content moves between target and content.
    #[test]
    fn config_hash_new_separator() {
        // Arrange
        let left = LoadedConfig {
            target: "/etc/ab".to_owned(),
            content: "c".to_owned(),
            mode: None,
        };
        let right = LoadedConfig {
            target: "/etc/a".to_owned(),
            content: "bc".to_owned(),
            mode: None,
        };
        // Act
        let left = ConfigHash::new(&[left]);
        let right = ConfigHash::new(&[right]);
        // Assert
        assert_ne!(left, right);
    }

    /// [`ConfigHash::new`] changes when config order changes.
    #[test]
    fn config_hash_new_order() {
        // Arrange
        let a = LoadedConfig {
            target: "/etc/a".to_owned(),
            content: "1".to_owned(),
            mode: None,
        };
        let b = LoadedConfig {
            target: "/etc/b".to_owned(),
            content: "2".to_owned(),
            mode: None,
        };
        // Act
        let forward = ConfigHash::new(&[a.clone(), b.clone()]);
        let reverse = ConfigHash::new(&[b, a]);
        // Assert
        assert_ne!(forward, reverse);
    }
}
