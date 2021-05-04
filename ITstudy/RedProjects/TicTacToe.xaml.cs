using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Used for cell coordinates
using System.Numerics;

// Image display, BitmapImage
using Windows.UI.Xaml.Media.Imaging;

// TODO remove if the app is ever finished
using System.Diagnostics;





namespace ITstudy.RedProjects
{
    /// <summary>
    /// A game of Tic-Tac-Toe between the user and the computer.
    /// </summary>
    public sealed partial class TicTacToe : Page
    {

        private class CellCoor
        {
            public int M { get; set; }
            public int N { get; set; }

            /// <summary>
            /// The coordinates of a single cell on the Gameboard.
            /// </summary>
            /// <param name="m">First coordinate, clamped between 0 and 2.</param>
            /// <param name="n">Second coordinate, clamped between 0 and 2.</param>
            public CellCoor(int m = 0, int n = 0)
            {
                this.M = Math.Clamp(m, 0, 2);
                this.N = Math.Clamp(n, 0, 2);
            }

        }





        // The types of behaviour that the computer can use when playing TicTacToe
        enum AIBehaviour { None, Random };
        public List<string> AIBehaviourList;

        // Players
        enum Players { Player, Computer };
        public List<string> PlayersList;

        // Representation of the Gameboard and its current cell states
        enum CellState { Empty, X, O };
        CellState[,] Gameboard = new CellState[3, 3];

        // Copy of gameboard with added bufferzone all around
        int[,] GameboardwBuffer = new int[5, 5];

        // Array of shifts in coordinates for all neighbours, clockwards starting at 3
        Vector2[] NeighbourCoordinates;

        // Cell coordinates to analyse, all possible locations for a row of three
        CellCoor[,] PossibleRows = new CellCoor[8, 3];

        // Path to TicTacToe symbol image files
        string SymbolX;
        string SymbolY;





        public TicTacToe()
        {
            this.InitializeComponent();
            FinishSetup();
            NewGame();
        }

        // General setup actions
        private void FinishSetup()
        {
            // Copy the enum of AIBehaviour to a List so it can be used by UI element(s)
            AIBehaviourList = new List<string>();
            foreach(string name in Enum.GetNames(typeof(AIBehaviour)))
            {
                AIBehaviourList.Add(name);
            }

            // Copy the enum of Players to a List so it can be used by UI element(s)

            // Set the default symbol for the player (0 for X, 1 for O)
            SelectPlayerSymbolComboBox.SelectedIndex = 0;

            // Set the default AI behaviour (see AIBehaviour enum for index values)
            AIBehaviourComboBox.SelectedIndex = 0;

            // Set the default starting player (0 for Player, 1 for Computer)
            StartingPlayerComboBox.SelectedIndex = 0;

            // Set the file-path for the TicTacToe symbol image files
            string symbolXPath = "Assets\\TicTacToe\\TicTacToeSymbolX.png";
            string symbolYPath = "Assets\\TicTacToe\\TicTacToeSymbolO.png";
            if (File.Exists(symbolXPath)) { SymbolX = symbolXPath; }
            else { Debug.WriteLine(string.Format("TicTacToe: Could not find image file for symbol X at {0}", symbolXPath)); }
            if (File.Exists(symbolYPath)) { SymbolY = symbolYPath; }
            else { Debug.WriteLine(string.Format("TicTacToe: Could not find image file for symbol X at {0}", symbolYPath)); }

            // Create a proxy representation of the gameboard with buffer zones to make finding existing neighbours simpler
            /* gameboard should look like this
             * 0 0 0 0 0
             * 0 1 1 1 0
             * 0 1 1 1 0
             * 0 1 1 1 0
             * 0 0 0 0 0
             */
            for (int gbFill1 = 0; gbFill1 < GameboardwBuffer.GetLength(0); gbFill1++)
            {
                for (int gbFill2 = 0; gbFill2 < GameboardwBuffer.GetLength(1); gbFill2++)
                {
                    if (gbFill1 == 0 || gbFill2 == 0 || gbFill1 == 4 || gbFill2 == 4)
                    {
                        GameboardwBuffer[gbFill1, gbFill2] = 0;
                    }
                    else
                    {
                        GameboardwBuffer[gbFill1, gbFill2] = 1;
                    }
                }
            }

            // Define the shifts in coordinates for all possible neighbours, clockwards starting at 3
            NeighbourCoordinates = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(1, -1),
                new Vector2(0, -1),
                new Vector2(-1, -1),
                new Vector2(-1, 0),
                new Vector2(-1, 1)
            };

