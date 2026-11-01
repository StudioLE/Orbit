//! Spy for decrypting secrets.

use crate::prelude::*;

/// [`SopsClient`] that records each call and returns [`DecryptedText::fixture`].
#[derive(FromServices)]
pub struct SopsClientSpy {
    #[di(default)]
    calls: Mutex<Vec<PathBuf>>,
}

impl SopsClientSpy {
    /// Each path passed to [`SopsClient::decrypt`], in call order.
    pub fn calls(&self) -> Vec<PathBuf> {
        self.calls
            .lock()
            .expect("should be able to lock calls")
            .clone()
    }
}

impl SopsClient for SopsClientSpy {
    fn decrypt(&self, path: &Path) -> Result<DecryptedText, Report<SopsError>> {
        self.calls
            .lock()
            .expect("should be able to lock calls")
            .push(path.to_path_buf());
        Ok(DecryptedText::fixture())
    }
}
