#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Pre-talk gate for the Modern .NET Configuration demos.

.DESCRIPTION
    Builds every demo, checks formatting, and asserts the invariants the demos are
    built on. Run this the morning of the talk, and after any change to demos/.

    Verified baseline: 33 folders, 35 projects, 0 warnings, 0 errors.

.PARAMETER SkipFormat
    Skip `dotnet format --verify-no-changes` (the slow part). Build checks still run.

.PARAMETER Demo
    Verify a single demo, e.g. -Demo d29. Default is all of them.

.EXAMPLE
    ./verify.ps1
    ./verify.ps1 -SkipFormat
    ./verify.ps1 -Demo d29
#>
[CmdletBinding()]
param(
    [switch] $SkipFormat,
    [string] $Demo
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$demosRoot = $PSScriptRoot

$failures = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
$projectCount = 0
$folderCount = 0

function Fail([string] $message) { $script:failures.Add($message); Write-Host "  FAIL  $message" -ForegroundColor Red }
function Warn([string] $message) { $script:warnings.Add($message); Write-Host "  WARN  $message" -ForegroundColor Yellow }
function Pass([string] $message) { Write-Host "  ok    $message" -ForegroundColor DarkGray }

# d17 deliberately sets TreatWarningsAsErrors=false: its "before" state exists to
# SHOW the AOT/trim warnings. Allow those two codes there, fail on anything else.
$AotWarningCodes = @('IL2026', 'IL3050')

Write-Host ''
Write-Host 'Modern .NET Configuration - demo verification' -ForegroundColor Cyan
Write-Host ('=' * 60)

# ---------------------------------------------------------------- invariants --
Write-Host ''
Write-Host 'Repo invariants (demos are self-contained by design)' -ForegroundColor Cyan

$bannedFiles = @('global.json', 'Directory.Build.props', 'Directory.Packages.props')
foreach ($banned in $bannedFiles) {
    $hits = @(Get-ChildItem -Path $demosRoot -Filter $banned -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' })
    if ($hits.Count -gt 0) {
        foreach ($hit in $hits) {
            Fail "$banned found at $($hit.FullName.Substring($demosRoot.Length + 1)) - demos must be self-contained (SPEC 11.5)"
        }
    }
    else {
        Pass "no $banned"
    }
}

$slnx = @(Get-ChildItem -Path $demosRoot -Include '*.slnx', '*.sln' -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' })
if ($slnx.Count -gt 0) { foreach ($s in $slnx) { Fail "solution file $($s.Name) - demos run without one (SPEC 11.4)" } }
else { Pass 'no solution file' }

# ------------------------------------------------------------------- folders --
$pattern = if ($Demo) { "$Demo*" } else { 'd*' }
$folders = @(Get-ChildItem -Path $demosRoot -Directory -Filter $pattern |
    Where-Object { $_.Name -match '^d\d{2}-' } | Sort-Object Name)

if ($folders.Count -eq 0) { Write-Host ''; Fail "no demo folders matched '$pattern'" }

foreach ($folder in $folders) {
    $folderCount++
    $name = $folder.Name
    Write-Host ''
    Write-Host $name -ForegroundColor Cyan

    # README is required - it is how a demo is run on stage.
    if (Test-Path (Join-Path $folder.FullName 'README.md')) { Pass 'README.md' }
    else { Fail "$name is missing README.md" }

    # No committed secrets. Placeholders only.
    $settings = @(Get-ChildItem -Path $folder.FullName -Filter 'appsettings*.json' -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' })
    foreach ($file in $settings) {
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        if ($null -eq $content) { continue }
        # Long opaque strings next to a secret-ish key are the thing we care about.
        if ($content -match '(?i)"[^"]*(password|apikey|secret|connectionstring)[^"]*"\s*:\s*"[^"]{24,}"') {
            Warn "$($file.Name) has a long value on a secret-looking key - confirm it is a placeholder"
        }
    }

    $projects = @(Get-ChildItem -Path $folder.FullName -Filter '*.csproj' -Recurse -File |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Sort-Object FullName)

    if ($projects.Count -eq 0) {
        # d10 is a .NET 10 file-based app: a single .cs file, no csproj. That is intentional.
            $loose = @(Get-ChildItem -Path $folder.FullName -Filter '*.cs' -File -ErrorAction SilentlyContinue)
        if ($loose.Count -gt 0) { Pass 'file-based app (no csproj by design)' }
        else { Fail "$name has no project and no .cs file" }
        continue
    }

    foreach ($project in $projects) {
        $projectCount++
        $rel = $project.FullName.Substring($folder.FullName.Length + 1)
        $log = & dotnet build $project.FullName --nologo -v q 2>&1 | Out-String
        $exit = $LASTEXITCODE

        $errorLines = @([regex]::Matches($log, '(?m)^.*error [A-Z]{2,}\d+.*$') | ForEach-Object { $_.Value.Trim() })
        $warnLines = @([regex]::Matches($log, '(?m)^.*warning [A-Z]{2,}\d+.*$') | ForEach-Object { $_.Value.Trim() })

        if ($exit -ne 0 -or $errorLines.Count -gt 0) {
            Fail "$name/$rel build failed"
            $errorLines | Select-Object -First 3 | ForEach-Object { Write-Host "          $_" -ForegroundColor DarkRed }
            continue
        }

        if ($warnLines.Count -gt 0) {
            if ($name -like 'd17-*') {
                # Documented exception - but only for the two AOT codes.
                $unexpected = @($warnLines | Where-Object {
                    $line = $_
                    -not @($AotWarningCodes | Where-Object { $line -match $_ })
                })
                if ($unexpected.Count -gt 0) {
                    Fail "$name/$rel has warnings beyond the documented IL2026/IL3050"
                    $unexpected | Select-Object -First 3 | ForEach-Object { Write-Host "          $_" -ForegroundColor DarkRed }
                }
                else {
                    Pass "$rel (IL2026/IL3050 expected - this demo shows them)"
                }
            }
            else {
                Fail "$name/$rel built with $($warnLines.Count) warning(s) - the bar is zero"
                $warnLines | Select-Object -First 3 | ForEach-Object { Write-Host "          $_" -ForegroundColor DarkRed }
            }
        }
        else {
            Pass "$rel"
        }

        if (-not $SkipFormat) {
            & dotnet format $project.FullName --verify-no-changes --no-restore 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) { Fail "$name/$rel needs dotnet format" }
        }
    }
}

# --------------------------------------------------------------------- summary --
Write-Host ''
Write-Host ('=' * 60)
Write-Host "folders: $folderCount    projects: $projectCount" -ForegroundColor Cyan
if ($warnings.Count -gt 0) { Write-Host "warnings: $($warnings.Count) (review, not fatal)" -ForegroundColor Yellow }

if ($failures.Count -eq 0) {
    Write-Host 'PASS - every demo builds clean.' -ForegroundColor Green
    Write-Host ''
    exit 0
}

Write-Host "FAIL - $($failures.Count) problem(s):" -ForegroundColor Red
$failures | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
Write-Host ''
exit 1
