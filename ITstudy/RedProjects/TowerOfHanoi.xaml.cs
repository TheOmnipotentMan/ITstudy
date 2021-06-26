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

using Windows.UI.Xaml.Shapes;
using System.Collections.ObjectModel;


// TODO remove if the app is ever finished
using System.Diagnostics;


namespace ITstudy.RedProjects
{
    /// <summary>
    /// Tower of Hanoi game.
    /// </summary>
    public sealed partial class TowerOfHanoi : Page
    {
        private struct TowerBlock
        {
            /// <summary>
            /// The width of this block as a whole number, ie 1-based index
            /// </summary>
            public int Width;
            /// <summary>
            /// The state of this block, eg Empty, Block, etc
            /// </summary>
            public BlockState State;

            public TowerBlock(int width, BlockState state = BlockState.Empty)
            {
                Width = width;
                State = state;
            }
        }


        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "48:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "x";
        // Date when this project was finished
        string ProjectDateFinished = "00/00/21";


        // Tower height values (max ui-elements is 10)
        int[] TowerHeights = new int[] { 4, 5, 6, 7, 8, 9, 10 };
        int TowerHeight;
        public int TowerHeightDefault;
        public int TowerHeightMinimum;
        public int TowerHeightMaximum;

        // Tower block base values
        double TowerBlockHeight = 40;
        double TowerBlockWidthMinimum = 80;
        double TowerBlockWidthStep;


        // Gameboard block positions
        enum BlockState { Empty, Available, Block, Selected };
        TowerBlock[,] Gameboard;
        int[] SelectedBlock = new int[2];
        double[] BlockWidths;

        // int[,] Gameboard2;

        // Tower blocks, UI-elements (have an inverse index relative to Gameboard)
        public ObservableCollection<TowerOfHanoiBlock> Tower0;
        public ObservableCollection<TowerOfHanoiBlock> Tower1;
        public ObservableCollection<TowerOfHanoiBlock> Tower2;


        // Tower block colours, create a number of block-colours, used to better distinguish between blocks with adjacent/similar sizes
        Windows.UI.Color[] BlockColours = new Windows.UI.Color[]
        {
            // Chocolate, #FFD2691E
            Windows.UI.Colors.Chocolate,
            // CadetBlue, #FF5F9EA0
            Windows.UI.Colors.CadetBlue,
            // OliveDrab, #FF6B8E23
            Windows.UI.Colors.OliveDrab,
            // DarkKhaki, #FFBDB76B
            Windows.UI.Colors.DarkKhaki
        };






        public TowerOfHanoi()
        {
            this.InitializeComponent();
            FinishSetup();
        }




        private void FinishSetup()
        {
            // Set the TowerHeight default, min and max 
            TowerHeightDefault = TowerHeights.ElementAt(TowerHeights.Length - 3);
            TowerHeightMinimum = TowerHeights.First();
            TowerHeightMaximum = TowerHeights.Last();
        }


        // Start a new game
        private void NewGame(int towerHeight = -1)
        {
            // Hide the win message
            WinTextBlock.Opacity = 0;

            // Ensure a reasonable tower height
            if (towerHeight < TowerHeightMinimum || towerHeight > TowerHeightMaximum)
            {
                towerHeight = TowerHeightDefault;
            }

            // Set TowerHeight a class-scope variable, to be available to other methods
            TowerHeight = towerHeight;

            // Set the starting state of Gameboard
            Gameboard = new TowerBlock[3, towerHeight];
            for (int level = 0; level < Gameboard.GetLength(1); level++)
            {
                // Set the tower on the first column
                Gameboard[0, level] = new TowerBlock(level + 1, BlockState.Block);

                // Set all other elements to 0
                Gameboard[1, level] = new TowerBlock(0, BlockState.Empty);
                Gameboard[2, level] = new TowerBlock(0, BlockState.Empty);

                Debug.WriteLine(string.Format("TowerOfHanoi: {0} {1} {2}", Gameboard[0, level].Width, Gameboard[1, level].Width, Gameboard[2, level].Width));
            }

            // Build the on-screen towers
            BuildTowers(Gameboard);

        }


