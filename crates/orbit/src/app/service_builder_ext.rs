//! Extension trait for configuring a [`ServiceBuilder`] with application services.

use crate::prelude::*;

/// Register all application services on a [`ServiceBuilder`].
pub trait ServiceBuilderExt {
    /// Register all application services.
    fn with_app_services(self) -> Self;

    /// Register all application services with mock Compose and sops clients and [`CliArgs::mock`].
    #[cfg(test)]
    fn with_mock_services(
        self,
        compose_config: MockComposeConfigReader,
        runner: MockComposeRunner,
        sops: MockSopsClient,
    ) -> Self;
}

impl ServiceBuilderExt for ServiceBuilder {
    fn with_app_services(self) -> Self {
        self.with_logging(create_logger)
            .with_trait::<dyn ComposeConfigReader, ComposeConfigReaderAdapter>()
            .with_trait::<dyn ComposeRunner, ComposeRunnerAdapter>()
            .with_trait::<dyn SopsClient, SopsClientAdapter>()
            .with_type::<CliArgs>()
            .with_type::<ComposeHandler>()
            .with_type::<DeploymentProvider>()
            .with_type::<DockerHostProvider>()
            .with_type::<SecretsProvider>()
            .with_type::<StackProvider>()
            .with_type::<StackTransformer>()
            .with_type::<StackValidator>()
            .with_type::<SubcommandHandler>()
            .with_type::<WorkingDirectory>()
    }

    #[cfg(test)]
    fn with_mock_services(
        self,
        compose_config: MockComposeConfigReader,
        runner: MockComposeRunner,
        sops: MockSopsClient,
    ) -> Self {
        self.with_app_services()
            .with_instance::<Arc<dyn ComposeConfigReader>>(Arc::new(compose_config))
            .with_instance::<Arc<dyn ComposeRunner>>(Arc::new(runner))
            .with_instance::<Arc<dyn SopsClient>>(Arc::new(sops))
            .with_instance(CliArgs::mock())
    }
}

#[cfg(not(test))]
fn create_logger(services: &ServiceProvider) -> Result<Logger, Report<ResolveError>> {
    let args = services.get::<CliArgs>()?;
    let logger = LoggerBuilder::new()
        .with_level(args.log_level.unwrap_or_default())
        .with_target("studiole_di::service_provider", LogLevel::Debug)
        .with_target(
            "studiole_di::traits::service_provider_get_trait",
            LogLevel::Debug,
        )
        .build();
    Ok(logger)
}

#[cfg(test)]
#[expect(
    clippy::unnecessary_wraps,
    reason = "signature required by with_logging"
)]
fn create_logger(_services: &ServiceProvider) -> Result<Logger, Report<ResolveError>> {
    let logger = LoggerBuilder::new().with_level(LogLevel::Trace).build();
    Ok(logger)
}
