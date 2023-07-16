using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Numerics;
using _1_Tetris.Persistence;
using System.Threading.Tasks;

namespace _1_Tetris.Model
{

    public class GameModel
    {
        #region Variables
        //Variables
        public ITetrisDataAccess _dataAccess { get; set; }
        public int width { get; private set; }
        public int height { get; private set; }
        private int[,] bgGround { get; set; }

        public int ellapsedTime;

        public System.Windows.Forms.Timer myTimer;

        public List<Point> Columns; //filling the columns
        public bool leert = false;
        private List<int[,]> pieces;

        //nextPiece
        int[,] nextPiece;
        private String nextPieceName;
        private Point NextPieceCoord1;
        private Point NextPieceCoord2;
        private Point NextPieceCoord3;
        private Point NextPieceCoord4;

        private int rotateNumber; // 0; 1;

        private int downMostFullLineIndex = -1; //which column is full
        private List<int> fullLineList;

        //eventek
        public event EventHandler<EventArgs> refreshTable;
        public event EventHandler<EventArgs> gameOver;
        #endregion

        #region Constructor
        //Constructor
        public GameModel(ITetrisDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            fullLineList = new List<int>();

            pieces = new List<int[,]>();
            //pieces[0]
            pieces.Add(new int[4, 4] { { 1, 0, 0, 0 }, { 1, 0, 0, 0 }, { 1, 0, 0, 0 }, { 1, 0, 0, 0 } });
            //pieces[1]
            pieces.Add(new int[4, 4] { { 1, 1, 0, 0 }, { 1, 1, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } });
            //pieces[2]
            pieces.Add(new int[4, 4] { { 1, 1, 1, 0 }, { 1, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } });
            //pieces[3]
            pieces.Add(new int[4, 4] { { 1, 1, 0, 0 }, { 0, 1, 1, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } });

            Columns = new List<Point>();
            myTimer = new System.Windows.Forms.Timer();
            newGame(4);
        }
        #endregion

        #region TimeTick handle
        //To TimeTick: nextPiece goes down and EllapsedTime passes
        private void TimeEventProcessor(object sender, EventArgs e)
        {
            ++ellapsedTime;
            buildColumns();

            if (leert)
            {
                buildColumns();
            }

            //FullLine check
            if (isFullLine())
            {
                MakebgGroundAfterFullLine();
                buildColumns();
                onRefreshTable();
            }

            //isEndGame check
            if (isEndgame())
            {
                onGameOver();
            }

            if ((NextPieceCoord1.X < height - 1 && NextPieceCoord2.X < height - 1 && NextPieceCoord3.X < height - 1 && NextPieceCoord4.X < height - 1)
                  && !checkStop())
            {
                leert = false;
                bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                NextPieceCoord1.X++;
                NextPieceCoord2.X++;
                NextPieceCoord3.X++;
                NextPieceCoord4.X++;

                bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;
                onRefreshTable(); //refresh signal
            }
            else
            {
                //hit the bottom
                leert = true;
                bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 2;
                bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 2;
                bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 2;
                bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 2;

                Random r = new Random();
                int rand = r.Next(0, pieces.Count);
                createNextPieceFromPieces(rand);

                switch (rand)
                {
                    case 0: nextPieceName = "Line"; break;
                    case 1: nextPieceName = "Square"; break;
                    case 2: nextPieceName = "LPiece"; break;
                    case 3: nextPieceName = "Rombus"; break;
                }
            }

            //signal to the view
            onRefreshTable();
        }
        #endregion

        #region Fall check, Columns fill
        public void buildColumns()
        {
            //columns List fill (columns max height)
            int min;
            for (int i = 0; i < width; i++)
            {
                min = 15;
                for (int j = height - 1; j >= 0; j--)
                {
                    if (bgGround[j, i] == 2)
                    {
                        min = j - 1;
                    }
                }
                Columns[i] = new Point(i, min);
            }
        }


