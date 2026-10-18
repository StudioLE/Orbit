//! Integration tests running the built `orbit` binary against local Docker.
//!
//! - Ignored by default; run with `cargo test --workspace -- --ignored`
//! - Need `docker`, `sops` and `age-keygen`

use std::fs;
use std::path::{Path, PathBuf};
use std::process::{Command, Output};
use tempfile::{Builder, TempDir};

/// Value of `API_TOKEN`, a short `x-orbit.environment` entry.
const SECRET: &str = "integration-secret-value";

/// Value of `MAIL_PASSWORD`, a long `x-orbit.environment` entry targeting `SMTP_PASSWORD`.
const RENAMED_SECRET: &str = "integration-renamed-secret";

/// Value of `TLS_KEY`, a short `x-orbit.secrets` entry.
const FILE_SECRET: &str = "integration-file-secret";

/// Value of `SIGNING_KEY`, a long `x-orbit.secrets` entry targeting `/etc/app/signing.key`.
const TARGET_FILE_SECRET: &str = "integration-target-file-secret";

/// Orbit puts config content and secrets in the container, and keeps `$` literal.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn orbit_up() {
    // Arrange
    let stack = TestStack::new();
    // Act
    let output = stack.orbit(&["up", "--detach"]);
    // Assert
    assert_success(&output);
    let id = stack.get_container_id();
    let config = stack.exec(&id, &["cat", "/etc/app.conf"]);
    assert_eq!(config, "greeting = hello\nhome = $HOME\n");
    let secret = stack.exec(&id, &["printenv", "API_TOKEN"]);
    assert_eq!(secret, format!("{SECRET}\n"));
}

/// Orbit puts `x-orbit.environment` in the container env and `x-orbit.secrets` in files only.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn orbit_up_secrets() {
    // Arrange
    let stack = TestStack::new();
    // Act
    let output = stack.orbit(&["up", "--detach"]);
    // Assert
    assert_success(&output);
    let id = stack.get_container_id();
    let renamed = stack.exec(&id, &["printenv", "SMTP_PASSWORD"]);
    assert_eq!(renamed, format!("{RENAMED_SECRET}\n"));
    let file = stack.exec(&id, &["cat", "/run/secrets/TLS_KEY"]);
    assert_eq!(file, FILE_SECRET);
    let target_file = stack.exec(&id, &["cat", "/etc/app/signing.key"]);
    assert_eq!(target_file, TARGET_FILE_SECRET);
    let env = stack.get_container_env(&id);
    for value in [FILE_SECRET, TARGET_FILE_SECRET] {
        assert!(
            !env.contains(value),
            "file secret should not be in env: {env}"
        );
    }
}

/// Orbit loads the stack with the shell environment, so `COMPOSE_PROJECT_NAME` names the project.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn orbit_up_project_name_from_env() {
    // Arrange
    let mut stack = TestStack::new();
    stack.name = format!("{}-custom", stack.name);
    // Act
    let output = stack.orbit_with_env(
        &[("COMPOSE_PROJECT_NAME", &stack.name)],
        &["up", "--detach"],
    );
    // Assert
    assert_success(&output);
    stack.get_container_id();
}

/// Orbit recreates the container when only config content changes.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn orbit_up_config_change() {
    // Arrange
    let stack = TestStack::new();
    assert_success(&stack.orbit(&["up", "--detach"]));
    let first = stack.get_container_id();
    assert_success(&stack.orbit(&["up", "--detach"]));
    let unchanged = stack.get_container_id();
    fs::write(
        stack.directory.path().join("app.conf"),
        "greeting = changed\n",
    )
    .expect("should write config");
    // Act
    let output = stack.orbit(&["up", "--detach"]);
    // Assert
    assert_success(&output);
    let changed = stack.get_container_id();
    assert_eq!(
        first, unchanged,
        "unchanged config should keep the container"
    );
    assert_ne!(
        first, changed,
        "changed config should recreate the container"
    );
    let config = stack.exec(&changed, &["cat", "/etc/app.conf"]);
    assert_eq!(config, "greeting = changed\n");
}

/// Plain `docker compose up` is refused and leaves the running container untouched.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn compose_up_refused() {
    // Arrange
    let stack = TestStack::new();
    assert_success(&stack.orbit(&["up", "--detach"]));
    let before = stack.get_container_id();
    // Act
    let output = stack.compose(&["up", "--detach"]);
    // Assert
    assert!(!output.status.success(), "plain up should fail");
    let stderr = String::from_utf8_lossy(&output.stderr);
    assert!(
        stderr.contains("unsupported external secret orbit"),
        "{stderr}"
    );
    let after = stack.get_container_id();
    assert_eq!(before, after);
}

/// `orbit run` passes arguments after the service to the container verbatim.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn orbit_run() {
    // Arrange
    let stack = TestStack::new();
    // Act
    let output = stack.orbit(&["run", "--rm", "app", "echo", "hello", "--loud", "-x"]);
    // Assert
    assert_success(&output);
    assert_eq!(String::from_utf8_lossy(&output.stdout), "hello --loud -x\n");
}

/// `orbit run` exits with the container's exit code.
#[test]
#[ignore = "needs docker, sops and age-keygen"]
fn orbit_run_exit_code() {
    // Arrange
    let stack = TestStack::new();
    // Act
    let output = stack.orbit(&["run", "--rm", "app", "sh", "-c", "exit 3"]);
    // Assert
    assert_eq!(output.status.code(), Some(3));
}

