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
        public bool IsBlack { get; set; }
        public PieceTypes Type { get; set; }
        private int _row;
        private int _column;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Row {
            get { return _row; }
            set {
                _row = value;
                OnPropertyChanged("Row");
            }
        }

        public int Column {
            get { return _column; }
            set {
                _column = value;
                OnPropertyChanged("Column");
            }
        }

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ImageSource {
            get { return "../Assets/" + (IsBlack ? "black" : "white") + ".png"; }
        }
    }

    public enum PieceTypes {
        Pawn,
        Dame
    }
}

