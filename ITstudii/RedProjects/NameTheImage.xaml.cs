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

// used to load images asynchronously
using Windows.UI.Xaml.Media.Imaging;

// TODO remove if the app is ever finished
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238


// Time spent on code = ~2h

namespace ITstudii.RedProjects
{
    // stuck at the moment, haven't figured out xaml DataTemplate yet

    // I would like to create a system that reads the NameTheImage folder and creates a game-category for every folder found therein
    // When a category is selected, read the png files in those folders, so that they can be used in the guessing game
    // This would allow you to create a new folder with its own new png images and use in the game without any alterations

    // At the moment, creating special classes and data templates is rather steep
    // So for now a lot of content will be written directly into code
    // Specifically; the categories and their folder-path


    /// <summary>
    /// A game in which you guess the name that belongs to the image shown.
    /// </summary>

    public sealed partial class NameTheImage : Page
    {
        // paths to folders
        string ApplicationFolder;
        string NameTheImageFolderPath;
        string FlagsFolderPath;
        string OneTwoThreeFolderPath;
        string RandomTestFolderPath;

        // path of user selected category
        string SelectedCategoryFolderPath;

        // stores all relevant image-paths for the selected category/folder
        List<string> CategoryContent;
        // second list is needed for single-run mode, to maintain a reference to the origional content, specifically to get the names of all the items
        List<string> CategoryContentDeducting;

        // general user values
        int NumberOfChoices;
        int NumberOfGuesses;
        bool IsGuessingContinuous;

        // general values
        int MaxNumberOfImagesAllowed = 400;
        int ImageToGuessIndex;
        List<int> RandomChoicesIndex;
        int TotalNumberOfImages;
        int CorrectAnswer;
        int TimesGuessed;
        int TimesAnswerWasCorrect;
        int TimesAnswerWasIncorrect;
        bool IsAnswered;

        // storage default background for button (I couldnt find a general default)
        Brush DefaultBackgroundColor;

        Random Randomiser;




        public NameTheImage()
        {
            this.InitializeComponent();

            SetFolderPaths();
        }




        private void SetFolderPaths()
        {
            // set paths relative, any path that starts with its name is automatically presumed to be located in the CurrentDirectory
            ApplicationFolder = Directory.GetCurrentDirectory();    // available for debug, not used
            NameTheImageFolderPath = "Assets\\NameTheImage";
            FlagsFolderPath = NameTheImageFolderPath + "\\FlagsOfTheWorld";
            OneTwoThreeFolderPath = NameTheImageFolderPath + "\\OneTwoThree";
            RandomTestFolderPath = NameTheImageFolderPath + "\\RandomTest";

            // check if folders actually exist, should be redundant
            if (!Directory.Exists(NameTheImageFolderPath)) { Debug.WriteLine(string.Format("NameTheImage: NameTheImageFolderPath path invalid, current path = {0}", NameTheImageFolderPath)); }
            else
            {
                if (!Directory.Exists(FlagsFolderPath)) { Debug.WriteLine(string.Format("NameTheImage: FlagsFolderPath path invalid, current path = {0}", FlagsFolderPath)); }
                if (!Directory.Exists(OneTwoThreeFolderPath)) { Debug.WriteLine(string.Format("NameTheImage: OneTwoThreeFolderPath path invalid, current path = {0}", OneTwoThreeFolderPath)); }
                if (!Directory.Exists(RandomTestFolderPath)) { Debug.WriteLine(string.Format("NameTheImage: RandomTestFolderPath path invalid, current path = {0}", RandomTestFolderPath)); }
            }
        }


