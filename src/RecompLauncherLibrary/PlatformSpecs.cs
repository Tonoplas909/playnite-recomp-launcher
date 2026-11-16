using System;
using System.Collections.Generic;

namespace RecompLauncherLibrary
{
    public static class PlatformSpecs
    {
        // RecompLauncher catalog `origin` -> Playnite platform specification id (Playnite's Emulation/Platforms.yaml).
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["n64"] = "nintendo_64", ["x360"] = "xbox360", ["xbox"] = "xbox",
            ["ps1"] = "sony_playstation", ["ps2"] = "sony_playstation2", ["ps3"] = "sony_playstation3",
            ["ps4"] = "sony_playstation4", ["ps5"] = "sony_playstation5",
            ["gc"] = "nintendo_gamecube", ["wii"] = "nintendo_wii", ["wiiu"] = "nintendo_wiiu",
            ["snes"] = "nintendo_super_nes", ["gba"] = "nintendo_gameboyadvance", ["gb"] = "nintendo_gameboy",
            ["gbc"] = "nintendo_gameboycolor", ["ds"] = "nintendo_ds", ["dc"] = "sega_dreamcast",
            ["saturn"] = "sega_saturn", ["psp"] = "sony_psp", ["switch"] = "nintendo_switch",
            ["switch2"] = "nintendo_switch2", ["nes"] = "nintendo_nes", ["3ds"] = "nintendo_3ds",
            ["vb"] = "nintendo_virtualboy", ["genesis"] = "sega_genesis", ["s32x"] = "sega_32x",
            ["gg"] = "sega_gamegear", ["arcade"] = "arcade", ["cdi"] = "philips_cdi",
            ["amiga"] = "commodore_amiga", ["pc"] = "pc_windows",
        };

        /// <summary>Playnite platform spec id for a catalog origin, or null (unknown, "other", empty).</summary>
        public static string FromOrigin(string origin) =>
            origin != null && Map.TryGetValue(origin, out var spec) ? spec : null;
    }
}
