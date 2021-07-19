using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AloysAdjustments.Common.UI.Controls
{
    public class DataGridEx : DataGrid
    {
        private readonly SortDescriptionCollection _defaultSorts;

        public DataGridEx()
        {
            _defaultSorts = new SortDescriptionCollection();
        }

        public void SetDefaultSorts(IEnumerable<SortDescription> sorts)
        {
            //TODO: add support for this
            if (_defaultSorts.Any())
                throw new NotSupportedException("Already added default sorts");

            foreach (var sort in sorts)
                _defaultSorts.Add(new SortDescription(sort.PropertyName, sort.Direction));
            ApplyDefaultSorts();
        }

        protected override void OnSorting(DataGridSortingEventArgs e)
        {
            CustomSort(e.Column);
        }
        
        private void ApplyDefaultSorts()
        {
            foreach (var sort in _defaultSorts)
                Items.SortDescriptions.Add(sort);
        }

        private void CustomSort(DataGridColumn column)
        {
            var path = column.SortMemberPath;
            if (String.IsNullOrEmpty(path))
                return;
            
            var newSort = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift;

            var direction = column.SortDirection == ListSortDirection.Ascending ? 
                ListSortDirection.Descending : ListSortDirection.Ascending;
            var sort = new SortDescription(path, direction);

            try
            {
                using (Items.DeferRefresh())
                {
                    if (newSort)
                    {
                        Items.SortDescriptions.Clear();
                        Items.SortDescriptions.Add(sort);

                        ApplyDefaultSorts();
                    }
                    else
                    {
                        var existingSortIdx = 0;
                        //check existing non-default sorts to see if we already sorted on this column
                        for (int i = 0; i < Items.SortDescriptions.Count - _defaultSorts.Count; i++)
                        {
                            if (Items.SortDescriptions[i].PropertyName == path)
                            {
                                existingSortIdx = i;
                                break;
                            }
                        }

                        if (existingSortIdx >= 0)
                            Items.SortDescriptions[existingSortIdx] = sort;
                        else
                        {
                            //insert before all the default sorts
                            Items.SortDescriptions.Insert(Items.SortDescriptions.Count - 1 - _defaultSorts.Count, sort);
                        }
                    }
                }

                column.SortDirection = direction;
            }
            catch (InvalidOperationException ex)
            {
                Items.SortDescriptions.Clear();
            }
        }
    }
}
