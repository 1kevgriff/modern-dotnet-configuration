#!/usr/bin/env pwsh
# Restores appsettings.json to its committed starting state after the live edit on stage.
# Self-contained on purpose: it does not shell out to git, so the folder still works
# when it has been copied somewhere else.

$ErrorActionPreference = 'Stop'

$settings = Join-Path $PSScriptRoot 'appsettings.json'

$original = @'
{
  "Weather": {
    "TimeoutSeconds": 30
  }
}
'@

Set-Content -Path $settings -Value $original -Encoding utf8NoBOM -NoNewline
Add-Content -Path $settings -Value "`n" -Encoding utf8NoBOM -NoNewline

Write-Host "reset: Weather:TimeoutSeconds is back to 30"
