using System.Collections.Generic;

namespace RecompLauncherLibrary
{
    public static class VersionUpdates
    {
        /// <summary>
        /// For games Playnite already has: game id -> new version, when RecompLauncher's version
        /// differs. Playnite does not refresh Version on re-import, so the plugin does it itself.
        /// </summary>
        public static IReadOnlyDictionary<string, string> Find(IReadOnlyDictionary<string, string> existingVersionsByGameId, IReadOnlyList<RecompEntry> current)
        {
            var changes = new Dictionary<string, string>();
            if (current == null) return changes;
            foreach (var e in current)
            {
                var v = GameMapper.VersionOf(e);
                if (existingVersionsByGameId.TryGetValue(e.Id, out var old) && old != v)
                    changes[e.Id] = v;
            }
            return changes;
        }
    }
}
