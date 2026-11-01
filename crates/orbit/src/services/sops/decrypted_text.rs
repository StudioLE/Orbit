//! Plaintext output of sops.

use crate::prelude::*;

/// Plaintext output of sops.
///
/// - [`Debug`] shows the length only, so the text cannot reach logs or reports
#[derive(Clone, Eq, PartialEq)]
pub struct DecryptedText(String);

impl DecryptedText {
    /// Decrypted text as a string slice.
    pub fn as_str(&self) -> &str {
        &self.0
    }

    /// Create a [`DecryptedText`] fixture with every secret the fixture project needs.
    #[cfg(test)]
    pub fn fixture() -> Self {
        Self::from("API_TOKEN=a\nMAIL_PASSWORD=b\nTLS_KEY=c".to_owned())
    }
}

impl From<String> for DecryptedText {
    fn from(text: String) -> Self {
        Self(text)
    }
}

impl Debug for DecryptedText {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
        f.debug_struct("DecryptedText")
            .field("len", &self.0.len())
            .finish()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`DecryptedText`] debug output shows the length without the text.
    #[test]
    fn decrypted_text_debug() {
        // Arrange
        let text = DecryptedText::from("API_TOKEN=hunter2hunter2".to_owned());
        // Act
        let debug = format!("{text:?}");
        // Assert
        assert_eq!(debug, "DecryptedText { len: 24 }");
    }
}
