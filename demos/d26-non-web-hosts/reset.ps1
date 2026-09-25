#!/usr/bin/env pwsh
# The worker demo asks you to set DOTNET_ENVIRONMENT. This clears it again so the next run of any
# other demo starts from Production.

$ErrorActionPreference = 'Stop'

if ($env:DOTNET_ENVIRONMENT) {
    Write-Host "Clearing DOTNET_ENVIRONMENT (was '$env:DOTNET_ENVIRONMENT')"
    Remove-Item Env:\DOTNET_ENVIRONMENT
}
else {
    Write-Host "DOTNET_ENVIRONMENT is already unset"
}
