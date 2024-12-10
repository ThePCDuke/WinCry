using Microsoft.Win32;
using System.Text.Json.Serialization;

namespace WinCry.Tweaks
{
    public class Tweak
    {
        public bool IsChecked { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string CategoryDescription { get; set; }
        public string KeyLocation { get; set; }

        public RegistryValueKind ValueType { get; set; }
        public string Value { get; set; }

        public string DefaultValue { get; set; }

        [JsonIgnore]
        public string CurrentValue { get; set; }

        public string RequiredWinBuild { get; set; }

        [JsonIgnore]
        public bool IsApplied { get; set; }
    }
}