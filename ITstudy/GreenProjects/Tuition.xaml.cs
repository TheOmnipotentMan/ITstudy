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

// used to format number-input from user
using Windows.Globalization.NumberFormatting;

// TODO remove if the app is ever finished
using System.Diagnostics;




namespace ITstudy.GreenProjects
{

    /// <summary>
    /// Stores a Name and a DateOfBirth for a child. Used to calculate tuition.
    /// </summary>
    public class TuitionChild
    {
        // The use of a class bit of overkill since only a collection of DateTime values is really required here, but this way felt to me closer to a real-life situation, where there might be more info than just name and birthday.
        // And it shows a get function in action.

        string Name;
        DateTime DateOfBirth;

        public TuitionChild(string name, DateTime dateOfBirth)
        {
            this.Name = name;
            this.DateOfBirth = dateOfBirth;
        }

        public string GetName() { return Name; }
        public DateTime GetDateOfBirth() { return DateOfBirth; }
    }




    /// <summary>
    /// Calculate elementary-school tuition for a family.
    /// </summary>
    public sealed partial class Tuition : Page
    {
        // General Parameters
        double StandardTuition;
        double MaximumTuition;
        double SingleParentReduction;
        double YoungChildPrice;
        double OlderChildPrice;
        DateTime AgePivot;
        int MaxYoungChildren;
        int MaxOlderChildren;
        bool IsSingleParent;

        // A list of all the children that will be going to school
        List<TuitionChild> TuitionChildren;

        // Values used at startup to give DatePicker in Child-info a default value
        public DateTimeOffset Child1DateTime = new DateTimeOffset(DateTime.Now.AddYears(-6));
        public DateTimeOffset Child2DateTime = new DateTimeOffset(DateTime.Now.AddYears(-10));
        public DateTimeOffset Child3DateTime = new DateTimeOffset(DateTime.Now.AddYears(-14));

        public Tuition()
        {
            this.InitializeComponent();
            SetNumberBoxFormatters();
        }





        // Get all the parameters from the user-input
        private void GetParameters()
        {
            // Get the calculation parameters
            StandardTuition = StandardTuitionNumberBox.Value;
            MaximumTuition = MaximumTuitionNumberBox.Value;
            SingleParentReduction = (SingleParentReductionNumberBox.Value / 100);
            YoungChildPrice = YoungChildPriceNumberBox.Value;
            OlderChildPrice = OlderChildPriceNumberBox.Value;
            AgePivot = DateTime.Now.AddYears(-(int)AgePivotNumberBox.Value);
            MaxYoungChildren = (int)YoungChildMaxNumberBox.Value;
            MaxOlderChildren = (int)OlderChildPriceNumberBox.Value;

            IsSingleParent = (FamilyCompositionRadioButtons.SelectedItem == (RadioButton)SingleParentRadioButton);


            // Create a new list and add all the user-input field to it (AddChildToList filters null input)
            TuitionChildren = new List<TuitionChild>();
            AddChildToList(Child1NameTextBox.Text, Child1BornDateTextBox.Date.DateTime);
            AddChildToList(Child2NameTextBox.Text, Child2BornDateTextBox.Date.DateTime);
            AddChildToList(Child3NameTextBox.Text, Child3BornDateTextBox.Date.DateTime);
            AddChildToList(Child4NameTextBox.Text, Child4BornDateTextBox.Date.DateTime);
            AddChildToList(Child5NameTextBox.Text, Child5BornDateTextBox.Date.DateTime);
            AddChildToList(Child6NameTextBox.Text, Child6BornDateTextBox.Date.DateTime);
            AddChildToList(Child7NameTextBox.Text, Child7BornDateTextBox.Date.DateTime);
            AddChildToList(Child8NameTextBox.Text, Child8BornDateTextBox.Date.DateTime);
            AddChildToList(Child9NameTextBox.Text, Child9BornDateTextBox.Date.DateTime);

        }

        // Add a child to the list, filters out null or empty input
        private void AddChildToList(string name, DateTime dateOfBirth)
        {
            if (!string.IsNullOrWhiteSpace(name) && dateOfBirth != null)
            {
                TuitionChildren.Add(new TuitionChild(name, dateOfBirth));
            }

            // Debug.WriteLine("Tuition: TuitionChildren.Count = {0}", TuitionChildren.Count);
        }



        // Calculate the tuition for the given family-composition
        private void CalculateTuitionButton_Click(object sender, RoutedEventArgs e) { CalculateTuition(); }
        private void CalculateTuition()
        {
            // Get/refresh all the parameters
            GetParameters();

            double tuitionToPay = 0;

            // Add the base-fee
            tuitionToPay += StandardTuition;

            // Count the number of young and older children
            int youngChildren = 0;
            int olderChildren = 0;
            if (TuitionChildren.Any()) 
            {
                foreach (TuitionChild child in TuitionChildren)
                {
                    if (child.GetDateOfBirth() > AgePivot) { youngChildren++; }
                    else { olderChildren++; }
                }
            }

            // Calculate the total cost for all the children and add it to the tuition
            youngChildren = Math.Clamp(youngChildren, 0, MaxYoungChildren);
            olderChildren = Math.Clamp(olderChildren, 0, MaxOlderChildren);
            tuitionToPay += (youngChildren * YoungChildPrice) + (olderChildren * OlderChildPrice);

            // Clamp the tuition to the Maximum allowed tuition
            tuitionToPay = Math.Clamp(tuitionToPay, double.MinValue, MaximumTuition);

            // Apply the single-parent discount if relevant
            if (IsSingleParent) { tuitionToPay = tuitionToPay * (1 - SingleParentReduction); }

            // Display the final tuition cost
            TuitionResultTextBlock.Text = tuitionToPay.ToString();
        }




        // Sets the formatters of NumberBoxes, regulates and corrects user input so it's easier to use
        private void SetNumberBoxFormatters()
        {
            // A basic NumberFormatter, used to regulate currency input by user
            IncrementNumberRounder currencyRounder = new IncrementNumberRounder { Increment = 0.01, RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp };
            DecimalFormatter currencyFormatter = new DecimalFormatter { IntegerDigits = 1, NumberRounder = currencyRounder };

            // A number rounder to create whole numbers, we don't charge tuition for parts of children.
            IncrementNumberRounder wholeNumberRounder = new IncrementNumberRounder { Increment = 1, RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp };
            DecimalFormatter wholeNumberFormatter = new DecimalFormatter { IntegerDigits = 1, FractionDigits = 0, NumberRounder = wholeNumberRounder };


            StandardTuitionNumberBox.NumberFormatter = currencyFormatter;
            MaximumTuitionNumberBox.NumberFormatter = currencyFormatter;

            YoungChildPriceNumberBox.NumberFormatter = currencyFormatter;
            OlderChildPriceNumberBox.NumberFormatter = currencyFormatter;

            SingleParentReductionNumberBox.NumberFormatter = wholeNumberFormatter;

            AgePivotNumberBox.NumberFormatter = wholeNumberFormatter;
            YoungChildMaxNumberBox.NumberFormatter = wholeNumberFormatter;
            OlderChildMaxNumberBox.NumberFormatter = wholeNumberFormatter;

        }
    }
}
