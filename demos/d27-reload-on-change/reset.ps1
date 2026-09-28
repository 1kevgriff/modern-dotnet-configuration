# Restores d27 to its committed starting state.
#
# The demo rewrites appsettings.json while it runs and puts it back on a clean exit.
# Run this if it was interrupted (Ctrl-C) between the edit and the restore.
#
# The environment variable needs no cleanup: the demo sets GREETING__FROMENV with the
# process-scoped overload, so it disappears when the process does.

$ErrorActionPreference = 'Stop'

$path = Join-Path $PSScriptRoot 'appsettings.json'

$committed = @'
{
  "Greeting": {
    "FromFile": "v1 - from appsettings.json",
    "FromEnv": "(overridden by the environment variable)"
  }
}
'@

Set-Content -Path $path -Value $committed -Encoding utf8
Write-Host "d27: appsettings.json restored to its committed state."
