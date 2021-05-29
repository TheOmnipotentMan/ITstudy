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


        // Gameboard state, block positions
        int[,] Gameboard;



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


            // Set the starting state of gameboard
            Gameboard = new int[towerHeight, 3];
            for (int mGB = 0; mGB < Gameboard.GetLength(0); mGB++)
            {
                // Set the tower on the first column
                Gameboard[mGB, 0] = mGB + 1;

                for (int nGB = 1; nGB < Gameboard.GetLength(1); nGB++)
                {
                    // Set all other elements to 0
                    Gameboard[mGB, nGB] = 0;
                }
            }


            // Generate the on-screen towers and their blocks
            double widthMax = GameboardGrid.ActualWidth / 3 - 20;
            TowerBlockWidthStep = (widthMax - TowerBlockWidthMinimum) / towerHeight;

            Tower0 = new ObservableCollection<TowerOfHanoiBlock>();
            Tower1 = new ObservableCollection<TowerOfHanoiBlock>();
            Tower2 = new ObservableCollection<TowerOfHanoiBlock>();

            for (int iBlocks = 0; iBlocks < towerHeight; iBlocks++)
            {
                Tower0.Add(new TowerOfHanoiBlock("0" + iBlocks.ToString(), TowerBlockHeight, widthMax));
                Tower0.Last().ShowBlock(TowerBlockWidthMinimum + iBlocks * TowerBlockWidthStep, BlockColours.ElementAt((iBlocks + BlockColours.Length) % BlockColours.Length));
                Debug.WriteLine(string.Format("TowerOfHanoi: Tower0 added block {0}", iBlocks));
            }

            // Make the first element of Tower0 clickable
            Tower0.First().IsClickable = true;

            // Set the on-screen towers to the generated collections
            GameboardTower0GridView.ItemsSource = Tower0;
            GameboardTower1GridView.ItemsSource = Tower1;
            GameboardTower2GridView.ItemsSource = Tower2;



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
            if (m + 1 >= Gameboard.GetLength(0)) { return true; }
            // Else find out if the block underneath is larger
            else
            {
                return (blockWidth < Gameboard[m + 1, n]);
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

        private void GameboardTower0Block_Click(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("TowerOfHanoi: tower block clicked"));
        }

        private void TowerBlockButton_Click(object sender, RoutedEventArgs e)
        {
            string b = (sender as Button).Name;
            Debug.WriteLine(string.Format("TowerOfHanoi: Towerblock clicked = {0}", b));

            if (!char.IsDigit(b[0]) || !char.IsDigit(b[1])) { return; }

            int tower = (int)char.GetNumericValue(b[0]);
            int level = (int)char.GetNumericValue(b[1]);

            switch (tower, level)
            {
                case (0, 0):
                    {
                        if (Tower0.ElementAt(level).BorderColour.Opacity == 0)
                        {
                            Tower0.ElementAt(level).ShowOutline(Windows.UI.Colors.Black);
                        }
                        else
                        {
                            Tower0.ElementAt(level).HideOutline();
                        }

                        Debug.WriteLine(string.Format("TowerOfHanoi: Towerblock ShowOutline called for {0}", level));

                        break;
                    }
            }
        }
    }
}
