//! Extension traits on existing types.

mod output_ok_or_report;
mod response;
mod structured_error_exit_code;
#[cfg(test)]
mod value_expect_object_mut;

pub use output_ok_or_report::*;
pub use response::*;
pub use structured_error_exit_code::*;
#[cfg(test)]
pub use value_expect_object_mut::*;
