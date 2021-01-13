using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Data;

namespace PresentationControls
{
    /// <summary>
    /// Used together with the ListSelectionWrapper in order to wrap data sources for a CheckBoxComboBox.
    /// It helps to ensure you don't add an extra "Selected" property to a class that don't really need or want that information.
    /// </summary>
    public class ObjectWrapper<T> : INotifyPropertyChanged
    {
        public ObjectWrapper(T item, ListWrapper<T> container)
            : base()
        {
            _Container = container;
            Item = item;
        }


        #region PRIVATE PROPERTIES

        /// <summary>
        /// Is this item selected.
        /// </summary>
        private bool _Selected = false;

        /// <summary>
        /// The containing list for these selections.
        /// </summary>
        private ListWrapper<T> _Container;

        #endregion

        #region PUBLIC PROPERTIES

        /// <summary>
        /// An indicator of how many items with the specified status is available for the current filter level.
        /// Thaught this would make the app a bit more user-friendly and help not to miss items in Statusses
        /// that are not often used.
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// A reference to the item wrapped.
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// The item display value. If ShowCount is true, it displays the "Name [Count]".
        /// </summary>
        public string Name
        {
            get 
            {
                string Name = null;
                if (_Container.DisplayNameGetter == null)
                    Name = Item.ToString();
                else if (Item is DataRow) // A specific implementation for DataRow
                    throw new NotImplementedException();
                else
                    Name = _Container.DisplayNameGetter(Item);
                return _Container.ShowCounts ? $"{Name} [{Count}]" : Name;
            }
        }
        /// <summary>
        /// The textbox display value. The names concatenated.
        /// </summary>
        public string NameConcatenated => _Container.SelectedNames;

        /// <summary>
        /// Indicates whether the item is selected.
        /// </summary>
        public bool Selected
        {
            get => _Selected;
            set 
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    OnPropertyChanged("Selected");
                    OnPropertyChanged("NameConcatenated");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
