FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
ARG CONFIGURATION="Release"
ARG PROJECT="Orbit.Cli/src/Orbit.Cli.csproj"
ARG RUNTIME="linux-musl-x64"
WORKDIR /app
COPY . .
RUN dotnet publish "${PROJECT}" \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:SelfContained=false \
  -p:DebugType=embedded \
  -p:GenerateDocumentationFile=false \
  --configuration "${CONFIGURATION}" \
  --runtime "${RUNTIME}" \
  --output artifacts

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
ARG EXECUTABLE="Orbit.Cli"
WORKDIR /app
RUN apk add libqrencode-tools wireguard-tools --no-cache
COPY --from=builder /app/artifacts/"${EXECUTABLE}" /bin/app
COPY --from=builder /app/artifacts/appsettings.json /app/
RUN chmod +x /bin/app
ENTRYPOINT ["app"]
