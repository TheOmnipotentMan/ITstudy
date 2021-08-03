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
    public sealed partial class TransportCompany : Page
    {


        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "08:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "2";
        // Date when this project was finished
        string ProjectDateFinished = "12/07/21";





        // Default price values
        double PriceSolidMassDefault = 0.55;
        double PriceSolidVolumeDefault = 0.80;
        double PriceLiquidMassDefault = 0.45;
        double PriceLiquidVolumeDefault = 1.25;
        double AbroadPercentageDefault = 45;
        double CustomsPercentageDefault = 3.5;
        double CustomsMinimumDefault = 45;



        public TransportCompany()
        {
            this.InitializeComponent();
        }


        private void CalculatePrice()
        {
            // The final price
            double finalPrice = -1;

            // Calculate the price
            // Start by calculating the price of the cargo per kilometer
            finalPrice = GetSolidMass() * GetPriceSolidMass() + GetSolidVolume() * GetPriceSolidVolume() + GetLiquidMass() * GetPriceLiquidMass() + GetLiquidVolume() * GetPriceLiquidVolume();
            // Multiply that price by the total distance travels, and add to that the price multiplied by the distance abroad multiplied by the percentage for kilometers abroad. Resulting in the cost for transporting the cargo over that distance
            finalPrice = finalPrice * GetTotalDistance() + finalPrice * GetDistanceAbroad() * GetAbroadPercentage();
            // Add the price for customs. Multiplying the total value of the cargo by the percentage levied by customs, clamping it to the minimum fee and multiplying it by the total number of border crossings.
            finalPrice += Math.Clamp(GetTotalValue() * GetCustomsPercentage(), GetCustomsMinimumFee(), double.MaxValue) * GetBorderCrossings();
            // Add the costs for the return journey, if required. Multiplying the minimum customs fee by the number of crossings
            finalPrice += (IsReturnIncluded()) ? GetCustomsMinimumFee() * GetBorderCrossings() : 0;

            // Display the final price on screen
            FinalPriceTextBlock.Text = Math.Round(finalPrice, 2).ToString();
        }


        // Price list fields
        private double GetPriceSolidMass()
        {
            return double.IsNaN(PriceMassSolidNumberBox.Value) ? PriceSolidMassDefault : PriceMassSolidNumberBox.Value;
        }
        private double GetPriceSolidVolume()
        {
            return double.IsNaN(PriceVolumeSolidNumberBox.Value) ? PriceSolidVolumeDefault : PriceVolumeSolidNumberBox.Value;
        }
        private double GetPriceLiquidMass()
        {
            return double.IsNaN(PriceMassLiquidNumberBox.Value) ? PriceLiquidMassDefault : PriceMassLiquidNumberBox.Value;
        }
        private double GetPriceLiquidVolume()
        {
            return double.IsNaN(PriceVolumeLiquidNumberBox.Value) ? PriceLiquidVolumeDefault : PriceVolumeLiquidNumberBox.Value;
        }

        private double GetAbroadPercentage()
        {
            return (double.IsNaN(AbroadPercentageNumberBox.Value) ? AbroadPercentageDefault : AbroadPercentageNumberBox.Value) / 100;
        }
        private double GetCustomsPercentage()
        {
            return (double.IsNaN(CustomsPercentageNumberBox.Value) ? CustomsPercentageDefault : CustomsPercentageNumberBox.Value) / 100;
        }
        private double GetCustomsMinimumFee()
        {
            return double.IsNaN(CustomsMinimumNumberBox.Value) ? CustomsMinimumDefault : CustomsMinimumNumberBox.Value;
        }


        // User input fields
        private double GetSolidMass()
        {
            return CargoSolidMassNumberBox.Value;
        }
        private double GetSolidVolume()
        {
            return CargoSolidVolumeNumberBox.Value;
        }
        private double GetLiquidMass()
        {
            return CargoLiquidMassNumberBox.Value;
        }
        private double GetLiquidVolume()
        {
            return CargoLiquidVolumeNumberBox.Value;
        }
        private double GetTotalValue()
        {
            return CargoValueNumberBox.Value;
        }

        private double GetTotalDistance()
        {
            return DistanceDrivenTotalNumberBox.Value;
        }
        private double GetDistanceAbroad()
        {
            return DistanceDrivenAbroadNumberBox.Value;
        }

        private int GetBorderCrossings()
        {
            return (int)Math.Round(BorderCrossingsNumberBox.Value);
        }
        private bool IsReturnIncluded()
        {
            return (IncludeReturnJourneyCheckBox.IsChecked == true);
        }




        private void CalculatePriceButton_Click(object sender, RoutedEventArgs e)
        {
            CalculatePrice();
        }
    }
}
