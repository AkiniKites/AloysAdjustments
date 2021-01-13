using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace PresentationControls
{
    /// <summary>
    /// Maintains an additional "Selected" & "Count" value for each item in a List.
    /// Useful in the CheckBoxComboBox. It holds a reference to the List[Index] Item and 
    /// whether it is selected or not.
    /// It also caters for a Count, if needed.
    /// </summary>
    public class ListWrapper<T> : List<ObjectWrapper<T>>
    {
        #region CONSTRUCTOR

        /// <summary>
        /// No property on the object is specified for display purposes, so simple ToString() operation 
        /// will be performed. And no Counts will be displayed
        /// </summary>
        public ListWrapper(IEnumerable<T> source) : this(source, false) { }
        /// <summary>
        /// No property on the object is specified for display purposes, so simple ToString() operation 
        /// will be performed.
        /// </summary>
        public ListWrapper(IEnumerable<T> source, bool showCounts)
            : base()
        {
            _Source = source;
            ShowCounts = showCounts;
            if (_Source is IBindingList list)
                list.ListChanged += ListSelectionWrapper_ListChanged;
            Populate();
        }
        /// <summary>
        /// A Display "Name" property is specified. ToString() will not be performed on items.
        /// This is specifically useful on DataTable implementations, or where PropertyDescriptors are used to read the values.
        /// If a PropertyDescriptor is not found, a Property will be used.
        /// </summary>
        public ListWrapper(IEnumerable<T> source, Func<T, string> displayNameGetter) : this(source, false, displayNameGetter) { }
        /// <summary>
        /// A Display "Name" property is specified. ToString() will not be performed on items.
        /// This is specifically useful on DataTable implementations, or where PropertyDescriptors are used to read the values.
        /// If a PropertyDescriptor is not found, a Property will be used.
        /// </summary>
        public ListWrapper(IEnumerable<T> source, bool showCounts, Func<T, string> displayNameGetter)
            : this(source, showCounts)
        {
            DisplayNameGetter = displayNameGetter;
        }

        #endregion

        #region PRIVATE PROPERTIES

        /// <summary>
        /// The original List of values wrapped. A "Selected" and possibly "Count" functionality is added.
        /// </summary>
        private readonly IEnumerable<T> _Source;

        #endregion

        #region PUBLIC PROPERTIES

        /// <summary>
        /// When specified, indicates that ToString() should not be performed on the items. 
        /// This property will be read instead. 
        /// This is specifically useful on DataTable implementations, where PropertyDescriptors are used to read the values.
        /// </summary>
        public Func<T, string> DisplayNameGetter { get; set; }
        /// <summary>
        /// Builds a concatenation list of selected items in the list.
        /// </summary>
        public string SelectedNames => String.Join(", ", this.Where(x => x.Selected).Select(x => x.Name));

        /// <summary>
        /// Indicates whether the Item display value (Name) should include a count.
        /// </summary>
        public bool ShowCounts { get; set; }

        #endregion

        #region HELPER MEMBERS

        /// <summary>
        /// Reset all counts to zero.
        /// </summary>
        public void ClearCounts()
        {
            foreach (ObjectWrapper<T> Item in this)
                Item.Count = 0;
        }
        /// <summary>
        /// Creates a ObjectSelectionWrapper item.
        /// Note that the constructor signature of sub classes classes are important.
        /// </summary>
        private ObjectWrapper<T> CreateSelectionWrapper(T item)
        {
            return new ObjectWrapper<T>(item, this);
        }

        public ObjectWrapper<T> FindObjectWithItem(T Object)
        {
            return Find(target => target.Item.Equals(Object));
        }

        private void Populate()
        {
            Clear();

            foreach (var item in _Source)
                Add(CreateSelectionWrapper(item));
        }

        #endregion

        #region EVENT HANDLERS

        private void ListSelectionWrapper_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    Add(CreateSelectionWrapper((T)((IBindingList)_Source)[e.NewIndex]));
                    break;
                case ListChangedType.ItemDeleted:
                    Remove(FindObjectWithItem((T)((IBindingList)_Source)[e.OldIndex]));
                    break;
                case ListChangedType.Reset:
                    Populate();
                    break;
            }
        }

        #endregion
    }
}
