FROM mcr.microsoft.com/devcontainers/java:1-21-bookworm

RUN export DEBIAN_FRONTEND=noninteractive \
    && rm -f /etc/apt/sources.list.d/yarn.list \
    && apt-get update \
    && apt-get install -y nodejs npm maven curl git \
    && mkdir -p /home/vscode/.m2/repository /home/vscode/.npm \
    && chown -R vscode:vscode /home/vscode/.m2 /home/vscode/.npm \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* \
    && echo "alias ll='ls -la'" >> /etc/bash.bashrc
