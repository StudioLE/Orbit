//! Rewrite a [`Stack`] into a project plain Compose can deploy.

use crate::prelude::*;
use std::fs;

/// Rewrite a [`Stack`] into a project plain Compose can deploy.
#[derive(Default, FromServices)]
pub struct StackTransformer;

impl StackTransformer {
    /// Create the project Compose deploys from `stack`.
    ///
    /// - Removes the `orbit` guard secret from services and the top level
    /// - Sets each `x-orbit.environment` target to `${SOURCE}`, replacing any existing value
    /// - Adds a top-level secret `orbit_<SOURCE>` per `x-orbit.secrets` source, shared by services
    /// - Inlines each `x-orbit.configs` file as a top-level config with `content`
    /// - Passes each `x-orbit.configs` `mode` to its config reference as written
    /// - Escapes `$` as `$$` in content so Compose does not interpolate it
    /// - Labels services that have configs with [`CONFIG_HASH_LABEL`]
    /// - Removes every `x-orbit` key
    #[expect(clippy::unused_self, reason = "stateless DI service")]
    pub fn transform(&self, stack: &Stack) -> Result<Value, Report<TransformError>> {
        let mut project = stack.project.clone();
        let root = project
            .as_object_mut()
            .ok_or_else(|| Report::new(TransformError::Shape))?;
        remove_guard(root);
        for (name, service) in &stack.settings.services {
            add_environment(root, name, &service.environment)?;
            add_secrets(root, name, &service.secrets)?;
            if service.configs.is_empty() {
                continue;
            }
            let configs = read_configs(&stack.directory, &service.configs)?;
            add_configs(root, name, &configs)?;
        }
        root.remove(EXTENSION_KEY);
        for service in get_services(root) {
            service.remove(EXTENSION_KEY);
        }
        Ok(project)
    }
}

/// Errors returned by [`StackTransformer`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum TransformError {
    /// The project does not have the shape Compose prints.
    #[error("Compose project has an unexpected shape")]
    Shape,
    /// Unable to read a config file.
    #[error("Unable to read config file")]
    Read,
    /// A config file is not UTF-8.
    #[error("Config file is not UTF-8")]
    Utf8,
    /// A generated config name is already taken.
    #[error("Config name is already taken")]
    ConfigName,
    /// A generated secret name is already taken.
    #[error("Secret name is already taken")]
    SecretName,
    /// Service labels are neither a list nor a map.
    #[error("Service labels are neither a list nor a map")]
    Labels,
}

/// Remove the `orbit` guard secret from every service and the top-level `secrets`.
///
/// - Removes `secrets` lists left empty
fn remove_guard(root: &mut Map<String, Value>) {
    for service in get_services(root) {
        if let Some(Value::Array(secrets)) = service.get_mut("secrets") {
            secrets.retain(|secret| {
                secret.get("source").and_then(Value::as_str) != Some(GUARD_SECRET)
            });
            if secrets.is_empty() {
                service.remove("secrets");
            }
        }
    }
    if let Some(Value::Object(secrets)) = root.get_mut("secrets") {
        secrets.remove(GUARD_SECRET);
        if secrets.is_empty() {
            root.remove("secrets");
        }
    }
}

/// Set each `entries` target in the service's `environment` to `${SOURCE}`.
fn add_environment(
    root: &mut Map<String, Value>,
    service_name: &str,
    entries: &[SecretEntry],
) -> Result<(), Report<TransformError>> {
    if entries.is_empty() {
        return Ok(());
    }
    let service = get_service(root, service_name)?;
    let Value::Object(environment) = service.entry("environment").or_insert_with(|| json!({}))
    else {
        return Err(Report::new(TransformError::Shape).attach("service", service_name));
    };
    for entry in entries {
        let value = format!("${{{}}}", entry.source);
        environment.insert(entry.target.clone(), json!(value));
    }
    Ok(())
}

