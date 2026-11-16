using System.Collections.Generic;
using System.IO;
using System.Text;
using Playnite.SDK.Models;

namespace RecompLauncherLibrary
{
    public static class GameMapper
    {
        public static GameMetadata ToMetadata(RecompEntry entry, string recompLauncherExe) =>
            new GameMetadata
            {
                GameId = entry.Id,
                Name = entry.Name.Length > 0 ? entry.Name : entry.Id,
                IsInstalled = true,
                InstallDirectory = entry.Path,
                Version = entry.Version.Length > 0 ? entry.Version : entry.Tag,
                GameActions = new List<GameAction>
                {
                    new GameAction
                    {
                        Name = "Play",
                        Type = GameActionType.File,
                        Path = recompLauncherExe,
                        Arguments = "--play " + Quote(entry.Id),
                        WorkingDir = Path.GetDirectoryName(recompLauncherExe),
                        IsPlayAction = true,
                    },
                },
            };

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
