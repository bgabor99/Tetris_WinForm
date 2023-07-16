using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _1_Tetris.Persistence
{
    public class TetrisFileDataAccess : ITetrisDataAccess
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

        public TetrisFileDataAccess(int width)
        {
            bgGround = new int[16, width];
            nextPiece = new int[4, 4];
        }

        public async Task LoadAsync(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    //By rows
                    String line = await reader.ReadLineAsync();
                    String[] numbers = line.Split(' '); //row

                    width = int.Parse(numbers[0]);
                    bgGround = new int[16, width];

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            nextPiece[i, j] = int.Parse(numbers[i * 4 + j + 1]);
                        }
                    }

                    nextPieceName = numbers[17];
                    nextPieceCoord1 = new Point(int.Parse(numbers[18]), int.Parse(numbers[19]));
                    nextPieceCoord2 = new Point(int.Parse(numbers[20]), int.Parse(numbers[21]));
                    nextPieceCoord3 = new Point(int.Parse(numbers[22]), int.Parse(numbers[23]));
                    nextPieceCoord4 = new Point(int.Parse(numbers[24]), int.Parse(numbers[25]));
                    rotateNumber = int.Parse(numbers[26]);

                    line = await reader.ReadLineAsync();
                    numbers = line.Split(' ');
                    for (int i = 0; i < 16; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bgGround[i, j] = int.Parse(numbers[i * width + j]);
                        }
                    }
                }
            }
            catch
            {
                throw new NullReferenceException();
            }
        }


        public async Task SaveAsync(String path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    //modell too
                    writer.Write(width);
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            await writer.WriteAsync(" " + nextPiece[i, j]);
                        }

                    }

                    await writer.WriteAsync(" " + nextPieceName);
                    await writer.WriteAsync(" " + nextPieceCoord1.X);
                    await writer.WriteAsync(" " + nextPieceCoord1.Y);
                    await writer.WriteAsync(" " + nextPieceCoord2.X);
                    await writer.WriteAsync(" " + nextPieceCoord2.Y);
                    await writer.WriteAsync(" " + nextPieceCoord3.X);
                    await writer.WriteAsync(" " + nextPieceCoord3.Y);
                    await writer.WriteAsync(" " + nextPieceCoord4.X);
                    await writer.WriteAsync(" " + nextPieceCoord4.Y);

                    await writer.WriteAsync(" " + rotateNumber);

                    await writer.WriteLineAsync();
                    for (int i = 0; i < 16; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            await writer.WriteAsync(bgGround[i, j] + " ");
                        }

                    }
                }
            }
            catch
            {
                throw new NullReferenceException();
            }
        }
    }
}
