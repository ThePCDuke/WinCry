using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Xml.Serialization;

namespace WinCry.Models
{
    class Helpers
    {
        /// <summary>
        /// Extracts embedded or any <paramref name="resource"/> to certain <paramref name="path"/> on disk
        /// </summary>
        /// <param name="resource">Resource to extract</param>
        /// <param name="fileName">Name of output file</param>
        /// <param name="path">Extraction path. If null -> TEMP</param>
        public static void ExtractEmbedFile(byte[] resource, string fileName, string path = null)
        {
            string _path;
            if (path == null)
                _path = Path.GetTempPath() + fileName;
            else
                _path = Path.Combine(path, fileName);

            File.WriteAllBytes(_path, resource);
        }

        public static void RunByCMD(string command, bool waitForExit = true)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $@"/c {command}"
            };
            process.StartInfo = startInfo;
            process.Start();

            if (waitForExit)
                process.WaitForExit();
        }

        /// <summary>
        /// Gets windows version from registry entry
        /// </summary>
        /// <returns></returns>
        public static int GetWinBuild()
        {
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                int _versionBuild = Convert.ToInt32(_registryKey.GetValue("CurrentBuild").ToString());
                return _versionBuild;
            }
        }

        /// <summary>
        /// Unzips archive from byte array
        /// </summary>
        /// <param name="data">Data to extract from</param>
        /// <param name="extractionDirectory">Directory to extract to</param>
        public static void UnzipFromByteArray(byte[] data, string extractionDirectory)
        {
            if (!Directory.Exists(extractionDirectory))
                Directory.CreateDirectory(extractionDirectory);

            Stream _initialStream = new MemoryStream(data);

            using (ZipArchive _archive = new ZipArchive(_initialStream))
            {
                foreach (ZipArchiveEntry _entry in _archive.Entries)
                {
                    if (_entry.Name == "")
                    {
                        Directory.CreateDirectory(Path.Combine(extractionDirectory, _entry.FullName));
                        continue;
                    }

                    using (var _outputFiCompressionLevel = File.Create(Path.Combine(extractionDirectory, _entry.FullName)))
                    {
                        Stream _stream = _entry.Open();
                        _stream.CopyTo(_outputFiCompressionLevel);
                    }
                }
            }
        }

        /// <summary>
        /// Unzips archive from file
        /// </summary>
        /// <param name="filePath">Full path to archive</param>
        /// <param name="extractionDirectory">Directory to extract to</param>
        public static void UnzipFromFile(string filePath, string extractionDirectory)
        {
            if (!Directory.Exists(extractionDirectory))
                Directory.CreateDirectory(extractionDirectory);

            using (ZipArchive _archive = ZipFile.OpenRead(filePath))
            {
                foreach (ZipArchiveEntry _entry in _archive.Entries)
                {
                    _entry.ExtractToFile(Path.Combine(extractionDirectory, _entry.FullName), true);
                }
            }
        }

        /// <summary>
        /// Deserializes collection from XML file
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="stream">Byte stream of file to deserialize from</param>
        /// <param name="collection">Collection to deserialize to</param>
        public static void DeserializeCollectionAsXML<T>(Byte[] stream, out ObservableCollection<T> collection)
        {
            XmlSerializer _serializer = new XmlSerializer(typeof(ObservableCollection<T>));
            using (Stream _stream = new MemoryStream(stream))
            using (StreamReader _reader = new StreamReader(_stream))
            {
                collection = _serializer.Deserialize(_reader) as ObservableCollection<T>;
            }
        }

        /// <summary>
        /// Gets collection of all WinCry compatible GPUs to tweak
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<string> GetInstalledGPUManufacturers()
        {
            ObservableCollection<string> _installed = new ObservableCollection<string>();

            if (IsNVIDIAGPUInstalled)
                _installed.Add("NVIDIA");

            if (IsAMDGPUInstalled)
                _installed.Add("AMD");

            return _installed;
        }

        /// <summary>
        /// Finds value in Win32_DisplayConfiguration. Basically for GPU detection
        /// </summary>
        /// <param name="value">Value to search for ('AMD', 'NVIDIA', etc.)</param>
        /// <returns></returns>
        private static bool FindValueInDisplayConfiguration(string value)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["VideoProcessor"] == null)
                            return false;

                        if (obj["VideoProcessor"].ToString().ToUpper().Contains(value.ToUpper()) || obj["Name"].ToString().ToUpper().Contains(value.ToUpper()))
                            return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if AMD GPU is installed
        /// </summary>
        public static bool IsAMDGPUInstalled
        {
            get { return FindValueInDisplayConfiguration("AMD"); }
        }

        /// <summary>
        /// Checks if NVIDIA GPU is installed
        /// </summary>
        public static bool IsNVIDIAGPUInstalled
        {
            get 
            { 
                if (!Environment.Is64BitOperatingSystem)
                    return false;

                return FindValueInDisplayConfiguration("NVIDIA"); 
            }
        }
    }
}