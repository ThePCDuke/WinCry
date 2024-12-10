using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using WinCry.Models;

namespace WinCry.Services
{
    class ServicesPreset : IPreset
    {
        #region Constructor

        public ServicesPreset() { }

        public ServicesPreset(byte[] resource)
        {
            Load(resource);
        }

        public ServicesPreset(string name)
        {
            Load(name);
        }

        #endregion

        #region Public Properties
        public string Name { get; set; }
        public ServicesOption Option { get; set; } = ServicesOption.Nothing;
        public ObservableCollection<Service> Services { get; set; }

        #endregion

        #region Functions

        public void Load(byte[] resource)
        {
            try
            {
                Stream initialStream = new MemoryStream(resource);

                using (StreamReader sr = new StreamReader(initialStream))
                {
                    string jsonString = sr.ReadToEnd();

                    ServicesPreset loadedPreset = JsonSerializer.Deserialize<ServicesPreset>(jsonString);

                    Name = loadedPreset.Name;
                    Option = loadedPreset.Option;
                    Services = loadedPreset.Services;
                }
            }
            catch
            {
                Option = ServicesOption.Nothing;
                Services = new ObservableCollection<Service>();
            }
        }

        public void Load(string name)
        {
            string fileName = $@"{StringConsts.ServicesPresetsFolder}{name}.json";

            if (File.Exists(fileName))
            {
                Load(File.ReadAllBytes(fileName));
            }
            else
            {
                Option = ServicesOption.Nothing;
                Services = new ObservableCollection<Service>();
            }
        }

        public void Save()
        {
            var options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            if (!Directory.Exists(StringConsts.ServicesPresetsFolder))
                Directory.CreateDirectory(StringConsts.ServicesPresetsFolder);

            File.WriteAllText($@"{StringConsts.ServicesPresetsFolder}{Name}.json", JsonSerializer.Serialize(this, options));
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ServicesPreset))
                return false;

            return (((ServicesPreset)obj).Name == this.Name);
        }
    }
}