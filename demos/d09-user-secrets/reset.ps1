#!/usr/bin/env pwsh
# Restores the committed starting state: no secrets in the store, no environment override.

Push-Location $PSScriptRoot
try {
    dotnet user-secrets clear
}
finally {
    Pop-Location
}

Remove-Item -Path 'Env:DOTNET_ENVIRONMENT' -ErrorAction SilentlyContinue

Write-Host "d09 reset: secret store cleared, DOTNET_ENVIRONMENT unset."
