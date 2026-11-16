using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class VersionUpdatesTests
    {
        private static List<RecompEntry> Current(params (string id, string version, string tag)[] items) =>
            items.Select(i => new RecompEntry { Id = i.id, Version = i.version, Tag = i.tag }).ToList();

        private static Dictionary<string, string> Existing(params (string id, string version)[] items) =>
            items.ToDictionary(i => i.id, i => i.version);

        [Fact]
        public void Find_VersionChanged_ReturnsNewVersion()
        {
            var r = VersionUpdates.Find(Existing(("a", "1.0")), Current(("a", "2.0", "v2")));
            Assert.Single(r);
            Assert.Equal("2.0", r["a"]);
        }

        [Fact]
        public void Find_Unchanged_ReturnsEmpty() =>
            Assert.Empty(VersionUpdates.Find(Existing(("a", "1.0")), Current(("a", "1.0", "v1"))));

        [Fact]
        public void Find_VersionEmpty_FallsBackToTag() =>
            Assert.Equal("nightly-2", VersionUpdates.Find(Existing(("a", "nightly-1")), Current(("a", "", "nightly-2")))["a"]);

        [Fact]
        public void Find_GameNotInCurrent_IsIgnored() =>
            Assert.Empty(VersionUpdates.Find(Existing(("gone", "1.0")), Current(("a", "2.0", "v2"))));

        [Fact]
        public void Find_NullCurrent_ReturnsEmpty() =>
            Assert.Empty(VersionUpdates.Find(Existing(("a", "1.0")), null));
    }
}
