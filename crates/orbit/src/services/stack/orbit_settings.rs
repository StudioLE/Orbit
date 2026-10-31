//! What Orbit reads from a Compose project.

use crate::prelude::*;

/// Name of the external secret that makes plain `docker compose up` fail.
pub const GUARD_SECRET: &str = "orbit";

/// Name of the Compose extension key Orbit reads.
pub const EXTENSION_KEY: &str = "x-orbit";

/// Directory Compose mounts secret files in.
pub const SECRETS_DIRECTORY: &str = "/run/secrets/";

/// What Orbit reads from a Compose project.
#[derive(Clone, Debug, Default, Eq, PartialEq, Serialize)]
pub struct OrbitSettings {
    /// Docker host URL from top-level `x-orbit.host`.
    pub host: Option<String>,
    /// Settings per service, by service name.
    pub services: BTreeMap<String, ServiceSettings>,
    /// Problems that do not stop a deploy, by service then top-level secret.
    pub warnings: Vec<SettingsWarning>,
}

impl OrbitSettings {
    /// Create [`OrbitSettings`] from the JSON of `docker compose config`.
    pub fn from_project(project: &Value) -> Result<Self, Report<OrbitSettingsError>> {
        let view = ProjectView::deserialize(project).change_context(OrbitSettingsError::Parse)?;
        let mut warnings = Vec::new();
        let services = view
            .services
            .into_iter()
            .map(|(name, service)| {
                let settings = service.into_settings(&name, &mut warnings);
                (name, settings)
            })
            .collect();
        warnings.extend(view.secrets.into_iter().filter_map(|(secret, view)| {
            view.environment
                .map(|variable| SettingsWarning::EnvironmentSecret { secret, variable })
        }));
        Ok(Self {
            host: view.extension.and_then(|extension| extension.host),
            services,
            warnings,
        })
    }

    /// Every name Orbit reads from `.sops.env`.
    ///
    /// - Merges the source of every `x-orbit.environment` and `x-orbit.secrets` entry
    pub fn get_secret_names(&self) -> BTreeSet<String> {
        self.services
            .values()
            .flat_map(|service| service.environment.iter().chain(&service.secrets))
            .map(|entry| entry.source.clone())
            .collect()
    }
}

