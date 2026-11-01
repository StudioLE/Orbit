//! Check a [`Stack`] has every config file and secret it needs.

use crate::prelude::*;

/// Check a [`Stack`] has every config file and secret it needs.
#[derive(Default, FromServices)]
pub struct StackValidator;

impl StackValidator {
    /// Check `stack` has every config file and secret it needs, reporting every problem at once.
    ///
    /// - Each `x-orbit.configs` source exists and is a file
    /// - Each `x-orbit.configs` number mode is at most `0777`
    /// - Each `x-orbit.environment` and `x-orbit.secrets` source is in `secrets`
    /// - Attaches each problem as `problem`
    #[expect(clippy::unused_self, reason = "stateless DI service")]
    pub fn validate(
        &self,
        stack: &Stack,
        secrets: &Secrets,
    ) -> Result<(), Report<ValidationError>> {
        let problems = get_problems(stack, secrets);
        if problems.is_empty() {
            return Ok(());
        }
        let report = problems
            .into_iter()
            .fold(Report::new(ValidationError::Invalid), |report, problem| {
                report.attach("problem", problem)
            });
        Err(report)
    }
}

/// Errors returned by [`StackValidator`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum ValidationError {
    /// The stack has one or more problems.
    #[error("Stack is invalid")]
    Invalid,
}

/// Every config and secret problem in `stack`.
fn get_problems(stack: &Stack, secrets: &Secrets) -> Vec<String> {
    let mut problems = get_config_problems(stack);
    problems.extend(get_secret_problems(stack, secrets));
    problems
}

/// Each `x-orbit.configs` source that does not exist or is not a file, and each number mode above `0777`.
fn get_config_problems(stack: &Stack) -> Vec<String> {
    let mut problems = Vec::new();
    for (name, service) in &stack.settings.services {
        for config in &service.configs {
            let path = config.get_path(&stack.directory);
            if !path.exists() {
                problems.push(format!(
                    "Service `{name}` config `{}` does not exist",
                    config.source
                ));
            } else if !path.is_file() {
                problems.push(format!(
                    "Service `{name}` config `{}` is not a file",
                    config.source
                ));
            }
            if let Some(mode) = &config.mode
                && mode.is_missing_leading_zero()
            {
                problems.push(format!(
                    "Service `{name}` config `{}` mode `{mode}` is decimal, not octal, so use `0{mode}` or `\"{mode}\"`",
                    config.source
                ));
            }
        }
    }
    problems
}

