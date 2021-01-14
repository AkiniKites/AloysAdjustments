using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PresentationControls
{
    /// <summary>
    /// A Typed List of the CheckBox items.
    /// Simply a wrapper for the CheckBoxComboBox.Items. A list of CheckBoxComboBoxItem objects.
    /// This List is automatically synchronised with the Items of the ComboBox and extended to
    /// handle the additional boolean value. That said, do not Add or Remove using this List, 
    /// it will be lost or regenerated from the ComboBox.Items.
    /// </summary>
    [ToolboxItem(false)]
    public class CheckBoxComboBoxItemList : IReadOnlyList<CheckBoxComboBoxItem>
    {
        public CheckBoxComboBoxItemList(CheckBoxComboBox checkBoxComboBox)
        {
            _checkBoxComboBox = checkBoxComboBox;
            _listImpl = new List<CheckBoxComboBoxItem>();
        }

        private readonly CheckBoxComboBox _checkBoxComboBox;
        private readonly List<CheckBoxComboBoxItem> _listImpl;

        public event EventHandler CheckBoxCheckedChanged;

        protected void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            CheckBoxCheckedChanged?.Invoke(sender, e);
        }
        private void item_CheckedChanged(object sender, EventArgs e)
        {
            OnCheckBoxCheckedChanged(sender, e);
        }
        
        internal void Add(CheckBoxComboBoxItem item)
        {
            item.CheckedChanged += item_CheckedChanged;
            _listImpl.Add(item);
        }

        internal void AddRange(IEnumerable<CheckBoxComboBoxItem> collection)
        {
            foreach (CheckBoxComboBoxItem Item in collection)
                Item.CheckedChanged += item_CheckedChanged;
            _listImpl.AddRange(collection);
        }

        internal void Clear()
        {
            foreach (CheckBoxComboBoxItem Item in this)
                Item.CheckedChanged -= item_CheckedChanged;
            _listImpl.Clear();
        }
        
        internal bool Remove(CheckBoxComboBoxItem item)
        {
            item.CheckedChanged -= item_CheckedChanged;
            return _listImpl.Remove(item);
        }

        /// <summary>
        /// Returns the item with the specified displayName or Text.
        /// </summary>
        public CheckBoxComboBoxItem this[string displayName]
        {
            get
            {
                int startIndex =
                    // An invisible item exists in this scenario to help 
                    // with the Text displayed in the TextBox of the Combo
                    _checkBoxComboBox.DropDownStyle == ComboBoxStyle.DropDownList 
                    && _checkBoxComboBox.DataSource == null
                        ? 1 // Ubiklou : 2008-04-28 : Ignore first item. (http://www.codeproject.com/KB/combobox/extending_combobox.aspx?fid=476622&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2526813&fr=1#xx2526813xx)
                        : 0;
                for(int i = startIndex; i <= Count - 1; i ++)
                {
                    var item = _listImpl[i];

                    string text;
                    // The binding might not be active yet
                    if (string.IsNullOrEmpty(item.Text)
                        // Ubiklou : 2008-04-28 : No databinding
                        && item.DataBindings != null 
                        && item.DataBindings["Text"] != null
                    )
                    {
                        var pi = item.ComboBoxItem.GetType().GetProperty(
                            item.DataBindings["Text"].BindingMemberInfo.BindingMember);
                        text = (string)pi.GetValue(item.ComboBoxItem, null);
                    }
                    else
                        text = item.Text;
                    if (text.CompareTo(displayName) == 0)
                        return item;
                }
                throw new ArgumentOutOfRangeException($"\"{displayName}\" does not exist in this combo box.");
            }
        }

        public IEnumerator<CheckBoxComboBoxItem> GetEnumerator()
        {
            return _listImpl.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_listImpl).GetEnumerator();
        }

        public int Count => _listImpl.Count;

        public CheckBoxComboBoxItem this[int index] => _listImpl[index];
    }
}