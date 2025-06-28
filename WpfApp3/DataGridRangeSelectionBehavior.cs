using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp3
{
    public static class DataGridRangeSelectionBehavior
    {
        public static bool GetEnableRangeSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableRangeSelectionProperty);
        }

        public static void SetEnableRangeSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableRangeSelectionProperty, value);
        }

        public static readonly DependencyProperty EnableRangeSelectionProperty =
            DependencyProperty.RegisterAttached("EnableRangeSelection", typeof(bool),
            typeof(DataGridRangeSelectionBehavior), new PropertyMetadata(false, OnEnableRangeSelectionChanged));

        private static void OnEnableRangeSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dd)
            {
                dataGrid = dd;
                if ((bool)e.NewValue)
                {
                    dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
                    dataGrid.PreviewMouseLeftButtonUp += DataGrid_PreviewMouseLeftButtonUp;
                    dataGrid.PreviewMouseMove += DataGrid_PreviewMouseMove;
                }
                else
                {
                    dataGrid.PreviewMouseLeftButtonDown -= DataGrid_PreviewMouseLeftButtonDown;
                    dataGrid.PreviewMouseLeftButtonUp -= DataGrid_PreviewMouseLeftButtonUp;
                    dataGrid.PreviewMouseMove -= DataGrid_PreviewMouseMove;
                }
            }
        }

        static DataGrid dataGrid;
        // Implement the same event handlers as above, but static
        // ...


        private static Point _startPoint;
        private static bool _isSelecting;
        private static DataGridCell _startCell;

        private static void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the cell under mouse
            var cell = GetCellUnderMouse(e.GetPosition(dataGrid));
            if (cell != null)
            {
                _startPoint = e.GetPosition(dataGrid);
                _startCell = cell;
                _isSelecting = true;

                // Clear previous selection if not holding Ctrl
                if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                {
                    dataGrid.SelectedCells.Clear();
                }

                // Select the initial cell
                dataGrid.SelectedCells.Add(new DataGridCellInfo(cell));
            }
        }

        private static void DataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(dataGrid);

                // Check if mouse moved enough to consider it a drag
                if (Math.Abs(currentPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var endCell = GetCellUnderMouse(currentPoint);
                    if (endCell != null)
                    {
                        SelectRange(_startCell, endCell);
                    }
                }
            }
        }

        private static void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;
            _startCell = null;
        }

        private static void SelectRange(DataGridCell startCell, DataGridCell endCell)
        {
            int startRow = dataGrid.Items.IndexOf(startCell.DataContext);
            int endRow = dataGrid.Items.IndexOf(endCell.DataContext);
            int startCol = startCell.Column.DisplayIndex;
            int endCol = endCell.Column.DisplayIndex;

            // Determine the rectangle bounds
            int minRow = Math.Min(startRow, endRow);
            int maxRow = Math.Max(startRow, endRow);
            int minCol = Math.Min(startCol, endCol);
            int maxCol = Math.Max(startCol, endCol);

            // Clear selection if not holding Ctrl
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                dataGrid.SelectedCells.Clear();
            }

            // Select all cells in the range
            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    var cell = GetCell(row, col);
                    if (cell != null)
                    {
                        dataGrid.SelectedCells.Add(new DataGridCellInfo(cell));
                    }
                }
            }
        }

        private static DataGridCell GetCellUnderMouse(Point position)
        {
            var hitTest = VisualTreeHelper.HitTest(dataGrid, position);
            if (hitTest == null) return null;

            var cell = hitTest.VisualHit;
            while (cell != null && !(cell is DataGridCell))
            {
                cell = VisualTreeHelper.GetParent(cell);
            }

            return cell as DataGridCell;
        }

        private static DataGridCell GetCell(int row, int col)
        {
            if (row < 0 || row >= dataGrid.Items.Count) return null;
            if (col < 0 || col >= dataGrid.Columns.Count) return null;

            var rowContainer = dataGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;
            if (rowContainer == null) return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
            if (presenter == null) return null;

            var cell = presenter.ItemContainerGenerator.ContainerFromIndex(col) as DataGridCell;
            return cell;
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
    }
}
