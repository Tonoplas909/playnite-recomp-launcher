using System.IO;
using System.Linq;
using Playnite.SDK.Models;
using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class GameMapperTests
    {
        private const string Exe = @"C:\RL\recomplauncher.exe";

        private static RecompEntry Entry(string id = "recompia", string name = "Recompia", string tag = "v1", string version = "1.0.0") =>
            new RecompEntry { Id = id, Name = name, Tag = tag, Version = version, Path = @"C:\G\recompia", Exe = "recompia.exe" };

        [Fact]
        public void ToMetadata_MapsFields()
        {
            var m = GameMapper.ToMetadata(Entry(), Exe);
            Assert.Equal("recompia", m.GameId);
            Assert.Equal("Recompia", m.Name);
            Assert.True(m.IsInstalled);
            Assert.Equal(@"C:\G\recompia", m.InstallDirectory);
            Assert.Equal("1.0.0", m.Version);
        }

        [Fact]
        public void ToMetadata_VersionEmpty_UsesTag() =>
            Assert.Equal("v1", GameMapper.ToMetadata(Entry(version: ""), Exe).Version);

        [Fact]
        public void ToMetadata_NameEmpty_UsesId() =>
            Assert.Equal("recompia", GameMapper.ToMetadata(Entry(name: ""), Exe).Name);

        [Fact]
        public void ToMetadata_PlayAction()
        {
            var a = Assert.Single(GameMapper.ToMetadata(Entry(), Exe).GameActions);
            Assert.Equal(GameActionType.File, a.Type);
            Assert.Equal(Exe, a.Path);
            Assert.Equal("--play \"recompia\"", a.Arguments);
            Assert.Equal(Path.GetDirectoryName(Exe), a.WorkingDir);
            Assert.True(a.IsPlayAction);
        }

        [Fact]
        public void ToMetadata_IdWithSpaceOrQuote_IsQuoted() =>
            Assert.Equal("--play \"a b\\\"c\"", GameMapper.ToMetadata(Entry(id: "a b\"c"), Exe).GameActions.Single().Arguments);

        [Fact]
        public void ToMetadata_IdEndingWithBackslash_DoublesItBeforeClosingQuote() =>
            Assert.Equal("--play \"a\\\\\"", GameMapper.ToMetadata(Entry(id: "a\\"), Exe).GameActions.Single().Arguments);
    }
}
