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


// TODO remove if the app is ever finished
using System.Diagnostics;




namespace ITstudy.RedProjects
{
    /// <summary>
    /// Translate text into morse code and back.
    /// </summary>
    public sealed partial class MorseCode : Page
    {
        // Struct that contains translation for a character
        public struct CharTranslation
        {
            // Base/plain text to recognise, ie the main alphabet used
            public string Text { get; }

            // Translations
            public string Morse { get; }
            public string Phonetic { get; }


            /// <summary>
            /// A data-set containing a number of strings/chars that translate into each other.
            /// </summary>
            /// <param name="text">Base characters to recognise, should be primary alphabet.</param>
            /// <param name="morse">Morse code (ITU) translation of text. Will have a white-space added at the end to distinguish between characters.</param>
            /// <param name="phonetic">Phonetic (NATO) translation of text. Will have a white-space added at the end to distinguish between characters.</param>
            public CharTranslation(string text, string morse = null, string phonetic = null)
            {
                this.Text = text;
                this.Morse = morse + " ";
                this.Phonetic = phonetic + " ";
            }

            /// <summary>
            /// Get the character translation by specifying the language. Language specifier is the enum Lang.
            /// </summary>
            /// <param name="lang">The language of the desired variable.</param>
            /// <returns>The string variable of this in the specified language. Null if there is no translation.</returns>
            public string ByLang(Lang lang)
            {
                switch (lang)
                {
                    case Lang.Text: { return Text; }
                    case Lang.Morse: { return Morse; }
                    case Lang.Phonetic: { return Phonetic; }
                    default: { return null; }
                }
            }

        }



        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "16:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "7";
        // Date when this project was finished
        string ProjectDateFinished = "22/05/21";




        // The collection of all character translations
        List<CharTranslation> Dictionary = new List<CharTranslation>();

        // Collection of possible Languages/translations, to be selected from by user
        public enum Lang { Text, Morse, Phonetic };
        public List<string> AvailableTranslations;




        public MorseCode()
        {
            this.InitializeComponent();
            FinishSetup();
        }


        private void FinishSetup()
        {
            // Add all the currently available translations to TextTranslations, copied from enum Translations meaning any derived collection index should match with the enum
            AvailableTranslations = new List<string>();
            foreach (string name in Enum.GetNames(typeof(Lang)))
            {
                AvailableTranslations.Add(name);
            }

            // Fill the Dictionary
            FillDictionary();

            // Fill the input field with generic text, set the language of input to text, and of output to morse
            InputTextBox.Text = File.ReadAllText("Assets\\LetterFrequencyText\\Lorem Ipsum.txt");
            InputLanguageComboBox.SelectedIndex = 0;
            OutputLanguageComboBox.SelectedIndex = 1;

        }


        // Fill the Dictionary with all desired character translations
        private void FillDictionary()
        {
            Dictionary = new List<CharTranslation>();

            // Letters
            Dictionary.Add(new CharTranslation("A", "•-", "Alpha"));
            Dictionary.Add(new CharTranslation("B", "-•••", "Bravo"));
            Dictionary.Add(new CharTranslation("C", "-•-•", "Charlie"));
            Dictionary.Add(new CharTranslation("D", "-••", "Delta"));
            Dictionary.Add(new CharTranslation("E", "•", "Echo"));
            Dictionary.Add(new CharTranslation("F", "••-•", "Foxtrot"));
            Dictionary.Add(new CharTranslation("G", "--•", "Golf"));
            Dictionary.Add(new CharTranslation("H", "••••", "Hotel"));
            Dictionary.Add(new CharTranslation("I", "••", "India"));
            Dictionary.Add(new CharTranslation("J", "•---", "Juliett"));
            Dictionary.Add(new CharTranslation("K", "-•-", "Kilo"));
            Dictionary.Add(new CharTranslation("L", "•-••", "Lima"));
            Dictionary.Add(new CharTranslation("M", "--", "Mike"));
            Dictionary.Add(new CharTranslation("N", "-•", "November"));
            Dictionary.Add(new CharTranslation("O", "---", "Oscar"));
            Dictionary.Add(new CharTranslation("P", "•--•", "Papa"));
            Dictionary.Add(new CharTranslation("Q", "--•-", "Quebec"));
            Dictionary.Add(new CharTranslation("R", "•-•", "Romeo"));
            Dictionary.Add(new CharTranslation("S", "•••", "Sierra"));
            Dictionary.Add(new CharTranslation("T", "-", "Tango"));
            Dictionary.Add(new CharTranslation("U", "••-", "Uniform"));
            Dictionary.Add(new CharTranslation("V", "•••-", "Victor"));
            Dictionary.Add(new CharTranslation("W", "•---", "Whiskey"));
            Dictionary.Add(new CharTranslation("X", "-••-", "Xray"));
            Dictionary.Add(new CharTranslation("Y", "-•--", "Yankee"));
            Dictionary.Add(new CharTranslation("Z", "--••", "Zulu"));

            // Numbers
            Dictionary.Add(new CharTranslation("0", "-----", "Zero"));
            Dictionary.Add(new CharTranslation("1", "•----", "One"));
            Dictionary.Add(new CharTranslation("2", "••---", "Two"));
            Dictionary.Add(new CharTranslation("3", "•••--", "Three"));
            Dictionary.Add(new CharTranslation("4", "••••-", "Four"));
            Dictionary.Add(new CharTranslation("5", "•••••", "Five"));
            Dictionary.Add(new CharTranslation("6", "-••••", "Six"));
            Dictionary.Add(new CharTranslation("7", "--•••", "Seven"));
            Dictionary.Add(new CharTranslation("8", "---••", "Eight"));
            Dictionary.Add(new CharTranslation("9", "----•", "Nine"));


            // Space, must be last to be added, is used to differentiate between characters and words by using different amount of spaces
            Dictionary.Add(new CharTranslation(" ", "   ", "   "));



        }


