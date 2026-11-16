using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RecompLauncherLibrary
{
    public static class DataLocator
    {
        /// <summary>
        /// Where RecompLauncher keeps library.json: beside the exe in portable mode
        /// (a "portable" or "portable.txt" file next to it), else in %APPDATA%.
        /// </summary>
        public static string LibraryJsonPath(string recompLauncherExe, string appData)
        {
            var exeDir = string.IsNullOrEmpty(recompLauncherExe) ? null : Path.GetDirectoryName(recompLauncherExe);
            if (exeDir != null
                && (File.Exists(Path.Combine(exeDir, "portable")) || File.Exists(Path.Combine(exeDir, "portable.txt"))))
                return Path.Combine(exeDir, "data", "library.json");
            return Path.Combine(appData, "com.recomplauncher.app", "library.json");
        }

        /// <summary>RecompLauncher's cached catalog, in the cache folder beside library.json.</summary>
        public static string CatalogPath(string libraryJsonPath) =>
            Path.Combine(Path.GetDirectoryName(libraryJsonPath), "cache", "pc-catalog.json");

        public static string FindExe(IEnumerable<string> candidates) =>
            candidates.FirstOrDefault(File.Exists);

        public static IEnumerable<string> DefaultExeCandidates()
        {
            // The first one is the real per-user install path (read from the Start/Desktop shortcut).
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RecompLauncher", "recomplauncher.exe");
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RecompLauncher", "recomplauncher.exe");
        }
    }
}
