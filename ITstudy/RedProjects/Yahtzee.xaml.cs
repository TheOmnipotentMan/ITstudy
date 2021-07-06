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

// Timers, for wait funtion
using System.Timers;

// Image display, BitmapImage
using Windows.UI.Xaml.Media.Imaging;

// used to create a paragraph for EndGame message, to pass to RichTextBlock
using Windows.UI.Xaml.Documents;

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
        private class Scorecard
        {
            /// <summary>
            /// The name of this player
            /// </summary>
            string PlayerName;
            /// <summary>
            /// Whether this player should be controlled by the computer or not
            /// </summary>
            bool IsComputer;


            // Upper section scores
            int Aces = -1;
            int Twos = -1;
            int Threes = -1;
            int Fours = -1;
            int Fives = -1;
            int Sixes = -1;
            int Bonus = -1;

            // Lower section scores
            int ThreeKind = -1;
            int FourKind = -1;
            int FullHouse = -1;
            int SmallStraight = -1;
            int LargeStraight = -1;
            int Yahtzee = -1;
            int Chance = -1;
            int YahtzeeBonus = -1;

            int TotalScore = 0;


            // Upper section dice
            int[] AcesDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] TwosDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] ThreesDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] FoursDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] FivesDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] SixesDice = new int[5] { 0, 0, 0, 0, 0 };

            // Lower section dice
            int[] ThreeKindDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] FourKindDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] FullHouseDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] SmallStraightDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] LargeStraightDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] YahtzeeDice = new int[5] { 0, 0, 0, 0, 0 };
            int[] ChanceDice = new int[5] { 0, 0, 0, 0, 0 };


            /// <summary>
            /// Create a yahtzee scorecard.
            /// </summary>
            /// <param name="playerName">The name of the player</param>
            /// <param name="isComputer">Is this player to be controlled by the computer</param>
            public Scorecard(string playerName, bool isComputer)
            {
                PlayerName = playerName;
                IsComputer = isComputer;
            }


            



            public string GetPlayerName() { return PlayerName; }
            public bool GetIsComputer() { return IsComputer; }


            /// <summary>
            /// Set the score for a specific category
            /// </summary>
            /// <param name="cat">The category which to set</param>
            /// <param name="score">The score</param>
            /// <param name="dice">The dice that resulted in the score</param>
            public void SetScore(Categories cat, int score, int[] dice)
            {
                // Make sure the are the correct number of dice
                if (dice.Count() != 5) { Debug.WriteLine($"Yahtzee: Scorecard.SetScore() int[] dice is of invalid size, dice.Count={dice.Count()}"); return; }

                Debug.WriteLine($"Yahtzee: Scorecard.SetScore() setting {cat}, {score}, {dice[0]} {dice[1]} {dice[2]} {dice[3]} {dice[4]}");

                // Enter the score
                switch (cat)
                {
                    // Upper section
                    case Categories.Aces: { Aces = score; AcesDice = dice; TotalScore += score; break; }
                    case Categories.Twos: { Twos = score; TwosDice = dice; TotalScore += score; break; }
                    case Categories.Threes: { Threes = score; ThreesDice = dice; TotalScore += score; break; }
                    case Categories.Fours: { Fours = score; FoursDice = dice; TotalScore += score; break; }
                    case Categories.Fives: { Fives = score; FivesDice = dice; TotalScore += score; break; }
                    case Categories.Sixes: { Sixes = score; SixesDice = dice; TotalScore += score; break; }
                    case Categories.Bonus: { Bonus = score; break; }

                    // Lower section
                    case Categories.ThreeKind: { ThreeKind = score; ThreeKindDice = dice; TotalScore += score; break; }
                    case Categories.FourKind: { FourKind = score; FourKindDice = dice; TotalScore += score; break; }
                    case Categories.FullHouse: { FullHouse = score; FullHouseDice = dice; TotalScore += score; break; }
                    case Categories.SmallStraight: { SmallStraight = score; SmallStraightDice = dice; TotalScore += score; break; }
                    case Categories.LargeStraight: { LargeStraight = score; LargeStraightDice = dice; TotalScore += score; break; }
                    case Categories.Yahtzee: { Yahtzee = score; YahtzeeDice = dice; TotalScore += score; break; }
                    case Categories.Chance: { Chance = score; ChanceDice = dice; TotalScore += score; break; }
                    case Categories.YahtzeeBonus: { if (YahtzeeBonus < 0) { YahtzeeBonus = 0; } YahtzeeBonus += score; TotalScore += score; break; }

                    default: { Debug.WriteLine($"Yahtzee: Scorecard.SetScore() did not recognise the specified category, cat = {(int)cat} {cat}"); break; }
                }
            }

            /// <summary>
            /// Get the score for a category
            /// </summary>
            /// <param name="cat"></param>
            /// <returns>The score</returns>
            public int GetScore(Categories cat)
            {
                switch (cat)
                {                    
                    // Upper section
                    case Categories.Aces: { return Aces; }
                    case Categories.Twos: { return Twos; }
                    case Categories.Threes: { return Threes; }
                    case Categories.Fours: { return Fours; }
                    case Categories.Fives: { return Fives; }
                    case Categories.Sixes: { return Sixes; }
                    case Categories.Bonus: { return Bonus; }

                    // Lower section
                    case Categories.ThreeKind: { return ThreeKind; }
                    case Categories.FourKind: { return FourKind; }
                    case Categories.FullHouse: { return FullHouse; }
                    case Categories.SmallStraight: { return SmallStraight; }
                    case Categories.LargeStraight: { return LargeStraight; }
                    case Categories.Yahtzee: { return Yahtzee; }
                    case Categories.Chance: { return Chance; }
                    case Categories.YahtzeeBonus: { return YahtzeeBonus; }

                    default: { Debug.WriteLine($"Yahtzee: Scorecard.GetScore() given category does not exist. cat = {(int)cat} {cat}"); return 0; }
                }
            }

            public int GetTotalScore() { return TotalScore; }

            /// <summary>
            /// Get the dice that resulted in the score for a specific category
            /// </summary>
            /// <param name="cat"></param>
            /// <returns>int[5] of dice values</returns>
            public int[] GetDice(Categories cat)
            {
                switch (cat)
                {
                    // Upper section
                    case Categories.Aces: { return AcesDice; }
                    case Categories.Twos: { return TwosDice; }
                    case Categories.Threes: { return ThreesDice; }
                    case Categories.Fours: { return FoursDice; }
                    case Categories.Fives: { return FivesDice; }
                    case Categories.Sixes: { return SixesDice; }

                    // Lower section
                    case Categories.ThreeKind: { return ThreeKindDice; }
                    case Categories.FourKind: { return FourKindDice; }
                    case Categories.FullHouse: { return FullHouseDice; }
                    case Categories.SmallStraight: { return SmallStraightDice; }
                    case Categories.LargeStraight: { return LargeStraightDice; }
                    case Categories.Yahtzee: { return YahtzeeDice; }
                    case Categories.Chance: { return ChanceDice; }

                    default: { Debug.WriteLine($"Yahtzee: Scorecard.GetDice() given category does not exist. cat = {(int)cat} {cat}"); return new int[5] { 0, 0, 0, 0, 0 }; }
                }
            }

            /// <summary>
            /// Get the score for a specified category as a string
            /// </summary>
            /// <param name="cat">The category of which to get the score</param>
            /// <returns>A string representation of the score</returns>
            public string GetScoreString(Categories cat)
            {
                int score = 0;
                switch (cat)
                {
                    // Upper section
                    case Categories.Aces: { score = Aces; break; }
                    case Categories.Twos: { score = Twos; break; }
                    case Categories.Threes: { score = Threes; break; }
                    case Categories.Fours: { score = Fours; break; }
                    case Categories.Fives: { score = Fives; break; }
                    case Categories.Sixes: { score = Sixes; break; }
                    case Categories.Bonus: { score = Bonus; break; }

                    // Lower section
                    case Categories.ThreeKind: { score = ThreeKind; break; }
                    case Categories.FourKind: { score = FourKind; break; }
                    case Categories.FullHouse: { score = FullHouse; break; }
                    case Categories.SmallStraight: { score = SmallStraight; break; }
                    case Categories.LargeStraight: { score = LargeStraight; break; }
                    case Categories.Yahtzee: { score = Yahtzee; break; }
                    case Categories.Chance: { score = Chance; break; }
                    case Categories.YahtzeeBonus: { score = YahtzeeBonus; break; }

                    default: { Debug.WriteLine($"Yahtzee: Scorecard.GetScoreString() given category does not exist. cat = {(int)cat} {cat}"); break; }
                }
                return score.ToString();
            }

            /// <summary>
            /// Get the dice that resulted in the score for a specific category as a string
            /// </summary>
            /// <param name="cat"></param>
            /// <returns>A string representation of the dice, eg (1, 2, 3, 4, 5)</returns>
            public string GetDiceString(Categories cat)
            {
                int[] dice = new int[5] { 0, 0, 0, 0, 0 };
                switch (cat)
                {
                    // Upper section
                    case Categories.Aces: { dice = AcesDice; break; }
                    case Categories.Twos: { dice = TwosDice; break; }
                    case Categories.Threes: { dice = ThreesDice; break; }
                    case Categories.Fours: { dice = FoursDice; break; }
                    case Categories.Fives: { dice = FivesDice; break; }
                    case Categories.Sixes: { dice = SixesDice; break; }

                    // Lower section
                    case Categories.ThreeKind: { dice = ThreeKindDice; break; }
                    case Categories.FourKind: { dice = FourKindDice; break; }
                    case Categories.FullHouse: { dice = FullHouseDice; break; }
                    case Categories.SmallStraight: { dice = SmallStraightDice; break; }
                    case Categories.LargeStraight: { dice = LargeStraightDice; break; }
                    case Categories.Yahtzee: { dice = YahtzeeDice; break; }
                    case Categories.Chance: { dice = ChanceDice; break; }

                    default: { Debug.WriteLine($"Yahtzee: Scorecard.GetDiceString() given category does not exist. cat = {(int)cat} {cat}"); break; }
                }
                return "(" + dice[0].ToString() + ", " + dice[1].ToString() + ", " + dice[2].ToString() + ", " + dice[3].ToString() + ", " + dice[4].ToString() + ")";
            }

            public bool IsScorecardFull()
            {
                return (Aces != -1 && Twos != -1 && Threes != -1 && Fours != -1 && Fives != -1 && Sixes != -1 &&
                        ThreeKind != -1 && FourKind != -1 && FullHouse != -1 && SmallStraight != -1 && LargeStraight != -1 && Yahtzee != -1 && Chance != -1);
            }
        }




























        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "32:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "5";
        // Date when this project was finished
        string ProjectDateFinished = "06/07/21";






        // EndGame message, final player scores
        public List<string> FinalScores;



        // Random number generator, for dice throws
        Random Random = new Random();

        // The current dice on the table, their values
        int[] CurrentDiceValues = new int[5];
        // Which dice to hold and which to roll
        bool[] HoldDice = new bool[5] { false, false, false, false, false };

        // The current round of the game
        int CurrentRound = 0;
        // The current roll of the player, starting at zero and ending at 3
        int CurrentRoll = 0;

        // List of players
        List<Scorecard> Players;
        // Current player
        int CurrentPlayer;

        // Categories, tags
        enum Categories { Aces, Twos, Threes, Fours, Fives, Sixes, Bonus, ThreeKind, FourKind, FullHouse, SmallStraight, LargeStraight, Yahtzee, Chance, YahtzeeBonus }

        

        // Scorecard category border colours, enabled & disabled
        SolidColorBrush CategoryScoreBorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        SolidColorBrush CategoryDumpBorderBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        SolidColorBrush CategoryDisabledBorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);

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


            // Set the tags for the scorecard buttons as required
            // Upper section
            ScorecardAcesButton.Tag = Categories.Aces;
            ScorecardTwosButton.Tag = Categories.Twos;
            ScorecardThreesButton.Tag = Categories.Threes;
            ScorecardFoursButton.Tag = Categories.Fours;
            ScorecardFivesButton.Tag = Categories.Fives;
            ScorecardSixesButton.Tag = Categories.Sixes;
            // Lower section
            ScorecardThreeKindButton.Tag = Categories.ThreeKind;
            ScorecardFourKindButton.Tag = Categories.FourKind;
            ScorecardFullHouseButton.Tag = Categories.FullHouse;
            ScorecardSmallStraightButton.Tag = Categories.SmallStraight;
            ScorecardLargeStraightButton.Tag = Categories.LargeStraight;
            ScorecardYahtzeeButton.Tag = Categories.Yahtzee;
            ScorecardChanceButton.Tag = Categories.Chance;
            ScorecardYahtzeeBonusButton.Tag = Categories.YahtzeeBonus;

        }








        private void NewGame(int playerCount)
        {
            // Create the players and their scorecards
            Players = new List<Scorecard>();
            for (int pl = 0; pl < playerCount; pl++)
            {
                Players.Add(new Scorecard(pl.ToString(), false));
            }

            // Reset the total scores
            UpdateTotalScores();

            // Set the current player and round counters to 0
            CurrentRound = 0;
            UpdateRoundCounter();
            CurrentPlayer = 0;

            // Start play
            NextPlayer();


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
                SetDiceHoldingAllowed(true);
            }
            // If the current roll is the last one, display a representation of this on screen, notifying the player that the next turn will be for the next player
            else if (CurrentRoll == rollsAllowed - 1)
            {
                // Disable the button to roll dice, this leave the player only able to commit its score to a category on the scorecard, thereby advancing the game (comment out for unlimited rolls)
                // TODO re-instate; RollDiceButton.IsEnabled = false;
            }

            // Increment the CurrentRoll counter and update it on screen
            CurrentRoll++;
            UpdateRollCounter();
            // Do the new roll
            RollDice();
                       

            // Analyse the dice and unlock the available categories for the current dice-values
            AnalyseDice(CurrentDiceValues);




        }
        

        /// <summary>
        /// Commit a score to a category and progress the game
        /// </summary>
        /// <param name="cat"></param>
        private void EndTurn(Categories cat)
        {
            // Enter the score on the scorecard of the current player
            Players[CurrentPlayer].SetScore(cat, CalculateScore(cat), CurrentDiceValues);

            // Figure out if the upper section bonus should be applied
            // !The following requires the categories 1 to 6, Aces to Sixes, to be the first six elements of the enum Categories!
            if ((int)cat <= (int)Categories.Sixes)
            {
                if (Players[CurrentPlayer].GetScore(Categories.Aces) + Players[CurrentPlayer].GetScore(Categories.Twos) + Players[CurrentPlayer].GetScore(Categories.Threes) +
                    Players[CurrentPlayer].GetScore(Categories.Fours) + Players[CurrentPlayer].GetScore(Categories.Fives) + Players[CurrentPlayer].GetScore(Categories.Sixes) > 62)
                {
                    Players[CurrentPlayer].SetScore(Categories.Bonus, GetScore(Categories.Bonus, CurrentDiceValues), CurrentDiceValues);
                }
            }

            // Update the Total Scores
            UpdateTotalScores();

            // Check if the Scorecard is full, if so try to find the next player whose scorecard still has an empty field
            if (Players[(CurrentPlayer + 1 + Players.Count()) % Players.Count()].IsScorecardFull())
            {
                int nextPlayer = -1;
                for (int p = CurrentPlayer + 2; p < (Players.Count() + (CurrentPlayer + 3)); p++)
                {
                    if (!Players[(p + Players.Count()) % Players.Count()].IsScorecardFull())
                    {
                        nextPlayer = (p + Players.Count()) % Players.Count();
                        break;
                    }
                }
                // If a player was found whose scorecard is not yet full, continue to that player
                if (nextPlayer > -1)
                {
                    NextPlayer(nextPlayer);
                }
                // Else end the game
                else
                {
                    EndGame();
                }
            }
            else
            {
                // Move on to the next player
                NextPlayer(CurrentPlayer + 1);
            }

        }


        /// <summary>
        /// Move the game on to the next player, clearing the previous dice-values and updating the scorecard to that of the current player
        /// </summary>
        private void NextPlayer(int player = -1)
        {
            Debug.WriteLine($"Yahtzee: NextPlayer() called for player {player}");

            // Ensure acceptable value for player
            if (player < -1)
            {
                Debug.WriteLine($"Yahtzee: NextPlayer() recieved invalid value for player, player={player}");
                return;
            }
            // If no value was given for player, or the value has reached the end of Players, select the first player and increment the CurrentRound counter
            if (player == -1 || player >= Players.Count()) { player = 0; CurrentRound++; UpdateRoundCounter(); }



            // Set the ToggleButtons, used to allow the holding of a die, to unchecked and not-clickable,
            // since there will be no values to hold until the first roll of the dice
            SetDiceHoldingAllowed(false);
            // Clear any and all dice values
            ClearDiceValues();
            // Clear all score-values on the scorecard
            ClearScorecardOnScreen();
            // Lock all categories on the scorecard
            LockCategories();



            CurrentPlayer = player;

            // Show the information of the current player
            ShowScorecardOnScreen(Players[player]);
            ShowNextPlayerDialog(Players[player].GetPlayerName());

            // Start dice rolling
            CurrentRoll = 0;
            UpdateRollCounter();
            RollDiceButton.IsEnabled = true;

        }


        /// <summary>
        /// End the game, display a message on screen with the player ranked by their final scores
        /// </summary>
        private void EndGame()
        {
            ShowScorecardOnScreen(Players[CurrentPlayer]);
            ShowEndGameDialog();
        }








        /// <summary>
        /// Get the score of the CurrentDiceValues for a specified category.
        /// Does not check if category is actually valid for current dice!
        /// </summary>
        /// <param name="cat"></param>
        /// <returns>The score for the given category</returns>
        private int GetScore(Categories cat, int[] dice = null)
        {
            int score = 0;
            if (dice == null) { dice = CurrentDiceValues; }

            switch (cat)
            {
                // Upper section
                case Categories.Aces:
                case Categories.Twos:
                case Categories.Threes:
                case Categories.Fours:
                case Categories.Fives:
                case Categories.Sixes:
                    {
                        if (dice == null) { Debug.WriteLine($"Yahtzee: Scorecard.CalculateScore() requires an int[] dice in order to calculate a score for {cat}"); return 0; }
                        // !The following requires the categories 1 to 6, Aces to Sixes, to be the first six elements of the enum Categories!
                        int catNumber = (int)cat + 1;
                        foreach (int up in dice)
                        {
                            if (up == catNumber)
                            {
                                score++;
                            }
                        }
                        score = score * catNumber;
                        break;
                    }
                case Categories.Bonus:
                    {
                        score = 35;
                        break;
                    }

                // Lower section
                case Categories.Chance:
                case Categories.ThreeKind:
                case Categories.FourKind:
                    {
                        if (dice == null) { Debug.WriteLine($"Yahtzee: Scorecard.CalculateScore() requires an int[] dice in order to calculate a score for {cat}"); return 0; }
                        foreach (int kind in dice)
                        {
                            score += kind;
                        }
                        break;
                    }
                case Categories.FullHouse:
                    {
                        score = 25;
                        break;
                    }
                case Categories.SmallStraight:
                    {
                        score = 30;
                        break;
                    }
                case Categories.LargeStraight:
                    {
                        score = 40;
                        break;
                    }
                case Categories.Yahtzee:
                case Categories.YahtzeeBonus:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Yahtzee) == -1)
                        {
                            score = 50;
                        }
                        else { score = 100; }
                        
                        break;
                    }
            }
            return score;
        }


        /// <summary>
        /// Calculate the score for a specific category
        /// Checks if a category is actually valid given the current dice
        /// </summary>
        /// <param name="cat">The desired category</param>
        /// <param name="dice">The dice values</param>
        /// <returns>The score for that category</returns>
        private int CalculateScore(Categories cat, int[] dice = null)
        {
            // Return value
            int score = -1;

            // Default to CurrenDiceValues for dice if it was left empty or if dice was of invalid size
            if (dice == null || dice.Length != 5) { dice = CurrentDiceValues; }

            // Get the score
            score = (IsCategoryValid(cat, dice)) ? GetScore(cat) : 0;

            return score;
        }











        /// <summary>
        /// Roll the dice, excluding the dice that have been set to hold
        /// Only rolls the dice, if you want the game to progress to the next roll or turn, use/call NextRoll() instead
        /// </summary>
        private void RollDice()
        {
            // Create a 'proxy' array for the current dice
            int[] dice = new int[CurrentDiceValues.Length];
            Array.Copy(CurrentDiceValues, 0, dice, 0, CurrentDiceValues.Length);

            // Roll new values
            for (int i = 0; i < CurrentDiceValues.Length; i++)
            {
                if (HoldDice[i] == false || CurrentDiceValues[i] == 0)
                {
                    CurrentDiceValues[i] = NewDie();
                }
                Debug.WriteLine($"Yahtzee: RollDice() roll={CurrentRoll} i={i}, HoldDice={HoldDice[i]}, dice={CurrentDiceValues[i]}");
            }

            // Store the new dice to CurrentDiceValues
            // Array.Copy(dice, 0, CurrentDiceValues, 0, CurrentDiceValues.Length);

            // Show the dice on screen
            ShowDice(CurrentDiceValues);
            
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
        /// Analyse the current dice, and find which categories to unlock on the scorecard
        /// </summary>
        private void AnalyseDice(int[] dice)
        {
            // If invalide dice was recieved, return early with an empty list
            if (dice.Length != 5)
            {
                Debug.WriteLine($"Yahtzee: AnalyseDice() recieved invalid number of dice, dice.Length = {dice.Length}");
                return;
            }

            // Array counting the number of occurances of each die-face, the times each number 1 to 6 occurs
            int[] faceCount = new int[6] { 0, 0, 0, 0, 0, 0 };

            // Upper section
            // Count the number of occurances of each number
            foreach (int x in CurrentDiceValues)
            {
                faceCount[x - 1]++;
            }
            UnlockCategory(Categories.Aces, (faceCount[0] > 0) ? GetScore(Categories.Aces) : 0);
            UnlockCategory(Categories.Twos, (faceCount[1] > 0) ? GetScore(Categories.Twos) : 0);
            UnlockCategory(Categories.Threes, (faceCount[2] > 0) ? GetScore(Categories.Threes) : 0);
            UnlockCategory(Categories.Fours, (faceCount[3] > 0) ? GetScore(Categories.Fours) : 0);
            UnlockCategory(Categories.Fives, (faceCount[4] > 0) ? GetScore(Categories.Fives) : 0);
            UnlockCategory(Categories.Sixes, (faceCount[5] > 0) ? GetScore(Categories.Sixes) : 0);

            // Lower section
            // Determine how many times any number occurs
            int sequence = 0;
            int straight = 0;
            bool twoCount = false;
            bool threeCount = false;
            bool fourCount = false;
            bool fiveCount = false;
            foreach (int count in faceCount)
            {
                sequence = (count > 0) ? sequence + 1 : 0;
                if (sequence > straight) { straight = sequence; }
                if (count == 2) { twoCount = true; }
                else if (count == 3) { threeCount = true; }
                else if (count == 4) { fourCount = true; }
                else if (count == 5) { fiveCount = true; }
            }
            UnlockCategory(Categories.ThreeKind, (threeCount) ? GetScore(Categories.ThreeKind) : 0);
            UnlockCategory(Categories.FourKind, (fourCount) ? GetScore(Categories.FourKind) : 0);
            UnlockCategory(Categories.FullHouse, (twoCount && threeCount) ? GetScore(Categories.FullHouse) : 0);
            UnlockCategory(Categories.SmallStraight, (straight >= 4) ? GetScore(Categories.SmallStraight) : 0);
            UnlockCategory(Categories.LargeStraight, (straight >= 5) ? GetScore(Categories.LargeStraight) : 0);
            UnlockCategory(Categories.Yahtzee, (fiveCount) ? GetScore(Categories.Yahtzee) : 0);
            UnlockCategory(Categories.Chance, GetScore(Categories.Chance));

        }
        

        /// <summary>
        /// Get whether a Category is valid for the given dice
        /// </summary>
        /// <param name="cat">The category to validate</param>
        /// <param name="dice">Teh dice values</param>
        /// <returns></returns>
        private bool IsCategoryValid(Categories cat, int[] dice)
        {
            // Return value
            bool isValid = false;

            // If invalide dice was recieved, return early with initial value
            if (dice.Length != 5)
            {
                Debug.WriteLine($"Yahtzee: AnalyseDice() recieved invalid number of dice, dice.Length = {dice.Length}");
                return isValid;
            }

            // Array counting the number of occurances of each die-face, the times each number 1 to 6 occurs
            int[] faceCount = new int[6] { 0, 0, 0, 0, 0, 0 };

            // Count the number of occurances of each number
            foreach (int x in CurrentDiceValues)
            {
                faceCount[x - 1]++;
            }

            int sequence = 0;
            int straight = 0;
            bool twoCount = false;
            bool threeCount = false;
            bool fourCount = false;
            bool fiveCount = false;
            foreach (int count in faceCount)
            {
                sequence = (count > 0) ? sequence + 1 : 0;
                if (sequence > straight) { straight = sequence; }
                Debug.WriteLine($"Yahtzee: AnalyseDice() sequence = {sequence}, straight = {straight}");
                if (count == 2) { twoCount = true; }
                else if (count == 3) { threeCount = true; }
                else if (count == 4) { fourCount = true; }
                else if (count == 5) { fiveCount = true; }
            }

            // Set isValid depending on the Category specified
            switch (cat)
            {
                // Upper Categories
                case Categories.Aces:
                case Categories.Twos:
                case Categories.Threes:
                case Categories.Fours:
                case Categories.Fives:
                case Categories.Sixes:
                    {                        
                        // If at least 1 die of the given number was present, isValid is true
                        isValid = faceCount[(int)cat] > 0;
                        break;
                    }

                // Lower Categories
                case Categories.ThreeKind: { isValid = threeCount; break; }
                case Categories.FourKind: { isValid = fourCount; break; }
                case Categories.FullHouse: { isValid = (twoCount && threeCount); break; }
                case Categories.SmallStraight: { isValid = (straight >= 4); break; }
                case Categories.LargeStraight: { isValid = (straight == 5); break; }
                case Categories.Yahtzee: 
                case Categories.YahtzeeBonus: { isValid = fiveCount; break; }
                case Categories.Chance: { isValid = true; break; }                
            }

            return isValid;
        }


        /// <summary>
        /// Unlock the specified category on the scorecard
        /// </summary>
        /// <param name="cat">The category to unlock</param>
        private void UnlockCategory(Categories cat, int score)
        {
            // Determine the appropriate colour for the border, showing whether this category has a score or if it is 0
            SolidColorBrush borderColour = (score > 0) ? CategoryScoreBorderBrush : CategoryDumpBorderBrush;

            switch (cat)
            {
                // Upper section
                case Categories.Aces:
                    {
                        // No score has yet been entered, unlock the category
                        if (Players[CurrentPlayer].GetScore(Categories.Aces) == -1)
                        {
                            ScorecardResultAcesScoreTextBlock.Text = score.ToString();
                            ScorecardAcesButton.IsHitTestVisible = true;
                            ScorecardAcesButton.BorderBrush = borderColour;
                        }
                        break;
                    }
                case Categories.Twos:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Twos) == -1)
                        {
                            ScorecardResultTwosScoreTextBlock.Text = score.ToString();
                            ScorecardTwosButton.IsHitTestVisible = true;
                            ScorecardTwosButton.BorderBrush = borderColour;
                        }
                            
                        break;
                    }
                case Categories.Threes:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Threes) == -1)
                        {
                            ScorecardResultThreesScoreTextBlock.Text = score.ToString();
                            ScorecardThreesButton.IsHitTestVisible = true;
                            ScorecardThreesButton.BorderBrush = borderColour;
                        }
                            
                        break;
                    }
                case Categories.Fours:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Fours) == -1)
                        {
                            ScorecardResultFoursScoreTextBlock.Text = score.ToString();
                            ScorecardFoursButton.IsHitTestVisible = true;
                            ScorecardFoursButton.BorderBrush = borderColour;
                        }

                            
                        break;
                    }
                case Categories.Fives:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Fives) == -1)
                        {
                            ScorecardResultFivesScoreTextBlock.Text = score.ToString();
                            ScorecardFivesButton.IsHitTestVisible = true;
                            ScorecardFivesButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }
                case Categories.Sixes:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Sixes) == -1)
                        {
                            ScorecardResultSixesScoreTextBlock.Text = score.ToString();
                            ScorecardSixesButton.IsHitTestVisible = true;
                            ScorecardSixesButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }

                // Lower section
                case Categories.ThreeKind:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.ThreeKind) == -1)
                        {
                            ScorecardResultThreeKindScoreTextBlock.Text = score.ToString();
                            ScorecardThreeKindButton.IsHitTestVisible = true;
                            ScorecardThreeKindButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }
                case Categories.FourKind:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.FourKind) == -1)
                        {
                            ScorecardResultFourKindScoreTextBlock.Text = score.ToString();
                            ScorecardFourKindButton.IsHitTestVisible = true;
                            ScorecardFourKindButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }
                case Categories.FullHouse:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.FullHouse) == -1)
                        {
                            ScorecardResultFullHouseScoreTextBlock.Text = score.ToString();
                            ScorecardFullHouseButton.IsHitTestVisible = true;
                            ScorecardFullHouseButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }
                case Categories.SmallStraight:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.SmallStraight) == -1)
                        {
                            ScorecardResultSmallStraightScoreTextBlock.Text = score.ToString();
                            ScorecardSmallStraightButton.IsHitTestVisible = true;
                            ScorecardSmallStraightButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }
                case Categories.LargeStraight:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.LargeStraight) == -1)
                        {
                            ScorecardResultLargeStraightScoreTextBlock.Text = score.ToString();
                            ScorecardLargeStraightButton.IsHitTestVisible = true;
                            ScorecardLargeStraightButton.BorderBrush = borderColour;
                        }
                        
                        break;
                    }
                case Categories.Yahtzee:
                case Categories.YahtzeeBonus:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Yahtzee) == -1)
                        {
                            ScorecardResultYahtzeeScoreTextBlock.Text = score.ToString();
                            ScorecardYahtzeeButton.IsHitTestVisible = true;
                            ScorecardYahtzeeButton.BorderBrush = borderColour;

                        }
                        else if (score != 0)
                        {
                            ScorecardResultYahtzeeBonusScoreTextBlock.Text = "+" + score.ToString();
                            ScorecardYahtzeeBonusButton.IsHitTestVisible = true;
                            ScorecardYahtzeeBonusButton.IsEnabled = true;
                            ScorecardYahtzeeBonusButton.BorderBrush = borderColour;
                        }
                        else
                        {
                            ScorecardYahtzeeBonusButton.IsEnabled = false;
                        }

                        break;
                    }
                case Categories.Chance:
                    {
                        if (Players[CurrentPlayer].GetScore(Categories.Chance) == -1)
                        {
                            ScorecardResultChanceScoreTextBlock.Text = score.ToString();
                            ScorecardChanceButton.IsHitTestVisible = true;
                            ScorecardChanceButton.BorderBrush = borderColour;
                        }

                        break;
                    }
            }

        }


        /// <summary>
        /// Lock all categories on the scorecard
        /// </summary>
        private void LockCategories()
        {
            // Upper section
            ScorecardAcesButton.IsHitTestVisible = false;
            ScorecardAcesButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardTwosButton.IsHitTestVisible = false;
            ScorecardTwosButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardThreesButton.IsHitTestVisible = false;
            ScorecardThreesButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardFoursButton.IsHitTestVisible = false;
            ScorecardFoursButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardFivesButton.IsHitTestVisible = false;
            ScorecardFivesButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardSixesButton.IsHitTestVisible = false;
            ScorecardSixesButton.BorderBrush = CategoryDisabledBorderBrush;
            // Lower section
            ScorecardThreeKindButton.IsHitTestVisible = false;
            ScorecardThreeKindButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardFourKindButton.IsHitTestVisible = false;
            ScorecardFourKindButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardFullHouseButton.IsHitTestVisible = false;
            ScorecardFullHouseButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardSmallStraightButton.IsHitTestVisible = false;
            ScorecardSmallStraightButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardLargeStraightButton.IsHitTestVisible = false;
            ScorecardLargeStraightButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardYahtzeeButton.IsHitTestVisible = false;
            ScorecardYahtzeeButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardChanceButton.IsHitTestVisible = false;
            ScorecardChanceButton.BorderBrush = CategoryDisabledBorderBrush;
            ScorecardYahtzeeBonusButton.IsHitTestVisible = false;
            ScorecardYahtzeeBonusButton.BorderBrush = CategoryDisabledBorderBrush;
        }


        /// <summary>
        /// Toggle hold on a specified die, and set the representation on-screen
        /// </summary>
        /// <param name="die">Which die to toggle hold on, 0-index</param>
        private void HoldDie(int die)
        {
            HoldDice[die] = !HoldDice[die];

            switch (die)
            {
                case 0: { Die1HoldBorder.Opacity = Convert.ToInt32(HoldDice[die]); break; }
                case 1: { Die2HoldBorder.Opacity = Convert.ToInt32(HoldDice[die]); break; }
                case 2: { Die3HoldBorder.Opacity = Convert.ToInt32(HoldDice[die]); break; }
                case 3: { Die4HoldBorder.Opacity = Convert.ToInt32(HoldDice[die]); break; }
                case 4: { Die5HoldBorder.Opacity = Convert.ToInt32(HoldDice[die]); break; }
            }
        }


        /// <summary>
        /// Set if the Buttons for die-holding should be available or not, relevant on the start of a turn where they should not be available
        /// </summary>
        /// <param name="state">The state of availablility to set the Buttons too</param>
        private void SetDiceHoldingAllowed(bool state)
        {
            // Clear all held dice on-screen
            Die1HoldBorder.Opacity = 0;
            Die2HoldBorder.Opacity = 0;
            Die3HoldBorder.Opacity = 0;
            Die4HoldBorder.Opacity = 0;
            Die5HoldBorder.Opacity = 0;

            // Set all holding of dice to false
            Array.Fill(HoldDice, false);

            // Set whether the buttons can are clickable or not, depending on newState
            Die1HoldButton.IsHitTestVisible = state;
            Die2HoldButton.IsHitTestVisible = state;
            Die3HoldButton.IsHitTestVisible = state;
            Die4HoldButton.IsHitTestVisible = state;
            Die5HoldButton.IsHitTestVisible = state;
        }


        private string[,] GetFinalScoresInfo()
        {
            // Return value, will hold information needed for a final scores message
            // n0 = player name, n1 = score
            string[,] scores = new string[Players.Count(), 2];

            // Get all player scores, and find the highest score, this will be used to sort the players by their scores
            List<int> playerScores = new List<int>();
            int highestScore = -1;
            int winner = -1;
            for (int i1 = 0; i1 < Players.Count(); i1++)
            {
                playerScores.Add(Players[i1].GetTotalScore());
                if (playerScores[i1] > highestScore) { highestScore = playerScores[i1]; winner = i1; }
            }

            // Log the winning player to scores, since it is already known
            scores[winner, 0] = Players[winner].GetPlayerName();
            scores[winner, 1] = highestScore.ToString();

            // Remove the winning players score from playerScores
            playerScores.RemoveAt(winner);

            // Get the info for the other players and log it to scoresPile
            int runnerUp = -1;
            for (int i2 = 1; i2 < Players.Count(); i2++)
            {
                runnerUp = 0;
                // Find the next highest score
                for (int j2 = 0; j2 < playerScores.Count(); j2++)
                {
                    if (playerScores[j2] > runnerUp) { runnerUp = j2; }
                }

                // Add the player to scores
                scores[runnerUp, 0] = Players[runnerUp].GetPlayerName();
                scores[runnerUp, 1] = Players[runnerUp].GetTotalScore().ToString();

                // Remove the player from the playerScores list
                playerScores.RemoveAt(runnerUp);
            }

            // TEST
            for (int x = 0; x < scores.GetLength(0); x++)
            {
                Debug.WriteLine($"Yahtzee: GetFinalScoresInfo() scores[{x}] = {scores[x, 0]} {scores[x, 1]}");
            }

            return scores;
        }

        








        /// <summary>
        /// Set the UI representation of the dice to the specified values, also updates roll-counter
        /// </summary>
        /// <param name="dice">int array of the 5 die values</param>
        private void ShowDice(int[] dice)
        {
            if (dice.Length < 5) { Debug.WriteLine(string.Format("Yahtzee: ShowDice() input int[] was too short; length={0}", dice.Length)); return; }

            // If all required image files were found, use them. Else use text numbers.
            if (DiceImageFilesPresent)
            {
                /*
                Die1Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[0]));
                Die2Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[1]));
                Die3Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[2]));
                Die4Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[3]));
                Die5Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[4]));
                */

                // If holdDice is false, ie the toggle button of the die is not checked, change the image
                if (!HoldDice[0])
                {
                    Die1Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[0]));
                }
                if (!HoldDice[1])
                {
                    Die2Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[1]));
                }
                if (!HoldDice[2])
                {
                    Die3Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[2]));
                }
                if (!HoldDice[3])
                {
                    Die4Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[3]));
                }
                if (!HoldDice[4])
                {
                    Die5Image.Source = new BitmapImage(GetDiceFaceImageSource(dice[4]));
                }
            }
            else
            {
                // If holdDice is false, ie the toggle button of the die is not checked, change the text
                if (!HoldDice[0])
                {
                    Die1TextBlock.Text = dice[0].ToString();
                }
                if (!HoldDice[1])
                {
                    Die2TextBlock.Text = dice[1].ToString();
                }
                if (!HoldDice[2])
                {
                    Die3TextBlock.Text = dice[2].ToString();
                }
                if (!HoldDice[3])
                {
                    Die4TextBlock.Text = dice[3].ToString();
                }
                if (!HoldDice[4])
                {
                    Die5TextBlock.Text = dice[4].ToString();
                }
            }

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
        /// Clear all dice values, by setting them to 0
        /// </summary>
        private void ClearDiceValues()
        {
            CurrentDiceValues = new int[5] { 0, 0, 0, 0, 0 };
            ShowDice(CurrentDiceValues);
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
        /// Display the Total Score for each player
        /// </summary>
        private void UpdateTotalScores()
        {
            Player1ScoreTextBlock.Text = (Players.Count() > 0) ? ConvertPlayerScoreToString(Players[0].GetPlayerName(), Players[0].GetIsComputer(), Players[0].GetTotalScore()) : string.Empty;
            Player2ScoreTextBlock.Text = (Players.Count() > 1) ? ConvertPlayerScoreToString(Players[1].GetPlayerName(), Players[1].GetIsComputer(), Players[1].GetTotalScore()) : string.Empty;
            Player3ScoreTextBlock.Text = (Players.Count() > 2) ? ConvertPlayerScoreToString(Players[2].GetPlayerName(), Players[2].GetIsComputer(), Players[2].GetTotalScore()) : string.Empty;
            Player4ScoreTextBlock.Text = (Players.Count() > 3) ? ConvertPlayerScoreToString(Players[3].GetPlayerName(), Players[3].GetIsComputer(), Players[3].GetTotalScore()) : string.Empty;
            Player5ScoreTextBlock.Text = (Players.Count() > 4) ? ConvertPlayerScoreToString(Players[4].GetPlayerName(), Players[4].GetIsComputer(), Players[4].GetTotalScore()) : string.Empty;
            Player6ScoreTextBlock.Text = (Players.Count() > 5) ? ConvertPlayerScoreToString(Players[5].GetPlayerName(), Players[5].GetIsComputer(), Players[5].GetTotalScore()) : string.Empty;
            Player7ScoreTextBlock.Text = (Players.Count() > 6) ? ConvertPlayerScoreToString(Players[6].GetPlayerName(), Players[6].GetIsComputer(), Players[6].GetTotalScore()) : string.Empty;
            Player8ScoreTextBlock.Text = (Players.Count() > 7) ? ConvertPlayerScoreToString(Players[7].GetPlayerName(), Players[7].GetIsComputer(), Players[7].GetTotalScore()) : string.Empty;
        }
        private string ConvertPlayerScoreToString(string name, bool isComputer, int score)
        {
            return "Player " + name + "(" + ((isComputer) ? "C" : "P") + ") : " + score.ToString();
        }

        /// <summary>
        /// Show the spefiifed Scorecard on screen
        /// </summary>
        /// <param name="scorecard">The scorecard to show</param>
        private void ShowScorecardOnScreen(Scorecard scorecard)
        {
            // Owner of the scorecard
            CurrentPlayerTextBlock.Text = "Player: " + scorecard.GetPlayerName();

            // Upper section
            if (scorecard.GetScore(Categories.Aces) != -1)
            {
                ScorecardResultAcesScoreTextBlock.Text = scorecard.GetScoreString(Categories.Aces);
                ScorecardResultAcesHandTextBlock.Text = scorecard.GetDiceString(Categories.Aces);
                ScorecardAcesButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultAcesScoreTextBlock.Text = string.Empty;
                ScorecardResultAcesHandTextBlock.Text = string.Empty;
                ScorecardAcesButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Twos) != -1)
            {
                ScorecardResultTwosScoreTextBlock.Text = scorecard.GetScoreString(Categories.Twos);
                ScorecardResultTwosHandTextBlock.Text = scorecard.GetDiceString(Categories.Twos);
                ScorecardTwosButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultTwosScoreTextBlock.Text = string.Empty;
                ScorecardResultTwosHandTextBlock.Text = string.Empty;
                ScorecardTwosButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Threes) != -1)
            {
                ScorecardResultThreesScoreTextBlock.Text = scorecard.GetScoreString(Categories.Threes);
                ScorecardResultThreesHandTextBlock.Text = scorecard.GetDiceString(Categories.Threes);
                ScorecardThreesButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultThreesScoreTextBlock.Text = string.Empty;
                ScorecardResultThreesHandTextBlock.Text = string.Empty;
                ScorecardThreesButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Fours) != -1)
            {
                ScorecardResultFoursScoreTextBlock.Text = scorecard.GetScoreString(Categories.Fours);
                ScorecardResultFoursHandTextBlock.Text = scorecard.GetDiceString(Categories.Fours);
                ScorecardFoursButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultFoursScoreTextBlock.Text = string.Empty;
                ScorecardResultFoursHandTextBlock.Text = string.Empty;
                ScorecardFoursButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Fives) != -1)
            {
                ScorecardResultFivesScoreTextBlock.Text = scorecard.GetScoreString(Categories.Fives);
                ScorecardResultFivesHandTextBlock.Text = scorecard.GetDiceString(Categories.Fives);
                ScorecardFivesButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultFivesScoreTextBlock.Text = string.Empty;
                ScorecardResultFivesHandTextBlock.Text = string.Empty;
                ScorecardFivesButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Sixes) != -1)
            {
                ScorecardResultSixesScoreTextBlock.Text = scorecard.GetScoreString(Categories.Sixes);
                ScorecardResultSixesHandTextBlock.Text = scorecard.GetDiceString(Categories.Sixes);
                ScorecardSixesButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultSixesScoreTextBlock.Text = string.Empty;
                ScorecardResultSixesHandTextBlock.Text = string.Empty;
                ScorecardSixesButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Bonus) != -1)
            {
                ScorecardResultBonusScoreTextBlock.Text = scorecard.GetScoreString(Categories.Bonus);
            }
            else
            {
                ScorecardResultBonusScoreTextBlock.Text = string.Empty;
            }
            // Lower section
            if (scorecard.GetScore(Categories.ThreeKind) != -1)
            {
                ScorecardResultThreeKindScoreTextBlock.Text = scorecard.GetScoreString(Categories.ThreeKind);
                ScorecardResultThreeKindHandTextBlock.Text = scorecard.GetDiceString(Categories.ThreeKind);
                ScorecardThreeKindButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultThreeKindScoreTextBlock.Text = string.Empty;
                ScorecardResultThreeKindHandTextBlock.Text = string.Empty;
                ScorecardThreeKindButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.FourKind) != -1)
            {
                ScorecardResultFourKindScoreTextBlock.Text = scorecard.GetScoreString(Categories.FourKind);
                ScorecardResultFourKindHandTextBlock.Text = scorecard.GetDiceString(Categories.FourKind);
                ScorecardFourKindButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultFourKindScoreTextBlock.Text = string.Empty;
                ScorecardResultFourKindHandTextBlock.Text = string.Empty;
                ScorecardFourKindButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.FullHouse) != -1)
            {
                ScorecardResultFullHouseScoreTextBlock.Text = scorecard.GetScoreString(Categories.FullHouse);
                ScorecardResultFullHouseHandTextBlock.Text = scorecard.GetDiceString(Categories.FullHouse);
                ScorecardFullHouseButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultFullHouseScoreTextBlock.Text = string.Empty;
                ScorecardResultFullHouseHandTextBlock.Text = string.Empty;
                ScorecardFullHouseButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.SmallStraight) != -1)
            {
                ScorecardResultSmallStraightScoreTextBlock.Text = scorecard.GetScoreString(Categories.SmallStraight);
                ScorecardResultSmallStraightHandTextBlock.Text = scorecard.GetDiceString(Categories.SmallStraight);
                ScorecardSmallStraightButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultSmallStraightScoreTextBlock.Text = string.Empty;
                ScorecardResultSmallStraightHandTextBlock.Text = string.Empty;
                ScorecardSmallStraightButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.LargeStraight) != -1)
            {
                ScorecardResultLargeStraightScoreTextBlock.Text = scorecard.GetScoreString(Categories.LargeStraight);
                ScorecardResultLargeStraightHandTextBlock.Text = scorecard.GetDiceString(Categories.LargeStraight);
                ScorecardLargeStraightButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultLargeStraightScoreTextBlock.Text = string.Empty;
                ScorecardResultLargeStraightHandTextBlock.Text = string.Empty;
                ScorecardLargeStraightButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Yahtzee) != -1)
            {
                ScorecardResultYahtzeeScoreTextBlock.Text = scorecard.GetScoreString(Categories.Yahtzee);
                ScorecardResultYahtzeeHandTextBlock.Text = scorecard.GetDiceString(Categories.Yahtzee);
                ScorecardYahtzeeButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultYahtzeeScoreTextBlock.Text = string.Empty;
                ScorecardResultYahtzeeHandTextBlock.Text = string.Empty;
                ScorecardYahtzeeButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.Chance) != -1)
            {
                ScorecardResultChanceScoreTextBlock.Text = scorecard.GetScoreString(Categories.Chance);
                ScorecardResultChanceHandTextBlock.Text = scorecard.GetDiceString(Categories.Chance);
                ScorecardChanceButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultChanceScoreTextBlock.Text = string.Empty;
                ScorecardResultChanceHandTextBlock.Text = string.Empty;
                ScorecardChanceButton.IsEnabled = true;
            }
            if (scorecard.GetScore(Categories.YahtzeeBonus) != -1)
            {
                ScorecardResultYahtzeeBonusScoreTextBlock.Text = scorecard.GetScoreString(Categories.YahtzeeBonus);
                ScorecardYahtzeeBonusButton.IsEnabled = false;
            }
            else
            {
                ScorecardResultYahtzeeBonusScoreTextBlock.Text = string.Empty;
            }

            ScorecardResultTotalScoreTextBlock.Text = scorecard.GetTotalScore().ToString();
        }

        /// <summary>
        /// Clear all text from the TextBlocks in the score column (result) of the on-screen scorecard
        /// </summary>
        private void ClearScorecardOnScreen()
        {
            // Upper section
            ScorecardResultAcesScoreTextBlock.Text = string.Empty;
            ScorecardResultAcesHandTextBlock.Text = string.Empty;
            ScorecardResultTwosScoreTextBlock.Text = string.Empty;
            ScorecardResultTwosHandTextBlock.Text = string.Empty;
            ScorecardResultThreesScoreTextBlock.Text = string.Empty;
            ScorecardResultThreesHandTextBlock.Text = string.Empty;
            ScorecardResultFoursScoreTextBlock.Text = string.Empty;
            ScorecardResultFoursHandTextBlock.Text = string.Empty;
            ScorecardResultFivesScoreTextBlock.Text = string.Empty;
            ScorecardResultFivesHandTextBlock.Text = string.Empty;
            ScorecardResultSixesScoreTextBlock.Text = string.Empty;
            ScorecardResultSixesHandTextBlock.Text = string.Empty;
            ScorecardResultBonusScoreTextBlock.Text = string.Empty;

            // Lower section
            ScorecardResultThreeKindScoreTextBlock.Text = string.Empty;
            ScorecardResultThreeKindHandTextBlock.Text = string.Empty;
            ScorecardResultFourKindScoreTextBlock.Text = string.Empty;
            ScorecardResultFourKindHandTextBlock.Text = string.Empty;
            ScorecardResultFullHouseScoreTextBlock.Text = string.Empty;
            ScorecardResultFullHouseHandTextBlock.Text = string.Empty;
            ScorecardResultSmallStraightScoreTextBlock.Text = string.Empty;
            ScorecardResultSmallStraightHandTextBlock.Text = string.Empty;
            ScorecardResultLargeStraightScoreTextBlock.Text = string.Empty;
            ScorecardResultLargeStraightHandTextBlock.Text = string.Empty;
            ScorecardResultYahtzeeScoreTextBlock.Text = string.Empty;
            ScorecardResultYahtzeeHandTextBlock.Text = string.Empty;
            ScorecardResultChanceScoreTextBlock.Text = string.Empty;
            ScorecardResultChanceHandTextBlock.Text = string.Empty;
            ScorecardResultYahtzeeBonusScoreTextBlock.Text = string.Empty;
        }



        
        /// <summary>
        /// Show a dialog stating clearly whose turn it is
        /// </summary>
        private async void ShowNextPlayerDialog(string player)
        {
            NextPlayerTextBlock.Text = "Player: " + player;
            await NextPlayerContentDialog.ShowAsync();
        }

        /// <summary>
        /// Show a dialog with all player scores, from highest to lowest
        /// </summary>
        private async void ShowEndGameDialog()
        {
            FinalScores = new List<string>();

            // Get the information needed for the final scores message
            // currently, n0 = player name, n1 = score
            string[,] scores = GetFinalScoresInfo();

            // Build the message displaying the final scores
            for (int i = 0; i < scores.GetLength(0); i++)
            {
                FinalScores.Add(i.ToString() + ". Player " + scores[i, 0] + " -- " + scores[i, 1]);
            }

            // Refresh the displaying ui-element
            EndGameFinalScoresListView.ItemsSource = FinalScores;

            // Display the EndGame dialog, which contains the final scores
            await EndGameContentDialog.ShowAsync();
        }






        private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the dialog to start a new game
            ContentDialogResult result = await NewGameContentDialog.ShowAsync();

            // If the selected button, result, was the one for a new game
            if (result == ContentDialogResult.Primary)
            {
                NewGame((int)TotalPlayerCountNumberBox.Value);
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
            Categories? cat = null;
            try
            {
                cat = (Categories)((Button)sender).Tag;
            }
            catch
            {
                Debug.WriteLine($"Yahtzee: ScorecardButton_Click() could not translate the Button tag into an element of enum Categories.");
            }

            // If a category was determined, enter the score
            if (cat != null)
            {
                // Debug.WriteLine($"Yahtzee: ScorecardButton_Click() found category {(int)cat} {cat}");
                EndTurn((Categories)cat);
            }
        }

        private void DieHoldButton_Click(object sender, RoutedEventArgs e)
        {
            int die = 0;
            int.TryParse(((Button)sender).Tag.ToString(), out die);
            if (die > 0 && die < 6)
            {
                HoldDie(die - 1);
            }
            else
            {
                Debug.WriteLine($"Yahtzee: DieHoldingButton_Click() failed to determine die, tag={((Button)sender).Tag.ToString()}, die={die}");
            }
        }
    }
}
