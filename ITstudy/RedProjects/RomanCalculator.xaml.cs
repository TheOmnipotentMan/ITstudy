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
    /// A calculator that uses roman numerals.
    /// </summary>
    public sealed partial class RomanCalculator : Page
    {

        // enum RomanNumerals { I = 1, V = 5, X = 10, L =50, C = 100, D = 500, M = 1000 }
        Dictionary<int, string> RomanNumerals = new Dictionary<int, string>();
        // enum RomanNumeralsExtended { I = 1, IV = 4, V = 5, IX = 9, X = 10, XL = 40, L = 50, XC = 90, C = 100, CD = 400, D = 500, CM = 900, M = 1000 }
        Dictionary<int, string> RomanNumeralsExtended = new Dictionary<int, string>();

        char[] ValidInput;


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



        /// <summary>
        /// A method to convert any roman-numerals number into its decimal equivalent.
        /// </summary>
        private int ConvertRomanToDecimal(string romanNumber)
        {
            // Value that will be returned if all goes well, -1 if string was invalid
            int decimalNumber = 0;

            // Check the input string for invalid chars
            foreach (char item in romanNumber)
            {
                if (!ValidInput.Contains(item)) { return -romanNumber.IndexOf(item); }
            }


            int i;
            int j = (RomanNumerals.Count - 1);
            int k = 0; // number of numerals actually counted


            /*
            for (i = (RomanNumerals.Count - 1); i >= 0; i--)
            {
                for (j = 0; j < romanNumber.Length; j++)
                {
                    if (RomanNumerals.ElementAt(i).Value == romanNumber[j].ToString())
                    {
                        if (((j + 1) != romanNumber.Length) && (i+2 <= RomanNumerals.Count) && romanNumber[j + 1] == RomanNumerals.ElementAt(i + 1).Value.First() | romanNumber[j + 1] == RomanNumerals.ElementAt(i + 2).Value.First())
                        {
                            if (romanNumber[j + 1] == RomanNumerals.ElementAt((i * 2) + 1).Value.First())
                            {
                                decimalNumber += RomanNumeralsExtended.ElementAt(i + 1).Key;
                            }
                            else if (romanNumber[j + 1] == RomanNumerals.ElementAt(i + 2).Value.First())
                            {
                                decimalNumber += RomanNumeralsExtended.ElementAt((i * 2) + 3).Key;
                            }
                        }
                        else
                        {
                            decimalNumber += RomanNumerals.ElementAt(i).Key;
                        }
                    }
                    else { break; }
                }
            }
            */

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



            if (k < romanNumber.Length)
            {
                return -k;
            }
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

            #region Oldversions
            /*

            VERSION02 logic is off and output is in reverse
            foreach(int item in Enum.GetValues(typeof(RomanNumeralsExtended)))
            {
                if (numberToConvert > item)
                {
                    romanNumber += Enum.GetName(typeof(RomanNumeralsExtended), item);
                    numberToConvert -= item;
                    continue;
                }
                else { break; }
            }



            VERSION01 too bloated
            while(numberToConvert > 0)
            {
                if (numberToConvert > (int)RomanNumerals.M)
                {
                    romanNumber += "M";
                    numberToConvert -= (int)RomanNumerals.M;
                    continue;
                }
                else if (numberToConvert > ((int)RomanNumerals.M - (int)RomanNumerals.C))
                {
                    romanNumber += "CM";
                    numberToConvert -= ((int)RomanNumerals.M - (int)RomanNumerals.C);
                    continue;
                }
                else if (numberToConvert > (int)RomanNumerals.D)
                {
                    romanNumber += "D";
                    numberToConvert -= (int)RomanNumerals.D;
                    continue;
                }
                else if (numberToConvert > ((int)RomanNumerals.D - (int)RomanNumerals.C))
                {
                    romanNumber += "CD";
                    numberToConvert -= ((int)RomanNumerals.D - (int)RomanNumerals.C);
                    continue;
                }
                else if (numberToConvert > (int)RomanNumerals.C)
                {
                    romanNumber += "C";
                    numberToConvert -= (int)RomanNumerals.C;
                    continue;
                }
                else if (numberToConvert > ((int)RomanNumerals.C - (int)RomanNumerals.X))
                {
                    romanNumber += "XC";
                    numberToConvert -= ((int)RomanNumerals.C - (int)RomanNumerals.X);
                    continue;
                }
                else if (numberToConvert > (int)RomanNumerals.L)
                {
                    romanNumber += "L";
                    numberToConvert -= (int)RomanNumerals.L;
                    continue;
                }
                else if (numberToConvert > ((int)RomanNumerals.L - (int)RomanNumerals.X))
                {
                    romanNumber += "XL";
                    numberToConvert -= ((int)RomanNumerals.L - (int)RomanNumerals.X);
                    continue;
                }
            }

            */
            #endregion Oldverions

            return romanNumber;
        }
    }
}
