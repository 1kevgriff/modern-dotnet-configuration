#!/usr/bin/env pwsh
# Clears the environment variable this demo sets, returning the flag to its committed state.
Remove-Item Env:\feature_management__feature_flags__0__enabled -ErrorAction SilentlyContinue
Write-Host 'd28 reset: feature_management__feature_flags__0__enabled cleared.'