            // Fill Rows will all possible locations for a row of three
            PossibleRows = new CellCoor[8, 3]
            {
                // Horizontal lines
                {  new CellCoor(0, 0), new CellCoor(0, 1), new CellCoor(0, 2) },
                {  new CellCoor(1, 0), new CellCoor(1, 1), new CellCoor(2, 2) },
                {  new CellCoor(2, 0), new CellCoor(2, 1), new CellCoor(2, 2) },

                // Vertical lines
                { new CellCoor(0, 0), new CellCoor(1, 0), new CellCoor(2, 0) },
                { new CellCoor(0, 1), new CellCoor(1, 1), new CellCoor(2, 1) },
                { new CellCoor(0, 2), new CellCoor(1, 2), new CellCoor(2, 2) },

                // Diagonal lines
                { new CellCoor(0, 0), new CellCoor(1, 1), new CellCoor(0, 2) },
                { new CellCoor(2, 0), new CellCoor(1, 1), new CellCoor(2, 2) }
            };

            // Fill Rows will all possible locations for a row of three
            PossibleRows = new CellCoor[8, 3]
            {
                // Lines starting in top left cell (0, 0)
                {  new CellCoor(0, 0), new CellCoor(0, 1), new CellCoor(0, 2) },
                {  new CellCoor(0, 0), new CellCoor(1, 0), new CellCoor(2, 0) },
                {  new CellCoor(0, 0), new CellCoor(1, 1), new CellCoor(2, 2) },

                // Lines containing the center cell (1, 1), excluding line (0, 0)-(1, 1)-(2, 2) already in array
                { new CellCoor(1, 1), new CellCoor(0, 1), new CellCoor(2, 1) },
                { new CellCoor(1, 1), new CellCoor(1, 0), new CellCoor(1, 2) },
                { new CellCoor(1, 1), new CellCoor(0, 2), new CellCoor(2, 0) },

                // Lines starting in the bottom right cell (2, 2), excluding lines already in array
                { new CellCoor(2, 2), new CellCoor(1, 2), new CellCoor(0, 2) },
                { new CellCoor(2, 2), new CellCoor(2, 1), new CellCoor(2, 0) }
            };
        }




        // Start a new game, sets all cells of Gameboard to CellState.Empty
        private void NewGameButton_Click(object sender, RoutedEventArgs e) { NewGame(); }
        private void NewGame()
        {
            // Clear the Gameboard, set the state of all cells to Empty
            for (int i = 0; i < Gameboard.GetLength(0); i++)
            {
                for (int j = 0; j < Gameboard.GetLength(1); j++)
                {
                    // Gameboard[i, j] = CellState.Empty;
                    UpdateGameboardCell(i, j, CellState.Empty);
                }
            }

            // Set the opacity of all row-of-three lines to 0 (invisible)
            GameboardLine00To02.Opacity = 0;
            GameboardLine10To12.Opacity = 0;
            GameboardLine20To22.Opacity = 0;
            GameboardLine00To20.Opacity = 0;
            GameboardLine01To21.Opacity = 0;
            GameboardLine02To22.Opacity = 0;
            GameboardLine00To22.Opacity = 0;
            GameboardLine02To20.Opacity = 0;
        }




        // Registers a mouse-click on a cell in gameboard
        private void GameboardButton_Click(object sender, RoutedEventArgs e)
        {
            int m = 0;
            int n = 0;

            Button b = sender as Button;
            string button = b.Name;

            switch (button)
            {
                case "GameboardButton00": { m = 0; n = 0; break; }
                case "GameboardButton01": { m = 0; n = 1; break; }
                case "GameboardButton02": { m = 0; n = 2; break; }
                case "GameboardButton10": { m = 1; n = 0; break; }
                case "GameboardButton11": { m = 1; n = 1; break; }
                case "GameboardButton12": { m = 1; n = 2; break; }
                case "GameboardButton20": { m = 2; n = 0; break; }
                case "GameboardButton21": { m = 2; n = 1; break; }
                case "GameboardButton22": { m = 2; n = 2; break; }
            }

            // Debug.WriteLine(string.Format("TicTacToe: GameboardButton_Click trying to change image at {0} from {1} to {2}", button, Gameboard[m, n].ToString(), Gameboard[m, n].Next().ToString()));

            UpdateGameboardCell(m, n, Gameboard[m, n].Next());
        }


