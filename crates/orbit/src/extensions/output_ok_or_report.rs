//! Conversion of a failed child process [`Output`] into an error report.

use crate::prelude::*;
use std::error::Error as StdError;
use std::process::Output;

/// Convert a [`Output`] into a `Result`, returning an error report on failure.
pub trait OkOrReport: Sized {
    /// Return `self` if the process exited successfully, or an error report with the given context.
    ///
    /// - Attaches stderr, stdout and exit code on failure
    fn ok_or_report<C: StdError + Send + Sync + 'static>(
        self,
        context: C,
    ) -> Result<Self, Report<C>>;
}

impl OkOrReport for Output {
    #[track_caller]
    fn ok_or_report<C: StdError + Send + Sync + 'static>(
        self,
        context: C,
    ) -> Result<Self, Report<C>> {
        if self.status.success() {
            Ok(self)
        } else {
            let response = self.to_response();
            let report = Report::new(context).attach_response(response);
            Err(report)
        }
    }
}
