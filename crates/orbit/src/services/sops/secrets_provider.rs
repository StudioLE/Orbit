//! Decrypt the secrets a [`Stack`] needs.

use crate::prelude::*;

/// Name of the sops-encrypted dotenv file in the stack directory.
const SOPS_FILE: &str = ".sops.env";

/// Decrypt the secrets a [`Stack`] needs from its `.sops.env`.
#[derive(FromServices)]
pub struct SecretsProvider {
    sops: Arc<dyn SopsClient>,
    parser: Arc<SecretsParser>,
}

impl SecretsProvider {
    /// Decrypt and parse `.sops.env` in the stack directory, or return no secrets if none are needed.
    ///
    /// - Fails with every needed secret name attached if `.sops.env` is missing
    pub fn get(&self, stack: &Stack) -> Result<Secrets, Report<SecretsProviderError>> {
        let names = stack.settings.get_secret_names();
        if names.is_empty() {
            return Ok(Secrets::default());
        }
        let path = stack.directory.join(SOPS_FILE);
        if !path.exists() {
            let report = names.into_iter().fold(
                Report::new(SecretsProviderError::SopsFile).attach_path(&path),
                |report, name| report.attach("secret", name),
            );
            return Err(report);
        }
        let text = self
            .sops
            .decrypt(&path)
            .change_context(SecretsProviderError::Decrypt)?;
        self.parser
            .parse(&text)
            .change_context(SecretsProviderError::Parse)
            .attach_path(&path)
    }
}

/// Errors returned by [`SecretsProvider`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum SecretsProviderError {
    /// The stack needs secrets but has no `.sops.env`.
    #[error("Stack needs secrets but has no `.sops.env`")]
    SopsFile,
    /// Unable to decrypt secrets.
    #[error("Unable to decrypt secrets")]
    Decrypt,
    /// Unable to parse decrypted secrets.
    #[error("Unable to parse decrypted secrets")]
    Parse,
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;

    /// [`SecretsProvider::get`] decrypts `.sops.env` in the stack directory.
    #[test]
    fn secrets_provider_get() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let stack = Stack {
            directory: directory.path().to_path_buf(),
            ..Stack::fixture()
        };
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_trait::<dyn SopsClient, SopsClientSpy>()
            .build();
        let provider = services.expect::<SecretsProvider>();
        // Act
        let secrets = provider.get(&stack).expect("secrets should decrypt");
        // Assert
        let calls = services.expect::<SopsClientSpy>().calls();
        assert_eq!(calls, vec![directory.path().join(SOPS_FILE)]);
        assert_eq!(secrets, Secrets::fixture());
    }

    /// [`SecretsProvider::get`] lists secret names when `.sops.env` is absent.
    #[test]
    fn secrets_provider_get_sops_file_missing() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        fs::remove_file(directory.path().join(SOPS_FILE)).expect("should remove .sops.env");
        let stack = Stack {
            directory: directory.path().to_path_buf(),
            ..Stack::fixture()
        };
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_trait::<dyn SopsClient, SopsClientSpy>()
            .build();
        let provider = services.expect::<SecretsProvider>();
        // Act
        let report = provider
            .get(&stack)
            .expect_err("missing .sops.env should fail");
        // Assert
        assert_eq!(report.current_context(), &SecretsProviderError::SopsFile);
        let display = report.to_string();
        for name in ["API_TOKEN", "MAIL_PASSWORD", "TLS_KEY"] {
            assert!(display.contains(&format!("secret: {name}")), "{display}");
        }
        let calls = services.expect::<SopsClientSpy>().calls();
        assert_eq!(calls, Vec::<PathBuf>::new());
    }

    /// [`SecretsProvider::get`] fails when the decrypted text is not dotenv.
    #[test]
    fn secrets_provider_get_parse() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let stack = Stack {
            directory: directory.path().to_path_buf(),
            ..Stack::fixture()
        };
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_instance(SopsClientStub {
                text: DecryptedText::from("bad".to_owned()),
            })
            .build();
        let provider = services.expect::<SecretsProvider>();
        // Act
        let report = provider.get(&stack).expect_err("bad dotenv should fail");
        // Assert
        assert_eq!(report.current_context(), &SecretsProviderError::Parse);
        let display = report.to_string();
        let path = directory.path().join(SOPS_FILE);
        assert!(display.contains(&path.display().to_string()), "{display}");
    }

    /// [`SecretsProvider::get`] skips sops when the stack needs no secrets.
    #[test]
    fn secrets_provider_get_no_secrets() {
        // Arrange
        let project = json!({ "services": { "app": {} } });
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_trait::<dyn SopsClient, SopsClientSpy>()
            .build();
        let provider = services.expect::<SecretsProvider>();
        // Act
        let secrets = provider.get(&stack).expect("no secrets should succeed");
        // Assert
        assert_eq!(secrets, Secrets::default());
        let calls = services.expect::<SopsClientSpy>().calls();
        assert_eq!(calls, Vec::<PathBuf>::new());
    }
}
