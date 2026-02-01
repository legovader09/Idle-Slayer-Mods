$Host.UI.RawUI.WindowTitle = "Idle Slayer Mod Creator"
if ($Host.UI.RawUI.WindowSize.Width -lt 100) {
    $Host.UI.RawUI.WindowSize = New-Object System.Management.Automation.Host.Size(100, 30)
}

function Write-ColoredText {
    param (
        [string]$Text,
        [string]$Color = "White"
    )
    
    Write-Host $Text -ForegroundColor $Color
}

function Show-AnimatedText {
    param (
        [string]$Text,
        [int]$Milliseconds = 10,
        [string]$Color = "Cyan"
    )
    
    foreach ($char in $Text.ToCharArray()) {
        Write-Host $char -NoNewline -ForegroundColor $Color
        Start-Sleep -Milliseconds $Milliseconds
    }
    Write-Host ""
}

function Write-HorizontalLine {
    param (
        [string]$Character = "═",
        [int]$Count = 80,
        [string]$Color = "DarkCyan"
    )
    
    Write-Host ($Character * $Count) -ForegroundColor $Color
}

Clear-Host
Write-HorizontalLine -Count 100 -Character "═"

$logo = @"
  _____    _ _         _____ _                       
 |_   _|  | | |       / ____| |                      
   | |  __| | | ___  | (___ | | __ _ _   _  ___ _ __ 
   | | / _' | |/ _ \  \___ \| |/ _' | | | |/ _ \ '__|
  _| || (_| | |  __/  ____) | | (_| | |_| |  __/ |   
 |_____\__,_|_|\___| |_____/|_|\__,_|\__, |\___|_|  
                                      __/ |         
                                     |___/          
  __  __           _    _____                _             
 |  \/  |         | |  / ____|              | |            
 | \  / | ___   __| | | |     _ __ ___  __ _| |_ ___  _ __ 
 | |\/| |/ _ \ / _' | | |    | '__/ _ \/ _' | __/ _ \| '__|
 | |  | | (_) | (_| | | |____| | |  __/ (_| | || (_) | |   
 |_|  |_|\___/ \__,_|  \_____|_|  \___|\__,_|\__\___/|_|   
                                                            
"@

foreach ($line in $logo -split "`n") {
    Write-ColoredText $line "Magenta"
    Start-Sleep -Milliseconds 100
}

Write-HorizontalLine -Count 100 -Character "═"
Show-AnimatedText "Welcome to the Idle Slayer Mod Creation Wizard!" -Color "Cyan" -Milliseconds 10
Show-AnimatedText "This creator assumes you already have Idle Slayer Mod Manager, or MelonLoader installed." -Color "Cyan" -Milliseconds 10
Write-HorizontalLine -Count 100 -Character "─" -Color "Blue"

$ISMMFolder = Join-Path -Path $env:LOCALAPPDATA -ChildPath "IdleSlayerModManager"
$ModLoaderPath = Join-Path -Path $ISMMFolder -ChildPath "ModLoader"
$DefaultISMMPath = $ISMMFolder
$DefaultIdleSlayerGamePath = Join-Path -Path ${env:ProgramFiles(x86)} -ChildPath "Steam\steamapps\common\Idle Slayer"
$ISMMDetected = Test-Path $ModLoaderPath
$IdleSlayerDetected = Test-Path $DefaultIdleSlayerGamePath

Write-HorizontalLine -Count 100 -Character "·" -Color "DarkGray"
Write-Host ">> " -NoNewline -ForegroundColor Yellow
do {
    Write-Host "Enter project name: " -NoNewline -ForegroundColor Cyan
    $ProjectName = Read-Host
    if ($ProjectName.Trim() -eq "") {
        Write-ColoredText "!! Project name cannot be empty. Please enter a value." "Yellow"
    }
} until ($ProjectName.Trim() -ne "")

if ($ProjectName -cmatch '[a-z][A-Z]') {
    $ProjectName = $ProjectName.Substring(0,1).ToUpper() + $ProjectName.Substring(1)
} else {
    $ProjectName = (Get-Culture).TextInfo.ToTitleCase($ProjectName.ToLower())
}

Write-Host ">> " -NoNewline -ForegroundColor Yellow
do {
    Write-Host "Enter author name: " -NoNewline -ForegroundColor Cyan
    $AuthorName = Read-Host
    if ($AuthorName.Trim() -eq "") {
        Write-ColoredText "!! Author name cannot be empty. Please enter a value." "Yellow"
    }
} until ($AuthorName.Trim() -ne "")

Write-Host ">> " -NoNewline -ForegroundColor Yellow
if ($ISMMDetected) {
    Write-ColoredText "√ ISMM installation detected!" "Green"
    Write-ColoredText "  Path: $DefaultISMMPath" "DarkGreen"
    $IdleSlayerDir = $DefaultISMMPath
} elseif ($IdleSlayerDetected) {
    Write-ColoredText "√ Idle Slayer game installation detected!" "Green"
    Write-ColoredText "  Path: $DefaultIdleSlayerGamePath" "DarkGreen"
    $IdleSlayerDir = $DefaultIdleSlayerGamePath
} else {
    Write-ColoredText "× Idle Slayer Mod Manager folder or Idle Slayer game folder could not be found in default location." "Yellow"
    
    do {
        Write-Host "Enter Idle Slayer Mod Manager OR Idle Slayer GAME directory:" -NoNewline -ForegroundColor Cyan
        $IdleSlayerDir = Read-Host
        $IdleSlayerDir = $IdleSlayerDir -replace '^"|"$', ''
        
        if ($IdleSlayerDir.Trim() -eq "") {
            Write-ColoredText "!! Directory cannot be empty. Please enter a value." "Yellow"
        }
    } until ($IdleSlayerDir.Trim() -ne "")
}

Write-Host ">> " -NoNewline -ForegroundColor Yellow
do {
    Write-Host "Enter project location (this creates a subfolder with the project name): " -NoNewline -ForegroundColor Cyan
    $ProjectLocation = Read-Host
    $ProjectLocation = $ProjectLocation -replace '^"|"$', ''
    
    if ($ProjectLocation.Trim() -eq "") {
        Write-ColoredText "!! Project location cannot be empty. Please enter a value." "Yellow"
    }
} until ($ProjectLocation.Trim() -ne "")

$SafeProjectName = $ProjectName -replace "\s", ""
$ProjectLocation = Join-Path -Path $ProjectLocation -ChildPath $SafeProjectName

Write-HorizontalLine -Count 100 -Character "─" -Color "Blue"

Write-Host "▼ " -NoNewline -ForegroundColor Cyan
Show-AnimatedText "Downloading mod template..." -Color "White"

$ZipURL = "https://github.com/legovader09/Idle-Slayer-Mods/releases/download/mod-template/ModTemplate.zip"
$ZipFile = "$ProjectLocation\ModTemplate.zip"

if (!(Test-Path $ProjectLocation)) {
    Write-Host "  Creating directory: " -NoNewline -ForegroundColor DarkGray
    Write-ColoredText $ProjectLocation "Gray"
    New-Item -ItemType Directory -Path $ProjectLocation -Force | Out-Null
}

Write-Host "  Fetching template from GitHub..." -ForegroundColor DarkGray
$ProgressPreference = 'SilentlyContinue'
Invoke-WebRequest -Uri $ZipURL -OutFile $ZipFile
$ProgressPreference = 'Continue'

Write-Host "  [" -NoNewline -ForegroundColor DarkGray
for ($i = 0; $i -lt 20; $i++) {
    Write-Host "■" -NoNewline -ForegroundColor Cyan
    Start-Sleep -Milliseconds 25
}
Write-Host "] " -NoNewline -ForegroundColor DarkGray
Write-ColoredText "Complete!" "Green"

Write-Host "□ " -NoNewline -ForegroundColor Cyan
Show-AnimatedText "Extracting mod template..." -Color "White"
Expand-Archive -Path $ZipFile -DestinationPath $ProjectLocation -Force

Remove-Item $ZipFile -Force
Write-Host "  Temporary files cleaned up" -ForegroundColor DarkGray

Write-Host "≡ " -NoNewline -ForegroundColor Cyan
Show-AnimatedText "Customizing your project..." -Color "White"
Write-Host "  Processing files in: $ProjectLocation" -ForegroundColor DarkGray

$fileCount = 0
Get-ChildItem -Path $ProjectLocation -Recurse -File | ForEach-Object {
    $fileCount++
    $filePath = $_.FullName
    $content = Get-Content -Path $filePath -Raw
    $content = $content -replace '\$safeprojectname\$', $SafeProjectName
    $content = $content -replace '\$projectname\$', $ProjectName
    $content = $content -replace '\$author\$', $AuthorName
    
    if ($IdleSlayerDir.StartsWith($env:LOCALAPPDATA, [System.StringComparison]::OrdinalIgnoreCase)) {
        $relativeDir = $IdleSlayerDir.Substring($env:LOCALAPPDATA.Length).TrimStart('\')
        if ([string]::IsNullOrEmpty($relativeDir)) {
            $replacement = '$(LocalAppData)'
        } else {
            $replacement = "`$(LocalAppData)\$relativeDir"
        }
        $content = $content -replace '\$idleslayerdir\$', $replacement
    } else {
        $content = $content -replace '\$idleslayerdir\$', $IdleSlayerDir
    }
    
    Set-Content -Path $filePath -Value $content
}
Write-Host "  Modified content in $fileCount files" -ForegroundColor DarkGray

$renamedCount = 0
Get-ChildItem -Path $ProjectLocation -Recurse -File | ForEach-Object {
    $oldFilePath = $_.FullName
    $newFileName = $_.Name
    $newFileName = $newFileName -replace '\$safeprojectname\$', $SafeProjectName
    $newFileName = $newFileName -replace '\$projectname\$', $ProjectName
    $newFileName = $newFileName -replace '\$author\$', $AuthorName

    if ($IdleSlayerDir.StartsWith($env:LOCALAPPDATA, [System.StringComparison]::OrdinalIgnoreCase)) {
        $relativeDir = $IdleSlayerDir.Substring($env:LOCALAPPDATA.Length).TrimStart('\')
        if ([string]::IsNullOrEmpty($relativeDir)) {
            $replacement = '$(LocalAppData)'
        } else {
            $replacement = "`$(LocalAppData)\$relativeDir"
        }
        $newFileName = $newFileName -replace '\$idleslayerdir\$', $replacement
    } else {
        $newFileName = $newFileName -replace '\$idleslayerdir\$', $IdleSlayerDir
    }

    if ($_.Name -ne $newFileName) {
        $renamedCount++
        $newFilePath = Join-Path $_.DirectoryName $newFileName
        Rename-Item -Path $oldFilePath -NewName $newFileName
    }
}
Write-Host "  Renamed $renamedCount files" -ForegroundColor DarkGray

Write-HorizontalLine -Count 100 -Character "─" -Color "Blue"

Write-Host "√ " -NoNewline -ForegroundColor Green
Show-AnimatedText "Mod project created successfully!" -Color "Green" -Milliseconds 10

Write-Host ""
Write-Host "• Project Summary:" -ForegroundColor Yellow
Write-Host "  Project Name: " -NoNewline -ForegroundColor DarkYellow
Write-ColoredText $ProjectName "White"
Write-Host "  Author: " -NoNewline -ForegroundColor DarkYellow
Write-ColoredText $AuthorName "White"
Write-Host "  Project Location: " -NoNewline -ForegroundColor DarkYellow
Write-ColoredText $ProjectLocation "White"
Write-Host "  Game Directory: " -NoNewline -ForegroundColor DarkYellow
Write-ColoredText $IdleSlayerDir "White"
Write-Host ""

Write-HorizontalLine -Count 100 -Character "═"
Write-Host "» Opening project folder..." -ForegroundColor Cyan
Start-Process "explorer.exe" -ArgumentList "$ProjectLocation"

Write-ColoredText "`nPress any key to exit..." "DarkGray"
$host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null