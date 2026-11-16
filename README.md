# RecompLauncher Library for Playnite

A [Playnite](https://playnite.link) library plugin that imports the games you installed with
[RecompLauncher](https://recomplauncher.com) and launches them from Playnite.

*Extension Playnite (bibliothèque) qui importe les recomps installées dans RecompLauncher.*

## What it does

- Imports every recomp installed in RecompLauncher as a Playnite game (one entry per recomp, no duplicates).
- **Play** starts the game through RecompLauncher (`recomplauncher.exe --play "<id>"`), so RecompLauncher
  still handles setup, saves and tracking. Playnite follows the session until the game closes.
- Names the Playnite game after the **original game** (for example *The Legend of Zelda: Tears of the Kingdom*
  instead of *TotK Recomp*) and sets its **platform**, so Playnite's built-in IGDB metadata provider can find it.
- Uses RecompLauncher's own **icon** for the game. Covers and backgrounds come from your metadata provider.
- Imports **play time** and **last played** from RecompLauncher.
- Updates the **version** of games that are already in Playnite.
- Removes a game from Playnite when it is no longer installed in RecompLauncher.
- Sets the source to "RecompLauncher".

Sync is one-way: RecompLauncher -> Playnite. Install, update and uninstall in RecompLauncher.

## Install

1. Download `RecompLauncherLibrary_1_0.pext` from the [latest release](../../releases/latest).
2. Double-click it (or drag it onto Playnite) and restart Playnite.
3. Run *Update Game Libraries*. If RecompLauncher is not found automatically, set the path to
   `recomplauncher.exe` in the add-on's settings.

To download metadata, select the imported games, then *Edit -> Download metadata -> IGDB*, or enable
automatic metadata download for new games in Playnite's settings.

> Playnite only applies a game's name, platform and icon when the game is **created**. If a game was
> imported by an older version of the plugin, delete it from Playnite once and update the library again.

## Limitations

- Reads RecompLauncher's **internal files** (`library.json` and the cached `pc-catalog.json` in
  `%APPDATA%\com.recomplauncher.app`, or the `data` folder in portable mode). They are not a public API,
  so a RecompLauncher update can break the plugin.
- Original name, platform and icon come from RecompLauncher's catalog. A recomp that is not in the catalog keeps
  the name RecompLauncher gives it, with no platform.
- Windows only.

## Build

Requires the .NET SDK. The plugin targets .NET Framework 4.6.2 and the Playnite SDK.

```
dotnet test tests/RecompLauncherLibrary.Tests
dotnet build src/RecompLauncherLibrary -c Release
```

To try the build without installing it: Playnite -> Settings -> For developers -> External extensions,
add `src/RecompLauncherLibrary/bin/Release/net462`, then restart Playnite.
To package it: `Toolbox.exe pack <build folder> <output folder>` (Toolbox is in Playnite's install folder).

## License

[MIT](LICENSE)
