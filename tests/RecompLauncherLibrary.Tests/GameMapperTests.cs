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
        public void ToMetadata_WithCatalogInfo_UsesOriginalGameNameAndPlatform()
        {
            var info = new CatalogInfo { GameName = "The Legend of Zelda: Tears of the Kingdom", Origin = "switch" };
            var m = GameMapper.ToMetadata(Entry(name: "TotK Recomp"), info);
            Assert.Equal("The Legend of Zelda: Tears of the Kingdom", m.Name);
            var platform = Assert.Single(m.Platforms);
            Assert.Equal("nintendo_switch", Assert.IsType<MetadataSpecProperty>(platform).Id);
        }

        [Fact]
        public void ToMetadata_NoCatalogInfo_KeepsEntryNameAndNoPlatform()
        {
            var m = GameMapper.ToMetadata(Entry(name: "Longshot"), null);
            Assert.Equal("Longshot", m.Name);
            Assert.True(m.Platforms == null || m.Platforms.Count == 0);
        }

        [Fact]
        public void ToMetadata_UnmappedOriginOrEmptyGameName_NoPlatformAndEntryName()
        {
            var m = GameMapper.ToMetadata(Entry(name: "Thing"), new CatalogInfo { GameName = "", Origin = "other" });
            Assert.Equal("Thing", m.Name);
            Assert.True(m.Platforms == null || m.Platforms.Count == 0);
        }

        [Fact]
        public void ToMetadata_MapsPlaytimeAndLastActivity()
        {
            var e = Entry();
            e.PlayedSecs = 3600;
            e.LastPlayed = 1794861505;
            var m = GameMapper.ToMetadata(e);
            Assert.Equal(3600UL, m.Playtime);
            Assert.Equal(System.DateTimeOffset.FromUnixTimeSeconds(1794861505).LocalDateTime, m.LastActivity);
        }

        [Fact]
        public void ToMetadata_AbsurdLastPlayed_NoLastActivityAndNoException()
        {
            var e = Entry();
            e.LastPlayed = ulong.MaxValue;
            Assert.Null(GameMapper.ToMetadata(e).LastActivity);
        }

        [Fact]
        public void ToMetadata_NeverPlayed_NoLastActivity()
        {
            var m = GameMapper.ToMetadata(Entry());
            Assert.Equal(0UL, m.Playtime);
            Assert.Null(m.LastActivity);
        }

        [Fact]
        public void ToMetadata_AlwaysSetsRecompLauncherSource() =>
            Assert.Equal("RecompLauncher", Assert.IsType<MetadataNameProperty>(GameMapper.ToMetadata(Entry()).Source).Name);

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
