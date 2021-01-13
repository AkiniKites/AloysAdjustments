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
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CheckBoxProperties
    {
        public CheckBoxProperties() { }

        private Appearance _Appearance = Appearance.Normal;
        private bool _AutoSize = false;
        private bool _AutoCheck = true;
        private bool _AutoEllipsis = false;
        private ContentAlignment _CheckAlign = ContentAlignment.MiddleLeft;
        private Color _FlatAppearanceBorderColor = Color.Empty;
        private int _FlatAppearanceBorderSize = 1;
        private Color _FlatAppearanceCheckedBackColor = Color.Empty;
        private Color _FlatAppearanceMouseDownBackColor = Color.Empty;
        private Color _FlatAppearanceMouseOverBackColor = Color.Empty;
        private FlatStyle _FlatStyle = FlatStyle.Standard;
        private Color _ForeColor = SystemColors.ControlText;
        private RightToLeft _RightToLeft = RightToLeft.No;
        private ContentAlignment _TextAlign = ContentAlignment.MiddleLeft;
        private bool _ThreeState = false;

        [DefaultValue(Appearance.Normal)]
        public Appearance Appearance
        {
            get => _Appearance;
            set { _Appearance = value; OnPropertyChanged(); }
        }
        [DefaultValue(true)]
        public bool AutoCheck
        {
            get => _AutoCheck;
            set { _AutoCheck = value; OnPropertyChanged(); }
        }
        [DefaultValue(false)]
        public bool AutoEllipsis
        {
            get => _AutoEllipsis;
            set { _AutoEllipsis = value; OnPropertyChanged(); }
        }
        [DefaultValue(false)]
        public bool AutoSize
        {
            get => _AutoSize;
            set { _AutoSize = true; OnPropertyChanged(); }
        }
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get => _CheckAlign;
            set { _CheckAlign = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceBorderColor
        {
            get => _FlatAppearanceBorderColor;
            set { _FlatAppearanceBorderColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(1)]
        public int FlatAppearanceBorderSize
        {
            get => _FlatAppearanceBorderSize;
            set { _FlatAppearanceBorderSize = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceCheckedBackColor
        {
            get => _FlatAppearanceCheckedBackColor;
            set { _FlatAppearanceCheckedBackColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceMouseDownBackColor
        {
            get => _FlatAppearanceMouseDownBackColor;
            set { _FlatAppearanceMouseDownBackColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(Color), "")]
        public Color FlatAppearanceMouseOverBackColor
        {
            get => _FlatAppearanceMouseOverBackColor;
            set { _FlatAppearanceMouseOverBackColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(FlatStyle.Standard)]
        public FlatStyle FlatStyle
        {
            get => _FlatStyle;
            set { _FlatStyle = value; OnPropertyChanged(); }
        }
        [DefaultValue(typeof(SystemColors), "ControlText")]
        public Color ForeColor
        {
            get => _ForeColor;
            set { _ForeColor = value; OnPropertyChanged(); }
        }
        [DefaultValue(RightToLeft.No)]
        public RightToLeft RightToLeft
        {
            get => _RightToLeft;
            set { _RightToLeft = value; OnPropertyChanged(); }
        }
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TextAlign
        {
            get => _TextAlign;
            set { _TextAlign = value; OnPropertyChanged(); }
        }
        [DefaultValue(false)]
        public bool ThreeState
        {
            get => _ThreeState;
            set { _ThreeState = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Called when any property changes.
        /// </summary>
        public event EventHandler PropertyChanged;

        protected void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}