        private bool IsSelectedCellValid(Players player, CellCoor cell)
        {
            if (Gameboard[cell.M, cell.N] == CellState.Empty) { return true; }
            else { return false; }
        }


        /// <summary>
        /// Update a cell on the Gameboard to the desired CellState. Returns true if succesfull, false if not.
        /// </summary>
        /// <param name="m">The x coordinate of the cell that should be changed. Must be between 0 and 2.</param>
        /// <param name="n">The y coordinate of the cell that should be changed. Must be between 0 and 2.</param>
        /// <param name="cellState">The desired state the cell will be changed to.</param>
        private bool UpdateGameboardCell(int m, int n, CellState cellState)
        {
            // Limit x and y to between 0 and 2
            if (m < 0 || m > 2) { Debug.WriteLine(string.Format("TicTacToe: UpdateGameboardCell recieved invalid m-coordinate for cell.")); return false; }
            if (n < 0 || n > 2) { Debug.WriteLine(string.Format("TicTacToe: UpdateGameboardCell recieved invalid n-coordinate for cell.")); return false; }

            // Update the CellState in the Gameboard array to the new cellState
            if (cellState > (CellState)2) { cellState = CellState.Empty; }
            Gameboard[m, n] = cellState;

            // Select the correct symbol image based on cellState
            string imagePath = string.Empty;
            if (cellState == CellState.Empty) { imagePath = string.Empty; }
            else if (Gameboard[m, n] == CellState.X) { imagePath = SymbolX; }
            else if (Gameboard[m, n] == CellState.O) { imagePath = SymbolY; }
            else { Debug.WriteLine(string.Format("TicTacToe: UpdateGameboardCell failed to determine the correct symbol to use from cellState")); return false; }
            BitmapImage newImage = new BitmapImage(new Uri("ms-appx:///" + imagePath));

            // Apply the new symbol image to the correct GameboardImage
            // (The use of a double-argument switch requires language version C# 8.0, the version was 7.3 originally, this is set manually in the .csproj file at the very end of the file, in case this change causes any problems)
            switch (m, n)
            {
                case (0, 0): { GameboardImage00.Source = newImage; break; }
                case (0, 1): { GameboardImage01.Source = newImage; break; }
                case (0, 2): { GameboardImage02.Source = newImage; break; }

                case (1, 0): { GameboardImage10.Source = newImage; break; }
                case (1, 1): { GameboardImage11.Source = newImage; break; }
                case (1, 2): { GameboardImage12.Source = newImage; break; }

                case (2, 0): { GameboardImage20.Source = newImage; break; }
                case (2, 1): { GameboardImage21.Source = newImage; break; }
                case (2, 2): { GameboardImage22.Source = newImage; break; }

                default: { return false; }
            }

            return true;

        }