/// Add a top-level `orbit_<SOURCE>` secret per entry and reference it from the service.
///
/// - Reuses a secret another service already added
/// - Fails with [`TransformError::SecretName`] if a different secret has the name
fn add_secrets(
    root: &mut Map<String, Value>,
    service_name: &str,
    entries: &[SecretEntry],
) -> Result<(), Report<TransformError>> {
    if entries.is_empty() {
        return Ok(());
    }
    let top_secrets = root
        .entry("secrets")
        .or_insert_with(|| json!({}))
        .as_object_mut()
        .ok_or_else(|| Report::new(TransformError::Shape))?;
    let mut references = Vec::new();
    for entry in entries {
        let secret_name = format!("orbit_{}", entry.source);
        let secret = json!({ "environment": entry.source });
        match top_secrets.get(&secret_name) {
            None => {
                top_secrets.insert(secret_name.clone(), secret);
            }
            Some(existing) if existing == &secret => {}
            Some(_) => {
                return Err(Report::new(TransformError::SecretName).attach("secret", secret_name));
            }
        }
        references.push(json!({ "source": secret_name, "target": entry.target }));
    }
    let service = get_service(root, service_name)?;
    extend_references(service, service_name, "secrets", references)
}

/// Read each config file in `configs`, resolving paths against `directory`.
fn read_configs(
    directory: &Path,
    configs: &[ConfigFile],
) -> Result<Vec<LoadedConfig>, Report<TransformError>> {
    configs
        .iter()
        .map(|config| {
            let path = config.get_path(directory);
            let bytes = fs::read(&path)
                .change_context(TransformError::Read)
                .attach_path(&path)?;
            let content = String::from_utf8(bytes)
                .change_context(TransformError::Utf8)
                .attach_path(&path)?;
            Ok(LoadedConfig {
                target: config.target.clone(),
                content,
                mode: config.mode.clone(),
            })
        })
        .collect()
}

/// Inline `configs` as top-level `orbit_<SERVICE>_<INDEX>` configs and reference them from the service.
///
/// - Escapes `$` as `$$` so Compose does not interpolate content
/// - Labels the service with [`CONFIG_HASH_LABEL`]
fn add_configs(
    root: &mut Map<String, Value>,
    service_name: &str,
    configs: &[LoadedConfig],
) -> Result<(), Report<TransformError>> {
    let top_configs = root
        .entry("configs")
        .or_insert_with(|| json!({}))
        .as_object_mut()
        .ok_or_else(|| Report::new(TransformError::Shape))?;
    let mut references = Vec::new();
    for (index, config) in configs.iter().enumerate() {
        let config_name = format!("orbit_{service_name}_{index}");
        if top_configs.contains_key(&config_name) {
            return Err(Report::new(TransformError::ConfigName).attach("config", config_name));
        }
        let content = config.content.replace('$', "$$");
        top_configs.insert(config_name.clone(), json!({ "content": content }));
        let mut reference = Map::new();
        reference.insert("source".to_owned(), json!(config_name));
        reference.insert("target".to_owned(), json!(config.target));
        if let Some(mode) = &config.mode {
            reference.insert("mode".to_owned(), json!(mode));
        }
        references.push(Value::Object(reference));
    }
    let service = get_service(root, service_name)?;
    extend_references(service, service_name, "configs", references)?;
    add_hash_label(service, service_name, &ConfigHash::new(configs))
}

/// Append `references` to the service's `key` list, creating the list when absent.
fn extend_references(
    service: &mut Map<String, Value>,
    service_name: &str,
    key: &str,
    references: Vec<Value>,
) -> Result<(), Report<TransformError>> {
    match service
        .entry(key)
        .or_insert_with(|| Value::Array(Vec::new()))
    {
        Value::Array(list) => {
            list.extend(references);
            Ok(())
        }
        _ => Err(Report::new(TransformError::Shape).attach("service", service_name)),
    }
}

/// Service `service_name` in `root`.
fn get_service<'a>(
    root: &'a mut Map<String, Value>,
    service_name: &str,
) -> Result<&'a mut Map<String, Value>, Report<TransformError>> {
    root.get_mut("services")
        .and_then(|services| services.get_mut(service_name))
        .and_then(Value::as_object_mut)
        .ok_or_else(|| Report::new(TransformError::Shape).attach("service", service_name))
}

/// Add [`CONFIG_HASH_LABEL`] in the form the service's labels are written.
fn add_hash_label(
    service: &mut Map<String, Value>,
    service_name: &str,
    hash: &ConfigHash,
) -> Result<(), Report<TransformError>> {
    match service.entry("labels").or_insert_with(|| json!({})) {
        Value::Array(labels) => labels.push(json!(format!("{CONFIG_HASH_LABEL}={hash}"))),
        Value::Object(labels) => {
            labels.insert(CONFIG_HASH_LABEL.to_owned(), json!(hash.to_string()));
        }
        _ => {
            return Err(Report::new(TransformError::Labels).attach("service", service_name));
        }
    }
    Ok(())
}

