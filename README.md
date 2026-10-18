# Orbit

Docker Compose expects plaintext secrets, and without Swarm it can't deploy local config files to a remote host.

Orbit extends `docker compose up` and `run` to solve both:

- Deploys local config files to a remote host
- Decrypts sops-encrypted secrets in memory

Deploy your Compose stacks over SSH from your own machine or CI. Orbit wraps the Compose commands you already use, with no agent on the host, no exposed Docker socket, and no extra services adding to the host's attack surface.

## How it works

### Secrets

Without Orbit, secrets sit in plaintext in `.env` or `docker-compose.yml`, or you wrap every command in `sops exec-env`.

With Orbit, you keep them in `.sops.env`, encrypted with [sops](https://github.com/getsops/sops), and commit the file alongside the stack.

When you run `orbit up`, Orbit decrypts `.sops.env` in memory and Compose gives each value to the container as a file or an environment variable. Nothing decrypted touches your machine's disk.

### Config files

Without Orbit, `configs: file: ./Caddyfile` is read on the remote host, not your machine, so you copy files to the host and keep them in sync yourself. Editing one doesn't recreate the container, so the running container keeps the old file.

With Orbit, you keep config files next to `docker-compose.yml` and list them under `x-orbit.configs`.

When you run `orbit up`, Orbit puts each file's content into the Compose config it sends to the host, and labels each service with a hash of its configs, so an edit recreates the container.

## Install

Homebrew:

```shell
brew install studiole/tap/orbit
```

Cargo:

```shell
cargo install studiole-orbit
```

Or download the binary from [GitHub Releases](https://github.com/StudioLE/Orbit/releases).

Orbit needs [Docker Compose](https://docs.docker.com/compose/) and [sops](https://github.com/getsops/sops) installed on your machine.

## Quick start

Create a stack directory with a `docker-compose.yml`:

```yaml
x-orbit:
  # Remote SSH host
  host: ssh://my-server

services:
  web:
    image: caddy
    environment:
      LOG_LEVEL: info
    x-orbit:
      # Env vars decrypted from .sops.env
      environment:
        # API_TOKEN from .sops.env, set as API_TOKEN
        - API_TOKEN
        # MAIL_PASSWORD from .sops.env, set as SMTP_PASSWORD
        - source: MAIL_PASSWORD
          target: SMTP_PASSWORD
      # Files decrypted from .sops.env
      secrets:
        # TLS_KEY from .sops.env, mounted at /run/secrets/TLS_KEY
        - TLS_KEY
        # SIGNING_KEY from .sops.env, mounted at /etc/app/signing.key
        - source: SIGNING_KEY
          target: /etc/app/signing.key
      # Local config files to be deployed to the remote container
      configs:
        - source: ./Caddyfile
          target: /etc/caddy/Caddyfile
```

Add your secrets with sops:

```shell
sops .sops.env
```

If this is your first time using sops, create a `.sops.yaml` with your encryption key first. See the [sops docs](https://github.com/getsops/sops).

Deploy from the stack directory:

```shell
orbit up --detach
```

## Optional guard

An optional guard can stop anyone running a plain `docker compose up` on the stack, which would deploy it without its secrets and config files.

```yaml
services:
  web:
    # …
    # Add the orbit guard to each of your services
    secrets:
      - orbit

secrets:
  # Define orbit as an external source
  orbit:
    external: true
```

Plain `docker compose up` then fails with `unsupported external secret orbit` before it touches any container. `docker compose ps`, `logs`, `down` and `pull` still work. Orbit removes the guard when it deploys.
