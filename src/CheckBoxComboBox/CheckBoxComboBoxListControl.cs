using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PresentationControls
{
    /// <summary>
    /// This ListControl that pops up to the User. It contains the CheckBoxComboBoxItems. 
    /// The items are docked DockStyle.Top in this control.
    /// </summary>
    [ToolboxItem(false)]
    public class CheckBoxComboBoxListControl : ScrollableControl
    {
        public CheckBoxComboBoxListControl(CheckBoxComboBox owner)
            : base()
        {
            DoubleBuffered = true;
            _CheckBoxComboBox = owner;
            Items = new CheckBoxComboBoxItemList(_CheckBoxComboBox);
            BackColor = SystemColors.Window;
            // AutoScaleMode = AutoScaleMode.Inherit;
            AutoScroll = true;
            ResizeRedraw = true;
            // if you don't set this, a Resize operation causes an error in the base class.
            MinimumSize = new Size(1, 1);
            MaximumSize = new Size(500, 500);
        }

        /// <summary>
        /// Simply a reference to the CheckBoxComboBox.
        /// </summary>
        private CheckBoxComboBox _CheckBoxComboBox;

        /// <summary>
        /// A Typed list of ComboBoxCheckBoxItems.
        /// </summary>
        public CheckBoxComboBoxItemList Items { get; }

        /// <summary>
        /// Prescribed by the Popup control to enable Resize operations.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (((Popup)Parent.Parent).ProcessResizing(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // Synchronises the CheckBox list with the items in the ComboBox.
            SynchroniseControlsWithComboBoxItems();
            base.OnVisibleChanged(e);
        }
        /// <summary>
        /// Maintains the controls displayed in the list by keeping them in sync with the actual 
        /// items in the combobox. (e.g. removing and adding as well as ordering)
        /// </summary>
        public void SynchroniseControlsWithComboBoxItems()
        {
            SuspendLayout();
            if (_CheckBoxComboBox._MustAddHiddenItem)
            {
                _CheckBoxComboBox.Items.Insert(
                    0, _CheckBoxComboBox.GetCSVText(false)); // INVISIBLE ITEM
                _CheckBoxComboBox.SelectedIndex = 0;
                _CheckBoxComboBox._MustAddHiddenItem = false;
            }
            Controls.Clear();

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (!_CheckBoxComboBox.Items.Contains(item.ComboBoxItem))
                {
                    Items.Remove(item);
                    item.Dispose();
                }
            }

            bool hadHiddenItem = 
                _CheckBoxComboBox.DropDownStyle == ComboBoxStyle.DropDownList
                && _CheckBoxComboBox.DataSource == null
                && !DesignMode;

            var newList = new CheckBoxComboBoxItemList(_CheckBoxComboBox);
            for(int i = 0; i <= _CheckBoxComboBox.Items.Count - 1; i ++)
            {
                object Object = _CheckBoxComboBox.Items[i];
                CheckBoxComboBoxItem item = null;
                // The hidden item could match any other item when only
                // one other item was selected.
                if (i == 0 && hadHiddenItem && Items.Count > 0)
                    item = Items[0];
                else
                {
                    int startIndex = hadHiddenItem
                        ? 1 // Skip the hidden item, it could match 
                        : 0;
                    for (int j = startIndex; j <= Items.Count - 1; j++)
                    {
                        if (Items[j].ComboBoxItem == Object)
                        {
                            item = Items[j];
                            break;
                        }
                    }
                }
                if (item == null)
                {
                    item = new CheckBoxComboBoxItem(_CheckBoxComboBox, Object);
                    item.ApplyProperties(_CheckBoxComboBox.CheckBoxProperties);
                }
                newList.Add(item);
                item.Dock = DockStyle.Top;
            }
            Items.Clear();
            Items.AddRange(newList);

            if (newList.Count > 0)
            {
                // This reverse helps to maintain correct docking order.
                newList.Reverse();
                // If you get an error here that "Cannot convert to the desired 
                // type, it probably means the controls are not binding correctly.
                // The Checked property is binded to the ValueMember property. 
                // It must be a bool for example.
                Controls.AddRange(newList.ToArray());
            }

            // Keep the first item invisible
            if (_CheckBoxComboBox.DropDownStyle == ComboBoxStyle.DropDownList
                && _CheckBoxComboBox.DataSource == null
                && !DesignMode)
                _CheckBoxComboBox.CheckBoxItems[0].Visible = false; 
            
            ResumeLayout();
        }
    }
}