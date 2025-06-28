namespace WpfApp3
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    public class DataGridDragCopyAdorner : Adorner
    {
        private readonly VisualBrush _visualBrush;
        private Point _position;
        private readonly double _cellWidth;
        private readonly double _cellHeight;
        private readonly int _rowCount;
        private readonly int _columnCount;

        public DataGridDragCopyAdorner(UIElement adornedElement,
                                     DataGridCell sourceCell,
                                     int rowCount,
                                     int columnCount,
                                     double cellWidth,
                                     double cellHeight)
            : base(adornedElement)
        {
            _rowCount = rowCount;
            _columnCount = columnCount;
            _cellWidth = cellWidth;
            _cellHeight = cellHeight;

            // Создаем визуальное представление для переноса
            var border = new Border
            {
                Background = SystemColors.HighlightBrush,
                Opacity = 0.7,
                Child = new TextBlock
                {
                    Text = sourceCell.Content?.ToString(),
                    Margin = new Thickness(4)
                }
            };

            _visualBrush = new VisualBrush(border)
            {
                Stretch = Stretch.None
            };

            IsHitTestVisible = false;
        }

        public void UpdatePosition(Point position)
        {
            _position = position;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Рисуем прямоугольник для каждой ячейки в выделенной области
            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    var rect = new Rect(
                        _position.X + col * _cellWidth,
                        _position.Y + row * _cellHeight,
                        _cellWidth,
                        _cellHeight);

                    drawingContext.DrawRectangle(_visualBrush, null, rect);
                }
            }
        }
    }

    public static class DataGridDragCopyBehavior
    {
        private static DataGrid _dataGrid;
        private static DataGridCell _sourceCell;
        private static Point _dragStartPoint;
        private static bool _isDragging;
        private static DataGridDragCopyAdorner _adorner;
        private static AdornerLayer _adornerLayer;
        private static int _rowCount = 1;
        private static int _columnCount = 1;

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DataGridDragCopyBehavior),
                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                _dataGrid = dataGrid;

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

        private static void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hitTestResult = VisualTreeHelper.HitTest(_dataGrid, e.GetPosition(_dataGrid));
            if (hitTestResult == null) return;

            _sourceCell = FindParent<DataGridCell>(hitTestResult.VisualHit);
            if (_sourceCell == null || _sourceCell.IsEditing || _sourceCell.IsReadOnly) return;

            _dragStartPoint = e.GetPosition(_dataGrid);
            _isDragging = false;
        }

        private static void DataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_sourceCell == null || e.LeftButton != MouseButtonState.Pressed) return;

            var currentPosition = e.GetPosition(_dataGrid);
            var diff = _dragStartPoint - currentPosition;

            if (!_isDragging && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                StartDrag(currentPosition);
            }

            if (_isDragging && _adorner != null)
            {
                _adorner.UpdatePosition(currentPosition);
            }
        }

        private static void StartDrag(Point currentPosition)
        {
            _isDragging = true;

            // Определяем размеры ячейки
            var cellWidth = _sourceCell.ActualWidth;
            var cellHeight = _sourceCell.ActualHeight;

            // Создаем и показываем Adorner
            _adornerLayer = AdornerLayer.GetAdornerLayer(_dataGrid);
            _adorner = new DataGridDragCopyAdorner(_dataGrid, _sourceCell, _rowCount, _columnCount, cellWidth, cellHeight);
            _adornerLayer.Add(_adorner);

            // Захватываем мышь
            _dataGrid.CaptureMouse();
            _dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
        }

        private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isDragging)
            {
                CleanupDrag();
            }
        }

        private static void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
            {
                CleanupDrag();
                return;
            }

            try
            {
                var currentPosition = e.GetPosition(_dataGrid);
                var hitTestResult = VisualTreeHelper.HitTest(_dataGrid, currentPosition);
                if (hitTestResult == null) return;

                var targetCell = FindParent<DataGridCell>(hitTestResult.VisualHit);
                if (targetCell != null && !targetCell.IsEditing && !targetCell.IsReadOnly)
                {
                    // Копируем значение в целевые ячейки
                    var sourceValue = _sourceCell.Content;
                    var targetRow = "sdf";
                    var targetColumn = "targetCell.GetValue(DataGrid.ColumnProperty)";

                    // Здесь можно реализовать копирование в несколько ячеек
                    // В этом примере копируем только в одну целевую ячейку
                    targetCell.Content = sourceValue;

                    // Если нужно обновить привязанные данные:
                    if (targetCell.IsEditing)
                    {
                        targetCell.GetBindingExpression(DataGridCell.ContentProperty)?.UpdateSource();
                    }
                }
            }
            finally
            {
                CleanupDrag();
            }
        }

        private static void CleanupDrag()
        {
            if (_adornerLayer != null && _adorner != null)
            {
                _adornerLayer.Remove(_adorner);
                _adorner = null;
                _adornerLayer = null;
            }

            if (_dataGrid != null)
            {
                _dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
                _dataGrid.ReleaseMouseCapture();
            }

            _sourceCell = null;
            _isDragging = false;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }
    }
}
