using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace PresentationControls
{
    /// <summary>
    /// CodeProject.com "Simple pop-up control" "http://www.codeproject.com/cs/miscctrl/simplepopup.asp".
    /// Represents a Windows combo box control with a custom popup control attached.
    /// </summary>
    [ToolboxBitmap(typeof(ComboBox)), ToolboxItem(true), ToolboxItemFilter("System.Windows.Forms"), 
     Description("Displays an editable text box with a drop-down list of permitted values.")]
    public partial class PopupComboBox : ComboBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupComboBox" /> class.
        /// </summary>
        public PopupComboBox()
        {
            InitializeComponent();
            base.DropDownHeight = base.DropDownWidth = 1;
            base.IntegralHeight = false;
        }

        /// <summary>
        /// The pop-up wrapper for the dropDownControl. 
        /// Made PROTECTED instead of PRIVATE so descendent classes can set its Resizable property.
        /// Note however the pop-up properties must be set after the dropDownControl is assigned, since this 
        /// popup wrapper is recreated when the dropDownControl is assigned.
        /// </summary>
        protected Popup dropDown;

        private Control dropDownControl;
        public Control DropDownControl
        {
            get => dropDownControl;
            set
            {
                if (dropDownControl == value)
                    return;
                dropDownControl = value;
                dropDown = new Popup(value);
            }
        }
        
        public void ShowDropDown()
        {
            dropDown?.Show(this);
        }
        
        public void HideDropDown()
        {
            dropDown?.Hide();
        }
        
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (NativeMethods.WM_REFLECT + NativeMethods.WM_COMMAND))
            {
                if (NativeMethods.HIWORD(m.WParam) == NativeMethods.CBN_DROPDOWN)
                {
                    // Blocks a redisplay when the user closes the control by clicking 
                    // on the combobox.
                    var ts = DateTime.Now.Subtract(dropDown.LastClosedTimeStamp);
                    if (!dropDown.Visible && ts.TotalMilliseconds > 30)
                        ShowDropDown();
                    Capture = true;
                    return;
                }
            }
            base.WndProc(ref m);
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new int DropDownWidth
        {
            get => base.DropDownWidth;
            set => base.DropDownWidth = value;
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new int DropDownHeight
        {
            get => base.DropDownHeight;
            set
            {
                dropDown.Height = value;
                base.DropDownHeight = value;
            }
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public bool DropDownResizeable
        {
            get => dropDown.Resizable;
            set => dropDown.Resizable = value;
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IntegralHeight
        {
            get => base.IntegralHeight;
            set => base.IntegralHeight = value;
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new ObjectCollection Items => base.Items;
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new int ItemHeight
        {
            get => base.ItemHeight;
            set => base.ItemHeight = value;
        }
    }
}
