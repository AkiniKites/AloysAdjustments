using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace PresentationControls
{
    /// <summary>
    /// Martin Lottering : 2007-10-27
    /// --------------------------------
    /// This is a usefull control in Filters. Allows you to save space and can replace a Grouped Box of CheckBoxes.
    /// Currently used on the TasksFilter for TaskStatusses, which means the user can select which Statusses to include
    /// in the "Search".
    /// This control does not implement a CheckBoxListBox, instead it adds a wrapper for the normal ComboBox and Items. 
    /// See the CheckBoxItems property.
    /// ----------------
    /// ALSO IMPORTANT: In Data Binding when setting the DataSource. The ValueMember must be a bool type property, because it will 
    /// be binded to the Checked property of the displayed CheckBox. Also see the DisplayMemberSingleItem for more information.
    /// ----------------
    /// Extends the CodeProject PopupComboBox "Simple pop-up control" "http://www.codeproject.com/cs/miscctrl/simplepopup.asp"
    /// by Lukasz Swiatkowski.
    /// </summary>
    public partial class CheckBoxComboBox : PopupComboBox
    {
        public CheckBoxComboBox()
            : base()
        {
            InitializeComponent();
            _CheckBoxProperties = new CheckBoxProperties();
            _CheckBoxProperties.PropertyChanged += _CheckBoxProperties_PropertyChanged;
            // Dumps the ListControl in a(nother) Container to ensure the ScrollBar on the ListControl does not
            // Paint over the Size grip. Setting the Padding or Margin on the Popup or host control does
            // not work as I expected. I don't think it can work that way.
            CheckBoxComboBoxListControlContainer ContainerControl = new CheckBoxComboBoxListControlContainer();
            _CheckBoxComboBoxListControl = new CheckBoxComboBoxListControl(this);
            _CheckBoxComboBoxListControl.Items.CheckBoxCheckedChanged += Items_CheckBoxCheckedChanged;
            ContainerControl.Controls.Add(_CheckBoxComboBoxListControl);
            // This padding spaces neatly on the left-hand side and allows space for the size grip at the bottom.
            ContainerControl.Padding = new Padding(4, 4, 0, 4);
            // The ListControl FILLS the ListContainer.
            _CheckBoxComboBoxListControl.Dock = DockStyle.Fill;
            // The DropDownControl used by the base class. Will be wrapped in a popup by the base class.
            DropDownControl = ContainerControl;
            // Must be set after the DropDownControl is set, since the popup is recreated.
            // NOTE: I made the dropDown protected so that it can be accessible here. It was private.
            dropDown.Resizable = false;
            dropDown.onCloseEvent += OnDropDownClosed;
        }

        /// <summary>
        /// The checkbox list control. The public CheckBoxItems property provides a direct reference to its Items.
        /// </summary>
        internal CheckBoxComboBoxListControl _CheckBoxComboBoxListControl;
        /// <summary>
        /// In DataBinding operations, this property will be used as the DisplayMember in the CheckBoxComboBoxListBox.
        /// The normal/existing "DisplayMember" property is used by the TextBox of the ComboBox to display 
        /// a concatenated Text of the items selected. This concatenation and its formatting however is controlled 
        /// by the Binded object, since it owns that property.
        /// </summary>
        private string _DisplayMemberSingleItem = null;
        internal bool _MustAddHiddenItem = false;
        private bool _ClosingDropDown = false;

        /// <summary>
        /// Builds a CSV string of the items selected.
        /// </summary>
        internal string GetCSVText(bool skipFirstItem)
        {
            string text = String.Empty;
            int startIndex =
                DropDownStyle == ComboBoxStyle.DropDownList 
                && DataSource == null
                && skipFirstItem ? 1 : 0;
            for (int i = startIndex; i <= _CheckBoxComboBoxListControl.Items.Count - 1; i++)
            {
                var item = _CheckBoxComboBoxListControl.Items[i];
                if (item.Checked)
                    text += string.IsNullOrEmpty(text) ? item.Text : $", {item.Text}";
            }
            return text;
        }

        protected virtual void OnDropDownClosed(object sender, EventArgs e)
        {
            _ClosingDropDown = true;
            base.OnDropDownClosed(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            if (!_ClosingDropDown)
                return;
            _ClosingDropDown = false;

            base.OnDropDownClosed(e);
        }

        /// <summary>
        /// A direct reference to the Items of CheckBoxComboBoxListControl.
        /// You can use it to Get or Set the Checked status of items manually if you want.
        /// But do not manipulate the List itself directly, e.g. Adding and Removing, 
        /// since the list is synchronised when shown with the ComboBox.Items. So for changing 
        /// the list contents, use Items instead.
        /// </summary>
        [Browsable(false)]
        public CheckBoxComboBoxItemList CheckBoxItems
        {
            get 
            { 
                // Added to ensure the CheckBoxItems are ALWAYS
                // available for modification via code.
                if (_CheckBoxComboBoxListControl.Items.Count != Items.Count)
                    _CheckBoxComboBoxListControl.SynchroniseControlsWithComboBoxItems();
                return _CheckBoxComboBoxListControl.Items; 
            }
        }
        /// <summary>
        /// The DataSource of the combobox. Refreshes the CheckBox wrappers when this is set.
        /// </summary>
        public new object DataSource
        {
            get => base.DataSource;
            set
            {
                base.DataSource = value;
                if (!string.IsNullOrEmpty(ValueMember))
                    // This ensures that at least the checkboxitems are available to be initialised.
                    _CheckBoxComboBoxListControl.SynchroniseControlsWithComboBoxItems();
            }
        }
        /// <summary>
        /// The ValueMember of the combobox. Refreshes the CheckBox wrappers when this is set.
        /// </summary>
        public new string ValueMember
        {
            get => base.ValueMember;
            set
            {
                base.ValueMember = value;
                if (!string.IsNullOrEmpty(ValueMember))
                    // This ensures that at least the checkboxitems are available to be initialised.
                    _CheckBoxComboBoxListControl.SynchroniseControlsWithComboBoxItems();
            }
        }
        /// <summary>
        /// In DataBinding operations, this property will be used as the DisplayMember in the CheckBoxComboBoxListBox.
        /// The normal/existing "DisplayMember" property is used by the TextBox of the ComboBox to display 
        /// a concatenated Text of the items selected. This concatenation however is controlled by the Binded 
        /// object, since it owns that property.
        /// </summary>
        public string DisplayMemberSingleItem
        {
            get { if (string.IsNullOrEmpty(_DisplayMemberSingleItem)) return DisplayMember; else return _DisplayMemberSingleItem; }
            set => _DisplayMemberSingleItem = value;
        }
        /// <summary>
        /// Made this property Browsable again, since the Base Popup hides it. This class uses it again.
        /// Gets an object representing the collection of the items contained in this 
        /// System.Windows.Forms.ComboBox.
        /// </summary>
        /// <returns>A System.Windows.Forms.ComboBox.ObjectCollection representing the items in 
        /// the System.Windows.Forms.ComboBox.
        /// </returns>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new ObjectCollection Items => base.Items;

        public event EventHandler CheckBoxCheckedChanged;

        private void Items_CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            OnCheckBoxCheckedChanged(sender, e);
        }

        protected void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            var listText = GetCSVText(true);
            // The DropDownList style seems to require that the text
            // part of the "textbox" should match a single item.
            if (DropDownStyle != ComboBoxStyle.DropDownList)
                Text = listText;
            // This refreshes the Text of the first item (which is not visible)
            else if (DataSource == null)
            {
                Items[0] = listText;
                // Keep the hidden item and first checkbox item in 
                // sync in order to ensure the Synchronise process
                // can match the items.
                CheckBoxItems[0].ComboBoxItem = listText;
            }
            
            CheckBoxCheckedChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Will add an invisible item when the style is DropDownList,
        /// to help maintain the correct text in main TextBox.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            base.OnDropDownStyleChanged(e);

            if (DropDownStyle == ComboBoxStyle.DropDownList
                && DataSource == null
                && !DesignMode)
            {
                _MustAddHiddenItem = true;
            }
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);

            _MustAddHiddenItem = DropDownStyle == ComboBoxStyle.DropDownList
                && DataSource == null && !DesignMode;
        }

        protected override void OnResize(EventArgs e)
        {
            // When the ComboBox is resized, the width of the dropdown 
            // is also resized to match the width of the ComboBox. I think it looks better.
            Size Size = new Size(Width, DropDownControl.Height);
            dropDown.Size = Size;
            base.OnResize(e);
        }

        /// <summary>
        /// A function to clear/reset the list.
        /// (Ubiklou : http://www.codeproject.com/KB/combobox/extending_combobox.aspx?msg=2526813#xx2526813xx)
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            if (DropDownStyle == ComboBoxStyle.DropDownList && DataSource == null)
                _MustAddHiddenItem = true;                
        }        /// <summary>
        /// Uncheck all items.
        /// </summary>
        public void ClearSelection()
        {
            foreach (CheckBoxComboBoxItem Item in CheckBoxItems)
                if (Item.Checked)
                    Item.Checked = false;
        }

        private CheckBoxProperties _CheckBoxProperties;

        /// <summary>
        /// The properties that will be assigned to the checkboxes as default values.
        /// </summary>
        [Description("The properties that will be assigned to the checkboxes as default values.")]
        [Browsable(true)]
        public CheckBoxProperties CheckBoxProperties
        {
            get => _CheckBoxProperties;
            set { _CheckBoxProperties = value; _CheckBoxProperties_PropertyChanged(this, EventArgs.Empty); }
        }

        private void _CheckBoxProperties_PropertyChanged(object sender, EventArgs e)
        {
            foreach (CheckBoxComboBoxItem Item in CheckBoxItems)
                Item.ApplyProperties(CheckBoxProperties);
        }

        protected override void WndProc(ref Message m)
        {
            // 323 : Item Added
            // 331 : Clearing
            if (m.Msg == 331
                && DropDownStyle == ComboBoxStyle.DropDownList
                && DataSource == null)
            {
                _MustAddHiddenItem = true;
            }
            
            base.WndProc(ref m);
        }
    }
}
