using System;

namespace WinCry.ViewModels
{
    /// <summary>
    /// A view model for dialog based tasks
    /// </summary>
    public class TaskViewModel : BaseViewModel
    {
        #region Events

        public delegate void DoneHandler(bool isDone);
        public event DoneHandler NotifyIsDone;

        #endregion

        #region Public Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged();
            }
        }

        private string _shortMessage;
        public string ShortMessage
        {
            get { return _shortMessage; }
            set
            {
                _shortMessage = value;
                OnPropertyChanged();
            }
        }

        private bool? _isSuccessfull;
        public bool? IsSuccessfull
        {
            get { return _isSuccessfull; }
            set
            {
                _isSuccessfull = value;
                OnPropertyChanged();
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;
                NotifyIsDone?.Invoke(value);
                OnPropertyChanged();
            }
        }

        private int _progress;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                if (_progress == 100)
                    IsCompleted = true;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Creates message with timestamp and adds it to TaskViewModels' Details property
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="isNewLined">Start with new line</param>
        public void CreateMessage(string message, bool isNewLined = true, bool insertTimeStamp = true)
        {
            if (isNewLined)
                Details += Environment.NewLine;

            if (insertTimeStamp)
                Details += $"{DateTime.Now:HH:mm:ss} - {message}";
            else
                Details += message;
        }

        /// <summary>
        /// Creates success <paramref name="message"/>
        /// </summary>
        /// <param name="message">Message to show</param>
        public void CreateSuccessMessage(string message)
        {
            CreateMessage(message);
            ShortMessage = message;
            IsSuccessfull = true;
            Progress = 100;
        }

        /// <summary>
        /// Creates failure <paramref name="message"/>
        /// </summary>
        /// <param name="message">Message to show</param>
        public void CreateFailureMessage(string message)
        {
            CreateMessage(message, false, false);
            ShortMessage = message;
            IsSuccessfull = false;
            Progress = 100;
        }

        /// <summary>
        /// Sets properties based on <paramref name="exception"/>'s output
        /// </summary>
        /// <param name="exception">Exception</param>
        public void CatchException(Exception exception)
        {
            IsSuccessfull = false;
            IsCompleted = true;
            ShortMessage = $@"{Dialogs.DialogConsts.Error}: {exception.Message}";
            CreateMessage(exception.ToString());
        }

        #endregion
    }
}
