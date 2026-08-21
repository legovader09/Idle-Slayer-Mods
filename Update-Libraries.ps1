# PowerShell script to update DLLs in the Libraries folder from IdleSlayerModManager local app data

$librariesDir = Join-Path $PSScriptRoot "Libraries"
$localAppData = $env:LOCALAPPDATA
$sourceDirs = @(
    (Join-Path $localAppData "IdleSlayerModManager\ModLoader\MelonLoader\Il2CppAssemblies"),
    (Join-Path $localAppData "IdleSlayerModManager\ModLoader\MelonLoader\net6")
)

if (-not (Test-Path $librariesDir)) {
    Write-Error "Libraries directory not found at $librariesDir"
    exit
}

Write-Host "Scanning Libraries folder: $librariesDir"

$dllFiles = Get-ChildItem -Path $librariesDir -Filter "*.dll"

foreach ($dll in $dllFiles) {
    $found = $false
    foreach ($sourceDir in $sourceDirs) {
        if (Test-Path $sourceDir) {
            $sourcePath = Join-Path $sourceDir $dll.Name
            if (Test-Path $sourcePath) {
                Write-Host "Updating $($dll.Name) from $sourceDir" -ForegroundColor Cyan
                try {
                    Copy-Item -Path $sourcePath -Destination $dll.FullName -Force
                    $found = $true
                    break # Stop looking in other source folders if found
                }
                catch {
                    Write-Error "Failed to copy $($dll.Name): $($_.Exception.Message)"
                }
            }
        }
    }

    if (-not $found) {
        Write-Host "No matching file found for $($dll.Name) in source folders." -ForegroundColor Yellow
    }
}

Write-Host "Update process completed. Press any key to exit..." -ForegroundColor Green

$host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null