        /// <summary>
        /// Build towers based on the given (Game)board, TowerBlock-array
        /// </summary>
        /// <param name="board">The board to build</param>
        private void BuildTowers(TowerBlock[,] board)
        {
            if (board.GetLength(0) != 3 || board.GetLength(1) > TowerHeightMaximum)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: BuildTowers() recieved array with invalid dimensions, {0}x{1}", board.GetLength(0), board.GetLength(1)));
                return;
            }

            // Generate new content the on-screen towers and their blocks
            double widthMax = GameboardGrid.ActualWidth / 3 - 20;
            TowerBlockWidthStep = (widthMax - TowerBlockWidthMinimum) / board.GetLength(1);

            BlockWidths = new double[board.GetLength(1)];

            Tower0 = new ObservableCollection<TowerOfHanoiBlock>();
            Tower1 = new ObservableCollection<TowerOfHanoiBlock>();
            Tower2 = new ObservableCollection<TowerOfHanoiBlock>();

            // Populate the Tower collections, done in reverse because the UI elements that use them are themselves rotated 180
            for (int iBlock = board.GetLength(1) - 1; iBlock > -1; iBlock--)
            {
                // Create the base elements
                Tower0.Add(new TowerOfHanoiBlock("0" + iBlock.ToString(), TowerBlockHeight, widthMax, board[0, iBlock].Width));
                Tower1.Add(new TowerOfHanoiBlock("1" + iBlock.ToString(), TowerBlockHeight, widthMax, board[1, iBlock].Width));
                Tower2.Add(new TowerOfHanoiBlock("2" + iBlock.ToString(), TowerBlockHeight, widthMax, board[2, iBlock].Width));

                // Store the 'base' block widths
                BlockWidths[iBlock] = TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep;

                // Fill the towers based on the info in board
                // Tower 0
                switch (board[0, iBlock].State)
                {
                    case BlockState.Empty:
                        {
                            // Default state upon construction
                            break;
                        }
                    case BlockState.Available:
                        {
                            Tower0.Last().SetStateAvailable(BlockWidths[iBlock]);
                            break;
                        }
                    case BlockState.Block:
                        {
                            Tower0.Last().SetStateBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            break;
                        }
                    case BlockState.Selected:
                        {
                            Tower0.Last().SetStateBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            Tower0.Last().SetStateSelected();
                            break;
                        }
                }
                // Tower 1
                switch (board[1, iBlock].State)
                {
                    case BlockState.Empty:
                        {
                            // Default state upon construction
                            break;
                        }
                    case BlockState.Available:
                        {
                            Tower1.Last().SetStateAvailable(BlockWidths[iBlock]);
                            break;
                        }
                    case BlockState.Block:
                        {
                            Tower1.Last().SetStateBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            break;
                        }
                    case BlockState.Selected:
                        {
                            Tower1.Last().SetStateBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            Tower1.Last().SetStateSelected();
                            break;
                        }
                }
                // Tower 2
                switch (board[2, iBlock].State)
                {
                    case BlockState.Empty:
                        {
                            // Default state upon construction
                            break;
                        }
                    case BlockState.Available:
                        {
                            Tower2.Last().SetStateAvailable(BlockWidths[iBlock]);
                            break;
                        }
                    case BlockState.Block:
                        {
                            Tower2.Last().SetStateBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            break;
                        }
                    case BlockState.Selected:
                        {
                            Tower2.Last().SetStateBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            Tower2.Last().SetStateSelected();
                            break;
                        }
                }

            }

            // Set the top block of the first tower as Clickable
            Tower0.Last().IsClickable = true;

            // Set the on-screen towers to the generated collections
            ReloadTower();

        }



        private void AnalyseInput(int tower, int level)
        {
            // Make sure the input is reasonable, not out-of-bounds
            if (tower < 0 || tower > 2 || level < 0 || level > TowerHeight)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() recieved an out-of-bounds value. tower = {0}, level = {1}", tower, level));
                return;
            }

