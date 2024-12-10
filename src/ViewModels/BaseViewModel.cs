using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinCry.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region OnProperty Changed
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