        public bool checkStop()
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].Y > 0 && bgGround[Columns[i].Y, i] == 1)
                {
                    return true;
                }
            }
            return false;
        }

        //Showed nextPiece on bgGroundon
        public void Spawn(int[,] bgGround, int[,] nextPiece) //bgGorund, nextPiece
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    bgGround[i, j] = nextPiece[i, j];
                }
            }
        }
        #endregion

        #region NewGame
        public void newGame(int widthP)
        {
            rotateNumber = 0;
            Columns.Clear();

            for (int i = 0; i < widthP; i++)
            {
                Columns.Add(new Point(0, 0));

            }

            this.height = 16;
            this.width = widthP;
            myTimer.Dispose();
            ellapsedTime = 0;
            myTimer = new System.Windows.Forms.Timer();
            myTimer.Interval = 1000;
            myTimer.Start();

            myTimer.Tick += new EventHandler(TimeEventProcessor); //timer event handler

            bgGround = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bgGround[i, j] = 0;
                }
            }

            Random r = new Random();
            int rand = r.Next(0, pieces.Count);
            createNextPieceFromPieces(rand);
            switch (rand)
            {
                case 0: nextPieceName = "Line"; break;
                case 1: nextPieceName = "Square"; break;
                case 2: nextPieceName = "LPiece"; break;
                case 3: nextPieceName = "Rombus"; break;
            }

            Spawn(bgGround, nextPiece);
        }
        #endregion

        #region CreateNextPiece
        // random chooes one which becomes the nextPiece
        public void createNextPieceFromPieces(int piecesInd)
        {
            rotateNumber = 0;
            nextPiece = new int[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    nextPiece[i, j] = pieces[piecesInd][i, j];
                }
            }

            //set NextPieceCoords based on the object shape
            switch (piecesInd)
            {
                //Line
                case 0:
                    NextPieceCoord1 = new Point(0, 0);
                    NextPieceCoord2 = new Point(1, 0);
                    NextPieceCoord3 = new Point(2, 0);
                    NextPieceCoord4 = new Point(3, 0);
                    break;
                //Square
                case 1:
                    NextPieceCoord1 = new Point(0, 0);
                    NextPieceCoord2 = new Point(0, 1);
                    NextPieceCoord3 = new Point(1, 0);
                    NextPieceCoord4 = new Point(1, 1);
                    break;
                //L
                case 2:
                    NextPieceCoord1 = new Point(0, 0);
                    NextPieceCoord2 = new Point(0, 1);
                    NextPieceCoord3 = new Point(0, 2);
                    NextPieceCoord4 = new Point(1, 0);
                    break;
                //Rombus
                case 3:
                    NextPieceCoord1 = new Point(0, 0);
                    NextPieceCoord2 = new Point(0, 1);
                    NextPieceCoord3 = new Point(1, 1);
                    NextPieceCoord4 = new Point(1, 2);
                    break;
            }

        }
        #endregion

        #region GetFieldCoord methods for display buttons
        public int GetFieldFromCoord(int x, int y)
        {
            return bgGround[x, y]; //bgGround[row, column]
        }
        #endregion

        #region Move
        public void Move(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    {
                        if (!isTableEdgeLeft() && !isNexToLeft())
                        {
                            //Left
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1.Y--;
                            NextPieceCoord2.Y--;
                            NextPieceCoord3.Y--;
                            NextPieceCoord4.Y--;

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            onRefreshTable();
                        }
                        break;
                    }

                case Direction.Right:
                    {
                        if (!isTableEdgeRight() && !isNexToRight())
                        {
                            //Right
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1.Y++;
                            NextPieceCoord2.Y++;
                            NextPieceCoord3.Y++;
                            NextPieceCoord4.Y++;

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            onRefreshTable();
                        }
                        break;
                    }

                // Line, Square, LPiece, Rombus
                case Direction.Rotate:
                    {

                        if (rotateNumber == 0 &&
                           nextPieceName == "Line" &&
                           isValidRotate0("Line", new Tuple<Point, Point, Point, Point>(NextPieceCoord1, NextPieceCoord2, NextPieceCoord3, NextPieceCoord4)))
                        {

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1 = new Point(NextPieceCoord1.X + 3, NextPieceCoord1.Y + 3);
                            NextPieceCoord2 = new Point(NextPieceCoord2.X + 2, NextPieceCoord2.Y + 2);
                            NextPieceCoord3 = new Point(NextPieceCoord3.X + 1, NextPieceCoord3.Y + 1);
                            NextPieceCoord4 = new Point(NextPieceCoord4.X, NextPieceCoord4.Y);

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            rotateNumber = 1;
                        }
                        else if (rotateNumber == 1 &&
                           nextPieceName == "Line" &&
                           isValidRotate1("Line", new Tuple<Point, Point, Point, Point>(NextPieceCoord1, NextPieceCoord2, NextPieceCoord3, NextPieceCoord4)))
                        {
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1 = new Point(NextPieceCoord1.X - 3, NextPieceCoord1.Y - 3);
                            NextPieceCoord2 = new Point(NextPieceCoord2.X - 2, NextPieceCoord2.Y - 2);
                            NextPieceCoord3 = new Point(NextPieceCoord3.X - 1, NextPieceCoord3.Y - 1);
                            NextPieceCoord4 = new Point(NextPieceCoord4.X, NextPieceCoord4.Y);

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            rotateNumber = 0;
                        }
                        if (rotateNumber == 0 &&
                          nextPieceName == "LPiece" &&
                          isValidRotate0("LPiece", new Tuple<Point, Point, Point, Point>(NextPieceCoord1, NextPieceCoord2, NextPieceCoord3, NextPieceCoord4)))
                        {
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1 = new Point(NextPieceCoord1.X, NextPieceCoord1.Y);
                            NextPieceCoord2 = new Point(NextPieceCoord2.X, NextPieceCoord2.Y);
                            NextPieceCoord3 = new Point(NextPieceCoord3.X + 1, NextPieceCoord3.Y - 1);
                            NextPieceCoord4 = new Point(NextPieceCoord4.X + 1, NextPieceCoord4.Y + 1);

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            rotateNumber = 1;
                        }
                        else if (rotateNumber == 1 &&
                                 nextPieceName == "LPiece" &&
                                 isValidRotate1("LPiece", new Tuple<Point, Point, Point, Point>(NextPieceCoord1, NextPieceCoord2, NextPieceCoord3, NextPieceCoord4)))
                        {
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1 = new Point(NextPieceCoord1.X, NextPieceCoord1.Y);
                            NextPieceCoord2 = new Point(NextPieceCoord2.X, NextPieceCoord2.Y);
                            NextPieceCoord3 = new Point(NextPieceCoord3.X - 1, NextPieceCoord3.Y + 1);
                            NextPieceCoord4 = new Point(NextPieceCoord4.X - 1, NextPieceCoord4.Y - 1);

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            rotateNumber = 0;
                        }

                        if (rotateNumber == 0 &&
                         nextPieceName == "Rombus" &&
                         isValidRotate0("Rombus", new Tuple<Point, Point, Point, Point>(NextPieceCoord1, NextPieceCoord2, NextPieceCoord3, NextPieceCoord4)))
                        {
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1 = new Point(NextPieceCoord1.X, NextPieceCoord1.Y);
                            NextPieceCoord2 = new Point(NextPieceCoord2.X - 1, NextPieceCoord2.Y - 1);
                            NextPieceCoord3 = new Point(NextPieceCoord3.X, NextPieceCoord3.Y);
                            NextPieceCoord4 = new Point(NextPieceCoord4.X - 1, NextPieceCoord4.Y - 1);

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            rotateNumber = 1;
                        }
                        else if (rotateNumber == 1 &&
                                nextPieceName == "Rombus" &&
                                isValidRotate1("Rombus", new Tuple<Point, Point, Point, Point>(NextPieceCoord1, NextPieceCoord2, NextPieceCoord3, NextPieceCoord4)))
                        {
                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 0;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 0;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 0;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 0;

                            NextPieceCoord1 = new Point(NextPieceCoord1.X, NextPieceCoord1.Y);
                            NextPieceCoord2 = new Point(NextPieceCoord2.X + 1, NextPieceCoord2.Y + 1);
                            NextPieceCoord3 = new Point(NextPieceCoord3.X, NextPieceCoord3.Y);
                            NextPieceCoord4 = new Point(NextPieceCoord4.X + 1, NextPieceCoord4.Y + 1);

                            bgGround[NextPieceCoord1.X, NextPieceCoord1.Y] = 1;
                            bgGround[NextPieceCoord2.X, NextPieceCoord2.Y] = 1;
                            bgGround[NextPieceCoord3.X, NextPieceCoord3.Y] = 1;
                            bgGround[NextPieceCoord4.X, NextPieceCoord4.Y] = 1;

                            rotateNumber = 0;
                        }
                        break;
                    }

            }
        }

        //Rotate check
        public bool isValidRotate0(string PieceName, Tuple<Point, Point, Point, Point> coords)
        {
            if (PieceName == "Line")
            {
                Point deltaC1 = coords.Item1;
                Point deltaC2 = coords.Item2;
                Point deltaC3 = coords.Item3;
                Point deltaC4 = coords.Item4;

                deltaC1.X += 3;
                deltaC1.Y += 3;

                deltaC2.X += 2;
                deltaC2.Y += 2;

                deltaC3.X += 1;
                deltaC3.Y += 1;

                if ((deltaC1.X >= 0 && deltaC1.X < height)
                    && (deltaC2.X >= 0 && deltaC2.X < height)
                    && (deltaC3.X >= 0 && deltaC3.X < height)
                    && (deltaC4.X >= 0 && deltaC4.X < height)
                    && (deltaC1.Y >= 0 && deltaC1.Y < width)
                    && (deltaC2.Y >= 0 && deltaC2.Y < width)
                    && (deltaC3.Y >= 0 && deltaC3.Y < width)
                    && (deltaC4.Y >= 0 && deltaC4.Y < width)
                    && (bgGround[deltaC1.X, deltaC1.Y] != 2
                    && bgGround[deltaC2.X, deltaC2.Y] != 2
                    && bgGround[deltaC3.X, deltaC3.Y] != 2
                    && bgGround[deltaC4.X, deltaC4.Y] != 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            if (PieceName == "LPiece")
            {
                Point deltaC1 = coords.Item1;
                Point deltaC2 = coords.Item2;
                Point deltaC3 = coords.Item3;
                Point deltaC4 = coords.Item4;

                deltaC3.X += 1;
                deltaC3.Y -= 1;

                deltaC4.X += 1;
                deltaC4.Y += 1;

                if ((deltaC1.X >= 0 && deltaC1.X < height)
                    && (deltaC2.X >= 0 && deltaC2.X < height)
                    && (deltaC3.X >= 0 && deltaC3.X < height)
                    && (deltaC4.X >= 0 && deltaC4.X < height)
                    && (deltaC1.Y >= 0 && deltaC1.Y < width)
                    && (deltaC2.Y >= 0 && deltaC2.Y < width)
                    && (deltaC3.Y >= 0 && deltaC3.Y < width)
                    && (deltaC4.Y >= 0 && deltaC4.Y < width)
                    && (bgGround[deltaC1.X, deltaC1.Y] != 2
                    && bgGround[deltaC2.X, deltaC2.Y] != 2
                    && bgGround[deltaC3.X, deltaC3.Y] != 2
                    && bgGround[deltaC4.X, deltaC4.Y] != 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            if (PieceName == "Rombus")
            {
                Point deltaC1 = coords.Item1;
                Point deltaC2 = coords.Item2;
                Point deltaC3 = coords.Item3;
                Point deltaC4 = coords.Item4;

                deltaC2.X -= 1;
                deltaC2.Y -= 1;

                deltaC4.X -= 1;
                deltaC4.Y -= 1;

                if ((deltaC1.X >= 0 && deltaC1.X < height)
                    && (deltaC2.X >= 0 && deltaC2.X < height)
                    && (deltaC3.X >= 0 && deltaC3.X < height)
                    && (deltaC4.X >= 0 && deltaC4.X < height)
                    && (deltaC1.Y >= 0 && deltaC1.Y < width)
                    && (deltaC2.Y >= 0 && deltaC2.Y < width)
                    && (deltaC3.Y >= 0 && deltaC3.Y < width)
                    && (deltaC4.Y >= 0 && deltaC4.Y < width)
                    && (bgGround[deltaC1.X, deltaC1.Y] != 2
                    && bgGround[deltaC2.X, deltaC2.Y] != 2
                    && bgGround[deltaC3.X, deltaC3.Y] != 2
                    && bgGround[deltaC4.X, deltaC4.Y] != 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

        }

        public bool isValidRotate1(string PieceName, Tuple<Point, Point, Point, Point> coords)
        {
            if (PieceName == "Line")
            {
                Point deltaC1 = coords.Item1;
                Point deltaC2 = coords.Item2;
                Point deltaC3 = coords.Item3;
                Point deltaC4 = coords.Item4;

                deltaC1.X -= 3;
                deltaC1.Y -= 3;

                deltaC2.X -= 2;
                deltaC2.Y -= 2;

                deltaC3.X -= 1;
                deltaC3.Y -= 1;

                if ((deltaC1.X >= 0 && deltaC1.X < height)
                    && (deltaC2.X >= 0 && deltaC2.X < height)
                    && (deltaC3.X >= 0 && deltaC3.X < height)
                    && (deltaC4.X >= 0 && deltaC4.X < height)
                    && (deltaC1.Y >= 0 && deltaC1.Y < width)
                    && (deltaC2.Y >= 0 && deltaC2.Y < width)
                    && (deltaC3.Y >= 0 && deltaC3.Y < width)
                    && (deltaC4.Y >= 0 && deltaC4.Y < width)
                    && (bgGround[deltaC1.X, deltaC1.Y] != 2
                    && bgGround[deltaC2.X, deltaC2.Y] != 2
                    && bgGround[deltaC3.X, deltaC3.Y] != 2
                    && bgGround[deltaC4.X, deltaC4.Y] != 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            if (PieceName == "LPiece")
            {
                Point deltaC1 = coords.Item1;
                Point deltaC2 = coords.Item2;
                Point deltaC3 = coords.Item3;
                Point deltaC4 = coords.Item4;

                deltaC3.X += 1;
                deltaC3.Y -= 1;

                deltaC4.X += 1;
                deltaC4.Y += 1;

                if ((deltaC1.X >= 0 && deltaC1.X < height)
                    && (deltaC2.X >= 0 && deltaC2.X < height)
                    && (deltaC3.X >= 0 && deltaC3.X < height)
                    && (deltaC4.X >= 0 && deltaC4.X < height)
                    && (deltaC1.Y >= 0 && deltaC1.Y < width)
                    && (deltaC2.Y >= 0 && deltaC2.Y < width)
                    && (deltaC3.Y >= 0 && deltaC3.Y < width)
                    && (deltaC4.Y >= 0 && deltaC4.Y < width)
                    && (bgGround[deltaC1.X, deltaC1.Y] != 2
                    && bgGround[deltaC2.X, deltaC2.Y] != 2
                    && bgGround[deltaC3.X, deltaC3.Y] != 2
                    && bgGround[deltaC4.X, deltaC4.Y] != 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            if (PieceName == "Rombus")
            {
                Point deltaC1 = coords.Item1;
                Point deltaC2 = coords.Item2;
                Point deltaC3 = coords.Item3;
                Point deltaC4 = coords.Item4;

                deltaC2.X += 1;
                deltaC2.Y += 1;

                deltaC4.X += 1;
                deltaC4.Y += 1;

                if ((deltaC1.X >= 0 && deltaC1.X < height)
                    && (deltaC2.X >= 0 && deltaC2.X < height)
                    && (deltaC3.X >= 0 && deltaC3.X < height)
                    && (deltaC4.X >= 0 && deltaC4.X < height)
                    && (deltaC1.Y >= 0 && deltaC1.Y < width)
                    && (deltaC2.Y >= 0 && deltaC2.Y < width)
                    && (deltaC3.Y >= 0 && deltaC3.Y < width)
                    && (deltaC4.Y >= 0 && deltaC4.Y < width)
                    && (bgGround[deltaC1.X, deltaC1.Y] != 2
                    && bgGround[deltaC2.X, deltaC2.Y] != 2
                    && bgGround[deltaC3.X, deltaC3.Y] != 2
                    && bgGround[deltaC4.X, deltaC4.Y] != 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            else
            {
                return false;
            }

        }

        //Check the next coords when move
        public bool isTableEdgeLeft()
        {
            if (NextPieceCoord1.Y - 1 < 0 ||
               NextPieceCoord2.Y - 1 < 0 ||
               NextPieceCoord3.Y - 1 < 0 ||
               NextPieceCoord4.Y - 1 < 0
               )
            {
                return true;
            }
            return false;
        }

        public bool isTableEdgeRight()
        {
            if (NextPieceCoord1.Y + 1 == width ||
                NextPieceCoord2.Y + 1 == width ||
                NextPieceCoord3.Y + 1 == width ||
                NextPieceCoord4.Y + 1 == width
               )
            {
                return true;
            }
            return false;
        }


        public bool isNexToLeft()
        {

            if (bgGround[NextPieceCoord1.X, NextPieceCoord1.Y - 1] == 2 ||
               bgGround[NextPieceCoord2.X, NextPieceCoord2.Y - 1] == 2 ||
               bgGround[NextPieceCoord3.X, NextPieceCoord3.Y - 1] == 2 ||
               bgGround[NextPieceCoord4.X, NextPieceCoord4.Y - 1] == 2
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isNexToRight()
        {

            if (bgGround[NextPieceCoord1.X, NextPieceCoord1.Y + 1] == 2 ||
               bgGround[NextPieceCoord2.X, NextPieceCoord2.Y + 1] == 2 ||
               bgGround[NextPieceCoord3.X, NextPieceCoord3.Y + 1] == 2 ||
               bgGround[NextPieceCoord4.X, NextPieceCoord4.Y + 1] == 2
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region isEndGame
        public bool isEndgame()
        {
            for (int i = 0; i < width; i++)
            {
                if (bgGround[0, i] == 2 || bgGround[1, i] == 2)
                {
                    return true;
                }
            }
            if (nextPieceName == "Line" && bgGround[4, 0] == 2)
            {
                return true;
            }
            else if (nextPieceName == "LPiece" && (bgGround[3, 0] == 2 || bgGround[3, 1] == 2 || bgGround[3, 2] == 2))
            {
                return true;
            }
            else if (nextPieceName == "Square" && (bgGround[3, 0] == 2 || bgGround[3, 1] == 2))
            {
                return true;
            }
            else if (nextPieceName == "Rombus" && (bgGround[2, 0] == 2 || bgGround[3, 1] == 2 || bgGround[3, 2] == 2))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region FullLine handle, delete row,others rows fall down by one row
        public bool isFullLine()
        {
            for (int i = height - 1; i >= 0; i--)
            {
                int greenCount = 0;
                for (int j = 0; j < width; j++)
                {
                    if (bgGround[i, j] == 2)
                    {
                        greenCount++;
                        if (greenCount == width)
                        {
                            downMostFullLineIndex = i;
                            Debug.WriteLine("Talalt teli sort, ezt (i): " + i);

                            return true;
                        }
                    }

                }
            }
            return false;
        }

        public void MakebgGroundAfterFullLine()
        {
            deleteFullLine();
            fallDownAboveFullLine();
        }

        //Fall down until it can
        public void fallDownAboveFullLine()
        {
            for (int i = downMostFullLineIndex; i >= 0; i--)
            {
                for (int j = 0; j < width; j++)
                {
                    if (bgGround[i, j] == 2)
                    {
                        bgGround[i, j] = 0;
                        bgGround[i + 1, j] = 2;
                        buildColumns();
                    }
                }
            }
            downMostFullLineIndex = -1;
        }

        public void deleteFullLine()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (downMostFullLineIndex != -1)
                    {
                        bgGround[downMostFullLineIndex, j] = 0;
                    }
                }
            }
        }
        #endregion

        #region Event methods
        public void onRefreshTable()
        {
            refreshTable?.Invoke(this, new EventArgs());
        }

        public void onGameOver()
        {
            gameOver?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Data reach methods
        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
            {
                throw new InvalidOperationException("No data access is provided");
            }
            //call then giving value to the variables
            await _dataAccess.LoadAsync(path);
            width = _dataAccess.width;
            bgGround = new int[16, width];

            Columns = new List<Point>();
            for (int i = 0; i < width; i++)
            {
                Columns.Add(new Point(-1, -1));
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    nextPiece[i, j] = _dataAccess.nextPiece[i, j];
                }
            }

            nextPieceName = _dataAccess.nextPieceName;

            NextPieceCoord1 = _dataAccess.nextPieceCoord1;
            NextPieceCoord2 = _dataAccess.nextPieceCoord2;
            NextPieceCoord3 = _dataAccess.nextPieceCoord3;
            NextPieceCoord4 = _dataAccess.nextPieceCoord4;

            rotateNumber = _dataAccess.rotateNumber;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bgGround[i, j] = _dataAccess.bgGround[i, j];
                }
            }
        }


        public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
            {
                throw new InvalidOperationException("No data access is provided");
            }

            _dataAccess.width = width;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _dataAccess.nextPiece[i, j] = nextPiece[i, j];
                }
            }

            _dataAccess.nextPieceName = nextPieceName;
            _dataAccess.nextPieceCoord1 = NextPieceCoord1;
            _dataAccess.nextPieceCoord2 = NextPieceCoord2;
            _dataAccess.nextPieceCoord3 = NextPieceCoord3;
            _dataAccess.nextPieceCoord4 = NextPieceCoord4;
            _dataAccess.rotateNumber = rotateNumber;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    _dataAccess.bgGround[i, j] = bgGround[i, j];
                }
            }
            onRefreshTable();
            await _dataAccess.SaveAsync(path);
        }
        #endregion
    }
}
