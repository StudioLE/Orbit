//! Trait and adapter for decrypting secrets with sops.

use crate::prelude::*;
use std::process::{Command, Output};

/// Decrypt sops-encrypted dotenv files.
pub trait SopsClient: Send + Sync {
    /// Decrypt the dotenv file at `path` into memory.
    fn decrypt(&self, path: &Path) -> Result<Secrets, Report<SopsError>>;
}

/// [`SopsClient`] that runs the `sops` binary.
#[derive(Default, FromServices)]
pub struct SopsClientAdapter;

impl SopsClient for SopsClientAdapter {
    /// Decrypt the dotenv file at `path` into memory.
    ///
    /// - Captures stdout into memory only; never attaches or logs it
    /// - Attaches stderr and exit code on failure
    fn decrypt(&self, path: &Path) -> Result<Secrets, Report<SopsError>> {
        debug!(path = %path.display(), "Decrypting secrets");
        let output = Command::new("sops")
            .args([
                "decrypt",
                "--input-type",
                "dotenv",
                "--output-type",
                "dotenv",
            ])
            .arg(path)
            .output()
            .change_context(SopsError::Spawn)
            .attach_path(path)?;
        if !output.status.success() {
            // Dropped rather than attached, because stdout may hold decrypted secrets
            let response = Output {
                stdout: Vec::new(),
                ..output
            }
            .to_response();
            return Err(Report::new(SopsError::Decrypt)
                .attach_path(path)
                .attach_response(response));
        }
        // Dropped rather than wrapped, because `FromUtf8Error` holds the decrypted bytes
        let text = String::from_utf8(output.stdout)
            .map_err(|_| Report::new(SopsError::Utf8).attach_path(path))?;
        Secrets::from_str(&text)
            .change_context(SopsError::Parse)
            .attach_path(path)
    }
}

/// Errors returned by [`SopsClient`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum SopsError {
    /// Unable to start sops.
    #[error("Unable to start sops")]
    Spawn,
    /// Sops exited with an error.
    #[error("Unable to decrypt secrets")]
    Decrypt,
    /// Decrypted output is not UTF-8.
    #[error("Decrypted secrets are not UTF-8")]
    Utf8,
    /// Decrypted output is not dotenv.
    #[error("Unable to parse decrypted secrets")]
    Parse,
}