            // Act based on the state of the selected block
            switch (Gameboard[tower, level].State)
            {
                // If the block was empty, ignore
                case BlockState.Empty:
                    {
                        break;
                    }
                // If the block state is Available, it was flagged as a valid move for the previously selected block, so move the block to the new position
                case BlockState.Available:
                    {
                        if (Gameboard[SelectedBlock[0], SelectedBlock[1]].State == BlockState.Selected)
                        {
                            MoveBlock(SelectedBlock[0], SelectedBlock[1], tower, level);
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() wants to call MoveBlock() but no SelectedBlock was found"));
                            return;
                        }
                        break;
                    }
                // If the block state is Block, it is a newly selected block
                case BlockState.Block:
                    {
                        NewBlockSelected(tower, level);
                        break;
                    }
                // If the block was already selected, unselect the block
                case BlockState.Selected:
                    {
                        NewBlockSelected(-(tower + 1), level);
                        break;
                    }
            }

        }


        // Mark the newly selected block on screen, and un-mark any blocks previously selected, and find the positions of all possible moves
        // Can also clear a selected block by giving a negative, 1-based, value for tower
        private void NewBlockSelected(int tower, int level)
        {
            // Make sure level in within bounds
            if (tower < -3 || tower > 2 || level < 0 || level > (TowerHeight - 1))
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: NewBlockSelected() recieved invalid value. tower = {0}, level = {1}", tower, level));
                return;
            }

            // If tower was negative, unselect the block and return
            if (tower < 0)
            {
                // Unselect the block
                Gameboard[(tower + 1) * -1, level].State = BlockState.Block;
                UpdateBlock((tower + 1) * -1, level);
                // Hide any borders from previous possible moves
                if (tower != -1)
                {
                    HideBorder(0, GetLowestEmptyBlock(0));
                }
                if (tower != -2)
                {
                    HideBorder(1, GetLowestEmptyBlock(1));
                }
                if (tower != -3)
                {
                    HideBorder(2, GetLowestEmptyBlock(2));
                }
                return;
            }
            // Else set SelectedBlock to the currently selected block and continue
            else
            {
                SelectedBlock[0] = tower; SelectedBlock[1] = level;
                Gameboard[tower, level].State = BlockState.Selected;
                UpdateBlock(tower, level);
            }

            int x = 0;

            // Find and show all possible moves
            if (tower != 0)
            {
                x = GetLowestEmptyBlock(0);
                if (x > -1 && IsMoveAllowed(Gameboard[tower, level].Width, 0, x))
                {
                    Gameboard[0, x].State = BlockState.Available;
                    UpdateBlock(0, x);
                }
            }
            if (tower != 1)
            {
                x = GetLowestEmptyBlock(1);
                if (x > -1 && IsMoveAllowed(Gameboard[tower, level].Width, 1, x))
                {
                    Gameboard[1, x].State = BlockState.Available;
                    UpdateBlock(1, x);
                }
            }
            if (tower != 2)
            {
                x = GetLowestEmptyBlock(2);
                if (x > -1 && IsMoveAllowed(Gameboard[tower, level].Width, 2, x))
                {
                    Gameboard[2, x].State = BlockState.Available;
                    UpdateBlock(2, x);
                }
            }
                        
        }



        /// <summary>
        /// Move a block from one position to another
        /// </summary>
        /// <param name="startTower">The original tower</param>
        /// <param name="startLevel">The original level</param>
        /// <param name="endTower">The destination tower</param>
        /// <param name="endLevel">The destination level</param>
        private void MoveBlock(int startTower, int startLevel, int endTower, int endLevel)
        {
            if (startTower < 0 || startTower > 2)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() recieved invalid input for startTower; {0}", startTower));
                return;
            }
            else if (startLevel < 0 || startLevel >= TowerHeight)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() recieved invalid input for startLevel; {0}", startLevel));
                return;
            }
            else if (endTower < 0 || endTower > 2)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() recieved invalid input for endTower; {0}", endTower));
                return;
            }
            else if (endLevel < 0 || endLevel >= TowerHeight)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() recieved invalid input for endLevel; {0}", endLevel));
                return;
            }

            // Change the cells in Gameboard
            Gameboard[endTower, endLevel].Width = Gameboard[startTower, startLevel].Width;
            Gameboard[endTower, endLevel].State = BlockState.Block;
            Gameboard[startTower, startLevel].Width = 0;
            Gameboard[startTower, startLevel].State = BlockState.Empty;

            // Change the on-screen representation of the blocks
            UpdateBlock(startTower, startLevel);
            UpdateBlock(endTower, endLevel);

            // Unset the other border if present
            int x = -1;
            int y = -1;
            if (startTower == 0) { x = (endTower == 1) ? 2 : 1; }
            else if (startTower == 1) { x = (endTower == 2) ? 0 : 2; }
            else { x = (endTower == 0) ? 1 : 0; }
            y = GetLowestEmptyBlock(x);
            if (y > -1)
            {
                Gameboard[x, y].State = BlockState.Empty;
                UpdateBlock(x, y);
            }

            // Set the new top block of startTower to clickable
            SetBlockIsClickable(startTower, startLevel + 1, true);

            // Set the block beneath the newly placed block to not clickable
            SetBlockIsClickable(endTower, endLevel + 1, false);

            // Check if the game has been won
            CheckForWinState();
        }


        private void CheckForWinState()
        {
            // If the highest possible position in the tower on the right contains a block, the game is won
            if (Gameboard[2, 0].State == BlockState.Block)
            {
                // Make this block not-clickable
                Tower2.Last().IsClickable = false;
                ReloadTower(2);

                // Display the win message
                WinTextBlock.Opacity = 1;
            }
        }


        /// <summary>
        /// Get the level of highest block of a speficied tower
        /// Can be out of range! Happens when no block was found
        /// </summary>
        /// <param name="tower">The tower to look at</param>
        /// <returns>The level of the highest block in this tower</returns>
        private int GetHighestBlock(int tower)
        {
            int highestLevel = -1;

            for (int level = (Gameboard.GetLength(1) - 1); level > -1; level--)
            {
                // Find the first block with state Empty
                if (Gameboard[tower, level].State == BlockState.Empty)
                {
                    // Make sure we did not hit on the first block, which means there are blocks in the tower
                    if (level != (Gameboard.GetLength(1) - 1))
                    {
                        highestLevel = level + 1;
                    }
                    break;
                }
            }

            return highestLevel;
        }


        /// <summary>
        /// Get the level of the lowest empty, or available, block of a specified tower
        /// Can be out of range! Happens when no empty block was found
        /// </summary>
        /// <param name="tower">The tower to look at</param>
        /// <returns>The level of the lowest empty block in the tower</returns>
        private int GetLowestEmptyBlock(int tower)
        {
            int lowestEmpty = -1;

            for (int level = (Gameboard.GetLength(1) - 1); level > -1; level--)
            {
                if (Gameboard[tower, level].State == BlockState.Empty || Gameboard[tower, level].State == BlockState.Available)
                {
                    lowestEmpty = level;
                    break;
                }
            }

            return lowestEmpty;
        }


        /// <summary>
        /// Get whether or not a move is allowed. Checks the position below the desired one to see if there is a block there, and if it is larger than the one we want to place on it.
        /// </summary>
        /// <param name="blockWidth">The width of the block we want to move</param>
        /// <param name="tower">The tower we want to move to</param>
        /// <param name="level">The level we want to move to</param>
        /// <returns>True or false; is the desired move is allowed or not.</returns>
        private bool IsMoveAllowed(int blockWidth, int tower, int level)
        {
            // If the desired position is at the bottom, always allowed
            if (level == Gameboard.GetLength(1) - 1) { return true; }
            // Else find out if the block underneath is larger
            else
            {
                return (blockWidth < Gameboard[tower, level + 1].Width);
            }
        }


        /// <summary>
        /// Set whether a specific block is clickable or not
        /// </summary>
        /// <param name="tower">The tower of the block</param>
        /// <param name="level">THe level of the block</param>
        /// <param name="isClickable">What to set Clickable to</param>
        private void SetBlockIsClickable(int tower, int level, bool isClickable)
        {
            if (level > -1 && level < TowerHeight)
            {
                switch (tower)
                {
                    case 0:
                        {
                            Tower0.ElementAt((TowerHeight - 1) - level).IsClickable = isClickable;
                            break;
                        }
                    case 1:
                        {
                            Tower1.ElementAt((TowerHeight - 1) - level).IsClickable = isClickable;
                            break;
                        }
                    case 2:
                        {
                            Tower2.ElementAt((TowerHeight - 1) - level).IsClickable = isClickable;
                            break;
                        }
                }
                ReloadTower(tower);
            }
            
        }


        /// <summary>
        /// Hide any border at the given position
        /// </summary>
        /// <param name="tower"></param>
        /// <param name="level"></param>
        private void HideBorder(int tower, int level)
        {
            if (level > -1 && level < TowerHeight)
            {
                switch (tower)
                {
                    case 0:
                        {
                            Tower0.ElementAt((TowerHeight - 1) - level).HideBorder();
                            break;
                        }
                    case 1:
                        {
                            Tower1.ElementAt((TowerHeight - 1) - level).HideBorder();
                            break;
                        }
                    case 2:
                        {
                            Tower2.ElementAt((TowerHeight - 1) - level).HideBorder();
                            break;
                        }
                }
                ReloadTower(tower);
            }
        }


        /// <summary>
        /// Update a single block in the tower collections on-screen, to its state in Gameboard
        /// </summary>
        /// <param name="tower">The tower where the block is located at</param>
        /// <param name="level">The level where the block is located at</param>
        private void UpdateBlock(int tower, int level)
        {
            if (tower < 0 || tower > 2 || level < 0 || level > TowerHeight - 1)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: UpdateBlock() revieced invalid input. tower={0}, level={1}", tower, level));
                return;
            }

            TowerBlock newBlock = Gameboard[tower, level];

            switch (newBlock.State)
            {
                case BlockState.Empty:
                    {
                        switch (tower)
                        {
                            case 0:
                                {
                                    Tower0.ElementAt((TowerHeight - 1) - level).SetStateEmpty();
                                    break;
                                }
                            case 1:
                                {
                                    Tower1.ElementAt((TowerHeight - 1) - level).SetStateEmpty();
                                    break;
                                }
                            case 2:
                                {
                                    Tower2.ElementAt((TowerHeight - 1) - level).SetStateEmpty();
                                    break;
                                }
                        }
                        break;
                    }

                case BlockState.Available:
                    {
                        switch (tower)
                        {
                            case 0:
                                {
                                    Tower0.ElementAt((TowerHeight - 1) - level).SetStateAvailable(BlockWidths[Gameboard[SelectedBlock[0], SelectedBlock[1]].Width - 1]);
                                    break;
                                }
                            case 1:
                                {
                                    Tower1.ElementAt((TowerHeight - 1) - level).SetStateAvailable(BlockWidths[Gameboard[SelectedBlock[0], SelectedBlock[1]].Width - 1]);
                                    break;
                                }
                            case 2:
                                {
                                    Tower2.ElementAt((TowerHeight - 1) - level).SetStateAvailable(BlockWidths[Gameboard[SelectedBlock[0], SelectedBlock[1]].Width - 1]);
                                    break;
                                }
                        }
                        break;
                    }

                case BlockState.Block:
                    {
                        switch (tower)
                        {
                            case 0:
                                {
                                    Tower0.ElementAt((TowerHeight - 1) - level).SetStateBlock(BlockWidths[newBlock.Width - 1], BlockColours.ElementAt((newBlock.Width - 1 + BlockColours.Length) % BlockColours.Length));
                                    break;
                                }
                            case 1:
                                {
                                    Tower1.ElementAt((TowerHeight - 1) - level).SetStateBlock(BlockWidths[newBlock.Width - 1], BlockColours.ElementAt((newBlock.Width - 1 + BlockColours.Length) % BlockColours.Length));
                                    break;
                                }
                            case 2:
                                {
                                    Tower2.ElementAt((TowerHeight - 1) - level).SetStateBlock(BlockWidths[newBlock.Width - 1], BlockColours.ElementAt((newBlock.Width - 1 + BlockColours.Length) % BlockColours.Length));
                                    break;
                                }
                        }
                        break;
                    }

                case BlockState.Selected:
                    {
                        // unset any previously possible moves
                        int x = GetLowestEmptyBlock(0);
                        if (x > -1) { Tower0.ElementAt((TowerHeight - 1) - x).SetStateEmpty(); }
                        x = GetLowestEmptyBlock(1);
                        if (x > -1) { Tower1.ElementAt((TowerHeight - 1) - x).SetStateEmpty(); }
                        x = GetLowestEmptyBlock(2);
                        if (x > -1) { Tower2.ElementAt((TowerHeight - 1) - x).SetStateEmpty(); }

                        switch (tower)
                        {
                            case 0:
                                {
                                    Tower0.ElementAt((TowerHeight - 1) - level).SetStateSelected();
                                    break;
                                }
                            case 1:
                                {
                                    Tower1.ElementAt((TowerHeight - 1) - level).SetStateSelected();
                                    break;
                                }
                            case 2:
                                {
                                    Tower2.ElementAt((TowerHeight - 1) - level).SetStateSelected();
                                    break;
                                }
                        }
                        break;
                    }

                default:
                    {
                        Debug.WriteLine(string.Format("TowerOfHanoi: ChangeBlock() did not recognise the given state, {0}", newBlock.State));
                        break;
                    }

            }

            ReloadTower(tower);

        }



        // Reload the any or all of the on-screen towers
        private void ReloadTower(int tower = -1)
        {
            switch (tower)
            {
                case 0:
                    {
                        GameboardTower0GridView.ItemsSource = null;
                        GameboardTower0GridView.ItemsSource = Tower0;
                        break;
                    }
                case 1:
                    {
                        GameboardTower1GridView.ItemsSource = null;
                        GameboardTower1GridView.ItemsSource = Tower1;
                        break;
                    }
                case 2:
                    {
                        GameboardTower2GridView.ItemsSource = null;
                        GameboardTower2GridView.ItemsSource = Tower2;
                        break;
                    }
                default:
                    {
                        GameboardTower0GridView.ItemsSource = null;
                        GameboardTower0GridView.ItemsSource = Tower0;
                        GameboardTower1GridView.ItemsSource = null;
                        GameboardTower1GridView.ItemsSource = Tower1;
                        GameboardTower2GridView.ItemsSource = null;
                        GameboardTower2GridView.ItemsSource = Tower2;
                        break;
                    }
            }
        }









        // Get to desired TowerHeigth from the ContentDialog, and send it to NewGame()
        private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the dialog to start a new game
            ContentDialogResult result = await NewGameContentDialog.ShowAsync();

            // If the selected button, result, was the one for a new game
            if (result == ContentDialogResult.Primary)
            {
                NewGame((int)TowerHeightNumberBox.Value);
            }
            // Else the button was "cancel"
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SolutionStepFirstButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SolutionStepBackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SolutionStepForwardButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SolutionStepLastButton_Click(object sender, RoutedEventArgs e)
        {

        }


        private void TowerBlockButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            /*
            try
            {
                button = sender as Button;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: TowerBlockButton_Click failed to convert event sender to a Button. {0}", ex));
                return;
            }
            */
            
            string b = button.Name;
            Debug.WriteLine(string.Format("TowerOfHanoi: Towerblock clicked = {0}", b));

            if (!char.IsDigit(b[0]) || !char.IsDigit(b[1]))
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: TowerBlockButton_Click() could not read a valid button name {0}", b));
                return;
            }
            else
            {
                AnalyseInput((int)char.GetNumericValue(b[0]), (int)char.GetNumericValue(b[1]));
            }

        }



        // Helper method, print the current widths of the elements in gameboard to Output
        private void PrintGameboardWidths()
        {
            Debug.WriteLine(string.Format("TowerOfHanoi: Current Gameboard values are;"));

            for (int i = 0; i < Gameboard.GetLength(1); i++)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: {0} {1} {2}", Gameboard[0, i].Width, Gameboard[1, i].Width, Gameboard[2, i].Width));
            }
        }

        // Helper method, print the current states of the elements in gameboard to Output
        private void PrintGameboardStates()
        {
            Debug.WriteLine(string.Format("TowerOfHanoi: Current Gameboard states are;"));

            for (int i = 0; i < Gameboard.GetLength(1); i++)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: {0} {1} {2}", Gameboard[0, i].State, Gameboard[1, i].State, Gameboard[2, i].State));
            }
        }


    }
}

