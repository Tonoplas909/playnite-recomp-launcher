using System.Linq;
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
            var m = GameMapper.ToMetadata(Entry());
            Assert.Equal("recompia", m.GameId);
            Assert.Equal("Recompia", m.Name);
            Assert.True(m.IsInstalled);
            Assert.Equal(@"C:\G\recompia", m.InstallDirectory);
            Assert.Equal("1.0.0", m.Version);
        }

        [Fact]
        public void ToMetadata_VersionEmpty_UsesTag() =>
            Assert.Equal("v1", GameMapper.ToMetadata(Entry(version: "")).Version);

        [Fact]
        public void ToMetadata_NameEmpty_UsesId() =>
            Assert.Equal("recompia", GameMapper.ToMetadata(Entry(name: "")).Name);

        [Fact]
        public void ToMetadata_HasNoFrozenGameActions() =>
            // Launching goes through the plugin's GetPlayActions so the exe path is never frozen into the game.
            Assert.True(GameMapper.ToMetadata(Entry()).GameActions == null || !GameMapper.ToMetadata(Entry()).GameActions.Any());

        [Fact]
        public void PlayArguments_Plain() =>
            Assert.Equal("--play \"recompia\"", GameMapper.PlayArguments("recompia"));

        [Fact]
        public void PlayArguments_IdWithSpaceOrQuote_IsQuoted() =>
            Assert.Equal("--play \"a b\\\"c\"", GameMapper.PlayArguments("a b\"c"));

        [Fact]
        public void PlayArguments_IdEndingWithBackslash_DoublesItBeforeClosingQuote() =>
            Assert.Equal("--play \"a\\\\\"", GameMapper.PlayArguments("a\\"));
    }
}
