using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Draughts {
    public class Piece : INotifyPropertyChanged {
        public PieceTypes Type { get; set; }
        public SideTypes Side { get; set; }
        public ColorTypes Color { get; set; }
        private Point _coor;
        public event PropertyChangedEventHandler PropertyChanged;

        public Point Coor {
            get { return _coor; }
            set {
                _coor = value;
                OnPropertyChanged("Coor");
            }
        }

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ImageSource {
            get { return "../Assets/" + (Color == ColorTypes.Black ? "black" : "white") + (Type == PieceTypes.Dame ? "D" : "P") + ".png"; }
        }
    }

    public enum PieceTypes {
        Pawn,
        Dame
    }

    public enum SideTypes {
        Top,
        Bottom
    }

    public enum ColorTypes {
        Black, 
        White
    }
}

