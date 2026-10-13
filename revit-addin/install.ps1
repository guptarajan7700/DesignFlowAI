$project = Join-Path $PSScriptRoot "src\DesignFlow.Revit\DesignFlow.Revit.csproj"
dotnet build $project -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $PSScriptRoot "src\DesignFlow.Revit\bin\Release\net8.0-windows\DesignFlow.Revit.dll"
if (-not (Test-Path $dll)) { throw "Build output was not found: $dll" }
$dll = (Resolve-Path $dll).Path

$years = @("2025", "2026")
foreach ($year in $years) {
    $folder = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year"
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
    $manifest = @"
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>DesignFlow AI</Name>
    <Assembly>$dll</Assembly>
    <AddInId>8f4c2a1e-6b7d-4e9a-9c3f-1d2e3f4a5b6c</AddInId>
    <FullClassName>DesignFlow.Revit.DesignFlowApp</FullClassName>
    <VendorId>DFLO</VendorId>
    <VendorDescription>DesignFlow AI studio plugins</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
    $manifestPath = Join-Path $folder "DesignFlow.Revit.addin"
    Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8
    Write-Host "Installed $manifestPath"
}

Write-Host "Restart Revit. The DesignFlow AI tab loads the Sync, Validation, Requirements, and Translation plugins."