        /// <summary>
        /// Analyses the current state of the Gameboard and will try to find a row of three identical symbols.
        /// If it finds one it will return the coordinates of the cells as an array, CellCoor[3], if it failed to find anything this array will only contain zeros.
        /// </summary>
        /// <returns></returns>
        private CellCoor[] AnalyseGameboard()
        {
            Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() is analysing."));

            // Array to return, set to default of all zero
            CellCoor[] coordinates = new CellCoor[3]
            {
                new CellCoor(),
                new CellCoor(),
                new CellCoor()
            };

            // Go over every possible location of a row of three and compare the symbols in those cells
            for (int i = 0; i < PossibleRows.GetLength(0); i+=3)
            {
                Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() looking at line {0}", i.ToString()));

                // if the first symbol is not Empty, and the all symbols match
                if (Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N] != CellState.Empty)
                {
                    Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() first cell was not empty."));


                }



                else { Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() found CellState.{0} at ({1},{2})", Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N].ToString(), PossibleRows[i, 0].M, PossibleRows[i, 0].N)); }
            }




            /* OLD VERSION 2, look at each possible row of three and see if the symbols match (doesn't work in current state)
            
            // Go over every possible location of a row of three and compare the symbols in those cells
            for (int i = 0; i < PossibleRows.GetLength(0); i++)
            {
                Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() looking at line {0}", i.ToString()));

                // if the first symbol is not Empty, and the first matches the second and thrid
                if (Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N] != CellState.Empty &&
                    Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N] == Gameboard[PossibleRows[i, 1].M, PossibleRows[i, 1].N] &&
                    Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N] == Gameboard[PossibleRows[i, 2].M, PossibleRows[i, 2].N])
                {
                    coordinates[0] = new CellCoor(PossibleRows[i, 0].M, PossibleRows[i, 0].N);
                    coordinates[1] = new CellCoor(PossibleRows[i, 1].M, PossibleRows[i, 1].N);
                    coordinates[2] = new CellCoor(PossibleRows[i, 2].M, PossibleRows[i, 2].N);
                    Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() found a row of three at ({0}, {1}) ({2}, {3}) ({4}, {5})", coordinates[0].M, coordinates[0].N, coordinates[1].M, coordinates[1].N, coordinates[2].M, coordinates[2].N));
                    break;
                }
                else { Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() found CellState.{0} at ({1},{2})", Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N].ToString(), PossibleRows[i, 0].M, PossibleRows[i, 0].N)); }
            }
             
            */



            /* OLD VERSION 1, that went over each cell and its neighours, over-complicated for what is actually needed, left in for show or as an example if you want to work with a larger field like in 'connect four'

            // Cell coordinates to start analysing, one of which will always be present in any row of three
            // Currently set to the cells in the top row and left column
            CellCoor[] startCells = new CellCoor[]
            {
                new CellCoor(0, 0),
                new CellCoor(0, 1),
                new CellCoor(0, 2),
                new CellCoor(1, 0),
                new CellCoor(2, 0)
            };

            // Starting from each start location, analyse the neighbouring cells to see if any contain a matching symbol
            foreach (CellCoor startCell in startCells)
            {
                // List that will contain all neigbouring cells whose symbol matches the symbol of startCell, excluding the Empty symbol
                List<CellCoor> firstHits = new List<CellCoor>();

                // Get all the neighbouring cells and check in the symbol matches that of startCell, excluding the Empty symbol
                foreach (CellCoor firstCell in GetNeighbouringCells(startCell))
                {
                    if (Gameboard[startCell.M, startCell.N] != CellState.Empty && Gameboard[startCell.M, startCell.N] == Gameboard[firstCell.M, firstCell.N])
                    {
                        firstHits.Add(firstCell);
                        Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() found a match at ({0}, {1}) and ({2}, {3})", startCell.M, startCell.N, firstHits.Last().M, firstHits.Last().N));
                    }
                }

                foreach (CellCoor firstCell in firstHits)
                {
                    foreach(CellCoor secondCell in GetNeighbouringCells(firstCell))
                    {
                        // If the symbols match, check if the cells are all in a line
                        if (Gameboard[firstCell.M, firstCell.N] == Gameboard[secondCell.M, secondCell.N])
                        {
                            if (((startCell.M + firstCell.M)*2 == secondCell.M) && ((startCell.N + firstCell.N + 1)*2 == secondCell.N))
                            {

                            }
                        }
                    }
                }
            }
            */

            return coordinates;
        }


        /// <summary>
        /// DEPRICATED. Returns a List with the coordinates of all valid neighbouring cells. (it was only used by old version 1 of AnalyseGameboard())
        /// </summary>
        /// <param name="cellCoor">The coordinates of the Cell from which the neighbours are determined.</param>
        /// <returns>A list containing the coordinates of all neigbouring cells.</returns>
        private List<CellCoor> GetNeighbouringCells(CellCoor cellCoor)
        {
            // Variable to return
            List<CellCoor> neighbours = new List<CellCoor>();
            
            // Apply the shift value stored in NeighbourCoordinates to the given cellCoor (with adjustment for GameboardwBuffer's size), which gives the coordinates of a current neighbour.
            // Add the coordinates of all neighbours where the equivalent value in GameboardwBuffer is equal to one (zero means out-of-bounds) to the List neighbours
            foreach(Vector2 neighbour in NeighbourCoordinates)
            {
                if (GameboardwBuffer[cellCoor.M + 1 + (int)neighbour.X, cellCoor.N + 1 + (int)neighbour.Y] == 1)
                {
                    neighbours.Add(new CellCoor(cellCoor.M + (int)neighbour.X, cellCoor.N + (int)neighbour.Y));
                    // Debug.WriteLine(string.Format("TicTacToe: GetNeigbouringCells({0}, {1}) found a neighbour at ({2}, {3})", cellCoor.M, cellCoor.N, neighbours.Last().M, neighbours.Last().N));
                }
            }

            return neighbours;
        }




