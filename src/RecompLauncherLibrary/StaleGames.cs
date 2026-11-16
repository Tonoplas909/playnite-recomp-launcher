using System.Collections.Generic;
using System.Linq;

namespace RecompLauncherLibrary
{
    public static class StaleGames
    {
        /// <summary>
        /// Game ids Playnite has but RecompLauncher no longer lists. A failed read
        /// (current == null) never marks anything stale.
        /// </summary>
        public static IReadOnlyList<string> Find(IEnumerable<string> existingGameIds, IReadOnlyList<RecompEntry> current)
        {
            if (current == null) return new string[0];
            var live = new HashSet<string>(current.Select(e => e.Id));
            return existingGameIds.Where(id => !live.Contains(id)).ToList();
        }
    }
}
