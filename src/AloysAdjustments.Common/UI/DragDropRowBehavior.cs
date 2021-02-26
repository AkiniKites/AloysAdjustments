using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace AloysAdjustments.Common.UI
{
    public class DragDropRowBehavior : Behavior<DataGrid>
    {
        private object _draggedItem;
        private FrameworkElement _draggedRow;
        private bool _isEditing;
        private bool _isDragging;

        public static readonly RoutedEvent DragEndedEvent =
            EventManager.RegisterRoutedEvent("DragEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DragDropRowBehavior));
        public static void AddDragEndedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            if (d is UIElement uie)
                uie.AddHandler(DragEndedEvent, handler);
        }
        public static void RemoveDragEndedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            if (d is UIElement uie)
                uie.RemoveHandler(DragEndedEvent, handler);
        }

        private void RaiseDragEndedEvent()
        {
            var args = new RoutedEventArgs(DragEndedEvent);
            AssociatedObject.RaiseEvent(args);
        }

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register("Popup", typeof(Popup), typeof(DragDropRowBehavior));
        public Popup Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.BeginningEdit += OnBeginEdit;
            AssociatedObject.CellEditEnding += OnEndEdit;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.BeginningEdit -= OnBeginEdit;
            AssociatedObject.CellEditEnding -= OnEndEdit;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;

            Popup = null;
            _draggedItem = null;
            _isEditing = false;
            _isDragging = false;
        }

        private void OnBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _isEditing = true;
            //in case we are in the middle of a drag/drop operation, cancel it...
            if (_isDragging) ResetDragDrop();
        }

        private void OnEndEdit(object sender, DataGridCellEditEndingEventArgs e)
        {
            _isEditing = false;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isEditing) return;

            var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(AssociatedObject));
            if (row == null || row.IsEditing) return;

            //set flag that indicates we're capturing mouse movements
            _isDragging = true;
            _draggedItem = row.Item;
            _draggedRow = row;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging || _isEditing)
                return;

            //get the target item
            var targetItem = AssociatedObject.SelectedItem;

            if (targetItem == null || !ReferenceEquals(_draggedItem, targetItem))
            {
                //get target index
                var targetIndex = ((AssociatedObject).ItemsSource as IList).IndexOf(targetItem);

                //remove the source from the list
                ((AssociatedObject).ItemsSource as IList).Remove(_draggedItem);

                //move source at the target's location
                ((AssociatedObject).ItemsSource as IList).Insert(targetIndex, _draggedItem);

                //select the dropped item
                AssociatedObject.SelectedItem = _draggedItem;
                RaiseDragEndedEvent();
            }

            //reset
            ResetDragDrop();
        }

        private void ResetDragDrop()
        {
            _isDragging = false;
            Popup.IsOpen = false;
            AssociatedObject.IsReadOnly = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
                return;

            Popup.DataContext = _draggedItem;
            if (!Popup.IsOpen)
            {
                AssociatedObject.IsReadOnly = true;
                Popup.Child = new Image() { Source = UIHelpers.ToImage(_draggedRow) };
                Popup.IsOpen = true;
            }

            var popupSize = new Size(Popup.ActualWidth, Popup.ActualHeight);
            Popup.PlacementRectangle = new Rect(e.GetPosition(AssociatedObject), popupSize);

            //make sure the row under the grid is being selected
            var position = e.GetPosition(AssociatedObject);
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(AssociatedObject, position);
            if (row != null) AssociatedObject.SelectedItem = row.Item;
        }
    }
}
