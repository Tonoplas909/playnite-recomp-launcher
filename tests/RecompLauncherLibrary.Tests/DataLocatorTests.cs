using System;
using System.IO;
using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class DataLocatorTests : IDisposable
    {
        private readonly string dir = Path.Combine(Path.GetTempPath(), "rl-loc-" + Guid.NewGuid().ToString("N"));
        private readonly string appData;
        private readonly string exe;

        public DataLocatorTests()
        {
            Directory.CreateDirectory(dir);
            appData = Path.Combine(dir, "AppData");
            exe = Path.Combine(dir, "RL", "recomplauncher.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(exe));
            File.WriteAllText(exe, "");
        }

        public void Dispose() { try { Directory.Delete(dir, true); } catch { } }

        [Fact]
        public void LibraryJsonPath_Normal_UsesAppData() =>
            Assert.Equal(Path.Combine(appData, "com.recomplauncher.app", "library.json"), DataLocator.LibraryJsonPath(exe, appData));

        [Fact]
        public void LibraryJsonPath_PortableFile_UsesExeDataFolder()
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(exe), "portable"), "");
            Assert.Equal(Path.Combine(Path.GetDirectoryName(exe), "data", "library.json"), DataLocator.LibraryJsonPath(exe, appData));
        }

        [Fact]
        public void LibraryJsonPath_PortableTxt_UsesExeDataFolder()
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(exe), "portable.txt"), "");
            Assert.Equal(Path.Combine(Path.GetDirectoryName(exe), "data", "library.json"), DataLocator.LibraryJsonPath(exe, appData));
        }

        [Fact]
        public void LibraryJsonPath_EmptyExe_UsesAppData()
        {
            var expected = Path.Combine(appData, "com.recomplauncher.app", "library.json");
            Assert.Equal(expected, DataLocator.LibraryJsonPath("", appData));
            Assert.Equal(expected, DataLocator.LibraryJsonPath(null, appData));
        }

        [Fact]
        public void CatalogPath_IsCacheFolderBesideLibraryJson()
        {
            var lib = Path.Combine(appData, "com.recomplauncher.app", "library.json");
            Assert.Equal(Path.Combine(appData, "com.recomplauncher.app", "cache", "pc-catalog.json"), DataLocator.CatalogPath(lib));
        }

        [Fact]
        public void FindExe_ReturnsFirstExisting() =>
            Assert.Equal(exe, DataLocator.FindExe(new[] { Path.Combine(dir, "missing.exe"), exe, Path.Combine(dir, "other.exe") }));

        [Fact]
        public void FindExe_NoneExist_ReturnsNull() =>
            Assert.Null(DataLocator.FindExe(new[] { Path.Combine(dir, "a.exe"), Path.Combine(dir, "b.exe") }));
    }
}
