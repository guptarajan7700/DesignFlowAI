# Loads DesignFlowAI in script order. Run from this folder:
#   .\run.ps1 -Server "localhost"
#   .\run.ps1 -Server "(localdb)\MSSQLLocalDB"
param(
    [string]$Server = "localhost",
    [string]$Database = "DesignFlowAI",
    [string]$User,
    [string]$Password
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd was not found on PATH. Install SQL Server tools or the go-sqlcmd utility, then run this script again."
}

function Invoke-SqlFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$DatabaseName
    )

    Write-Host ">> $Path"
    $arguments = @("-S", $Server, "-b", "-I", "-i", $Path)
    if ($DatabaseName) {
        $arguments += @("-d", $DatabaseName)
    }
    if ($User) {
        $arguments += @("-U", $User, "-P", $Password)
    }
    else {
        $arguments += "-E"
    }

    # sqlcmd writes normal progress to stderr. Keep that from becoming a terminating error.
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    & sqlcmd @arguments
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorAction
    if ($exitCode -ne 0) {
        throw "sqlcmd failed for $Path (exit $exitCode)."
    }
}

Invoke-SqlFile -Path (Join-Path $PSScriptRoot "000_create_database.sql")

$folders = @(
    "01_schema",
    "02_constraints",
    "03_stored_procedures",
    "04_views",
    "05_seed_data",
    "06_migrations"
)

foreach ($folder in $folders) {
    $directory = Join-Path $PSScriptRoot $folder
    Get-ChildItem -Path $directory -Filter "*.sql" | Sort-Object Name | ForEach-Object {
        Invoke-SqlFile -Path $_.FullName -DatabaseName $Database
    }
}

Write-Host ""
Write-Host "DesignFlowAI is loaded. Verify with:"
Write-Host "sqlcmd -S $Server -d $Database -E -Q ""SELECT Code, Status, OpenIssueCount FROM dbo.vw_ProjectDashboard ORDER BY Code;"""
