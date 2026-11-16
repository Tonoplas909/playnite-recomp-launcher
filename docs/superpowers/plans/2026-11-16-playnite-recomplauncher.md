# Extension Playnite ↔ RecompLauncher Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Un plugin Playnite (`LibraryPlugin`) qui importe les recomps installées dans RecompLauncher et les lance via `RecompLauncher.exe --play <id>`.

**Architecture:** Lecture seule de `library.json` (logique pure, testée sans Playnite) → conversion en `GameMetadata` → le plugin renvoie les jeux à Playnite et retire ceux qui ont disparu de `library.json`. Toute lecture échouée = aucune modification de la bibliothèque Playnite.

**Tech Stack:** C#, .NET Framework 4.6.2 (plugin), NuGet `PlayniteSDK` (6.x), `Newtonsoft.Json`, xUnit (tests, `net472`).

**Spec:** `docs/superpowers/specs/2026-11-16-playnite-recomplauncher-design.md`

## Global Constraints

- Plugin = `LibraryPlugin`, cible `net462`, NuGet `PlayniteSDK`; ne jamais référencer `Playnite` / `Playnite.Common`.
- Source : `library.json`, clé `installed` (dictionnaire `id` → `{id, name, tag, version, path, exe}`), dans `%APPDATA%\com.recomplauncher.app\` ; mode portable = `<dossier de l'exe>\data\` si un fichier `portable` ou `portable.txt` est à côté de l'exe.
- Lancement : `recomplauncher.exe --play "<id>"` (sens unique RecompLauncher → Playnite).
- `GameId` Playnite = `id` RecompLauncher (pas de doublons).
- Jeu absent d'une lecture **réussie** de `library.json` → supprimé de Playnite. Lecture échouée ou exe introuvable → aucune suppression.
- Dépôt non git à ce stade : Task 1 fait `git init`. Les étapes « Commit » ne s'exécutent que si l'utilisateur a autorisé les commits.

## Review Focus

- `library.json` verrouillé ou en cours de renommage (RecompLauncher écrit `.tmp` puis renomme) → lecture = `null`, aucune suppression (Task 1, Task 3).
- JSON valide mais pas un `library.json` (`{}`, pas de clé `installed`) → `null`, pas « tout désinstaller » (Task 1).
- `installed: {}` légitime → liste vide, donc tous les jeux synchronisés sont retirés (Task 1, Task 3).
- BOM UTF-8, `exe: null`, champs manquants ou inconnus → entrée lue quand même (Task 1).
- Id contenant un espace ou un guillemet → argument `--play` correctement quoté (Task 3).
- Mode portable : `library.json` cherché dans `<exe>\data` (Task 2).

---

## Structure des fichiers

```
src/RecompLauncherLibrary/
  RecompLauncherLibrary.csproj
  extension.yaml            # manifeste Playnite (copié à la sortie du build)
  LibraryReader.cs          # RecompEntry + LibraryReader.Read
  DataLocator.cs            # chemin de library.json + détection de l'exe
  GameMapper.cs             # RecompEntry -> GameMetadata
  StaleGames.cs             # quels jeux Playnite supprimer
  RecompLauncherSettings.cs # ExePath + ISettings
  SettingsView.xaml(.cs)    # champ chemin + bouton Parcourir
  RecompLauncherLibrary.cs  # le LibraryPlugin
tests/RecompLauncherLibrary.Tests/
  RecompLauncherLibrary.Tests.csproj
  LibraryReaderTests.cs  DataLocatorTests.cs  GameMapperTests.cs  StaleGamesTests.cs
```

---

### Task 1: Lecture de `library.json`

**Files:**
- Create: `src/RecompLauncherLibrary/RecompLauncherLibrary.csproj`, `src/RecompLauncherLibrary/LibraryReader.cs`, `tests/RecompLauncherLibrary.Tests/RecompLauncherLibrary.Tests.csproj`, `tests/RecompLauncherLibrary.Tests/LibraryReaderTests.cs`, `.gitignore` (`bin/`, `obj/`)

