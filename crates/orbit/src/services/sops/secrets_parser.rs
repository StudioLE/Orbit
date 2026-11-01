//! Parse decrypted dotenv text into [`Secrets`].

use crate::prelude::*;

/// Parse decrypted dotenv text into [`Secrets`].
#[derive(Default, FromServices)]
pub struct SecretsParser;

impl SecretsParser {
    /// Parse dotenv text into [`Secrets`].
    ///
    /// - Splits each line at the first `=`
    /// - Unescapes `\n` as a newline, matching how sops writes multiline values
    /// - Skips blank lines and lines starting with `#`
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
                Ok((name, value.replace("\\n", "\n")))
            })
            .collect()
    }
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

    /// [`SecretsParser::parse`] splits at the first `=`, unescapes `\n` and skips comments.
    #[test]
    fn secrets_parser_parse() {
        // Arrange
        let services = ServiceBuilder::new().with_test_services().build();
        let parser = services.expect::<SecretsParser>();
        let text = DecryptedText::from(
            "A=has=equals and spaces \nB=\"quoted\"\n# comment\n\nC=multi\\nline\nD=\n".to_owned(),
        );
        // Act
        let secrets = parser.parse(&text).expect("dotenv should parse");
        // Assert
        let pairs: Vec<_> = secrets.iter().collect();
        assert_eq!(
            pairs,
            vec![
                ("A", "has=equals and spaces "),
                ("B", "\"quoted\""),
                ("C", "multi\nline"),
                ("D", ""),
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
}
