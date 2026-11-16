using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RecompLauncherLibrary
{
    public sealed class RecompEntry
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";
        public string Version { get; set; } = "";
        public string Path { get; set; } = "";
        public string Exe { get; set; }
    }

    public static class LibraryReader
    {
        /// <summary>
        /// Entries of RecompLauncher's library.json, or null when the file is missing, unreadable
        /// (locked, malformed) or is not a library file. An empty list means nothing is installed.
        /// </summary>
        public static IReadOnlyList<RecompEntry> Read(string libraryJsonPath)
        {
            JObject root;
            try
            {
                using (var fs = new FileStream(libraryJsonPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                    root = JObject.Parse(sr.ReadToEnd());
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
            {
                return null;
            }

            var installed = root["installed"] as JObject;
            if (installed == null) return null;

            var list = new List<RecompEntry>();
            foreach (var prop in installed.Properties())
            {
                var o = prop.Value as JObject;
                if (o == null) continue;
                var id = Str(o, "id");
                list.Add(new RecompEntry
                {
                    Id = id.Length > 0 ? id : prop.Name,
                    Name = Str(o, "name"),
                    Tag = Str(o, "tag"),
                    Version = Str(o, "version"),
                    Path = Str(o, "path"),
                    Exe = o["exe"]?.Type == JTokenType.String ? (string)o["exe"] : null,
                });
            }
            return list;
        }

        private static string Str(JObject o, string name) =>
            o[name]?.Type == JTokenType.String ? (string)o[name] : "";
    }
}
