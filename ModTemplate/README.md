To get started with this mod template, open **PowerShell**, then copy and run the following command:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://github.com/legovader09/Idle-Slayer-Mods/releases/download/mod-template/QuickStart.ps1'))
```

This will execute a script that will download the Mod Template project, and fill in all the important details to have a working mod right away.


#### What the script does

The QuickStart script automates the process of setting up an Idle Slayer mod project by:

1. Creating a new directory for your project
2. Downloading the latest mod template
3. Customizing all files with your project details
4. Setting up references to your Idle Slayer installation


#### Setup Process

The script will ask you the following information:

- **Project name** - The name of your mod (will be used throughout your project). Spaces are allowed here, but will be truncated for namespaces and identifiers. Note that the project name will also capitalize the start of words, and convert to PascalCase for those the truncated version.
- **Author name** - Your name or handle (will be displayed in the mod). Spaces are allowed, and any leading or trailing spaces will be trimmed.
- **Idle Slayer directory OR Mod manager directory** - The path to your Idle Slayer installation directory or mod manager directory (the script will try to auto-detect this)
- **Project directory** - Where you want your mod project to be created (note that the script will create a subfolder within the chosen directory). You may include the quotes around the path, for example, if the path was copied from explorer.

After completing these steps, the script will open the project folder automatically so you can begin working on your mod.


#### Requirements

- PowerShell 5.0 or higher
- An installed copy of Idle Slayer
- MelonLoader installed for Idle Slayer **OR** [Idle Slayer Mod Manager](https://discord.gg/SF9fcdk4uK)
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (for the mod project)
- Basic knowledge of C#, and programming foundations. It is highly recommended to learn the fundamentals if you are a beginner looking to get into the modding world. See also [my existing mods](http://github.com/legovader09/Idle-Slayer-Mods) for examples on how they work.
- For more modding resources, we have a useful page in our [Discord](https://discord.gg/SF9fcdk4uK) server, too.


#### Next Steps

Once your project is created, open the project file (.csproj) in your preferred C# IDE (Visual Studio, Rider, etc) to begin modifying the code. The template includes all the necessary references and a basic mod structure to allow you to use the mod right away.

Once you have built your project, a ZIP folder with your **mod.dll**, **manifest.json**, and **banner.png** gets created in a **Publish** folder located in your project directory, as well as sending a copy of your mod.dll to your Mods folder (either located where Mod Manager hosts the file, or your Idle Slayer game folder (automatically found or specificed during the Mod Creation script).

After building, you can simply launch Idle Slayer and you will see your mod loading in the console.


#### Debugging your code

Debugging your code is simple. Once the game is launched, you can head back to your **IDE** (code editor) - in this example we use **Visual Studio 2026**, however most IDEs should have this feature.

Next, head to `Debug` -> `Attach to Process` and find `Idle Slayer.exe` in the list, click on it and then press `Attach`. Now you can set breakpoints on your code where you can see step-by-step what your code is doing. If you don't know what breakpoints are, I recommend searching it up.


#### Making your mod compatible with the Mod Manager

The Mod Manager supports feature rich content enhancements outlined below. **These are entirely optional** and your mod will be compatible with the mod manager if you just provide the mod.dll file.

Here's an example of a mod **manifest.json**, this data will be used by the **Mod Manager** to display rich information, versioning, nexus mods links, and more. If you do not provide one with your mod, the **Mod Manager** will try to automatically populate key information, such as **name**, **author**, and **version**.
```json
{
  "name": "Auto Boost and Wind Dash",
  "author": "Doomnik",
  "description": "Automatically triggers boost and optionally wind dash on cooldown (enabled in config) with hotkeys B, and N.",
  "version": "1.1.5",
  "nexus_id": "4",
  "donate_url": "https://paypal.me/doomnikh",
  "github_url": "https://www.github.com/legovader09/Idle-Slayer-Mods",
  "dependencies": [
    "IdleSlayerMods.Common@1.1.9"
  ]
}
```

The **banner.png** is used to display a custom banner for your mod in the **Mod Manager**. The image should be sized **160x80** as this is how it is rendered on the manager. If your banner image is bigger, it will auto center and crop the rest. If you do not provide a banner with your mod, the **Mod Manager** will automatically create a randomly generated one with a purple background, and the mod title displayed in the center.
