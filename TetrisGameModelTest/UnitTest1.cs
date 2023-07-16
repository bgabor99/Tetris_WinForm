using Microsoft.VisualStudio.TestTools.UnitTesting;
using _1_Tetris.Persistence;
using _1_Tetris.Model;
using Moq;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace TetrisGameModelTest
{
    [TestClass]
    public class UnitTest1
    {

        private GameModel _model;
        private Mock<ITetrisDataAccess> _mock;
        private TetrisTable _table;
        int gameOverCount = 0;
        int changeCount = 0;

        [TestInitialize]
        public void Initialize()
        {
            int[,] bgGround = new int[4, 16];

            bgGround.SetValue(2, 0, 15);
            bgGround.SetValue(2, 1, 15);
            bgGround.SetValue(2, 2, 15);
            bgGround.SetValue(2, 0, 14);
            bgGround.SetValue(2, 1, 14);
            bgGround.SetValue(2, 2, 14);

            int[,] nextPiece = new int[4, 4];

            nextPiece.SetValue(1, 0, 0);
            nextPiece.SetValue(1, 0, 1);
            nextPiece.SetValue(1, 0, 2);
            nextPiece.SetValue(1, 0, 3);

            _table = new TetrisTable(4, nextPiece, "Line",
                                        new Point(0, 0),
                                        new Point(0, 1),
                                        new Point(0, 2),
                                        new Point(0, 3),
                                        0, bgGround);

            _mock = new Mock<ITetrisDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>())).Returns(() => Task.FromResult(_table));

            _model = new GameModel(_mock.Object);
            _model._dataAccess = _mock.Object;
            _model.gameOver += gameIsOver;
            _model.refreshTable += changed;
        }

        private void changed(object sender, EventArgs e)
        {
            changeCount++;
        }

        private void gameIsOver(object sender, EventArgs e)
        {
            gameOverCount++;
        }

        [TestMethod]
        public void New4x16GameTest()
        {
            Assert.AreEqual(4, _table.width);
            Assert.AreEqual(1, _table.nextPiece[0, 0]);
            Assert.AreEqual(1, _table.nextPiece[0, 1]);
            Assert.AreEqual(1, _table.nextPiece[0, 2]);
            Assert.AreEqual(1, _table.nextPiece[0, 3]);
            Assert.AreEqual(0, _table.nextPiece[1, 1]);
        }

        [TestMethod]
        public void GameOverTest()
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _table.bgGround[j, i] = 2;
                }
            }
            Assert.IsFalse(_model.isEndgame());
        }

        [TestMethod]
        public void LoadTest()
        {
            _model.LoadGameAsync(String.Empty);
            _mock.Verify(mock => mock.LoadAsync(String.Empty), Times.Once());
        }

        [TestMethod]
        public void LoadTest2()
        {
            _model.LoadGameAsync(String.Empty);
            _mock.Verify(mock => mock.LoadAsync(String.Empty), Times.Once());

            Assert.AreEqual(4, _table.width);
            Assert.AreEqual("Line", _table.nextPieceName);
            Assert.AreEqual(0, _table.rotateNumber);
            Assert.AreEqual(2, _table.bgGround[0, 15]);
            Assert.AreEqual(2, _table.bgGround[1, 15]);
            Assert.AreEqual(2, _table.bgGround[2, 15]);
            Assert.AreEqual(2, _table.bgGround[0, 14]);
            Assert.AreEqual(2, _table.bgGround[1, 14]);
            Assert.AreEqual(2, _table.bgGround[2, 14]);

            Assert.AreEqual(1, _table.nextPiece[0, 0]);
            Assert.AreEqual(1, _table.nextPiece[0, 1]);
            Assert.AreEqual(1, _table.nextPiece[0, 2]);
            Assert.AreEqual(1, _table.nextPiece[0, 3]);
        }

        [TestMethod]
        public void MoveRight()
        {
            _model.Move(_1_Tetris.Direction.Right);
            Assert.AreEqual(1, _table.nextPiece[0, 0]);
            Assert.AreEqual(1, _table.nextPiece[0, 1]);
            Assert.AreEqual(1, _table.nextPiece[0, 2]);
            Assert.AreEqual(1, _table.nextPiece[0, 3]);
            _model.Move(_1_Tetris.Direction.Rotate);
            Assert.AreEqual(0, _table.rotateNumber);
        }

        [TestMethod]
        public void MoveLeft()
        {
            _model.Move(_1_Tetris.Direction.Left);
            Assert.AreEqual(1, _table.nextPiece[0, 0]);
            Assert.AreEqual(1, _table.nextPiece[0, 1]);
            Assert.AreEqual(1, _table.nextPiece[0, 2]);
            Assert.AreEqual(1, _table.nextPiece[0, 3]);

            _model.Move(_1_Tetris.Direction.Rotate);
            Assert.AreEqual(0, _table.rotateNumber);
        }

        [TestMethod]
        public void Time()
        {
            _model.newGame(4);
            Assert.IsTrue(_model.myTimer.Enabled);
        }
    }
}
