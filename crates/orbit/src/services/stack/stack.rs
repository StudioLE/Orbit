//! Compose project loaded from a stack directory.

use crate::prelude::*;

/// Compose project loaded from a stack directory.
#[derive(Clone, Debug, PartialEq)]
pub struct Stack {
    /// Compose project name.
    pub name: String,
    /// Directory holding `docker-compose.yml`.
    pub directory: PathBuf,
    /// Project JSON from `docker compose config`, without interpolation.
    pub project: Value,
    /// What Orbit reads from the project.
    pub settings: OrbitSettings,
}

impl Stack {
    /// Create a [`Stack`] from a Compose `project`, parsing its [`OrbitSettings`].
    pub fn new(
        name: impl Into<String>,
        directory: PathBuf,
        project: Value,
    ) -> Result<Self, Report<OrbitSettingsError>> {
        let settings = OrbitSettings::from_project(&project)?;
        Ok(Self {
            name: name.into(),
            directory,
            project,
            settings,
        })
    }
}

#[cfg(test)]
mod mocks {
    use super::*;
    use std::fs::{self, create_dir, read_dir};
    use tempfile::TempDir;

    impl Stack {
        /// Create a mock [`Stack`] from the fixture project and directory.
        pub fn mock() -> Self {
            Self::mock_with(Self::mock_project())
        }

        /// Create a mock [`Stack`] from `project` in the fixture directory.
        pub fn mock_with(project: Value) -> Self {
            Self::new("hello", Self::mock_directory(), project).expect("project should parse")
        }

        /// Fixture project JSON.
        pub fn mock_project() -> Value {
            serde_json::from_str(include_str!("fixtures/project.json"))
                .expect("fixture should be valid JSON")
        }

        /// Fixture project JSON without the `orbit` guard secret.
        pub fn mock_project_without_guard() -> Value {
            let mut project = Self::mock_project();
            project.expect_object_mut("/secrets").remove(GUARD_SECRET);
            for service in project.expect_object_mut("/services").values_mut() {
                if let Some(Value::Array(secrets)) = service.get_mut("secrets") {
                    secrets.retain(|secret| {
                        secret.get("source").and_then(Value::as_str) != Some(GUARD_SECRET)
                    });
                }
            }
            project
        }

        /// Fixture directory holding the config files of the fixture project.
        pub fn mock_directory() -> PathBuf {
            Path::new(env!("CARGO_MANIFEST_DIR")).join("src/services/stack/fixtures/stack")
        }

        /// Temp copy of [`Stack::mock_directory`] with a placeholder `.sops.env`.
        pub fn mock_temp_directory() -> TempDir {
            let directory = TempDir::new().expect("should create temp dir");
            copy_directory(&Self::mock_directory(), directory.path());
            fs::write(directory.path().join(".sops.env"), "").expect("should write .sops.env");
            directory
        }
    }

    fn copy_directory(source: &Path, destination: &Path) {
        for entry in read_dir(source).expect("should read fixture directory") {
            let entry = entry.expect("should read fixture entry");
            let target = destination.join(entry.file_name());
            if entry.path().is_dir() {
                create_dir(&target).expect("should create directory");
                copy_directory(&entry.path(), &target);
            } else {
                fs::copy(entry.path(), &target).expect("should copy file");
            }
        }
    }
}
