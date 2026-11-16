using System;
using System.IO;
using System.Text;
using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class CatalogReaderTests : IDisposable
    {
        private readonly string dir = Path.Combine(Path.GetTempPath(), "rl-cat-" + Guid.NewGuid().ToString("N"));

        public CatalogReaderTests() { Directory.CreateDirectory(dir); }
        public void Dispose() { try { Directory.Delete(dir, true); } catch { } }

        private string Write(string json)
        {
            var p = Path.Combine(dir, "pc-catalog.json");
            File.WriteAllText(p, json, new UTF8Encoding(false));
            return p;
        }

        [Fact]
        public void Read_AboutNameWinsOverGame_GameUsedWhenNoAbout()
        {
            var p = Write(@"{""apps"":[
              {""id"":""totk-pc-recomp"",""name"":""TotK Recomp"",""game"":""Tears of the Kingdom"",""origin"":""switch"",""about"":{""name"":""The Legend of Zelda: Tears of the Kingdom"",""igdb"":119388}},
              {""id"":""other-recomp"",""name"":""Other"",""game"":""Some Game"",""origin"":""n64""}]}");
            var r = CatalogReader.Read(p);
            Assert.Equal(2, r.Count);
            Assert.Equal("The Legend of Zelda: Tears of the Kingdom", r["totk-pc-recomp"].GameName);
            Assert.Equal("switch", r["totk-pc-recomp"].Origin);
            Assert.Equal("Some Game", r["other-recomp"].GameName);
            Assert.Equal("n64", r["other-recomp"].Origin);
        }

        [Fact]
        public void Read_MissingFields_AreEmptyStrings()
        {
            var r = CatalogReader.Read(Write(@"{""apps"":[{""id"":""a""}]}"));
            Assert.Equal("", r["a"].GameName);
            Assert.Equal("", r["a"].Origin);
            Assert.Equal("", r["a"].Icon);
        }

        [Fact]
        public void Read_Icon_IsRead() =>
            Assert.Equal("https://recompstore.com/media/i.png",
                CatalogReader.Read(Write(@"{""apps"":[{""id"":""a"",""icon"":""https://recompstore.com/media/i.png""}]}"))["a"].Icon);

        [Fact]
        public void Read_AppsMinList_IsAlsoRead()
        {
            // Real catalog shape: apps_min is a list of groups [{apps:[...], min:"launcher version"}].
            // Newer recomps (e.g. Longshot) live there, not in apps.
            var r = CatalogReader.Read(Write(@"{""apps"":[{""id"":""a"",""game"":""A""}],
              ""apps_min"":[{""apps"":[{""id"":""longshot"",""game"":""The Legend of Zelda: Ocarina of Time"",""origin"":""switch2"",""icon"":""https://x/i.png""}],""min"":""0.90.0""},
                            {""apps"":[{""id"":""b"",""game"":""B""}],""min"":""0.91.0""}]}"));
            Assert.Equal(3, r.Count);
            Assert.Equal("The Legend of Zelda: Ocarina of Time", r["longshot"].GameName);
            Assert.Equal("switch2", r["longshot"].Origin);
            Assert.Equal("https://x/i.png", r["longshot"].Icon);
        }

        [Fact]
        public void Read_IdInBothLists_AppsWins()
        {
            var r = CatalogReader.Read(Write(@"{""apps"":[{""id"":""a"",""game"":""From apps""}],""apps_min"":[{""apps"":[{""id"":""a"",""game"":""From min""}]}]}"));
            Assert.Equal("From apps", Assert.Single(r).Value.GameName);
        }

        [Fact]
        public void Read_MalformedAppsMin_IsIgnored()
        {
            // not a list, a list of non-objects, a group without apps: the main list still reads.
            foreach (var bad in new[] { @"{""apps"":[]}", "[1,2]", @"[{""min"":""0.9""}]", @"[{""apps"":{}}]" })
            {
                var r = CatalogReader.Read(Write(@"{""apps"":[{""id"":""a""}],""apps_min"":" + bad + "}"));
                Assert.Single(r);
            }
        }

        [Fact]
        public void Read_AppWithoutId_IsSkipped() =>
            Assert.Empty(CatalogReader.Read(Write(@"{""apps"":[{""game"":""x""},{""id"":"""",""game"":""y""}]}")));

        [Fact]
        public void Read_MissingFile_ReturnsEmptyNotNull()
        {
            var r = CatalogReader.Read(Path.Combine(dir, "nope.json"));
            Assert.NotNull(r);
            Assert.Empty(r);
        }

        [Fact]
        public void Read_MalformedJson_ReturnsEmpty() => Assert.Empty(CatalogReader.Read(Write("{broken")));

        [Fact]
        public void Read_NoAppsArray_ReturnsEmpty() => Assert.Empty(CatalogReader.Read(Write(@"{""apps"":{}}")));
    }
}
