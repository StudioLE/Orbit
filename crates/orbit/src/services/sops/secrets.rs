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
        Self::from_iter([("API_TOKEN", "a"), ("MAIL_PASSWORD", "b"), ("TLS_KEY", "c")])
    }

    /// Create a [`Secrets`] fixture with only `API_TOKEN`.
    #[cfg(test)]
    pub fn fixture_api_token() -> Self {
        Self::from_iter([("API_TOKEN", "a")])
    }
}

impl<K, V> FromIterator<(K, V)> for Secrets
where
    K: Into<String>,
    V: Into<String>,
{
    /// Create [`Secrets`] from name and value pairs.
    ///
    /// - Keeps the last value when a name repeats
    fn from_iter<T>(pairs: T) -> Self
    where
        T: IntoIterator,
        T::Item: Into<(K, V)>,
    {
        let secrets = pairs
            .into_iter()
            .map(|pair| {
                let (name, value) = pair.into();
                (name.into(), value.into())
            })
            .collect();
        Self(secrets)
    }
}

impl Debug for Secrets {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
        f.debug_struct("Secrets")
            .field("names", &self.0.keys().collect::<Vec<_>>())
            .finish()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`Secrets`] debug output lists names without values.
    #[test]
    fn secrets_debug() {
        // Arrange
        let secrets = Secrets::from_iter([
            ("API_TOKEN", "hunter2hunter2"),
            ("DB_PASSWORD", "swordfish"),
        ]);
        // Act
        let debug = format!("{secrets:?}");
        // Assert
        assert_eq!(debug, r#"Secrets { names: ["API_TOKEN", "DB_PASSWORD"] }"#);
    }
}
