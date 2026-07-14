<#
.SYNOPSIS
    Applies all migrations, then all seed files, against the local MySQL database.
    Run this from PowerShell. Connection details are read from ../.env
    (DB_CONNECTION_STRING) by default so no credentials are hardcoded here.

.EXAMPLE
    .\apply-db.ps1
    .\apply-db.ps1 -SkipSeed              # migrations only, no sample data
    .\apply-db.ps1 -CreateDatabase         # also create the database first
    .\apply-db.ps1 -Password "something"   # override any single value if needed
#>

param(
    [string]$MySqlHost,
    [int]$Port,
    [string]$Database,
    [string]$User,
    [string]$Password,
    [switch]$SkipSeed,
    [switch]$CreateDatabase
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$envFile = Join-Path (Split-Path -Parent $scriptDir) ".env"

# Parse DB_CONNECTION_STRING out of .env (server=..;port=..;database=..;user=..;password=..;)
# so nothing here needs a hardcoded credential.
if ((-not $MySqlHost -or -not $Database -or -not $User -or -not $Password) -and (Test-Path $envFile)) {
    $line = Get-Content $envFile | Where-Object { $_ -match '^\s*DB_CONNECTION_STRING\s*=' } | Select-Object -First 1
    if ($line) {
        $connString = $line -replace '^\s*DB_CONNECTION_STRING\s*=\s*', ''
        $parts = @{}
        foreach ($pair in $connString -split ';') {
            if ($pair -match '^\s*([^=]+)\s*=\s*(.*)\s*$') {
                $parts[$matches[1].Trim().ToLower()] = $matches[2].Trim()
            }
        }
        if (-not $MySqlHost) { $MySqlHost = $parts['server'] }
        if (-not $Port -and $parts['port']) { $Port = [int]$parts['port'] }
        if (-not $Database) { $Database = $parts['database'] }
        if (-not $User) { $User = $parts['user'] }
        if (-not $Password) { $Password = $parts['password'] }
    }
}

if (-not $MySqlHost) { $MySqlHost = "localhost" }
if (-not $Port) { $Port = 3306 }

if (-not $Database -or -not $User -or -not $Password) {
    Write-Error "Could not determine database connection details. Either create $envFile with DB_CONNECTION_STRING set, or pass -Database -User -Password explicitly."
    exit 1
}

function Invoke-SqlFile($file) {
    Write-Host "Applying $($file.Name)..." -ForegroundColor Cyan
    Get-Content $file.FullName -Raw | mysql --host=$MySqlHost --port=$Port --user=$User --password=$Password $Database
    if ($LASTEXITCODE -ne 0) {
        throw "Failed applying $($file.Name) (exit code $LASTEXITCODE)"
    }
}

if (-not (Get-Command mysql -ErrorAction SilentlyContinue)) {
    Write-Error "mysql client not found on PATH. Install MySQL client tools (or run these .sql files via MySQL Workbench / DBeaver instead)."
    exit 1
}

if ($CreateDatabase) {
    Write-Host "Creating database '$Database' if it doesn't exist..." -ForegroundColor Cyan
    mysql --host=$MySqlHost --port=$Port --user=$User --password=$Password -e "CREATE DATABASE IF NOT EXISTS ``$Database``;"
    if ($LASTEXITCODE -ne 0) { throw "Failed to create database (exit code $LASTEXITCODE)" }
}

Write-Host "`n=== Migrations ===" -ForegroundColor Yellow
Get-ChildItem (Join-Path $scriptDir "migrations") -Filter "*.sql" | Sort-Object Name | ForEach-Object {
    Invoke-SqlFile $_
}

if (-not $SkipSeed) {
    Write-Host "`n=== Seed data ===" -ForegroundColor Yellow
    Invoke-SqlFile (Get-Item (Join-Path $scriptDir "seed.sql"))
}

Write-Host "`nDone." -ForegroundColor Green