        private void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure a language was specified for input and output
            if (InputLanguageComboBox.SelectedIndex == -1) { InputLanguageComboBox.SelectedIndex = 0; }
            if (OutputLanguageComboBox.SelectedIndex == -1) { OutputLanguageComboBox.SelectedIndex = 1; }

            // Translate the text from input to output
            OutputTextBlock.Text = Translate(InputTextBox.Text, (Lang)InputLanguageComboBox.SelectedIndex, (Lang)OutputLanguageComboBox.SelectedIndex);
        }

        /// <summary>
        /// Translate a string(input) into the desired language. Build in such a way that it can be made into a public method with minimal effort.
        /// </summary>
        /// <param name="input">The string to translate</param>
        /// <param name="inLang">The language of input</param>
        /// <param name="outLang">The desired language of the returned string</param>
        /// <returns>A string translated into the desired language</returns>
        private string Translate(string input, Lang inLang, Lang outLang)
        {
            string translation = null;

            switch (inLang)
            {
                case Lang.Text:
                    {
                        translation = TranslateFromText(input, outLang);
                        break;
                    }
                case Lang.Morse:
                    {
                        translation = TranslateFromMorse(input, outLang);
                        break;
                    }
                case Lang.Phonetic:
                    {
                        translation = TranslateFromPhonetic(input, outLang);
                        break;
                    }
                default:
                    {
                        Debug.WriteLine(string.Format("MorseCode: Translate() did not recognise the value of inLang; Lang.{0}", inLang));
                        translation = "ERROR";
                        break;
                    }
            }

            return translation;
        }


        // Translate an individual character
        private string TranslateChar(string input, Lang inLang, Lang outLang)
        {
            string translation = null;

            // If input matches the item in Dictionary, get its translation
            foreach (CharTranslation item in Dictionary)
            {
                if (input.Equals(item.ByLang(inLang), StringComparison.OrdinalIgnoreCase)) { translation = item.ByLang(outLang); }
            }

            // If translation is null, meaning no translation was found for input, return input unchanged.
            // Else there is a translation, so return the translation
            return (string.IsNullOrEmpty(translation)) ? input : translation;
        }


        // Is a character in Dictionary (not used)
        private bool IsInDictionary(string input, Lang inLang)
        {
            bool isInDictionary = false;

            foreach (CharTranslation c in Dictionary)
            {
                if (input == c.ByLang(inLang))
                {
                    isInDictionary = true;
                    break;
                }
            }

            return isInDictionary;
        }


        private string TranslateFromText(string input, Lang outLang)
        {
            string translation = string.Empty;

            // Build the translation character by character
            // Feeding each char, as a string, to TranslateChar and adding the return to translation
            for (int i = 0; i < input.Length; i++)
            {
                translation += TranslateChar(input[i].ToString(), Lang.Text, outLang);
            }

            return translation;
        }


