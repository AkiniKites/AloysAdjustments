using System;
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
    /// The CheckBox items displayed in the Popup of the ComboBox.
    /// </summary>
    [ToolboxItem(false)]
    public class CheckBoxComboBoxItem : CheckBox
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">A reference to the CheckBoxComboBox.</param>
        /// <param name="comboBoxItem">A reference to the item in the ComboBox.Items that this object is extending.</param>
        public CheckBoxComboBoxItem(CheckBoxComboBox owner, object comboBoxItem)
            : base()
        {
            Height = (int)(FontHeight * 1.2);
            DoubleBuffered = true;
            _checkBoxComboBox = owner;
            ComboBoxItem = comboBoxItem;
            if (_checkBoxComboBox.DataSource != null)
                AddBindings();
            else
                Text = comboBoxItem.ToString();
        }

        /// <summary>
        /// A reference to the CheckBoxComboBox.
        /// </summary>
        private readonly CheckBoxComboBox _checkBoxComboBox;

        /// <summary>
        /// A reference to the Item in ComboBox.Items that this object is extending.
        /// </summary>
        public object ComboBoxItem { get; internal set; }

        /// <summary>
        /// When using Data Binding operations via the DataSource property of the ComboBox. This
        /// adds the required Bindings for the CheckBoxes.
        /// </summary>
        public void AddBindings()
        {
            // Note, the text uses "DisplayMemberSingleItem", not "DisplayMember" (unless its not assigned)
            DataBindings.Add(
                "Text",
                ComboBoxItem,
                _checkBoxComboBox.DisplayMemberSingleItem
            );
            // The ValueMember must be a bool type property usable by the CheckBox.Checked.
            DataBindings.Add(
                "Checked",
                ComboBoxItem,
                _checkBoxComboBox.ValueMember,
                false,
                // This helps to maintain proper selection state in the Binded object,
                // even when the controls are added and removed.
                DataSourceUpdateMode.OnPropertyChanged,
                false, null, null);
            // Helps to maintain the Checked status of this
            // checkbox before the control is visible
            if (ComboBoxItem is INotifyPropertyChanged)
                ((INotifyPropertyChanged)ComboBoxItem).PropertyChanged += 
                    new PropertyChangedEventHandler(
                        CheckBoxComboBoxItem_PropertyChanged);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            // Found that when this event is raised, the bool value of the binded item is not yet updated.
            if (_checkBoxComboBox.DataSource != null)
            {
                var pi = ComboBoxItem.GetType().GetProperty(_checkBoxComboBox.ValueMember);
                pi.SetValue(ComboBoxItem, Checked, null);
            }
            base.OnCheckedChanged(e);
            // Forces a refresh of the Text displayed in the main TextBox of the ComboBox,
            // since that Text will most probably represent a concatenation of selected values.
            // Also see DisplayMemberSingleItem on the CheckBoxComboBox for more information.
            if (_checkBoxComboBox.DataSource != null)
            {
                var tmp = _checkBoxComboBox.DisplayMember;
                _checkBoxComboBox.DisplayMember = null;
                _checkBoxComboBox.DisplayMember = tmp;
            }
        }

        internal void ApplyProperties(CheckBoxProperties properties)
        {
            Appearance = properties.Appearance;
            AutoCheck = properties.AutoCheck;
            AutoEllipsis = properties.AutoEllipsis;
            AutoSize = properties.AutoSize;
            CheckAlign = properties.CheckAlign;
            FlatAppearance.BorderColor = properties.FlatAppearanceBorderColor;
            FlatAppearance.BorderSize = properties.FlatAppearanceBorderSize;
            FlatAppearance.CheckedBackColor = properties.FlatAppearanceCheckedBackColor;
            FlatAppearance.MouseDownBackColor = properties.FlatAppearanceMouseDownBackColor;
            FlatAppearance.MouseOverBackColor = properties.FlatAppearanceMouseOverBackColor;
            FlatStyle = properties.FlatStyle;
            ForeColor = properties.ForeColor;
            RightToLeft = properties.RightToLeft;
            TextAlign = properties.TextAlign;
            ThreeState = properties.ThreeState;
        }

        /// <summary>
        /// Added this handler because the control doesn't seem 
        /// to initialize correctly until shown for the first
        /// time, which also means the summary text value
        /// of the combo is out of sync initially.
        /// </summary>
        private void CheckBoxComboBoxItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _checkBoxComboBox.ValueMember)
            {
                Checked =
                    (bool)ComboBoxItem
                        .GetType()
                        .GetProperty(_checkBoxComboBox.ValueMember)
                        .GetValue(ComboBoxItem, null);
            }
        }
    }
}