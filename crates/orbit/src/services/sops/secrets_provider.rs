//! Decrypt the secrets a [`Stack`] needs.

use crate::prelude::*;

/// Name of the sops-encrypted dotenv file in the stack directory.
const SOPS_FILE: &str = ".sops.env";

/// Decrypt the secrets a [`Stack`] needs from its `.sops.env`.
#[derive(FromServices)]
pub struct SecretsProvider {
    sops: Arc<dyn SopsClient>,
}

impl SecretsProvider {
    /// Decrypt `.sops.env` in the stack directory, or return no secrets IF none are needed.
    ///
    /// - Fails with every needed secret name attached IF `.sops.env` is missing
    pub fn get(&self, stack: &Stack) -> Result<Secrets, Report<SecretsError>> {
        let names = stack.settings.get_secret_names();
        if names.is_empty() {
            return Ok(Secrets::default());
        }
        let path = stack.directory.join(SOPS_FILE);
        if !path.exists() {
            let report = names.into_iter().fold(
                Report::new(SecretsError::SopsFile).attach_path(&path),
                |report, name| report.attach("secret", name),
            );
            return Err(report);
        }
        self.sops
            .decrypt(&path)
            .change_context(SecretsError::Decrypt)
    }
}

/// Errors returned by [`SecretsProvider`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum SecretsError {
    /// The stack needs secrets but has no `.sops.env`.
    #[error("Stack needs secrets but has no `.sops.env`")]
    SopsFile,
    /// Unable to decrypt secrets.
    #[error("Unable to decrypt secrets")]
    Decrypt,
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;
    use tempfile::TempDir;

    /// [`SecretsProvider::get`] decrypts `.sops.env` in the stack directory.
    #[test]
    fn secrets_provider_get() {
        // Arrange
        let directory = Stack::mock_temp_directory();
        let stack = mock_stack(&directory);
        let mut sops = MockSopsClient::new();
        let sops_path = directory.path().join(SOPS_FILE);
        sops.expect_decrypt()
            .withf(move |path| path == sops_path)
            .times(1)
            .returning(|_| Ok(mock_secrets()));
        let provider = get_provider(sops);
        // Act
        let secrets = provider.get(&stack).expect("secrets should decrypt");
        // Assert
        assert_eq!(secrets, mock_secrets());
    }

    /// [`SecretsProvider::get`] lists secret names when `.sops.env` is absent.
    #[test]
    fn secrets_provider_get_sops_file_missing() {
        // Arrange
        let directory = Stack::mock_temp_directory();
        fs::remove_file(directory.path().join(SOPS_FILE)).expect("should remove .sops.env");
        let stack = mock_stack(&directory);
        let mut sops = MockSopsClient::new();
        sops.expect_decrypt().never();
        let provider = get_provider(sops);
        // Act
        let report = provider
            .get(&stack)
            .expect_err("missing .sops.env should fail");
        // Assert
        assert_eq!(report.current_context(), &SecretsError::SopsFile);
        let display = report.to_string();
        for name in ["API_TOKEN", "MAIL_PASSWORD", "TLS_KEY"] {
            assert!(display.contains(&format!("secret: {name}")), "{display}");
        }
    }

    /// [`SecretsProvider::get`] skips sops when the stack needs no secrets.
    #[test]
    fn secrets_provider_get_no_secrets() {
        // Arrange
        let stack = Stack::mock_with(json!({ "services": { "app": {} } }));
        let mut sops = MockSopsClient::new();
        sops.expect_decrypt().never();
        let provider = get_provider(sops);
        // Act
        let secrets = provider.get(&stack).expect("no secrets should succeed");
        // Assert
        assert_eq!(secrets, Secrets::default());
    }

    fn get_provider(sops: MockSopsClient) -> Arc<SecretsProvider> {
        ServiceBuilder::new()
            .with_mock_services(
                MockComposeConfigReader::new(),
                MockComposeRunner::new(),
                sops,
            )
            .build()
            .expect::<SecretsProvider>()
    }

    fn mock_stack(directory: &TempDir) -> Stack {
        let mut stack = Stack::mock();
        stack.directory = directory.path().to_path_buf();
        stack
    }

    fn mock_secrets() -> Secrets {
        Secrets::from_str("API_TOKEN=a\nMAIL_PASSWORD=b\nTLS_KEY=c").expect("dotenv should parse")
    }
}