        private string TranslateFromMorse(string input, Lang outLang)
        {
            string translation = string.Empty;

            // The list to be fill with the individual morse-characters of input
            List<string> charList = new List<string>();

            // Get the length specified for a space between words (return input unchanged if space was not found at the last element of Dictionary)
            if (Dictionary[Dictionary.Count - 1].Text != " ") { Debug.WriteLine("MorseCode: Last element of Dictionary should be that of space!"); return input; }
            if (Dictionary[Dictionary.Count - 1].Morse.Length < 2) { Debug.WriteLine("MorseCode: The number of spaces between words, that has been specified in Dictionary, is too low to translate Morse. Increase the number of spaces."); return input; }

            // Seperate the individual morse-characters, by finding the next occuring whitespace, which indicates the end of a character, or the start of a space between words.
            int startMorse = 0;
            for (int iMorse = 0; iMorse < input.Length; iMorse++)
            {
                // If the character is a morse-sign, continue to the next iteration
                if (input[iMorse] == '-' || input[iMorse] == '•')
                {
                    continue;
                }
                // Else if the character is a space, it can mean it is the end of a morse character and/or a (longer) space between words
                else if (input[iMorse] == ' ')
                {
                    // If the previous character was a morse-sign, a morse-character was found
                    if (iMorse > 0 && (input[iMorse - 1] == '-' || input[iMorse - 1] == '•'))
                    {
                        charList.Add(input.Substring(startMorse, (iMorse - startMorse) + 1));
                    }

                    // Else if the second char is also space, it is a space between words, add it to list
                    else if (iMorse + 1 < input.Length && input[iMorse + 1] == ' ')
                    {
                        // Find the next character that is not a space, whilest counting the number of spaces
                        int spaceLength = 2;
                        for (spaceLength = 2; spaceLength + iMorse < input.Length; spaceLength++)
                        {
                            if (input[iMorse + spaceLength] != ' ') { break; }
                        }
                        charList.Add(input.Substring(iMorse, spaceLength));
                        iMorse += spaceLength - 1;
                    }
                    startMorse = iMorse + 1;
                }
                // Else the char is not a recognised character (probably punctuation), add it to list by itself
                else
                {
                    charList.Add(input.Substring(iMorse, 1));
                    startMorse = iMorse + 1;
                }
            }

            // Get the translation for each element, ie each morse character, in charList and add it to translation
            foreach (string s in charList)
            {
                translation += TranslateChar(s, Lang.Morse, outLang);
            }

            return translation;
        }


        private string TranslateFromPhonetic(string input, Lang outLang)
        {
            string translation = string.Empty;

            // The list to be fill with the individual phonetic-characters of input
            List<string> charList = new List<string>();

            // Get the length specified for a space between words (return input unchanged if space was not found at the last element of Dictionary)
            if (Dictionary[Dictionary.Count - 1].Text != " ") { Debug.WriteLine("MorseCode: Last element of Dictionary should be that of space, but it is not!"); return input; }
            if (Dictionary[Dictionary.Count - 1].Phonetic.Length < 2) { Debug.WriteLine("MorseCode: Number of spaces between words to low to translate Phonetic. Increase the number of spaces."); return input; }

            // Seperate the individual Phonetic characters, by finding the next occuring whitespace, which indicates the end of a character, of the start of a space between words.
            int startPhon = 0;
            for (int iPhon = 0; iPhon < input.Length; iPhon++)
            {
                // If the character is a morse-sign, continue to the next iteration
                if (Char.IsLetter(input[iPhon]))
                {
                    continue;
                }
                // Else if it is a space, it is the end of a character and/or the start of a space between words
                else if (input[iPhon] == ' ')
                {
                    // If the previous element was a letter, a character was found
                    if (iPhon > 0 && Char.IsLetter(input[iPhon - 1]))
                    {
                        charList.Add(input.Substring(startPhon, (iPhon - startPhon) + 1));
                    }

                    // Else if the second char is also space, it is a space between words, add it to list as such
                    else if (iPhon + 1 < input.Length && input[iPhon + 1] == ' ')
                    {
                        // Find the next character that is not a space, counting the number of spaces
                        int spaceLength = 2;
                        for (spaceLength = 2; spaceLength + iPhon < input.Length; spaceLength++)
                        {
                            if (input[iPhon + spaceLength] != ' ') { break; }
                        }
                        charList.Add(input.Substring(iPhon, spaceLength));
                        iPhon += spaceLength - 1;
                    }
                    startPhon = iPhon + 1;
                }
                // Else the character is not a recognised character (probably punctuation), add it to list by itself
                else
                {
                    charList.Add(input.Substring(iPhon, 1));
                    startPhon = iPhon + 1;
                }
            }

            // Get the translation for each element, ie each phonetic character, in charList and add it to translation
            foreach (string s in charList)
            {
                translation += TranslateChar(s, Lang.Phonetic, outLang);
            }

            return translation;
        }

    }

}
