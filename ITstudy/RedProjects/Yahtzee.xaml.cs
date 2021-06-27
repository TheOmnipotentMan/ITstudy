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


// Image display, BitmapImage
using Windows.UI.Xaml.Media.Imaging;

// TODO remove if the app is ever finished
using System.Diagnostics;





namespace ITstudy.RedProjects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Yahtzee : Page
    {
        // Score sheet
        private class ScoreBoard
        {
            /// <summary>
            /// Whether this player should be controlled by the computer or not
            /// </summary>
            bool IsComputer;

            // Upper section
            int Aces = -1;
            int Twos = -1;
            int Threes = -1;
            int Fours = -1;
            int Fives = -1;
            int Sixes = -1;

            // Lower section
            int ThreeOfAKind = -1;
            int FourOfAKind = -1;
            int FullHouse = -1;
            int SmallStraight = -1;
            int LargeStraight = -1;
            int Yahtzee = -1;
            int Chance = -1;

            public ScoreBoard(bool isComputer)
            {
                IsComputer = isComputer;
            }
        }


        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "16:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "x";
        // Date when this project was finished
        string ProjectDateFinished = "00/00/21";


        // Random number generator, for dice throws
        Random Random = new Random();

        // The current dice on the table, their values
        int[] CurrentDiceValues = new int[5];

        // The current round of the game
        int CurrentRound = 0;

        // The current roll of the player, starting at zero and ending at 3
        int CurrentRoll = 0;


        // Die face image sources
        Uri DiceFace1 = null;
        Uri DiceFace2 = null;
        Uri DiceFace3 = null;
        Uri DiceFace4 = null;
        Uri DiceFace5 = null;
        Uri DiceFace6 = null;
        Uri DiceFaceNone = null;
        Uri DiceFaceError = null;
        bool DiceImageFilesPresent = false;

        // Category names
        #region CategoryNames
        public string CatergoryAces = "Aces";
        public string CatergoryTwos = "Twos";
        public string CatergoryThrees = "Threes";
        public string CatergoryFours = "Fours";
        public string CatergoryFives = "Fives";
        public string CatergorySixes = "Sixes";
        public string CatergoryUpperBonus = "Bonus";

        public string CatergoryThreeKind = "Three of a Kind";
        public string CatergoryFourKind = "Four of a Kind";
        public string CatergoryFullHouse = "Full House";
        public string CatergorySmallStraight = "Small Straight";
        public string CatergoryLargeStraight = "Large Straight";
        public string CatergoryYahtzee = "Yahtzee";
        public string CatergoryChance = "Chance";
        public string CatergoryYahtzeeBonus = "Yahtzee Bonus";
        #endregion CategoryNames



        public Yahtzee()
        {
            this.InitializeComponent();
            FinishSetup();
        }


        private void FinishSetup()
        {
            // Set opacity of ContentDialogs back to 1, initially set to 0 in xaml to visually hide the dialogs in the editor
            NewGameContentDialog.Opacity = 1;
            GameRulesContentDialog.Opacity = 1;

            // Set the file-path for the dice face image files, log a message if no file exists at specified location
            // Count the image files found, if all files were found set DiceImageFilesPresent to true
            int filesFound = 0;
            string pathBase = "ms-appx:///";
            string face1Path = "Assets\\Yahtzee\\DiceFace1.png";
            string face2Path = "Assets\\Yahtzee\\DiceFace2.png";
            string face3Path = "Assets\\Yahtzee\\DiceFace3.png";
            string face4Path = "Assets\\Yahtzee\\DiceFace4.png";
            string face5Path = "Assets\\Yahtzee\\DiceFace5.png";
            string face6Path = "Assets\\Yahtzee\\DiceFace6.png";
            string faceNonePath = "Assets\\Yahtzee\\DiceFaceNone.png";
            string faceErrorPath = "Assets\\Yahtzee\\DiceFaceError.png";
            string fileNotFoundStatement = "Yahtzee: Could not find image file for DiceFace{0} at {1}";
            if (File.Exists(face1Path)) { DiceFace1 = new Uri(pathBase + face1Path); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "1", face1Path)); }
            if (File.Exists(face2Path)) { DiceFace2 = new Uri(pathBase + face2Path); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "2", face2Path)); }
            if (File.Exists(face3Path)) { DiceFace3 = new Uri(pathBase + face3Path); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "3", face3Path)); }
            if (File.Exists(face4Path)) { DiceFace4 = new Uri(pathBase + face4Path); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "4", face4Path)); }
            if (File.Exists(face5Path)) { DiceFace5 = new Uri(pathBase + face5Path); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "5", face5Path)); }
            if (File.Exists(face6Path)) { DiceFace6 = new Uri(pathBase + face6Path); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "6", face6Path)); }
            if (File.Exists(faceNonePath)) { DiceFaceNone = new Uri(pathBase + faceNonePath); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "6", faceNonePath)); }
            if (File.Exists(faceErrorPath)) { DiceFaceError = new Uri(pathBase + faceErrorPath); filesFound++; }
            else { Debug.WriteLine(string.Format(fileNotFoundStatement, "6", faceErrorPath)); }
            if (filesFound == 8) { DiceImageFilesPresent = true; }

        }








        private void NewGame(int playerCount, int computerCount)
        {
            // Clear any dice values on screen
            ClearDiceValues();

            // Set the current dice values to 0
            CurrentDiceValues = new int[5] { 0, 0, 0, 0, 0 };

            // Set the current roll and round counters to 0
            CurrentRoll = 0;
            UpdateRollCounter();
            CurrentRound = 0;
            UpdateRoundCounter();

            // Set the ToggleButtons, used to allow the holding of a die, to unchecked and not-clickable,
            // since there will be no values to hold until the first roll of the dice
            SetDiceToggleButtonsAvailable(false);

            // Set the RollDiceButton IsEnabled to true, to allow for the starting player to roll the dice
            RollDiceButton.IsEnabled = true;



        }








        /// <summary>
        /// Progresses the game to the next roll, or moves the game on to the NextPlayer() if max allowed rolls (3) has been reached
        /// </summary>
        private void NextRoll()
        {
            // Number of allowed dice-rolls, yahtzee rules specify 3
            int rollsAllowed = 3;

            // If the current roll is the first, enable the ToggleButtons on the dice to allow holding chosen dice
            if (CurrentRoll == 0)
            {
                SetDiceToggleButtonsAvailable(true);
            }
            // If the current roll is the last one, display a representation of this on screen, notifying the player that the next turn will be for the next player
            else if (CurrentRoll == rollsAllowed - 1)
            {
                // Disable the button to roll dice, this leave the player only able to commit its score to a category on the scorecard, thereby advancing the game
                // TODO reinstate: RollDiceButton.IsEnabled = false;
            }

            // Increment the CurrentRoll counter, then call RollDice() to do the new roll
            CurrentRoll++;
            RollDice();

        }


        /// <summary>
        /// Move the game on to the next player, clearing the previous dice-values and updating the scorecard to that of the current player
        /// </summary>
        private void NextPlayer()
        {
            ClearDiceValues();
            CurrentRoll = 0;
            RollDiceButton.IsEnabled = true;
            
        }


        /// <summary>
        /// Roll the dice, excluding the dice that have been set to hold
        /// This only rolls the dice, if you want the game to also progress to the next turn/roll, use NextRoll() instead
        /// </summary>
        private void RollDice()
        {
            // Get which dice to hold and which to roll
            bool[] holdDice = new bool[5]
            {
                (CurrentDiceValues[0] == 0 ? false : (Die1ToggleButton.IsChecked == true)),
                (CurrentDiceValues[1] == 0 ? false : (Die2ToggleButton.IsChecked == true)),
                (CurrentDiceValues[2] == 0 ? false : (Die3ToggleButton.IsChecked == true)),
                (CurrentDiceValues[3] == 0 ? false : (Die4ToggleButton.IsChecked == true)),
                (CurrentDiceValues[4] == 0 ? false : (Die5ToggleButton.IsChecked == true)),
            };
            
            // Roll new values
            int[] dice = new int[5];
            for (int i = 0; i < dice.Length; i++)
            {
                dice[i] = (holdDice[i]) ? CurrentDiceValues[i] : NewDie();
                Debug.WriteLine($"Yahtzee: RollDice() roll={CurrentRoll} i={i}, holdDice={holdDice[i]}, dice={dice[i]}");
            }

            // Set the new values to the CurrentDiceValues
            CurrentDiceValues = dice;

            // Show the dice on screen
            ShowDice(CurrentDiceValues, holdDice);
        }
        /// <summary>
        /// Get a new roll of a die
        /// </summary>
        /// <returns>A number between 0 and 6</returns>
        private int NewDie()
        {
            return Random.Next(1, 7);
        }








        /// <summary>
        /// Set the UI representation of the dice to the specified values, also updates roll-counter
        /// </summary>
        /// <param name="dice">int array of the 5 die values</param>
        /// <param name="holdDice">Which dice to </param>
        private void ShowDice(int[] dice, bool[] holdDice)
        {
            if (dice.Length < 5) { Debug.WriteLine(string.Format("Yahtzee: ShowDice() input int[] was too short; length={0}", dice.Length)); return; }

            // If all required image files were found, use them. Else use text numbers.
            if (DiceImageFilesPresent)
            {
                // If holdDice is false, ie the toggle button of the die is not checked, change the image
                if (!holdDice[0])
                {
                    Die1Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[0]));
                }
                if (!holdDice[0])
                {
                    Die2Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[1]));
                }
                if (!holdDice[0])
                {
                    Die3Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[2]));
                }
                if (!holdDice[0])
                {
                    Die4Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[3]));
                }
                if (!holdDice[0])
                {
                    Die5Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[4]));
                }
            }
            else
            {
                // If holdDice is false, ie the toggle button of the die is not checked, change the text
                if (!holdDice[0])
                {
                    Die1TextBlock.Text = dice[0].ToString();
                }
                if (!holdDice[1])
                {
                    Die2TextBlock.Text = dice[1].ToString();
                }
                if (!holdDice[2])
                {
                    Die3TextBlock.Text = dice[2].ToString();
                }
                if (!holdDice[3])
                {
                    Die4TextBlock.Text = dice[3].ToString();
                }
                if (!holdDice[4])
                {
                    Die5TextBlock.Text = dice[4].ToString();
                }
            }

            // Update the roll-counter (turn counter)
            UpdateRollCounter();
        }
        // Returns the Uri filepath to the relevant image source, for the specified number. Returns an error image source if no file is available for the given number, or null if no file was found.
        private Uri GetDiceFaceImageSource(int x)
        {
            Uri uri = null;
            switch (x)
            {
                case 0: { uri = DiceFaceNone; break; }
                case 1: { uri = DiceFace1; break; }
                case 2: { uri = DiceFace2; break; }
                case 3: { uri = DiceFace3; break; }
                case 4: { uri = DiceFace4; break; }
                case 5: { uri = DiceFace5; break; }
                case 6: { uri = DiceFace6; break; }
                default: { Debug.WriteLine(string.Format("Yahtzee: GetDiceFaceImageSource() did not find a matching case for input {0}", x)); uri = DiceFaceError; break; }
            }
            return uri;
        }


        /// <summary>
        /// Update the RollCounterTextBlock to the value of CurrentRoll
        /// </summary>
        private void UpdateRollCounter() { RollCounterTextBlock.Text = "Roll " + CurrentRoll.ToString(); }
        /// <summary>
        /// Update the GameRoundTextBlock to the value of CurrentRound
        /// </summary>
        private void UpdateRoundCounter() { GameRoundTextBlock.Text = "Round " + ((CurrentRound < 10) ? "0" : "") + CurrentRound.ToString(); }


        /// <summary>
        /// Clear all dice values on screen
        /// </summary>
        private void ClearDiceValues()
        {
            // Clear any images
            Die1Image.Source = null;
            Die2Image.Source = null;
            Die3Image.Source = null;
            Die4Image.Source = null;
            Die5Image.Source = null;

            // Clear any text
            Die1TextBlock.Text = "";
            Die2TextBlock.Text = "";
            Die3TextBlock.Text = "";
            Die4TextBlock.Text = "";
            Die5TextBlock.Text = "";
        }








        /// <summary>
        /// Set if the ToggleButtons for die-holding should be available or not, relevant on the start of a turn where they should not be available
        /// </summary>
        /// <param name="state">The state of availablility to set the ToggleButtons too</param>
        private void SetDiceToggleButtonsAvailable(bool? state = null)
        {
            // Uncheck all ToggleButtons
            Die1ToggleButton.IsChecked = false;
            Die2ToggleButton.IsChecked = false;
            Die3ToggleButton.IsChecked = false;
            Die4ToggleButton.IsChecked = false;
            Die5ToggleButton.IsChecked = false;

            // Cast the bool? state to the bool newState, with null = false. https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
            bool newState = state ?? false;

            // Set whether the buttons can are clickable or not, depending on newState
            Die1ToggleButton.IsHitTestVisible = newState;
            Die2ToggleButton.IsHitTestVisible = newState;
            Die3ToggleButton.IsHitTestVisible = newState;
            Die4ToggleButton.IsHitTestVisible = newState;
            Die5ToggleButton.IsHitTestVisible = newState;
        }


        



        private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the dialog to start a new game
            ContentDialogResult result = await NewGameContentDialog.ShowAsync();

            // If the selected button, result, was the one for a new game
            if (result == ContentDialogResult.Primary)
            {
                NewGame((int)TotalPlayerCountNumberBox.Value, (int)ComputerPlayerCountNumberBox.Value);
            }
            // Else the button was "cancel"
        }

        private async void GameRulesButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the dialog explaining the basic rules of Yahtzee
            await GameRulesContentDialog.ShowAsync();
        }

        private void RollDiceButton_Click(object sender, RoutedEventArgs e)
        {
            NextRoll();
        }

        private void ScorecardButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
