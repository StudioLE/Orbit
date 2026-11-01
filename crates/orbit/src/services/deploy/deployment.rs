//! A [`Stack`] bound to a Docker host, ready to run with Compose.

use crate::prelude::*;
use tempfile::NamedTempFile;

/// A [`Stack`] bound to a Docker host, with its secrets and plain Compose file.
///
/// - Holds everything `docker compose` needs to run against the host
/// - Deletes the temp file on drop
#[derive(Debug)]
pub struct Deployment {
    /// Compose project name.
    pub name: String,
    /// Stack directory, passed as `--project-directory`.
    pub directory: PathBuf,
    /// Docker host URL.
    pub host: String,
    /// Temp file holding the transformed project.
    pub file: NamedTempFile,
    /// Decrypted secrets.
    pub secrets: Secrets,
}

impl Deployment {
    /// Create a [`ComposeInvocation`] running `arguments` against this project.
    ///
    /// - Prepends `-p`, `-f` and `--project-directory`
    pub fn to_invocation(&self, arguments: Vec<String>) -> ComposeInvocation {
        let mut all = vec![
            "-p".to_owned(),
            self.name.clone(),
            "-f".to_owned(),
            self.file.path().display().to_string(),
            "--project-directory".to_owned(),
            self.directory.display().to_string(),
        ];
        all.extend(arguments);
        ComposeInvocation {
            arguments: all,
            host: self.host.clone(),
            secrets: self.secrets.clone(),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`Deployment::to_invocation`] prepends the project name, file and directory.
    #[test]
    fn deployment_to_invocation() {
        // Arrange
        let file = NamedTempFile::new().expect("should create temp file");
        let path = file.path().display().to_string();
        let secrets = Secrets::fixture_api_token();
        let deployment = Deployment {
            name: "hello".to_owned(),
            directory: PathBuf::from("/stack"),
            host: "ssh://example-host".to_owned(),
            file,
            secrets: secrets.clone(),
        };
        // Act
        let invocation = deployment.to_invocation(vec!["up".to_owned()]);
        // Assert
        let expected = [
            "-p",
            "hello",
            "-f",
            path.as_str(),
            "--project-directory",
            "/stack",
            "up",
        ];
        assert_eq!(invocation.arguments, expected);
        assert_eq!(invocation.host, "ssh://example-host");
        assert_eq!(invocation.secrets, secrets);
    }
}
