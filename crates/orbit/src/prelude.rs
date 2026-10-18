//! Common imports used throughout the crate.
#![allow(unused_imports, reason = "prelude re-exports for convenience")]

pub use crate::app::Cli;
pub(crate) use crate::app::*;
pub(crate) use crate::commands::*;
pub(crate) use crate::extensions::*;
pub(crate) use crate::services::*;

pub(crate) use serde::{Deserialize, Serialize};
pub(crate) use serde_json::{Map, Value, json};
pub(crate) use std::collections::{BTreeMap, BTreeSet, HashMap, HashSet, VecDeque};
pub(crate) use std::convert::Infallible;
pub(crate) use std::error::Error;
pub(crate) use std::fmt::{Debug, Display, Formatter, Result as FmtResult};
pub(crate) use std::io::{self, Read as IoRead, Write as IoWrite};
pub(crate) use std::path::{Path, PathBuf};
pub(crate) use std::str::FromStr;
pub(crate) use std::sync::Arc;
pub(crate) use strum::Display;
pub(crate) use studiole_di::prelude::*;
pub(crate) use studiole_logging::prelude::*;
pub(crate) use studiole_report::prelude::*;
pub(crate) use thiserror::Error;
pub(crate) use tracing::{debug, error, info, trace, warn};
pub(crate) use usage_rs::Args;

#[cfg(test)]
pub(crate) use insta::{assert_json_snapshot, assert_snapshot, assert_yaml_snapshot};
