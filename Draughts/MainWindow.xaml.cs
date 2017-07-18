using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Draughts {
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private ObservableCollection<Piece> Pieces;
        private const int Size = 8;

        public MainWindow() {
            Pieces = new ObservableCollection<Piece>();
            InitializeComponent();
            DataContext = Pieces;
            NewGame();
        }

        private void NewGame() {
            Pieces.Clear();
            GeneratePieces();
        }

        private void GeneratePieces() {
            bool isBlack = true;
            for (int i = 0; i < Size; i++) {
                Enumerable.Range(0, Size).Where(j => j % 2 != i % 2).Select(x => new Piece() {
                    Row = i,
                    Column = x,
                    IsBlack = isBlack,
                    Type = PieceTypes.Pawn
                }).ToList().ForEach(Pieces.Add);
                if (i == 2) {
                    i = 4;
                    isBlack = false;
                }
            }
        }

        private Point startPoint;
        private void Rectangle_PreviewMouseMove(object sender, MouseEventArgs e) {
            startPoint = e.GetPosition(null);
        }

        private Point cellToRemove;
        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed
                //&& (System.Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                //System.Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                ) {
                cellToRemove = GetGridsCellAtCursorsPosition((Grid)sender);
                Image image = e.OriginalSource as Image;

                DataObject data = new DataObject(typeof(ImageSource), image.Source);

                DragDrop.DoDragDrop(image, data, DragDropEffects.All);
            }
        }

        private void Rectangle_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(ImageSource))) {
                Point cell = GetGridsCellAtCursorsPosition((Rectangle)sender);
                Piece toRemove = Pieces.SingleOrDefault(p => { return p.Row == cellToRemove.X && p.Column == cellToRemove.Y; });
                if (IfDropPossible(toRemove, cell)) {
                    Pieces.Add(new Piece() { Row = (int)cell.X, Column = (int)cell.Y, IsBlack = toRemove.IsBlack, Type = PieceTypes.Pawn });
                    Pieces.Remove(toRemove);
                }
            }
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(typeof(ImageSource)) || sender == e.Source) {
                e.Effects = DragDropEffects.None;
            }
        }

    //    private static T FindAnchestor<T>(DependencyObject current)
    //where T : DependencyObject {
    //        do {
    //            if (current is T) {
    //                return (T)current;
    //            }
    //            current = VisualTreeHelper.GetParent(current);
    //        }
    //        while (current != null);
    //        return null;
    //    }

        private Point GetGridsCellAtCursorsPosition(Rectangle sender) {
            int x = uniformGrid.Children.IndexOf(sender);
            int elementRow = x / Size;
            int elementCol = x % Size;
            return new Point(elementRow, elementCol);
        }

        private Point GetGridsCellAtCursorsPosition(Grid sender) {
            var point = Mouse.GetPosition(sender);

            int row = 0;
            int col = 0;
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;

            foreach (var rowDefinition in sender.RowDefinitions) {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y)
                    break;
                row++;
            }

            foreach (var columnDefinition in sender.ColumnDefinitions) {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X)
                    break;
                col++;
            }
            return new Point(row, col);
        }

        private bool IfDropPossible(Piece start, Point end) {
            bool ret = false;
            if(start.Type == PieceTypes.Pawn) {
                if (Math.Abs(start.Row - end.X) <= 1 && Math.Abs(start.Column - end.Y) <= 1)
                    ret = true;
            }
            else if(start.Type == PieceTypes.Dame) {
                if (Math.Abs(start.Row - end.X) == Math.Abs(start.Column - end.Y))
                    ret = true;
            }
            return ret;
        }
    }
}