        private void StartGuessingButton_Click(object sender, RoutedEventArgs e) { StartGuessing(); }
        private void StartGuessing()
        {
            // store the default background of a button (can't figure out a way to auto-default)
            if (GuessButton0.Background != (SolidColorBrush)Resources["CorrectAnswerColor"] && GuessButton0.Background != (SolidColorBrush)Resources["IncorrectAnswerColor"])
            {
                DefaultBackgroundColor = GuessButton0.Background;
            }

            // set the path to the folder of the selected category
            if ((ComboBoxItem)CategoryComboBox.SelectedItem == null) { CategoryComboBox.SelectedItem = OneTwoThree; }
            string selectedCategory = ((ComboBoxItem)CategoryComboBox.SelectedItem).Name;
            switch (selectedCategory)
            {
                case "FlagsOfTheWorld"  : { SelectedCategoryFolderPath = FlagsFolderPath; break; }
                case "OneTwoThree"      : { SelectedCategoryFolderPath = OneTwoThreeFolderPath; break; }
                case "RandomTest"       : { SelectedCategoryFolderPath = RandomTestFolderPath; break; } // contains invalid characters, crashes the application
                default: { Debug.WriteLine("NameTheImage: failed to set a path for SelectedCategoryFolderPath, using default!"); SelectedCategoryFolderPath = OneTwoThreeFolderPath; break; }
            }

            // set the number of available choices
            if ((ComboBoxItem)AvailableChoicesComboBox.SelectedItem == null) { AvailableChoicesComboBox.SelectedItem = (ComboBoxItem)AvailableChoicesComboBox.Items.First(); }
            string selectedNumberOfChoices = ((ComboBoxItem)AvailableChoicesComboBox.SelectedItem).Name;
            switch (selectedNumberOfChoices)
            {
                case "Choices1": { NumberOfChoices = 1; break; }
                case "Choices2": { NumberOfChoices = 2; break; }
                case "Choices3": { NumberOfChoices = 3; break; }
                case "Choices4": { NumberOfChoices = 4; break; }
                case "Choices5": { NumberOfChoices = 5; break; }
                case "Choices6": { NumberOfChoices = 6; break; }
                default: { Debug.WriteLine("NameTheImage: failed to set NumberOfChoices, using default!"); NumberOfChoices = 1; break; }
            }

            // set the number of available/allowed guesses
            if ((ComboBoxItem)AvailableGuessesComboBox.SelectedItem == null) { AvailableGuessesComboBox.SelectedItem = (ComboBoxItem)AvailableGuessesComboBox.Items.Last(); }
            string selectedNumberOfGuesses = ((ComboBoxItem)AvailableGuessesComboBox.SelectedItem).Name;
            switch (selectedNumberOfGuesses)
            {
                case "Guesses1": { NumberOfGuesses = 1; break; }
                case "Guesses2": { NumberOfGuesses = 2; break; }
                case "Guesses3": { NumberOfGuesses = 3; break; }
                case "Guesses4": { NumberOfGuesses = 4; break; }
                case "Guesses5": { NumberOfGuesses = 5; break; }
                case "Guesses6": { NumberOfGuesses = 6; break; }
            }

            // activate the desired number of buttons, deactivate the rest
            switch (NumberOfChoices)
            {
                case 1: { GuessButton0.IsEnabled = true; GuessButton1.IsEnabled = false; GuessButton2.IsEnabled = false; GuessButton3.IsEnabled = false; GuessButton4.IsEnabled = false; GuessButton5.IsEnabled = false; break; }
                case 2: { GuessButton0.IsEnabled = true; GuessButton1.IsEnabled = true; GuessButton2.IsEnabled = false; GuessButton3.IsEnabled = false; GuessButton4.IsEnabled = false; GuessButton5.IsEnabled = false; break; }
                case 3: { GuessButton0.IsEnabled = true; GuessButton1.IsEnabled = true; GuessButton2.IsEnabled = true; GuessButton3.IsEnabled = false; GuessButton4.IsEnabled = false; GuessButton5.IsEnabled = false; break; }
                case 4: { GuessButton0.IsEnabled = true; GuessButton1.IsEnabled = true; GuessButton2.IsEnabled = true; GuessButton3.IsEnabled = true; GuessButton4.IsEnabled = false; GuessButton5.IsEnabled = false; break; }
                case 5: { GuessButton0.IsEnabled = true; GuessButton1.IsEnabled = true; GuessButton2.IsEnabled = true; GuessButton3.IsEnabled = true; GuessButton4.IsEnabled = true; GuessButton5.IsEnabled = false; break; }
                case 6: { GuessButton0.IsEnabled = true; GuessButton1.IsEnabled = true; GuessButton2.IsEnabled = true; GuessButton3.IsEnabled = true; GuessButton4.IsEnabled = true; GuessButton5.IsEnabled = true; break; }
            }

            // set the desired mode of operation
            IsGuessingContinuous = GuessModeSwitch.IsOn;

            Randomiser = new Random();

            Debug.WriteLine(string.Format("NameTheImage: StartGuessing with; SelectedCategoryPath({0}), NumberOfChoices({1}), NumberOfGuesses({2})", SelectedCategoryFolderPath, NumberOfChoices, NumberOfGuesses));

            // initialize the selected category
            InitializeNewCategory();
        }


