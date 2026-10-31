//! Spy for running Docker Compose.

use crate::prelude::*;

/// [`ComposeRunner`] that records each call and always succeeds.
#[derive(FromServices)]
pub struct ComposeRunnerSpy {
    #[di(default)]
    calls: Mutex<Vec<ComposeInvocation>>,
}

impl ComposeRunnerSpy {
    /// Each [`ComposeInvocation`] passed to [`ComposeRunner::execute`], in call order.
    pub fn calls(&self) -> Vec<ComposeInvocation> {
        self.calls
            .lock()
            .expect("should be able to lock calls")
            .clone()
    }
}

impl ComposeRunner for ComposeRunnerSpy {
    fn execute(&self, invocation: ComposeInvocation) -> Result<(), Report<ComposeRunError>> {
        self.calls
            .lock()
            .expect("should be able to lock calls")
            .push(invocation);
        Ok(())
    }
}
