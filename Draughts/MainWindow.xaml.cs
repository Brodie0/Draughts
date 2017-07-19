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
            GeneratePiecesInRows(0, 3, ColorTypes.Black, PieceTypes.Pawn, SideTypes.Top);
            GeneratePiecesInRows(Size - 3, Size, ColorTypes.White, PieceTypes.Pawn, SideTypes.Bottom);
        }

        private void GeneratePiecesInRows(int start, int end, ColorTypes color, PieceTypes type, SideTypes side) {
            for (int i = start; i < end; i++) {
                Enumerable.Range(0, Size).Where(j => j % 2 != i % 2).Select(x => new Piece() {
                    Row = i,
                    Column = x,
                    Color = color,
                    Type = type,
                    Side = side
                }).ToList().ForEach(Pieces.Add);
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
                Point endCellCoor = GetGridsCellAtCursorsPosition((Rectangle)sender);
                Piece startCell = Pieces.SingleOrDefault(p => { return p.Row == cellToRemove.X && p.Column == cellToRemove.Y; });
                if (IfDropPossible(startCell, endCellCoor)) {
                    if (IfOthersideCell(endCellCoor, startCell))
                        startCell.Type = PieceTypes.Dame;
                    Pieces.Add(new Piece() { Row = (int)endCellCoor.X, Column = (int)endCellCoor.Y, Color = startCell.Color, Type = startCell.Type, Side = startCell.Side });
                    if(IfCapture(startCell, endCellCoor)) {
                        Point pointToCapture = PointToCapture(startCell, endCellCoor);
                        Piece pieceToCapture = Pieces.SingleOrDefault(p => { return p.Row == pointToCapture.X && p.Column == pointToCapture.Y; });
                        Pieces.Remove(pieceToCapture);
                    }                       
                    Pieces.Remove(startCell);
                }
            }
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(typeof(ImageSource)) || sender == e.Source) {
                e.Effects = DragDropEffects.None;
            }
        }

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
                if ((end.X - start.Row == 1 && start.Side == SideTypes.Top || start.Row - end.X == 1 && start.Side == SideTypes.Bottom) && Math.Abs(start.Column - end.Y) == 1)
                    ret = true;
                else
                    ret = IfCapture(start, end);
            }
            else if(start.Type == PieceTypes.Dame) {
                if (Math.Abs(start.Row - end.X) == Math.Abs(start.Column - end.Y) /*&& IfNoMoreThanOnePieceOnTheWay()*/)
                    ret = true;
            }
            return ret;
        }

        private bool IfCapture(Piece start, Point end) {
            if (Math.Abs(start.Row - end.X) == 2 && Math.Abs(start.Column - end.Y) == 2) {
                Point middleCell = new Point(start.Row + (end.X - start.Row) / 2, start.Column + (end.Y - start.Column) / 2);
                if (Pieces.SingleOrDefault(p => { return p.Row == middleCell.X && p.Column == middleCell.Y && p.Color != start.Color; }) != null)
                    return true;
            }
            return false;
        }

        private Point PointToCapture(Piece start, Point end) {
            return new Point(start.Row + (end.X - start.Row) / 2, start.Column + (end.Y - start.Column) / 2);
        }

        private bool IfOthersideCell(Point cell, Piece piece) {
            if(piece.Type == PieceTypes.Pawn) {
                if ((piece.Side == SideTypes.Top && cell.X == Size - 1) || (piece.Side == SideTypes.Bottom && cell.X == 0))
                    return true;
            }
            return false;
        }
    }
}
