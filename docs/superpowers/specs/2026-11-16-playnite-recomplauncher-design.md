# Extension Playnite ↔ RecompLauncher — design (court)

Statut : approuvé par l'utilisateur dans la conversation (2026-11-16).

## Intention
Les recomps s'installent et se mettent à jour dans RecompLauncher (elripalda/RecompLauncher).
Playnite doit refléter ce qui est installé et pouvoir le lancer, sans réinstaller ni dupliquer.

## Objectifs
- Chaque jeu installé dans RecompLauncher apparaît dans Playnite (nom, version, dossier).
- Lancer le jeu depuis Playnite démarre RecompLauncher avec `--play <id>`.
- Pas de doublons entre deux synchronisations.

## Hors périmètre
- Installer/désinstaller depuis Playnite.
- Sync Playnite → RecompLauncher.
- Catalogue complet des recomps non installées.

## Contraintes
- Plugin Playnite : `LibraryPlugin`, C# / .NET Framework 4.6.2, SDK Playnite (NuGet `PlayniteSDK`). Ne pas référencer d'assemblies non-SDK.
- Source de vérité : `library.json` dans `%APPDATA%\com.recomplauncher.app\` (ou à côté de l'exe en mode portable). Format interne non documenté : entrées `installed.<id>` avec `id`, `name`, `tag`, `path`, `exe`. Parseur tolérant (champs inconnus ignorés, fichier absent ou invalide = aucun changement, jamais de suppression en masse).
- Lancement : `RecompLauncher.exe --play <id>` (déjà utilisé par les raccourcis Steam/bureau ; le processus reste ouvert jusqu'à la fin du jeu).

## Approches
1. **Lire `library.json` + lancer via `--play`** — retenue. Simple, rien à installer côté RecompLauncher. Risque : format interne susceptible de changer (atténué par le parseur tolérant et un test).
2. API distante (`LAUNCHER-API.md`) — écartée : couvre compte/cloud, pas la bibliothèque locale.
3. Scruter les dossiers de jeux — écartée : fragile.

## Design
- À chaque « Mettre à jour la bibliothèque », lecture de `library.json` ; création ou mise à jour d'un jeu Playnite par entrée installée (`GameId` = `id` RecompLauncher).
- Action de lancement : `RecompLauncher.exe --play <id>`.
- Sens unique RecompLauncher → Playnite. Une entrée absente de `library.json` est retirée de Playnite (choix par défaut, validé implicitement).
- Paramètres du plugin : chemin de RecompLauncher (auto-détecté si possible, modifiable).

## Laissé au builder
Détection automatique du chemin, nom/ID du plugin, mise en page des paramètres, métadonnées (icônes) si disponibles.
