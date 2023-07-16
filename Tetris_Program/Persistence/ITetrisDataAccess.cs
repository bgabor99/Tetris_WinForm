using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _1_Tetris.Persistence
{
    public interface ITetrisDataAccess
    {
        public int width { get; set; }
        public int[,] nextPiece { get; set; }
        public String nextPieceName { get; set; }

        public Point nextPieceCoord1 { get; set; }
        public Point nextPieceCoord2 { get; set; }
        public Point nextPieceCoord3 { get; set; }
        public Point nextPieceCoord4 { get; set; }
        public int rotateNumber { get; set; }

        public int[,] bgGround { get; set; }

        Task LoadAsync(String path);
        Task SaveAsync(String path);
    }
}
