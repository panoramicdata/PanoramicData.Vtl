<#
.SYNOPSIS
    Publishes the NuGet package to nuget.org.
.DESCRIPTION
    This script performs the following steps:
    1. Checks for clean git working tree (porcelain)
    2. Determines the Nerdbank GitVersioning version
    3. Checks that nuget-key.txt exists, has content, and is gitignored
    4. Runs unit tests (unless -SkipTests is specified)
    5. Publishes to nuget.org
.PARAMETER SkipTests
    Skips running unit tests.
#>
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'

# Step 1: Check for git porcelain (clean working tree)
Write-Host "Checking for clean git working tree..." -ForegroundColor Cyan
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "Git working tree is not clean. Please commit or stash your changes."
    exit 1
}
Write-Host "Git working tree is clean." -ForegroundColor Green

# Step 2: Determine the Nerdbank GitVersioning version
Write-Host "Determining Nerdbank GitVersioning version..." -ForegroundColor Cyan
$nbgvOutput = nbgv get-version -f json 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to get Nerdbank GitVersioning version. Ensure nbgv is installed: dotnet tool install -g nbgv"
    exit 1
}
$versionInfo = $nbgvOutput | ConvertFrom-Json
$version = $versionInfo.NuGetPackageVersion
if (-not $version) {
    Write-Error "Failed to determine NuGet package version from Nerdbank GitVersioning."
    exit 1
}
Write-Host "Version: $version" -ForegroundColor Green

# Step 3: Check that nuget-key.txt exists, has content, and is gitignored
Write-Host "Checking nuget-key.txt..." -ForegroundColor Cyan
$nugetKeyPath = Join-Path $PSScriptRoot "nuget-key.txt"
if (-not (Test-Path $nugetKeyPath)) {
    Write-Error "nuget-key.txt does not exist in the solution root."
    exit 1
}
$nugetKey = (Get-Content $nugetKeyPath -Raw).Trim()
if (-not $nugetKey) {
    Write-Error "nuget-key.txt is empty."
    exit 1
}
$gitIgnoreCheck = git check-ignore "nuget-key.txt" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "nuget-key.txt is not gitignored. Add it to .gitignore to protect your API key."
    exit 1
}
Write-Host "nuget-key.txt exists, has content, and is gitignored." -ForegroundColor Green

# Step 4: Run unit tests (unless -SkipTests is specified)
if (-not $SkipTests) {
    Write-Host "Running unit tests..." -ForegroundColor Cyan
    dotnet test --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unit tests failed."
        exit 1
    }
    Write-Host "Unit tests passed." -ForegroundColor Green
} else {
    Write-Host "Skipping unit tests." -ForegroundColor Yellow
}

# Step 5: Build and pack
Write-Host "Building and packing..." -ForegroundColor Cyan
dotnet pack "PanoramicData.Vtl\PanoramicData.Vtl.csproj" --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build/pack failed."
    exit 1
}
Write-Host "Build and pack succeeded." -ForegroundColor Green

# Step 6: Publish to nuget.org
Write-Host "Publishing to nuget.org..." -ForegroundColor Cyan
$nupkgPath = Get-ChildItem -Path "PanoramicData.Vtl\bin\Release\*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $nupkgPath) {
    Write-Error "No .nupkg file found in PanoramicData.Vtl\bin\Release\"
    exit 1
}
dotnet nuget push $nupkgPath.FullName --api-key $nugetKey --source https://api.nuget.org/v3/index.json --skip-duplicate
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish to nuget.org."
    exit 1
}
Write-Host "Successfully published $($nupkgPath.Name) to nuget.org." -ForegroundColor Green

exit 0
