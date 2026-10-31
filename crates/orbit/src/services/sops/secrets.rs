//! Decrypted secret values keyed by name.

use crate::prelude::*;

/// Decrypted secret values keyed by name.
///
/// - [`Debug`] lists names only, so values cannot reach logs or reports
#[derive(Clone, Default, Eq, PartialEq)]
pub struct Secrets(BTreeMap<String, String>);

impl Secrets {
    /// Is there a secret named `name`?
    pub fn contains(&self, name: &str) -> bool {
        self.0.contains_key(name)
    }

    /// Iterate over names and values in name order.
    pub fn iter(&self) -> impl Iterator<Item = (&str, &str)> {
        self.0
            .iter()
            .map(|(name, value)| (name.as_str(), value.as_str()))
    }

    /// Create a [`Secrets`] fixture with every secret the fixture project needs.
    #[cfg(test)]
    pub fn fixture() -> Self {
        Self::from_str("API_TOKEN=a\nMAIL_PASSWORD=b\nTLS_KEY=c").expect("dotenv should parse")
    }
}

impl FromStr for Secrets {
    type Err = Report<SecretsError>;

    /// Parse dotenv text into [`Secrets`].
    ///
    /// - Splits each line at the first `=`
    /// - Unescapes `\n` as a newline, matching how sops writes multiline values
    /// - Skips blank lines and lines starting with `#`
    fn from_str(text: &str) -> Result<Self, Self::Err> {
        let mut secrets = BTreeMap::new();
        for (index, line) in text.lines().enumerate() {
            if line.trim().is_empty() || line.starts_with('#') {
                continue;
            }
            let Some((name, value)) = line.split_once('=') else {
                return Err(Report::new(SecretsError::Separator).attach("line", index + 1));
            };
            if name.is_empty() {
                return Err(Report::new(SecretsError::Name).attach("line", index + 1));
            }
            secrets.insert(name.to_owned(), value.replace("\\n", "\n"));
        }
        Ok(Self(secrets))
    }
}

impl Debug for Secrets {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
        f.debug_struct("Secrets")
            .field("names", &self.0.keys().collect::<Vec<_>>())
            .finish()
    }
}

/// Errors returned when parsing [`Secrets`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum SecretsError {
    /// A line has no `=` separator.
    #[error("Line is not KEY=VALUE")]
    Separator,
    /// A line has an empty name before `=`.
    #[error("Line has an empty name")]
    Name,
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`Secrets::from_str`] splits at the first `=`, unescapes `\n` and skips comments.
    #[test]
    fn secrets_from_str() {
        // Arrange
        let text = "A=has=equals and spaces \nB=\"quoted\"\n# comment\n\nC=multi\\nline\nD=\n";
        // Act
        let secrets = Secrets::from_str(text).expect("dotenv should parse");
        // Assert
        let pairs: Vec<_> = secrets.iter().collect();
        assert_eq!(
            pairs,
            vec![
                ("A", "has=equals and spaces "),
                ("B", "\"quoted\""),
                ("C", "multi\nline"),
                ("D", ""),
            ]
        );
    }

    /// [`Secrets::from_str`] rejects a line without `=` and names the line, not its content.
    #[test]
    fn secrets_from_str_separator() {
        // Arrange
        let text = "A=1\nhunter2hunter2\n";
        // Act
        let report = Secrets::from_str(text).expect_err("line without `=` should fail");
        // Assert
        assert_eq!(report.current_context(), &SecretsError::Separator);
        let display = format!("{report:?}");
        assert!(display.contains("line: 2"), "{display}");
        assert!(!display.contains("hunter2"), "{display}");
    }

    /// [`Secrets::from_str`] rejects a line with an empty name.
    #[test]
    fn secrets_from_str_name() {
        let report = Secrets::from_str("=value").expect_err("empty name should fail");
        assert_eq!(report.current_context(), &SecretsError::Name);
    }

    /// [`Secrets`] debug output lists names without values.
    #[test]
    fn secrets_debug() {
        // Arrange
        let secrets = Secrets::from_str("API_TOKEN=hunter2hunter2\nDB_PASSWORD=swordfish")
            .expect("dotenv should parse");
        // Act
        let debug = format!("{secrets:?}");
        // Assert
        assert_eq!(debug, r#"Secrets { names: ["API_TOKEN", "DB_PASSWORD"] }"#);
    }
}
