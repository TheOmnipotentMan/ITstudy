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


namespace ITstudii.GreenProjects
{

    /// <summary>
    /// Calculate the costs of a given taxi-ride.
    /// </summary>
    public sealed partial class TaxiService : Page
    {

        // ---- Taxi Service Parameters ----

        DateTime TaxiStartDateTime;
        DayOfWeek TimeStartDayOfTheWeek;

        TimeSpan RideDuration;

        TimeSpan DayPriceStart;
        TimeSpan NightPriceStart;

        TimeSpan TimeDrivenDay;
        TimeSpan TimeDrivenNight;

        // something like uint would require some casting around
        double KilometersDriven;

        double PricePerKilometer;
        double DayPrice;
        double NightPrice;
        double PremiumRate;

        bool IsPremiumRate;

        double TaxiRidePrice;

        // ---------------------------------



        public TaxiService()
        {
            this.InitializeComponent();

            SetNumberBoxFormats();
        }




        private void CalculateRidePrice()
        {
            TaxiRidePrice = 0;
            TimeDrivenDay = new TimeSpan();
            TimeDrivenNight = new TimeSpan();

            GetRideParameters();

            // Check if PremiumRate should be added, true when ride starts between Fri 22:00 and Mon 07:00 
            if ((TaxiStartDateTime.DayOfWeek == DayOfWeek.Friday) && (TaxiStartDateTime.Hour >= 22)) { IsPremiumRate = true; }
            else if ((TaxiStartDateTime.DayOfWeek == DayOfWeek.Saturday) || (TaxiStartDateTime.DayOfWeek == DayOfWeek.Sunday)) { IsPremiumRate = true; }
            else if ((TaxiStartDateTime.DayOfWeek == DayOfWeek.Monday) && (TaxiStartDateTime.Hour < 7)) { IsPremiumRate = true; }
            else { IsPremiumRate = false; }

            // Create two TimeSpans, adjusted for the starting time, one for the RideDuration and one that will count up until it reaches that adjusted RideDuration
            TimeSpan RideDurationAdjusted = new TimeSpan();
            TimeSpan rideDurationAdjusted = new TimeSpan();
            RideDurationAdjusted = RideDuration + TaxiStartDateTime.TimeOfDay;
            rideDurationAdjusted = TaxiStartDateTime.TimeOfDay;

            // Iteration (time) step used in while loop
            TimeSpan IterationTimeStep = new TimeSpan(0, 1, 0);

            // Counter to limit the max loops of while, should be redundant
            int i = 0;

            // Iterate over, the adjusted values of, rideDuration until it reaches RideDuration
            while (rideDurationAdjusted < RideDurationAdjusted)
            {
                // if the loop runs more times then 2 days, break
                i++; if (i >= 2880) { break; }
                Debug.WriteLine(string.Format("TaxiService: r={0}, R={1}.", rideDurationAdjusted, RideDurationAdjusted));
                Debug.WriteLine(string.Format("TaxiService: While-loop={0} TimeDrivenDay={1}, TimeDrivenNight={2}.", i, TimeDrivenDay, TimeDrivenNight));

                if (rideDurationAdjusted.Hours < DayPriceStart.Hours)
                {
                    TimeDrivenNight += IterationTimeStep;
                    rideDurationAdjusted += IterationTimeStep;
                    continue;
                }

                else if (rideDurationAdjusted.Hours < NightPriceStart.Hours)
                {
                    TimeDrivenDay += IterationTimeStep;
                    rideDurationAdjusted += IterationTimeStep;
                    continue;
                }

                else if (rideDurationAdjusted.Hours >= NightPriceStart.Hours)
                {
                    TimeDrivenNight += IterationTimeStep;
                    rideDurationAdjusted += IterationTimeStep;
                    continue;
                }

                else
                {
                    Debug.WriteLine("TaxiService: CalculateRidePrice screwed up!");
                    Debug.WriteLine(string.Format("TaxiService: r={0}, R={1}.", rideDurationAdjusted, RideDurationAdjusted));
                    Debug.WriteLine(string.Format("TaxiService: While-loop={0} TimeDrivenDay={1}, TimeDrivenNight={2}.", i, TimeDrivenDay, TimeDrivenNight));
                    break;
                }
            }
            

            // would have been nice to just use PremiumRate*IsPremiumRate, but you can't have everything
            int IsPremiumRateInt = Convert.ToInt32(IsPremiumRate);

            // Calculate price for distance
            double TotalCostDistance = KilometersDriven * PricePerKilometer;
            // Calculate price for time driven during the day
            double TotalCostTimeDay = TimeDrivenDay.TotalMinutes * DayPrice;
            // Calculate price for time driven during the day
            double TotalCostTimeNight = TimeDrivenNight.TotalMinutes * NightPrice;

            // Final calculation, add everything together and apply premium
            TaxiRidePrice = ((100 + (PremiumRate * IsPremiumRateInt))/100) * (TotalCostDistance + TotalCostTimeDay + TotalCostTimeNight);

            PriceOutput.Text = TaxiRidePrice.ToString();

            Debug.WriteLine(string.Format("TaxiService: TotalCostDistance={0}, TotalCostTimeDay={1}, TotalCostTimeNight={2}.", TotalCostDistance, TotalCostTimeDay, TotalCostTimeNight));

        }


