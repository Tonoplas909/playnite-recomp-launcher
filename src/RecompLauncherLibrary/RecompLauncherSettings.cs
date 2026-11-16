using System.Collections.Generic;
using System.IO;
using Playnite.SDK;
using Playnite.SDK.Data;

namespace RecompLauncherLibrary
{
    public class RecompLauncherSettings : ObservableObject
    {
        private string exePath = "";

        /// <summary>Path to recomplauncher.exe. Empty = auto-detect.</summary>
        public string ExePath { get => exePath; set => SetValue(ref exePath, value ?? ""); }
    }

    public class RecompLauncherSettingsViewModel : ObservableObject, ISettings
    {
        private readonly RecompLauncherLibrary plugin;
        private RecompLauncherSettings editingClone;
        private RecompLauncherSettings settings;

        public RecompLauncherSettings Settings { get => settings; set => SetValue(ref settings, value); }

        public RecompLauncherSettingsViewModel(RecompLauncherLibrary plugin)
        {
            this.plugin = plugin;
            Settings = plugin.LoadPluginSettings<RecompLauncherSettings>() ?? new RecompLauncherSettings();
        }

        public void BeginEdit() => editingClone = Serialization.GetClone(Settings);

        public void CancelEdit() => Settings = editingClone;

        public void EndEdit() => plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            if (Settings.ExePath.Length > 0 && !File.Exists(Settings.ExePath))
                errors.Add("recomplauncher.exe introuvable : " + Settings.ExePath);
            return errors.Count == 0;
        }
    }
}