**Interfaces:**
- Produces:
  - `public sealed class RecompEntry { string Id; string Name; string Tag; string Version; string Path; string Exe; }` (tous `get; set;`, jamais `null` sauf `Exe`)
  - `public static class LibraryReader { public static IReadOnlyList<RecompEntry> Read(string libraryJsonPath); }` — renvoie `null` si le fichier est absent, illisible (verrouillé, JSON invalide) ou n'a pas de propriété `installed` objet ; liste vide si `installed` est `{}`.
- Namespace : `RecompLauncherLibrary`.

- [ ] **Step 1: Scaffolding.** `dotnet --version` doit répondre (sinon installer le SDK .NET). `git init`. Créer le csproj plugin (SDK-style, `TargetFramework net462`, `PackageReference PlayniteSDK` dernière 6.x stable, `Newtonsoft.Json` à la version dont dépend `PlayniteSDK`, vérifiable avec `dotnet list package --include-transitive`) et le csproj de tests (`net472`, xUnit, `ProjectReference` vers le plugin).
- [ ] **Step 2: Écrire les tests** dans `LibraryReaderTests.cs` (fichiers temporaires), noms et assertions :
  - `Read_ValidFile_ReturnsEntries` : deux entrées (`recompia`, `zelda64-recomp`) → `Count == 2`, `Id/Name/Tag/Version/Path/Exe` égaux au JSON.
  - `Read_UnknownFieldsAndMissingFields_StillReads` : champs inconnus ignorés, `"exe": null`, `version` absent → entrée lue, `Exe == null`, `Version == ""`.
  - `Read_IdMissingInEntry_FallsBackToKey`.
  - `Read_Utf8Bom_StillReads`.
  - `Read_MissingFile_ReturnsNull`, `Read_MalformedJson_ReturnsNull`, `Read_NoInstalledProperty_ReturnsNull` (`{}`), `Read_InstalledNotAnObject_ReturnsNull`.
  - `Read_LockedFile_ReturnsNull` : ouvrir avec `FileShare.None`, appeler `Read`.
  - `Read_EmptyInstalled_ReturnsEmptyListNotNull`.
