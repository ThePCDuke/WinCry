using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinCry.Dialogs;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Tweaks
{
    class TweaksModel : DataGridBasedModel
    {
        /// <summary>
        /// Imports given tweak to registry 
        /// </summary>
        /// <param name="tweak">Tweak to import</param>
        public static void Import(Tweak tweak)
        {
            string _output = tweak.Value;

            if (tweak.ValueType == RegistryValueKind.DWord || tweak.ValueType == RegistryValueKind.QWord)
                _output = int.Parse(tweak.Value, System.Globalization.NumberStyles.HexNumber).ToString();

            if (tweak.KeyLocation.EndsWith("*"))
            {
                RegistryKey _currentKey = GetRegistryKeyLocationPath(tweak.KeyLocation);

                foreach (string record in _currentKey.GetSubKeyNames())
                {
                    using (RegistryKey _subRegistryKey = _currentKey.OpenSubKey(record, true))
                    {
                        _subRegistryKey.SetValue(tweak.Key, _output, tweak.ValueType);
                    }
                }

                return;
            }

            Registry.SetValue(tweak.KeyLocation, tweak.Key, _output, tweak.ValueType);
        }

        /// <summary>
        /// Sets default value for given tweak in registry
        /// </summary>
        /// <param name="tweak">Tweak to restore</param>
        public static void Restore(Tweak tweak)
        {
            string _output = tweak.DefaultValue;

            if ((tweak.DefaultValue != null) && (tweak.ValueType == RegistryValueKind.DWord || tweak.ValueType == RegistryValueKind.QWord))
                _output = int.Parse(tweak.DefaultValue, System.Globalization.NumberStyles.HexNumber).ToString();

            if (tweak.KeyLocation.EndsWith("*"))
            {
                RegistryKey _currentKey = GetRegistryKeyLocationPath(tweak.KeyLocation);

                foreach (string record in _currentKey.GetSubKeyNames())
                {
                    using (RegistryKey _subRegistryKey = _currentKey.OpenSubKey(record, true))
                    {
                        if (tweak.DefaultValue == null)
                        {
                            if (_subRegistryKey.GetValue(tweak.Key) != null)
                                _subRegistryKey.DeleteValue(tweak.Key);
                        }

                        else _subRegistryKey.SetValue(tweak.Key, tweak.DefaultValue, tweak.ValueType);
                    }
                }

                return;
            }

            if (tweak.DefaultValue == null)
            {
                RemoveRegistryKey(tweak.KeyLocation, tweak.Key);
            }
            else if (tweak.DefaultValue != null)
                Registry.SetValue(tweak.KeyLocation, tweak.Key, _output, tweak.ValueType);
        }

        /// <summary>
        /// Checks current value for given tweak from registry and updates it
        /// </summary>
        /// <param name="tweak">Tweak to update</param>
        public static void Update(Tweak tweak)
        {
            if (tweak.KeyLocation.EndsWith("*"))
            {
                RegistryKey _currentKey = GetRegistryKeyLocationPath(tweak.KeyLocation);

                string[] _allSubKeys = _currentKey.GetSubKeyNames();
                int _match = 0;
                int _allSubKeysCount = _allSubKeys.Length;

                foreach (string record in _allSubKeys)
                {
                    using (RegistryKey _subRegistryKey = _currentKey.OpenSubKey(record))
                    {
                        if (_subRegistryKey.GetValue(tweak.Key) != null)
                            tweak.CurrentValue = _subRegistryKey.GetValue(tweak.Key).ToString();
                        else tweak.CurrentValue = null;

                        if (tweak.CurrentValue == tweak.Value)
                        {
                            _match += 1;
                        }
                    }
                }

                if (_match == _allSubKeysCount)
                    tweak.IsApplied = true;
                else tweak.IsApplied = false;

                return;
            }

            object _registryValue = Registry.GetValue($@"{tweak.KeyLocation}", $@"{tweak.Key}", null);

            if (_registryValue == null)
            {
                tweak.IsApplied = false;
                tweak.CurrentValue = null;
                return;
            }

            string _registryValueString = _registryValue.ToString();

            if (tweak.ValueType == RegistryValueKind.DWord)
            {
                try
                {
                    _registryValueString = Convert.ToString(int.Parse(_registryValueString), 16);
                }
                catch
                {
                    tweak.IsApplied = false;
                    tweak.CurrentValue = null;
                    return;
                }
            }

            tweak.CurrentValue = _registryValueString;

            if (_registryValueString == tweak.Value)
                tweak.IsApplied = true;
            else tweak.IsApplied = false;
        }

        /// <summary>
        /// Checks current values for given tweaks from registry and updates them
        /// </summary>
        /// <param name="tweaksCollection">Tweaks to update</param>
        public static void Update(ObservableCollection<Tweak> tweaksCollection)
        {
            foreach (Tweak _tweak in tweaksCollection)
            {
                Update(_tweak);
            }
        }

        /// <summary>
        /// Applying given tweak
        /// </summary>
        /// <param name="tweak">Tweak to apply</param>
        /// <param name="option">Applying option</param>
        public static void Apply(Tweak tweak, TweaksOption option)
        {
            switch (option)
            {
                case TweaksOption.Import:
                    {
                        Import(tweak);
                        break;
                    }

                case TweaksOption.Restore:
                    {
                        Restore(tweak);
                        break;
                    }

                case TweaksOption.Update:
                    {
                        Update(tweak);
                        break;
                    }
            }
        }

        /// <summary>
        /// Applies all checked tweaks from <paramref name="tweaksCollection"/>
        /// </summary>
        /// <param name="tweaksCollection">Collection of tweaks to apply</param>
        /// <param name="option">Applying option</param>
        /// <param name="taskViewModel">TaskViewModel for catching results</param>
        /// <returns></returns>
        public static Task Apply(ObservableCollection<Tweak> tweaksCollection, TweaksOption option, TaskViewModel taskViewModel = null)
        {
            if (taskViewModel == null)
                taskViewModel = new TaskViewModel();

            double _totalTweaks = tweaksCollection.Where(t => t.IsChecked).Count();
            int _current = 0;

            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.Tweaks;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                foreach (Tweak _tweak in tweaksCollection.Where(t => t.IsChecked))
                {
                    try
                    {
                        taskViewModel.CreateMessage($"{_tweak.Name} ({_tweak.Category})... ");

                        Apply(_tweak, option);
                        Update(_tweak);

                        _current += 1;

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

                        double _currentPercent = _current / _totalTweaks * 100;
                        taskViewModel.Progress = (int)_currentPercent;
                        taskViewModel.ShortMessage = $@"({_current}/{_totalTweaks}) {_tweak.Name}";
                    }
                    catch (Exception ex)
                    {
                        taskViewModel.CatchException(ex);
                        break;
                    }
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        /// <summary>
        /// Checks if tweak can be applied on current system
        /// </summary>
        /// <param name="tweak">Tweak to check</param>
        /// <returns></returns>
        private static bool IsRelevant(Tweak tweak)
        {
            int _windowsBuild = Helpers.GetWinBuild();
            int _result;
            switch (tweak.RequiredWinBuild[tweak.RequiredWinBuild.Length - 1])
            {
                case '+':
                    {
                        if (!int.TryParse(tweak.RequiredWinBuild.Substring(0, tweak.RequiredWinBuild.Length - 1), out _result))
                            return false;
                        if (_windowsBuild >= _result)
                            return true;
                        else
                            return false;
                    }
                case '-':
                    {
                        if (!int.TryParse(tweak.RequiredWinBuild.Substring(0, tweak.RequiredWinBuild.Length - 1), out _result))
                            return false;
                        if (_windowsBuild <= _result)
                            return true;
                        else
                            return false;
                    }
                default:
                    {
                        if (!int.TryParse(tweak.RequiredWinBuild, out _result))
                            return false;
                        if (_result == _windowsBuild)
                            return true;
                        else
                            return false;
                    }
            }
        }

        /// <summary>
        /// Returns collection of tweaks from saved preset
        /// </summary>
        /// <param name="presetName">Name of saved preset</param>
        /// <returns></returns>
        public static ObservableCollection<Tweak> SeekForRelevant(ObservableCollection<Tweak> tweaksCollection)
        {
            ObservableCollection<Tweak> output = new ObservableCollection<Tweak>();

            foreach (Tweak _tweak in tweaksCollection)
            {
                if (IsRelevant(_tweak))
                    output.Add(_tweak);
            }
            return output;
        }

        /// <summary>
        /// Gets all tweaks categories from tweaks collection
        /// </summary>
        /// <param name="tweaksCollection">Collection to get types from</param>
        /// <returns></returns>
        public static ObservableCollection<string> GetAllTweaksCategories(ObservableCollection<Tweak> tweaksCollection)
        {
            ObservableCollection<string> _types = new ObservableCollection<string>();

            foreach (Tweak _tweak in tweaksCollection)
            {
                if (!_types.Contains(_tweak.Category))
                    _types.Add(_tweak.Category);
            }

            return _types;
        }

        /// <summary>
        /// Gets all tweaks categories' descriptions from tweaks collection
        /// </summary>
        /// <param name="tweaksCollection">Collection to get types from</param>
        /// <returns></returns>
        public static ObservableCollection<string> GetAllTweaksCategoriesDescriptions(ObservableCollection<Tweak> tweaksCollection)
        {
            ObservableCollection<string> _types = new ObservableCollection<string>();
            ObservableCollection<string> _descriptions = new ObservableCollection<string>();

            foreach (Tweak _tweak in tweaksCollection)
            {
                if (!_types.Contains(_tweak.Category))
                {
                    _types.Add(_tweak.Category);
                    _descriptions.Add(_tweak.CategoryDescription);
                }
            }

            return _descriptions;
        }

        /// <summary>
        /// Builds folder-tweaks tree from tweaks collection based on it's categories
        /// </summary>
        /// <param name="tweaksCollection"></param>
        /// <returns></returns>
        public static TreeBranchViewModel<string> BuildTree(ObservableCollection<Tweak> tweaksCollection)
        {
            TreeBranchViewModel<string> _tree = new TreeBranchViewModel<string>("Root", "root");

            TreeBranchViewModel<string> _subNode;
            TreeBranchViewModel<string> _currentNode;

            ObservableCollection<string> _categories = GetAllTweaksCategories(tweaksCollection);
            ObservableCollection<string> _descriptions = GetAllTweaksCategoriesDescriptions(tweaksCollection);

            _subNode = _tree;

            for (int i = 0; i <= _categories.Count - 1; i++)
            {
                _subNode = _tree;
                string[] _trimmedPath = _categories.ElementAt(i).Split('\\').Select(str => str.Trim()).ToArray();

                string[] _trimmedDescriptions = { null };

                if (_descriptions.ElementAt(i) != null)
                    _trimmedDescriptions = _descriptions.ElementAt(i).Split('\\').Select(str => str.Trim()).ToArray();

                for (int j = 0; j <= _trimmedPath.Length - 1; j++)
                {
                    string _current = _trimmedPath.ElementAt(j);
                    string _currentDescription = null;

                    if (_trimmedDescriptions.Length - 1 >= j && _trimmedDescriptions.ElementAt(j) != "")
                        _currentDescription = _trimmedDescriptions.ElementAt(j);

                    _currentNode = new TreeBranchViewModel<string>(_current, _categories.ElementAt(i))
                    {
                        Description = _currentDescription
                    };
                    var _foundedNode = _tree.Flatten().Where(t => t.Name == _current).ToList().FirstOrDefault();
                    if (_foundedNode == null)
                    {
                        var _new = _tree.Flatten().Where(t => t.FullPath.Contains(_categories.ElementAt(i)) || _categories.ElementAt(i).Contains(t.Name)).ToList().LastOrDefault();

                        if (_new != null)
                        {
                            _new.Add(_currentNode);
                        }
                        else
                        {
                            _subNode.Add(_currentNode);
                            _subNode = _currentNode;
                        }
                    }
                }
            }

            foreach (Tweak _tweak in tweaksCollection)
            {
                var _folder = _tree.Flatten().LastOrDefault(t => (t.FullPath == _tweak.Category) && (t.Tweak == null));

                if (_folder == null)
                {
                    _folder = _tree.Flatten().LastOrDefault(t => t.Name == _tweak.Category && (t.Tweak == null));
                }

                if (_folder != null)
                {
                    _folder.Add(new TreeBranchViewModel<string>(_tweak.Name, _tweak));
                }
            }

            return _tree;
        }

        /// <summary>
        /// Gets hive from given registry key path
        /// </summary>
        /// <param name="path">Full registry key path</param>
        /// <returns></returns>
        private static RegistryHive GetRegistryKeyHive(string path)
        {
            RegistryHive _hive = RegistryHive.LocalMachine;

            int _trimIndex = path.IndexOf(@"\");
            string _trimmedHive = path.Substring(0, _trimIndex);

            switch (_trimmedHive)
            {
                case "HKEY_CLASSES_ROOT":
                    {
                        _hive = RegistryHive.ClassesRoot;
                        break;
                    }
                case "HKEY_CURRENT_USER":
                    {
                        _hive = RegistryHive.CurrentUser;
                        break;
                    }
                case "HKEY_LOCAL_MACHINE":
                    {
                        _hive = RegistryHive.LocalMachine;
                        break;
                    }
                case "HKEY_USERS":
                    {
                        _hive = RegistryHive.Users;
                        break;
                    }
                case "HKEY_CURRENT_CONFIG":
                    {
                        _hive = RegistryHive.CurrentConfig;
                        break;
                    }
            }

            return _hive;
        }

        /// <summary>
        /// Gets registry key from given registry key path
        /// </summary>
        /// <param name="path">Full registry key path</param>
        /// <returns></returns>
        private static RegistryKey GetRegistryKeyLocationPath(string path)
        {
            int _trimIndex = path.IndexOf(@"\");

            int _endTrimmer = _trimIndex;

            if (path.EndsWith("*"))
                _endTrimmer += 3;

            string _trimmedPathFull = path.Substring(_trimIndex + 1, path.Length - _endTrimmer);

            RegistryHive _hive = GetRegistryKeyHive(path);
            RegistryKey _baseKey = RegistryKey.OpenBaseKey(_hive, RegistryView.Default);
            RegistryKey _currentKey = _baseKey.OpenSubKey(_trimmedPathFull, true);

            if (_currentKey == null)
                return null;

            return _baseKey.OpenSubKey(_trimmedPathFull, true);
        }

        /// <summary>
        /// Removes registry key from key location
        /// </summary>
        /// <param name="path">Registry key location without key name</param>
        /// <param name="key">Registry key name</param>
        private static void RemoveRegistryKey(string path, string key)
        {
            int _trimIndex = path.IndexOf(@"\");
            int _trimIndexLast = path.LastIndexOf(@"\");
            string _trimmedPathFull = path.Substring(_trimIndex + 1, path.Length - _trimIndex - 1);
            string _trimmedPathKey = path.Substring(_trimIndexLast + 1);
            string _trimmedPathKeyFolder = _trimmedPathFull.Substring(0, _trimmedPathFull.Length - _trimmedPathKey.Length - 1);

            RegistryHive _hive = GetRegistryKeyHive(path);
            RegistryKey _baseKey = RegistryKey.OpenBaseKey(_hive, RegistryView.Default);
            RegistryKey _currentKey = _baseKey.OpenSubKey(_trimmedPathFull, true);

            if (_currentKey == null)
                return;

            if (_currentKey.GetValue(key) != null)
                _currentKey.DeleteValue(key);

            if (_currentKey.GetValueNames().Length == 0)
                _baseKey.OpenSubKey(_trimmedPathKeyFolder, true).DeleteSubKey(_trimmedPathKey);
        }
    }
}