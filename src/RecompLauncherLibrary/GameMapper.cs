using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Models;

namespace RecompLauncherLibrary
{
    public static class GameMapper
    {
        /// <summary>
        /// No GameActions on purpose: Playnite only applies them when a game is first imported, which
        /// would freeze the exe path. Launching is done by the plugin's GetPlayActions instead.
        /// </summary>
        public static GameMetadata ToMetadata(RecompEntry entry, CatalogInfo info = null)
        {
            // The original game's name + platform let Playnite's metadata providers (IGDB) find the game.
            var originalName = info?.GameName ?? "";
            var spec = PlatformSpecs.FromOrigin(info?.Origin);
            return new GameMetadata
            {
                GameId = entry.Id,
                Name = originalName.Length > 0 ? originalName : entry.Name.Length > 0 ? entry.Name : entry.Id,
                Source = new MetadataNameProperty("RecompLauncher"),
                Platforms = spec == null ? null : new HashSet<MetadataProperty> { new MetadataSpecProperty(spec) },
                IsInstalled = true,
                InstallDirectory = entry.Path,
                Version = VersionOf(entry),
            };
        }

        public static string VersionOf(RecompEntry entry) => entry.Version.Length > 0 ? entry.Version : entry.Tag;

        public static string PlayArguments(string gameId) => "--play " + Quote(gameId);

        /// <summary>Windows command-line quoting: backslashes only matter right before a quote.</summary>
        private static string Quote(string s)
        {
            var sb = new StringBuilder("\"");
            var backslashes = 0;
            foreach (var c in s)
            {
                if (c == '\\') { backslashes++; continue; }
                if (c == '"') sb.Append('\\', backslashes * 2 + 1).Append('"');
                else sb.Append('\\', backslashes).Append(c);
                backslashes = 0;
            }
            return sb.Append('\\', backslashes * 2).Append('"').ToString();
        }
    }
}
