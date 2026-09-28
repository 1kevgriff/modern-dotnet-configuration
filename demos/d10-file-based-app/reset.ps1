#!/usr/bin/env pwsh
# Restores the committed starting state: no secrets in the file-based app's store.

Push-Location $PSScriptRoot
try {
    dotnet user-secrets clear --file config.cs
}
finally {
    Pop-Location
}

Write-Host "d10 reset: secret store cleared."
