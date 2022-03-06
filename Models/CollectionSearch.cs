using System.Collections.ObjectModel;
using WinCry.Services;
using WinCry.Tweaks;

namespace WinCry.Models
{
    class CollectionSearch
    {
        public static ObservableCollection<Service> GetAllServicesThatContains(ObservableCollection<Service> initialCollection, string text)
        {
            ObservableCollection<Service> _services = new ObservableCollection<Service>();
            IStringSearchAlgorithm searchAlg = new StringSearch
            {
                Keywords = new string[] { text.ToLower() }
            };

            foreach (Service _service in initialCollection)
            {
                string _shortName = _service.ShortName== null? "" : _service.ShortName.ToLower();
                string _fullName = _service.FullName == null ? "" : _service.FullName.ToLower();
                string _description = _service.Description == null ? "" : _service.Description.ToLower();

                if (searchAlg.ContainsAny(_shortName) || searchAlg.ContainsAny(_fullName) || searchAlg.ContainsAny(_description))
                {
                    _services.Add(_service);
                    continue;
                }
            }

            return _services;
        }

        public static ObservableCollection<Tweak> GetAllTweaksThatContains(ObservableCollection<Tweak> initialCollection, string text)
        {
            ObservableCollection<Tweak> _tweaks = new ObservableCollection<Tweak>();
            IStringSearchAlgorithm searchAlg = new StringSearch
            {
                Keywords = new string[] { text.ToLower() }
            };

            foreach (Tweak _tweak in initialCollection)
            {
                string _key = _tweak.Key == null ? "" : _tweak.Key.ToLower();
                string _name = _tweak.Name == null ? "" : _tweak.Name.ToLower();
                string _description = _tweak.Description == null ? "" : _tweak.Description.ToLower();

                if (searchAlg.ContainsAny(_key) || searchAlg.ContainsAny(_description) || searchAlg.ContainsAny(_name))
                {
                    _tweaks.Add(_tweak);
                    continue;
                }
            }

            return _tweaks;
        }
    }
}
