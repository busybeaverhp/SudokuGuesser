using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace _20160515_SudokuGuesser
{
    public partial class Form1 : Form
    {
        List<int[,]> easyPuzzles = new List<int[,]>();
        List<int[,]> mediumPuzzles = new List<int[,]>();
        List<int[,]> hardPuzzles = new List<int[,]>();
        List<int[,]> evilPuzzles = new List<int[,]>();
        List<int[,]> testPuzzles = new List<int[,]>();

        List<int[,]> lookupTable = new List<int[,]>();

        Random rand = new Random();
        Stopwatch stopWatch = new Stopwatch();

        int[,] loadedPuzzleState = new int[9, 9];

        // f(M) is the the Sudoku Puzzle as the reader see, with numbers and blanks.
        int[,] currentMatrixState = new int[9, 9];
        
        // f'(M) is the derivation of the puzzle, known as the heatmap containing the range of the next legal move per cell.
        List<int>[,] allowableRangeMap = new List<int>[9, 9];

        // f''(M) is the derivation of the heatmap.
        List<int>[,][,] sectorizedAllowableRangeMap = new List<int>[3, 3][,];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region EASY PUZZLES

            int[,] easyPuzzle1 = 
                                   {{0, 0, 0,   0, 0, 4,    7, 6, 0 }, 
                                    {0, 0, 5,   8, 0, 0,    2, 0, 1 }, 
                                    {0, 4, 0,   0, 0, 6,    5, 3, 9 },
 
                                    {0, 7, 3,   9, 8, 0,    0, 0, 0 },
                                    {0, 9, 0,   0, 0, 0,    0, 5, 0 },
                                    {0, 0, 0,   0, 6, 7,    9, 2, 0 },

                                    {3, 6, 2,   7, 0, 0,    0, 9, 0 },
                                    {4, 0, 7,   0, 0, 3,    1, 0, 0 },
                                    {0, 1, 9,   6, 0, 0,    0, 0, 0 }};

            easyPuzzles.Add(easyPuzzle1);

            #endregion

            #region MEDIUM PUZZLES

            int[,] mediumPuzzle1 = 
                                   {{0, 4, 0,   6, 0, 1,    0, 7, 0 }, 
                                    {7, 0, 5,   0, 0, 9,    0, 0, 2 }, 
                                    {6, 0, 0,   0, 0, 3,    0, 8, 0 },
 
                                    {2, 0, 0,   0, 0, 5,    0, 3, 0 },
                                    {3, 0, 0,   2, 0, 4,    0, 0, 8 },
                                    {0, 6, 0,   9, 0, 0,    0, 0, 4 },

                                    {0, 8, 0,   7, 0, 0,    0, 0, 1 },
                                    {5, 0, 0,   3, 0, 0,    9, 0, 7 },
                                    {0, 7, 0,   1, 0, 6,    0, 4, 0 }};

            mediumPuzzles.Add(mediumPuzzle1);

            #endregion

            #region HARD PUZZLES

            int[,] hardPuzzle1 = 
                                   {{0, 2, 0,   0, 1, 3,    6, 0, 0 }, 
                                    {0, 0, 6,   8, 0, 0,    0, 0, 0 }, 
                                    {8, 0, 0,   0, 0, 0,    0, 9, 0 },
 
                                    {0, 1, 0,   6, 5, 0,    0, 0, 0 },
                                    {7, 0, 3,   0, 8, 0,    9, 0, 4 },
                                    {0, 0, 0,   0, 4, 9,    0, 6, 0 },

                                    {0, 3, 0,   0, 0, 0,    0, 0, 2 },
                                    {0, 0, 0,   0, 0, 8,    1, 0, 0 },
                                    {0, 0, 5,   7, 3, 0,    0, 4, 0 }};

            hardPuzzles.Add(hardPuzzle1);

            #endregion

            #region EVIL PUZZLES

            int[,] evilPuzzle1 = 
                                   {{0, 6, 0,   0, 0, 0,    0, 5, 2 }, 
                                    {0, 0, 0,   6, 0, 0,    0, 0, 1 }, 
                                    {7, 0, 5,   0, 0, 0,    0, 0, 0 },
 
                                    {8, 2, 0,   4, 0, 0,    0, 0, 0 },
                                    {0, 7, 0,   3, 0, 5,    0, 9, 0 },
                                    {0, 0, 0,   0, 0, 8,    0, 2, 3 },

                                    {0, 0, 0,   0, 0, 0,    5, 0, 9 },
                                    {1, 0, 0,   0, 0, 6,    0, 0, 0 },
                                    {4, 3, 0,   0, 0, 0,    0, 8, 0 }};

            evilPuzzles.Add(evilPuzzle1);

            int[,] evilPuzzle2 = 
                                   {{0, 0, 0,   7, 4, 0,    0, 0, 9 }, 
                                    {0, 1, 0,   0, 0, 0,    0, 6, 0 }, 
                                    {0, 6, 7,   0, 9, 0,    0, 0, 0 },
 
                                    {0, 0, 0,   0, 5, 2,    0, 9, 7 },
                                    {0, 0, 4,   0, 0, 0,    2, 0, 0 },
                                    {5, 2, 0,   8, 7, 0,    0, 0, 0 },

                                    {0, 0, 0,   0, 8, 0,    1, 2, 0 },
                                    {0, 5, 0,   0, 0, 0,    0, 3, 0 },
                                    {6, 0, 0,   0, 1, 3,    0, 0, 0 }};

            evilPuzzles.Add(evilPuzzle2);

            int[,] evilPuzzle3 = 
                                   {{0, 0, 0,   7, 0, 0,    8, 0, 0 }, 
                                    {0, 4, 0,   0, 1, 8,    0, 0, 0 }, 
                                    {7, 0, 3,   0, 0, 0,    9, 0, 0 },
 
                                    {0, 3, 0,   0, 2, 0,    0, 5, 0 },
                                    {5, 0, 0,   3, 0, 7,    0, 0, 8 },
                                    {0, 2, 0,   0, 6, 0,    0, 7, 0 },

                                    {0, 0, 6,   0, 0, 0,    4, 0, 2 },
                                    {0, 0, 0,   9, 8, 0,    0, 3, 0 },
                                    {0, 0, 5,   0, 0, 4,    0, 0, 0 }};

            evilPuzzles.Add(evilPuzzle3);

            int[,] evilPuzzle4 = 
                                   {{1, 5, 0,   0, 0, 0,    0, 7, 8 }, 
                                    {3, 7, 0,   0, 4, 0,    0, 6, 1 }, 
                                    {0, 0, 2,   0, 0, 0,    9, 0, 0 },
 
                                    {0, 0, 0,   5, 0, 2,    0, 0, 0 },
                                    {0, 9, 0,   0, 0, 0,    0, 2, 0 },
                                    {0, 0, 0,   1, 0, 6,    0, 0, 0 },

                                    {0, 0, 4,   0, 0, 0,    5, 0, 0 },
                                    {7, 2, 0,   0, 3, 0,    0, 9, 6 },
                                    {9, 3, 0,   0, 0, 0,    0, 4, 2 }};

            evilPuzzles.Add(evilPuzzle4);

            #endregion

            #region TEST PUZZLE

            int[,] testPuzzle1 = 
                                   {{9, 0, 0,   0, 0, 0,    0, 0, 3 }, 
                                    {0, 0, 4,   1, 0, 6,    2, 0, 0 }, 
                                    {0, 1, 0,   0, 3, 0,    0, 6, 0 },
 
                                    {0, 9, 0,   0, 5, 0,    0, 3, 0 },
                                    {0, 0, 8,   9, 0, 2,    6, 0, 0 },
                                    {0, 4, 0,   0, 6, 0,    0, 8, 0 },

                                    {0, 5, 0,   0, 4, 0,    0, 1, 0 },
                                    {0, 0, 1,   5, 0, 8,    3, 0, 0 },
                                    {8, 0, 0,   0, 0, 0,    0, 0, 7 }};

            testPuzzles.Add(testPuzzle1);

            #endregion

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    sectorizedAllowableRangeMap[i, j] = new List<int>[3, 3];

            DisableCalculationButtons();
        }

        #region GET-PUZZLE BUTTONS

        private void btnGetEasyPuzzles_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            int[,] sourceArray = easyPuzzles[rand.Next(0, easyPuzzles.Count)];
            Array.Copy(sourceArray, 0, currentMatrixState, 0, sourceArray.Length);

            BackendToDisplay();

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;

                    if (((TextBox)x).Text != "")
                    {
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                        ((TextBox)x).ReadOnly = true;
                    }
                }
            }

            lblMainPuzzle.Text = "Puzzle Instance - Easy";
            lblMainPuzzle.ForeColor = btnGetEasyPuzzles.ForeColor;

            RangeMapToDisplay();

            EnableButtonsAfterCalculation();
        }

        private void btnGetMediumPuzzles_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            int[,] sourceArray = mediumPuzzles[rand.Next(0, mediumPuzzles.Count)];
            Array.Copy(sourceArray, 0, currentMatrixState, 0, sourceArray.Length);

            BackendToDisplay();

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;

                    if (((TextBox)x).Text != "")
                    {
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                        ((TextBox)x).ReadOnly = true;
                    }
                }
            }

            lblMainPuzzle.Text = "Puzzle Instance - Medium";
            lblMainPuzzle.ForeColor = btnGetMediumPuzzles.ForeColor;

            RangeMapToDisplay();

            EnableButtonsAfterCalculation();
        }

        private void btnGetHardPuzzles_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            int[,] sourceArray = hardPuzzles[rand.Next(0, hardPuzzles.Count)];
            Array.Copy(sourceArray, 0, currentMatrixState, 0, sourceArray.Length);

            BackendToDisplay();

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;

                    if (((TextBox)x).Text != "")
                    {
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                        ((TextBox)x).ReadOnly = true;
                    }
                }
            }

            lblMainPuzzle.Text = "Puzzle Instance - Hard";
            lblMainPuzzle.ForeColor = btnGetHardPuzzles.ForeColor;

            RangeMapToDisplay();

            EnableButtonsAfterCalculation();
        }

        private void btnGetEvilPuzzles_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            int[,] sourceArray = evilPuzzles[rand.Next(0, evilPuzzles.Count)];
            Array.Copy(sourceArray, 0, currentMatrixState, 0, sourceArray.Length);

            BackendToDisplay();

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;

                    if (((TextBox)x).Text != "")
                    {
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                        ((TextBox)x).ReadOnly = true;
                    }
                }
            }

            lblMainPuzzle.Text = "Puzzle Instance - Hellish";
            lblMainPuzzle.ForeColor = btnGetEvilPuzzles.ForeColor;

            RangeMapToDisplay();

            EnableButtonsAfterCalculation();
        }

        private void btnTestPuzzle_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            int[,] sourceArray = testPuzzles[rand.Next(0, testPuzzles.Count)];
            Array.Copy(sourceArray, 0, currentMatrixState, 0, sourceArray.Length);

            BackendToDisplay();

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;

                    if (((TextBox)x).Text != "")
                    {
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                        ((TextBox)x).ReadOnly = true;
                    }
                }
            }

            lblMainPuzzle.Text = "Puzzle Instance - Test";
            lblMainPuzzle.ForeColor = btnTestPuzzle.ForeColor;

            RangeMapToDisplay();

            EnableButtonsAfterCalculation();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            SavePuzzleState();

            ConsoleWriteLine("CURRENT PUZZLE STATE SAVED!");
            ConsoleWriteLine();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    ConsoleWrite(currentMatrixState[i, j].ToString().PadRight(6));
                }

                ConsoleWriteLine();
            }

            ConsoleWriteLine();
            ConsoleWriteLine("CURRENT PUZZLE STATE SAVED!");
            ConsoleWriteLine();

            EnableButtonsAfterCalculation();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            LoadPuzzleState();

            int[,] sourceArray = loadedPuzzleState;
            Array.Copy(sourceArray, 0, currentMatrixState, 0, sourceArray.Length);

            BackendToDisplay();

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;

                    if (((TextBox)x).Text != "")
                    {
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                        ((TextBox)x).ReadOnly = true;
                    }
                }
            }

            RangeMapToDisplay();

            lblMainPuzzle.Text = "Loaded Save State";
            lblMainPuzzle.ForeColor = Color.Black;

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        #endregion 

        #region CALCULATION-RELATED BUTTONS

        private void btnEliminateForbiddenColumnsAndRowValues_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnCompleteExposedSingles_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            txtConsole.Clear();
            CompleteImplicitFirstDegreeValues();

            txtConsole.Clear();
            BackendToDisplay();

            txtConsole.Clear();
            RangeMapToDisplay();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnIdentifyHiddenSingles_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            txtConsole.Clear();
            CompleteImplicitSectorValues();

            txtConsole.Clear();
            BackendToDisplay();

            txtConsole.Clear();
            RangeMapToDisplay();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnUseDoublesToEliminateRowAndColumnRanges_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            txtConsole.Clear();

            UseExposedDoublesToEliminateCandidatesFromRows();
            UseExposedDoublesToEliminateCandidatesFromColumns();
            UseExposedDoublesToEliminateCandidatesFromSectors();

            RangeMapToDisplayWithoutRebuildingRangeMap();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnIdentifyAndExposeTriplets_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            
            txtConsole.Clear();
            IdentifyAndExposeTripletsInRows();

            txtConsole.Clear();
            IdentifyAndExposeTripletsInColumns();

            txtConsole.Clear();
            IdentifyAndExposeTripletsInSectors();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnIdentifyAndExposeQuadrets_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();
            
            txtConsole.Clear();
            IdentifyAndExposeQuadretsInRows();

            txtConsole.Clear();
            IdentifyAndExposeQuadretsInColumns();

            txtConsole.Clear();
            IdentifyAndExposeQuadretsInSectors();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnIdentifyAndExposeQuintets_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            txtConsole.Clear();
            IdentifyAndExposeQuintetsInRows();

            txtConsole.Clear();
            IdentifyAndExposeQuintetsInColumns();

            txtConsole.Clear();
            IdentifyAndExposeQuintetsInSectors();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnIdentifyAndExposeHexets_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            txtConsole.Clear();
            IdentifyAndExposeHexetsInRows();

            txtConsole.Clear();
            IdentifyAndExposeHexetsInColumns();

            txtConsole.Clear();
            IdentifyAndExposeHexetsInSectors();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        private void btnAttemptOneClickSolve_Click(object sender, EventArgs e)
        {
            DisableButtonsBeforeCalculation();

            while (true)
            {
                bool exposedSingleValues = false;

                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (currentMatrixState[i, j] == 0 && allowableRangeMap[i, j].Count == 1)
                            exposedSingleValues = true;
                    }
                }

                if (exposedSingleValues == true)
                    AttemptCompleteExposedSingleValues();

                else
                {
                    AttemptCompleteImplicitSectorValues();
                    UseDoublesToEliminateRowsAndColumnRanges();
                    IdentifyAndExposeTriplets();
                    IdentifyAndExposeQuadrets();
                    UseDoublesToEliminateRowsAndColumnRanges();
                }

                int releaseCounter = 0;
                foreach (List<int> list in allowableRangeMap)
                    foreach (int number in list)
                        releaseCounter++;

                if (releaseCounter == 81)
                {
                    AttemptCompleteExposedSingleValues();
                    break;
                }  
            }

            CheckIfPuzzleIsSolved();
            EnableButtonsAfterCalculation();
        }

        #endregion

        #region MODULES OF METHODS

        private void AttemptCompleteExposedSingleValues()
        {
            txtConsole.Clear();
            CompleteImplicitFirstDegreeValues();

            txtConsole.Clear();
            BackendToDisplay();

            txtConsole.Clear();
            RangeMapToDisplay();
        }

        private void AttemptCompleteImplicitSectorValues()
        {
            txtConsole.Clear();
            CompleteImplicitSectorValues();

            txtConsole.Clear();
            BackendToDisplay();

            txtConsole.Clear();
            RangeMapToDisplay();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void UseDoublesToEliminateRowsAndColumnRanges()
        {
            txtConsole.Clear();

            UseExposedDoublesToEliminateCandidatesFromRows();
            UseExposedDoublesToEliminateCandidatesFromColumns();
            UseExposedDoublesToEliminateCandidatesFromSectors();

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeTriplets()
        {
            txtConsole.Clear();
            IdentifyAndExposeTripletsInRows();

            txtConsole.Clear();
            IdentifyAndExposeTripletsInColumns();

            txtConsole.Clear();
            IdentifyAndExposeTripletsInSectors();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuadrets()
        {
            txtConsole.Clear();
            IdentifyAndExposeQuadretsInRows();

            txtConsole.Clear();
            IdentifyAndExposeQuadretsInColumns();

            txtConsole.Clear();
            IdentifyAndExposeQuadretsInSectors();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableRowValues();

            txtConsole.Clear();
            EliminateImplicitlyNonAllowableColumnValues();

            txtConsole.Clear();
            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        #endregion

        private List<int> CollectTargetRowValues(int[,] array, int i)
        {
            CurrentAction("Collecting Allowable Row Values");

            List<int> row = new List<int>();

            for (int a = 0; a < array.GetLength(0); a++)
            {
                row.Add(array[i, a]);
            }

            return row;
        }

        private List<int> CollectTargetColumnValues(int[,] array, int j)
        {
            CurrentAction("Collecting Allowable Column Values");

            List<int> column = new List<int>();

            for (int a = 0; a < array.GetLength(0); a++)
            {
                column.Add(array[a, j]);
            }

            return column;
        }

        private List<int> CollectSurroundingValues(int[,]array, int i, int j)
        {
            CurrentAction("Collecting Allowable Sector Values");

            List<int> surroundingValues = new List<int>();

            // First sector
            if (i < 3 && j < 3)
            {
                for (int a = 0; a < 3; a++)
                    for (int b = 0; b < 3; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }
            
            // Second sector
            else if (i < 3 && j > 2 && j < 6)
            {
                for (int a = 0; a < 3; a++)
                    for (int b = 3; b < 6; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Third sector
            else if (i < 3 && j > 5)
            {
                for (int a = 0; a < 3; a++)
                    for (int b = 6; b < 9; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Fourth sector
            else if (i > 2 && i < 6 && j < 3)
            {
                for (int a = 3; a < 6; a++)
                    for (int b = 0; b < 3; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Fifth sector
            else if (i > 2 && i < 6 && j > 2 && j < 6)
            {
                for (int a = 3; a < 6; a++)
                    for (int b = 3; b < 6; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Sixth sector
            else if (i > 2 && i < 6 && j > 5)
            {
                for (int a = 3; a < 6; a++)
                    for (int b = 6; b < 9; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Seventh sector
            else if (i > 5 && j < 3)
            {
                for (int a = 6; a < 9; a++)
                    for (int b = 0; b < 3; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Eighth sector
            else if (i > 5 && j > 2 && j < 6)
            {
                for (int a = 6; a < 9; a++)
                    for (int b = 3; b < 6; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            // Ninth sector
            else if (i > 5 && j > 5)
            {
                for (int a = 6; a < 9; a++)
                    for (int b = 6; b < 9; b++)
                    {
                        surroundingValues.Add(array[a, b]);
                    }
            }

            return surroundingValues;
        }

        private List<int> UnionTargetRowColumnSurroundingValues(List<int> rowValues, List<int> columnValues, List<int> surroundingValues)
        {
            CurrentAction("Unionizing Sets");

            var listA = new List<int>(rowValues);
            var listB = new List<int>(columnValues);
            var listC = new List<int>(surroundingValues);

            var listFirstUnion = listA.Union(listB);
            var listFinalUnion = listFirstUnion.Union(listC);

            List<int> exclusionaryValues = new List<int>(listFinalUnion);

            return exclusionaryValues;
        }

        private void BuildRangeMap()
        {
            CurrentAction("Building Heatmap");

            ConsoleWriteLine("BUILDING ALLOWABLE RANGE MAP");

            for (int i = 0; i < currentMatrixState.GetLength(0); i++)
                for (int j = 0; j < currentMatrixState.GetLength(1); j++)
                {
                    if (currentMatrixState[i, j] != 0)
                    {
                        allowableRangeMap[i, j] = new List<int>() { currentMatrixState[i, j] };
                    }

                    else if (currentMatrixState[i, j] == 0)
                    {
                        allowableRangeMap[i, j] = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                        txtConsole.Text += "Coordinate: " + "[" + i + ", " + j + "]" + "\n";

                        List<int> nonAllowableRowValues = CollectTargetRowValues(currentMatrixState, i);
                        txtConsole.Text += "Row Scan: ";
                        foreach (int number in nonAllowableRowValues)
                            txtConsole.Text += number.ToString();
                        ConsoleWriteLine();

                        List<int> nonAllowableColumnValues = CollectTargetColumnValues(currentMatrixState, j);
                        txtConsole.Text += "Column Scan: ";
                        foreach (int number in nonAllowableColumnValues)
                            txtConsole.Text += number.ToString();
                        ConsoleWriteLine();

                        List<int> nonAllowableSurroundingValues = CollectSurroundingValues(currentMatrixState, i, j);
                        txtConsole.Text += "Surrounding Scan: ";
                        foreach (int number in nonAllowableSurroundingValues)
                            txtConsole.Text += number.ToString();
                        ConsoleWriteLine();

                        List<int> nonAllowableValues = UnionTargetRowColumnSurroundingValues(nonAllowableRowValues, nonAllowableColumnValues, nonAllowableSurroundingValues);
                        txtConsole.Text += "Union Set: ";
                        foreach (int number in nonAllowableValues)
                            txtConsole.Text += number.ToString();
                        ConsoleWriteLine();

                        List<int> allowableValues = allowableRangeMap[i, j].Except(nonAllowableValues).ToList();
                        txtConsole.Text += "Allowable Set: ";
                        foreach (int number in allowableValues)
                            txtConsole.Text += number.ToString();
                        ConsoleWriteLine(); ConsoleWriteLine();

                        allowableRangeMap[i, j] = new List<int>();

                        foreach (int number in allowableValues)
                            allowableRangeMap[i, j].Add(number);
                    }
                }

            ConsoleWriteLine("ALLOWABLE RANGE MAP BUILT");
            ConsoleWriteLine();

            ConsoleWriteLine("BUILDING SECTORIZED ALLOWABLE RANGE MAP");
            ConsoleWriteLine();

            BuildSectorizedRangeMaps();

            ConsoleWriteLine("SECTORIZED ALLOWABLE RANGE MAP BUILT");
            ConsoleWriteLine();
        }

        private void BuildSectorizedRangeMaps()
        {
            CurrentAction("Building Sectorized Rangemaps");

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    ConsoleWriteLine("Sector " + "[" + (i) + ", " + (j) + "]");

                    for (int a = 0; a < 3; a++)
                        for (int b = 0; b < 3; b++)
                        {
                            sectorizedAllowableRangeMap[i, j][a, b] = allowableRangeMap[(i * 3 + a), (j * 3 + b)];

                            foreach (int number in allowableRangeMap[(i * 3 + a), (j * 3 + b)])
                            {
                                ConsoleWrite(number.ToString());
                            }    
                        }

                    ConsoleWriteLine(); ConsoleWriteLine();
                }                 
        }

        private void CompleteImplicitFirstDegreeValues()
        {
            CurrentAction("Attempt Solving Singles");

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (allowableRangeMap[i, j].Count == 1 && currentMatrixState[i, j] == 0)
                    {
                        ConsoleWriteLine("Coordinate " + "[" + i + ", " + j + "] " + "implicitly updated to " + allowableRangeMap[i, j][0]);
                        currentMatrixState[i, j] = allowableRangeMap[i, j][0];
                    }
                }
        }

        private void CompleteImplicitSectorValues()
        {
            CurrentAction("Attempt Solving Hidden Singles");

            List<int>[] uniqueValuesInSector = new List<int>[9];
            for (int i = 0; i < uniqueValuesInSector.Length; i++)
                uniqueValuesInSector[i] = new List<int>();

            List<int> uniqueValue = new List<int>();

            uniqueValuesInSector = FindUniqueAllowableValuesInSector();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            var uniqueIntersectAllowable = allowableRangeMap[(3 * i + a), (3 * j + b)].Intersect(uniqueValuesInSector[3 * i + j]);
                            uniqueValue = uniqueIntersectAllowable.ToList();

                            if (uniqueValue.Count > 0 && currentMatrixState[(3 * i + a), (3 * j + b)] == 0)
                                currentMatrixState[(3 * i + a), (3 * j + b)] = uniqueValue[0];
                        }
                    }
                }
            }
        }

        private List<int>[] FindUniqueAllowableValuesInSector()
        {
            CurrentAction("Gathering Unique Sector Values");

            List<int>[] allowableValuesInSector = new List<int>[9];
            for (int i = 0; i < 9; i++)
                allowableValuesInSector[i] = new List<int>();

            int[][] frequencyOfAllowableValuesInSector = new int[9][];
            for (int i = 0; i < frequencyOfAllowableValuesInSector.Length; i++)
                frequencyOfAllowableValuesInSector[i] = new int[9];

            List<int>[] uniqueValuesInSector = new List<int>[9];
            for (int i = 0; i < uniqueValuesInSector.Length; i++)
                uniqueValuesInSector[i] = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            if (currentMatrixState[(3 * i + a), (3 * j + b)] == 0)
                            {
                                allowableValuesInSector[(3 * i + j)].AddRange(allowableRangeMap[(3 * i + a), (3 * j + b)]);
                            }
                        }
                    }
                }
            }

            // Compiles a list of unique allowable values for each of the nine sectors.
            for (int i = 0; i < allowableValuesInSector.Length; i++)
                if (allowableValuesInSector[i] != null)
                {
                    for (int j = 0; j < allowableValuesInSector[i].Count; j++)
                    {
                        if (allowableValuesInSector[i][j] == 1)
                            frequencyOfAllowableValuesInSector[i][0]++;

                        else if (allowableValuesInSector[i][j] == 2)
                            frequencyOfAllowableValuesInSector[i][1]++;

                        else if (allowableValuesInSector[i][j] == 3)
                            frequencyOfAllowableValuesInSector[i][2]++;

                        else if (allowableValuesInSector[i][j] == 4)
                            frequencyOfAllowableValuesInSector[i][3]++;

                        else if (allowableValuesInSector[i][j] == 5)
                            frequencyOfAllowableValuesInSector[i][4]++;

                        else if (allowableValuesInSector[i][j] == 6)
                            frequencyOfAllowableValuesInSector[i][5]++;

                        else if (allowableValuesInSector[i][j] == 7)
                            frequencyOfAllowableValuesInSector[i][6]++;

                        else if (allowableValuesInSector[i][j] == 8)
                            frequencyOfAllowableValuesInSector[i][7]++;

                        else if (allowableValuesInSector[i][j] == 9)
                            frequencyOfAllowableValuesInSector[i][8]++;
                    }
                }

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (frequencyOfAllowableValuesInSector[i][j] == 1)
                    {
                        uniqueValuesInSector[i].Add(j+1);
                    }
                }

            for (int i = 0; i < allowableValuesInSector.Length; i++)
            {
                int sectorNumber = i + 1;
                txtConsole.Text += "Sector " + sectorNumber + " Allowable Element Count: ";

                if (allowableValuesInSector[i] != null)
                    foreach (int number in allowableValuesInSector[i])
                        txtConsole.Text += number;
                txtConsole.Text += "\n\n";
            }

            for (int i = 0; i < uniqueValuesInSector.Length; i++)
            {
                int sectorNumber = i + 1;
                txtConsole.Text += "Sector " + sectorNumber + " Unique Elements: ";

                if (uniqueValuesInSector[i] != null)
                    foreach (int number in uniqueValuesInSector[i])
                        txtConsole.Text += number;
                txtConsole.Text += "\n\n";
            }

            return uniqueValuesInSector;
        }

        private void EliminateImplicitlyNonAllowableRowValues()
        {
            CurrentAction("Eliminating Forbidden Row Values");

            bool[][,][] rowNumberExistBoolean = new bool[9][,][];
            for (int i = 0; i < 9; i++)
            {
                rowNumberExistBoolean[i] = new bool[3, 3][];
                for (int a = 0; a < 3; a++)
                    for (int b = 0; b < 3; b++)
                    {
                        rowNumberExistBoolean[i][a, b] = new bool[3];
                    }
            }

            for (int num = 1; num < 10; num++)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int a = 0; a < 3; a++)
                        {
                            for (int b = 0; b < 3; b++)
                            {
                                if (sectorizedAllowableRangeMap[i, j][a, b].Contains(num) == true && currentMatrixState[(3 * i + a), (3 * j + b)] == 0)
                                {
                                    rowNumberExistBoolean[num - 1][i, j][a] = true;
                                }
                            }

                            ConsoleWriteLine("Does sector " + "[" + i + ", " + j + "], row " + a + " contain " + num + "? " + rowNumberExistBoolean[num - 1][i, j][a].ToString());
                        }

                        int allowableNumberExistsInHowManyRowsPerSector = 0;

                        foreach (bool element in rowNumberExistBoolean[num - 1][i, j])
                        {
                            if (element == true)
                            {
                                allowableNumberExistsInHowManyRowsPerSector++;   
                            }
                        }

                        ConsoleWriteLine("Number " + num + " has " + allowableNumberExistsInHowManyRowsPerSector + " row possibilities in sector " + "[" + i + ", " + j + "].");
                        ConsoleWriteLine();

                        int rowTargetedForNumDeletion = -1;
                        int subRowExcludedFromNumDeletion = -1;

                        if (allowableNumberExistsInHowManyRowsPerSector == 1)
                        {
                            for (int r = 0; r < 3; r++)
                            {
                                if (rowNumberExistBoolean[num - 1][i, j][r] == true)
                                {
                                    rowTargetedForNumDeletion = 3 * i + r;
                                    subRowExcludedFromNumDeletion = j * 3;
                                }
                            }

                            ConsoleWriteLine("Row targeted for " + num + " deletion: " + rowTargetedForNumDeletion);
                            ConsoleWriteLine("Subrow excluded from " + num + " deletion: " + subRowExcludedFromNumDeletion + " to " + (subRowExcludedFromNumDeletion + 2));
                            ConsoleWriteLine();

                            for (int x = 0; x < 9; x++)
                            {
                                if (currentMatrixState[rowTargetedForNumDeletion, x] == 0 && 
                                    allowableRangeMap[rowTargetedForNumDeletion, x].Contains(num) == true &&
                                    (x > subRowExcludedFromNumDeletion + 2 || x < subRowExcludedFromNumDeletion))
                                {
                                    ConsoleWriteLine("Current matrix state @ [" + rowTargetedForNumDeletion + ", " + x + "]: " + currentMatrixState[rowTargetedForNumDeletion, x]);

                                    ConsoleWriteLine("allowable range @ [" + rowTargetedForNumDeletion + ", " + x + "]: ");
                                    foreach (int number in allowableRangeMap[rowTargetedForNumDeletion, x])
                                        ConsoleWrite(number.ToString());
                                    ConsoleWriteLine();
                                    ConsoleWriteLine("Deletion target > " + (subRowExcludedFromNumDeletion + 2) + " and " + "target < " + subRowExcludedFromNumDeletion);

                                    for (int d = 0; d < allowableRangeMap[rowTargetedForNumDeletion, x].Count; d++)
                                    {
                                        if (allowableRangeMap[rowTargetedForNumDeletion, x][d] == num)
                                        {
                                            allowableRangeMap[rowTargetedForNumDeletion, x].RemoveAt(d);
                                            ConsoleWriteLine("Deletion of " + num + " at [" + rowTargetedForNumDeletion + ", " + x + "] successful.");
                                        }
                                    }

                                    ConsoleWriteLine("allowable range @ [" + rowTargetedForNumDeletion + ", " + x + "]: ");
                                    foreach (int number in allowableRangeMap[rowTargetedForNumDeletion, x])
                                        ConsoleWrite(number.ToString());
                                    ConsoleWriteLine(); ConsoleWriteLine();
                                }
                            }
                        }
                    } 
                }
            }
        }

        private void EliminateImplicitlyNonAllowableColumnValues()
        {
            CurrentAction("Eliminating Forbidden Column Values");

            bool[][,][] columnNumberExistBoolean = new bool[9][,][];
            for (int i = 0; i < 9; i++)
            {
                columnNumberExistBoolean[i] = new bool[3, 3][];
                for (int b = 0; b < 3; b++)
                    for (int a = 0; a < 3; a++)
                    {
                        columnNumberExistBoolean[i][a, b] = new bool[3];
                    }
            }

            for (int num = 1; num < 10; num++)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            for (int a = 0; a < 3; a++)
                            {
                                if (sectorizedAllowableRangeMap[i, j][a, b].Contains(num) == true && currentMatrixState[(3 * i + a), (3 * j + b)] == 0)
                                {
                                    columnNumberExistBoolean[num - 1][i, j][b] = true;
                                }
                            }

                            ConsoleWriteLine("Does sector " + "[" + i + ", " + j + "], column " + b + " contain " + num + "? " + columnNumberExistBoolean[num - 1][i, j][b].ToString());
                        }

                        int allowableNumberExistsInHowManyColumnsPerSector = 0;

                        foreach (bool element in columnNumberExistBoolean[num - 1][i, j])
                        {
                            if (element == true)
                            {
                                allowableNumberExistsInHowManyColumnsPerSector++;
                            }
                        }

                        ConsoleWriteLine("Number " + num + " has " + allowableNumberExistsInHowManyColumnsPerSector + " column possibilities in sector " + "[" + i + ", " + j + "].");
                        ConsoleWriteLine();

                        int columnsTargetedForNumDeletion = -1;
                        int subColumnExcludedFromNumDeletion = -1;

                        if (allowableNumberExistsInHowManyColumnsPerSector == 1)
                        {
                            for (int r = 0; r < 3; r++)
                            {
                                if (columnNumberExistBoolean[num - 1][i, j][r] == true)
                                {
                                    columnsTargetedForNumDeletion = 3 * j + r;
                                    subColumnExcludedFromNumDeletion = i * 3;
                                }
                            }

                            ConsoleWriteLine("Column targeted for " + num + " deletion: " + columnsTargetedForNumDeletion);
                            ConsoleWriteLine("Subcolumn excluded from " + num + " deletion: " + subColumnExcludedFromNumDeletion + " to " + (subColumnExcludedFromNumDeletion + 2));
                            ConsoleWriteLine();

                            for (int x = 0; x < 9; x++)
                            {
                                if (currentMatrixState[x, columnsTargetedForNumDeletion] == 0 &&
                                    allowableRangeMap[x, columnsTargetedForNumDeletion].Contains(num) == true &&
                                    (x > subColumnExcludedFromNumDeletion + 2 || x < subColumnExcludedFromNumDeletion))
                                {
                                    ConsoleWriteLine("Current matrix state @ [" + x + ", " + columnsTargetedForNumDeletion + "]: " + currentMatrixState[x, columnsTargetedForNumDeletion]);

                                    ConsoleWriteLine("allowable range @ [" + x + ", " + columnsTargetedForNumDeletion + "]: ");
                                    foreach (int number in allowableRangeMap[x, columnsTargetedForNumDeletion])
                                        ConsoleWrite(number.ToString());
                                    ConsoleWriteLine();
                                    ConsoleWriteLine("Deletion target > " + (subColumnExcludedFromNumDeletion + 2) + " and " + "target < " + subColumnExcludedFromNumDeletion);

                                    for (int d = 0; d < allowableRangeMap[x, columnsTargetedForNumDeletion].Count; d++)
                                    {
                                        if (allowableRangeMap[x, columnsTargetedForNumDeletion][d] == num)
                                        {
                                            allowableRangeMap[x, columnsTargetedForNumDeletion].RemoveAt(d);
                                            ConsoleWriteLine("Deletion of " + num + " at [" + x + ", " + columnsTargetedForNumDeletion + "] successful.");
                                        }
                                    }

                                    ConsoleWriteLine("allowable range @ [" + x + ", " + columnsTargetedForNumDeletion + "]: ");
                                    foreach (int number in allowableRangeMap[x, columnsTargetedForNumDeletion])
                                        ConsoleWrite(number.ToString());
                                    ConsoleWriteLine(); ConsoleWriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UseExposedDoublesToEliminateCandidatesFromRows()
        {
            CurrentAction("Using 2xPair for Row Elimination");

            bool areBothSetsEqual = false;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    areBothSetsEqual = false;

                    if (allowableRangeMap[i, j].Count == 2)
                    {
                        for (int c = 0; c < 9; c++)
                        {
                            if (c != j && allowableRangeMap[i, c].Count == 2)
                            {
                                areBothSetsEqual = allowableRangeMap[i, j].All(allowableRangeMap[i, c].Contains);

                                ConsoleWriteLine("Are coordinates [" + i + ", " + j + " ] and [" + i + ", " + c + " ] equal? " + areBothSetsEqual);
                                ConsoleWriteLine();

                                if (areBothSetsEqual == true)
                                {
                                    for (int x = 0; x < 9; x++)
                                    {
                                        if (x != j && x != c)
                                        {
                                            if (allowableRangeMap[i, x].Contains(allowableRangeMap[i, j][0]))
                                            {
                                                ConsoleWriteLine("Value " + allowableRangeMap[i, j][0] + " Removed from coordinate [" + i + ", " + x + " ]");
                                                allowableRangeMap[i, x].Remove(allowableRangeMap[i, j][0]);
                                                ConsoleWriteLine();
                                            }

                                            if (allowableRangeMap[i, x].Contains(allowableRangeMap[i, j][1]))
                                            {
                                                ConsoleWriteLine("Value " + allowableRangeMap[i, j][1] + " Removed from coordinate [" + i + ", " + x + " ]");
                                                allowableRangeMap[i, x].Remove(allowableRangeMap[i, j][1]);
                                                ConsoleWriteLine();
                                            }            
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UseExposedDoublesToEliminateCandidatesFromColumns()
        {
            CurrentAction("Using 2xPair for Column Elimination");

            bool areBothSetsEqual = false;

            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    areBothSetsEqual = false;

                    if (allowableRangeMap[i, j].Count == 2)
                    {
                        for (int c = 0; c < 9; c++)
                        {
                            if (c != i && allowableRangeMap[c, j].Count == 2)
                            {
                                areBothSetsEqual = allowableRangeMap[i, j].All(allowableRangeMap[c, j].Contains);

                                ConsoleWriteLine("Are coordinates [" + i + ", " + j + " ] and [" + c + ", " + j + " ] equal? " + areBothSetsEqual);
                                ConsoleWriteLine();

                                if (areBothSetsEqual == true)
                                {
                                    for (int x = 0; x < 9; x++)
                                    {
                                        if (x != i && x != c)
                                        {
                                            if (allowableRangeMap[x, j].Contains(allowableRangeMap[i, j][0]))
                                            {
                                                ConsoleWriteLine("Value " + allowableRangeMap[i, j][0] + " Removed from coordinate [" + x + ", " + j + " ]");
                                                allowableRangeMap[x, j].Remove(allowableRangeMap[i, j][0]);
                                                ConsoleWriteLine();
                                            }

                                            if (allowableRangeMap[x, j].Contains(allowableRangeMap[i, j][1]))
                                            {
                                                ConsoleWriteLine("Value " + allowableRangeMap[i, j][1] + " Removed from coordinate [" + x + ", " + j + " ]");
                                                allowableRangeMap[x, j].Remove(allowableRangeMap[i, j][1]);
                                                ConsoleWriteLine();
                                            }  
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UseExposedDoublesToEliminateCandidatesFromSectors()
        {
            BuildSectorizedRangeMaps();
            CurrentAction("Using 2xPair for Sector Elimination");

            bool areBothSetsEqual = false;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            areBothSetsEqual = false;

                            if (sectorizedAllowableRangeMap[i, j][a, b].Count == 2)
                            {
                                ConsoleWriteLine("Coordinate [" + (3 * i + a) + ", " + (3 * j + b) + "] range count = 2.");
                                ConsoleWriteLine();

                                for (int m = 0; m < 3; m++)
                                {
                                    for (int n = 0; n < 3; n++)
                                    {
                                        if ((m != a || n != b) && sectorizedAllowableRangeMap[i, j][m, n].Count == 2)
                                        {
                                            areBothSetsEqual = sectorizedAllowableRangeMap[i, j][a, b].All(sectorizedAllowableRangeMap[i, j][m, n].Contains);

                                            ConsoleWriteLine("Are coordinates [" + (3 * i + a) + ", " + (3 * j + b) + "] and [" + (3 * i + m) + ", " + (3 * j + n) + "] equal? " + areBothSetsEqual);
                                            ConsoleWriteLine();

                                            if (areBothSetsEqual == true)
                                            {
                                                for (int x = 0; x < 3; x++)
                                                {
                                                    for (int y = 0; y < 3; y++)
                                                    {
                                                        if ((x != a || y != b) && (x != m || y != n))
                                                        {
                                                            if (sectorizedAllowableRangeMap[i, j][x, y].Contains(sectorizedAllowableRangeMap[i, j][a, b][0]))
                                                            {
                                                                ConsoleWriteLine("Value " + sectorizedAllowableRangeMap[i, j][a, b][0] + " Removed from coordinate [" + (3 * i + x) + ", " + (3 * j + y) + "]");
                                                                sectorizedAllowableRangeMap[i, j][x, y].Remove(sectorizedAllowableRangeMap[i, j][a, b][0]);

                                                                allowableRangeMap[(3 * i + x), (3 * j + y)] = sectorizedAllowableRangeMap[i, j][x, y];
                                                                ConsoleWriteLine();
                                                            }

                                                            if (sectorizedAllowableRangeMap[i, j][x, y].Contains(sectorizedAllowableRangeMap[i, j][a, b][1]))
                                                            {
                                                                ConsoleWriteLine("Value " + sectorizedAllowableRangeMap[i, j][a, b][1] + " Removed from coordinate [" + (3 * i + x) + ", " + (3 * j + y) + "]");
                                                                sectorizedAllowableRangeMap[i, j][x, y].Remove(sectorizedAllowableRangeMap[i, j][a, b][1]);

                                                                allowableRangeMap[(3 * i + x), (3 * j + y)] = sectorizedAllowableRangeMap[i, j][x, y];
                                                                ConsoleWriteLine();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void IdentifyAndExposeTripletsInRows()
        {
            CurrentAction("Scanning for Row Triplets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int j = 0; j < 9; j++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Row " + (i) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");
                    
                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 3 &&
                                listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 3 &&
                                listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 3 &&
                                a != b && a != c && b != c)
                            {
                                joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                joinedListOfOccurences.AddRange(listOfOccurences[c]);

                                var distinct = joinedListOfOccurences.Distinct();
                                joinedListOfOccurences = distinct.ToList();

                                if (joinedListOfOccurences.Count == 3)
                                {
                                    List<int> tripletValues = new List<int>();
                                    tripletValues.Add(a + 1); tripletValues.Add(b + 1); tripletValues.Add(c + 1);

                                    List<int> tripletPositions = new List<int>(joinedListOfOccurences);

                                    ConsoleWriteLine("Triplet Found on Row " + i + "! Values: " + (a + 1).ToString() + (b + 1).ToString() + (c + 1).ToString());
                                    ConsoleWrite("Triplet Coordinates: " + "[" + (tripletPositions[0] / 9) + ", " + (tripletPositions[0] % 9) + "], ");
                                    ConsoleWrite("[" + (tripletPositions[1] / 9) + ", " + (tripletPositions[1] % 9) + "], ");
                                    ConsoleWrite("[" + (tripletPositions[2] / 9) + ", " + (tripletPositions[2] % 9) + "]");
                                    ConsoleWriteLine(); ConsoleWriteLine();

                                    for (int x = 1; x < 10; x++ )
                                    {
                                        if (tripletValues.Contains(x) == false)
                                        {
                                            foreach (int position in tripletPositions)
                                            {
                                                if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                {
                                                    allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                    ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                }
                                            }
                                        }
                                    }

                                    ConsoleWriteLine();
                                }
                            }   
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeTripletsInColumns()
        {
            CurrentAction("Scanning for Column Triplets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int j = 0; j < 9; j++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int i = 0; i < 9; i++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Column " + (j) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 3 &&
                                listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 3 &&
                                listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 3 &&
                                a != b && a != c && b != c)
                            {
                                joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                joinedListOfOccurences.AddRange(listOfOccurences[c]);

                                var distinct = joinedListOfOccurences.Distinct();
                                joinedListOfOccurences = distinct.ToList();

                                if (joinedListOfOccurences.Count == 3)
                                {
                                    List<int> tripletValues = new List<int>();
                                    tripletValues.Add(a + 1); tripletValues.Add(b + 1); tripletValues.Add(c + 1);

                                    List<int> tripletPositions = new List<int>(joinedListOfOccurences);

                                    ConsoleWriteLine("Triplet Found on Column " + j + "! Values: " + (a + 1).ToString() + (b + 1).ToString() + (c + 1).ToString());
                                    ConsoleWrite("Triplet Coordinates: " + "[" + (tripletPositions[0] / 9) + ", " + (tripletPositions[0] % 9) + "], ");
                                    ConsoleWrite("[" + (tripletPositions[1] / 9) + ", " + (tripletPositions[1] % 9) + "], ");
                                    ConsoleWrite("[" + (tripletPositions[2] / 9) + ", " + (tripletPositions[2] % 9) + "]");
                                    ConsoleWriteLine(); ConsoleWriteLine();

                                    for (int x = 1; x < 10; x++)
                                    {
                                        if (tripletValues.Contains(x) == false)
                                        {
                                            foreach (int position in tripletPositions)
                                            {
                                                if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                {
                                                    allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                    ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                }
                                            }
                                        }
                                    }

                                    ConsoleWriteLine();
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeTripletsInSectors()
        {
            BuildSectorizedRangeMaps();
            CurrentAction("Scanning for Sector Triplets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int o = 0; o < 9; o++)
                        listOfOccurences[o] = new List<int>();

                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            foreach (int number in sectorizedAllowableRangeMap[i, j][a, b])
                            {
                                listOfOccurences[(number - 1)].Add((27 * i) + (3 * j) + (9 * a) + (1 * b));
                            }
                        }
                    }

                    ConsoleWriteLine("Sector " + (3 * i + j) + " List of Occurence: ");

                    for (int o = 0; o < 9; o++)
                    {
                        ConsoleWrite((o + 1) + ": ");

                        foreach (int number in listOfOccurences[o])
                            ConsoleWrite(number + ", ");

                        ConsoleWriteLine();
                    }

                    ConsoleWriteLine();

                    List<int> joinedListOfOccurences = new List<int>();

                    for (int a = 0; a < listOfOccurences.Length; a++)
                    {
                        for (int b = 1; b < listOfOccurences.Length; b++)
                        {
                            for (int c = 2; c < listOfOccurences.Length; c++)
                            {
                                if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 3 &&
                                listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 3 &&
                                listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 3 &&
                                a != b && a != c && b != c)
                                {
                                    joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[c]);

                                    var distinct = joinedListOfOccurences.Distinct();
                                    joinedListOfOccurences = distinct.ToList();

                                    if (joinedListOfOccurences.Count == 3)
                                    {
                                        List<int> tripletValues = new List<int>();
                                        tripletValues.Add(a + 1); tripletValues.Add(b + 1); tripletValues.Add(c + 1);

                                        List<int> tripletPositions = new List<int>(joinedListOfOccurences);

                                        ConsoleWriteLine("Triplet Found on Column " + j + "! Values: " + (a + 1).ToString() + (b + 1).ToString() + (c + 1).ToString());
                                        ConsoleWrite("Triplet Coordinates: " + "[" + (tripletPositions[0] / 9) + ", " + (tripletPositions[0] % 9) + "], ");
                                        ConsoleWrite("[" + (tripletPositions[1] / 9) + ", " + (tripletPositions[1] % 9) + "], ");
                                        ConsoleWrite("[" + (tripletPositions[2] / 9) + ", " + (tripletPositions[2] % 9) + "]");
                                        ConsoleWriteLine(); ConsoleWriteLine();

                                        for (int x = 1; x < 10; x++)
                                        {
                                            if (tripletValues.Contains(x) == false)
                                            {
                                                foreach (int position in tripletPositions)
                                                {
                                                    if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                    {
                                                        allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                        ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                    }
                                                }
                                            }
                                        }

                                        ConsoleWriteLine();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuadretsInRows()
        {
            CurrentAction("Scanning for Row Quadrets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int j = 0; j < 9; j++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Row " + (i) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            for (int d = 3; d < listOfOccurences.Length; d++)
                            {
                                if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 4 &&
                                                               listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 4 &&
                                                               listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 4 &&
                                                               listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 4 &&
                                                               a != b && a != c && a != d && 
                                                                         b != c && b != d &&
                                                                                   c != d)
                                {
                                    joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[d]);

                                    var distinct = joinedListOfOccurences.Distinct();
                                    joinedListOfOccurences = distinct.ToList();

                                    if (joinedListOfOccurences.Count == 4)
                                    {
                                        List<int> quadretValues = new List<int>();
                                        quadretValues.Add(a + 1); quadretValues.Add(b + 1); quadretValues.Add(c + 1); quadretValues.Add(d + 1);

                                        List<int> quadretPositions = new List<int>(joinedListOfOccurences);

                                        ConsoleWriteLine("Quadret Found on Row " + i + "! Values: " + (a + 1).ToString() + (b + 1).ToString() + 
                                                                                                      (c + 1).ToString() + (d + 1).ToString());

                                        ConsoleWrite("Quadret Coordinates: " + "[" + (quadretPositions[0] / 9) + ", " + (quadretPositions[0] % 9) + "], ");
                                        ConsoleWrite("[" + (quadretPositions[1] / 9) + ", " + (quadretPositions[1] % 9) + "], ");
                                        ConsoleWrite("[" + (quadretPositions[2] / 9) + ", " + (quadretPositions[2] % 9) + "], ");
                                        ConsoleWrite("[" + (quadretPositions[3] / 9) + ", " + (quadretPositions[3] % 9) + "], ");
                                        ConsoleWriteLine(); ConsoleWriteLine();

                                        for (int x = 1; x < 10; x++)
                                        {
                                            if (quadretValues.Contains(x) == false)
                                            {
                                                foreach (int position in quadretPositions)
                                                {
                                                    if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                    {
                                                        allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                        ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                    }
                                                }
                                            }
                                        }

                                        ConsoleWriteLine();
                                    }
                                }
                            } 
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuadretsInColumns()
        {
            CurrentAction("Scanning for Column Quadrets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int j = 0; j < 9; j++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int i = 0; i < 9; i++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Column " + (j) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            for (int d = 3; d < listOfOccurences.Length; d++)
                            {
                                if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 4 &&
                                                               listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 4 &&
                                                               listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 4 &&
                                                               listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 4 &&
                                                               a != b && a != c && a != d &&
                                                                         b != c && b != d &&
                                                                                   c != d)
                                {
                                    joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                    joinedListOfOccurences.AddRange(listOfOccurences[d]);

                                    var distinct = joinedListOfOccurences.Distinct();
                                    joinedListOfOccurences = distinct.ToList();

                                    if (joinedListOfOccurences.Count == 4)
                                    {
                                        List<int> quadretValues = new List<int>();
                                        quadretValues.Add(a + 1); quadretValues.Add(b + 1); quadretValues.Add(c + 1); quadretValues.Add(d + 1);

                                        List<int> quadretPositions = new List<int>(joinedListOfOccurences);

                                        ConsoleWriteLine("Quadret Found on Column " + j + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                      (c + 1).ToString() + (d + 1).ToString());

                                        ConsoleWrite("Quadret Coordinates: " + "[" + (quadretPositions[0] / 9) + ", " + (quadretPositions[0] % 9) + "], ");
                                        ConsoleWrite("[" + (quadretPositions[1] / 9) + ", " + (quadretPositions[1] % 9) + "], ");
                                        ConsoleWrite("[" + (quadretPositions[2] / 9) + ", " + (quadretPositions[2] % 9) + "], ");
                                        ConsoleWrite("[" + (quadretPositions[3] / 9) + ", " + (quadretPositions[3] % 9) + "], ");
                                        ConsoleWriteLine(); ConsoleWriteLine();

                                        for (int x = 1; x < 10; x++)
                                        {
                                            if (quadretValues.Contains(x) == false)
                                            {
                                                foreach (int position in quadretPositions)
                                                {
                                                    if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                    {
                                                        allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                        ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                    }
                                                }
                                            }
                                        }

                                        ConsoleWriteLine();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuadretsInSectors()
        {
            BuildSectorizedRangeMaps();
            CurrentAction("Scanning for Sector Quadrets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int o = 0; o < 9; o++)
                        listOfOccurences[o] = new List<int>();

                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            foreach (int number in sectorizedAllowableRangeMap[i, j][a, b])
                            {
                                listOfOccurences[(number - 1)].Add((27 * i) + (3 * j) + (9 * a) + (1 * b));
                            }
                        }
                    }

                    ConsoleWriteLine("Sector " + (3 * i + j) + " List of Occurence: ");

                    for (int o = 0; o < 9; o++)
                    {
                        ConsoleWrite((o + 1) + ": ");

                        foreach (int number in listOfOccurences[o])
                            ConsoleWrite(number + ", ");

                        ConsoleWriteLine();
                    }

                    ConsoleWriteLine();

                    List<int> joinedListOfOccurences = new List<int>();

                    for (int a = 0; a < listOfOccurences.Length; a++)
                    {
                        for (int b = 1; b < listOfOccurences.Length; b++)
                        {
                            for (int c = 2; c < listOfOccurences.Length; c++)
                            {
                                for (int d = 3; d < listOfOccurences.Length; d++)
                                {
                                    if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 4 &&
                                                                   listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 4 &&
                                                                   listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 4 &&
                                                                   listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 4 &&
                                                                   a != b && a != c && a != d &&
                                                                             b != c && b != d &&
                                                                                       c != d)
                                    {
                                        joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[d]);

                                        var distinct = joinedListOfOccurences.Distinct();
                                        joinedListOfOccurences = distinct.ToList();

                                        if (joinedListOfOccurences.Count == 4)
                                        {
                                            List<int> quadretValues = new List<int>();
                                            quadretValues.Add(a + 1); quadretValues.Add(b + 1); quadretValues.Add(c + 1); quadretValues.Add(d + 1);

                                            List<int> quadretPositions = new List<int>(joinedListOfOccurences);

                                            ConsoleWriteLine("Quadret Found in Sector " + (3 * i + j) + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                          (c + 1).ToString() + (d + 1).ToString());

                                            ConsoleWrite("Quadret Coordinates: " + "[" + (quadretPositions[0] / 9) + ", " + (quadretPositions[0] % 9) + "], ");
                                            ConsoleWrite("[" + (quadretPositions[1] / 9) + ", " + (quadretPositions[1] % 9) + "], ");
                                            ConsoleWrite("[" + (quadretPositions[2] / 9) + ", " + (quadretPositions[2] % 9) + "], ");
                                            ConsoleWrite("[" + (quadretPositions[3] / 9) + ", " + (quadretPositions[3] % 9) + "], ");
                                            ConsoleWriteLine(); ConsoleWriteLine();

                                            for (int x = 1; x < 10; x++)
                                            {
                                                if (quadretValues.Contains(x) == false)
                                                {
                                                    foreach (int position in quadretPositions)
                                                    {
                                                        if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                        {
                                                            allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                            ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                        }
                                                    }
                                                }
                                            }

                                            ConsoleWriteLine();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuintetsInRows()
        {
            CurrentAction("Scanning for Row Quintets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int j = 0; j < 9; j++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Row " + (i) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            for (int d = 3; d < listOfOccurences.Length; d++)
                            {
                                for (int e = 4; e < listOfOccurences.Length; e++)
                                {
                                    if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 5 &&
                                        listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 5 &&
                                        listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 5 &&
                                        listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 5 &&
                                        listOfOccurences[e].Count >= 2 && listOfOccurences[e].Count <= 5 &&
                                        a != b && a != c && a != d && a != e &&
                                                  b != c && b != d && b != e &&
                                                            c != d && c != e &&
                                                                      d != e)                                                          
                                    {
                                        joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[d]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[e]);

                                        var distinct = joinedListOfOccurences.Distinct();
                                        joinedListOfOccurences = distinct.ToList();

                                        if (joinedListOfOccurences.Count == 5)
                                        {
                                            List<int> quintetValues = new List<int>();
                                            quintetValues.Add(a + 1); quintetValues.Add(b + 1); quintetValues.Add(c + 1); quintetValues.Add(d + 1);
                                            quintetValues.Add(e + 1);

                                            List<int> quintetPositions = new List<int>(joinedListOfOccurences);

                                            ConsoleWriteLine("Quintet Found on Row " + i + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                          (c + 1).ToString() + (d + 1).ToString() +
                                                                                                          (e + 1).ToString());

                                            ConsoleWrite("Quintet Coordinates: " + "[" + (quintetPositions[0] / 9) + ", " + (quintetPositions[0] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[1] / 9) + ", " + (quintetPositions[1] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[2] / 9) + ", " + (quintetPositions[2] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[3] / 9) + ", " + (quintetPositions[3] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[4] / 9) + ", " + (quintetPositions[4] % 9) + "], ");
                                            ConsoleWriteLine(); ConsoleWriteLine();

                                            for (int x = 1; x < 10; x++)
                                            {
                                                if (quintetValues.Contains(x) == false)
                                                {
                                                    foreach (int position in quintetPositions)
                                                    {
                                                        if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                        {
                                                            allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                            ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                        }
                                                    }
                                                }
                                            }

                                            ConsoleWriteLine();
                                        }
                                    }
                                }  
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuintetsInColumns()
        {
            CurrentAction("Scanning for Column Quintets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int j = 0; j < 9; j++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int i = 0; i < 9; i++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Column " + (j) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            for (int d = 3; d < listOfOccurences.Length; d++)
                            {
                                for (int e = 4; e < listOfOccurences.Length; e++)
                                {
                                    if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 5 &&
                                        listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 5 &&
                                        listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 5 &&
                                        listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 5 &&
                                        listOfOccurences[e].Count >= 2 && listOfOccurences[e].Count <= 5 &&
                                        a != b && a != c && a != d && a != e &&
                                                  b != c && b != d && b != e &&
                                                            c != d && c != e &&
                                                                      d != e)
                                    {
                                        joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[d]);
                                        joinedListOfOccurences.AddRange(listOfOccurences[e]);

                                        var distinct = joinedListOfOccurences.Distinct();
                                        joinedListOfOccurences = distinct.ToList();

                                        if (joinedListOfOccurences.Count == 5)
                                        {
                                            List<int> quintetValues = new List<int>();
                                            quintetValues.Add(a + 1); quintetValues.Add(b + 1); quintetValues.Add(c + 1); quintetValues.Add(d + 1);
                                            quintetValues.Add(e + 1);

                                            List<int> quintetPositions = new List<int>(joinedListOfOccurences);

                                            ConsoleWriteLine("Quintet Found on Column " + j + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                          (c + 1).ToString() + (d + 1).ToString() +
                                                                                                          (e + 1).ToString());

                                            ConsoleWrite("Quintet Coordinates: " + "[" + (quintetPositions[0] / 9) + ", " + (quintetPositions[0] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[1] / 9) + ", " + (quintetPositions[1] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[2] / 9) + ", " + (quintetPositions[2] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[3] / 9) + ", " + (quintetPositions[3] % 9) + "], ");
                                            ConsoleWrite("[" + (quintetPositions[4] / 9) + ", " + (quintetPositions[4] % 9) + "], ");
                                            ConsoleWriteLine(); ConsoleWriteLine();

                                            for (int x = 1; x < 10; x++)
                                            {
                                                if (quintetValues.Contains(x) == false)
                                                {
                                                    foreach (int position in quintetPositions)
                                                    {
                                                        if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                        {
                                                            allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                            ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                        }
                                                    }
                                                }
                                            }

                                            ConsoleWriteLine();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeQuintetsInSectors()
        {
            BuildSectorizedRangeMaps();
            CurrentAction("Scanning for Sector Quintets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int o = 0; o < 9; o++)
                        listOfOccurences[o] = new List<int>();

                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            foreach (int number in sectorizedAllowableRangeMap[i, j][a, b])
                            {
                                listOfOccurences[(number - 1)].Add((27 * i) + (3 * j) + (9 * a) + (1 * b));
                            }
                        }
                    }

                    ConsoleWriteLine("Sector " + (3 * i + j) + " List of Occurence: ");

                    for (int o = 0; o < 9; o++)
                    {
                        ConsoleWrite((o + 1) + ": ");

                        foreach (int number in listOfOccurences[o])
                            ConsoleWrite(number + ", ");

                        ConsoleWriteLine();
                    }

                    ConsoleWriteLine();

                    List<int> joinedListOfOccurences = new List<int>();

                    for (int a = 0; a < listOfOccurences.Length; a++)
                    {
                        for (int b = 1; b < listOfOccurences.Length; b++)
                        {
                            for (int c = 2; c < listOfOccurences.Length; c++)
                            {
                                for (int d = 3; d < listOfOccurences.Length; d++)
                                {
                                    for (int e = 4; e < listOfOccurences.Length; e++)
                                    {
                                        if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 5 &&
                                            listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 5 &&
                                            listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 5 &&
                                            listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 5 &&
                                            listOfOccurences[e].Count >= 2 && listOfOccurences[e].Count <= 5 &&
                                            a != b && a != c && a != d && a != e &&
                                                      b != c && b != d && b != e &&
                                                                c != d && c != e &&
                                                                          d != e)
                                        {
                                            joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[d]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[e]);

                                            var distinct = joinedListOfOccurences.Distinct();
                                            joinedListOfOccurences = distinct.ToList();

                                            if (joinedListOfOccurences.Count == 5)
                                            {
                                                List<int> quintetValues = new List<int>();
                                                quintetValues.Add(a + 1); quintetValues.Add(b + 1); quintetValues.Add(c + 1); quintetValues.Add(d + 1);
                                                quintetValues.Add(e + 1);

                                                List<int> quintetPositions = new List<int>(joinedListOfOccurences);

                                                ConsoleWriteLine("Quintet Found in Sector " + (3 * i + j) + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                              (c + 1).ToString() + (d + 1).ToString() +
                                                                                                              (e + 1).ToString());

                                                ConsoleWrite("Quintet Coordinates: " + "[" + (quintetPositions[0] / 9) + ", " + (quintetPositions[0] % 9) + "], ");
                                                ConsoleWrite("[" + (quintetPositions[1] / 9) + ", " + (quintetPositions[1] % 9) + "], ");
                                                ConsoleWrite("[" + (quintetPositions[2] / 9) + ", " + (quintetPositions[2] % 9) + "], ");
                                                ConsoleWrite("[" + (quintetPositions[3] / 9) + ", " + (quintetPositions[3] % 9) + "], ");
                                                ConsoleWrite("[" + (quintetPositions[4] / 9) + ", " + (quintetPositions[4] % 9) + "], ");
                                                ConsoleWriteLine(); ConsoleWriteLine();

                                                for (int x = 1; x < 10; x++)
                                                {
                                                    if (quintetValues.Contains(x) == false)
                                                    {
                                                        foreach (int position in quintetPositions)
                                                        {
                                                            if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                            {
                                                                allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                                ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                            }
                                                        }
                                                    }
                                                }

                                                ConsoleWriteLine();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeHexetsInRows()
        {
            CurrentAction("Scanning for Row Hexets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int j = 0; j < 9; j++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Row " + (i) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            for (int d = 3; d < listOfOccurences.Length; d++)
                            {
                                for (int e = 4; e < listOfOccurences.Length; e++)
                                {
                                    for (int f = 5; f < listOfOccurences.Length; f++)
                                    {
                                        if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 6 &&
                                                                               listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 6 &&
                                                                               listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 6 &&
                                                                               listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 6 &&
                                                                               listOfOccurences[e].Count >= 2 && listOfOccurences[e].Count <= 6 &&
                                                                               listOfOccurences[f].Count >= 2 && listOfOccurences[f].Count <= 6 &&
                                                                               a != b && a != c && a != d && a != e && a != f &&
                                                                                         b != c && b != d && b != e && b != f &&
                                                                                                   c != d && c != e && c != f &&
                                                                                                             d != e && d != f &&
                                                                                                                       e != f)
                                        {
                                            joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[d]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[e]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[f]);

                                            var distinct = joinedListOfOccurences.Distinct();
                                            joinedListOfOccurences = distinct.ToList();

                                            if (joinedListOfOccurences.Count == 6)
                                            {
                                                List<int> hexetValues = new List<int>();
                                                hexetValues.Add(a + 1); hexetValues.Add(b + 1); hexetValues.Add(c + 1); hexetValues.Add(d + 1);
                                                hexetValues.Add(e + 1); hexetValues.Add(f + 1);

                                                List<int> hexetPositions = new List<int>(joinedListOfOccurences);

                                                ConsoleWriteLine("Hexet Found on Row " + i + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                              (c + 1).ToString() + (d + 1).ToString() +
                                                                                                              (e + 1).ToString() + (f + 1).ToString());

                                                ConsoleWrite("Hexet Coordinates: " + "[" + (hexetPositions[0] / 9) + ", " + (hexetPositions[0] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[1] / 9) + ", " + (hexetPositions[1] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[2] / 9) + ", " + (hexetPositions[2] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[3] / 9) + ", " + (hexetPositions[3] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[4] / 9) + ", " + (hexetPositions[4] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[5] / 9) + ", " + (hexetPositions[5] % 9) + "], ");
                                                ConsoleWriteLine(); ConsoleWriteLine();

                                                for (int x = 1; x < 10; x++)
                                                {
                                                    if (hexetValues.Contains(x) == false)
                                                    {
                                                        foreach (int position in hexetPositions)
                                                        {
                                                            if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                            {
                                                                allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                                ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                            }
                                                        }
                                                    }
                                                }

                                                ConsoleWriteLine();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeHexetsInColumns()
        {
            CurrentAction("Scanning for Column Hexets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int j = 0; j < 9; j++)
            {
                for (int o = 0; o < 9; o++)
                    listOfOccurences[o] = new List<int>();

                for (int i = 0; i < 9; i++)
                {
                    foreach (int number in allowableRangeMap[i, j])
                    {
                        listOfOccurences[(number - 1)].Add(9 * i + j);
                    }
                }

                ConsoleWriteLine("Column " + (j) + " List of Occurence: ");

                for (int o = 0; o < 9; o++)
                {
                    ConsoleWrite((o + 1) + ": ");

                    foreach (int number in listOfOccurences[o])
                        ConsoleWrite(number + ", ");

                    ConsoleWriteLine();
                }

                ConsoleWriteLine();

                List<int> joinedListOfOccurences = new List<int>();

                for (int a = 0; a < listOfOccurences.Length; a++)
                {
                    for (int b = 1; b < listOfOccurences.Length; b++)
                    {
                        for (int c = 2; c < listOfOccurences.Length; c++)
                        {
                            for (int d = 3; d < listOfOccurences.Length; d++)
                            {
                                for (int e = 4; e < listOfOccurences.Length; e++)
                                {
                                    for (int f = 5; f < listOfOccurences.Length; f++)
                                    {
                                        if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 6 &&
                                                                               listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 6 &&
                                                                               listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 6 &&
                                                                               listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 6 &&
                                                                               listOfOccurences[e].Count >= 2 && listOfOccurences[e].Count <= 6 &&
                                                                               listOfOccurences[f].Count >= 2 && listOfOccurences[f].Count <= 6 &&
                                                                               a != b && a != c && a != d && a != e && a != f &&
                                                                                         b != c && b != d && b != e && b != f &&
                                                                                                   c != d && c != e && c != f &&
                                                                                                             d != e && d != f &&
                                                                                                                       e != f)
                                        {
                                            joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[d]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[e]);
                                            joinedListOfOccurences.AddRange(listOfOccurences[f]);

                                            var distinct = joinedListOfOccurences.Distinct();
                                            joinedListOfOccurences = distinct.ToList();

                                            if (joinedListOfOccurences.Count == 6)
                                            {
                                                List<int> hexetValues = new List<int>();
                                                hexetValues.Add(a + 1); hexetValues.Add(b + 1); hexetValues.Add(c + 1); hexetValues.Add(d + 1);
                                                hexetValues.Add(e + 1); hexetValues.Add(f + 1);

                                                List<int> hexetPositions = new List<int>(joinedListOfOccurences);

                                                ConsoleWriteLine("Hexet Found on Column " + j + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                              (c + 1).ToString() + (d + 1).ToString() +
                                                                                                              (e + 1).ToString() + (f + 1).ToString());

                                                ConsoleWrite("Hexet Coordinates: " + "[" + (hexetPositions[0] / 9) + ", " + (hexetPositions[0] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[1] / 9) + ", " + (hexetPositions[1] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[2] / 9) + ", " + (hexetPositions[2] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[3] / 9) + ", " + (hexetPositions[3] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[4] / 9) + ", " + (hexetPositions[4] % 9) + "], ");
                                                ConsoleWrite("[" + (hexetPositions[5] / 9) + ", " + (hexetPositions[5] % 9) + "], ");
                                                ConsoleWriteLine(); ConsoleWriteLine();

                                                for (int x = 1; x < 10; x++)
                                                {
                                                    if (hexetValues.Contains(x) == false)
                                                    {
                                                        foreach (int position in hexetPositions)
                                                        {
                                                            if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                            {
                                                                allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                                ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                            }
                                                        }
                                                    }
                                                }

                                                ConsoleWriteLine();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void IdentifyAndExposeHexetsInSectors()
        {
            BuildSectorizedRangeMaps();
            CurrentAction("Scanning for Sector Hexets");

            List<int>[] listOfOccurences = new List<int>[9];
            for (int o = 0; o < 9; o++)
                listOfOccurences[o] = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int o = 0; o < 9; o++)
                        listOfOccurences[o] = new List<int>();

                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            foreach (int number in sectorizedAllowableRangeMap[i, j][a, b])
                            {
                                listOfOccurences[(number - 1)].Add((27 * i) + (3 * j) + (9 * a) + (1 * b));
                            }
                        }
                    }

                    ConsoleWriteLine("Sector " + (3 * i + j) + " List of Occurence: ");

                    for (int o = 0; o < 9; o++)
                    {
                        ConsoleWrite((o + 1) + ": ");

                        foreach (int number in listOfOccurences[o])
                            ConsoleWrite(number + ", ");

                        ConsoleWriteLine();
                    }

                    ConsoleWriteLine();

                    List<int> joinedListOfOccurences = new List<int>();

                    for (int a = 0; a < listOfOccurences.Length; a++)
                    {
                        for (int b = 1; b < listOfOccurences.Length; b++)
                        {
                            for (int c = 2; c < listOfOccurences.Length; c++)
                            {
                                for (int d = 3; d < listOfOccurences.Length; d++)
                                {
                                    for (int e = 4; e < listOfOccurences.Length; e++)
                                    {
                                        for (int f = 5; f < listOfOccurences.Length; f++)
                                        {
                                            if (listOfOccurences[a].Count >= 2 && listOfOccurences[a].Count <= 6 &&
                                                                                   listOfOccurences[b].Count >= 2 && listOfOccurences[b].Count <= 6 &&
                                                                                   listOfOccurences[c].Count >= 2 && listOfOccurences[c].Count <= 6 &&
                                                                                   listOfOccurences[d].Count >= 2 && listOfOccurences[d].Count <= 6 &&
                                                                                   listOfOccurences[e].Count >= 2 && listOfOccurences[e].Count <= 6 &&
                                                                                   listOfOccurences[f].Count >= 2 && listOfOccurences[f].Count <= 6 &&
                                                                                   a != b && a != c && a != d && a != e && a != f &&
                                                                                             b != c && b != d && b != e && b != f &&
                                                                                                       c != d && c != e && c != f &&
                                                                                                                 d != e && d != f &&
                                                                                                                           e != f)
                                            {
                                                joinedListOfOccurences.AddRange(listOfOccurences[a]);
                                                joinedListOfOccurences.AddRange(listOfOccurences[b]);
                                                joinedListOfOccurences.AddRange(listOfOccurences[c]);
                                                joinedListOfOccurences.AddRange(listOfOccurences[d]);
                                                joinedListOfOccurences.AddRange(listOfOccurences[e]);
                                                joinedListOfOccurences.AddRange(listOfOccurences[f]);

                                                var distinct = joinedListOfOccurences.Distinct();
                                                joinedListOfOccurences = distinct.ToList();

                                                if (joinedListOfOccurences.Count == 6)
                                                {
                                                    List<int> hexetValues = new List<int>();
                                                    hexetValues.Add(a + 1); hexetValues.Add(b + 1); hexetValues.Add(c + 1); hexetValues.Add(d + 1);
                                                    hexetValues.Add(e + 1); hexetValues.Add(f + 1);

                                                    List<int> hexetPositions = new List<int>(joinedListOfOccurences);

                                                    ConsoleWriteLine("Hexet Found in Sector " + (3 * j + i) + "! Values: " + (a + 1).ToString() + (b + 1).ToString() +
                                                                                                                  (c + 1).ToString() + (d + 1).ToString() +
                                                                                                                  (e + 1).ToString() + (f + 1).ToString());

                                                    ConsoleWrite("Hexet Coordinates: " + "[" + (hexetPositions[0] / 9) + ", " + (hexetPositions[0] % 9) + "], ");
                                                    ConsoleWrite("[" + (hexetPositions[1] / 9) + ", " + (hexetPositions[1] % 9) + "], ");
                                                    ConsoleWrite("[" + (hexetPositions[2] / 9) + ", " + (hexetPositions[2] % 9) + "], ");
                                                    ConsoleWrite("[" + (hexetPositions[3] / 9) + ", " + (hexetPositions[3] % 9) + "], ");
                                                    ConsoleWrite("[" + (hexetPositions[4] / 9) + ", " + (hexetPositions[4] % 9) + "], ");
                                                    ConsoleWrite("[" + (hexetPositions[5] / 9) + ", " + (hexetPositions[5] % 9) + "], ");
                                                    ConsoleWriteLine(); ConsoleWriteLine();

                                                    for (int x = 1; x < 10; x++)
                                                    {
                                                        if (hexetValues.Contains(x) == false)
                                                        {
                                                            foreach (int position in hexetPositions)
                                                            {
                                                                if (allowableRangeMap[(position / 9), (position % 9)].Contains(x) == true)
                                                                {
                                                                    allowableRangeMap[(position / 9), (position % 9)].Remove(x);
                                                                    ConsoleWriteLine("Removed " + x + " from : [" + (position / 9) + ", " + (position % 9) + "]");
                                                                }
                                                            }
                                                        }
                                                    }

                                                    ConsoleWriteLine();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            RangeMapToDisplayWithoutRebuildingRangeMap();
        }

        private void LoadLookupTable(string fileName, List<int[,]> list)
        {
            CurrentAction("Loading Lookup Table");

            try
            {
                using (Stream streamOpen = File.Open(fileName, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    var tempLoadedItem = (List<int[,]>)bin.Deserialize(streamOpen);

                    list = tempLoadedItem;
                }
            }

            catch (IOException ioe)
            {
                MessageBox.Show(ioe.Message);
            }
        }

        private void BackendToDisplay()
        {
            CurrentAction("Updating Cell States");

            List<int> currentMatrixState1D = new List<int>();

            foreach (int i in currentMatrixState)
                currentMatrixState1D.Add(i);

            foreach (Object x in tableLayoutPanel.Controls)
            {
                if (x is TextBox)
                {
                    ((TextBox)x).BackColor = Color.White;
                    ((TextBox)x).Text = currentMatrixState1D[0].ToString();

                    if (currentMatrixState1D[0] == 0)
                    {
                        ((TextBox)x).Text = "";
                        ((TextBox)x).ReadOnly = false;
                    }

                    else if (currentMatrixState1D[0] != 0)
                        ((TextBox)x).BackColor = Color.FromArgb(232, 232, 232);
                     
                    currentMatrixState1D.RemoveAt(0);
                }
            }
        }

        private void RangeMapToDisplay()
        {
            CurrentAction("Rendering Range Map");
            ConsoleWriteLine("RANGE MAP TO DISPLAY");

            BuildRangeMap();

            List<int> currentMatrixState1D = new List<int>();
            List<List<int>> allowableList1D = new List<List<int>>();

            for (int i = 0; i < allowableRangeMap.GetLength(0); i++)
                for (int j = 0; j <allowableRangeMap.GetLength(1); j++)
                {
                    List<int> entry = allowableRangeMap[i, j].ToList();
                    allowableList1D.Add(entry);
                    currentMatrixState1D.Add(currentMatrixState[i, j]);
                }

            int linearIndex = 0;

            foreach (Object x in sudokuRangeMapPanel.Controls)
            {
                if (x is RichTextBox)
                {
                    ((RichTextBox)x).Text = "";

                    if (allowableList1D[linearIndex].Count == 1 && currentMatrixState1D[linearIndex] != 0)
                    {
                        ((RichTextBox)x).Font = new Font("Arial", 6);
                        ((RichTextBox)x).ForeColor = Color.FromArgb(0, 128, 0);
                        ((RichTextBox)x).BackColor = Color.FromArgb(191, 205, 219);
                        ((RichTextBox)x).Text += "SOLVED";
                    }

                    else if (allowableList1D[linearIndex].Count != 0)
                    {
                        for (int j = 0; j < allowableList1D[linearIndex].Count; j++)
                        {
                            ((RichTextBox)x).Font = new Font("Arial", 8);
                            ((RichTextBox)x).ForeColor = Color.FromArgb(0, 0, 0);
                            ((RichTextBox)x).Text += allowableList1D[linearIndex][j].ToString();

                            if (allowableList1D[linearIndex].Count == 1)
                                ((RichTextBox)x).BackColor = Color.FromArgb(240, 240, 255);

                            else if (allowableList1D[linearIndex].Count == 2)
                                ((RichTextBox)x).BackColor = Color.FromArgb(225, 225, 240);

                            else if (allowableList1D[linearIndex].Count == 3)
                                ((RichTextBox)x).BackColor = Color.FromArgb(210, 210, 225);

                            else if (allowableList1D[linearIndex].Count > 3)
                                ((RichTextBox)x).BackColor = Color.FromArgb(195, 195, 210);
                        }
                    }

                    linearIndex++;   
                }
            }

            ConsoleWriteLine("RANGE MAP DISPLAYED");
        }

        private void RangeMapToDisplayWithoutRebuildingRangeMap()
        {
            CurrentAction("Rendering Range Map");

            ConsoleWriteLine("RANGE MAP TO DISPLAY");

            List<int> currentMatrixState1D = new List<int>();
            List<List<int>> allowableList1D = new List<List<int>>();

            for (int i = 0; i < allowableRangeMap.GetLength(0); i++)
                for (int j = 0; j < allowableRangeMap.GetLength(1); j++)
                {
                    List<int> entry = allowableRangeMap[i, j].ToList();
                    allowableList1D.Add(entry);
                    currentMatrixState1D.Add(currentMatrixState[i, j]);
                }

            int linearIndex = 0;

            foreach (Object x in sudokuRangeMapPanel.Controls)
            {
                if (x is RichTextBox)
                {
                    ((RichTextBox)x).Text = "";

                    if (allowableList1D[linearIndex].Count == 1 && currentMatrixState1D[linearIndex] != 0)
                    {
                        ((RichTextBox)x).Font = new Font("Arial", 6);
                        ((RichTextBox)x).ForeColor = Color.FromArgb(0, 128, 0);
                        ((RichTextBox)x).BackColor = Color.FromArgb(191, 205, 219);
                        ((RichTextBox)x).Text += "SOLVED";
                    }

                    else if (allowableList1D[linearIndex].Count != 0)
                    {
                        for (int j = 0; j < allowableList1D[linearIndex].Count; j++)
                        {
                            ((RichTextBox)x).Font = new Font("Arial", 8);
                            ((RichTextBox)x).ForeColor = Color.FromArgb(0, 0, 0);
                            ((RichTextBox)x).Text += allowableList1D[linearIndex][j].ToString();

                            if (allowableList1D[linearIndex].Count == 1)
                                ((RichTextBox)x).BackColor = Color.FromArgb(240, 240, 255);

                            else if (allowableList1D[linearIndex].Count == 2)
                                ((RichTextBox)x).BackColor = Color.FromArgb(225, 225, 240);

                            else if (allowableList1D[linearIndex].Count == 3)
                                ((RichTextBox)x).BackColor = Color.FromArgb(210, 210, 225);

                            else if (allowableList1D[linearIndex].Count > 3)
                                ((RichTextBox)x).BackColor = Color.FromArgb(195, 195, 210);
                        }
                    }

                    linearIndex++;
                }
            }

            ConsoleWriteLine("RANGE MAP DISPLAYED");
        }

        private void SavePuzzleState()
        {
            CurrentAction("Saving Puzzle State");

            try
            {
                File.Delete("SavedPuzzleState.bin");
                using (Stream streamCreate = File.Open("SavedPuzzleState.bin", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    bin.Serialize(streamCreate, currentMatrixState);
                }
            }

            catch (IOException ioe)
            {
                MessageBox.Show(ioe.Message);
            }
        }

        private void LoadPuzzleState()
        {
            CurrentAction("Loading Puzzle State");

            try
            {
                using (Stream streamOpen = File.Open("SavedPuzzleState.bin", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    var tempLoadedItem = (int[,])bin.Deserialize(streamOpen);

                    Array.Copy(tempLoadedItem, 0, loadedPuzzleState, 0, tempLoadedItem.Length);
                }
            }

            catch (IOException ioe)
            {
                MessageBox.Show(ioe.Message);
            }
        }

        private void CheckIfPuzzleIsSolved()
        {
            int solvedCells = 0;

            foreach (int cell in currentMatrixState)
                if (cell != 0)
                    solvedCells++;

            if (solvedCells >= 81)
                if (lblMainPuzzle.Text.Contains("[SOLVED!]") == false)
                    lblMainPuzzle.Text += " [SOLVED!]";
        }

        private void CurrentAction(string input)
        {
            lblCurrentAction.Text = input + "...";
        }

        private void ConsoleWrite(string message)
        {
            txtConsole.Text += message;
        }

        private void ConsoleWriteLine(string message)
        {
            txtConsole.Text += message + "\n";
        }

        private void ConsoleWriteLine()
        {
            txtConsole.Text += "\n";
        }

        private void txtConsole_TextChanged(object sender, EventArgs e)
        {
            txtConsole.SelectionStart = txtConsole.Text.Length;
            txtConsole.ScrollToCaret();
        }

        private void DisableCalculationButtons()
        {
            btnEliminateForbiddenColumnsAndRowValues.Enabled = false;
            btnCompleteExposedSingles.Enabled = false;
            btnIdentifyHiddenSingles.Enabled = false;
            btnUseDoublesToEliminateRowAndColumnRanges.Enabled = false;

            btnIdentifyAndExposeTriplets.Enabled = false;
            btnIdentifyAndExposeQuadrets.Enabled = false;
            btnIdentifyAndExposeQuintets.Enabled = false;
            btnIdentifyAndExposeHexets.Enabled = false;

            btnAttemptOneClickSolve.Enabled = false;
        }

        private void DisableButtonsBeforeCalculation()
        {
            foreach (Object x in this.Controls)
            {
                if (x is Button)
                {
                    ((Button)x).Enabled = false;
                }
            }
        }

        private void EnableButtonsAfterCalculation()
        {
            foreach (Object x in this.Controls)
            {
                if (x is Button)
                {
                    ((Button)x).Enabled = true;
                }
            }

            CurrentAction("");
        }
    }
}
