#!/usr/bin/env pwsh
# Clears the environment variables this demo asks you to set, for the current session.

$names = @(
    'POSTGRESQLCONNSTR_Default',
    'SQLCONNSTR_Legacy'
)

foreach ($name in $names) {
    Remove-Item -Path "Env:$name" -ErrorAction SilentlyContinue
}

Write-Host "d05 reset: environment variables cleared."
