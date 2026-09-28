#!/usr/bin/env pwsh
# Clears the environment variables this demo asks you to set, for the current session.

$names = @(
    'Weather__ApiBaseUrl',
    'Weather__AllowedOrigins__0',
    'Weather__AllowedOrigins__1',
    'MYAPP_Weather__TimeoutSeconds'
)

foreach ($name in $names) {
    Remove-Item -Path "Env:$name" -ErrorAction SilentlyContinue
}

Write-Host "d04 reset: environment variables cleared."
