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
        #region CONSTRUCTOR

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

        #endregion

        #region PRIVATE PROPERTIES

        /// <summary>
        /// Simply a reference to the CheckBoxComboBox.
        /// </summary>
        private CheckBoxComboBox _CheckBoxComboBox;

        #endregion

        /// <summary>
        /// A Typed list of ComboBoxCheckBoxItems.
        /// </summary>
        public CheckBoxComboBoxItemList Items { get; }

        #region RESIZE OVERRIDE REQUIRED BY THE POPUP CONTROL

        /// <summary>
        /// Prescribed by the Popup control to enable Resize operations.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if ((Parent.Parent as Popup).ProcessResizing(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        #endregion

        #region PROTECTED MEMBERS

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
            #region Disposes all items that are no longer in the combo box list

            for (int Index = Items.Count - 1; Index >= 0; Index--)
            {
                CheckBoxComboBoxItem Item = Items[Index];
                if (!_CheckBoxComboBox.Items.Contains(Item.ComboBoxItem))
                {
                    Items.Remove(Item);
                    Item.Dispose();
                }
            }

            #endregion
            #region Recreate the list in the same order of the combo box items

            bool HasHiddenItem = 
                _CheckBoxComboBox.DropDownStyle == ComboBoxStyle.DropDownList
                && _CheckBoxComboBox.DataSource == null
                && !DesignMode;

            CheckBoxComboBoxItemList NewList = new CheckBoxComboBoxItemList(_CheckBoxComboBox);
            for(int Index0 = 0; Index0 <= _CheckBoxComboBox.Items.Count - 1; Index0 ++)
            {
                object Object = _CheckBoxComboBox.Items[Index0];
                CheckBoxComboBoxItem Item = null;
                // The hidden item could match any other item when only
                // one other item was selected.
                if (Index0 == 0 && HasHiddenItem && Items.Count > 0)
                    Item = Items[0];
                else
                {
                    int StartIndex = HasHiddenItem
                        ? 1 // Skip the hidden item, it could match 
                        : 0;
                    for (int Index1 = StartIndex; Index1 <= Items.Count - 1; Index1++)
                    {
                        if (Items[Index1].ComboBoxItem == Object)
                        {
                            Item = Items[Index1];
                            break;
                        }
                    }
                }
                if (Item == null)
                {
                    Item = new CheckBoxComboBoxItem(_CheckBoxComboBox, Object);
                    Item.ApplyProperties(_CheckBoxComboBox.CheckBoxProperties);
                }
                NewList.Add(Item);
                Item.Dock = DockStyle.Top;
            }
            Items.Clear();
            Items.AddRange(NewList);

            #endregion
            #region Add the items to the controls in reversed order to maintain correct docking order

            if (NewList.Count > 0)
            {
                // This reverse helps to maintain correct docking order.
                NewList.Reverse();
                // If you get an error here that "Cannot convert to the desired 
                // type, it probably means the controls are not binding correctly.
                // The Checked property is binded to the ValueMember property. 
                // It must be a bool for example.
                Controls.AddRange(NewList.ToArray());
            }

            #endregion

            // Keep the first item invisible
            if (_CheckBoxComboBox.DropDownStyle == ComboBoxStyle.DropDownList
                && _CheckBoxComboBox.DataSource == null
                && !DesignMode)
                _CheckBoxComboBox.CheckBoxItems[0].Visible = false; 
            
            ResumeLayout();
        }

        #endregion
    }
}