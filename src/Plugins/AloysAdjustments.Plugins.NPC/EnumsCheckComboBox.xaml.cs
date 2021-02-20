using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloysAdjustments.UI.Converters;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace AloysAdjustments.Plugins.NPC
{
    public partial class EnumsCheckComboBox : CheckComboBox
    {
        public static readonly DependencyProperty SelectedFlagsProperty = DependencyProperty.Register(nameof(SelectedFlags),
            typeof(Enum), typeof(EnumsCheckComboBox), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedFlagsChanged));

        private readonly MethodInfo _updateList;
        private bool _ignoreSelected;
        private bool _ignoreSelectedFlags;
        private bool _ignoreTextValueChanged;

        public Enum SelectedFlags
        {
            get => (Enum)GetValue(SelectedFlagsProperty);
            set => SetValue(SelectedFlagsProperty, value);
        }

        private Type _flagsType;
        public Type FlagsType
        {
            get => _flagsType;
            set
            {
                if (_flagsType == value)
                    return;
                _flagsType = value;
                OnFlagsChanged();
            }
        }

        private static void OnSelectedFlagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ecb = (EnumsCheckComboBox)d;

            if (ecb._ignoreSelectedFlags) return;
            ecb._ignoreSelectedFlags = true;

            ecb.SelectedItems.Clear();

            var flags = e.NewValue == null ? 0 : (int)e.NewValue;
            if (flags != 0)
            {
                foreach (var item in ecb.ItemsCollection)
                {
                    var obj = ecb.GetItemValue(item);
                    if (obj != null)
                    {
                        var itemValue = (int)obj;

                        if ((itemValue & flags) == itemValue)
                            ecb.SelectedItems.Add(item);
                    }
                }
            }

            ecb._ignoreSelectedFlags = false;
        }

        public EnumsCheckComboBox()
        {
            _updateList = typeof(Selector).GetMethod("UpdateFromList", BindingFlags.Instance | BindingFlags.NonPublic);

            InitializeComponent();
        }

        private void OnFlagsChanged()
        {
            if (FlagsType == null)
            {
                ItemsSource = null;
                return;
            }
            if (!FlagsType.IsEnum)
                throw new Exception($"Value must be an enum type: {FlagsType.Name}");

            ItemsSource = Enum.GetValues(FlagsType);
        }

        protected override void OnSelectedItemsCollectionChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnSelectedItemsCollectionChanged(sender, e);

            if (_ignoreSelected) return;
            if (FlagsType == null)
                return;

            _ignoreSelected = true;

            int flags = 0;

            if (SelectedItems != null)
            {
                foreach (var item in SelectedItems)
                    flags |= (int)GetItemValue(item);
            }

            SelectedFlags = (Enum)Enum.ToObject(FlagsType, flags);
            _ignoreSelected = false;
        }

        protected override void OnTextChanged(string oldValue, string newValue)
        {
            if (!IsInitialized || _ignoreTextValueChanged || !IsEditable)
                return;
            UpdateFromText();
        }

        protected override void UpdateText()
        {
            string str = string.Join(Delimiter, SelectedItems.Cast<object>().Select(GetEnumDisplayValue));
            if (!string.IsNullOrEmpty(Text) && Text.Equals(str))
                return;

            _ignoreTextValueChanged = true;
            SetCurrentValue(TextProperty, str);
            _ignoreTextValueChanged = false;
        }

        private void UpdateFromText()
        {
            List<string> selectedValues = null;
            if (!string.IsNullOrEmpty(Text))
            {
                selectedValues = Text.Replace(" ", string.Empty).Split(new string[1]
                {
                    Delimiter
                }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            _updateList.Invoke(this, new object[] { selectedValues, new Func<object, object>(GetEnumDisplayValue) });
        }

        private object GetEnumDisplayValue(object item)
        {
            var str = EnumConverter.GetEnumDescription((Enum)item);
            if (String.IsNullOrEmpty(str))
                return item;
            return str;
        }
    }
}
