using System;
using System.Collections.Generic;
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

            var entries = LibraryReader.Read(DataLocator.LibraryJsonPath(exe, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
            if (entries == null) return none; // unreadable library.json: change nothing

            var existing = PlayniteApi.Database.Games.Where(g => g.PluginId == Id).ToList();
            var stale = new HashSet<string>(StaleGames.Find(existing.Select(g => g.GameId), entries));
            if (stale.Count > 0)
                PlayniteApi.Database.Games.Remove(existing.Where(g => stale.Contains(g.GameId)).ToList());

            return entries.Select(e => GameMapper.ToMetadata(e, exe)).ToList();
        }

        private string ResolveExe()
        {
            var configured = settings.Settings.ExePath;
            if (configured.Length > 0 && System.IO.File.Exists(configured)) return configured;
            return DataLocator.FindExe(DataLocator.DefaultExeCandidates());
        }
    }
}
