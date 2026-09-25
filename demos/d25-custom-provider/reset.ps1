#!/usr/bin/env pwsh
# Restores settings.env to its committed contents. The demo rewrites it on every run.

$ErrorActionPreference = 'Stop'
$envFile = Join-Path $PSScriptRoot 'settings.env'

@(
    '# settings.env - read by a hand-written provider, not by anything built in.'
    "# '__' becomes ':', so Greeting__Message sets Greeting:Message."
    'Greeting__Message=Hello from the .env file'
) | Set-Content -Path $envFile -Encoding utf8

Write-Host "Reset settings.env"
