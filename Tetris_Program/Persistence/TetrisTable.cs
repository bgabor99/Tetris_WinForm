using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace _1_Tetris.Persistence
{
    public class TetrisTable
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

        public TetrisTable(int width, int[,] nextPiece, string nextPieceName, Point nextPieceCoord1, Point nextPieceCoord2, Point nextPieceCoord3, Point nextPieceCoord4, int rotateNumber, int[,] bgGround)
        {
            //save file content is this
            //width nextPiece[4,4] nextPieceName nextPieceCoord1.X nextPieceCoord1.Y nextPieceCoord2.X nextPieceCoord2.Y nextPieceCoord3.X nextPieceCoord3.Y nextPieceCoord4.X nextPieceCoord4.Y rotateNumber
            //\n bgGround[4,16]/bgGround[8,16]bgGround[12,16]
            this.width = width;
            this.nextPiece = nextPiece;
            this.nextPieceName = nextPieceName;
            this.nextPieceCoord1 = nextPieceCoord1;
            this.nextPieceCoord2 = nextPieceCoord2;
            this.nextPieceCoord3 = nextPieceCoord3;
            this.nextPieceCoord4 = nextPieceCoord4;
            this.rotateNumber = rotateNumber;
            this.bgGround = bgGround;
        }

    }
}
