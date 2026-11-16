using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class StaleGamesTests
    {
        private static List<RecompEntry> Current(params string[] ids) =>
            ids.Select(i => new RecompEntry { Id = i }).ToList();

        [Fact]
        public void Find_IdMissingFromCurrent_IsStale() =>
            Assert.Equal(new[] { "b" }, StaleGames.Find(new[] { "a", "b" }, Current("a")));

        [Fact]
        public void Find_AllPresent_ReturnsEmpty() =>
            Assert.Empty(StaleGames.Find(new[] { "a", "b" }, Current("a", "b")));

        [Fact]
        public void Find_NullCurrent_ReturnsEmpty() =>
            Assert.Empty(StaleGames.Find(new[] { "a", "b" }, null));

        [Fact]
        public void Find_EmptyCurrent_ReturnsAllExisting() =>
            Assert.Equal(new[] { "a", "b" }, StaleGames.Find(new[] { "a", "b" }, Current()));
    }
}
