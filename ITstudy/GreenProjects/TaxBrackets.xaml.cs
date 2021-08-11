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





namespace ITstudy.GreenProjects
{
    /// <summary>
    /// Calculate the amount of tax to be paid for any income, within a tax-bracket system.
    /// </summary>
    public sealed partial class TaxBrackets : Page
    {

        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "10:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "3";
        // Date when this project was finished
        string ProjectDateFinished = "11/08/21";





        // Default values
        private double TaxFreeSum01 = 419.00;
        private double TaxFreeSum02 = 8799.00;
        private double TaxFreeSum03 = 17179.00;
        private double TaxFreeSum04 = 15503.00;
        private double TaxFreeSum05 = 15503.00;

        private double TaxBracket01Value = 8000.00;
        private double TaxBracket03Value = 54000.00;
        private double TaxBracket02Value = 25000.00;

        private double TaxBracket00Perc = 35.75;
        private double TaxBracket01Perc = 37.05;
        private double TaxBracket02Perc = 50;
        private double TaxBracket03Perc = 60;

        private double TaxFreePercentage = 12;
        private double TaxFreePercMax = 6704.00;




        private int TaxFreeSumSelected = 0;






        public TaxBrackets()
        {
            this.InitializeComponent();

            TaxFreeSum01RadioButton.IsChecked = true;
            TaxFreeSumSelected = 1;

            SetTaxFreeSumTextBlock();
        }




        private void CalculateTax()
        {
            double tax = 0;
            double income = (double.IsNaN(IncomeNumberBox.Value)) ? 0 : Math.Round(IncomeNumberBox.Value, 2);

            // Deduct the Tax Free Sum
            switch (TaxFreeSumSelected)
            {
                case 1: { income -= TaxFreeSum01; break; }
                case 2: { income -= TaxFreeSum02; break; }
                case 3: { income -= TaxFreeSum03; break; }
                case 4: { income -= TaxFreeSum04; break; }
                case 5: { income -= TaxFreeSum05; break; }
                default: { Debug.WriteLine($"TaxBrackets: CalculateTax() could not deduct a Tax Free Sum, TaxFreeSumSelected was not recognised {TaxFreeSumSelected}"); break; }
            }

            // Deduct the Tax Free Percentage
            if (income > 0)
            {
                income -= Math.Clamp(income * (TaxFreePercentage / 100), 0, TaxFreePercMax);
            }

            // Calculate the tax for the brackets
            if (income > TaxBracket03Value)
            {
                tax += (income - TaxBracket03Value) * (TaxBracket03Perc / 100);
                income = TaxBracket03Value;
            }
            if (income > TaxBracket02Value)
            {
                tax += (income - TaxBracket02Value) * (TaxBracket02Perc / 100);
                income = TaxBracket02Value;
            }
            if (income > TaxBracket01Value)
            {
                tax += (income - TaxBracket01Value) * (TaxBracket01Perc / 100);
                income = TaxBracket01Value;
            }
            if (income > 0)
            {
                tax += income * (TaxBracket00Perc / 100);
            }

            /*
            if (income > 0)
            {
                tax += (income % TaxBracket01Value) * (TaxBracket00Perc / 100);
                income -= TaxBracket01Value;
            }
            if (income > 0)
            {
                tax += (income % (TaxBracket02Value - TaxBracket01Value)) * (TaxBracket01Perc / 100);
                income -= TaxBracket02Value - TaxBracket01Value;
            }
            if (income > 0)
            {
                tax += (income % (TaxBracket03Value - TaxBracket02Value)) * (TaxBracket02Perc / 100);
                income -= TaxBracket03Value - TaxBracket02Value;
            }
            if (income > 0)
            {
                tax += income * (TaxBracket03Perc / 100);
            }
            */

            // Display the resulting value on screen
            TaxResultTextBlock.Text = tax.ToString("N2");

        }






