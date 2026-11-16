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

## Ajout : métadonnées IGDB (2026-11-16, demandé par l'utilisateur)
Pour que le fournisseur IGDB intégré à Playnite retrouve le jeu d'origine, le plugin lit le catalogue en cache de RecompLauncher (`cache\pc-catalog.json`, à côté de `library.json`) : nom du jeu d'origine (`about.name`, sinon `game`) et console d'origine (`origin`), traduite en identifiant de plateforme Playnite (`PlatformSpecs`). Les jeux reçoivent aussi la source « RecompLauncher ». Recomp absente du catalogue (ex. Longshot) : nom RecompLauncher conservé, pas de plateforme. Le catalogue est facultatif : toute erreur de lecture = pas d'enrichissement, la synchro continue. Playnite n'applique nom/plateforme qu'à la création d'un jeu : un jeu déjà importé doit être supprimé de Playnite puis réimporté.

## Ajout : icône et catalogue `apps_min` (2026-11-16, demandé par l'utilisateur)
L'icône du catalogue (`icon`, URL) devient l'icône Playnite du jeu (IGDB ne fournit pas d'icône). Le catalogue a deux listes : `apps` et `apps_min`, qui est une liste de groupes `[{apps:[...], min:"version du lanceur"}]` contenant les recomps récentes (ex. Longshot, absent de `apps`). Les deux sont lues, `apps` l'emporte si un id est dans les deux.

## Ajout : temps de jeu (2026-11-16, demandé par l'utilisateur)
`played_secs` (secondes) et `last_played` (secondes Unix, 0 = jamais) de `library.json` alimentent `Playtime` et `LastActivity` de Playnite à chaque synchro. RecompLauncher est la source de vérité : sa valeur écrase celle de Playnite (une session lancée depuis Playnite via `--play` est comptée par RecompLauncher, donc pas de double comptage).

## Laissé au builder
Détection automatique du chemin, nom/ID du plugin, mise en page des paramètres, métadonnées (icônes) si disponibles.
