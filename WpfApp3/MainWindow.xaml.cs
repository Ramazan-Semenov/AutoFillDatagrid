using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var items = new List<DataItem>();
            for (int i = 0; i < 20; i++)
            {
                items.Add(new DataItem
                {
                    Property1 = $"Value {i}-1",
                    Property2 = $"Value {i}-2",
                    Property3 = $"Value {i}-3"
                });
            }

            dataGrid.ItemsSource = items;
        }

        //private Point _dragStartPoint;
        //private bool _isDragging;
        //private DataGridCell _dragStartCell;

        //private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var cell = GetCellUnderMouse(e.GetPosition(dataGrid));
        //    if (cell != null)
        //    {
        //        _dragStartPoint = e.GetPosition(dataGrid);
        //        _dragStartCell = cell;
        //        _isDragging = false;
        //    }
        //}

        //private void DataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed && _dragStartCell != null)
        //    {
        //        var position = e.GetPosition(dataGrid);
        //        if (Math.Abs(position.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
        //            Math.Abs(position.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
        //        {
        //            _isDragging = true;
        //        }
        //    }
        //}

        //private void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (_isDragging && _dragStartCell != null)
        //    {
        //        var endCell = GetCellUnderMouse(e.GetPosition(dataGrid));
        //        if (endCell != null && endCell != _dragStartCell)
        //        {
        //            AutoFillCells(_dragStartCell, endCell);
        //        }
        //    }

        //    _isDragging = false;
        //    _dragStartCell = null;
        //}

        //private DataGridCell GetCellUnderMouse(Point position)
        //{
        //    var hitTest = VisualTreeHelper.HitTest(dataGrid, position);
        //    if (hitTest == null) return null;

        //    var cell = hitTest.VisualHit;
        //    while (cell != null && !(cell is DataGridCell))
        //    {
        //        cell = VisualTreeHelper.GetParent(cell);
        //    }

        //    return cell as DataGridCell;
        //}

        //private void AutoFillCells(DataGridCell startCell, DataGridCell endCell)
        //{
        //    int startRow = dataGrid.Items.IndexOf(startCell.DataContext);
        //    int endRow = dataGrid.Items.IndexOf(endCell.DataContext);
        //    int startCol = startCell.Column.DisplayIndex;
        //    int endCol = endCell.Column.DisplayIndex;

        //    // Get the source value to copy
        //    object sourceValue = GetCellValue(startCell);

        //    // Fill all cells in the range with the source value
        //    if (startRow == endRow) // Fill horizontally
        //    {
        //        int minCol = Math.Min(startCol, endCol);
        //        int maxCol = Math.Max(startCol, endCol);

        //        for (int col = minCol; col <= maxCol; col++)
        //        {
        //            var cell = GetCell(startRow, col);
        //            if (cell != null) SetCellValue(cell, sourceValue);
        //        }
        //    }
        //    else if (startCol == endCol) // Fill vertically
        //    {
        //        int minRow = Math.Min(startRow, endRow);
        //        int maxRow = Math.Max(startRow, endRow);

        //        for (int row = minRow; row <= maxRow; row++)
        //        {
        //            var cell = GetCell(row, startCol);
        //            if (cell != null) SetCellValue(cell, sourceValue);
        //        }
        //    }
        //}

        //private DataGridCell GetCell(int row, int col)
        //{
        //    if (row < 0 || row >= dataGrid.Items.Count) return null;
        //    if (col < 0 || col >= dataGrid.Columns.Count) return null;

        //    var rowContainer = dataGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;
        //    if (rowContainer == null) return null;

        //    var presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
        //    if (presenter == null) return null;

        //    var cell = presenter.ItemContainerGenerator.ContainerFromIndex(col) as DataGridCell;
        //    return cell;
        //}

        //private object GetCellValue(DataGridCell cell)
        //{
        //    if (cell.Content is TextBlock textBlock)
        //    {
        //        return textBlock.Text;
        //    }
        //    return cell.Content;
        //}

        //private void SetCellValue(DataGridCell cell, object value)
        //{
        //    var column = cell.Column as DataGridBoundColumn;
        //    if (column != null)
        //    {
        //        var binding = column.Binding as Binding;
        //        if (binding != null)
        //        {
        //            var propertyName = binding.Path.Path;
        //            var item = cell.DataContext;
        //            var property = item.GetType().GetProperty(propertyName);
        //            if (property != null)
        //            {
        //                try
        //                {
        //                    property.SetValue(item, Convert.ChangeType(value, property.PropertyType), null);
        //                }
        //                catch
        //                {
        //                    // Handle conversion errors if needed
        //                }
        //            }
        //        }
        //    }
        //}

        //private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(obj, i);
        //        if (child != null && child is T)
        //            return (T)child;
        //        else
        //        {
        //            var childOfChild = FindVisualChild<T>(child);
        //            if (childOfChild != null)
        //                return childOfChild;
        //        }
        //    }
        //    return null;
        //}
        private DataGridCell _autoFillStartCell;

        private void AutoFillThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null) return;

            // Get the parent cell of the thumb
            var startCell = FindParent<DataGridCell>(thumb);
            if (startCell == null) return;

            // Store the starting cell if this is the first drag event
            if (_autoFillStartCell == null)
            {
                _autoFillStartCell = startCell;
            }

            // Get the cell under the current mouse position
            var mousePos = Mouse.GetPosition(dataGrid);
            var endCell = GetCellUnderMouse(mousePos);
            if (endCell == null) return;

            // Highlight the potential fill range (visual feedback)
            ClearTempSelection();
            HighlightFillRange(_autoFillStartCell, endCell);
        }

        private void AutoFillThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null || _autoFillStartCell == null) return;

            // Get the ending cell
            var mousePos = Mouse.GetPosition(dataGrid);
            var endCell = GetCellUnderMouse(mousePos);

            if (endCell != null)
            {
                // Perform the autofill
                ExecuteAutoFill(_autoFillStartCell, endCell);
            }

            // Clean up
            ClearTempSelection();
            _autoFillStartCell = null;
        }

        private void ExecuteAutoFill(DataGridCell startCell, DataGridCell endCell)
        {
            object sourceValue = GetCellValue(startCell);

            int startRow = dataGrid.Items.IndexOf(startCell.DataContext);
            int endRow = dataGrid.Items.IndexOf(endCell.DataContext);
            int col = startCell.Column.DisplayIndex;

            // Determine fill direction
            bool fillDown = endRow > startRow;

            if (fillDown)
            {
                // Fill downward
                for (int row = startRow + 1; row <= endRow; row++)
                {
                    var cell = GetCell(row, col);
                    if (cell != null)
                    {
                        SetCellValue(cell, sourceValue);
                    }
                }
            }
            else
            {
                // Fill upward
                for (int row = startRow - 1; row >= endRow; row--)
                {
                    var cell = GetCell(row, col);
                    if (cell != null)
                    {
                        SetCellValue(cell, sourceValue);
                    }
                }
            }
        }

        private void HighlightFillRange(DataGridCell startCell, DataGridCell endCell)
        {
            int startRow = dataGrid.Items.IndexOf(startCell.DataContext);
            int endRow = dataGrid.Items.IndexOf(endCell.DataContext);
            int col = startCell.Column.DisplayIndex;

            // Determine fill direction
            bool fillDown = endRow > startRow;

            if (fillDown)
            {
                // Highlight downward
                for (int row = startRow; row <= endRow; row++)
                {
                    var cell = GetCell(row, col);
                    if (cell != null)
                    {
                        cell.Background = Brushes.LightBlue;
                    }
                }
            }
            else
            {
                // Highlight upward
                for (int row = startRow; row >= endRow; row--)
                {
                    var cell = GetCell(row, col);
                    if (cell != null)
                    {
                        cell.Background = Brushes.LightBlue;
                    }
                }
            }
        }

        private void ClearTempSelection()
        {
            foreach (var item in dataGrid.Items)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    var presenter = FindVisualChild<DataGridCellsPresenter>(row);
                    if (presenter != null)
                    {
                        for (int i = 0; i < presenter.Items.Count; i++)
                        {
                            var cell = presenter.ItemContainerGenerator.ContainerFromIndex(i) as DataGridCell;
                            if (cell != null && cell.Background == Brushes.LightBlue)
                            {
                                cell.ClearValue(DataGridCell.BackgroundProperty);
                            }
                        }
                    }
                }
            }
        }

        #region Helper Methods

        private DataGridCell GetCellUnderMouse(Point position)
        {
            var hitTest = VisualTreeHelper.HitTest(dataGrid, position);
            if (hitTest == null) return null;

            return FindParent<DataGridCell>(hitTest.VisualHit);
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        private DataGridCell GetCell(int row, int col)
        {
            if (row < 0 || row >= dataGrid.Items.Count) return null;
            if (col < 0 || col >= dataGrid.Columns.Count) return null;

            var rowContainer = dataGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;
            if (rowContainer == null) return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
            if (presenter == null) return null;

            return presenter.ItemContainerGenerator.ContainerFromIndex(col) as DataGridCell;
        }

        private object GetCellValue(DataGridCell cell)
        {
            if (cell.Content is TextBlock textBlock)
            {
                return textBlock.Text;
            }
            return cell.Content;
        }

        private void SetCellValue(DataGridCell cell, object value)
        {
            var column = cell.Column as DataGridBoundColumn;
            if (column != null)
            {
                var binding = column.Binding as Binding;
                if (binding != null)
                {
                    var propertyName = binding.Path.Path;
                    var item = cell.DataContext;
                    var property = item.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        try
                        {
                            property.SetValue(item, Convert.ChangeType(value, property.PropertyType), null);
                        }
                        catch
                        {
                            // Handle conversion errors if needed
                        }
                    }
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        #endregion
        //  private DataGridCell _autoFillStartCell;
        private Thumb _currentThumb;

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell == null) return;

            // Find the thumb in the cell template
            var thumb = FindVisualChild<Thumb>(cell);
            if (thumb == null) return;

            // Check if click was on the thumb (bottom-right 8x8 area)
            var pos = e.GetPosition(cell);
            if (pos.X < cell.ActualWidth - 8 || pos.Y < cell.ActualHeight - 8)
            {
                // Click was not on the thumb
                return;
            }

            _autoFillStartCell = cell;
            _currentThumb = thumb;

            // Capture mouse and handle drag events
            thumb.CaptureMouse();
            thumb.MouseMove += AutoFillThumb_MouseMove;
            thumb.LostMouseCapture += AutoFillThumb_LostMouseCapture;
            thumb.PreviewMouseLeftButtonUp += AutoFillThumb_PreviewMouseLeftButtonUp;

            e.Handled = true;
        }

        private void AutoFillThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentThumb == null || _autoFillStartCell == null) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the cell under the current mouse position
                var mousePos = e.GetPosition(dataGrid);
                var endCell = GetCellUnderMouse(mousePos);
                if (endCell == null) return;

                // Highlight the potential fill range (visual feedback)
                ClearTempSelection();
                HighlightFillRange(_autoFillStartCell, endCell);
            }
        }

        private void AutoFillThumb_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CompleteAutoFill();
        }

        private void AutoFillThumb_LostMouseCapture(object sender, MouseEventArgs e)
        {
            CompleteAutoFill();
        }

        private void CompleteAutoFill()
        {
            if (_currentThumb == null || _autoFillStartCell == null) return;

            // Get the ending cell
            var mousePos = Mouse.GetPosition(dataGrid);
            var endCell = GetCellUnderMouse(mousePos);

            if (endCell != null)
            {
                // Perform the autofill
                ExecuteAutoFill(_autoFillStartCell, endCell);
            }

            // Clean up
            ClearTempSelection();
            CleanupThumbEvents();
            _autoFillStartCell = null;
            _currentThumb = null;
        }

        private void CleanupThumbEvents()
        {
            if (_currentThumb != null)
            {
                _currentThumb.MouseMove -= AutoFillThumb_MouseMove;
                _currentThumb.LostMouseCapture -= AutoFillThumb_LostMouseCapture;
                _currentThumb.PreviewMouseLeftButtonUp -= AutoFillThumb_PreviewMouseLeftButtonUp;
            }
        }

        // Остальные методы (ExecuteAutoFill, HighlightFillRange, ClearTempSelection, и helper methods) 
        // остаются такими же, как в предыдущем примере
    }

    public class DataItem : INotifyPropertyChanged
    {
        private string property1;
        private string property2;
        private string property3;

        public string Property1 { get => property1; set { property1 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Property1))); } }
        public string Property2 { get => property2; set { property2 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Property2))); } }
        public string Property3 { get => property3; set { property3 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Property3))); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }




}
