using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfApp3
{
    public class DataGridSelectionAdorner : Adorner
    {
        private readonly DataGrid datagrid;
        private readonly SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0));
        readonly Pen pen = new Pen(new SolidColorBrush(Colors.Black), 1);
        private readonly Dictionary<DataGridCellInfo, int[]> cellInfoToTableRowAndColumn = new Dictionary<DataGridCellInfo, int[]>();

        public DataGridSelectionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            datagrid = (DataGrid)adornedElement;
            pen.DashStyle = new DashStyle(new[] { 3.0, 3.0 }, 0);
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            ItemContainerGenerator generator = datagrid.ItemContainerGenerator;
            IEnumerable<int> rows =
                    datagrid.SelectedCells.Select(c =>
                        generator.IndexFromContainer(
                            generator.ContainerFromItem(c.Item)
                        )
                    );
            IEnumerable<int> columns = datagrid.SelectedCells.Select(
                c => c.Column.DisplayIndex
            );
            int minRow = rows.Min();
            int maxRow = rows.Max();
            int minColumn = columns.Min();
            int maxColumn = columns.Max();

            foreach (var cell in datagrid.SelectedCells)
            {
                int row = generator.IndexFromContainer(generator.ContainerFromItem(cell.Item));
                int column = cell.Column.DisplayIndex;
                cellInfoToTableRowAndColumn[cell] = new[] { row, column };
            }

            var topLeft = cellInfoToTableRowAndColumn.First(c => c.Value[0] == minRow && c.Value[1] == minColumn).Key;
            var bottomRight = cellInfoToTableRowAndColumn.First(c => c.Value[0] == maxRow && c.Value[1] == maxColumn).Key;

            var topLeftCell = GetDataGridCell(topLeft);
            var bottomRightCell = GetDataGridCell(bottomRight);

            const double marginX = 4.5;
            const double marginY = 3.5;
            Point topLeftPoint = topLeftCell.TranslatePoint(new Point(marginX, marginY), datagrid);
            Point bottomRightPoint = bottomRightCell.TranslatePoint(
                new Point(bottomRightCell.RenderSize.Width - marginX, bottomRightCell.RenderSize.Height - marginY),
                datagrid
            );

            drawingContext.DrawRectangle(backgroundBrush, pen, new Rect(topLeftPoint, bottomRightPoint));
        }

        private static DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
                return (DataGridCell)cellContent.Parent;
            return null;
        }
    }
}
