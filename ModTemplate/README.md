To get started with this mod template, open PowerShell and run the following command in **PowerShell**:

```
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://github.com/legovader09/Idle-Slayer-Mods/releases/download/mod-template/QuickStart.ps1'))
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

- **Project name** - The name of your mod (will be used throughout your project)
- **Author name** - Your name or handle (will be displayed in the mod)
- **Idle Slayer directory** - The path to your Idle Slayer installation directory (the script will try to auto-detect this)
- **Project directory** - Where you want your mod project to be created (note that the script will create a subfolder within the chosen directory)

After completing these steps, the script will open the project folder automatically so you can begin working on your mod.

## Requirements

- PowerShell 5.0 or higher
- An installed copy of Idle Slayer
- Basic knowledge of C# if you plan to modify the code
- MelonLoader installed for Idle Slayer
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Next Steps

Once your project is created, open the project file (.csproj) in your preferred C# IDE to begin modifying the code. The template includes all the necessary references and a basic mod structure to help you get started quickly.