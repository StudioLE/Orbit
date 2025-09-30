FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
ARG NUGET_USER
ARG NUGET_PASSWORD
ARG NUGET_FEED
ARG CONFIGURATION="Release"
ENV CONFIGURATION="${CONFIGURATION}"
WORKDIR /app
RUN apk add libqrencode-tools wireguard-tools --no-cache
RUN dotnet nuget add source \
  --username "${NUGET_USER}" \
  --password "${NUGET_PASSWORD}" \
  --store-password-in-clear-text \
  "${NUGET_FEED}"
COPY . .
WORKDIR /app/Orbit.Cli/src/
RUN dotnet restore \
  --nologo \
  --verbosity minimal
RUN dotnet build \
  --configuration "${CONFIGURATION}" \
  -p:ContinuousIntegrationBuild=true \
  -p:PublishRepositoryUrl=true \
  -p:EmbedUntrackedSources=true \
  -p:IncludeSymbols=true \
  -p:SymbolPackageFormat=snupkg \
  -p:SurveyorFeed="${NUGET_USER}" \
  -p:SurveyorToken="${NUGET_PASSWORD}" \
  --nologo \
  --no-restore \
  --verbosity minimal
CMD dotnet test --configuration "${CONFIGURATION}"

