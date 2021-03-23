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

using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

// TODO remove if the app is ever finished
using System.Diagnostics;

namespace ITstudy.RedProjects
{

    /// <summary>
    /// Custom class for the counting of a Unicode character, contains a uint Count and an IncrementCount() method.
    /// </summary>
    public class CharCounter
    {
        public uint Count;

        public CharCounter(uint count = 0)
        {
            this.Count = count;
        }

        public uint GetCount() { return Count; }

        public void IncrementCount()
        {
            Count++;
        }
    }


    /*
     * This exercise was to focus on the algorithm, and so I have purposefully avoided the use of collections that would simplify the syntax in certain places, and things like .contains(), .IndexOf() or .Find().
     * As a result relevant methods have their mechanics laid bare.
     */

    /// <summary>
    /// Let's you provide a txt document or string and counts the amount of times any character occurs
    /// </summary>
    public sealed partial class LetterFrequency : Page
    {

        string StringToAnalyse;

        // Pre-defined collections of Chars to count (set to alphabet). This list is manually matched up with the UI, so be aware when editing.
        List<CharCounter> LettersCount;
        char[] LettersChars =
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            };

        // User defined dictionary, can contain any Char/unicode the user wants, using dictionary in case the user leave a field empty
        List<CharCounter> UserDefinedCount;
        List<char> UserDefinedChars;





        public LetterFrequency()
        {
            this.InitializeComponent();
        }







        // Create the pre-defined list of characters to search for
        private void SetPredefinedList()
        {
            LettersCount = new List<CharCounter>();

            foreach (char item in LettersChars)
            {
                LettersCount.Add(new CharCounter());
            }
        }


        // Create the user defined list of characters to search for
        private void SetUserDefinedList()
        {
            UserDefinedCount = new List<CharCounter>();
            UserDefinedChars = new List<char>();


            // Add the character from the input-field to the dictionary, any field left empty will be set to 0 and count that (done to preserve index of lists)
            if (!string.IsNullOrWhiteSpace(UserInput01Char.Text)) { UserDefinedChars.Add(UserInput01Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput02Char.Text)) { UserDefinedChars.Add(UserInput02Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput03Char.Text)) { UserDefinedChars.Add(UserInput03Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput04Char.Text)) { UserDefinedChars.Add(UserInput04Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput05Char.Text)) { UserDefinedChars.Add(UserInput05Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput06Char.Text)) { UserDefinedChars.Add(UserInput06Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput07Char.Text)) { UserDefinedChars.Add(UserInput07Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput08Char.Text)) { UserDefinedChars.Add(UserInput08Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput09Char.Text)) { UserDefinedChars.Add(UserInput09Char.Text.First()); } else { UserDefinedChars.Add('0'); }
            if (!string.IsNullOrWhiteSpace(UserInput10Char.Text)) { UserDefinedChars.Add(UserInput10Char.Text.First()); } else { UserDefinedChars.Add('0'); }

            // Add the user input to the, with an index matching that of UserDefinedChars
            for (int i = 0; i < UserDefinedChars.Count; i++)
            {
                UserDefinedCount.Add(new CharCounter());
            }
        }


        // Import a pre-fabricated text file to the TextBox
        // If you want to be able to read any file on system read this; https://docs.microsoft.com/en-us/windows/uwp/files/file-access-permissions & https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-using-file-and-folder-pickers

        private void ImportFileButton_Click(object sender, RoutedEventArgs e) { ImportFileToTextBox(); }
        private void ImportFileComboBox_NewSelected(object sender, SelectionChangedEventArgs e) { ImportFileToTextBox(); }
        private void ImportFileToTextBox()
        {
            string fileToImportPath;
            StreamReader srimport;

            if (ImportFileComboBox.SelectedItem == null) { ImportFileComboBox.SelectedItem = (ComboBoxItem)ImportFileComboBox.Items.First(); }
            string selectedComboBoxItem = ((ComboBoxItem)ImportFileComboBox.SelectedItem).Name;
            switch (selectedComboBoxItem)
            {
                // To add a new prefab text, include it in the project and add its info here
                case "LorumIpsum": { fileToImportPath = "Assets\\LetterFrequencyText\\Lorem Ipsum.txt"; break; }
                case "LetterSpam": { fileToImportPath = "Assets\\LetterFrequencyText\\LetterSpam.txt"; break; }

                default: { fileToImportPath = "Assets\\LetterFrequencyText\\Lorem Ipsum.txt"; break; }
            }

            if (!File.Exists(fileToImportPath)) { Debug.WriteLine(string.Format("LetterFrequency: ImportFileToTextBox() was unable to find a file at path!")); }
            else
            {
                if (Path.GetExtension(fileToImportPath) == ".txt")
                {
                    try
                    {
                        srimport = new StreamReader(fileToImportPath);
                        PlainTextInput.Text = srimport.ReadToEnd();
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(string.Format("LetterFrequency: ImportFileToTextBox() failed to read the file to TextBox! Exception = {0}", e.ToString()));
                    }
                }
                else { Debug.WriteLine("LetterFrequency: file given in ImportFileToTextBox() has to be a txt file!"); }
            }
        }







