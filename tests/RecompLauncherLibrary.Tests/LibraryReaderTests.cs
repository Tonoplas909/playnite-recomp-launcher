using System;
using System.IO;
using System.Text;
using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class LibraryReaderTests : IDisposable
    {
        private readonly string dir = Path.Combine(Path.GetTempPath(), "rl-tests-" + Guid.NewGuid().ToString("N"));

        public LibraryReaderTests() { Directory.CreateDirectory(dir); }
        public void Dispose() { try { Directory.Delete(dir, true); } catch { } }

        private string Write(string json, Encoding enc = null)
        {
            var p = Path.Combine(dir, "library.json");
            File.WriteAllText(p, json, enc ?? new UTF8Encoding(false));
            return p;
        }

        [Fact]
        public void Read_ValidFile_ReturnsEntries()
        {
            var p = Write(@"{""installed"":{
              ""recompia"":{""id"":""recompia"",""name"":""Recompia"",""tag"":""v1.0"",""version"":""1.0.0"",""path"":""C:\\G\\recompia"",""exe"":""recompia.exe""},
              ""zelda64-recomp"":{""id"":""zelda64-recomp"",""name"":""Zelda 64"",""tag"":""v2"",""version"":""2.0"",""path"":""C:\\G\\zelda"",""exe"":""Zelda64Recomp.exe""}}}");
            var r = LibraryReader.Read(p);
            Assert.Equal(2, r.Count);
            var z = Assert.Single(r, e => e.Id == "zelda64-recomp");
            Assert.Equal("Zelda 64", z.Name);
            Assert.Equal("v2", z.Tag);
            Assert.Equal("2.0", z.Version);
            Assert.Equal(@"C:\G\zelda", z.Path);
            Assert.Equal("Zelda64Recomp.exe", z.Exe);
        }

        [Fact]
        public void Read_UnknownFieldsAndMissingFields_StillReads()
        {
            var p = Write(@"{""installed"":{""a"":{""id"":""a"",""name"":""A"",""tag"":""t"",""path"":""C:\\a"",""exe"":null,""future_field"":{""x"":1}}},""settings"":{}}");
            var e = Assert.Single(LibraryReader.Read(p));
            Assert.Null(e.Exe);
            Assert.Equal("", e.Version);
        }

        [Fact]
        public void Read_IdMissingInEntry_FallsBackToKey()
        {
            var p = Write(@"{""installed"":{""keyid"":{""name"":""K"",""tag"":""t"",""path"":""C:\\k""}}}");
            Assert.Equal("keyid", Assert.Single(LibraryReader.Read(p)).Id);
        }

        [Fact]
        public void Read_Utf8Bom_StillReads()
        {
            var p = Write(@"{""installed"":{""a"":{""id"":""a"",""name"":""A"",""tag"":""t"",""path"":""C:\\a""}}}", new UTF8Encoding(true));
            Assert.Single(LibraryReader.Read(p));
        }

        [Fact]
        public void Read_MissingFile_ReturnsNull() => Assert.Null(LibraryReader.Read(Path.Combine(dir, "nope.json")));

        [Fact]
        public void Read_MalformedJson_ReturnsNull() => Assert.Null(LibraryReader.Read(Write("{broken")));

        [Fact]
        public void Read_NoInstalledProperty_ReturnsNull() => Assert.Null(LibraryReader.Read(Write("{}")));

        [Fact]
        public void Read_InstalledNotAnObject_ReturnsNull() => Assert.Null(LibraryReader.Read(Write(@"{""installed"":[1,2]}")));

        [Fact]
        public void Read_LockedFile_ReturnsNull()
        {
            var p = Write(@"{""installed"":{}}");
            using (new FileStream(p, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Assert.Null(LibraryReader.Read(p));
        }

        [Fact]
        public void Read_EmptyInstalled_ReturnsEmptyListNotNull()
        {
            var r = LibraryReader.Read(Write(@"{""installed"":{}}"));
            Assert.NotNull(r);
            Assert.Empty(r);
        }
    }
}
