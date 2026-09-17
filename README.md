# CyberCAT-SimpleGUI

A simplified offshoot of SirBitesalot's CyberCAT.

## Features

**Feature**                                                                     | **Stability**
------------------------------------------------------------------------------- | -----------
Save and load presets for your character's appearance.                          | Stable
Edit the quantity, flags, & mod tree of items in your inventory.                | Stable
Edit quest facts.                                                               | Stable
Quick actions - dedicated controls for changing money & making items legendary. | Semi-Stable

## Game Version Support

**Game Version**    | **Support** | **CyberCAT-SimpleGUI Version**
--------------------| ------------|-----------------------------
Patch 2.31 (PC, base game and Phantom Liberty) | Read-only loading checked on two EP1 saves and one no-DLC save; unchanged saves round-tripped and parsed again; editing and game acceptance still need testing | development branch
Patch 2.3          | Source compatibility update | v0.28c
Patch 1.5X          | Full        | >=v0.20a
Patch 1.3           | Partial     | >=v0.10a_r1
Patch 1.23          | Full        | >=v0.10a

## Usage

Install the .NET 8 Windows Desktop Runtime (or a newer major Windows Desktop Runtime) before running the editor.

1. Run **CP2077SaveEditor.exe**
2. Click **"Load Save"**
3. Make changes to your save.
    - Double-click items in your inventory to edit them.
    - Double-click nodes in an item's mod tree to edit them
4. Click **"Save Changes"**

When replacing an existing `sav.dat`, the editor now keeps a new timestamped `.bak` copy of the previous file in the same save directory. The replacement is written to a temporary file first. For patch 2.31, use a duplicate save directory until edited saves have been checked in game.

## Todo

- Refactor, refactor, refactor.
- Eat some of the spaghetti.
- Improve the functionality of the appearance tab.
- Move contribution section from readme to github wiki

## Credits

[CyberCAT by SirBitesalot and other contributors](https://github.com/WolvenKit/CyberCAT)
[WolvenKit by the WolvenKit team](https://github.com/WolvenKit/WolvenKit)

## Contribution

### VSCodium

**Task** | **Info**
-------- | ----------------------------------
Requires | .NET 8 SDK
Setup    | None
Build    | Terminal > Run Build Task...
Debug    | Unsupported

### Visual Studio 2022 (17.8 or later)

**Task** | **Info**
-------- | ----------------------------------
Requires | .NET 8 SDK, .NET desktop development workload
Setup    | Solution > Restore NuGet Packages
Build    | Build > Rebuild Solution
Debug    | Debug > Start Debugging
