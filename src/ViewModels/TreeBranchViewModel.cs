using System.Collections;
using System.Collections.Generic;
using WinCry.Tweaks;

namespace WinCry.ViewModels
{
    public class TreeBranchViewModel<T> : BaseViewModel, IEnumerable<TreeBranchViewModel<T>>
    {
        #region Constructor

        public TreeBranchViewModel(T value, string fullPath)
        {
            _value = value;
            FullPath = fullPath;
        }

        public TreeBranchViewModel(T value, Tweak tweak)
        {
            _value = value;
            FullPath = tweak.Category;
            Tweak = tweak;
            _isChecked = tweak.IsChecked;
        }

        #endregion

        #region Private members

        private readonly T _value;

        #endregion

        #region Public Properties

        public Tweak Tweak { get; set; }

        bool? _isChecked = false;
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        public string Name => _value.ToString();
        public string FullPath { get; set; }
        public string Description { get; set; }
        public TreeBranchViewModel<T> Parent { get; set; }
        public List<TreeBranchViewModel<T>> Children { get; set; } = new List<TreeBranchViewModel<T>>();

        #endregion

        #region Functions

        public void Initialize()
        {
            foreach (TreeBranchViewModel<T> child in Children)
            {
                child.Parent = this;
                child.Initialize();
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            if (Tweak != null)
                Tweak.IsChecked = (bool)value;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        public void Add(TreeBranchViewModel<T> item)
        {
            Children.Add(item);
            Initialize();
            SetIsChecked(item.IsChecked, false, true);
        }

        public IEnumerator<TreeBranchViewModel<T>> GetEnumerator() => Children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}