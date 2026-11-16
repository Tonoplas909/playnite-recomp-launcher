using Xunit;

namespace RecompLauncherLibrary.Tests
{
    public class PlatformSpecsTests
    {
        // RecompLauncher catalog `origin` -> Playnite platform specification id (Emulation/Platforms.yaml).
        [Theory]
        [InlineData("n64", "nintendo_64")]
        [InlineData("x360", "xbox360")]
        [InlineData("xbox", "xbox")]
        [InlineData("ps1", "sony_playstation")]
        [InlineData("ps2", "sony_playstation2")]
        [InlineData("ps3", "sony_playstation3")]
        [InlineData("ps4", "sony_playstation4")]
        [InlineData("ps5", "sony_playstation5")]
        [InlineData("gc", "nintendo_gamecube")]
        [InlineData("wii", "nintendo_wii")]
        [InlineData("wiiu", "nintendo_wiiu")]
        [InlineData("snes", "nintendo_super_nes")]
        [InlineData("gba", "nintendo_gameboyadvance")]
        [InlineData("gb", "nintendo_gameboy")]
        [InlineData("gbc", "nintendo_gameboycolor")]
        [InlineData("ds", "nintendo_ds")]
        [InlineData("dc", "sega_dreamcast")]
        [InlineData("saturn", "sega_saturn")]
        [InlineData("psp", "sony_psp")]
        [InlineData("switch", "nintendo_switch")]
        [InlineData("switch2", "nintendo_switch2")]
        [InlineData("nes", "nintendo_nes")]
        [InlineData("3ds", "nintendo_3ds")]
        [InlineData("vb", "nintendo_virtualboy")]
        [InlineData("genesis", "sega_genesis")]
        [InlineData("s32x", "sega_32x")]
        [InlineData("gg", "sega_gamegear")]
        [InlineData("arcade", "arcade")]
        [InlineData("cdi", "philips_cdi")]
        [InlineData("amiga", "commodore_amiga")]
        [InlineData("pc", "pc_windows")]
        [InlineData("SWITCH", "nintendo_switch")]
        public void FromOrigin_KnownOrigin_ReturnsSpecId(string origin, string spec) =>
            Assert.Equal(spec, PlatformSpecs.FromOrigin(origin));

        [Theory]
        [InlineData("other")]
        [InlineData("something-new")]
        [InlineData("")]
        [InlineData(null)]
        public void FromOrigin_UnknownOrEmpty_ReturnsNull(string origin) =>
            Assert.Null(PlatformSpecs.FromOrigin(origin));
    }
}
