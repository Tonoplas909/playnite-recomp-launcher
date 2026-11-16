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

            var apps = root["apps"] as JArray;
            if (apps == null) return result;

            foreach (var app in apps.OfType<JObject>())
            {
                var id = Str(app["id"]);
                if (id.Length == 0) continue;
                var aboutName = Str((app["about"] as JObject)?["name"]);
                result[id] = new CatalogInfo
                {
                    GameName = aboutName.Length > 0 ? aboutName : Str(app["game"]),
                    Origin = Str(app["origin"]),
                };
            }
            return result;
        }

        private static string Str(JToken t) => t?.Type == JTokenType.String ? (string)t : "";
    }
}