        private void InitializeNewCategory()
        {
            TimesAnswerWasCorrect = 0;
            CorrectGuessCountTextBlock.Text = TimesAnswerWasCorrect.ToString();
            TimesAnswerWasIncorrect = 0;
            IncorrectGuessCountTextBlock.Text = TimesAnswerWasIncorrect.ToString();

            CategoryContent = new List<string>();
            TotalNumberOfImages = Directory.GetFiles(SelectedCategoryFolderPath, "*.png").Length;
            Debug.WriteLine(string.Format("NameTheImage: Number of images found in folder = {0}", TotalNumberOfImages));

            // ensure the number of images is reasonable
            if (TotalNumberOfImages > MaxNumberOfImagesAllowed) { return; }
            if (TotalNumberOfImages < 1) { return; }

            // load all images in folder into a Dictionary
            for (int i = 0; i < TotalNumberOfImages; i++)
            {
                CategoryContent.Add(Directory.GetFiles(SelectedCategoryFolderPath, "*.png").ElementAt(i));
                // Debug.WriteLine(string.Format("NameTheImage: Category Content added, <{0}, {1}>", CategoryContent.ElementAt(i).Key, CategoryContent.ElementAt(i).Value));
                continue;
            }

            if (!IsGuessingContinuous) { CategoryContentDeducting = new List<string>(CategoryContent); }

            Debug.WriteLine("NameTheImage: files in folder have been added to List. {0} out of {1}.", CategoryContent.Count, TotalNumberOfImages);
            ImageCurrentCountTextBlock.Text = TimesAnswerWasCorrect.ToString();
            ImageTotalCountTextBlock.Text = TotalNumberOfImages.ToString();

            // start a new guess
            NewGuess();
        }