        // Show where a row of three has been detected, by setting the opacity of a line-image on the equivalent line to 1
        private void ShowRowOfThree(CellCoor cell1, CellCoor cell2, CellCoor cell3)
        {
            // Show the Line where a row of three was detected
            // Multiple argument switch, needs C# version 8.0+
            switch (cell1.M, cell1.N, cell3.M, cell3.N)
            {
                // Horizontal
                case (0, 0, 0, 2): { GameboardLine00To02.Opacity = 1; break; }
                case (1, 0, 1, 2): { GameboardLine10To12.Opacity = 1; break; }
                case (2, 0, 2, 2): { GameboardLine20To22.Opacity = 1; break; }
                // Vertical
                case (0, 0, 2, 0): { GameboardLine00To20.Opacity = 1; break; }
                case (0, 1, 2, 1): { GameboardLine01To21.Opacity = 1; break; }
                case (0, 2, 2, 2): { GameboardLine02To22.Opacity = 1; break; }
                // Diagonal
                case (0, 0, 2, 2): { GameboardLine00To22.Opacity = 1; break; }
                case (0, 2, 2, 0): { GameboardLine02To20.Opacity = 1; break; }
            }
        }




        // Test method, a collection of testing functionality triggered by a button click
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            // Iterates through all possible symbols at gameboard field 0,1
            /*
            int x = 0;
            int y = 1;

            if ( Gameboard[x, y] < (CellState)2 ) { Gameboard[x, y]++; }
            else { Gameboard[x, y] = CellState.Empty; }

            string imagePath = string.Empty;
            if (Gameboard[x, y] == CellState.Empty) { imagePath = string.Empty; }
            else if (Gameboard[x, y] == CellState.X) { imagePath = SymbolX; }
            else if (Gameboard[x, y] == CellState.O) { imagePath = SymbolY; }
            else { Debug.WriteLine(string.Format("TicTacToe: failed to determine the correct symbol to use from CellState")); }

            GameboardImage10.Source = new BitmapImage(new Uri("ms-appx:///" + imagePath));
            */


            // Test UpdateGameBoardCell by calling it with every possible argument combination, from left to right and up to down
            // (Could have used non-local elements so as to not have to go through the entire gameboard each time, but this is only for testing so I went with local/self-contained as it leaves no traces elsewhere)
            /*
            bool stopLoop = false;
            for (int m = 0; m < Gameboard.GetLength(1); m++)
            {
                for (int n = 0; n < Gameboard.GetLength(0); n++)
                {
                    int m1 = m;
                    int n1 = n + 1;

                    if (n == 2)
                    {
                        n1 = 0;
                        if (m == 2) { m1 = 0; }
                        else { m1 = m + 1; }
                    }

                    if ((Gameboard[m, n] > Gameboard[m1, n1]) || (Gameboard[m, n] == CellState.Empty && Gameboard[m1, n1] == CellState.O))
                    {
                        if (Gameboard[m1, n1] == CellState.O) { UpdateGameboardCell(m1, n1, CellState.Empty); stopLoop = true; break; }
                        else
                        {
                            CellState nextState;
                            if (Gameboard[m1, n1] == CellState.Empty) { nextState = CellState.X; }
                            else if (Gameboard[m1, n1] == CellState.X) { nextState = CellState.O; }
                            else { nextState = CellState.Empty; }
                            UpdateGameboardCell(m1, n1, nextState);
                            stopLoop = true;
                            break;
                        }
                    }
                    else if (m == 2 && n == 2) 
                    {
                        CellState nextState;
                        if (Gameboard[m1, n1] == CellState.Empty) { nextState = CellState.X; }
                        else if (Gameboard[m1, n1] == CellState.X) { nextState = CellState.O; }
                        else { nextState = CellState.Empty; }
                        UpdateGameboardCell(m1, n1, nextState);
                        stopLoop = true;
                        break;
                    }
                }
                if (stopLoop) { break; }
            }
            */


            // Analyse the current state of the Gameboard
            CellCoor[] line = AnalyseGameboard();
            if (line[2].M != 0 && line[2].N != 0)
            {
                ShowRowOfThree(line[1], line[2], line[3]);
            }
        }

    }
}
