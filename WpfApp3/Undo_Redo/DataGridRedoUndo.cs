using System.Windows.Controls;

namespace WpfApp3.Undo_Redo
{
    internal class DataGridRedoUndo
    {
        private readonly UndoRedoStack _undoRedoStack = new UndoRedoStack();
        private DataGridCellInfo _currentCell;
        private object _currentCellOldValue;
        private DataGrid dataGrid;
        public DataGridRedoUndo(DataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
            dataGrid.BeginningEdit += DataGrid_BeginningEdit;
            dataGrid.CellEditEnding += DataGrid_CellEditEnding;
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _currentCell = new DataGridCellInfo(e.Row.Item, e.Column);
            _currentCellOldValue = GetCellValue(_currentCell);
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var newValue = GetCellValue(new DataGridCellInfo(e.Row.Item, e.Column));

            if (!Equals(_currentCellOldValue, newValue))
            {
                var rowIndex = dataGrid.Items.IndexOf(e.Row.Item);
                var columnIndex = e.Column.DisplayIndex;
                var command = new CellEditCommand(dataGrid, rowIndex, columnIndex, _currentCellOldValue, newValue);
                _undoRedoStack.Execute(command);
            }
        }

        private object GetCellValue(DataGridCellInfo cell)
        {
            if (cell.Item == null) return null;

            var propertyName = cell.Column.SortMemberPath;
            if (string.IsNullOrEmpty(propertyName)) return null;

            var property = cell.Item.GetType().GetProperty(propertyName);
            return property?.GetValue(cell.Item);
        }

        public void UndoApply()
        {
            _undoRedoStack.Undo();
        }

        public void RedoApply()
        {
            _undoRedoStack.Redo();
        }
    }

}
