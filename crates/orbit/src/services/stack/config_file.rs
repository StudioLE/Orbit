//! Local file Orbit inlines into a service as a Compose config.

use crate::prelude::*;

/// Local file Orbit inlines into a service, from `x-orbit.configs`.
#[derive(Clone, Debug, Deserialize, Eq, PartialEq, Serialize)]
#[serde(deny_unknown_fields)]
pub struct ConfigFile {
    /// Path on the local machine, relative to the stack directory or absolute.
    pub source: String,
    /// Path in the container.
    pub target: String,
}

impl ConfigFile {
    /// Resolve [`ConfigFile::source`] against the stack `directory`.
    pub fn get_path(&self, directory: &Path) -> PathBuf {
        directory.join(&self.source)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// [`ConfigFile::get_path`] joins a relative source to the directory.
    #[test]
    fn config_file_get_path() {
        let config = ConfigFile::mock("./web/app.conf");
        let path = config.get_path(Path::new("/stack"));
        assert_eq!(path, PathBuf::from("/stack/./web/app.conf"));
    }

    /// [`ConfigFile::get_path`] keeps an absolute source.
    #[test]
    fn config_file_get_path_absolute() {
        let config = ConfigFile::mock("/etc/app.conf");
        let path = config.get_path(Path::new("/stack"));
        assert_eq!(path, PathBuf::from("/etc/app.conf"));
    }

    impl ConfigFile {
        fn mock(source: &str) -> Self {
            Self {
                source: source.to_owned(),
                target: "/etc/app.conf".to_owned(),
            }
        }
    }
}
