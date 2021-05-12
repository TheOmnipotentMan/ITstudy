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

// Used to create a delay
using System.Threading.Tasks;

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

            public bool Equals(CellCoor cell)
            {
                return (cell.M == this.M && cell.N == this.N);
            }

        }

        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "28:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "6";
        // Date when this project was finished
        string ProjectDateFinished = "06/05/21";

        // The types of behaviour that the computer can use when playing TicTacToe
        enum AIBehaviour { CellValue, Random, None };
        public List<string> AIBehaviourList;
        AIBehaviour CurrentAIBehaviour;

        // Players
        enum Players { Player, Computer };
        public List<string> PlayersList;
        Players CurrentPlayersTurn;

        // Player Symbols
        CellState PlayerSymbol;
        CellState ComputerSymbol;

        // Representation of the Gameboard and its current cell states
        enum CellState { Empty, X, O };
        CellState[,] Gameboard = new CellState[3, 3];

        // Copy of gameboard with added bufferzone all around (not used atm)
        int[,] GameboardwBuffer = new int[5, 5];

        // Array of shifts in coordinates for all neighbours, clockwards starting at 3 (not used atm)
        Vector2[] NeighbourCoordinates;

        // Cell coordinates to analyse, all possible locations for a row of three.
        CellCoor[,] PossibleRows = new CellCoor[8, 3];

        // Path to TicTacToe symbol image files
        string SymbolX;
        string SymbolY;

        // Random number generator, used for random AIBehaviour
        Random Rand = new Random();



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
            foreach(string behaviourName in Enum.GetNames(typeof(AIBehaviour)))
            {
                AIBehaviourList.Add(behaviourName);
            }

            // Copy the enum of Players to a List so it can be used by UI element(s)
            PlayersList = new List<string>();
            foreach(string playerName in Enum.GetNames(typeof(Players)))
            {
                PlayersList.Add(playerName);
            }

            // Set the screen representation of the starting symbol for the player (using this value, or one of the next two) in any meaningfull way results in crashes as the Objects have not been completed/finished, its item-list does not yet exist for instance even though we're setting an index value here)
            SelectPlayerSymbolComboBox.SelectedIndex = 0;
            // Set the screen representation of the starting AI behaviour
            AIBehaviourComboBox.SelectedIndex = 0;
            // Set the screen representation of the starting player
            StartingPlayerComboBox.SelectedIndex = 0;

            // Set the file-path for the TicTacToe symbol image files
            string symbolXPath = "Assets\\TicTacToe\\TicTacToeSymbolX.png";
            string symbolYPath = "Assets\\TicTacToe\\TicTacToeSymbolO.png";
            if (File.Exists(symbolXPath)) { SymbolX = symbolXPath; }
            else { Debug.WriteLine(string.Format("TicTacToe: Could not find image file for symbol X at {0}", symbolXPath)); }
            if (File.Exists(symbolYPath)) { SymbolY = symbolYPath; }
            else { Debug.WriteLine(string.Format("TicTacToe: Could not find image file for symbol X at {0}", symbolYPath)); }

            // Create a proxy representation of the gameboard with buffer zones to make finding existing neighbours simpler (DEPRICATED)
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

            // Fill Rows will all possible locations for a row of three, each column starts with a 'fulcrum' cell, one of these will be present in any row of three.
            // These starting cells can speed up going over possible cell in gameboard, since if any of these three are empty, there can be no row of three.
            PossibleRows = new CellCoor[8, 3]
            {
                // Lines containing the top left cell (0, 0)
                { new CellCoor(0, 0), new CellCoor(0, 1), new CellCoor(0, 2) },
                { new CellCoor(0, 0), new CellCoor(1, 0), new CellCoor(2, 0) },
                { new CellCoor(0, 0), new CellCoor(1, 1), new CellCoor(2, 2) },

                // Lines containing the center cell (1, 1), excluding line (0, 0)-(1, 1)-(2, 2) already in array
                { new CellCoor(1, 1), new CellCoor(0, 1), new CellCoor(2, 1) },
                { new CellCoor(1, 1), new CellCoor(1, 0), new CellCoor(1, 2) },
                { new CellCoor(1, 1), new CellCoor(0, 2), new CellCoor(2, 0) },

                // Lines containing the bottom right cell (2, 2), excluding lines already in array
                { new CellCoor(2, 2), new CellCoor(0, 2), new CellCoor(1, 2) },
                { new CellCoor(2, 2), new CellCoor(2, 0), new CellCoor(2, 1) }
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
                    Gameboard[i, j] = CellState.Empty;
                }
            }

            // Set the Symbols on screen to Empty
            string imagePath = string.Empty;
            BitmapImage newImage = new BitmapImage(new Uri("ms-appx:///" + imagePath));
            GameboardImage00.Source = null;
            GameboardImage01.Source = null;
            GameboardImage02.Source = null;
            GameboardImage10.Source = null;
            GameboardImage11.Source = null;
            GameboardImage12.Source = null;
            GameboardImage20.Source = null;
            GameboardImage21.Source = null;
            GameboardImage22.Source = null;

            // Set the opacity of all row-of-three lines to 0 (invisible)
            GameboardLine00To02.Opacity = 0;
            GameboardLine10To12.Opacity = 0;
            GameboardLine20To22.Opacity = 0;
            GameboardLine00To20.Opacity = 0;
            GameboardLine01To21.Opacity = 0;
            GameboardLine02To22.Opacity = 0;
            GameboardLine00To22.Opacity = 0;
            GameboardLine02To20.Opacity = 0;

            // Clear the EndGame messages
            EndGameMessageTextBlock.Text = string.Empty;
            StartNewGameHint.Text = string.Empty;

            // Set the behaviour for the computer
            CurrentAIBehaviour = (AIBehaviourComboBox.SelectedIndex < 0) ? (AIBehaviour)0 : (AIBehaviour)AIBehaviourComboBox.SelectedIndex;

            // Set the player to go first
            CurrentPlayersTurn = (StartingPlayerComboBox.SelectedIndex < 0) ? (Players)0 : (Players)StartingPlayerComboBox.SelectedIndex;

            // Set the symbols for each player
            if (SelectPlayerSymbolComboBox.SelectedIndex == 0)
            {
                PlayerSymbol = CellState.X;
                ComputerSymbol = CellState.O;
            }
            else
            {
                PlayerSymbol = CellState.O;
                ComputerSymbol = CellState.X;
            }

            // If the user has selected for the computer to go first
            if (CurrentPlayersTurn == Players.Computer)
            {
                DoComputerTurn();
            }
            else
            {
                // wait on the user to make the first move; GameboardButton_Click(), event
            }

            Debug.WriteLine(string.Format("TicTacToe: Starting a new game with setting;"));
            Debug.WriteLine(string.Format("TicTacToe: PlayerSymbol = {0}, ComputerSymbol = {1}, AIBehaviour = {2}, First turn = {3}", PlayerSymbol, ComputerSymbol, CurrentAIBehaviour, CurrentPlayersTurn));
        }




        // Registers a mouse-click on a cell in gameboard
        private void GameboardButton_Click(object sender, RoutedEventArgs e)
        {
            // If the user gives input on the Gameboard while it is not its turn, do nothing
            if (CurrentPlayersTurn == Players.Computer) { return; }

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
                default: { Debug.WriteLine(string.Format("TicTacToe: GameboardButton_Click() did not recognise button {0}", button)); break; }
            }

            // Debug.WriteLine(string.Format("TicTacToe: GameboardButton_Click() registered {0}", button));

            // If AI Behaviour is set to None, allow the user to change the symbols freely
            if (CurrentAIBehaviour == AIBehaviour.None)
            {
                UpdateGameboardCell(new CellCoor(m, n), Gameboard[m, n].Next());
            }
            // Check if the selected cell is a valid choice, ie not already filled with a symbol
            else if (Gameboard[m, n] == CellState.Empty)
            {
                UpdateGameboardCell(new CellCoor(m, n), PlayerSymbol);
            }
            else
            {
                Debug.WriteLine(string.Format("TicTacToe: GameboardButton_Click() selected cell is not valid"));
                ShowInvalidCell(new CellCoor(m, n));
            }
        }




        // Start of turn for the computer and its behaviour(s), selects correct behaviour based on current settings
        private async void DoComputerTurn()
        {
            // If this method was called but it was not the computers turn, do nothing
            if (CurrentPlayersTurn == Players.Player) { Debug.WriteLine(string.Format("TicTacToe: DoComputerTurn() was called but it was not the computers turn.")); return; }

            // If the user has selected the AIBehaviour None, don't do anything but set the CurrentPlayersTurn to the user/player
            if (CurrentAIBehaviour == AIBehaviour.None) { CurrentPlayersTurn = Players.Player; return; }

            // Wait a little bit, so as to not play a move near instantaneous
            await Task.Delay(500);


            // If the current behaviour is set to Random
            if (CurrentAIBehaviour == AIBehaviour.Random)
            {
                DoComputerTurnRandom();
            }

            // If the current behaviour is set to CellValue
            if (CurrentAIBehaviour == AIBehaviour.CellValue)
            {
                DoComputerTurnCellValue();
            }

        }


        // Computer behaviour, Random
        private void DoComputerTurnRandom()
        {
            // Add all empty cells on the Gameboard to a list
            List<CellCoor> availableCells = new List<CellCoor>();
            for (int m = 0; m < Gameboard.GetLength(0); m++)
            {
                for (int n = 0; n < Gameboard.GetLength(1); n++)
                {
                    if (Gameboard[m, n] == CellState.Empty)
                    {
                        availableCells.Add(new CellCoor(m, n));
                    }
                }
            }

            // Randomly select a cell from the list, and play it
            UpdateGameboardCell(availableCells[Rand.Next(0, availableCells.Count)], ComputerSymbol);
        }


        // Computer behaviour, CellValue
        private void DoComputerTurnCellValue()
        {
            // Create an array of int with the same dimensions as Gameboard, and set each element to 0
            int[,] cellValues = new int[Gameboard.GetLength(0), Gameboard.GetLength(1)];
            for (int iCell = 0; iCell < cellValues.GetLength(0); iCell++)
            {
                for (int jCell = 0; jCell < cellValues.GetLength(1); jCell++)
                {
                    cellValues[iCell, jCell] = 0;
                }
            }

            // Value of cells if the row is completely empty of symbols
            int emptyValue = 1;
            // Value of cells if the row contains one ComputerSymbol
            int oneValue = 3;
            // Value of cells if the row contains two ComputerSymbols, is always larger than the maximum value a cell can reach with just emptyValue & oneValue, meaning this cell gets absolute priority
            int twoValue = ((emptyValue + oneValue) * PossibleRows.GetLength(0)) + 1;
            // Value of cells if the row contains two PlayerSymbols, almost the same as twoValue but the resulting cellValue should always be lower than twoValue (except when a single cell about to complete more than one row)
            int counterValue = (emptyValue + oneValue) * PossibleRows.GetLength(0);

            // Go over PossibleRows to see which rows contain only Empty cells or cells with ComputerSymbol, if so add a certain value, one of the above, to that cell in cellValues
            for (int i = 0; i < PossibleRows.GetLength(0); i++)
            {
                CellState cell0 = Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N];
                CellState cell1 = Gameboard[PossibleRows[i, 1].M, PossibleRows[i, 1].N];
                CellState cell2 = Gameboard[PossibleRows[i, 2].M, PossibleRows[i, 2].N];

                // If none of the cells contain the PlayerSymbol
                if (!cell0.Equals(PlayerSymbol) && !cell1.Equals(PlayerSymbol) && !cell2.Equals(PlayerSymbol))
                {
                    // Add all values of the CellStates together and adjust for the current ComputerSymbol (1 if X, 2 if O),
                    // Resulting in the number of times the ComputerSymbols is present in that row
                    int totalRowValue = (int)cell0 + (int)cell1 + (int)cell2;
                    totalRowValue = totalRowValue / (int)ComputerSymbol;

                    switch (totalRowValue)
                    {
                        // If the row contains no ComputerSymbol, ie it is empty, add emptyValue to all cells of that row
                        case (0):
                            {
                                cellValues[PossibleRows[i, 0].M, PossibleRows[i, 0].N] += emptyValue;
                                cellValues[PossibleRows[i, 1].M, PossibleRows[i, 1].N] += emptyValue;
                                cellValues[PossibleRows[i, 2].M, PossibleRows[i, 2].N] += emptyValue;
                                break;
                            }
                        // If the row contains one ComputerSymbol, add oneValue to all the empty cells in that row
                        case (1):
                            {
                                cellValues[PossibleRows[i, 0].M, PossibleRows[i, 0].N] += (cell0 == CellState.Empty) ? oneValue : 0;
                                cellValues[PossibleRows[i, 1].M, PossibleRows[i, 1].N] += (cell1 == CellState.Empty) ? oneValue : 0;
                                cellValues[PossibleRows[i, 2].M, PossibleRows[i, 2].N] += (cell2 == CellState.Empty) ? oneValue : 0;
                                break;
                            }
                        // If the row contains two ComputerSymbols, add twoValue to the empty cell
                        case (2):
                            {
                                cellValues[PossibleRows[i, 0].M, PossibleRows[i, 0].N] += (cell0 == CellState.Empty) ? twoValue : 0;
                                cellValues[PossibleRows[i, 1].M, PossibleRows[i, 1].N] += (cell1 == CellState.Empty) ? twoValue : 0;
                                cellValues[PossibleRows[i, 2].M, PossibleRows[i, 2].N] += (cell2 == CellState.Empty) ? twoValue : 0;
                                break;
                            }
                        // Something went wrong, log a message
                        default:
                            {
                                Debug.WriteLine(string.Format("TicTacToe: DoComputerTurn CellValue incorrectly calculates the number of ComputerSymbols in a row."));
                                break;
                            }
                    }

                }
                else
                {
                    // Check if the row contains two PlayerSymbols, meaning the player is about to win
                    if (((int)cell0 + (int)cell1 + (int)cell2) / (int)PlayerSymbol == 2)
                    {
                        // If the cell is not empty nor already equal to counterValue (meaning it has already been adjusted for the fact the player is about to win, preventing adding counterValue more then once, and thereby the possibility of the computer prioritising countering the player instead of winning itself)
                        if (cell0.Equals(CellState.Empty) && cellValues[PossibleRows[i, 0].M, PossibleRows[i, 0].N] != counterValue)
                        {
                            cellValues[PossibleRows[i, 0].M, PossibleRows[i, 0].N] += counterValue;
                        }
                        else if (cell1.Equals(CellState.Empty) && cellValues[PossibleRows[i, 1].M, PossibleRows[i, 1].N] != counterValue)
                        {
                            cellValues[PossibleRows[i, 1].M, PossibleRows[i, 1].N] += counterValue;
                        }
                        else if (cell2.Equals(CellState.Empty) && cellValues[PossibleRows[i, 1].M, PossibleRows[i, 1].N] != counterValue)
                        {
                            cellValues[PossibleRows[i, 2].M, PossibleRows[i, 2].N] += counterValue;
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("TicTacToe: CellValues detected a row with two PlayerSymbols, but could not find the empty cell"));
                        }
                    }
                }

                // Should we log the cellValue values every loop, so each time a row in PossibleRows was analysed
                bool logEveryLoop = false;
                if (logEveryLoop)
                {
                    Debug.WriteLine(string.Format("TicTacToe: CellValues loop {0}", i.ToString()));
                    Debug.WriteLine(string.Format("TicTacToe: CellValues {0}, {1}, {2}", cellValues[0, 0].ToString(), cellValues[0, 1].ToString(), cellValues[0, 2].ToString()));
                    Debug.WriteLine(string.Format("TicTacToe: CellValues {0}, {1}, {2}", cellValues[1, 0].ToString(), cellValues[1, 1].ToString(), cellValues[1, 2].ToString()));
                    Debug.WriteLine(string.Format("TicTacToe: CellValues {0}, {1}, {2}", cellValues[2, 0].ToString(), cellValues[2, 1].ToString(), cellValues[2, 2].ToString()));
                }
            }

            // Should we log the cellValue values when all rows of PossibleRows were looked at
            bool logFinalCellValues = true;
            if (logFinalCellValues)
            {
                Debug.WriteLine(string.Format("TicTacToe: final CellValues are;"));
                Debug.WriteLine(string.Format("TicTacToe: CellValues {0}, {1}, {2}", cellValues[0, 0].ToString(), cellValues[0, 1].ToString(), cellValues[0, 2].ToString()));
                Debug.WriteLine(string.Format("TicTacToe: CellValues {0}, {1}, {2}", cellValues[1, 0].ToString(), cellValues[1, 1].ToString(), cellValues[1, 2].ToString()));
                Debug.WriteLine(string.Format("TicTacToe: CellValues {0}, {1}, {2}", cellValues[2, 0].ToString(), cellValues[2, 1].ToString(), cellValues[2, 2].ToString()));
            }

            // Find the highest value in cellValues
            int highestCellValue = 0;
            foreach (int cellValue in cellValues)
            {
                if (cellValue > highestCellValue) { highestCellValue = cellValue; }
            }
            // Go over cellValues again, adding the coordinates of every cell that matches the highestCellValue to a List
            List<CellCoor> highestCells = new List<CellCoor>();
            for (int iHighestValue = 0; iHighestValue < cellValues.GetLength(0); iHighestValue++)
            {
                for (int jHighestValue = 0; jHighestValue < cellValues.GetLength(1); jHighestValue++)
                {
                    if (cellValues[iHighestValue, jHighestValue] == highestCellValue)
                    {
                        highestCells.Add(new CellCoor(iHighestValue, jHighestValue));
                    }
                }
            }

            // The cell to play
            CellCoor cellToPlay =  new CellCoor();

            // If highestCells contains more than one element look if any of the cells would hinder the player from continuing one of its rows
            if (highestCells.Count > 1)
            {
                Debug.WriteLine("CellValue looking for counterPlayer cells");

                // New list of possible cells to play
                List<CellCoor> highestCells2 = new List<CellCoor>();

                foreach (CellCoor highCell in highestCells)
                {
                    for (int iCounterPlayer = 0; iCounterPlayer < PossibleRows.GetLength(0); iCounterPlayer++)
                    {
                        CellCoor antiCell0 = PossibleRows[iCounterPlayer, 0];
                        CellCoor antiCell1 = PossibleRows[iCounterPlayer, 1];
                        CellCoor antiCell2 = PossibleRows[iCounterPlayer, 2];

                        // If the current cell of highestCells is present in a row of PossibleRows
                        if (highCell.Equals(antiCell0) || highCell.Equals(antiCell1) || highCell.Equals(antiCell2))
                        {
                            // If the row contains a PlayerSymbol, add it to the new list
                            if (Gameboard[antiCell0.M, antiCell0.N].Equals(PlayerSymbol) || Gameboard[antiCell1.M, antiCell1.N].Equals(PlayerSymbol) || Gameboard[antiCell2.M, antiCell2.N].Equals(PlayerSymbol))
                            {
                                highestCells2.Add(highCell);
                                Debug.WriteLine(string.Format("CellValue found a counterPlayer cell at ({0}, {1})", highCell.M, highCell.N));
                            }
                        }
                    }
                }

                // If any cells were found and added to the new list, set the origional list to the new
                if (highestCells2.Count > 0)
                {
                    highestCells = new List<CellCoor>(highestCells2);
                }
            }

            // Pick a cell from highestCells, at random since all cells in the List are equally valuable
            cellToPlay = highestCells[Rand.Next(0, highestCells.Count)];
            Debug.WriteLine(string.Format("TicTacToe: DoComputerTurnCellValue() wants to play ({0}, {1}), out of {2} choices", cellToPlay.M, cellToPlay.N, highestCells.Count));

            // Play the selected cell
            UpdateGameboardCell(cellToPlay, ComputerSymbol);
            
        }





        /// <summary>
        /// Update a cell on the Gameboard to the desired CellState. Then calls AnalyseGameboard() to see if the new state has any rows of three, and if not checks if the Gameboard has any empty cells left for the game to continue.
        /// Finally if it has not yet exited, it set the next player to make a move.
        /// </summary>
        /// <param name="cell">The coordinates of the cell that should be changed.</param>
        /// <param name="cellState">The state the specified cell will be changed to.</param>
        private void UpdateGameboardCell(CellCoor cell, CellState cellState)
        {
            int m = cell.M;
            int n = cell.N;

            // Update the CellState in the Gameboard array to the new cellState
            if (cellState > (CellState)2) { cellState = CellState.Empty; }
            Gameboard[m, n] = cellState;

            // Select the correct symbol image based on cellState
            string imagePath = string.Empty;
            if (cellState == CellState.Empty) { imagePath = string.Empty; }
            else if (Gameboard[m, n] == CellState.X) { imagePath = SymbolX; }
            else if (Gameboard[m, n] == CellState.O) { imagePath = SymbolY; }
            else { Debug.WriteLine(string.Format("TicTacToe: UpdateGameboardCell failed to determine the correct symbol to use from cellState")); return; }
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

                default: { Debug.WriteLine(string.Format("TicTacToe: UpdateGameboardCell() failed to find a cell to update.")); return; }
            }

            // Analyse the state of the Gameboard
            // If any rows were detected, show them on screen, and end the current game
            List<CellCoor> rowsOfThree = new List<CellCoor>(AnalyseGameboard());
            if (rowsOfThree.Any())
            {
                for(int i = 0; i < rowsOfThree.Count; i += 3)
                {
                    ShowRowOfThree(rowsOfThree[i], rowsOfThree[i + 1]);
                }
                // End the game with the CellState of the first row of three found, ie the winning symbol and thereby winning player
                EndGame(Gameboard[rowsOfThree[0].M, rowsOfThree[0].N]);
                return;
            }
            // Else no rows were found, check if there are any Empty cells left, if not end the current game
            else
            {
                bool noEmptyCell = true;
                foreach (CellState state in Gameboard)
                {
                    if (state == CellState.Empty) { noEmptyCell = false; }
                }
                // If there are none, end the current game with no CellState (defaults to Empty, meaning a draw)
                if (noEmptyCell) 
                {
                    EndGame();
                    return;
                }
                // Else the game can continue, set the next player to make a move
                else
                {
                    CurrentPlayersTurn = CurrentPlayersTurn.Next();
                    Debug.WriteLine(string.Format("TicTacToe: Current turn is for {0}", CurrentPlayersTurn.ToString()));
                    // If the computer is next to make a move, call its method
                    if (CurrentPlayersTurn == Players.Computer)
                    {
                        DoComputerTurn();
                    }
                    else
                    {
                        // wait on user to make a move; GameboardButton_Click(), event
                    }
                }
            }
        }



        /// <summary>
        /// Analyses the current state of the Gameboard and will try to find a row of three identical symbols.
        /// If it finds any it will return the coordinates of the cells as a List, if it failed to find anything this List will be empty.
        /// </summary>
        /// <returns>A List containing the cell-coordinates of all found row of three. If no row was found the List will be empty.</returns>
        private List<CellCoor> AnalyseGameboard()
        {
            Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() is analysing."));

            // Array to return, set to default of all zero
            List<CellCoor> coordinates = new List<CellCoor>();

            // Go over the three 'fulcrum' cells of PossibleRows, of which at least one will always be part of any possible row of three (located at PossibleRows[x, n] with x = 0, 3, 6)
            for (int i = 0; i < PossibleRows.GetLength(0); i+=3)
            {
                Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() looking at PossibleRows {0}; cell({1}, {2}).", i.ToString(), PossibleRows[i, 0].M.ToString(), PossibleRows[i, 0].N.ToString()));

                CellState cell0 = Gameboard[PossibleRows[i, 0].M, PossibleRows[i, 0].N];

                // If the first symbol is not an Empty cell
                if (cell0 != CellState.Empty)
                {
                    // Go over the relevant rows of PossibleRows, checking if the symbols of the Gameboard-cells at the coordinates match
                    int rowsToCheck = ((i == 6) ? 2 : 3) + i;
                    for (int j = i; j < rowsToCheck; j++)
                    {
                        CellState cell1 = Gameboard[PossibleRows[j, 1].M, PossibleRows[j, 1].N];
                        CellState cell2 = Gameboard[PossibleRows[j, 2].M, PossibleRows[j, 2].N];

                        Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() looking at cell0({0}, {1}).{2} cell1({3}, {4}).{5} cell2({6}, {7}).{8}.", PossibleRows[i, 0].M, PossibleRows[i, 0].N, cell0.ToString(), PossibleRows[j, 1].M, PossibleRows[j, 1].N, cell1.ToString(), PossibleRows[j, 2].M, PossibleRows[j, 2].N, cell2.ToString()));


                        // if all the symbols match, add them to the coordinates List
                        if (Gameboard[PossibleRows[j, 0].M, PossibleRows[j, 0].N] == Gameboard[PossibleRows[j, 1].M, PossibleRows[j, 1].N] && Gameboard[PossibleRows[j, 0].M, PossibleRows[j, 0].N] == Gameboard[PossibleRows[j, 2].M, PossibleRows[j, 2].N])
                        {
                            Debug.WriteLine(string.Format("TicTacToe: AnalyseGameboard() all cells, ({0}, {1}) ({2}, {3}) ({4}, {5}), from line({6}) match, adding them to coordinates List.", PossibleRows[j, 0].M.ToString(), PossibleRows[j, 0].N.ToString(), PossibleRows[j, 1].M.ToString(), PossibleRows[j, 1].N.ToString(), PossibleRows[j, 2].M.ToString(), PossibleRows[j, 2].N.ToString(), j.ToString()));
                            coordinates.Add(new CellCoor(PossibleRows[j, 0].M, PossibleRows[j, 0].N));
                            coordinates.Add(new CellCoor(PossibleRows[j, 1].M, PossibleRows[j, 1].N));
                            coordinates.Add(new CellCoor(PossibleRows[j, 2].M, PossibleRows[j, 2].N));
                        }

                    }

                }

            }




            /* OLD VERSION 2, look at each possible row of three and see if the symbols match, inefficient since it went over every possible line although some might be excluded by previous passes, also doesn't work in current state
            
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
        /// DEPRICATED. Returns a List with the coordinates of all valid neighbouring cells. (was used only by old version 1 of AnalyseGameboard())
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


        // Give visual feedback that the cell the player selected was invalid
        private async void ShowInvalidCell(CellCoor cell)
        {
            double opacity = 0.8;
            int duration = 1000;
            switch(cell.M, cell.N)
            {
                case (0, 0): { InvalidCell00Border.Opacity = opacity; await Task.Delay(duration); InvalidCell00Border.Opacity = 0; break; }
                case (0, 1): { InvalidCell01Border.Opacity = opacity; await Task.Delay(duration); InvalidCell01Border.Opacity = 0; break; }
                case (0, 2): { InvalidCell02Border.Opacity = opacity; await Task.Delay(duration); InvalidCell02Border.Opacity = 0; break; }
                case (1, 0): { InvalidCell10Border.Opacity = opacity; await Task.Delay(duration); InvalidCell10Border.Opacity = 0; break; }
                case (1, 1): { InvalidCell11Border.Opacity = opacity; await Task.Delay(duration); InvalidCell11Border.Opacity = 0; break; }
                case (1, 2): { InvalidCell12Border.Opacity = opacity; await Task.Delay(duration); InvalidCell12Border.Opacity = 0; break; }
                case (2, 0): { InvalidCell20Border.Opacity = opacity; await Task.Delay(duration); InvalidCell20Border.Opacity = 0; break; }
                case (2, 1): { InvalidCell21Border.Opacity = opacity; await Task.Delay(duration); InvalidCell21Border.Opacity = 0; break; }
                case (2, 2): { InvalidCell22Border.Opacity = opacity; await Task.Delay(duration); InvalidCell22Border.Opacity = 0; break; }
            }
        }


        // Show where a row of three has been detected, by setting the opacity of a line-image on the equivalent line to 1
        // Takes in the first two cell-coordinates of a row, based on values in the rows of PossibleRows
        private void ShowRowOfThree(CellCoor cell0, CellCoor cell1)
        {
            bool isRowFound = true;

            // Show the Line where a row of three was detected (layout is matched to PossibleRows)
            if (cell0.Equals(PossibleRows[0, 0]))
            {
                if (cell1.Equals(PossibleRows[0, 1])) { GameboardLine00To02.Opacity = 1; }
                else if (cell1.Equals(PossibleRows[1, 1])) { GameboardLine00To20.Opacity = 1; }
                else if (cell1.Equals(PossibleRows[2, 1])) { GameboardLine00To22.Opacity = 1; }
                else { isRowFound = false; }
            }
            else if (cell0.Equals(PossibleRows[3, 0]))
            {
                if (cell1.Equals(PossibleRows[3, 1])) { GameboardLine01To21.Opacity = 1; }
                else if (cell1.Equals(PossibleRows[4, 1])) { GameboardLine10To12.Opacity = 1; }
                else if (cell1.Equals(PossibleRows[5, 1])) { GameboardLine02To20.Opacity = 1; }
                else { isRowFound = false; }
            }
            else if (cell0.Equals(PossibleRows[6, 0]))
            {
                if (cell1.Equals(PossibleRows[6, 1])) { GameboardLine02To22.Opacity = 1; }
                else if(cell1.Equals(PossibleRows[7, 1])) { GameboardLine20To22.Opacity = 1; }
                else { isRowFound = false; }
            }
            else
            {
                isRowFound = false;
            }

            if (!isRowFound)
            {
                Debug.WriteLine(string.Format("TicTacToe: ShowRowOfThree did not find a row with cell({0}, {1}) & cell({2}, {3})", cell0.M, cell0.N, cell1.M, cell1.N));
            }

        }



        // Determine and set the win state of the game, takes in a CellState that represents the winner of the current game, Empty if draw
        private void EndGame(CellState cellState = CellState.Empty)
        {
            // Display a message based on who won
            if (cellState == CellState.Empty)
            {
                EndGameMessageTextBlock.Text = "It's a draw";
                EndGameMessageTextBlock.Foreground = (SolidColorBrush)Resources["Draw"];
            }
            else
            {
                if (cellState == PlayerSymbol)
                {
                    EndGameMessageTextBlock.Text = "You win!";
                    EndGameMessageTextBlock.Foreground = (SolidColorBrush)Resources["PlayerWon"];
                }
                else if (cellState == ComputerSymbol)
                {
                    EndGameMessageTextBlock.Text  = "You lose.";
                    EndGameMessageTextBlock.Foreground = (SolidColorBrush)Resources["PlayerLost"];
                }
            }
            // Suggest to the player to start a new game
            StartNewGameHint.Text = "Click 'New Game' to try again";
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


            // Analyse the current state of the Gameboard, and mark any detected rows of three
            /*
            List<CellCoor> line = AnalyseGameboard();
            for (int i = 0; i < line.Count; i += 3)
            {
                ShowRowOfThree(line[i], line[i + 1]);
            }
            */


            // Cell DoCompterTurnCellValue() to test functionality, not expecting a turn to be played, only to log debug messages of its analysis of the Gameboard cells
            DoComputerTurnCellValue();
        }

    }
}