/// Copy of the fixture stack with an encrypted `.sops.env` and a throwaway age key.
///
/// - Runs `docker compose down` on drop
struct TestStack {
    directory: TempDir,
    keys: TempDir,
    host: String,
    name: String,
}

impl TestStack {
    fn new() -> Self {
        let directory = Builder::new()
            .prefix("orbit-it-")
            .tempdir()
            .expect("should create stack dir");
        let fixture = Path::new(env!("CARGO_MANIFEST_DIR")).join("tests/fixtures/stack");
        for name in ["docker-compose.yml", "app.conf"] {
            fs::copy(fixture.join(name), directory.path().join(name)).expect("should copy fixture");
        }
        let keys = TempDir::new().expect("should create key dir");
        let public_key = create_key(keys.path());
        encrypt_secrets(keys.path(), directory.path(), &public_key);
        let name = directory
            .path()
            .file_name()
            .expect("temp dir should have a name")
            .to_string_lossy()
            .to_lowercase();
        Self {
            directory,
            keys,
            host: get_docker_host(),
            name,
        }
    }

    fn get_key_path(&self) -> PathBuf {
        self.keys.path().join("key.txt")
    }

    fn orbit(&self, arguments: &[&str]) -> Output {
        self.orbit_with_env(&[], arguments)
    }

    fn orbit_with_env(&self, env: &[(&str, &str)], arguments: &[&str]) -> Output {
        Command::new(env!("CARGO_BIN_EXE_orbit"))
            .args(["--host", &self.host])
            .args(arguments)
            .current_dir(self.directory.path())
            .env("SOPS_AGE_KEY_FILE", self.get_key_path())
            .envs(env.iter().copied())
            .output()
            .expect("should run orbit")
    }

    fn compose(&self, arguments: &[&str]) -> Output {
        Command::new("docker")
            .args(["compose", "-p", &self.name])
            .args(arguments)
            .current_dir(self.directory.path())
            .env("DOCKER_HOST", &self.host)
            .output()
            .expect("should run docker compose")
    }

    fn get_container_id(&self) -> String {
        let output = self.compose(&["ps", "--quiet", "app"]);
        assert_success(&output);
        let id = String::from_utf8_lossy(&output.stdout).trim().to_owned();
        assert!(!id.is_empty(), "app container should be running");
        id
    }

    fn get_container_env(&self, id: &str) -> String {
        let output = Command::new("docker")
            .args(["inspect", "--format", "{{json .Config.Env}}", id])
            .env("DOCKER_HOST", &self.host)
            .output()
            .expect("should run docker inspect");
        assert_success(&output);
        String::from_utf8_lossy(&output.stdout).into_owned()
    }

    fn exec(&self, id: &str, arguments: &[&str]) -> String {
        let output = Command::new("docker")
            .args(["exec", id])
            .args(arguments)
            .env("DOCKER_HOST", &self.host)
            .output()
            .expect("should run docker exec");
        assert_success(&output);
        String::from_utf8_lossy(&output.stdout).into_owned()
    }
}

impl Drop for TestStack {
    fn drop(&mut self) {
        let output = self.compose(&["down", "--remove-orphans", "--timeout", "0"]);
        if !output.status.success() {
            eprintln!(
                "docker compose down failed: {}",
                String::from_utf8_lossy(&output.stderr)
            );
        }
    }
}

fn create_key(directory: &Path) -> String {
    let output = Command::new("age-keygen")
        .arg("-o")
        .arg(directory.join("key.txt"))
        .output()
        .expect("should run age-keygen");
    assert_success(&output);
    String::from_utf8_lossy(&output.stderr)
        .split_whitespace()
        .find(|word| word.starts_with("age1"))
        .expect("age-keygen should print the public key")
        .to_owned()
}

fn encrypt_secrets(keys: &Path, stack: &Path, public_key: &str) {
    let plain = keys.join("plain.env");
    fs::write(
        &plain,
        format!(
            "API_TOKEN={SECRET}\nMAIL_PASSWORD={RENAMED_SECRET}\nTLS_KEY={FILE_SECRET}\nSIGNING_KEY={TARGET_FILE_SECRET}\n"
        ),
    )
    .expect("should write plain secrets");
    let output = Command::new("sops")
        .args(["encrypt", "--age", public_key])
        .args(["--input-type", "dotenv", "--output-type", "dotenv"])
        .arg(&plain)
        .current_dir(keys)
        .output()
        .expect("should run sops");
    assert_success(&output);
    fs::write(stack.join(".sops.env"), &output.stdout).expect("should write .sops.env");
}

fn get_docker_host() -> String {
    let output = Command::new("docker")
        .args([
            "context",
            "inspect",
            "--format",
            "{{.Endpoints.docker.Host}}",
        ])
        .output()
        .expect("should run docker context inspect");
    assert_success(&output);
    String::from_utf8_lossy(&output.stdout).trim().to_owned()
}

fn assert_success(output: &Output) {
    assert!(
        output.status.success(),
        "exit {:?}\nstdout: {}\nstderr: {}",
        output.status.code(),
        String::from_utf8_lossy(&output.stdout),
        String::from_utf8_lossy(&output.stderr)
    );
}
