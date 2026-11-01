//! Secret that Compose would read differently from Orbit.

use crate::prelude::*;

/// Secret that Compose would read differently from Orbit.
///
/// - Holds the secret's name only, so values cannot reach logs
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct SecretsWarning {
    /// Secret name.
    pub name: String,
    /// Way Compose would read the value differently.
    pub kind: SecretsWarningKind,
}

impl Display for SecretsWarning {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> fmt::Result {
        write!(formatter, "Secret `{}` {}", self.name, self.kind)
    }
}

/// Way Compose would read a secret value differently from Orbit.
#[derive(Clone, Copy, Debug, Display, Eq, PartialEq)]
pub enum SecretsWarningKind {
    /// An unquoted value has `$`.
    #[strum(
        to_string = "has `$` outside quotes, which Compose would expand. Wrap it in single quotes"
    )]
    UnquotedDollar,
    /// An unquoted value has ` #`.
    #[strum(
        to_string = "has ` #` outside quotes, which Compose would cut as a comment. Wrap it in single quotes"
    )]
    UnquotedComment,
    /// An unquoted value starts or ends with whitespace.
    #[strum(
        to_string = "starts or ends with whitespace outside quotes, which Compose would trim. Wrap it in single quotes"
    )]
    UnquotedWhitespace,
    /// A double-quoted value has `$`.
    #[strum(to_string = "has `$` in double quotes, which Compose would expand. Use single quotes")]
    DoubleQuotedDollar,
    /// A double-quoted value has `\`.
    #[strum(
        to_string = "has `\\` in double quotes, which Compose would unescape. Use single quotes"
    )]
    DoubleQuotedBackslash,
    /// A value starts with a quote but doesn't end with the same one.
    #[strum(
        to_string = "starts with a quote but doesn't end with the same one, so Orbit keeps it as written"
    )]
    UnmatchedQuote,
}
