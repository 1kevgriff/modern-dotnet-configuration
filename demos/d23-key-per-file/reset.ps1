#!/usr/bin/env pwsh
# d23 leaves nothing behind when run with `dotnet run`. The Docker path leaves a
# container, a network and a locally-built image; this removes them.

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot

try {
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker compose down --rmi local 2>&1 | Out-Null
        Write-Host 'Removed the d23 container, network and local image.'
    }
    else {
        Write-Host 'Docker not installed — nothing to reset.'
    }
}
finally {
    Pop-Location
}
