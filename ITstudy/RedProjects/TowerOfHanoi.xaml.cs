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
            // The base width of this block, as a whole number
            public int Width;
            // The state of this block, eg Empty, Block, etc
            public BlockState State;

            public TowerBlock(int width, BlockState state = BlockState.Empty)
            {
                Width = width;
                State = state;
            }
        }


        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "24:00";
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
                Tower0.Add(new TowerOfHanoiBlock("0" + iBlock.ToString(), board[0, iBlock].Width, TowerBlockHeight, widthMax));
                Tower1.Add(new TowerOfHanoiBlock("1" + iBlock.ToString(), board[1, iBlock].Width, TowerBlockHeight, widthMax));
                Tower2.Add(new TowerOfHanoiBlock("2" + iBlock.ToString(), board[2, iBlock].Width, TowerBlockHeight, widthMax));

                // Store the 'base' block widths
                BlockWidths[iBlock] = TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep;

                // Fill the towers based on the info in board
                // Tower 0
                switch (board[0, iBlock].State)
                {
                    case BlockState.Empty:
                        {
                            break;
                        }
                    case BlockState.Available:
                        {
                            Tower0.Last().ShowAvailable(BlockWidths[iBlock]);
                            break;
                        }
                    case BlockState.Block:
                        {
                            Tower0.Last().ShowBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            break;
                        }
                    case BlockState.Selected:
                        {
                            Tower0.Last().ShowBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            Tower0.Last().ShowSelected();
                            break;
                        }
                }
                // Tower 1
                switch (board[1, iBlock].State)
                {
                    case BlockState.Empty:
                        {
                            break;
                        }
                    case BlockState.Available:
                        {
                            Tower1.Last().ShowAvailable(BlockWidths[iBlock]);
                            break;
                        }
                    case BlockState.Block:
                        {
                            Tower1.Last().ShowBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            break;
                        }
                    case BlockState.Selected:
                        {
                            Tower1.Last().ShowBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            Tower1.Last().ShowSelected();
                            break;
                        }
                }
                // Tower 2
                switch (board[2, iBlock].State)
                {
                    case BlockState.Empty:
                        {
                            break;
                        }
                    case BlockState.Available:
                        {
                            Tower2.Last().ShowAvailable(BlockWidths[iBlock]);
                            break;
                        }
                    case BlockState.Block:
                        {
                            Tower2.Last().ShowBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            break;
                        }
                    case BlockState.Selected:
                        {
                            Tower2.Last().ShowBlock(TowerBlockWidthMinimum + iBlock * TowerBlockWidthStep, BlockColours.ElementAt((iBlock + BlockColours.Length) % BlockColours.Length));
                            Tower2.Last().ShowSelected();
                            break;
                        }
                }

            }

            // Set the top block of the first tower as Clickable
            Tower0.Last().IsClickable = true;

            // Set the on-screen towers to the generated collections
            LoadItemsTower0();
            LoadItemsTower1();
            LoadItemsTower2();
        }



        private void AnalyseInput(int tower, int level)
        {
            // Make sure the input is reasonable, not out-of-bounds
            if (tower < 0 || tower > 2)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() recieved an out-of-bounds value for tower. tower = {0}, level = {1}", tower, level));
                return;
            }
            if (level < 0 || level > TowerHeight)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() recieved an out-of-bounds value for level. tower = {0}, level = {1}", tower, level));
                return;
            }


            switch (Gameboard[tower, level].State)
            {
                case BlockState.Empty:
                    {
                        break;
                    }
                case BlockState.Available:
                    {
                        if (Gameboard[SelectedBlock[0], SelectedBlock[1]].State != BlockState.Selected)
                        {
                            Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() want to call MoveBlock() but no SelectedBlock was found"));
                            return;
                        }
                        else
                        {
                            MoveBlock(tower, level, SelectedBlock[0], SelectedBlock[1]);
                        }
                        break;
                    }
                case BlockState.Block:
                    {
                        NewBlockSelected(tower, level);
                        break;
                    }
                case BlockState.Selected:
                    {
                        NewBlockSelected(-1, 0);
                        break;
                    }
            }

        }


        // Mark the newly selected block on screen, and un-mark any blocks previously selected, and find the positions of all possible moves
        // can clear all selected by giving a out-of-bounds value for tower, ie smaller than 0 or larger than 2
        private void NewBlockSelected(int tower, int level)
        {
            // Make sure level in within bounds
            if (level < 0 || level > (TowerHeight - 1))
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: NewBlockSelected() recieved invalid value for level, {0}", level));
                return;
            }

            // Set SelectedBlock to the currently selected block, if relevant
            if (tower < 0 || tower > 2)
            {
                Tower0.ElementAt((TowerHeight - 1) - GetHighestBlock(0)).HideBorder();
                Tower1.ElementAt((TowerHeight - 1) - GetHighestBlock(1)).HideBorder();
                Tower2.ElementAt((TowerHeight - 1) - GetHighestBlock(2)).HideBorder();
                return;
            }
            else
            {
                SelectedBlock[0] = tower; SelectedBlock[1] = level;
            }

            int x = 0;

            // Find possible moves and set or unset the border of the top blocks as required
            if (tower != 0)
            {
                x = GetHighestBlock(0);                
                if (IsMoveAllowed(Gameboard[tower, level].Width, 0, x))
                {
                    Gameboard[0, x].State = BlockState.Available;
                    Tower0.ElementAt((TowerHeight - 1) - x).ShowAvailable(BlockWidths[level]);
                    Tower0.ElementAt((TowerHeight - 1) - x).IsClickable = true;
                }
                else
                {
                    Tower0.ElementAt((TowerHeight - 1) - x).HideBorder();
                }
            }
            else
            {
                Gameboard[tower, level].State = BlockState.Selected;
                Tower0.ElementAt((TowerHeight - 1) - level).ShowSelected();
            }
            if (tower != 1)
            {                
                x = GetHighestBlock(1);                
                if (IsMoveAllowed(Gameboard[tower, level].Width, 1, x))
                {
                    Gameboard[1, x].State = BlockState.Available;
                    Tower1.ElementAt((TowerHeight - 1) - x).ShowAvailable(BlockWidths[level]);
                    Tower1.ElementAt((TowerHeight - 1) - x).IsClickable = true;
                }
                else
                {
                    Tower1.ElementAt((TowerHeight - 1) - x).HideBorder();
                }
            }
            else
            {
                Gameboard[tower, level].State = BlockState.Selected;
                Tower1.ElementAt((TowerHeight - 1) - level).ShowSelected();
            }
            if (tower != 2)
            {                
                x = GetHighestBlock(2);                
                if (IsMoveAllowed(Gameboard[tower, level].Width, 2, x))
                {
                    Gameboard[2, x].State = BlockState.Available;
                    Tower2.ElementAt((TowerHeight - 1) - x).ShowAvailable(BlockWidths[level]);
                    Tower2.ElementAt((TowerHeight - 1) - x).IsClickable = true;
                }
                else
                {
                    Tower2.ElementAt((TowerHeight - 1) - x).HideBorder();
                }
            }
            else
            {
                Gameboard[tower, level].State = BlockState.Selected;
                Tower2.ElementAt((TowerHeight - 1) - level).ShowSelected();                
            }

            // Reload all towers on screen
            ReloadTower();
                        
        }



        /// <summary>
        /// Move a block from one position to another
        /// </summary>
        /// <param name="endTower">The destination tower</param>
        /// <param name="endLevel">The destination level</param>
        /// <param name="startTower">The original tower</param>
        /// <param name="startLevel">The original level</param>
        private void MoveBlock(int endTower, int endLevel, int startTower = -1, int startLevel = -1)
        {
            if (endTower < 0 || endTower > 2 || endLevel < 0 || endLevel >= TowerHeight || startTower > 2 || startLevel >= TowerHeight)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() recieved invalid input"));
                return;
            }


            // If the default parameters were used, can find the currently selected block by looking at the UI-towers, and use it for start pos
            if (startTower < 0 || startLevel < 0)
            {
                for (int iL = TowerHeight - 1; iL < -1; iL--)
                {
                    if (Tower0.ElementAt(iL).BlockOpacity > 0 && Tower0.ElementAt(iL).BorderSelectedOpacity > 0) { startTower = 0; startLevel = iL; }
                    else if (Tower1.ElementAt(iL).BlockOpacity > 0 && Tower1.ElementAt(iL).BorderSelectedOpacity > 0) { startTower = 1; startLevel = iL; }
                    else if (Tower2.ElementAt(iL).BlockOpacity > 0 && Tower2.ElementAt(iL).BorderSelectedOpacity > 0) { startTower = 2; startLevel = iL; }
                }
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() found values for start pos, {0}, {1}", startTower, startLevel));
            }

            Gameboard[endTower, endLevel] = Gameboard[startTower, startLevel];
            Gameboard[startTower, startLevel].Width = 0;
            Gameboard[startTower, startLevel].State = BlockState.Empty;


            switch (endTower, startTower)
            {
                case (0, 1):
                    {
                        Tower0[(TowerHeight - 1) - endLevel] = Tower1.ElementAt((TowerHeight - 1) - startLevel);
                        Tower1.RemoveAt((TowerHeight - 1) - startLevel);
                        break;
                    }
                case (0, 2):
                    {
                        Tower0[(TowerHeight - 1) - endLevel] = Tower2.ElementAt((TowerHeight - 1) - startLevel);
                        Tower2.RemoveAt((TowerHeight - 1) - startLevel);
                        break;
                    }
                case (1, 0):
                    {
                        Tower1[(TowerHeight - 1) - endLevel] = Tower0.ElementAt((TowerHeight - 1) - startLevel);
                        Tower0.RemoveAt((TowerHeight - 1) - startLevel);
                        break;
                    }
                case (1, 2):
                    {
                        Tower1[(TowerHeight - 1) - endLevel] = Tower2.ElementAt((TowerHeight - 1) - startLevel);
                        Tower2.RemoveAt((TowerHeight - 1) - startLevel);
                        break;
                    }
                case (2, 0):
                    {
                        Tower2[(TowerHeight - 1) - endLevel] = Tower0.ElementAt((TowerHeight - 1) - startLevel);
                        Tower0.RemoveAt((TowerHeight - 1) - startLevel);
                        break;
                    }
                case (2, 1):
                    {
                        Tower2[(TowerHeight - 1) - endLevel] = Tower1.ElementAt((TowerHeight - 1) - startLevel);
                        Tower1.RemoveAt((TowerHeight - 1) - startLevel);
                        break;
                    }
            }

            NewBlockSelected(-1, 0);

        }



        /// <summary>
        /// Get the level of highest block of a speficied tower on the Gameboard
        /// </summary>
        /// <param name="tower">Which tower to look at</param>
        /// <returns></returns>
        private int GetHighestBlock(int tower)
        {
            int highestLevel = -1;

            for (int level = (Gameboard.GetLength(1) - 1); level > -1; level--)
            {
                // Find the first level with the value 0
                if (Gameboard[tower, level].Width == 0)
                {
                    // if the level is the bottom level, this is the highest possible level
                    if (level == Gameboard.GetLength(1) - 1)
                    {
                        highestLevel = level;
                    }
                    // else the highest level is the level below
                    else
                    {
                        highestLevel = level + 1;
                    }
                    break;
                }
            }

            return highestLevel;
        }


        /// <summary>
        /// Get whether or not a move is allowed. Checks the position below the desired one to see if there is a block there, and if is larger than the one we want to place on it.
        /// </summary>
        /// <param name="blockWidth">The width of the block we want to move</param>
        /// <param name="m">The column we want to move to</param>
        /// <param name="n">The row we want to move to</param>
        /// <returns>True or false; is the desired move is allowed or not.</returns>
        private bool IsMoveAllowed(int blockWidth, int tower, int level)
        {
            // If the desired position is at the bottom, always allowed
            if (level + 1 >= Gameboard.GetLength(1)) { return true; }
            // Else find out if the block underneath is larger
            else
            {
                return (blockWidth < Gameboard[tower, level + 1].Width);
            }
        }




        // Not-so-subtle way to reload towers when anything changes (would have liked to use PropertyChanged but couldn't figure it out)
        public void ReloadTower(int tower = -1)
        {
            switch (tower)
            {
                case 0:
                    {
                        LoadItemsTower0();
                        break;
                    }
                case 1:
                    {
                        LoadItemsTower1();
                        break;
                    }
                case 2:
                    {
                        LoadItemsTower2();
                        break;
                    }
                default:
                    {
                        LoadItemsTower0();
                        LoadItemsTower1();
                        LoadItemsTower2();
                        break;
                    }
            }
        }
        private void LoadItemsTower0()
        {
            GameboardTower0GridView.ItemsSource = null;
            GameboardTower0GridView.ItemsSource = Tower0;
        }
        private void LoadItemsTower1()
        {
            GameboardTower1GridView.ItemsSource = null;
            GameboardTower1GridView.ItemsSource = Tower1;
        }
        private void LoadItemsTower2()
        {
            GameboardTower2GridView.ItemsSource = null;
            GameboardTower2GridView.ItemsSource = Tower2;
        }


        private void PrintCurrentGameboardWidths()
        {
            Debug.WriteLine(string.Format("TowerOfHanoi: Current Gameboard values are;"));

            for (int i = 0; i < Gameboard.GetLength(1); i++)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: {0} {1} {2}", Gameboard[0, i].Width, Gameboard[1, i].Width, Gameboard[2, i].Width));
            }
        }


        private void PrintCurrentGameboardStates()
        {
            Debug.WriteLine(string.Format("TowerOfHanoi: Current Gameboard states are;"));

            for (int i = 0; i < Gameboard.GetLength(1); i++)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: {0} {1} {2}", Gameboard[0, i].State, Gameboard[1, i].State, Gameboard[2, i].State));
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


    }
}

