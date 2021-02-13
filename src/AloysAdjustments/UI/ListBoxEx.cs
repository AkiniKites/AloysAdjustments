using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AloysAdjustments.UI
{
    public class ListBoxEx : ListBox
    {
        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList),
                typeof(ListBoxEx), new PropertyMetadata(null));

        public ListBoxEx()
        {
            SelectionChanged += ListBoxEx_SelectionChanged;
        }

        void ListBoxEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newList = new object[SelectedItems.Count];
            SelectedItems.CopyTo(newList, 0);
            SelectedItemsList = newList;
        }

        public IList SelectedItemsList
        {
            get => (IList)GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }
    }
}