        private void SetTaxFreeSumTextBlock(int block = 0, string text = "")
        {
            if (block == 0)
            {
                TaxFreeSum01TextBlock.Text = "€ " + TaxFreeSum01;
                TaxFreeSum02TextBlock.Text = "€ " + TaxFreeSum02;
                TaxFreeSum03TextBlock.Text = "€ " + TaxFreeSum03;
                TaxFreeSum04TextBlock.Text = "€ " + TaxFreeSum04;
                TaxFreeSum05TextBlock.Text = "€ " + TaxFreeSum05;
            }
            else
            {
                if (block < 1 || block > 5) { Debug.WriteLine($"TaxBrackets: SetTaxFreeSumTextBlock() recieved invalid value for block, {block}"); }
                else
                {
                    if (block == 1) { TaxFreeSum01TextBlock.Text = "€ " + text; }
                    else if (block == 2) { TaxFreeSum02TextBlock.Text = "€ " + text; }
                    else if (block == 3) { TaxFreeSum03TextBlock.Text = "€ " + text; }
                    else if (block == 4) { TaxFreeSum04TextBlock.Text = "€ " + text; }
                    else if (block == 5) { TaxFreeSum05TextBlock.Text = "€ " + text; }
                }
            }
        }


        // Copy the newly entered value from the NumberBox to the Textbox
        private void TaxBracket01_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracketTextBlock01.Text = "€ " + sender.Value.ToString("N2");
            TaxBracketTextBlock10.Text = "€ " + sender.Value.ToString("N2");
        }
        private void TaxBracket02_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracketTextBlock11.Text = "€ " + sender.Value.ToString("N2");
            TaxBracketTextBlock20.Text = "€ " + sender.Value.ToString("N2");
        }
        private void TaxBracket03_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracketTextBlock21.Text = "€ " + sender.Value.ToString("N2");
            TaxBracketTextBlock30.Text = "€ " + sender.Value.ToString("N2");
        }
        private void TaxBracket00_PercChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracket00PercTextBlock.Text = sender.Value.ToString("N2") + " %";
        }
        private void TaxBracket01_PercChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracket01PercTextBlock.Text = sender.Value.ToString("N2") + " %";
        }
        private void TaxBracket02_PercChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracket02PercTextBlock.Text = sender.Value.ToString("N2") + " %";
        }
        private void TaxBracket03_PercChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxBracket03PercTextBlock.Text = sender.Value.ToString("N2") + " %";
        }

        private void TaxFreeSum_Checked(object sender, RoutedEventArgs e)
        {
            int buttonSelected = 0;
            if (int.TryParse(((RadioButton)sender).Tag.ToString(), out buttonSelected))
            {
                TaxFreeSumSelected = buttonSelected;

                if (buttonSelected != 1) { TaxFreeSum01RadioButton.IsChecked = false; }
                if (buttonSelected != 2) { TaxFreeSum02RadioButton.IsChecked = false; }
                if (buttonSelected != 3) { TaxFreeSum03RadioButton.IsChecked = false; }
                if (buttonSelected != 4) { TaxFreeSum04RadioButton.IsChecked = false; }
                if (buttonSelected != 5) { TaxFreeSum05RadioButton.IsChecked = false; }
            }
            else
            {
                Debug.WriteLine($"TaxBrackets: TaxFreeSum_Checked() failed to get a number from tag of sender");
            }
        }

        private void TaxFreeSum_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            int tag = 1;

            if (!int.TryParse(sender.Tag.ToString(), out tag))
            {
                Debug.WriteLine($"TaxBrackets: TaxFreeSum_ValueChanged() failed int.TryParse on sender.Tag");
            }

            SetTaxFreeSumTextBlock(tag, sender.Value.ToString("N2"));
        }

        private void TaxFreePercentage_PercChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxFreePercentagePercTextBlock.Text = sender.Value.ToString("N2");
        }

        private void TaxFreePercentage_MaxChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            TaxFreePercentageMaxTextBlock.Text = sender.Value.ToString("N2");
        }

        private void CalculateTaxButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateTax();
        }
    }
}
