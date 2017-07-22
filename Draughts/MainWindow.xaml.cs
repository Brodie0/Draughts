using System;
using System.Collections.Generic;
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
                    Coor = new Point(i, x),
                    Color = color,
                    Type = type,
                    Side = side
                }).ToList().ForEach(Pieces.Add);
            }
        }

        private Point startPoint;
        private void Drag(object sender, MouseButtonEventArgs e) {
            startPoint = GetGridsCellAtCursorsPosition((Grid)sender);
            Image image = e.OriginalSource as Image;
            DataObject data = new DataObject(typeof(ImageSource), image.Source);
            DragDrop.DoDragDrop(image, data, DragDropEffects.All);
        }

        private void MyDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(ImageSource))) {
                Point endCellCoor = GetGridsCellAtCursorsPosition((Rectangle)sender);
                Piece startCell = Pieces.SingleOrDefault(p => p.Coor == startPoint);
                if (startCell.Type == PieceTypes.Pawn) {
                    if (IfOthersideCell(startCell, endCellCoor))
                        startCell.Type = PieceTypes.Dame;
                    if (PawnMoveAllowed(startCell, endCellCoor))
                        MovePiece(startCell, endCellCoor);
                    else if (PawnCaptureMoveAllowed(startCell, endCellCoor)) {
                        CapturePieceBetween(startCell, endCellCoor);
                        MovePiece(startCell, endCellCoor);
                    }
                }
                else if (startCell.Type == PieceTypes.Dame) {
                    if (DameMoveAllowed(startCell, endCellCoor))
                        MovePiece(startCell, endCellCoor);
                    else if (DameCaptureMoveAllowed(startCell, endCellCoor)) {
                        CapturePieceBetween(startCell, endCellCoor);
                        MovePiece(startCell, endCellCoor);
                    }
                }
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

        private bool PawnMoveAllowed(Piece start, Point end) {
            return ((end.X - start.Coor.X == 1 && start.Side == SideTypes.Top || start.Coor.X - end.X == 1 && start.Side == SideTypes.Bottom) 
                && Math.Abs(start.Coor.Y - end.Y) == 1) ? true : false;
        }

        private bool PawnCaptureMoveAllowed(Piece start, Point end) {
            if (start.Type == PieceTypes.Pawn) {
                if (Math.Abs(start.Coor.X - end.X) == 2 && Math.Abs(start.Coor.Y - end.Y) == 2) {
                    Point middleCell = new Point(start.Coor.X + (end.X - start.Coor.X) / 2, start.Coor.Y + (end.Y - start.Coor.Y) / 2);
                    if (Pieces.SingleOrDefault(p => { return p.Coor == middleCell && p.Color != start.Color; }) != null)
                        return true;
                }
            }
            return false;
        }

        private bool DameMoveAllowed(Piece start, Point end) {
            return (Math.Abs(start.Coor.X - end.X) == Math.Abs(start.Coor.Y - end.Y) && IfNoMoreThanXPiecesOnTheWay(start, end, 0)) ? true : false;
        }

        private bool DameCaptureMoveAllowed(Piece start, Point end) {
            return (Math.Abs(start.Coor.X - end.X) == Math.Abs(start.Coor.Y - end.Y) && IfNoMoreThanXPiecesOnTheWay(start, end, 1, start.Color)) ? true : false;
        }

        private bool IfOthersideCell(Piece src, Point dest) {
            return ((src.Side == SideTypes.Top && dest.X == Size - 1) || (src.Side == SideTypes.Bottom && dest.X == 0)) ? true : false;
        }

        private bool IfNoMoreThanXPiecesOnTheWay(Piece start, Point end, int X, ColorTypes colorOfDame = default(ColorTypes)) {
            List<Point> pairs = GenerateWayPoints(start.Coor, end);
            List<Point> pointsOfPieces = Pieces.Select(p => p.Coor).ToList();
            List<Point> pointsOfDamesColorPieces = Pieces.Select(p => p).Where(p => p.Color == colorOfDame).Select(p => p.Coor).ToList();
            List<Point> pointsOfPiecesOnTheWay = pointsOfPieces.Intersect(pairs).ToList();
            return (pointsOfPiecesOnTheWay.Count > X || pointsOfPiecesOnTheWay.Intersect(pointsOfDamesColorPieces).ToList().Count > 0) ? false : true;
        }

        private void MovePiece(Piece src, Point dest) {
            Pieces.Add(new Piece() { Coor = dest, Color = src.Color, Type = src.Type, Side = src.Side });
            Pieces.Remove(src);
        }

        private void CapturePieceBetween(Piece src, Point dest) {
            Point pointToCapture = PointToCapture(src, dest);
            Piece pieceToCapture = Pieces.SingleOrDefault(p => p.Coor == pointToCapture);
            Pieces.Remove(pieceToCapture);
        }

        private Point PointToCapture(Piece start, Point end) {
            List<Point> pairs = GenerateWayPoints(start.Coor, end);
            List<Point> pointsOfPieces = Pieces.Select(p => p.Coor).ToList();
            List<Point> pointsOfPiecesOnTheWay = pointsOfPieces.Intersect(pairs).ToList();
            return Pieces.SingleOrDefault(p => p.Coor == pointsOfPiecesOnTheWay.ElementAt(0)).Coor;
        }

        private List<Point> GenerateWayPoints(Point src, Point dest) {
            List<int> xAxis = RangeWithBoundsExcluded((int)src.X, (int)dest.X);
            List<int> yAxis = RangeWithBoundsExcluded((int)src.Y, (int)dest.Y);
            return xAxis.Zip(yAxis, (a, b) => new Point(a, b)).ToList();
        }

        //sequence always starts from "from" variable side
        private List<int> RangeWithBoundsExcluded(int from, int to) {
            return (from >= to) ? Enumerable.Range(to + 1, from - to - 1).Reverse().ToList() : Enumerable.Range(from + 1, to - from - 1).ToList();
        }
    }
}
