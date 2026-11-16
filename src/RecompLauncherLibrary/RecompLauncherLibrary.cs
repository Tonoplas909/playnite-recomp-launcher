using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace RecompLauncherLibrary
{
    public class RecompLauncherLibrary : LibraryPlugin
    {
        private readonly RecompLauncherSettingsViewModel settings;
        private bool exeMissingNotified;

        public override Guid Id { get; } = Guid.Parse("7b1f6f3a-5d2e-4c8e-9a41-2f0c8d6e3b17");
        public override string Name => "RecompLauncher";

        public RecompLauncherLibrary(IPlayniteAPI api) : base(api)
        {
            settings = new RecompLauncherSettingsViewModel(this);
            Properties = new LibraryPluginProperties { HasSettings = true };
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView(PlayniteApi);

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var none = new List<GameMetadata>();

            var exe = ResolveExe();
            if (exe == null)
            {
                if (!exeMissingNotified)
                {
                    exeMissingNotified = true;
                    PlayniteApi.Notifications.Add("RecompLauncher-exe-missing",
                        "RecompLauncher introuvable : indique le chemin de recomplauncher.exe dans les paramètres de l'extension.",
                        NotificationType.Error);
                }
                return none; // no removal when we can't read the source
            }

            var libraryPath = DataLocator.LibraryJsonPath(exe, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            var entries = LibraryReader.Read(libraryPath);
            if (entries == null) return none; // unreadable library.json: change nothing

            // Optional: original game name + platform, so Playnite's metadata providers can match the game.
            var catalog = CatalogReader.Read(DataLocator.CatalogPath(libraryPath));

            var existing = PlayniteApi.Database.Games.Where(g => g.PluginId == Id).ToList();

            var stale = new HashSet<string>(StaleGames.Find(existing.Select(g => g.GameId), entries));
            if (stale.Count > 0)
            {
                PlayniteApi.Database.Games.Remove(existing.Where(g => stale.Contains(g.GameId)).ToList());
                existing = existing.Where(g => !stale.Contains(g.GameId)).ToList();
            }

            // Playnite does not refresh the Version of an already imported game.
            var versions = existing.Where(g => g.GameId != null).GroupBy(g => g.GameId).ToDictionary(g => g.Key, g => g.First().Version ?? "");
            foreach (var change in VersionUpdates.Find(versions, entries))
            {
                foreach (var game in existing.Where(g => g.GameId == change.Key))
                {
                    game.Version = change.Value;
                    PlayniteApi.Database.Games.Update(game);
                }
            }

            return entries.Select(e => GameMapper.ToMetadata(e, catalog.TryGetValue(e.Id, out var info) ? info : null)).ToList();
        }

        // Launch goes through here (not through GameActions stored in the database) so that a
        // moved or re-pointed recomplauncher.exe applies to games that are already imported.
        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (args.Game.PluginId != Id) yield break;
            var exe = ResolveExe();
            if (exe == null) yield break;
            yield return new AutomaticPlayController(args.Game)
            {
                Name = "Play",
                Type = AutomaticPlayActionType.File,
                Path = exe,
                Arguments = GameMapper.PlayArguments(args.Game.GameId),
                WorkingDir = Path.GetDirectoryName(exe),
            };
        }

        private string ResolveExe()
        {
            var configured = settings.Settings.ExePath;
            if (configured.Length > 0 && File.Exists(configured)) return configured;
            return DataLocator.FindExe(DataLocator.DefaultExeCandidates());
        }
    }
}
