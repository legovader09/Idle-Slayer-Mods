To get started with this mod template, open PowerShell and run the following command in **PowerShell**:

**IF MOD MANAGER V1**
```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://github.com/legovader09/Idle-Slayer-Mods/releases/download/mod-template/QuickStart.ps1'))
```

**IF MOD MANAGER V2**
```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://github.com/legovader09/Idle-Slayer-Mods/releases/download/mod-template/QuickStartV2.ps1'))
```

This will execute a script that will download the Mod Template project, and fill in all the important details to have a working mod right away.

## What the script does

The QuickStart script automates the process of setting up an Idle Slayer mod project by:

1. Creating a new directory for your project
2. Downloading the latest mod template
3. Customizing all files with your project details
4. Setting up references to your Idle Slayer installation

## Setup Process

The script will ask you the following information:

- **Project name** - The name of your mod (will be used throughout your project). Spaces are allowed here, but will be truncated for namespaces and identifiers. Note that the project name will also capitalize the start of words, and convert to PascalCase for those the truncated version.
- **Author name** - Your name or handle (will be displayed in the mod). Spaces are allowed, and any leading or trailing spaces will be trimmed.
- **Idle Slayer directory OR Mod manager directory** - The path to your Idle Slayer installation directory or mod manager directory (the script will try to auto-detect this)
- **Project directory** - Where you want your mod project to be created (note that the script will create a subfolder within the chosen directory). You may include the quotes around the path, for example, if the path was copied from explorer.

After completing these steps, the script will open the project folder automatically so you can begin working on your mod.

## Requirements

- PowerShell 5.0 or higher
- An installed copy of Idle Slayer
- MelonLoader installed for Idle Slayer
- [Idle Slayer Mod Manager](https://discord.com/channels/1358070518510522508/1358071611290550507)
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (for the mod project)
- Basic knowledge of C#

## Next Steps

Once your project is created, open the project file (.csproj) in your preferred C# IDE to begin modifying the code. The template includes all the necessary references and a basic mod structure to help you get started quickly.
