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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CampingSite : Page
    {
        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "08:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "3";
        // Date when this project was finished
        string ProjectDateFinished = "12/05/21";


        // Pricing values
        int PriceSiteBase = 25;
        int PriceSitePeak = 30;
        int PriceSiteMeterMore = 3;
        int PriceSiteMeterLess = 2;
        int PricePerPerson = 5;
        int PricePerDog = 4;
        int PriceParking = 6;

        // Default values of input-fields
        DateTime PeakSeasonStart = new DateTime(2021, 7, 11);   // not a leap year
        DateTime PeakSeasonEnd = new DateTime(2021, 8, 15);
        DateTime DefaultReservationStart = new DateTime(2021, 7, 4);
        DateTime DefaultReservationEnd = new DateTime(2021, 7, 18);
        int DefaultSiteSize = 10;
        int DefaultPersonCount = 2;
        int DefaultDogCount = 0;
        bool DefaultIsVehicleOnSite = false;




        public CampingSite()
        {
            this.InitializeComponent();
            SetDefaultValues();
        }


        // Set the default values of the input-field
        private void SetDefaultValues()
        {
            // Duration of Peak-season
            PeakSeasonStartDatePicker.Date = PeakSeasonStart;
            PeakSeasonEndDatePicker.Date = PeakSeasonEnd;

            // Default length of reservation
            StartDateCalenderDatePicker.Date = DefaultReservationStart;
            EndDateDateCalenderPicker.Date = DefaultReservationEnd;

            // Default reservation details
            SiteSizeNumberBox.Value = DefaultSiteSize;
            PersonCountNumberBox.Value = DefaultPersonCount;
            DogCountNumberBox.Value = DefaultDogCount;
            VehicleOnSiteCheckBox.IsChecked = DefaultIsVehicleOnSite;
        }


        // Calculate the price of the site based on the values entered
        private void CalculatePriceButton_Click(object sender, RoutedEventArgs e) { CalculatePrice(); }
        private void CalculatePrice()
        {
            // Error message to display, empty if no errors found
            string errorMessage = string.Empty;

            // How many whole years the site is booked for
            int yearsBooked = 0;

            // Check the input fields for possible errors, invalid values
            if (DateTime.Compare(PeakSeasonStartDatePicker.Date.Date, PeakSeasonEndDatePicker.Date.Date) >= 0) { PeakSeasonEndDatePicker.Date = new DateTime(PeakSeasonStartDatePicker.Date.Year + 1, PeakSeasonEndDatePicker.Date.Month, PeakSeasonEndDatePicker.Date.Day); }
            if (!StartDateCalenderDatePicker.Date.HasValue) { errorMessage += "No starting-date given. "; StartDateCalenderDatePicker.Date = DefaultReservationStart; }
            if (!EndDateDateCalenderPicker.Date.HasValue) { errorMessage += "No end-date given. "; EndDateDateCalenderPicker.Date = DefaultReservationEnd; }
            if (DateTime.Compare(StartDateCalenderDatePicker.Date.Value.DateTime, EndDateDateCalenderPicker.Date.Value.DateTime) >= 0)
            { 
                errorMessage += "Starting date is after end date. ";
                StartDateCalenderDatePicker.Date = DefaultReservationStart;
                EndDateDateCalenderPicker.Date = DefaultReservationEnd;
            }
            if (EndDateDateCalenderPicker.Date.Value.DateTime > StartDateCalenderDatePicker.Date.Value.DateTime.AddYears(1)) 
            {
                yearsBooked = EndDateDateCalenderPicker.Date.Value.DateTime.Year - StartDateCalenderDatePicker.Date.Value.DateTime.Year;
                EndDateDateCalenderPicker.Date = new DateTime(StartDateCalenderDatePicker.Date.Value.DateTime.Year, EndDateDateCalenderPicker.Date.Value.DateTime.Month, EndDateDateCalenderPicker.Date.Value.DateTime.Month);
            }
            if (SiteSizeNumberBox.Value <= 0) { errorMessage += "Site space given is not valid. "; SiteSizeNumberBox.Value = 10; }
            if (PersonCountNumberBox.Value <= 0) { errorMessage += "Site must have occupants. "; PersonCountNumberBox.Value = DefaultPersonCount; }
            if (DogCountNumberBox.Value < 0) { errorMessage += "We are not responible for missing animals. "; DogCountNumberBox.Value = DefaultDogCount; }

            // Display the resulting errorMessage
            // If no errors were found it should be string.Empty and thereby clear ErrorMessageTextBlock.Text of any previous message
            ErrorMessageTextBlock.Text = errorMessage;




            // Final price of camp-site
            double finalPrice = 0;

            // Price of site for a whole year, used when the duration is longer than a year
            double priceOfYears = 0;

            // Create local variables of Start and EndDate for easier reading
            DateTime startDate = StartDateCalenderDatePicker.Date.Value.Date;
            DateTime endDate = EndDateDateCalenderPicker.Date.Value.Date;
            DateTime startPeak = PeakSeasonStartDatePicker.Date.Date;
            DateTime endPeak = PeakSeasonEndDatePicker.Date.Date;

            // The total number of days the site is booked for reservation, also ensuring a minimum of one day
            int totalDays = (int)Math.Round(endDate.Subtract(startDate).TotalDays);
            if (totalDays <= 0) { totalDays = 1; }

            // The number of days during peak season
            int daysInPeak = 0;

            // Variables of the positions of the Start- and End-Date relative to Peak Season, -1 if it's before peak, 0 if it's during peak and 1 if it's after peak
            int startToPeak = IsDateInPeakSeason(startDate);
            int endToPeak = IsDateInPeakSeason(endDate);
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() date-peak , startToPeak = {0}, endToPeak = {1}.", startToPeak, endToPeak));



            // Switch based on whether the start- and end-date are each before, during or after peak-season
            // Calculating the number of days that are within peak-season
            switch (startToPeak, endToPeak)
            {
                // Both dates are before peak
                case (-1, -1): { break; }

                // Starting date is before peak, end date is in peak
                case (-1, 0):
                    {
                        daysInPeak = (int)Math.Round(endDate.Subtract(new DateTime(endDate.Year, startPeak.Month, startPeak.Day)).TotalDays);
                        break;
                    }

                // Starting date is before peak, end date is after peak
                case (-1, 1):
                    {
                        daysInPeak = (int)Math.Round(endPeak.Subtract(startPeak).TotalDays + 1);
                        break;
                    }

                // Starting date is in peak, end date is before peak (meaning next year)
                case (0, -1):
                    {
                        daysInPeak = (int)Math.Round(new DateTime(startDate.Year, endPeak.Month, endPeak.Day).Subtract(startDate).TotalDays + 1);
                        break;
                    }

                // Both dates are in peak
                case (0, 0):
                    {
                        daysInPeak = totalDays;
                        break;
                    }

                // Starting date is in peak, end date is after peak
                case (0, 1):
                    {
                        daysInPeak = (int)Math.Round(new DateTime(startDate.Year, endPeak.Month, endPeak.Day).Subtract(startDate).TotalDays + 1);
                        break;
                    }

                // Starting date is after peak, end date is before peak
                case (1, -1): { break; }

                // Starting date is after peak, end date is in peak
                case (1, 0):
                    {
                        daysInPeak = (int)Math.Round(endDate.Subtract(new DateTime(endDate.Year, startPeak.Month, startPeak.Day)).TotalDays);
                        break;
                    }

                // Both dates are after peak
                case (1, 1): { break; }

                // If there was no matching case, log the situation to output
                default:
                    {
                        Debug.WriteLine(string.Format("CampingSite: CalculatePrice() dates-in-peak switch did not find a matching case, startToPeak = {0}, endToPeak = {1}.", startToPeak, endToPeak));
                        break;
                    }

            }

            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() date-peak switch found daysInPeak = {0}", daysInPeak));


            // Calculate the price of the site itself, add it to total
            int sitePriceIncrease = (SiteSizeNumberBox.Value > 10) ? PriceSiteMeterMore : PriceSiteMeterLess;
            finalPrice += sitePriceIncrease * Math.Abs(SiteSizeNumberBox.Value - 10) + PriceSiteBase;
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price1 = {0}", finalPrice));

            // Calculate the cost of the people and dogs, add it to total
            finalPrice += PersonCountNumberBox.Value * PricePerPerson + DogCountNumberBox.Value * PricePerDog;
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price2 = {0}", finalPrice));

            // If a vehicle is parked on site, add it to the total
            int vehicleOnSite = ((bool)VehicleOnSiteCheckBox.IsChecked) ? 1 : 0;
            finalPrice += PriceParking * vehicleOnSite;
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price3 = {0}", finalPrice));

            // If the total duration was longer than a year
            if (yearsBooked >= 1)
            {
                // Calculate the cost of renting the site for an entire year
                priceOfYears = finalPrice * 365;
                priceOfYears += (PriceSitePeak - PriceSiteBase) * Math.Round(endPeak.Subtract(startPeak).TotalDays + 1);
                priceOfYears = priceOfYears * yearsBooked;
                Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price of years = {0}", priceOfYears));
            }

            // Multiply the current total by the total number of days
            finalPrice = finalPrice * totalDays;
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price4 = {0}", finalPrice));

            // Calculate the additional price of the site for its days in peak season, add it to total
            finalPrice += (PriceSitePeak - PriceSiteBase) * daysInPeak;
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price5 = {0}", finalPrice));

            // Add the cost for the whole years the site was booked (probably 0)
            finalPrice += priceOfYears;
            Debug.WriteLine(string.Format("CampingSite: CalculatePrice() price6 = {0}", finalPrice));

            // Round the finalCost to two decimal places
            finalPrice = Math.Round(finalPrice, 2);

            // Show the resulting finalPrice on screen
            FinalPriceTextBlock.Text = finalPrice.ToString();
        }


        /// <summary>
        /// Determines if a given date is before, during or after peak season.
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>-1 if date is before peak, 0 if date is during peak, 1 if date is after peak</returns>
        private int IsDateInPeakSeason(DateTime date)
        {
            // Position of given date to peak-season
            int pos = 0;

            pos = date.CompareTo(new DateTime(date.Year, PeakSeasonStartDatePicker.Date.Month, PeakSeasonStartDatePicker.Date.Day));
            if (pos == 1)
            {
                pos = date.CompareTo(new DateTime(date.Year, PeakSeasonEndDatePicker.Date.Month, PeakSeasonEndDatePicker.Date.Day));
                pos = Math.Clamp(pos, 0, 1);
            }

            return pos;
        }

    }

}
