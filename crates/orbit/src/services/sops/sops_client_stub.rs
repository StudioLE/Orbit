//! Stub for decrypting secrets.

use crate::prelude::*;

/// [`SopsClient`] returning fixed [`Secrets`].
pub struct SopsClientStub {
    /// Value returned by [`SopsClient::decrypt`].
    pub secrets: Secrets,
}

impl FromServices for SopsClientStub {
    type Error = Infallible;

    fn from_services(_services: &ServiceProvider) -> Result<Self, Report<Self::Error>> {
        Ok(Self {
            secrets: Secrets::fixture(),
        })
    }
}

impl SopsClient for SopsClientStub {
    fn decrypt(&self, _path: &Path) -> Result<Secrets, Report<SopsError>> {
        Ok(self.secrets.clone())
    }
}
