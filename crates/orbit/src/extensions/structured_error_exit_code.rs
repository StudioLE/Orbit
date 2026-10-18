//! Process exit code for a [`StructuredError`].

use crate::prelude::*;
use std::error::Error as StdError;

/// Get the process exit code for an error.
pub trait GetExitCode {
    /// Exit code of the first [`ComposeRunError::Exit`] in the chain, else `1`.
    fn get_exit_code(&self) -> u8;
}

impl GetExitCode for StructuredError {
    fn get_exit_code(&self) -> u8 {
        let mut current: Option<&(dyn StdError + 'static)> = Some(self);
        while let Some(error) = current {
            let context = error
                .downcast_ref::<StructuredError>()
                .map(StructuredError::current_context);
            let compose = context
                .and_then(|context| context.downcast_ref::<Report<ComposeRunError>>())
                .map(Report::current_context)
                .or_else(|| context.and_then(|context| context.downcast_ref::<ComposeRunError>()));
            if let Some(ComposeRunError::Exit(code)) = compose {
                return *code;
            }
            current = error.source();
        }
        1
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`GetExitCode::get_exit_code`] finds a Compose exit code wrapped by a handler.
    #[test]
    fn get_exit_code_compose() {
        // Arrange
        let result: Result<(), Report<ComposeRunError>> =
            Err(Report::new(ComposeRunError::Exit(3)));
        let error = StructuredError::from(
            result
                .change_context(ComposeHandlerError::Compose)
                .expect_err("should be an error"),
        );
        // Act
        let code = error.get_exit_code();
        // Assert
        assert_eq!(code, 3);
    }

    /// [`GetExitCode::get_exit_code`] returns `1` for an Orbit error.
    #[test]
    fn get_exit_code_orbit() {
        // Arrange
        let result: Result<(), Report<ComposeRunError>> = Err(Report::new(ComposeRunError::Spawn));
        let error = StructuredError::from(
            result
                .change_context(ComposeHandlerError::Compose)
                .expect_err("should be an error"),
        );
        // Act
        let code = error.get_exit_code();
        // Assert
        assert_eq!(code, 1);
    }
}