#[derive(Deserialize)]
struct ProjectView {
    #[serde(default, rename = "x-orbit")]
    extension: Option<ProjectExtension>,
    #[serde(default)]
    secrets: BTreeMap<String, SecretView>,
    #[serde(default)]
    services: BTreeMap<String, ServiceView>,
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct ProjectExtension {
    host: Option<String>,
}

#[derive(Deserialize)]
struct SecretView {
    environment: Option<String>,
}

#[derive(Deserialize)]
struct ServiceView {
    #[serde(default)]
    environment: BTreeMap<String, Value>,
    #[serde(default, rename = "x-orbit")]
    extension: Option<ServiceExtension>,
}

#[derive(Default, Deserialize)]
#[serde(deny_unknown_fields)]
struct ServiceExtension {
    #[serde(default)]
    environment: Vec<SecretEntryView>,
    #[serde(default)]
    secrets: Vec<SecretEntryView>,
    #[serde(default)]
    configs: Vec<ConfigFile>,
}

/// Entry in `x-orbit.environment` or `x-orbit.secrets`, as a name or a `source` and `target`.
#[derive(Deserialize)]
#[serde(untagged)]
enum SecretEntryView {
    Name(String),
    Entry(SecretEntry),
}

impl SecretEntryView {
    /// Resolve to a [`SecretEntry`], giving a name the target `target_prefix` + name.
    fn into_entry(self, target_prefix: &str) -> SecretEntry {
        match self {
            Self::Name(name) => SecretEntry {
                target: format!("{target_prefix}{name}"),
                source: name,
            },
            Self::Entry(entry) => entry,
        }
    }
}

impl ServiceView {
    /// Resolve to [`ServiceSettings`], pushing a warning per bare or replaced `environment` key.
    fn into_settings(
        self,
        service_name: &str,
        warnings: &mut Vec<SettingsWarning>,
    ) -> ServiceSettings {
        let extension = self.extension.unwrap_or_default();
        let settings = ServiceSettings {
            environment: extension
                .environment
                .into_iter()
                .map(|entry| entry.into_entry(""))
                .collect(),
            secrets: extension
                .secrets
                .into_iter()
                .map(|entry| entry.into_entry(SECRETS_DIRECTORY))
                .collect(),
            configs: extension.configs,
        };
        for (name, value) in self.environment {
            let service = service_name.to_owned();
            let warning = if settings
                .environment
                .iter()
                .any(|entry| entry.target == name)
            {
                SettingsWarning::Replaced { service, name }
            } else if value.is_null() {
                SettingsWarning::BareName { service, name }
            } else {
                continue;
            };
            warnings.push(warning);
        }
        settings
    }
}

/// Errors returned by [`OrbitSettings`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum OrbitSettingsError {
    /// The project does not match the expected shape.
    #[error("Unable to read Orbit settings from the Compose project")]
    Parse,
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`OrbitSettings::from_project`] reads hosts, configs and bare names.
    #[test]
    fn orbit_settings_from_project() {
        // Arrange
        let project = Stack::fixture_project();
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        assert_yaml_snapshot!(settings);
    }

    /// [`OrbitSettings::from_project`] rejects an unknown key under `x-orbit`.
    #[test]
    fn orbit_settings_from_project_unknown_key() {
        // Arrange
        let mut project = Stack::fixture_project();
        project
            .expect_object_mut("/x-orbit")
            .insert("hots".to_owned(), json!("ssh://typo"));
        // Act
        let report = OrbitSettings::from_project(&project).expect_err("typo should fail");
        // Assert
        assert_eq!(report.current_context(), &OrbitSettingsError::Parse);
    }

    /// [`OrbitSettings::from_project`] gives a short `x-orbit.environment` entry the same name as target.
    #[test]
    fn orbit_settings_from_project_environment_short() {
        // Arrange
        let project = json!({ "services": { "app": {
            "image": "alpine",
            "x-orbit": { "environment": ["API_TOKEN"] }
        }}});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let expected = vec![SecretEntry {
            source: "API_TOKEN".to_owned(),
            target: "API_TOKEN".to_owned(),
        }];
        assert_eq!(get_app(&settings).environment, expected);
    }

    /// [`OrbitSettings::from_project`] gives a short `x-orbit.secrets` entry a target in `/run/secrets/`.
    #[test]
    fn orbit_settings_from_project_secrets_short() {
        // Arrange
        let project = json!({ "services": { "app": {
            "image": "alpine",
            "x-orbit": { "secrets": ["TLS_KEY"] }
        }}});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let expected = vec![SecretEntry {
            source: "TLS_KEY".to_owned(),
            target: "/run/secrets/TLS_KEY".to_owned(),
        }];
        assert_eq!(get_app(&settings).secrets, expected);
    }

    /// [`OrbitSettings::from_project`] keeps the target of a long entry.
    #[test]
    fn orbit_settings_from_project_long() {
        // Arrange
        let project = json!({ "services": { "app": {
            "image": "alpine",
            "x-orbit": {
                "environment": [{ "source": "MAIL_PASSWORD", "target": "SMTP_PASSWORD" }],
                "secrets": [{ "source": "SIGNING_KEY", "target": "/etc/app/signing.key" }]
            }
        }}});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let service = get_app(&settings);
        let environment = vec![SecretEntry {
            source: "MAIL_PASSWORD".to_owned(),
            target: "SMTP_PASSWORD".to_owned(),
        }];
        let secrets = vec![SecretEntry {
            source: "SIGNING_KEY".to_owned(),
            target: "/etc/app/signing.key".to_owned(),
        }];
        assert_eq!(service.environment, environment);
        assert_eq!(service.secrets, secrets);
    }

    /// [`OrbitSettings::from_project`] rejects a long entry without a target.
    #[test]
    fn orbit_settings_from_project_long_missing_target() {
        // Arrange
        let project = json!({ "services": { "app": {
            "image": "alpine",
            "x-orbit": { "secrets": [{ "source": "TLS_KEY" }] }
        }}});
        // Act
        let report = OrbitSettings::from_project(&project).expect_err("entry should fail");
        // Assert
        assert_eq!(report.current_context(), &OrbitSettingsError::Parse);
    }

    /// [`OrbitSettings::from_project`] reads a number or text `mode` on an `x-orbit.configs` entry.
    #[test]
    fn orbit_settings_from_project_config_mode() {
        // Arrange
        let project = json!({ "services": { "app": {
            "x-orbit": { "configs": [
                { "source": "a.sh", "target": "/a.sh", "mode": 493 },
                { "source": "b.sh", "target": "/b.sh", "mode": "0755" }
            ]}
        }}});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let configs = serde_json::to_value(&get_app(&settings).configs).expect("should serialize");
        let expected = json!([
            { "source": "a.sh", "target": "/a.sh", "mode": 493 },
            { "source": "b.sh", "target": "/b.sh", "mode": "0755" }
        ]);
        assert_eq!(configs, expected);
    }

    /// [`OrbitSettings::from_project`] warns on an `environment` key with no value.
    #[test]
    fn orbit_settings_from_project_warn_bare_name() {
        // Arrange
        let project = json!({ "services": { "app": {
            "environment": { "GIT_SHA": null, "LOG_LEVEL": "info" }
        }}});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let expected = vec![SettingsWarning::BareName {
            service: "app".to_owned(),
            name: "GIT_SHA".to_owned(),
        }];
        assert_eq!(settings.warnings, expected);
    }

    /// [`OrbitSettings::from_project`] warns on a top-level secret with an `environment` source.
    #[test]
    fn orbit_settings_from_project_warn_environment_secret() {
        // Arrange
        let project = json!({ "secrets": {
            "orbit": { "external": true },
            "tls": { "environment": "TLS_KEY" }
        }});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let expected = vec![SettingsWarning::EnvironmentSecret {
            secret: "tls".to_owned(),
            variable: "TLS_KEY".to_owned(),
        }];
        assert_eq!(settings.warnings, expected);
    }

    /// [`OrbitSettings::from_project`] warns once on an `environment` key also in `x-orbit.environment`.
    #[test]
    fn orbit_settings_from_project_warn_replaced() {
        // Arrange
        let project = json!({ "services": { "app": {
            "environment": { "API_TOKEN": null, "SMTP_PASSWORD": "plain" },
            "x-orbit": { "environment": [
                "API_TOKEN",
                { "source": "MAIL_PASSWORD", "target": "SMTP_PASSWORD" }
            ]}
        }}});
        // Act
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Assert
        let expected = ["API_TOKEN", "SMTP_PASSWORD"].map(|name| SettingsWarning::Replaced {
            service: "app".to_owned(),
            name: name.to_owned(),
        });
        assert_eq!(settings.warnings, expected);
    }

    /// [`OrbitSettings::get_secret_names`] skips bare `environment` keys and `environment` secrets.
    #[test]
    fn orbit_settings_get_secret_names_old_style() {
        // Arrange
        let project = json!({
            "services": { "app": { "environment": { "API_TOKEN": null } } },
            "secrets": { "tls": { "environment": "TLS_KEY" } }
        });
        let settings = OrbitSettings::from_project(&project).expect("project should parse");
        // Act
        let names = settings.get_secret_names();
        // Assert
        assert!(names.is_empty(), "{names:?}");
    }

    /// [`OrbitSettings::get_secret_names`] returns the source of every entry in every service.
    #[test]
    fn orbit_settings_get_secret_names() {
        // Arrange
        let settings =
            OrbitSettings::from_project(&Stack::fixture_project()).expect("project should parse");
        // Act
        let names = settings.get_secret_names();
        // Assert
        let expected = ["API_TOKEN", "MAIL_PASSWORD", "TLS_KEY"].map(str::to_owned);
        assert_eq!(names, BTreeSet::from(expected));
    }

    fn get_app(settings: &OrbitSettings) -> &ServiceSettings {
        settings.services.get("app").expect("app should exist")
    }
}
