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
    /// calculate the price of renting a dump-container.
    /// </summary>
    public sealed partial class DumpContainerRenting : Page
    {
        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "04:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "2";
        // Date when this project was finished
        string ProjectDateFinished = "12/05/21";



        // Pricing values, initialised to default values
        double PricePerM3 = 40d;
        double PriceReturnContainerSmall = 60d;
        double PriceReturnContainerLarge = 125d;
        int CutoffSmallLarge = 2;
        double ReturnCustomerDiscount = 0.15d;
        bool ApplyDiscountOnSecondContainer = true;

        // Input values
        DateTime StartDate;
        DateTime EndDate;
        int ContainerVolume;
        int ContainerCount;
        bool IsReturnCustomer;

        // Default input values
        DateTime DefaultStartDate = new DateTime(2021, 5, 10);
        DateTime DefaultEndDate = new DateTime(2021, 5, 14);
        int DefaultContainerVolume = 12;
        int DefaultContainerCount = 1;
        bool DefaultIsReturnCustomer = false;

        // Error message used if any input value is invalid
        string ErrorMessage = string.Empty;


        public DumpContainerRenting()
        {
            this.InitializeComponent();
            SetInitialValues();
        }


        private void SetInitialValues()
        {
            // Set default/initial prices
            PricePerM3NumberBox.Value = PricePerM3;
            PriceReturnContainerSmallNumberBox.Value = PriceReturnContainerSmall;
            PriceReturnContainerLargeNumberBox.Value = PriceReturnContainerLarge;
            CutoffSmallLargeNumberBox.Value = CutoffSmallLarge;
            ReturnCustomerDiscountNumberBox.Value = ReturnCustomerDiscount * 100;
            ApplyDiscountOnSecondCheckBox.IsChecked = ApplyDiscountOnSecondContainer;

            // Set default/initial input data
            StartDateCalenderDatePicker.Date = DefaultStartDate;
            EndDateCalenderDatePicker.Date = DefaultEndDate;
            ContainerVolumeNumberBox.Value = DefaultContainerVolume;
            ContainerCountNumberBox.Value = DefaultContainerCount;
            IsReturnCustomerCheckBox.IsChecked = DefaultIsReturnCustomer;
        }


        private void CalculatePriceButton_Click(object sender, RoutedEventArgs e) { CalculatePrice(); }
        private void CalculatePrice()
        {
            // Clear an previous error Message(s)
            ErrorMessage = string.Empty;
            ErrorMessageTextBlock.Text = string.Empty;

            // Final price of renting container(s)
            double finalPrice = 0;

            // Get the current input values, return early if something went wrong
            if (!GetPriceListValues() || !GetInputValues())
            {
                ErrorMessageTextBlock.Text = ErrorMessage;
                return;
            }

            // Calculate the total cost of the container(s) for the given input
            finalPrice = PricePerM3 * ContainerVolume * ContainerCount * EndDate.Subtract(StartDate).TotalDays + ContainerCount * ((ContainerVolume > CutoffSmallLarge) ? PriceReturnContainerLarge : PriceReturnContainerSmall);
            if (IsReturnCustomer)
            {
                finalPrice = finalPrice * (1 - ReturnCustomerDiscount);
            }
            else if (ApplyDiscountOnSecondContainer && ContainerCount > 1)
            {
                finalPrice -= (PricePerM3 * ContainerVolume * (ContainerCount - 1) * EndDate.Subtract(StartDate).TotalDays + (ContainerCount - 1) * ((ContainerVolume > CutoffSmallLarge) ? PriceReturnContainerLarge : PriceReturnContainerSmall)) * ReturnCustomerDiscount;
            }
            finalPrice = Math.Round(finalPrice, 2);

            // Display the cost on-screen
            FinalPriceTextBlock.Text = finalPrice.ToString();
        }


        // Get the values from the price-list, checking is all are valid and returning accordingly
        private bool GetPriceListValues()
        {
            // Value to return
            bool allValid = true;

            if (IsValidDouble(PricePerM3NumberBox.Value))
            {
                PricePerM3 = PricePerM3NumberBox.Value;
            }
            else
            {
                ErrorMessage += "Price/m³ is invalid. ";
                allValid = false;
            }

            if (IsValidDouble(PriceReturnContainerSmallNumberBox.Value))
            {
                PriceReturnContainerSmall = PriceReturnContainerSmallNumberBox.Value;
            }
            else
            {
                ErrorMessage += "Return container small price is invalid. ";
                allValid = false;
            }

            if (IsValidDouble(PriceReturnContainerLargeNumberBox.Value))
            {
                PriceReturnContainerLarge = PriceReturnContainerLargeNumberBox.Value;
            }
            else
            {
                ErrorMessage += "Return container large price is invalid. ";
                allValid = false;
            }

            if (IsValidDouble(CutoffSmallLargeNumberBox.Value))
            {
                CutoffSmallLarge = Convert.ToInt32(CutoffSmallLargeNumberBox.Value);
            }
            else
            {
                ErrorMessage += "Cutoff small-large is invalid. ";
                allValid = false;
            }

            if (IsValidDouble(ReturnCustomerDiscountNumberBox.Value) && ReturnCustomerDiscountNumberBox.Value <= 100d)
            {
                ReturnCustomerDiscount = ReturnCustomerDiscountNumberBox.Value / 100;
            }
            else
            {
                ErrorMessage += "Return customer discount is invalid. ";
                allValid = false;
            }

            if (ApplyDiscountOnSecondCheckBox.IsChecked != null)
            {
                ApplyDiscountOnSecondContainer = (ApplyDiscountOnSecondCheckBox.IsChecked == true);
            }
            else
            {
                ErrorMessage += "Apply discount on second container is invalid. ";
                allValid = false;
            }

            return allValid;
        }

        // Get the values from the data input, checking is all are valid and returning accordingly
        private bool GetInputValues()
        {
            // Value to return
            bool allValid = true;

            if (StartDateCalenderDatePicker.Date.HasValue && EndDateCalenderDatePicker.Date.HasValue && EndDateCalenderDatePicker.Date.Value.Date.CompareTo(StartDateCalenderDatePicker.Date.Value.Date) == 1)
            {
                StartDate = StartDateCalenderDatePicker.Date.Value.Date;
                EndDate = EndDateCalenderDatePicker.Date.Value.Date;
            }
            else
            {
                ErrorMessage += "Start and/or end date are invalid. ";
                allValid = false;
            }

            if (IsValidDouble(ContainerVolumeNumberBox.Value))
            {
                ContainerVolume = Convert.ToInt32(ContainerVolumeNumberBox.Value);
            }
            else
            {
                ErrorMessage += "Container volume is invalid. ";
                allValid = false;
            }

            if (IsValidDouble(ContainerCountNumberBox.Value))
            {
                ContainerCount = Convert.ToInt32(ContainerCountNumberBox.Value);
            }
            else
            {
                ErrorMessage += "Container count is invalid. ";
                allValid = false;
            }

            if (IsReturnCustomerCheckBox.IsChecked != null)
            {
                IsReturnCustomer = (IsReturnCustomerCheckBox.IsChecked == true);
            }
            else
            {
                ErrorMessage += "Is customer a returning customer is invalid. ";
                allValid = false;
            }

            return allValid;
        }


        private bool IsValidDouble(double d)
        {
            return !(double.IsNaN(d) || double.IsInfinity(d) || d < 0);
        }
    }
}
