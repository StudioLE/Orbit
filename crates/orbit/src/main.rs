//! Binary entrypoint for `orbit`.

use orbit::prelude::*;
use std::process::ExitCode;

fn main() -> ExitCode {
    Cli::new().run()
}
