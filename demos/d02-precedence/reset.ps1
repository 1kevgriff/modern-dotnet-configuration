#!/usr/bin/env pwsh
# Restores the shell to this demo's starting state: no environment overrides.
Remove-Item Env:Weather__TimeoutSeconds -ErrorAction SilentlyContinue
Remove-Item Env:DOTNET_ENVIRONMENT -ErrorAction SilentlyContinue
Write-Host 'd02 reset: DOTNET_ENVIRONMENT and Weather__TimeoutSeconds cleared.'
