using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using WinCry.Memory;
using WinCry.Models;
using WinCry.Services;
using WinCry.Tweaks;

namespace WinCry.Settings
{
    class SettingsPreset : IPreset
    {
        #region Public Properties
        public string Name { get; set; }
        public Setting Settings { get; set; }
        public ServicesPreset ServicesPreset { get; set; }
        public TweaksPreset TweaksPreset { get; set; }
        public MemoryData MemoryData { get; set; }

        #endregion

        #region Constructor

        public SettingsPreset() { }

        public SettingsPreset(byte[] resource)
        {
            Load(resource);
        }

        public SettingsPreset(string name)
        {
            Load(name);
        }

        #endregion

        #region Functions

        public void Load(string name)
        {
            string fileName = $@"{StringConsts.SettingsPresetsFolder}{name}.json";

            if (File.Exists(fileName))
            {
                Load(File.ReadAllBytes(fileName));
            }
            else
            {
                Name = "Preset";
                Settings = new Setting();
                ServicesPreset = new ServicesPreset();
                TweaksPreset = new TweaksPreset();
                MemoryData = new MemoryData();
            }
        }

        public void Load(byte[] byteArray)
        {
            try
            {
                Stream initialStream = new MemoryStream(byteArray);

                using (StreamReader sr = new StreamReader(initialStream))
                {
                    string jsonString = sr.ReadToEnd();

                    SettingsPreset loadedPreset = JsonSerializer.Deserialize<SettingsPreset>(jsonString);

                    Name = loadedPreset.Name;
                    Settings = loadedPreset.Settings;
                    ServicesPreset = loadedPreset.ServicesPreset;
                    TweaksPreset = loadedPreset.TweaksPreset;
                    MemoryData = loadedPreset.MemoryData;
                }
            }
            catch
            {
                Name = "Кривой пресет";
                Settings = new Setting();
                ServicesPreset = new ServicesPreset() { Name = "Кривой пресет" };
                TweaksPreset = new TweaksPreset { Name = "Кривой пресет" };
                MemoryData = new MemoryData();
            }
        }

        /// <summary>
        /// Saves current preset as JSON file 
        /// </summary>
        /// <param name="name">Preset name</param>
        public void Save()
        {
            var options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            if (!Directory.Exists(StringConsts.SettingsPresetsFolder))
                Directory.CreateDirectory(StringConsts.SettingsPresetsFolder);

            File.WriteAllText($@"{StringConsts.SettingsPresetsFolder}{Name}.json", JsonSerializer.Serialize(this, options));
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is SettingsPreset))
                return false;

            return (((SettingsPreset)obj).Name == this.Name);
        }
    }
}