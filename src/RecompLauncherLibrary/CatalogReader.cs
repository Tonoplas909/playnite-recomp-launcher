using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RecompLauncherLibrary
{
    public sealed class CatalogInfo
    {
        /// <summary>The original game this recomp is of (IGDB-style name), "" when unknown.</summary>
        public string GameName { get; set; } = "";
        /// <summary>RecompLauncher's console id for the original game (switch, n64, ...), "" when unknown.</summary>
        public string Origin { get; set; } = "";
        /// <summary>URL of the recomp's small icon (the one RecompLauncher shows), "" when none.</summary>
        public string Icon { get; set; } = "";
    }

    public static class CatalogReader
    {
        /// <summary>
        /// Original-game info by recomp id, from RecompLauncher's cached pc-catalog.json. Optional
        /// enrichment: any failure yields an empty map, never an exception.
        /// </summary>
        public static IReadOnlyDictionary<string, CatalogInfo> Read(string pcCatalogJsonPath)
        {
            var result = new Dictionary<string, CatalogInfo>();
            JObject root;
            try
            {
                using (var fs = new FileStream(pcCatalogJsonPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                using (var jr = new JsonTextReader(sr))
                    root = JObject.Load(jr);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
            {
                return result;
            }

            // The main list, then apps_min: groups [{apps:[...], min:"launcher version"}] holding newer
            // recomps (e.g. Longshot). The main list wins when an id is in both.
            var groups = (root["apps_min"] as JArray)?.OfType<JObject>().Select(g => g["apps"] as JArray) ?? Enumerable.Empty<JArray>();
            var lists = new[] { root["apps"] as JArray }.Concat(groups);
            foreach (var app in lists.Where(l => l != null).SelectMany(l => l.OfType<JObject>()))
            {
                var id = Str(app["id"]);
                if (id.Length == 0 || result.ContainsKey(id)) continue;
                var aboutName = Str((app["about"] as JObject)?["name"]);
                result[id] = new CatalogInfo
                {
                    GameName = aboutName.Length > 0 ? aboutName : Str(app["game"]),
                    Origin = Str(app["origin"]),
                    Icon = Str(app["icon"]),
                };
            }
            return result;
        }

        private static string Str(JToken t) => t?.Type == JTokenType.String ? (string)t : "";
    }
}
