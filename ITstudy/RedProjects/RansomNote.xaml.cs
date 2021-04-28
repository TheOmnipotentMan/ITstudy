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

// contains ObservableCollection class
using System.Collections.ObjectModel;

// Enables integration of rich media, ie fonts
using Windows.Media;

// used to create a paragraph, to pass to RichTextBlock
using Windows.UI.Xaml.Documents;

// TODO remove if the app is ever finished
using System.Diagnostics;




namespace ITstudy.RedProjects
{
    /// <summary>
    /// Given some text it will assign a font to each individual letter randomly selected from a selection of fonts.
    /// </summary>
    public sealed partial class RansomNote : Page
    {

        // The collection of available fonts on the system
        public List<TextBlock> AvailableFontsListViewItems;
        // The user-selected fonts to user in the scramble
        private List<string> SelectedFonts;
        

        public RansomNote()
        {
            this.InitializeComponent();
            ImportSystemFonts();
        }


        private void ImportSystemFonts()
        {
            // Create clean new list
            AvailableFontsListViewItems = new List<TextBlock>();

            // Get the fonts that are available at default location of system
            string[] systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();

            // For each font, add it to the list
            for (int i = 0; i < systemFonts.Length; i++)
            {
                string item = systemFonts.ElementAt(i);
                AvailableFontsListViewItems.Add(new TextBlock());
                AvailableFontsListViewItems.ElementAt(i).Text = item;
                AvailableFontsListViewItems.ElementAt(i).FontFamily = new FontFamily(item);
            }

            // Set the ItemsSource of FontSelectionListView to AvailableFontsList, displaying all available fonts to user
            // And updating/refreshing the display when calling this method, same can be achieved by using something like ObservableCollection but this way the type of collection doesn't matter as much
            FontSelectionListView.ItemsSource = AvailableFontsListViewItems;
        }



        private void ScrambleFontsButton_Click(object sender, RoutedEventArgs e) { GenerateRansomNote(); }
        private void GenerateRansomNote()
        {
            CreateFontListFromSelected();
            ScrambleTextFonts();
        }



        private void CreateFontListFromSelected()
        {
            // Create clean new list
            SelectedFonts = new List<string>();

            // Make sure at least one font has been selected in the FontSelectionListView
            if (FontSelectionListView.SelectedItems.Count < 1)
            {
                FontSelectionListView.SelectedItem = FontSelectionListView.Items.First();
            }

            // Get the currently selected items in FontSelectionListView
            var selectedItems = FontSelectionListView.SelectedItems.Cast<TextBlock>();

            // Add the text of each item, which represents the font, to the SelectedFonts list, and limit the count of SelectedFonts to maximumNumberOfFonts
            int maximumNumberOfFonts = 20;
            int selectionLength = selectedItems.Count() <= maximumNumberOfFonts ? selectedItems.Count() : maximumNumberOfFonts;
            for (int i = 0; i < selectionLength; i++)
            {
                SelectedFonts.Add(selectedItems.ElementAt(i).Text);
            }
        }



        private void ScrambleTextFonts()
        {
            // Text to scramble
            string text;
            // Max size of text (string.length) to scramble (2000 should be just over one sheet of A4 in terms of chars)
            int maxStringLength = 2000;
            // The output text, ie the scrambled version
            Paragraph scrambledText = new Paragraph();
            // Random number generator to select a random font
            var rand = new Random();
            // The random font to assign to the individual character
            string randomFont = SelectedFonts.First();


            // Get the text to be scrambled, and limit its size
            TextInputRichEditBox.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);
            if (string.IsNullOrWhiteSpace(text)) { LoadPrefabText(); TextInputRichEditBox.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text); }
            if (text.Length > maxStringLength) { text = text.Substring(0, maxStringLength); }


            // Create an array of integers, to act as a weight system for the selected fonts when selecting one at random
            int defaultFontWeight = (int)Math.Ceiling((text.Length / SelectedFonts.Count) * 1.1);
            int[] fontWeights = new int[SelectedFonts.Count];
            int fontWeightsTotal = defaultFontWeight * SelectedFonts.Count;
            for (int i = 0; i < fontWeights.Length; i++)
            {
                fontWeights[i] = defaultFontWeight;
            }

            
            // Pick a font at (weighted) random and assign it to the char c in a new run, and add the run to the scrambledText paragraph
            foreach (char c in text)
            {
                // Generate a random number based on the total weight in the weighting system,
                // then iterate through fontWeights, adding the value of fontWeights at i to selectingCount, until the value selectingCount exceeds that of the random number.
                // This means a font has been selected, with its index in SelectedFonts being i, so it's assigned to randomFont.
                // Finally the value in fontWeights at the current i, and fontWeightsTotal is decreased by one to make the next selection of this location relatively a little less likely
                int randomNumber = rand.Next(0, fontWeightsTotal);
                int selectingCount = 0;
                for (int i = 0; i < fontWeights.Length; i++)
                {
                    selectingCount += fontWeights[i];

                    if (selectingCount >= randomNumber)
                    {
                        if (i > SelectedFonts.Count) { Debug.WriteLine("RansomNote: ScrambleTextFonts weighted random for-loop i-value exceeded SelectedFonts.Count."); break; }
                        randomFont = SelectedFonts.ElementAt(i);
                        fontWeights[i]--;
                        fontWeightsTotal--;
                        break;
                    }
                }

                Run run = new Run();
                run.Text = c.ToString();
                run.FontFamily = new FontFamily(randomFont);
                scrambledText.Inlines.Add(run);
            }

            // Set the text in TextOutputRichTextBlock to the newly scrambled text
            TextOutputRichTextBlock.Blocks.Clear();
            TextOutputRichTextBlock.Blocks.Add(scrambledText);


            // Write all the current fontWeights values to the debug console
            Debug.WriteLine(string.Format("RansomNote: Final fontWeights are;"));
            for (int i = 0; i < fontWeights.Length; i++)
            {
                Debug.WriteLine(string.Format("RansomNote: {0}-{1} = {2}", i.ToString(), SelectedFonts.ElementAt(i), fontWeights[i].ToString()));
            }
        }


        private void LoadPrefabTextButton_Click(object sender, RoutedEventArgs e) { LoadPrefabText(); }
        private void LoadPrefabText()
        {
            string fileToImportPath = "Assets\\LetterFrequencyText\\Lorem Ipsum.txt";
            StreamReader srimport;

            if (!File.Exists(fileToImportPath)) { Debug.WriteLine(string.Format("RansomNote: LoadPrefabText was unable to find a file at path!")); }
            else
            {
                try
                {
                    srimport = new StreamReader(fileToImportPath);
                    TextInputRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, srimport.ReadToEnd());
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(string.Format("RansomNote: LoadPrefabText failed to read the file to the RichEditBox. Exception = {0}", exception.ToString()));
                }
            }
        }
    }
}
