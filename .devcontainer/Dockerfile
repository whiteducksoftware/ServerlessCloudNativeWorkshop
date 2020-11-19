# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.140.1/containers/ubuntu/.devcontainer/base.Dockerfile
ARG VARIANT

FROM mcr.microsoft.com/vscode/devcontainers/base:0-${VARIANT}

ARG VARIANT
ARG DOTNET_VERSION=3.1
ARG NODE_VERSION=12

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

RUN apt-get update \
  && apt-get install -y software-properties-common build-essential \
  && add-apt-repository universe \
  \
  # Change default shell to zsh
  && chsh -s $(which zsh) \
  \
  # Add dotnet repo
  && wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
  && dpkg -i packages-microsoft-prod.deb \
  \
  # Add azure repo
  && curl -sL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | sudo tee /etc/apt/trusted.gpg.d/microsoft.gpg > /dev/null \
  && echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/azure-cli.list \
  \
  # Install apt packages
  && apt-get update \
  && apt install -y dotnet-sdk-${DOTNET_VERSION} dotnet-runtime-${DOTNET_VERSION} aspnetcore-runtime-${DOTNET_VERSION} powershell nodejs yarn azure-cli azure-functions-core-tools-3 \
  \
  && apt-get clean \
  && rm -rf packages-microsoft-prod.deb /var/lib/{apt,dpkg,cache,log}/
