using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using WinCry.Models;

namespace WinCry.Tweaks
{
    class TweaksPreset : IPreset
    {
        #region Constructor

        public TweaksPreset() { }

        public TweaksPreset(byte[] resource)
        {
            Load(resource);
        }

        public TweaksPreset(string name)
        {
            Load(name);
        }

        #endregion

        #region Public Properties

        public string Name { get; set; }
        public TweaksOption Option { get; set; }
        public ObservableCollection<Tweak> Tweaks { get; set; }

        #endregion

        #region Functions

        public void Load(byte[] resource)
        {
            Stream initialStream = new MemoryStream(resource);

            using (StreamReader sr = new StreamReader(initialStream))
            {
                string jsonString = sr.ReadToEnd();

                TweaksPreset loadedPreset = JsonSerializer.Deserialize<TweaksPreset>(jsonString);

                Name = loadedPreset.Name;
                Option = loadedPreset.Option;
                Tweaks = loadedPreset.Tweaks;
            }
        }

        public void Load(string name)
        {
            string fileName = $@"{StringConsts.TweaksPresetsFolder}{name}.json";

            if (File.Exists(fileName))
            {
                Load(File.ReadAllBytes(fileName));
            }
            else
            {
                Option = TweaksOption.Nothing;
                Tweaks = new ObservableCollection<Tweak>();
            }
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            if (!Directory.Exists(StringConsts.TweaksPresetsFolder))
                Directory.CreateDirectory(StringConsts.TweaksPresetsFolder);

            File.WriteAllText($@"{StringConsts.TweaksPresetsFolder}{Name}.json", JsonSerializer.Serialize(this, options));
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TweaksPreset))
                return false;

            return (((TweaksPreset)obj).Name == this.Name);
        }
    }
}