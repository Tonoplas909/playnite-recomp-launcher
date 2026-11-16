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
