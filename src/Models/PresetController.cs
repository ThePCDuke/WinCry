using System.Collections.ObjectModel;
using System.IO;
using WinCry.Services;
using WinCry.Settings;
using WinCry.Tweaks;

namespace WinCry.Models
{
    internal class PresetController
    {
        /// <summary>
        /// Gets list of saved user settings presets
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<SettingsPreset> LoadSettingsPresetsList()
        {
            ObservableCollection<SettingsPreset> presets = new ObservableCollection<SettingsPreset>()
            {
                new SettingsPreset(Properties.Resources.SettingsBasic)
            };

            if (!Directory.Exists(StringConsts.SettingsPresetsFolder))
            {
                return presets;
            }

            foreach (string presetName in Directory.GetFiles(StringConsts.SettingsPresetsFolder))
            {
                if (Path.GetExtension(presetName) == ".json")
                {
                    presets.Add(new SettingsPreset(Path.GetFileNameWithoutExtension(presetName)));
                }
            }

            return presets;
        }

        /// <summary>
        /// Gets list of saved user services presets
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<ServicesPreset> LoadServicesPresetsList()
        {
            ObservableCollection<ServicesPreset> presets = new ObservableCollection<ServicesPreset>();

            foreach (SettingsPreset settingsPreset in LoadSettingsPresetsList())
            {
                presets.Add(settingsPreset.ServicesPreset);
            }

            if (!Directory.Exists(StringConsts.ServicesPresetsFolder))
            {
                return presets;
            }

            foreach (string presetName in Directory.GetFiles(StringConsts.ServicesPresetsFolder))
            {
                if (Path.GetExtension(presetName) == ".json")
                {
                    presets.Add(new ServicesPreset(Path.GetFileNameWithoutExtension(presetName)));
                }
            }

            return presets;
        }

        /// <summary>
        /// Gets list of saved user tweaks presets
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<TweaksPreset> LoadTweaksPresetsList()
        {
            ObservableCollection<TweaksPreset> presets = new ObservableCollection<TweaksPreset>();

            foreach (SettingsPreset settingsPreset in LoadSettingsPresetsList())
            {
                presets.Add(settingsPreset.TweaksPreset);
            }

            if (!Directory.Exists(StringConsts.TweaksPresetsFolder))
            {
                return presets;
            }

            foreach (string _presetName in Directory.GetFiles(StringConsts.TweaksPresetsFolder))
            {
                if (Path.GetExtension(_presetName) == ".json")
                {
                    presets.Add(new TweaksPreset(Path.GetFileNameWithoutExtension(_presetName)));
                }
            }

            return presets;
        }
    }
}
