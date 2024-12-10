using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class ProgressWindowViewModel : BaseViewModel, IDialogRequestClose
    {
        #region Constructor

        public ProgressWindowViewModel()
        {
            _tasks = new ObservableCollection<Task>();
            TaskViewModels = new ObservableCollection<TaskViewModel>();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Notify on NotifyIsDone task event
        /// </summary>
        /// <param name="isDone">Is it done</param>
        private void NotifyOnDone(bool isDone)
        {
            int _total = TaskViewModels.Count();
            int _totalDone = TaskViewModels.Where(t => t.IsCompleted).Count();

            if (_totalDone == _total)
                IsDone = true;
            else
                IsDone = false;
        }

        /// <summary>
        /// Adds <paramref name="task"/> to ProgressWindowViewModels` arrays
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="taskViewModel">TaskViewModel of task</param>
        public void AddTask(Task task, TaskViewModel taskViewModel, Action action = null)
        {
            TaskViewModels.Add(taskViewModel);
            _tasks.Add(task);

            if (action != null)
            {
                task.ContinueWith((t1) =>
                {
                    if (t1.IsCompleted)
                    {
                        action.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// Starts all tasks
        /// </summary>
        public void StartTasks()
        {
            foreach (TaskViewModel _taskVM in TaskViewModels)
            {
                _taskVM.NotifyIsDone += NotifyOnDone;
            }

            foreach (Task _task in _tasks)
            {
                _task.Start();
            }
        }

        #endregion

        #region Private Properties

        private readonly ObservableCollection<Task> _tasks;

        #endregion

        #region Public Properties

        private string _dialogCaption = DialogConsts.ApplyingCaption;
        public string DialogCaption
        {
            get { return _dialogCaption; }
            set
            {
                _dialogCaption = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TaskViewModel> _taskViewModels;
        public ObservableCollection<TaskViewModel> TaskViewModels
        {
            get { return _taskViewModels; }
            set
            {
                _taskViewModels = value;
                OnPropertyChanged();
            }
        }

        private bool _isDone;
        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                _isDone = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        private RelayCommand _ok;
        public RelayCommand OK
        {
            get
            {
                return _ok ??
                   (_ok = new RelayCommand(obj =>
                   {
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
                   }));
            }
        }

        #endregion

        #region Events

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #endregion
    }
}