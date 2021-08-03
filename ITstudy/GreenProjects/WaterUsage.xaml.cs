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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ITstudy.GreenProjects
{
    /// <summary>
    /// Calculate the lowest cost of water for the amount used
    /// </summary>
    public sealed partial class WaterUsage : Page
    {

        private class WaterRate
        {
            private float _pricePerYear;
            private float _pricePerCubicMeter;

            public WaterRate(float pricePerYear, float pricePerCubicMeter)
            {
                _pricePerYear = pricePerYear;
                _pricePerCubicMeter = pricePerCubicMeter;
            }

            public float PricePerYear
            {
                get { return _pricePerYear; }
                set { _pricePerYear = value; }
            }
            public float PricePerCubicMeter
            {
                get { return _pricePerCubicMeter; }
                set { _pricePerCubicMeter = value; }
            }
        }



        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "03:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "2";
        // Date when this project was finished
        string ProjectDateFinished = "03/08/21";


        // Default Rates
        WaterRate Rate1 = new WaterRate(100.0f, 0.25f);
        WaterRate Rate2 = new WaterRate(75.0f, 0.38f);

        double TippingPoint;


        public WaterUsage()
        {
            this.InitializeComponent();
            CalculateTippingPoint();
        }


        /// <summary>
        /// Calculate the cheapest price for the water used.
        /// Requires that Rate1 caters to large-consumers and Rate2 to small-consumers
        /// </summary>
        /// <param name="waterUsed"></param>
        private void CalculatePrice(double waterUsed)
        {
            if (waterUsed >= TippingPoint)
            {
                DisplayResult(Rate1.PricePerYear + Rate1.PricePerCubicMeter * waterUsed, 1);
            }
            else
            {
                DisplayResult(Rate2.PricePerYear + Rate2.PricePerCubicMeter * waterUsed, 2);
            }
        }

        // This method of calculation does not get screwed up when rate 1 is set to cater for small-consumer and rate 2 to large-consumer
        private void CalculatePriceWithoutTippingPoint(double waterUsed)
        {
            double price1 = Rate1.PricePerYear + waterUsed * Rate1.PricePerCubicMeter;
            double price2 = Rate2.PricePerYear + waterUsed * Rate2.PricePerCubicMeter;

            if (price1 <= price2)
            {
                DisplayResult(Rate1.PricePerYear + Rate1.PricePerCubicMeter * waterUsed, 1);
            }
            else
            {
                DisplayResult(Rate2.PricePerYear + Rate2.PricePerCubicMeter * waterUsed, 2);
            }
        }



        // Display the resulting final price on screen, along with which rate was used
        private void DisplayResult(double price, int rate)
        {
            FinalPriceTextBlock.Text = "€ " + price.ToString("N2") + "    (Rate " + rate.ToString() + ")";
        }



        /// <summary>
        /// Calculate the tipping point between the two rates, below which rate 2 is cheapest, and above which rate 1 is cheapest.
        /// Only works when rate 1 is aimed at large consumers and rate 2 at small consumers.
        /// </summary>
        private void CalculateTippingPoint()
        {
            TippingPoint = (Rate1.PricePerYear - Rate2.PricePerYear) / (Rate2.PricePerCubicMeter - Rate1.PricePerCubicMeter);
        }




        private void WaterUsed_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            CalculatePrice(sender.Value);
        }

        private void Rate_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            CalculateTippingPoint();
        }
    }
}
