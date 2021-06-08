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
            int Width;
            BlockState State;

            public TowerBlock(int width, BlockState state = BlockState.Empty)
            {
                Width = width;
                State = state;
            }
        }


        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "08:00";
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
        enum BlockState { Empty, Block, Selected, Available };

        BlockState[,] Gameboard;

        int[,] Gameboard2;

        // Tower blocks, UI-elements
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
            Gameboard2 = new int[3, towerHeight];
            for (int level = 0; level < Gameboard2.GetLength(1); level++)
            {
                // Set the tower on the first column
                Gameboard2[0, level] = level + 1;

                // Set all other elements to 0
                Gameboard2[1, level] = 0;
                Gameboard2[2, level] = 0;

                /* Different approach for the two lines above, bit overkill seeing as this only needs to loop 2 times
                for (int tower = 1; tower < Gameboard.GetLength(0); tower++)
                {
                    // Set all other elements to 0
                    Gameboard[tower, level] = 0;
                }
                */

                Debug.WriteLine(string.Format("TowerOfHanoi: {0} {1} {2}", Gameboard2[0, level], Gameboard2[1, level], Gameboard2[2, level]));
            }


            // Generate the on-screen towers and their blocks
            double widthMax = GameboardGrid.ActualWidth / 3 - 20;
            TowerBlockWidthStep = (widthMax - TowerBlockWidthMinimum) / towerHeight;

            Tower0 = new ObservableCollection<TowerOfHanoiBlock>();
            Tower1 = new ObservableCollection<TowerOfHanoiBlock>();
            Tower2 = new ObservableCollection<TowerOfHanoiBlock>();

            for (int iBlocks = towerHeight; iBlocks > 0; iBlocks--)
            {
                Tower0.Add(new TowerOfHanoiBlock("0" + (iBlocks - 1).ToString(), iBlocks, TowerBlockHeight, widthMax));
                Tower1.Add(new TowerOfHanoiBlock("0" + (iBlocks - 1).ToString(), iBlocks, TowerBlockHeight, widthMax));
                Tower2.Add(new TowerOfHanoiBlock("0" + (iBlocks - 1).ToString(), iBlocks, TowerBlockHeight, widthMax));

                Tower0.Last().ShowBlock(TowerBlockWidthMinimum + (iBlocks - 1) * TowerBlockWidthStep, BlockColours.ElementAt(((iBlocks - 1) + BlockColours.Length) % BlockColours.Length));
                // Debug.WriteLine(string.Format("TowerOfHanoi: Towers added block {0}", iBlocks));
            }

            // Set the top block of the first tower as Clickable
            Tower0.Last().IsClickable = true;


            // Set the on-screen towers to the generated collections
            GameboardTower0GridView.ItemsSource = Tower0;
            GameboardTower1GridView.ItemsSource = Tower1;
            GameboardTower2GridView.ItemsSource = Tower2;





        }



        private void AnaylseInput(int tower, int level)
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

            /* OLD
            // Check if any block was already selected, if so unselect it
            // If it was also the input, the player unselected it deliberately, and so return
            bool blockWasUnselected = false;
            int x = GetHighestBlock(0);
            if (Tower0.ElementAt(x).BlockOpacity > 0 && Tower0.ElementAt(x).BorderOpacity > 0)
            {
                Tower0.ElementAt(x).HideBorder();
                if (tower == 0) { blockWasUnselected = true; }
            }
            x = GetHighestBlock(1);
            if (Tower1.ElementAt(x).BlockOpacity > 0 && Tower1.ElementAt(x).BorderOpacity > 0)
            {
                Tower1.ElementAt(x).HideBorder();
                if (tower == 1) { blockWasUnselected = true; }
            }
            x = GetHighestBlock(2);
            if (Tower2.ElementAt(x).BlockOpacity > 0 && Tower2.ElementAt(x).BorderOpacity > 0)
            {
                Tower2.ElementAt(x).HideBorder();
                if (tower == 2) { blockWasUnselected = true; }
            }
            if (blockWasUnselected)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() blockWasUnselected"));
                return;
            }
            */

            bool blockWasUnselected = false;
            bool blockWillBeMoved = false;
            int x = TowerHeight - GetHighestBlock(0);
            if (Tower0.ElementAt(x).BorderOpacity > 0)
            {
                // If block was selected previously, unselect it
                if (Tower0.ElementAt(x).BlockOpacity > 0)
                {
                    Tower0.ElementAt(x).HideBorder();
                    if (tower == 0) { blockWasUnselected = true; }
                }
                // Else this position is a valid move to make
                else
                {
                    blockWillBeMoved = true;
                }
                
                
            }
            x = TowerHeight - GetHighestBlock(1);
            if (Tower1.ElementAt(x).BlockOpacity > 0 && Tower1.ElementAt(x).BorderOpacity > 0)
            {
                Tower1.ElementAt(x).HideBorder();
                if (tower == 1) { blockWasUnselected = true; }
            }
            x = TowerHeight - GetHighestBlock(2);
            if (Tower2.ElementAt(x).BlockOpacity > 0 && Tower2.ElementAt(x).BorderOpacity > 0)
            {
                Tower2.ElementAt(x).HideBorder();
                if (tower == 2) { blockWasUnselected = true; }
            }
            if (blockWasUnselected)
            {
                Debug.WriteLine(string.Format("TowerOfHanoi: AnalyseInput() blockWasUnselected"));
                return;
            }
















            // Check the other towers for valid moves/block-positions
            if (tower != 0)
            {

            }
            if (tower != 1)
            {

            }
            if (tower != 2)
            {

            }



            for (int i = 0; i < 3; i++)
            {
                // skip if current tower
                if (i == tower) { continue; }

                else
                {

                }

            }
        }



        private void MoveBlock(int endTower, int endLevel, int startTower = -1, int startLevel = -1)
        {
            // If the default parameters were used, find the currently selected block and use it for start pos
            if (startTower < 0 || startLevel < 0)
            {
                for (int iL = TowerHeight - 1; iL < -1; iL--)
                {
                    if (Tower0.ElementAt(iL).BlockOpacity > 0 && Tower0.ElementAt(iL).BorderOpacity > 0) { startTower = 0; startLevel = iL; }
                    else if (Tower1.ElementAt(iL).BlockOpacity > 0 && Tower1.ElementAt(iL).BorderOpacity > 0) { startTower = 1; startLevel = iL; }
                    else if (Tower2.ElementAt(iL).BlockOpacity > 0 && Tower2.ElementAt(iL).BorderOpacity > 0) { startTower = 2; startLevel = iL; }
                }
                Debug.WriteLine(string.Format("TowerOfHanoi: MoveBlock() found values for start pos, {0}, {1}", startTower, startLevel));
            }
        }



        /// <summary>
        /// Get the level of highest block of a speficied tower on the Gameboard
        /// </summary>
        /// <param name="tower"></param>
        /// <returns></returns>
        private int GetHighestBlock(int tower)
        {
            int highestLevel = -1;

            for (int level = (Gameboard2.GetLength(1) - 1); level > 0; level--)
            {
                // Find the first level with the value 0
                if (Gameboard2[tower, level] == 0)
                {
                    // if the level is the bottom level, this is the highest level
                    if (level == Gameboard2.GetLength(1) - 1)
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
        private bool IsMoveAllowed(int blockWidth, int m, int n)
        {
            // If the desired position is at the bottom, always allowed
            if (m + 1 >= Gameboard2.GetLength(0)) { return true; }
            // Else find out if the block underneath is larger
            else
            {
                return (blockWidth < Gameboard2[m + 1, n]);
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

            if (!char.IsDigit(b[0]) || !char.IsDigit(b[1])) { Debug.WriteLine(string.Format("TowerOfHanoi: TowerBlockButton_Click could not read button name {0}", b)); return; }

            int tower = (int)char.GetNumericValue(b[0]);
            int level = (int)char.GetNumericValue(b[1]);

            switch (tower)
            {
                case (0):
                    {
                        Debug.WriteLine(string.Format("TowerOfHanoi: Tower0 Towerblock called for {0}", level));

                        break;
                    }
                case (1):
                    {
                        Debug.WriteLine(string.Format("TowerOfHanoi: Tower1 Towerblock called for {0}", level));

                        break;
                    }
                case (2):
                    {
                        Debug.WriteLine(string.Format("TowerOfHanoi: Tower2 Towerblock called for {0}", level));

                        break;
                    }
            }

            Debug.WriteLine(string.Format("TowerOfHanoi: Towerblock clicked at {0} {1}", tower, level));

        }


    }
}






/*
 
 
        private class TowerBlockObj : Grid
        {
            // Base variables, should not change once constructed
            public int[] BlockPos = new int[2];
            public string BlockName { get; }


            public int BlockNumber { get; }
            private double BlockWidth;
            private double BlockHeight;
            private SolidColorBrush BlockColour = new SolidColorBrush();


            // UI elements
            Rectangle Rectangle = new Rectangle();
            Border Border = new Border();
            Button Button = new Button();


            /// <summary>
            /// Tower block, to be inserted in a StackPanel.
            /// </summary>
            /// <param name="m">Position of block on Gameboard, m-coordinate.</param>
            /// <param name="n">Position of block on Gameboard, n-coordinate.</param>
            /// <param name="blockNumber">The width of this block represented as a whole number. ie 1, 2, 3, 4, etc. Not actual width.</param>
            /// <param name="width">The actual, maximum width of this block</param>
            /// <param name="height">The height of this block</param>
            /// <param name="state">The desired state of this block</param>
            TowerBlockObj(int m, int n, int blockNumber, double width, double height, Windows.UI.Color blockColour, BlockState state = BlockState.Empty)
            {
                BlockPos[0] = m; BlockPos[1] = n;
                BlockName = m.ToString() + n.ToString();
                BlockNumber = blockNumber;

                BlockWidth = width;
                BlockHeight = height;
                BlockColour.Color = blockColour;


                Rectangle.Width = BlockWidth;
                Rectangle.Height = BlockHeight;
                Rectangle.Fill = BlockColour;
                Button.Width = BlockWidth;
                Button.Height = BlockHeight;
                Button.Background.Opacity = 0;
                Button.IsHitTestVisible = false;
                

                switch (state)
                {
                    case BlockState.Block:
                        {
                            ShowBlock();
                            break;
                        }
                    case BlockState.Outline:
                        {
                            ShowOutline();
                            break;
                        }
                    case BlockState.Empty:
                    default:
                        {
                            break;
                        }
                }

            }


            /// <summary>
            /// Show this block on screen
            /// </summary>
            public void ShowBlock(double width = -1)
            {
                if (width < 0) { width = BlockWidth; }

                // Clear any pre-existing child objects
                this.Children.Clear();
                                
                // Add the relevant elements to the TowerBlock
                this.Children.Add(Rectangle);
                this.Children.Add(Button);
            }

            /// <summary>
            /// Show the outline of this block on screen
            /// </summary>
            public void ShowOutline()
            {
                // Clear any pre-existing child objects
                this.Children.Clear();


            }

            /// <summary>
            /// Hide all UI elements of this block
            /// </summary>
            public void Hide()
            {
                this.Children.Clear();
            }


            public void SetIsClickable(bool isClickable)
            {
                Button.IsHitTestVisible = isClickable;
            }

        } 
 
 
 */
