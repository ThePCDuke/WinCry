namespace WinCry.Models
{
    interface IPreset
    {
        /// <summary>
        /// Name of the preset.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Loads preset from certaing folder.
        /// </summary>
        /// <param name="presetName">Name of the preset.</param>
        void Load(string presetName);

        /// <summary>
        /// Loads preset from resource.
        /// </summary>
        /// <param name="resource">Array of resources bytes.</param>
        void Load(byte[] resource);

        /// <summary>
        /// Saves preset.
        /// </summary>
        void Save();
    }
}