/// Each `x-orbit.environment` and `x-orbit.secrets` source missing from `secrets`.
fn get_secret_problems(stack: &Stack, secrets: &Secrets) -> Vec<String> {
    stack
        .settings
        .get_secret_names()
        .into_iter()
        .filter(|name| !secrets.contains(name))
        .map(|name| format!("Secret `{name}` is not in `.sops.env`"))
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`StackValidator::validate`] accepts the fixture stack with every secret.
    #[test]
    fn stack_validator_validate() {
        let output = StackValidator.validate(&Stack::fixture(), &Secrets::fixture());
        assert!(output.is_ok(), "{output:?}");
    }

    /// [`StackValidator::validate`] accepts a stack without the `orbit` guard.
    #[test]
    fn stack_validator_validate_without_guard() {
        // Arrange
        let project = Stack::fixture_project_without_guard();
        // Act
        let problems = get_problems(project, &Secrets::fixture());
        // Assert
        assert!(problems.is_empty(), "{problems:?}");
    }

    /// [`StackValidator::validate`] reports a config source that does not exist.
    #[test]
    fn stack_validator_validate_config_missing() {
        // Arrange
        let mut project = Stack::fixture_project();
        set_source(&mut project, "worker", 0, "./missing.conf");
        // Act
        let problems = get_problems(project, &Secrets::fixture());
        // Assert
        assert_eq!(
            problems,
            vec!["Service `worker` config `./missing.conf` does not exist"]
        );
    }

    /// [`StackValidator::validate`] reports a config source that is a directory.
    #[test]
    fn stack_validator_validate_config_not_file() {
        // Arrange
        let mut project = Stack::fixture_project();
        set_source(&mut project, "worker", 0, "./web");
        // Act
        let problems = get_problems(project, &Secrets::fixture());
        // Assert
        assert_eq!(
            problems,
            vec!["Service `worker` config `./web` is not a file"]
        );
    }

    /// [`StackValidator::validate`] reports a config mode that is octal digits written as decimal.
    #[test]
    fn stack_validator_validate_config_mode_decimal() {
        // Arrange
        let config = ConfigFile {
            source: "./worker.conf".to_owned(),
            mode: Some(ConfigMode::Number(755)),
            ..ConfigFile::fixture()
        };
        let worker = ServiceSettings {
            configs: vec![config],
            ..ServiceSettings::default()
        };
        let stack = Stack {
            settings: OrbitSettings {
                services: BTreeMap::from([("worker".to_owned(), worker)]),
                ..OrbitSettings::default()
            },
            ..Stack::fixture()
        };
        // Act
        let problems = super::get_problems(&stack, &Secrets::fixture());
        // Assert
        assert_eq!(
            problems,
            vec![
                "Service `worker` config `./worker.conf` mode `755` is decimal, not octal, so use `0755` or `\"755\"`"
            ]
        );
    }

    /// [`StackValidator::validate`] accepts a text config mode above `0777`.
    #[test]
    fn stack_validator_validate_config_mode_text() {
        // Arrange
        let config = ConfigFile {
            source: "./worker.conf".to_owned(),
            mode: Some(ConfigMode::Text("4755".to_owned())),
            ..ConfigFile::fixture()
        };
        let worker = ServiceSettings {
            configs: vec![config],
            ..ServiceSettings::default()
        };
        let stack = Stack {
            settings: OrbitSettings {
                services: BTreeMap::from([("worker".to_owned(), worker)]),
                ..OrbitSettings::default()
            },
            ..Stack::fixture()
        };
        // Act
        let problems = super::get_problems(&stack, &Secrets::fixture());
        // Assert
        assert!(problems.is_empty(), "{problems:?}");
    }

    /// [`StackValidator::validate`] reports secret names missing from `.sops.env`.
    #[test]
    fn stack_validator_validate_secret_missing() {
        // Arrange
        let secrets = Secrets::fixture_api_token();
        // Act
        let problems = get_problems(Stack::fixture_project(), &secrets);
        // Assert
        assert_eq!(
            problems,
            vec![
                "Secret `MAIL_PASSWORD` is not in `.sops.env`",
                "Secret `TLS_KEY` is not in `.sops.env`",
            ]
        );
    }

    /// [`StackValidator::validate`] reports every problem in one report.
    #[test]
    fn stack_validator_validate_all() {
        // Arrange
        let mut project = Stack::fixture_project();
        set_source(&mut project, "worker", 0, "./missing.conf");
        set_source(&mut project, "web", 1, "./web");
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        let secrets = Secrets::default();
        // Act
        let report = StackValidator
            .validate(&stack, &secrets)
            .expect_err("stack should be invalid");
        // Assert
        assert_eq!(report.current_context(), &ValidationError::Invalid);
        assert_snapshot!(report.to_string());
    }

    fn get_problems(project: Value, secrets: &Secrets) -> Vec<String> {
        let stack =
            Stack::new("hello", Stack::fixture_directory(), project).expect("project should parse");
        super::get_problems(&stack, secrets)
    }

    fn set_source(project: &mut Value, service: &str, index: usize, source: &str) {
        let pointer = format!("/services/{service}/x-orbit/configs/{index}");
        project
            .expect_object_mut(&pointer)
            .insert("source".to_owned(), json!(source));
    }
}
