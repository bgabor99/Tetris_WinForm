using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using _1_Tetris.Model;
using _1_Tetris.Persistence;

namespace _1_Tetris
{
    public partial class Form1 : Form
    {

        #region Variables
        //Variables

        GameModel _model; //modell
        ITetrisDataAccess _dataAccess; //dataaccess

        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;

        private int width;
        private int height;
        private bool playOn = true;

        //view
        private TableLayoutPanel table; 
        private MenuStrip menu;
        private ToolStripMenuItem easy;
        private ToolStripMenuItem medium;
        private ToolStripMenuItem hard;
        private StatusBar mainStatusBar;
        private Button pauseNplay;
        private Button saveButton;
        private Button loadButton;
        #endregion

        #region Constructor
        public Form1()
        {
            InitializeComponent();

            _openFileDialog = new OpenFileDialog();
            _openFileDialog.Filter = ".txt | *.txt";
            _saveFileDialog = new SaveFileDialog();
            _saveFileDialog.Filter = ".txt | *.txt";

            _model = new GameModel(_dataAccess);
            this.width = _model.width;
            this.height = _model.height;
            
            _model._dataAccess = new TetrisFileDataAccess(width);
            this.KeyPreview = true;

            table = new TableLayoutPanel();
            menu = new MenuStrip();
            pauseNplay = new Button();
            pauseNplay.Location = new Point(600, 200);
            pauseNplay.Size = new Size(100, 50);
            pauseNplay.Text = "Pause";
            Controls.Add(pauseNplay);

            saveButton = new Button();
            saveButton.Location = new Point(600, 300);
            saveButton.Size = new Size(100, 50);
            saveButton.Text = "Save";
            saveButton.Enabled = false;
            Controls.Add(saveButton);

            loadButton = new Button();
            loadButton.Location = new Point(600, 360);
            loadButton.Size = new Size(100, 50);
            loadButton.Text = "Load";
            loadButton.Enabled = false;
            Controls.Add(loadButton);

            easy = new ToolStripMenuItem("4x16", null, new EventHandler(New4x16Game));
            medium = new ToolStripMenuItem("8x16", null, new EventHandler(New8x16Game));
            hard = new ToolStripMenuItem("12x16", null, new EventHandler(New12x16Game));

            mainStatusBar = new StatusBar();
            Controls.Add(mainStatusBar);
            mainStatusBar.Text = "Eltelt idő: " + _model.ellapsedTime; 

            menuSetup(menu);
            Controls.Add(menu);
            SetupTable(width, height, table);
            Controls.Add(table);

            //Refresh by modell signal
            _model.refreshTable += UpdateTable;
            KeyUp += Form1_KeyPress_1;
            _model.gameOver += gameOver; 

            table.Focus();
            //pauseNPlay Button_click
            pauseNplay.Click += PauseOrPlay;
            //load, save Button click
            saveButton.Click += saveButton_Click;
            loadButton.Click += loadButton_Click;
        }
        #endregion

        #region Dataaccess event handlers
        private async void loadButton_Click(object sender, EventArgs e)
        {
            Boolean restartTimer = _model.myTimer.Enabled;
            _model.myTimer.Stop();

           if(_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _model.LoadGameAsync(_openFileDialog.FileName);
                    width = _model.width;
                    SetupTable(width,16,table);
                    saveButton.Enabled = true; 
                }
                catch
                {
                    MessageBox.Show("A játék betöltése sikertelen");
                    saveButton.Enabled = true; 
                }
            }

            if (restartTimer)
            {
                _model.myTimer.Start();
            }
        }

        private async void saveButton_Click(object sender, EventArgs e)
        {
            Boolean restartTimer = _model.myTimer.Enabled;
            _model.myTimer.Stop();

            if (_saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _model.SaveGameAsync(_saveFileDialog.FileName);
                }
                catch
                {
                    MessageBox.Show("A játék mentése sikertelen!");
                }

                if (restartTimer)
                {
                    _model.myTimer.Start();
                }
            }
        }
        #endregion

        #region pauseNplay EventHandlers and gameOver EventHandlers
        private void PauseOrPlay(object sender, EventArgs e)
        {
            if (playOn)
            {
                pauseNplay.Text = "Play";
                _model.myTimer.Stop();
                KeyUp -= Form1_KeyPress_1;
                menu.Enabled = false;
                loadButton.Enabled = true;
                saveButton.Enabled = true;
                playOn = false;
            }
            else
            {
                pauseNplay.Text = "Pause";
                _model.myTimer.Start();
                KeyUp += Form1_KeyPress_1;
                menu.Enabled = true;
                loadButton.Enabled = false;
                saveButton.Enabled = false;
                playOn = true;
            }
        }

        private void gameOver(object sender, EventArgs e)
        {
            _model.newGame(4);
            MessageBox.Show("Vesztettél. játék vége");
        }
        #endregion

        #region MenuStrip EventHandlers
        private void New4x16Game(object sender, EventArgs e)
        {
            _model.newGame(4);
            _model._dataAccess = new TetrisFileDataAccess(4);
            SetupTable(4,16,table);
        }

        private void New8x16Game(object sender, EventArgs e)
        {
            _model.newGame(8);
            _model._dataAccess = new TetrisFileDataAccess(8);
            SetupTable(8,16,table);
        }

        private void New12x16Game(object sender, EventArgs e)
        {
            _model.newGame(12);
            _model._dataAccess = new TetrisFileDataAccess(12);
            SetupTable(12,16,table);
        }

        private void menuSetup(MenuStrip menu)
        {
            menu.Items.Add(easy);
            menu.Items.Add(medium);
            menu.Items.Add(hard);
        }
        #endregion

        #region SetupTable, UpdateTable
        private void SetupTable(int width, int height, TableLayoutPanel table)
        {
            table.Controls.Clear();
            table.RowCount = height;
            table.ColumnCount = width;
            table.AutoSize = true;
            table.Location = new Point(50, 50);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Button b = new Button();
                    switch(_model.GetFieldFromCoord(i,j))
                    {
                        case 0: b.BackColor = Color.White;
                            break;
                        case 1: b.BackColor = Color.Blue;
                            break;
                    }
                    //new buttons everywhere
                    b.Size = new Size(35, 35);
                    table.Controls.Add(b, j, i);
                }
            }
        }

        private void UpdateTable(object sender, EventArgs e)
        {
            this.width = _model.width;
            this.height = _model.height;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Button b = new Button();
                    switch (_model.GetFieldFromCoord(i, j))
                    {
                        case 0:
                            b.BackColor = Color.White;
                            break;
                        case 1:
                            b.BackColor = Color.Blue;
                            break;
                        case 2:
                            b.BackColor = Color.Green;
                            break;
                    }
                    table.GetControlFromPosition(j, i).BackColor = b.BackColor;
                }
            }
            mainStatusBar.Text = "Eltelt idő: " + _model.ellapsedTime;
        }
        #endregion

        #region KeyPress for _model_Move
        private void Form1_KeyPress_1(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {   
                case Keys.A: _model.Move(Direction.Left); break;
                case Keys.D: _model.Move(Direction.Right); break;
                case Keys.W: _model.Move(Direction.Rotate); break;           
            }
        }
        #endregion
    }
}
