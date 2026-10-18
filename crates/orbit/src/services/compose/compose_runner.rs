//! Trait and adapter for running Docker Compose against a Docker host.

use crate::prelude::*;
use signal_hook::consts::SIGINT;
use signal_hook::flag::register;
use signal_hook::low_level::unregister;
use std::os::unix::process::ExitStatusExt;
use std::process::{Command, ExitStatus};
use std::sync::atomic::AtomicBool;

/// Run Docker Compose against a Docker host.
#[cfg_attr(test, mockall::automock)]
pub trait ComposeRunner: Send + Sync {
    /// Run `docker compose` with inherited stdio and wait for it to exit.
    fn execute(&self, invocation: ComposeInvocation) -> Result<(), Report<ComposeRunError>>;
}

/// [`ComposeRunner`] that runs the `docker` binary.
#[derive(Default, FromServices)]
pub struct ComposeRunnerAdapter;

impl ComposeRunner for ComposeRunnerAdapter {
    /// Run `docker compose` with inherited stdio and wait for it to exit.
    ///
    /// - Sets `DOCKER_HOST` and every secret on top of the inherited environment
    /// - Ignores `SIGINT` while Compose runs, so Ctrl+C reaches Compose and Orbit still cleans up
    fn execute(&self, invocation: ComposeInvocation) -> Result<(), Report<ComposeRunError>> {
        debug!(arguments = ?invocation.arguments, host = %invocation.host, "Running Compose");
        let mut command = Command::new("docker");
        command
            .arg("compose")
            .args(&invocation.arguments)
            .env("DOCKER_HOST", &invocation.host)
            .envs(invocation.secrets.iter());
        let interrupted = Arc::new(AtomicBool::new(false));
        let signal = register(SIGINT, interrupted).change_context(ComposeRunError::Signal)?;
        let status = command.status();
        unregister(signal);
        let status = status.change_context(ComposeRunError::Spawn)?;
        if status.success() {
            Ok(())
        } else {
            let code = get_exit_code(status);
            Err(Report::new(ComposeRunError::Exit(code)))
        }
    }
}

/// Exit code of a failed process.
///
/// - Follows the shell convention of `128 + signal` for a process killed by a signal
/// - Falls back to `1` when neither is known
fn get_exit_code(status: ExitStatus) -> u8 {
    status
        .code()
        .or_else(|| status.signal().map(|signal| 128 + signal))
        .and_then(|code| u8::try_from(code).ok())
        .unwrap_or(1)
}

/// Errors returned by [`ComposeRunner`].
#[derive(Clone, Copy, Debug, Eq, Error, PartialEq)]
pub enum ComposeRunError {
    /// Unable to start docker.
    #[error("Unable to start docker")]
    Spawn,
    /// Unable to register the interrupt handler.
    #[error("Unable to handle interrupts")]
    Signal,
    /// Compose exited with a non-zero code.
    #[error("Compose exited with code {0}")]
    Exit(u8),
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`get_exit_code`] returns the code of a process that exited.
    #[test]
    fn get_exit_code_exited() {
        let status = ExitStatus::from_raw(3 << 8);
        assert_eq!(get_exit_code(status), 3);
    }

    /// [`get_exit_code`] returns `128 + signal` for a process killed by a signal.
    #[test]
    fn get_exit_code_signaled() {
        let status = ExitStatus::from_raw(SIGINT);
        assert_eq!(get_exit_code(status), 130);
    }
}
