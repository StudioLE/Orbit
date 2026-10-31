//! Create a [`Deployment`] from the stack in the working directory.

use crate::prelude::*;
use serde_json::to_writer_pretty;
use std::env::{temp_dir, var_os};
use std::fs::Permissions;
use std::os::unix::fs::PermissionsExt;
use tempfile::{Builder, NamedTempFile};

/// Create a [`Deployment`] from the stack in the working directory.
#[derive(FromServices)]
pub struct DeploymentProvider {
    stacks: Arc<StackProvider>,
    hosts: Arc<DockerHostProvider>,
    secrets: Arc<SecretsProvider>,
    validator: Arc<StackValidator>,
    transformer: Arc<StackTransformer>,
}

impl DeploymentProvider {
    /// Create a [`Deployment`] from the stack in the [`WorkingDirectory`].
    ///
    /// 1. Loads the stack with [`StackProvider::get`]
    /// 2. Resolves the Docker host with [`DockerHostProvider::get`]
    /// 3. Decrypts the secrets it needs with [`SecretsProvider::get`]
    /// 4. Checks the stack against the secrets with [`StackValidator::validate`]
    /// 5. Rewrites `x-orbit` into plain Compose with [`StackTransformer::transform`]
    /// 6. Writes the project to a `0600` temp file in `$XDG_RUNTIME_DIR`, else the system temp dir
    pub fn get(&self) -> Result<Deployment, Report<DeploymentError>> {
        let stack = self.stacks.get().change_context(DeploymentError::Load)?;
        let host = self
            .hosts
            .get(&stack.settings)
            .change_context(DeploymentError::Host)?;
        let secrets = self
            .secrets
            .get(&stack)
            .change_context(DeploymentError::Secrets)?;
        self.validator
            .validate(&stack, &secrets)
            .change_context(DeploymentError::Validate)?;
        let project = self
            .transformer
            .transform(&stack)
            .change_context(DeploymentError::Transform)?;
        let file = write_project(&stack.name, &project)?;
        debug!(path = %file.path().display(), "Wrote Compose project");
        Ok(Deployment {
            name: stack.name,
            directory: stack.directory,
            host,
            file,
            secrets,
        })
    }
}

/// Write `project` as JSON to a `0600` temp file named `orbit-<name>-*.json`.
fn write_project(name: &str, project: &Value) -> Result<NamedTempFile, Report<DeploymentError>> {
    let directory = get_runtime_directory();
    let mut file = create_private_file(name, &directory)
        .change_context(DeploymentError::Create)
        .attach_path(&directory)?;
    write_json(&mut file, project)
        .change_context(DeploymentError::Write)
        .attach_path(file.path())?;
    Ok(file)
}

/// Directory for runtime files: `$XDG_RUNTIME_DIR`, else the system temp dir.
fn get_runtime_directory() -> PathBuf {
    var_os("XDG_RUNTIME_DIR")
        .filter(|value| !value.is_empty())
        .map_or_else(temp_dir, PathBuf::from)
}

/// Create a `0600` temp file named `orbit-<name>-*.json` in `directory`.
fn create_private_file(name: &str, directory: &Path) -> io::Result<NamedTempFile> {
    Builder::new()
        .prefix(&format!("orbit-{name}-"))
        .suffix(".json")
        .permissions(Permissions::from_mode(0o600))
        .tempfile_in(directory)
}

/// Write `value` as pretty JSON to `writer` and flush it.
fn write_json(writer: &mut impl Write, value: &Value) -> io::Result<()> {
    to_writer_pretty(&mut *writer, value)?;
    writer.flush()
}

