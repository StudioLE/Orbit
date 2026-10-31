//! Stub for reading the Compose project.

use crate::prelude::*;

/// [`ComposeConfigReader`] returning a fixed project or [`ComposeConfigError`].
pub struct ComposeConfigReaderStub {
    /// Value returned by [`ComposeConfigReader::read`].
    pub project: Result<Value, ComposeConfigError>,
}

impl FromServices for ComposeConfigReaderStub {
    type Error = Infallible;

    fn from_services(_services: &ServiceProvider) -> Result<Self, Report<Self::Error>> {
        Ok(Self {
            project: Ok(Stack::fixture_project()),
        })
    }
}

impl ComposeConfigReader for ComposeConfigReaderStub {
    fn read(&self) -> Result<Value, Report<ComposeConfigError>> {
        self.project.clone().map_err(Report::new)
    }
}