        private void NewGuess()
        {
            TimesGuessed = 0;
            IsAnswered = false;
            NextGuessButton.Content = "Skip";

            // reset all (answer)buttons
            GuessButton0.Background = DefaultBackgroundColor; GuessButton0Text.Text = string.Empty;
            GuessButton1.Background = DefaultBackgroundColor; GuessButton1Text.Text = string.Empty;
            GuessButton2.Background = DefaultBackgroundColor; GuessButton2Text.Text = string.Empty;
            GuessButton3.Background = DefaultBackgroundColor; GuessButton3Text.Text = string.Empty;
            GuessButton4.Background = DefaultBackgroundColor; GuessButton4Text.Text = string.Empty;
            GuessButton5.Background = DefaultBackgroundColor; GuessButton5Text.Text = string.Empty;

            if (!IsGuessingContinuous)
            {
                if (CategoryContentDeducting.Count < 1) { return; } // should never happen
                else
                {
                    // Generate index value for image to guess
                    ImageToGuessIndex = Randomiser.Next(0, CategoryContentDeducting.Count);
                }
            }
            else
            {
                // Generate index value for image to guess
                ImageToGuessIndex = Randomiser.Next(0, CategoryContent.Count);
            }

            // generate random index values for the other guesses
            RandomChoicesIndex = new List<int>();
            for (int i = 0; i < NumberOfChoices; i++)
            {
                int newvalue = Randomiser.Next(0, CategoryContent.Count);
                // prevent the dummie answer from being correct
                if (newvalue == ImageToGuessIndex) { i--; continue; }
                RandomChoicesIndex.Add(newvalue);
                continue;
            }

            // determine which button(number) will be the correct one
            CorrectAnswer = Randomiser.Next(0, NumberOfChoices);

            // assign content to the buttons
            int ibutt = 0;
            while (ibutt < NumberOfChoices)
            {
                if (ibutt == CorrectAnswer)
                {
                    if (IsGuessingContinuous)
                    {
                        switch (ibutt)
                        {
                            case 0: { GuessButton0Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(ImageToGuessIndex)); break; }
                            case 1: { GuessButton1Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(ImageToGuessIndex)); break; }
                            case 2: { GuessButton2Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(ImageToGuessIndex)); break; }
                            case 3: { GuessButton3Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(ImageToGuessIndex)); break; }
                            case 4: { GuessButton4Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(ImageToGuessIndex)); break; }
                            case 5: { GuessButton5Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(ImageToGuessIndex)); break; }
                        }
                    }
                    else
                    {
                        switch (ibutt)
                        {
                            case 0: { GuessButton0Text.Text = Path.GetFileNameWithoutExtension(CategoryContentDeducting.ElementAt(ImageToGuessIndex)); break; }
                            case 1: { GuessButton1Text.Text = Path.GetFileNameWithoutExtension(CategoryContentDeducting.ElementAt(ImageToGuessIndex)); break; }
                            case 2: { GuessButton2Text.Text = Path.GetFileNameWithoutExtension(CategoryContentDeducting.ElementAt(ImageToGuessIndex)); break; }
                            case 3: { GuessButton3Text.Text = Path.GetFileNameWithoutExtension(CategoryContentDeducting.ElementAt(ImageToGuessIndex)); break; }
                            case 4: { GuessButton4Text.Text = Path.GetFileNameWithoutExtension(CategoryContentDeducting.ElementAt(ImageToGuessIndex)); break; }
                            case 5: { GuessButton5Text.Text = Path.GetFileNameWithoutExtension(CategoryContentDeducting.ElementAt(ImageToGuessIndex)); break; }
                        }
                    }
                }
                else
                {
                    switch (ibutt)
                    {
                        case 0: { GuessButton0Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(RandomChoicesIndex.ElementAt(ibutt))); break; }
                        case 1: { GuessButton1Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(RandomChoicesIndex.ElementAt(ibutt))); break; }
                        case 2: { GuessButton2Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(RandomChoicesIndex.ElementAt(ibutt))); break; }
                        case 3: { GuessButton3Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(RandomChoicesIndex.ElementAt(ibutt))); break; }
                        case 4: { GuessButton4Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(RandomChoicesIndex.ElementAt(ibutt))); break; }
                        case 5: { GuessButton5Text.Text = Path.GetFileNameWithoutExtension(CategoryContent.ElementAt(RandomChoicesIndex.ElementAt(ibutt))); break; }
                    }
                }
                ibutt++;
                continue;
            }

            // load the new image for the guess
            LoadNewImage();

        }

        private async void LoadNewImage()
        {
            // weird async uri stuff because you're not allowed to just access files even if they're included in the project, it has to go through the StrorageFile system

            string filestring;
            if (IsGuessingContinuous) { filestring = Path.Combine("ms-appx:///", CategoryContent.ElementAt(ImageToGuessIndex)); }
            else { filestring = Path.Combine("ms-appx:///", CategoryContentDeducting.ElementAt(ImageToGuessIndex)); }

            Uri filepath = new Uri(filestring);
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(filepath);

            // Copied from https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.bitmapsource.setsourceasync?view=winrt-19041#examples
            // Ensure the stream is disposed once the image is loaded (TODO figure out, does it ensure or should I ensure)
            using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap
                BitmapImage newImage = new BitmapImage();

                await newImage.SetSourceAsync(fileStream);
                ImageToGuess.Source = newImage;
            }
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageTextBlock.Text = string.Empty;
            ErrorMessageTextBlock.Foreground = (SolidColorBrush)Resources["IncorrectAnswerColor"];
            if (IsAnswered) { ErrorMessageTextBlock.Text = "Please click the 'Next' button to continue."; return; }
            Button answerbutton = sender as Button;
            string answer = answerbutton.Name;
            switch (answer)
            {
                case "GuessButton0": 
                    { 
                        if (CorrectAnswer == 0) { GuessButton0.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; AnswerCorrect(); }
                        else { GuessButton0.Background = (SolidColorBrush)Resources["IncorrectAnswerColor"]; AnswerIncorrect(); }
                        break;
                    }
                case "GuessButton1":
                    {
                        if (CorrectAnswer == 1) { GuessButton1.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; AnswerCorrect(); }
                        else { GuessButton1.Background = (SolidColorBrush)Resources["IncorrectAnswerColor"]; AnswerIncorrect(); }
                        break;
                    }
                case "GuessButton2":
                    {
                        if (CorrectAnswer == 2) { GuessButton2.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; AnswerCorrect(); }
                        else { GuessButton2.Background = (SolidColorBrush)Resources["IncorrectAnswerColor"]; AnswerIncorrect(); }
                        break;
                    }
                case "GuessButton3":
                    {
                        if (CorrectAnswer == 3) { GuessButton3.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; AnswerCorrect(); }
                        else { GuessButton3.Background = (SolidColorBrush)Resources["IncorrectAnswerColor"]; AnswerIncorrect(); }
                        break;
                    }
                case "GuessButton4":
                    {
                        if (CorrectAnswer == 4) { GuessButton4.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; AnswerCorrect(); }
                        else { GuessButton4.Background = (SolidColorBrush)Resources["IncorrectAnswerColor"]; AnswerIncorrect(); }
                        break;
                    }
                case "GuessButton5":
                    {
                        if (CorrectAnswer == 5) { GuessButton5.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; AnswerCorrect(); }
                        else { GuessButton5.Background = (SolidColorBrush)Resources["IncorrectAnswerColor"]; AnswerIncorrect(); }
                        break;
                    }
                default: { ErrorMessageTextBlock.Text = "Could not process guess."; break; }
            }
        }

        private void AnswerCorrect()
        {
            // Debug.WriteLine("NameTheImage: selected answer is correct.");

            IsAnswered = true;
            TimesAnswerWasCorrect++;
            NextGuessButton.Content = "Next";
            CorrectGuessCountTextBlock.Text = TimesAnswerWasCorrect.ToString();

            ErrorMessageTextBlock.Foreground = (SolidColorBrush)Resources["CorrectAnswerColor"];
            ErrorMessageTextBlock.Text = "Correct!";
        }

        private void AnswerIncorrect()
        {
            // Debug.WriteLine("NameTheImage: selected answer is incorrect.");

            IsAnswered = false; // redundant, should already be false

            TimesGuessed++;
            if (TimesGuessed >= NumberOfGuesses) 
            {
                // show the correct answer
                switch(CorrectAnswer)
                {
                    case 0: { GuessButton0.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; break; }
                    case 1: { GuessButton1.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; break; }
                    case 2: { GuessButton2.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; break; }
                    case 3: { GuessButton3.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; break; }
                    case 4: { GuessButton4.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; break; }
                    case 5: { GuessButton5.Background = (SolidColorBrush)Resources["CorrectAnswerColor"]; break; }
                }

                IsAnswered = true;
                TimesAnswerWasIncorrect++;
                NextGuessButton.Content = "Next";
                IncorrectGuessCountTextBlock.Text = TimesAnswerWasIncorrect.ToString();
                ErrorMessageTextBlock.Text = "Better luck next time.";
            }
            else { ErrorMessageTextBlock.Text = "Incorrect, try again."; }
        }

        // Wrap up the current guess and, unless we've finished, start a new guess
        private void NextGuessButton_Click(object sender, RoutedEventArgs e) { NextGuess(); }
        private void NextGuess()
        {
            if (IsAnswered) 
            { 
                ImageCurrentCountTextBlock.Text = TimesAnswerWasCorrect.ToString();
                if (!IsGuessingContinuous)
                {
                    if (CategoryContentDeducting.Count == 1) 
                    {
                        ErrorMessageTextBlock.Foreground = (SolidColorBrush)Resources["CorrectAnswerColor"];
                        ErrorMessageTextBlock.Text = "Congratulations!";
                        LoadFinalImage();
                        return; 
                    }
                    CategoryContentDeducting.RemoveAt(ImageToGuessIndex);
                }
            }
            NewGuess();
        }

        private async void LoadFinalImage()
        {
            // weird async uri stuff because you're not allowed to just access files even if they're included in the project, it has to go through the StrorageFile system
            string filestring = (Directory.GetFiles("Assets", "VeryNiceBorat.png")).First();
            if (!File.Exists(filestring)) { Debug.WriteLine("NameTheImage: missing FinalImage! {0}", filestring); return; }
            Uri filepath = new Uri(Path.Combine("ms-appx:///", filestring));
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(filepath);

            // Copied from https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.bitmapsource.setsourceasync?view=winrt-19041#examples
            // Ensure the stream is disposed once the image is loaded (TODO figure out, does it ensure or should I ensure)
            using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap
                BitmapImage newImage = new BitmapImage();

                await newImage.SetSourceAsync(fileStream);
                ImageToGuess.Source = newImage;
            }
        }


        private void DebugButton_Click(object sender, RoutedEventArgs e) { DebugTestingFuction(); }
        private void DebugTestingFuction()
        {
            // Debug.WriteLine(string.Format("NameTheImage: FlagsFolderPath = {0}", FlagsFolderPath));

            /* -- Dictionary test --
            Random randomiser = new Random();
            int randomchoicetest = randomiser.Next(0, 4);
            string dictionarypath;
            CategoryFolderPaths.TryGetValue(randomchoicetest, out dictionarypath);
            Debug.WriteLine(string.Format("NameTheImage: CategoryFolderPaths dictionary test = <{0}, {1}>", randomchoicetest, dictionarypath));
            */

            LoadFinalImage();
        }

        
    }
}