/// Errors returned by [`DeploymentProvider`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum DeploymentError {
    /// Unable to load the stack.
    #[error("Unable to load the stack")]
    Load,
    /// No Docker host.
    #[error("Unable to resolve the Docker host")]
    Host,
    /// Unable to get secrets.
    #[error("Unable to get secrets")]
    Secrets,
    /// The stack is invalid.
    #[error("Unable to validate the stack")]
    Validate,
    /// Unable to transform the stack.
    #[error("Unable to transform the stack")]
    Transform,
    /// Unable to create the temp file.
    #[error("Unable to create the Compose project file")]
    Create,
    /// Unable to write the temp file.
    #[error("Unable to write the Compose project")]
    Write,
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;

    /// [`DeploymentProvider::get`] loads, then decrypts, then writes a private temp file.
    #[test]
    fn deployment_provider_get() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_trait::<dyn SopsClient, SopsClientSpy>()
            .with_instance(WorkingDirectory::new(directory.path().to_path_buf()))
            .build();
        let provider = services.expect::<DeploymentProvider>();
        // Act
        let deployment = provider.get().expect("stack should prepare");
        // Assert
        let calls = services.expect::<SopsClientSpy>().calls();
        assert_eq!(calls, vec![directory.path().join(".sops.env")]);
        assert_eq!(deployment.name, "hello");
        assert_eq!(deployment.host, "ssh://example-host");
        assert_eq!(deployment.secrets, Secrets::fixture());
        let path = deployment.file.path().to_path_buf();
        let mode = fs::metadata(&path)
            .expect("file should exist")
            .permissions()
            .mode();
        assert_eq!(mode & 0o777, 0o600);
        let text = fs::read_to_string(&path).expect("file should be readable");
        let project: Value = serde_json::from_str(&text).expect("file should be JSON");
        assert!(project.get("x-orbit").is_none());
        assert!(project.pointer("/configs/orbit_web_0").is_some());
        drop(deployment);
        assert!(!path.exists(), "temp file should be deleted on drop");
    }

    /// [`DeploymentProvider::get`] stops when secrets are unavailable.
    #[test]
    fn deployment_provider_get_secrets_error() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        fs::remove_file(directory.path().join(".sops.env")).expect("should remove .sops.env");
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_trait::<dyn SopsClient, SopsClientSpy>()
            .with_instance(WorkingDirectory::new(directory.path().to_path_buf()))
            .build();
        let provider = services.expect::<DeploymentProvider>();
        // Act
        let report = provider.get().expect_err("missing .sops.env should fail");
        // Assert
        assert_eq!(report.current_context(), &DeploymentError::Secrets);
        let calls = services.expect::<SopsClientSpy>().calls();
        assert_eq!(calls, Vec::<PathBuf>::new());
    }

    /// [`DeploymentProvider::get`] stops on validation problems.
    #[test]
    fn deployment_provider_get_invalid() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_instance(SopsClientStub {
                secrets: Secrets::from_str("API_TOKEN=a").expect("dotenv should parse"),
            })
            .with_instance(WorkingDirectory::new(directory.path().to_path_buf()))
            .build();
        let provider = services.expect::<DeploymentProvider>();
        // Act
        let report = provider.get().expect_err("missing secrets should fail");
        // Assert
        assert_eq!(report.current_context(), &DeploymentError::Validate);
    }

    /// [`DeploymentProvider::get`] fails without `--host` or `x-orbit.host`.
    #[test]
    fn deployment_provider_get_host_missing() {
        // Arrange
        let directory = Stack::fixture_temp_directory();
        let mut project = Stack::fixture_project();
        project.expect_object_mut("").remove("x-orbit");
        let services = ServiceBuilder::new()
            .with_test_services()
            .with_trait::<dyn SopsClient, SopsClientSpy>()
            .with_instance(ComposeConfigReaderStub {
                project: Ok(project),
            })
            .with_instance(WorkingDirectory::new(directory.path().to_path_buf()))
            .build();
        let provider = services.expect::<DeploymentProvider>();
        // Act
        let report = provider.get().expect_err("host should be missing");
        // Assert
        assert_eq!(report.current_context(), &DeploymentError::Host);
        let calls = services.expect::<SopsClientSpy>().calls();
        assert_eq!(calls, Vec::<PathBuf>::new());
    }
}
