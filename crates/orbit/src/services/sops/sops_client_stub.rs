//! Stub for decrypting secrets.

use crate::prelude::*;

/// [`SopsClient`] returning fixed [`DecryptedText`].
pub struct SopsClientStub {
    /// Value returned by [`SopsClient::decrypt`].
    pub text: DecryptedText,
}

impl FromServices for SopsClientStub {
    type Error = Infallible;

    fn from_services(_services: &ServiceProvider) -> Result<Self, Report<Self::Error>> {
        Ok(Self {
            text: DecryptedText::fixture(),
        })
    }
}

impl SopsClient for SopsClientStub {
    fn decrypt(&self, _path: &Path) -> Result<DecryptedText, Report<SopsError>> {
        Ok(self.text.clone())
    }
}