        private void GetRideParameters()
        {
            // These could also be exposed to the user for more input control
            DayPriceStart = new TimeSpan(8, 0, 0);
            NightPriceStart = new TimeSpan(18, 0, 0);

            // Set pricing parameters
            PricePerKilometer = PricePerKmInput.Value;
            DayPrice = PricePerMinuteDayInput.Value;
            NightPrice = PricePerMinuteNightInput.Value;
            PremiumRate = WeekendPremiumInput.Value;

            // In case of Marty McTimelord
            if (RideDurationInput.Time < TimeSpan.Zero) { DurationInputError.Text = "delorean or tardis?"; }
            else { DurationInputError.Text = ""; }
            RideDuration = RideDurationInput.Time.Duration();

            // TODO find out why a switch(i){case 0:} structure didn't work here, can it not take classes as evaluation input?
            if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputMonday) { TimeStartDayOfTheWeek = DayOfWeek.Monday; }
            else if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputTuesday) { TimeStartDayOfTheWeek = DayOfWeek.Tuesday; }
            else if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputWednesday) { TimeStartDayOfTheWeek = DayOfWeek.Wednesday; }
            else if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputThursday) { TimeStartDayOfTheWeek = DayOfWeek.Thursday; }
            else if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputFriday) { TimeStartDayOfTheWeek = DayOfWeek.Friday; }
            else if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputSaturday) { TimeStartDayOfTheWeek = DayOfWeek.Saturday; }
            else if ((ComboBoxItem)TimeStartDayInput.SelectedItem == (ComboBoxItem)DayInputSunday) { TimeStartDayOfTheWeek = DayOfWeek.Sunday; }

            // Set TaxiStartDateTime to zero and add the currently entered values
            TaxiStartDateTime = new DateTime();
            TaxiStartDateTime = TaxiStartDateTime.AddDays((double)TimeStartDayOfTheWeek);
            TaxiStartDateTime = TaxiStartDateTime.AddHours(TimeStartInput.Time.Hours);
            TaxiStartDateTime = TaxiStartDateTime.AddMinutes(TimeStartInput.Time.Minutes);

            KilometersDriven = Math.Abs(DistanceDrivenInput.Value);
        }


        private void SetNumberBoxFormats()
        {
            // Format the input to 2 decimals, used for money-parameter input, could also use CurrencyFormatter
            IncrementNumberRounder moneyRounder = new IncrementNumberRounder { Increment = 0.01, RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp };
            DecimalFormatter moneyFormatter = new DecimalFormatter { IntegerDigits = 1, FractionDigits = 2 , NumberRounder = moneyRounder };

            // Format the input to 3 decimals, used for distance input
            IncrementNumberRounder distanceRounder = new IncrementNumberRounder { Increment = 0.001, RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp };
            DecimalFormatter distanceFormatter = new DecimalFormatter { IntegerDigits = 1, FractionDigits = 1, NumberRounder = distanceRounder };

            // Format the input to whole numbers, used for the weekend premium precentage, could use PercentFormatter
            IncrementNumberRounder wholeNumberRounder = new IncrementNumberRounder { Increment = 1, RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp };
            DecimalFormatter premiumRateFormatter = new DecimalFormatter { IntegerDigits = 1, NumberRounder = wholeNumberRounder };

            // Set the relevant formatter for the NumberBox
            PricePerKmInput.NumberFormatter = moneyFormatter;
            PricePerMinuteDayInput.NumberFormatter = moneyFormatter;
            PricePerMinuteNightInput.NumberFormatter = moneyFormatter;
            WeekendPremiumInput.NumberFormatter = premiumRateFormatter;
        }


        private void CalculateButton_Click(object sender, RoutedEventArgs e) { CalculateRidePrice(); }


        // Set the Time in the UI to the current time of the System
        private void SetTimeStartInput_Click(object sender, RoutedEventArgs e) { SetTimeStartToCurrent(); }
        private void SetTimeStartToCurrent()
        {
            DateTime CurrentDateTime;

            CurrentDateTime = DateTime.Now;

            int day = (int)CurrentDateTime.DayOfWeek;

            // set the day of the week to the ComboBox
            switch (CurrentDateTime.DayOfWeek)
            {
                case DayOfWeek.Monday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputMonday; break; }
                case DayOfWeek.Tuesday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputTuesday; break; }
                case DayOfWeek.Wednesday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputWednesday; break; }
                case DayOfWeek.Thursday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputThursday; break; }
                case DayOfWeek.Friday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputFriday; break; }
                case DayOfWeek.Saturday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputSaturday; break; }
                case DayOfWeek.Sunday: { TimeStartDayInput.SelectedItem = (ComboBoxItem)DayInputSunday; break; }
            }

            TimeStartInput.Time = CurrentDateTime.TimeOfDay;
        }





        // The following block of code is my attempt at an algorithm that would not just count over every minute but would be able to count hours when possible
        // However, I am not yet adept enough at this to figure it out in a reasonable time
        // It is in no way finished nor correct

        /*
         
         // Iterate over, the adjusted values of, rideDuration until it reaches RideDuration
            while (rideDurationAdjusted < RideDurationAdjusted)
            {
                
                Debug.WriteLine(string.Format("TaxiService: r={0}, R={1}.", rideDurationAdjusted, RideDurationAdjusted));
                i++;
                if (i > 1000) { Debug.WriteLine("TaxiService.CalculateRidePrice while-loop has exceeded the allowed number of iterations!"); break; }

                timediff = new TimeSpan();
                timediffdays = new TimeSpan(rideDurationAdjusted.Days, 0, 0, 0);

                // when current time is lower than start of DayPrice
                if (rideDurationAdjusted.Hours < DayPriceStart.Hours)
                {
                    if (RideDurationAdjusted.Subtract(rideDurationAdjusted).Hours > DayPriceStart.Hours) { timediff = DayPriceStart - rideDurationAdjusted.Subtract(timediffdays); }
                    else { timediff = RideDurationAdjusted - rideDurationAdjusted; }
                    TimeDrivenNight += timediff;
                }

                // when current time is lower than start of NightPrice
                else if (rideDurationAdjusted.Hours < NightPriceStart.Hours)
                {
                    if (RideDurationAdjusted.Subtract(rideDurationAdjusted).Hours > NightPriceStart.Subtract(DayPriceStart).Hours) { timediff = NightPriceStart - rideDurationAdjusted.Subtract(timediffdays); }
                    else { timediff = RideDurationAdjusted - rideDurationAdjusted; }
                    TimeDrivenDay += timediff;
                }

                // when current time is higher than start of NightPrice
                else if (rideDurationAdjusted.Hours >= NightPriceStart.Hours)
                {
                    if (RideDurationAdjusted.Subtract(rideDurationAdjusted).Hours <= NightPriceStart.Hours) { timediff = TimeSpan.FromDays(1) - NightPriceStart; }
                    else { timediff = RideDurationAdjusted - rideDurationAdjusted; }
                    TimeDrivenNight += timediff;
                }

                else { Debug.WriteLine("TaxiService while loop made an oopsie."); break; }

                Debug.WriteLine(string.Format("TaxiService: While-loop={0} TimeDrivenDay={1}, TimeDrivenNight={2}.", i, TimeDrivenDay, TimeDrivenNight));

                // iteration ++
                rideDurationAdjusted += timediff.Duration();
                continue;
            }
         
        */



    }
}
