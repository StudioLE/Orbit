//! File mode of a config in the container.

use crate::prelude::*;

/// File mode of a config in the container, from `x-orbit.configs`.
///
/// - Passed to Compose as written
#[derive(Clone, Debug, Deserialize, Eq, PartialEq, Serialize)]
#[serde(untagged)]
pub enum ConfigMode {
    /// Number, used as the mode.
    ///
    /// - YAML reads `0755` as octal and `755` as decimal
    Number(u32),
    /// Text, read by Compose as octal.
    Text(String),
}

impl ConfigMode {
    /// Is the mode likely octal digits written as a decimal number?
    ///
    /// - Flags a [`ConfigMode::Number`] above `0777`, as configs rarely need setuid, setgid or sticky bits
    /// - Misses `400` to `511`, which are also valid modes
    pub fn is_missing_leading_zero(&self) -> bool {
        matches!(self, Self::Number(mode) if *mode > 0o777)
    }
}

impl Display for ConfigMode {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> fmt::Result {
        match self {
            Self::Number(mode) => write!(formatter, "{mode}"),
            Self::Text(mode) => write!(formatter, "{mode}"),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`ConfigMode::is_missing_leading_zero`] flags numbers above `0777` only.
    #[test]
    fn config_mode_is_missing_leading_zero() {
        // Arrange
        let cases = [
            (ConfigMode::Number(493), false),
            (ConfigMode::Number(511), false),
            (ConfigMode::Number(512), true),
            (ConfigMode::Number(644), true),
            (ConfigMode::Number(755), true),
            (ConfigMode::Text("755".to_owned()), false),
            (ConfigMode::Text("4755".to_owned()), false),
        ];
        for (mode, expected) in cases {
            // Act
            let output = mode.is_missing_leading_zero();
            // Assert
            assert_eq!(output, expected, "{mode:?}");
        }
    }
}
