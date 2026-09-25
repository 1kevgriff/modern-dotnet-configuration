#!/usr/bin/env pwsh
# Restores the shell to this demo's starting state: no machine-level override.
Remove-Item Env:Weather__ApiBaseUrl -ErrorAction SilentlyContinue
Write-Host 'd07 reset: Weather__ApiBaseUrl cleared.'