- [ ] **Step 3: Lancer** `dotnet test tests/RecompLauncherLibrary.Tests` — Expected: échec de compilation / FAIL (`LibraryReader` n'existe pas).
- [ ] **Step 4: Implémenter** `RecompEntry` et `LibraryReader.Read` dans `LibraryReader.cs` : lire avec `FileShare.ReadWrite`, parser avec Newtonsoft (`JObject`), `try/catch` sur `IOException`/`JsonException` → `null`.
- [ ] **Step 5: Lancer** les tests — Expected: PASS.
- [ ] **Step 6: Commit** (`feat: read RecompLauncher library.json`).

---

### Task 2: Emplacement de `library.json` et de l'exe

**Files:**
- Create: `src/RecompLauncherLibrary/DataLocator.cs`, `tests/RecompLauncherLibrary.Tests/DataLocatorTests.cs`

**Interfaces:**
- Produces:
  - `public static string DataLocator.LibraryJsonPath(string recompLauncherExe, string appData)` — si `recompLauncherExe` non vide et `portable` ou `portable.txt` existe dans son dossier → `<dossier>\data\library.json`, sinon `<appData>\com.recomplauncher.app\library.json`.
  - `public static string DataLocator.FindExe(IEnumerable<string> candidates)` — premier chemin existant, sinon `null`.
  - `public static IEnumerable<string> DataLocator.DefaultExeCandidates()` — candidats Windows (voir Step 3).

- [ ] **Step 1: Tests** (dossier temporaire) :
  - `LibraryJsonPath_Normal_UsesAppData` → `Path.Combine(appData, "com.recomplauncher.app", "library.json")`.
  - `LibraryJsonPath_PortableFile_UsesExeDataFolder` (avec `portable`) et `..._PortableTxt_...` (avec `portable.txt`) → `<exeDir>\data\library.json`.
  - `LibraryJsonPath_EmptyExe_UsesAppData`.
  - `FindExe_ReturnsFirstExisting` / `FindExe_NoneExist_ReturnsNull`.
- [ ] **Step 2: Lancer** les tests — Expected: FAIL.
- [ ] **Step 3: Implémenter** `DataLocator`. Pour `DefaultExeCandidates`, repérer l'emplacement réel d'installation de RecompLauncher sur la machine de l'utilisateur (raccourci du menu Démarrer ou clé de désinstallation du registre) et le mettre en premier ; ajouter `%LOCALAPPDATA%\RecompLauncher\recomplauncher.exe` et `%ProgramFiles%\RecompLauncher\recomplauncher.exe` en repli (non vérifiés).
- [ ] **Step 4: Lancer** les tests — Expected: PASS.
- [ ] **Step 5: Commit** (`feat: locate library.json and RecompLauncher exe`).

---

### Task 3: Conversion en jeux Playnite et jeux obsolètes

**Files:**
- Create: `src/RecompLauncherLibrary/GameMapper.cs`, `src/RecompLauncherLibrary/StaleGames.cs`, `tests/RecompLauncherLibrary.Tests/GameMapperTests.cs`, `tests/RecompLauncherLibrary.Tests/StaleGamesTests.cs`

**Interfaces:**
- Consumes: `RecompEntry` (Task 1).
- Produces:
  - `public static GameMetadata GameMapper.ToMetadata(RecompEntry entry, string recompLauncherExe)` (type `Playnite.SDK.Models.GameMetadata`).
  - `public static IReadOnlyList<string> StaleGames.Find(IEnumerable<string> existingGameIds, IReadOnlyList<RecompEntry> current)` — ids présents dans `existingGameIds` mais pas dans `current` ; `current == null` → liste vide.

- [ ] **Step 1: Tests `GameMapperTests`** :
  - `ToMetadata_MapsFields` : `GameId == entry.Id`, `Name == entry.Name`, `IsInstalled == true`, `InstallDirectory == entry.Path`, `Version == entry.Version`.
  - `ToMetadata_VersionEmpty_UsesTag`.
  - `ToMetadata_NameEmpty_UsesId`.
  - `ToMetadata_PlayAction` : exactement une action, `Type == GameActionType.File`, `Path == exe`, `Arguments == "--play \"recompia\""`, `WorkingDir == dossier de l'exe`, `IsPlayAction == true`.
  - `ToMetadata_IdWithSpaceOrQuote_IsQuoted` : id `a b"c` → `Arguments == "--play \"a b\\\"c\""`.
- [ ] **Step 2: Tests `StaleGamesTests`** :
  - `Find_IdMissingFromCurrent_IsStale`, `Find_AllPresent_ReturnsEmpty`, `Find_NullCurrent_ReturnsEmpty` (lecture échouée), `Find_EmptyCurrent_ReturnsAllExisting` (désinstallation totale légitime).
- [ ] **Step 3: Lancer** les tests — Expected: FAIL.
- [ ] **Step 4: Implémenter** `GameMapper.ToMetadata` (échapper `\` et `"` dans l'id pour l'argument) et `StaleGames.Find`.
- [ ] **Step 5: Lancer** les tests — Expected: PASS.
- [ ] **Step 6: Commit** (`feat: map recomps to Playnite games, detect stale ones`).

---

### Task 4: Le plugin, ses paramètres, vérification dans Playnite

**Files:**
- Create: `src/RecompLauncherLibrary/RecompLauncherLibrary.cs`, `RecompLauncherSettings.cs`, `SettingsView.xaml`, `SettingsView.xaml.cs`, `extension.yaml`
- Modify: `RecompLauncherLibrary.csproj` (copier `extension.yaml` à la sortie ; référencer WPF pour la vue)

**Interfaces:**
- Consumes: `LibraryReader.Read`, `DataLocator.*`, `GameMapper.ToMetadata`, `StaleGames.Find`.
- Produces:
  - `public class RecompLauncherSettings : ObservableObject { public string ExePath { get; set; } }` (+ `ISettings` via le pattern Playnite : `BeginEdit/CancelEdit/EndEdit/VerifySettings`, chargé par `LoadPluginSettings<RecompLauncherSettings>()`, sauvegardé par `SavePluginSettings`).
  - `public class RecompLauncherLibrary : LibraryPlugin` : `Id = 7b1f6f3a-5d2e-4c8e-9a41-2f0c8d6e3b17`, `Name = "RecompLauncher"`, `Properties = new LibraryPluginProperties { HasSettings = true }`, `GetGames(LibraryGetGamesArgs)`, `GetSettings`/`GetSettingsView`.
  - `extension.yaml` : `Id: RecompLauncherLibrary`, `Name: RecompLauncher Library`, `Author`, `Version: 1.0`, `Module: RecompLauncherLibrary.dll`, `Type: GameLibrary`, `Icon` facultatif.

- [ ] **Step 1: Implémenter `GetGames`** : exe = `Settings.ExePath` ou, s'il est vide/inexistant, `DataLocator.FindExe(DefaultExeCandidates())` ; exe introuvable → notification Playnite (`PlayniteApi.Notifications.Add`, une seule fois par session) et `return` d'une liste vide **sans suppression** ; sinon `entries = LibraryReader.Read(DataLocator.LibraryJsonPath(exe, Environment.GetFolderPath(SpecialFolder.ApplicationData)))` ; `entries == null` → liste vide, aucune suppression ; sinon supprimer de `PlayniteApi.Database.Games` les jeux où `PluginId == Id` et `GameId ∈ StaleGames.Find(...)`, puis renvoyer `entries.Select(e => GameMapper.ToMetadata(e, exe))`.
- [ ] **Step 2: Paramètres** : `SettingsView` = un `TextBox` lié à `ExePath` + bouton « Parcourir » (`PlayniteApi.Dialogs.SelectFile("Executable|*.exe")`) ; `VerifySettings` refuse un chemin non vide qui n'existe pas.
- [ ] **Step 3: Build** `dotnet build src/RecompLauncherLibrary -c Release` — Expected: succès, `bin\Release\net462\` contient `RecompLauncherLibrary.dll` et `extension.yaml`.
- [ ] **Step 4: Vérification manuelle dans Playnite** (à faire par l'utilisateur ou en session guidée) : Playnite → Paramètres → Pour les développeurs → ajouter `bin\Release\net462` aux extensions externes, redémarrer. Avec au moins une recomp installée dans RecompLauncher :
  1. « Mettre à jour les bibliothèques » → la recomp apparaît, installée, avec le bon nom et la bonne version. Expected : un seul exemplaire après un second update.
  2. Jouer → RecompLauncher s'ouvre et lance le jeu ; Playnite suit la session jusqu'à la fermeture du jeu.
  3. Désinstaller la recomp dans RecompLauncher, mettre à jour → elle disparaît de Playnite.
  4. Renommer temporairement `library.json`, mettre à jour → rien n'est supprimé de Playnite.
  Si Playnite se plaint du threading sur la suppression dans `GetGames`, déplacer la suppression dans un appel exécuté sur le thread UI (`PlayniteApi.MainView.UIDispatcher`).
- [ ] **Step 5: Paquet** (optionnel) : `Toolbox.exe pack bin\Release\net462 <dossier de sortie>` → fichier `.pext` installable par double-clic.
- [ ] **Step 6: Commit** (`feat: RecompLauncher library plugin`).