/// Each service in `root` that is a map.
fn get_services(root: &mut Map<String, Value>) -> impl Iterator<Item = &mut Map<String, Value>> {
    root.get_mut("services")
        .and_then(Value::as_object_mut)
        .into_iter()
        .flat_map(|services| services.values_mut())
        .filter_map(Value::as_object_mut)
}

#[cfg(test)]
mod tests {
    use super::*;
    use tempfile::TempDir;

    /// [`StackTransformer::transform`] rewrites the fixture stack.
    #[test]
    fn stack_transformer_transform() {
        // Arrange
        let stack = Stack::fixture();
        // Act
        let project = StackTransformer
            .transform(&stack)
            .expect("transform should succeed");
        // Assert
        assert_json_snapshot!(project);
    }

    /// [`StackTransformer::transform`] gives the same project with or without the `orbit` guard.
    #[test]
    fn stack_transformer_transform_without_guard() {
        // Arrange
        let guarded = Stack::fixture();
        let unguarded = Stack::new(
            "hello",
            Stack::fixture_directory(),
            Stack::fixture_project_without_guard(),
        )
        .expect("project should parse");
        // Act
        let expected = StackTransformer
            .transform(&guarded)
            .expect("guarded should transform");
        let project = StackTransformer
            .transform(&unguarded)
            .expect("unguarded should transform");
        // Assert
        assert_eq!(project, expected);
    }

    /// [`StackTransformer::transform`] changes the hash label when only config content changes.
    #[test]
    fn stack_transformer_transform_content_change() {
        // Arrange
        let directory = TempDir::new().expect("should create temp dir");
        let project = json!({
            "services": { "app": {
                "secrets": [{ "source": "orbit" }],
                "x-orbit": { "configs": [{ "source": "app.conf", "target": "/etc/app.conf" }] }
            }},
            "secrets": { "orbit": { "external": true } }
        });
        let stack = Stack::new("hello", directory.path().to_path_buf(), project)
            .expect("project should parse");
        let path = directory.path().join("app.conf");
        // Act
        fs::write(&path, "a").expect("should write config");
        let before = StackTransformer
            .transform(&stack)
            .expect("should transform");
        fs::write(&path, "b").expect("should write config");
        let after = StackTransformer
            .transform(&stack)
            .expect("should transform");
        // Assert
        let label = "/services/app/labels/orbit.config-hash";
        assert_ne!(before.pointer(label), after.pointer(label));
        assert!(before.get("secrets").is_none());
        assert!(before.pointer("/services/app/secrets").is_none());
    }

    /// [`StackTransformer::transform`] passes each config `mode` to its reference as written.
    #[test]
    fn stack_transformer_transform_config_mode() {
        // Arrange
        let directory = TempDir::new().expect("should create temp dir");
        let project = json!({ "services": { "app": { "x-orbit": { "configs": [
            { "source": "a.sh", "target": "/a.sh", "mode": 493 },
            { "source": "b.sh", "target": "/b.sh", "mode": "0755" },
            { "source": "c.conf", "target": "/c.conf" }
        ]}}}});
        let stack = Stack::new("hello", directory.path().to_path_buf(), project)
            .expect("project should parse");
        for name in ["a.sh", "b.sh", "c.conf"] {
            fs::write(directory.path().join(name), "x").expect("should write config");
        }
        // Act
        let project = StackTransformer
            .transform(&stack)
            .expect("transform should succeed");
        // Assert
        let expected = json!([
            { "source": "orbit_app_0", "target": "/a.sh", "mode": 493 },
            { "source": "orbit_app_1", "target": "/b.sh", "mode": "0755" },
            { "source": "orbit_app_2", "target": "/c.conf" }
        ]);
        assert_eq!(project.pointer("/services/app/configs"), Some(&expected));
    }

