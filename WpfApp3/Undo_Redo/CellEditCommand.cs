using System.Windows.Controls;

namespace WpfApp3.Undo_Redo
{
    public class CellEditCommand : IUndoableCommand
    {
        private readonly DataGrid _dataGrid;
        private readonly int _rowIndex;
        private readonly int _columnIndex;
        private readonly object _oldValue;
        private readonly object _newValue;

        public CellEditCommand(DataGrid dataGrid, int rowIndex, int columnIndex, object oldValue, object newValue)
        {
            _dataGrid = dataGrid;
            _rowIndex = rowIndex;
            _columnIndex = columnIndex;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public void Execute()
        {
            ApplyValue(_newValue);
        }

        public void Undo()
        {
            ApplyValue(_oldValue);
        }

        public void Redo()
        {
            ApplyValue(_newValue);
        }

        private void ApplyValue(object value)
        {
            if (_dataGrid.Items.Count <= _rowIndex) return;

            var row = _dataGrid.Items[_rowIndex];
            var propertyName = _dataGrid.Columns[_columnIndex].SortMemberPath;

            if (string.IsNullOrEmpty(propertyName)) return;

            var property = row.GetType().GetProperty(propertyName);
            property?.SetValue(row, value);
        }
    }
}
