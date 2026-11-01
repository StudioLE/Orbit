//! Parse decrypted dotenv text into [`Secrets`].

use crate::prelude::*;

/// Parse decrypted dotenv text into [`Secrets`].
#[derive(Default, FromServices)]
pub struct SecretsParser;

impl SecretsParser {
    /// Parse dotenv text into [`Secrets`].
    ///
    /// - Splits each line at the first `=`
    /// - Strips one matching pair of `'` or `"` around the value, as Compose does
    /// - Unescapes `\n` as a newline, matching how sops writes multiline values
    /// - Skips blank lines and lines starting with `#`
    /// - Logs a [`SecretsWarning`] at `warn` for each way Compose would read a value differently
    /// - Attaches the line number to errors but not its content, so values cannot reach reports
    ///
    /// # Errors
    ///
    /// - [`SecretsParserError::Separator`] if a line has no `=`
    /// - [`SecretsParserError::Name`] if a line has an empty name
    #[expect(clippy::unused_self, reason = "stateless DI service")]
    pub fn parse(&self, text: &DecryptedText) -> Result<Secrets, Report<SecretsParserError>> {
        text.as_str()
            .lines()
            .enumerate()
            .filter(|(_, line)| !line.trim().is_empty() && !line.starts_with('#'))
            .map(|(index, line)| {
                let Some((name, value)) = line.split_once('=') else {
                    return Err(
                        Report::new(SecretsParserError::Separator).attach("line", index + 1)
                    );
                };
                if name.is_empty() {
                    return Err(Report::new(SecretsParserError::Name).attach("line", index + 1));
                }
                for warning in get_warnings(name, value) {
                    warn!("{warning}");
                }
                Ok((name, unquote(value).replace("\\n", "\n")))
            })
            .collect()
    }
}

/// Quote character around `raw`, if it starts and ends with the same `'` or `"`.
fn get_quote(raw: &str) -> Option<char> {
    let quote = raw
        .chars()
        .next()
        .filter(|first| matches!(first, '\'' | '"'))?;
    (raw.len() >= 2 && raw.ends_with(quote)).then_some(quote)
}

/// Strip one matching pair of `'` or `"` around `raw`.
fn unquote(raw: &str) -> &str {
    match get_quote(raw) {
        Some(_) => &raw[1..raw.len() - 1],
        None => raw,
    }
}

/// Get each way Compose would read the raw value of secret `name` differently from Orbit.
///
/// - Single-quoted values never warn, as Compose also takes them literally
/// - Checks double-quoted values after unescaping `\n`, which both read the same
/// - Skips the unquoted checks for unmatched quotes, as Compose reads those as quoted
fn get_warnings(name: &str, raw: &str) -> Vec<SecretsWarning> {
    let mut kinds = Vec::new();
    match get_quote(raw) {
        Some('\'') => {}
        Some(_) => {
            let value = unquote(raw).replace("\\n", "\n");
            if value.contains('$') {
                kinds.push(SecretsWarningKind::DoubleQuotedDollar);
            }
            if value.contains('\\') {
                kinds.push(SecretsWarningKind::DoubleQuotedBackslash);
            }
        }
        None if raw.starts_with(['\'', '"']) => {
            kinds.push(SecretsWarningKind::UnmatchedQuote);
        }
        None => {
            if raw.contains('$') {
                kinds.push(SecretsWarningKind::UnquotedDollar);
            }
            if raw.contains(" #") {
                kinds.push(SecretsWarningKind::UnquotedComment);
            }
            if raw.starts_with(char::is_whitespace) || raw.ends_with(char::is_whitespace) {
                kinds.push(SecretsWarningKind::UnquotedWhitespace);
            }
        }
    }
    kinds
        .into_iter()
        .map(|kind| SecretsWarning {
            name: name.to_owned(),
            kind,
        })
        .collect()
}

/// Errors returned by [`SecretsParser`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum SecretsParserError {
    /// A line has no `=` separator.
    #[error("Line is not KEY=VALUE")]
    Separator,
    /// A line has an empty name before `=`.
    #[error("Line has an empty name")]
    Name,
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`SecretsParser::parse`] splits at the first `=`, strips matching quotes, unescapes `\n` and skips comments.
    #[test]
    fn secrets_parser_parse() {
        // Arrange
        let services = ServiceBuilder::new().with_test_services().build();
        let parser = services.expect::<SecretsParser>();
        let text = DecryptedText::from(
            [
                "A=has=equals and spaces ",
                "B=\"quoted\"",
                "# comment",
                "",
                "C=multi\\nline",
                "D=",
                "E='single'",
                "F=''",
                "G=\"multi\\nline\"",
                "H=\"mismatched'",
                "I=\"",
            ]
            .join("\n"),
        );
        // Act
        let secrets = parser.parse(&text).expect("dotenv should parse");
        // Assert
        let pairs: Vec<_> = secrets.iter().collect();
        assert_eq!(
            pairs,
            vec![
                ("A", "has=equals and spaces "),
                ("B", "quoted"),
                ("C", "multi\nline"),
                ("D", ""),
                ("E", "single"),
                ("F", ""),
                ("G", "multi\nline"),
                ("H", "\"mismatched'"),
                ("I", "\""),
            ]
        );
    }

    /// [`SecretsParser::parse`] rejects a line without `=` and names the line, not its content.
    #[test]
    fn secrets_parser_parse_separator() {
        // Arrange
        let services = ServiceBuilder::new().with_test_services().build();
        let parser = services.expect::<SecretsParser>();
        let text = DecryptedText::from("A=1\nhunter2hunter2\n".to_owned());
        // Act
        let report = parser
            .parse(&text)
            .expect_err("line without `=` should fail");
        // Assert
        assert_eq!(report.current_context(), &SecretsParserError::Separator);
        let display = format!("{report:?}");
        assert!(display.contains("line: 2"), "{display}");
        assert!(!display.contains("hunter2"), "{display}");
    }

    /// [`SecretsParser::parse`] rejects a line with an empty name.
    #[test]
    fn secrets_parser_parse_name() {
        // Arrange
        let services = ServiceBuilder::new().with_test_services().build();
        let parser = services.expect::<SecretsParser>();
        let text = DecryptedText::from("=value".to_owned());
        // Act
        let report = parser.parse(&text).expect_err("empty name should fail");
        // Assert
        assert_eq!(report.current_context(), &SecretsParserError::Name);
    }

    /// [`super::get_warnings`] lists each way Compose would read a raw value differently.
    #[test]
    fn get_warnings() {
        // Arrange
        let values = [
            "abc",
            "'$2a$x'",
            "'a #b'",
            "\"a\\nb\"",
            "$2a$x",
            "a #b",
            " abc",
            "abc ",
            "\"$x\"",
            "\"a\\tb\"",
            "\"abc'",
            "\"",
            "\"abc\" # note",
            "$x #y",
        ];
        // Act
        let warnings: BTreeMap<_, Vec<_>> = values
            .into_iter()
            .map(|value| {
                let messages = super::get_warnings("A", value)
                    .iter()
                    .map(ToString::to_string)
                    .collect();
                (value, messages)
            })
            .collect();
        // Assert
        assert_yaml_snapshot!(warnings);
    }
}