    /// [`StackTransformer::transform`] rejects a config file that is not UTF-8, naming the path.
    #[test]
    fn stack_transformer_transform_not_utf8() {
        // Arrange
        let directory = TempDir::new().expect("should create temp dir");
        let project = json!({
            "services": { "app": {
                "secrets": [{ "source": "orbit" }],
                "x-orbit": { "configs": [{ "source": "app.conf", "target": "/etc/app.conf" }] }
            }},
            "secrets": { "orbit": { "external": true } }
        });
        let stack = Stack::new("hello", directory.path().to_path_buf(), project)
            .expect("project should parse");
        fs::write(directory.path().join("app.conf"), [0xff, 0xfe]).expect("should write config");
        // Act
        let report = StackTransformer
            .transform(&stack)
            .expect_err("binary config should fail");
        // Assert
        assert_eq!(report.current_context(), &TransformError::Utf8);
        let display = report.to_string();
        assert!(display.contains("app.conf"), "{display}");
    }

    /// [`StackTransformer::transform`] rejects a generated config name that already exists.
    #[test]
    fn stack_transformer_transform_config_name() {
        // Arrange
        let mut project = Stack::fixture_project();
        project.expect_object_mut("").insert(
            "configs".to_owned(),
            json!({ "orbit_worker_0": { "content": "taken" } }),
        );
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        // Act
        let report = StackTransformer
            .transform(&stack)
            .expect_err("taken name should fail");
        // Assert
        assert_eq!(report.current_context(), &TransformError::ConfigName);
    }

    /// [`StackTransformer::transform`] sets each `x-orbit.environment` target to its source variable.
    #[test]
    fn stack_transformer_transform_environment() {
        // Arrange
        let project = json!({ "services": { "app": {
            "environment": { "LOG_LEVEL": "info" },
            "x-orbit": { "environment": [
                "API_TOKEN",
                { "source": "MAIL_PASSWORD", "target": "SMTP_PASSWORD" }
            ]}
        }}});
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        // Act
        let project = StackTransformer
            .transform(&stack)
            .expect("transform should succeed");
        // Assert
        let expected = json!({
            "API_TOKEN": "${API_TOKEN}",
            "LOG_LEVEL": "info",
            "SMTP_PASSWORD": "${MAIL_PASSWORD}"
        });
        assert_eq!(
            project.pointer("/services/app/environment"),
            Some(&expected)
        );
    }

    /// [`StackTransformer::transform`] replaces an `environment` value named in `x-orbit.environment`.
    #[test]
    fn stack_transformer_transform_environment_replace() {
        // Arrange
        let project = json!({ "services": { "app": {
            "environment": { "API_TOKEN": "plain" },
            "x-orbit": { "environment": ["API_TOKEN"] }
        }}});
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        // Act
        let project = StackTransformer
            .transform(&stack)
            .expect("transform should succeed");
        // Assert
        let expected = json!({ "API_TOKEN": "${API_TOKEN}" });
        assert_eq!(
            project.pointer("/services/app/environment"),
            Some(&expected)
        );
    }

    /// [`StackTransformer::transform`] adds one top-level secret per source, shared by services.
    #[test]
    fn stack_transformer_transform_secrets() {
        // Arrange
        let project = json!({ "services": {
            "app": { "x-orbit": { "secrets": ["TLS_KEY"] } },
            "web": { "x-orbit": { "secrets": [
                { "source": "TLS_KEY", "target": "/etc/ssl/key.pem" }
            ]}}
        }});
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        // Act
        let project = StackTransformer
            .transform(&stack)
            .expect("transform should succeed");
        // Assert
        let secrets = json!({ "orbit_TLS_KEY": { "environment": "TLS_KEY" } });
        let app = json!([{ "source": "orbit_TLS_KEY", "target": "/run/secrets/TLS_KEY" }]);
        let web = json!([{ "source": "orbit_TLS_KEY", "target": "/etc/ssl/key.pem" }]);
        assert_eq!(project.get("secrets"), Some(&secrets));
        assert_eq!(project.pointer("/services/app/secrets"), Some(&app));
        assert_eq!(project.pointer("/services/web/secrets"), Some(&web));
    }

    /// [`StackTransformer::transform`] rejects a generated secret name that already exists.
    #[test]
    fn stack_transformer_transform_secret_name() {
        // Arrange
        let project = json!({
            "services": { "app": { "x-orbit": { "secrets": ["TLS_KEY"] } } },
            "secrets": { "orbit_TLS_KEY": { "file": "./key.pem" } }
        });
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        // Act
        let report = StackTransformer
            .transform(&stack)
            .expect_err("taken name should fail");
        // Assert
        assert_eq!(report.current_context(), &TransformError::SecretName);
    }
}
