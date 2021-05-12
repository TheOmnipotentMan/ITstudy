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
    /// A calculator that uses roman numerals. Can add, subtract, multiply and divide. Can not handle a negative result of an equation, in that case will display it as positive. Neither can it display decimal places.
    /// </summary>
    public sealed partial class RomanCalculator : Page
    {
        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "16:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "5";
        // Date when this project was finished
        string ProjectDateFinished = "27/03/21";


        // A collection of all individual roman numerals
        Dictionary<int, string> RomanNumerals = new Dictionary<int, string>();
        // An copy+extention of RomanNumerals that also contains all combinations of numerals
        Dictionary<int, string> RomanNumeralsExtended = new Dictionary<int, string>();
        // An array version of RomanNumerals that is used to validate user-input
        char[] ValidInput;


        enum ValidCalculatorInputs { I, V, X, L, C, D, M, Plus, Minus, Multiply, Divide, Equals }
        List<string> CurrentCalculatorInput;



        public RomanCalculator()
        {
            this.InitializeComponent();

            CreateCollections();
            
        }

        private void CreateCollections()
        {
            // Fill the basic Dictionary
            RomanNumerals.Add(1, "I");
            RomanNumerals.Add(5, "V");
            RomanNumerals.Add(10, "X");
            RomanNumerals.Add(50, "L");
            RomanNumerals.Add(100, "C");
            RomanNumerals.Add(500, "D");
            RomanNumerals.Add(1000, "M");

            // Fill the extended Dictionary
            RomanNumeralsExtended.Add(1, "I");
            RomanNumeralsExtended.Add(4, "IV");
            RomanNumeralsExtended.Add(5, "V");
            RomanNumeralsExtended.Add(9, "IX");
            RomanNumeralsExtended.Add(10, "X");
            RomanNumeralsExtended.Add(40, "XL");
            RomanNumeralsExtended.Add(50, "L");
            RomanNumeralsExtended.Add(90, "XC");
            RomanNumeralsExtended.Add(100, "C");
            RomanNumeralsExtended.Add(400, "CD");
            RomanNumeralsExtended.Add(500, "D");
            RomanNumeralsExtended.Add(900, "CM");
            RomanNumeralsExtended.Add(1000, "M");

            // Create an Array of valid roman numerals
            List<char> validInput = new List<char>();
            for(int i = 0; i < RomanNumerals.Count; i++)
            {
                validInput.Add(RomanNumerals.ElementAt(i).Value.First());
            }
            ValidInput = validInput.ToArray();

            // Create a new list for the input from the calculator
            CurrentCalculatorInput = new List<string>();
        }


        // Translate the input given by the user to information UpdateCalculator() can use (ie the equivalent of the input in enum ValidCalculatorInputs).
        private void CalculatorInputButton_Click(object sender, RoutedEventArgs e)
        {
            string caller = (sender as Button).Name;

            // Remove the universal template from the calling buttons Name (if present)
            // Copied from https://stackoverflow.com/questions/2201595/c-sharp-simplest-way-to-remove-first-occurrence-of-a-substring-from-another-st
            string templatePart = "CalculatorButton";
            int index = caller.IndexOf(templatePart);
            caller = (index < 0) ? caller : caller.Remove(index, templatePart.Length);

            switch (caller)
            {
                // Numbers
                case ("I"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.I);
                        break;
                    }
                case ("V"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.V);
                        break;
                    }
                case ("X"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.X);
                        break;
                    }
                case ("L"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.L);
                        break;
                    }
                case ("C"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.C);
                        break;
                    }
                case ("D"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.D);
                        break;
                    }
                case ("M"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.M);
                        break;
                    }

                // Operators
                case ("Plus"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.Plus);
                        break;
                    }
                case ("Minus"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.Minus);
                        break;
                    }
                case ("Multiply"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.Multiply);
                        break;
                    }
                case ("Divide"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.Divide);
                        break;
                    }
                case ("Equals"):
                    {
                        UpdateCalculator(ValidCalculatorInputs.Equals);
                        break;
                    }

                default: { Debug.WriteLine("RomanCalculator: CalculatorInputButton_Click switch encountered an unknown case, {0}", caller); break; }

            }
        }


        // Handles the information the user has entered in the calculator, by adding it as a string to the list CurrentCalculatorInput.
        // It does this depending on how long CurrentCalculatorInput actually is at that moment, and what input was given.
        // Which tells it if it should add to the last string, in the case of the input a numeral right after another,
        // or create a new string for the next component of the user input.
        private void UpdateCalculator(ValidCalculatorInputs input)
        {
            string inputAsString = Enum.GetName(typeof(ValidCalculatorInputs), input);

            int listLength = CurrentCalculatorInput.Count;
            switch (listLength)
            {
                case (0):
                    {
                        if (input < ValidCalculatorInputs.Plus) 
                        {
                            CurrentCalculatorInput.Add(inputAsString);
                        }
                        else
                        {

                        }

                        break;
                    }
                case (1):
                    {
                        if (input < ValidCalculatorInputs.Plus)
                        {
                            CurrentCalculatorInput[0] += inputAsString;
                        }
                        else if (input == ValidCalculatorInputs.Equals)
                        {

                        }
                        else
                        {
                            CurrentCalculatorInput.Add(inputAsString);
                        }

                        break;
                    }
                case (2):
                    {
                        if (input < ValidCalculatorInputs.Plus)
                        {
                            CurrentCalculatorInput.Add(inputAsString);
                        }
                        else if (input == ValidCalculatorInputs.Equals)
                        {
                            CurrentCalculatorInput.RemoveAt(1);
                        }
                        else
                        {
                            CurrentCalculatorInput[1] = inputAsString;
                        }

                        break;
                    }
                case (3):
                    {
                        if (input < ValidCalculatorInputs.Plus)
                        {
                            CurrentCalculatorInput[2] += inputAsString;
                        }
                        else
                        {
                            int result = DoCalculation();
                            CurrentCalculatorInput.Clear();
                            CurrentCalculatorInput.Add(ConvertDecimalToRoman(result));

                            if (input < ValidCalculatorInputs.Equals)
                            {
                                CurrentCalculatorInput.Add(inputAsString);
                            }
                        }

                        break;
                    }
            }

            UpdateCalculatorDisplay();
        }


        // Return the result of the calculation that was specified in the elements of CurrentCalculatorInput
        private int DoCalculation()
        {
            int result = 0;
            int number1 = ConvertRomanToDecimal(CurrentCalculatorInput[0]);
            int number2 = ConvertRomanToDecimal(CurrentCalculatorInput[2]);

            switch (CurrentCalculatorInput[1].ToString())
            {
                case ("Plus"): { return number1 + number2; }
                case ("Minus"): { return number1 - number2; }
                case ("Multiply"): { return number1 * number2; }
                case ("Divide"):
                    {
                        if (number2 == 0) { return 1; }
                        else { return number1 / number2; }
                    }
            }

            return result;
        }


        // Update the display textblock of the calculator UI
        private void UpdateCalculatorDisplay()
        {
            string textToDisplay = string.Empty;
            for (int i = 0; i < CurrentCalculatorInput.Count; i++)
            {
                if (i == 0)
                {
                    textToDisplay = CurrentCalculatorInput[i];
                }
                else if (i == 1)
                {
                    string currentOperator = string.Empty;
                    switch (CurrentCalculatorInput[1])
                    {
                        case ("Plus"): { currentOperator = "+"; break; }
                        case ("Minus"): { currentOperator = "-"; break; }
                        case ("Multiply"): { currentOperator = "*"; break; }
                        case ("Divide"): { currentOperator = "/"; break; }
                    }
                    textToDisplay = textToDisplay + " " + currentOperator;
                }
                else
                {
                    textToDisplay = textToDisplay + " " + CurrentCalculatorInput[i];
                }
            }
            DisplayTextBlock.Text = textToDisplay;
        }


        // A reset for the input of the calculator, voids the input
        private void ResetCurrentCalculator_Click(object sender, RoutedEventArgs e)
        {
            CurrentCalculatorInput = new List<string>();
            UpdateCalculatorDisplay();
        }


        private void TranslateNumberButton_Click(object sender, RoutedEventArgs e)
        {
            NumberTranslatorInputErrorTextBlock.Text = string.Empty;

            string inputString = NumberTranslatorInput.Text;
            int inputDecimalValue;


            if (int.TryParse(inputString, out inputDecimalValue))
            {
                NumberTranslatorOutput.Text = ConvertDecimalToRoman(inputDecimalValue);
            }
            else
            {
                int decimalNumber = ConvertRomanToDecimal(inputString);

                if (decimalNumber < 0)
                {
                    // Display an error message declaring the (first) offending character in the input.
                    NumberTranslatorInputErrorTextBlock.Text = string.Format("Letter at {0} is incorrect.", (Math.Abs(decimalNumber) + 1));
                }
                else if (decimalNumber == 0)
                {
                    // Should never happen, unless you are messing with ConvertRomanToDecimal()
                    NumberTranslatorInputErrorTextBlock.Text = "Something went wrong.";
                }
                else
                {
                    NumberTranslatorOutput.Text = decimalNumber.ToString();
                }
            }
        }


        private int ConvertRomanToDecimal(string romanNumber)
        {
            // Value that will be returned, positive if all goes well, minus if input was invalid (with the value being equal to the offending char), 0 shouldn't happen.
            int decimalNumber = 0;

            // Check the input string for invalid chars
            foreach (char item in romanNumber)
            {
                if (!ValidInput.Contains(item)) { return -romanNumber.IndexOf(item); }
            }


            /* The actual conversion from Roman to Decimal;
             * Starts with an iteration over the input(romanNumber), and for each character it tries to find the matching char in RomanNumerals by iterating over RomanNumerals in reverse (from M to I).
             * When it finds a match, it first checks if the char could be part of a subtraction-combination (IV, XC, etc).
             * 
             * To do this, it first makes sure that the current romanNumber element isn't the last, and that the element of RomanNumerals we are matching with is not one of the last two (M or D) since those don't ever start a subtraction-combination.
             * If both those conditions are met, it checks the next element in romanNumber to see if it is equal to the element in RomanNumerals that is one higher, or two higher.
             * And if one of those comes back true, it increments decimalNumber by the equivalent value of the subtraction-combination, which is stored in RomanNumeralsExtended. (could also use RomanNumerals and a subtraction instead of RomanNumeralsExtended)
             * It then does an additional increment of i, since we did two elements instead of the normal one, and increments k to represent to total number of numerals that have been handled.
             * 
             * If that is not the case and the elements simply match, it increases decimalNumber by the value stored in the Key of the RomanNumerals element. 
             * It then increment k by one since it handled a single numeral.
             * 
             * If the elements don't match, j is decreased which moves the loop to the next element of RomanNumerals
             * 
             * Finally when j reaches the end of RomanNumerals, we return to the romanNumber loop, which moves on to the next element of romanNumber.
             * 
             * The whole circus result in decimalNumber accumulating the values of the numerals in romanNumber, step by step.
             */
            int i;
            int j = (RomanNumerals.Count - 1);
            int k = 0;
            for (i = 0; i < romanNumber.Length; i++)
            {
                while (j >= 0)
                {
                    if (romanNumber[i] == RomanNumerals.ElementAt(j).Value.First())
                    {
                        if (((i + 1) != romanNumber.Length) && (j + 2 < RomanNumerals.Count))
                        {
                            if (romanNumber[i + 1] == RomanNumerals.ElementAt(j + 1).Value.First())
                            {
                                decimalNumber += RomanNumeralsExtended.ElementAt((j * 2) + 1).Key;
                                i++;
                                k += 2;
                                break;
                            }
                            else if (romanNumber[i + 1] == RomanNumerals.ElementAt(j + 2).Value.First())
                            {
                                decimalNumber += RomanNumeralsExtended.ElementAt((j * 2) + 3).Key;
                                i++;
                                k += 2;
                                break;
                            }
                        }

                        decimalNumber += RomanNumerals.ElementAt(j).Key;
                        k++;
                        break;
                    }
                    else
                    {
                        j--;
                        continue;
                    }
                }
            }

            Debug.WriteLine("RomanCalculator: ConvertRomanToDecimal did {0} to {1}", romanNumber, decimalNumber);

            // if k doesn't equal romanNumber.Length, it means the order of numerals in the input was incorrect (e.g. VMC or MMIXX)
            if (k < romanNumber.Length)
            {
                return -k;
            }
            // else everything went well and the correct decimalNumber is returned
            else
            {
                return decimalNumber;
            }
        }


        private string ConvertDecimalToRoman(int decimalNumber)
        {
            string romanNumber = string.Empty;
            int numberToConvert = Math.Abs(decimalNumber);

            for (int i = (RomanNumeralsExtended.Count - 1); i >= 0; i--)
            {
                while (numberToConvert >= RomanNumeralsExtended.ElementAt(i).Key)
                {
                    romanNumber += RomanNumeralsExtended.ElementAt(i).Value;
                    numberToConvert -= RomanNumeralsExtended.ElementAt(i).Key;
                    continue;
                }
            }

            Debug.WriteLine("RomanCalculator: ConvertDecimalToRoman created {0}", romanNumber);

            return romanNumber;
        }

    }
}