        private void StartCountingButton_Click(object sender, RoutedEventArgs e) { StartCounting(); }
        private void StartCounting()
        {

            // set the string to read/count from
            if (PlainTextInput.Text == null || PlainTextInput.Text == string.Empty) { ImportFileToTextBox(); }
            StringToAnalyse = PlainTextInput.Text;

            // Check which output-screen (PivotItem) in active, and call that counting method
            if (OutputDisplayPivot.SelectedItem == (PivotItem)PreDefinedItemsPivotItem)
            {
                CountPreDefinedItems();
            }
            else if (OutputDisplayPivot.SelectedItem == (PivotItem)UserDefinedItemsPivotItem)
            {
                CountUserDefinedItems();
            }
            // The following should be impossible to reach, unless a new PivotItem is added to the OutputDisplayPivot
            // It is meant as a default, for when a new PivotItem hasn't yet been implemented
            else
            {
                OutputDisplayPivot.SelectedItem = (PivotItem)OutputDisplayPivot.Items.First();
                CountPreDefinedItems();
            }
        }


        private void CountPreDefinedItems()
        {
            // Make sure we have a proper setup
            SetPredefinedList();

            // Create arrays from StringToAnalyse and LettersChars, this should speed up iteration
            char[] charsToAnalyse = StringToAnalyse.ToCharArray();
            char[] charsToFind = LettersChars.ToArray();

            // The actual counting of the chars in the string
            // Iterates over the chars in the string, and for every iteration it iterates through the charList of chars we want to count
            // If those two match, we use the charList index to increment the count of that char in LetterList
            int i = 0;
            int j = charsToAnalyse.Length;
            int i1;
            int j1 = charsToFind.Length;

            for (i = 0; i < j; i++)
            {
                // protection while debugging
                if (i >= 10000) { Debug.WriteLine("LetterFrequency: CountPreDefinedItems first for-loop reached its allowed number of iterations!"); return; }

                i1 = 0;
                for (i1 = 0; i1 < j1; i1++)
                {
                    // protection while debugging
                    if (i1 >= 100) { Debug.WriteLine("LetterFrequency: CountPreDefinedItems second for-loop reached its allowed number of iterations!"); return; }

                    if (charsToAnalyse.ElementAt(i) == charsToFind.ElementAt(i1))
                    {
                        LettersCount.ElementAt(i1).IncrementCount();
                    }
                }
            }

            Debug.WriteLine("LetterFrequency: CountPreDifinedItems has finished.");

            // The following code is a manual alignment between the LettersList and the (UI element) Basic Latin output
            CharCountAa.Text = (LettersCount.ElementAt(0).GetCount() + LettersCount.ElementAt(26).GetCount()).ToString();
            CharCountBb.Text = (LettersCount.ElementAt(1).GetCount() + LettersCount.ElementAt(27).GetCount()).ToString();
            CharCountCc.Text = (LettersCount.ElementAt(2).GetCount() + LettersCount.ElementAt(28).GetCount()).ToString();
            CharCountDd.Text = (LettersCount.ElementAt(3).GetCount() + LettersCount.ElementAt(29).GetCount()).ToString();
            CharCountEe.Text = (LettersCount.ElementAt(4).GetCount() + LettersCount.ElementAt(30).GetCount()).ToString();
            CharCountFf.Text = (LettersCount.ElementAt(5).GetCount() + LettersCount.ElementAt(31).GetCount()).ToString();
            CharCountGg.Text = (LettersCount.ElementAt(6).GetCount() + LettersCount.ElementAt(32).GetCount()).ToString();
            CharCountHh.Text = (LettersCount.ElementAt(7).GetCount() + LettersCount.ElementAt(33).GetCount()).ToString();
            CharCountIi.Text = (LettersCount.ElementAt(8).GetCount() + LettersCount.ElementAt(34).GetCount()).ToString();
            CharCountJj.Text = (LettersCount.ElementAt(9).GetCount() + LettersCount.ElementAt(35).GetCount()).ToString();
            CharCountKk.Text = (LettersCount.ElementAt(10).GetCount() + LettersCount.ElementAt(36).GetCount()).ToString();
            CharCountLl.Text = (LettersCount.ElementAt(11).GetCount() + LettersCount.ElementAt(37).GetCount()).ToString();
            CharCountMm.Text = (LettersCount.ElementAt(12).GetCount() + LettersCount.ElementAt(38).GetCount()).ToString();
            CharCountNn.Text = (LettersCount.ElementAt(13).GetCount() + LettersCount.ElementAt(39).GetCount()).ToString();
            CharCountOo.Text = (LettersCount.ElementAt(14).GetCount() + LettersCount.ElementAt(40).GetCount()).ToString();
            CharCountPp.Text = (LettersCount.ElementAt(15).GetCount() + LettersCount.ElementAt(41).GetCount()).ToString();
            CharCountQq.Text = (LettersCount.ElementAt(16).GetCount() + LettersCount.ElementAt(42).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(17).GetCount() + LettersCount.ElementAt(43).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(18).GetCount() + LettersCount.ElementAt(44).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(19).GetCount() + LettersCount.ElementAt(45).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(20).GetCount() + LettersCount.ElementAt(46).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(21).GetCount() + LettersCount.ElementAt(47).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(22).GetCount() + LettersCount.ElementAt(48).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(23).GetCount() + LettersCount.ElementAt(49).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(24).GetCount() + LettersCount.ElementAt(50).GetCount()).ToString();
            CharCountAa.Text = (LettersCount.ElementAt(25).GetCount() + LettersCount.ElementAt(51).GetCount()).ToString();

        }


