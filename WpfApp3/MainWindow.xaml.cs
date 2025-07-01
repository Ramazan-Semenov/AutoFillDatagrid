using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        private readonly UndoRedoStack _undoRedoStack = new UndoRedoStack();

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
            InitializeCommands();
        }
        private void InitializeCommands()
        {
            // Команды копирования/вставки
            CommandBinding copyCommand = new CommandBinding(ApplicationCommands.Copy, CopyCommandExecuted, CopyCommandCanExecute);
            CommandBinding pasteCommand = new CommandBinding(ApplicationCommands.Paste, PasteCommandExecuted, PasteCommandCanExecute);
            this.CommandBindings.Add(copyCommand);
            this.CommandBindings.Add(pasteCommand);

            // Команды Undo/Redo
            CommandBinding undoCommand = new CommandBinding(ApplicationCommands.Undo, UndoCommandExecuted, UndoCommandCanExecute);
            CommandBinding redoCommand = new CommandBinding(ApplicationCommands.Redo, RedoCommandExecuted, RedoCommandCanExecute);
            this.CommandBindings.Add(undoCommand);
            this.CommandBindings.Add(redoCommand);
        }

        private void CopyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dataGrid.SelectedCells.Count > 0;
        }
        private void CopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedCells = dataGrid.SelectedCells;
            if (selectedCells.Count == 0) return;

            // Определяем границы выделенного диапазона
            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minCol = int.MaxValue, maxCol = int.MinValue;

            foreach (var cell in selectedCells)
            {
                int row = dataGrid.Items.IndexOf(cell.Item);
                int col = cell.Column.DisplayIndex;

                minRow = Math.Min(minRow, row);
                maxRow = Math.Max(maxRow, row);
                minCol = Math.Min(minCol, col);
                maxCol = Math.Max(maxCol, col);
            }

            // Собираем данные в формате, аналогичном Excel (табуляция между столбцами, новая строка между строками)
            StringBuilder sb = new StringBuilder();

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (col > minCol) sb.Append('\t');

                    // Проверяем, есть ли ячейка в выделении
                    var cellInfo = selectedCells.FirstOrDefault(c =>
                        dataGrid.Items.IndexOf(c.Item) == row &&
                        c.Column.DisplayIndex == col);

                    if (cellInfo != null)
                    {
                        var propertyInfo = cellInfo.Item.GetType().GetProperty(cellInfo.Column.SortMemberPath);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(cellInfo.Item, null);
                            sb.Append(value?.ToString() ?? "");
                        }
                    }
                }
                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
        }
        private void PasteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText() && dataGrid.SelectedCells.Count > 0;
        }

        private void PasteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Clipboard.ContainsText()) return;

            string clipboardText = Clipboard.GetText();
            var startCell = dataGrid.SelectedCells[0];
            int startRow = dataGrid.Items.IndexOf(startCell.Item);
            int startCol = startCell.Column.DisplayIndex;

            var pasteCommand = new PasteCommand(dataGrid, startRow, startCol, clipboardText);
            _undoRedoStack.Execute(pasteCommand);
        }

        private void UndoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _undoRedoStack.CanUndo;
        }

        private void UndoCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _undoRedoStack.Undo();
        }

        private void RedoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _undoRedoStack.CanRedo;
        }

        private void RedoCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _undoRedoStack.Redo();
        }
        private DataGridCell _autoFillStartCell;




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

    public abstract class Command
    {
        public abstract void Execute();
        public abstract void UnExecute();
    }

    public class UndoRedoStack
    {
        private readonly Stack<Command> _undoStack = new Stack<Command>();
        private readonly Stack<Command> _redoStack = new Stack<Command>();

        public void Execute(Command command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.UnExecute();
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }

    public class PasteCommand : Command
    {
        private readonly DataGrid _dataGrid;
        private readonly int _startRow;
        private readonly int _startCol;
        private readonly string _clipboardText;
        private List<CellChange> _changes = new List<CellChange>();

        public PasteCommand(DataGrid dataGrid, int startRow, int startCol, string clipboardText)
        {
            _dataGrid = dataGrid;
            _startRow = startRow;
            _startCol = startCol;
            _clipboardText = clipboardText;
        }

        public override void Execute()
        {
            if (_changes.Count == 0) // Первое выполнение
            {
                PerformPaste();
            }
            else // Повторное выполнение (Redo)
            {
                ApplyChanges();
            }
        }

        public override void UnExecute()
        {
            RevertChanges();
        }

        private void PerformPaste()
        {
            string[] rows = _clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int r = 0; r < rows.Length; r++)
            {
                if (string.IsNullOrEmpty(rows[r])) continue;

                string[] cells = rows[r].Split('\t');
                for (int c = 0; c < cells.Length; c++)
                {
                    int targetRow = _startRow + r;
                    int targetCol = _startCol + c;

                    if (targetRow >= _dataGrid.Items.Count || targetCol >= _dataGrid.Columns.Count)
                        continue;

                    var item = _dataGrid.Items[targetRow];
                    var column = _dataGrid.Columns[targetCol] as DataGridBoundColumn;

                    if (column != null)
                    {
                        var binding = column.Binding as Binding;
                        if (binding != null)
                        {
                            string propertyName = binding.Path.Path;
                            var propertyInfo = item.GetType().GetProperty(propertyName);

                            if (propertyInfo != null && propertyInfo.CanWrite)
                            {
                                // Сохраняем старое значение
                                object oldValue = propertyInfo.GetValue(item, null);

                                try
                                {
                                    object newValue = Convert.ChangeType(cells[c], propertyInfo.PropertyType);
                                    _changes.Add(new CellChange
                                    {
                                        Row = targetRow,
                                        Column = targetCol,
                                        PropertyName = propertyName,
                                        OldValue = oldValue,
                                        NewValue = newValue
                                    });

                                    propertyInfo.SetValue(item, newValue, null);
                                }
                                catch
                                {
                                    // Ошибка преобразования - пропускаем ячейку
                                }
                            }
                        }
                    }
                }
            }

            _dataGrid.Items.Refresh();
        }

        private void ApplyChanges()
        {
            foreach (var change in _changes)
            {
                if (change.Row >= _dataGrid.Items.Count) continue;

                var item = _dataGrid.Items[change.Row];
                var propertyInfo = item.GetType().GetProperty(change.PropertyName);

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(item, change.NewValue, null);
                }
            }
            _dataGrid.Items.Refresh();
        }

        private void RevertChanges()
        {
            foreach (var change in _changes)
            {
                if (change.Row >= _dataGrid.Items.Count) continue;

                var item = _dataGrid.Items[change.Row];
                var propertyInfo = item.GetType().GetProperty(change.PropertyName);

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(item, change.OldValue, null);
                }
            }
            _dataGrid.Items.Refresh();
        }

        private class CellChange
        {
            public int Row { get; set; }
            public int Column { get; set; }
            public string PropertyName { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
        }
    }
}