        private void CountUserDefinedItems()
        {
            // Create the list
            SetUserDefinedList();

            // Create arrays from StringToAnalyse and UserDefinedChars, this should speed up iteration
            char[] charsToAnalyse = StringToAnalyse.ToCharArray();
            char[] charsToFind = UserDefinedChars.ToArray();

            // The actual counting of the chars in the string, different approach from CountPreDefinedItems on purpose, though it makes little difference in execution I think
            // Iterates over the chars in the string, and for every iteration it tries to find that element(char) in UserDefinedChars
            // If it finds it, we use the UserDefinedChars index to increment the count of that char in UserDefinedList
            int i = 0;
            int j = charsToAnalyse.Length;
            int i1;
            int j1 = charsToFind.Length;


            for (i = 0; i < j; i++)
            {
                // protection while debugging
                if (i >= 10000) { Debug.WriteLine("LetterFrequency: CountPreDefinedItems first for-loop reached its allowed number of iterations!"); return; }

                i1 = 0;
                for (i1 = 0; i1 < j1; i1++)
                {
                    // protection while debugging
                    if (i1 >= 100) { Debug.WriteLine("LetterFrequency: CountPreDefinedItems second for-loop reached its allowed number of iterations!"); return; }

                    if (charsToAnalyse.ElementAt(i) == charsToFind.ElementAt(i1))
                    {
                        UserDefinedCount.ElementAt(i1).IncrementCount();
                    }
                }
            }

            // The following code is a manual alignment between the UserDefinedList and the (UI element) Custom output
            CharCountCustom01.Text = UserDefinedCount.ElementAt(0).GetCount().ToString();
            CharCountCustom02.Text = UserDefinedCount.ElementAt(1).GetCount().ToString();
            CharCountCustom03.Text = UserDefinedCount.ElementAt(2).GetCount().ToString();
            CharCountCustom04.Text = UserDefinedCount.ElementAt(3).GetCount().ToString();
            CharCountCustom05.Text = UserDefinedCount.ElementAt(4).GetCount().ToString();
            CharCountCustom06.Text = UserDefinedCount.ElementAt(5).GetCount().ToString();
            CharCountCustom07.Text = UserDefinedCount.ElementAt(6).GetCount().ToString();
            CharCountCustom08.Text = UserDefinedCount.ElementAt(7).GetCount().ToString();
            CharCountCustom09.Text = UserDefinedCount.ElementAt(8).GetCount().ToString();
            CharCountCustom10.Text = UserDefinedCount.ElementAt(9).GetCount().ToString();

        }

    }






}
