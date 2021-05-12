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

// contains ObservableCollection class
using System.Collections.ObjectModel;

// TODO remove if the app is ever finished
using System.Diagnostics;



namespace ITstudy.GreenProjects
{
    /// <summary>
    /// Calculate the cost of renting a vehicle
    /// </summary>
    public sealed partial class CarRental : Page
    {

        /// <summary>
        /// Any type of rentable vehicle, base class for Vehicles collection
        /// </summary>
        public class CarRentalVehicle
        {
            public string VehicleName { get; }
            public float PricePerKm { get; }
            public float PricePerDay { get; }
            public int FreeKmPerDay { get; }
            public float FuelConsumption { get; }

            public CarRentalVehicle(string vehicleName, float pricePerKm, float pricePerDay, int freeKmPerDay, float litersOfFuelPerHundredKm)
            {
                this.VehicleName = vehicleName;
                this.PricePerKm = pricePerKm;
                this.PricePerDay = pricePerDay;
                this.FreeKmPerDay = freeKmPerDay;
                this.FuelConsumption = litersOfFuelPerHundredKm;
            }
        }


        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "09:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "4";
        // Date when this project was finished
        string ProjectDateFinished = "20/04/21";


        // A Collection of all the rentable vehicles and a proxy-version that will contain all vehicle-names for display and selection by user (https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-quickstart)
        private ObservableCollection<CarRentalVehicle> Vehicles;
        private ObservableCollection<string> VehicleNames;

        // Fuel price per liter
        double FuelPrice = 1.50d;




        public CarRental()
        {
            this.InitializeComponent();
            InitializeVehiclePool();
        }




        // Add specified vehicles to the vehicle-pool, ie ObservableCollection<Vehicle> vehicles, and copy all vheicle names to the collection VehicleNames which will be used to display and select the vehicles.
        private void InitializeVehiclePool()
        {
            Vehicles = new ObservableCollection<CarRentalVehicle>();
            Vehicles.Add(new CarRentalVehicle("Car", 0.20f, 50.0f, 100, 6.0f));
            Vehicles.Add(new CarRentalVehicle("Van", 0.30f, 95.0f, 0, 10.0f));

            // Copy every VehicleName from Vehicles to VehicleNames, with matching indexes
            VehicleNames = new ObservableCollection<string>();
            for (int i = 0; i < Vehicles.Count; i++)
            {
                VehicleNames.Add(Vehicles.ElementAt(i).VehicleName);
            }
        }


        // Show all the pricing details of a vehicle on screen, the vehicle is selected via its index in Vehicles-collection
        private void SetPricingDetails(int vehicleIndex)
        {
            PricingDetailsVehicleName.Text = Vehicles.ElementAt(vehicleIndex).VehicleName;
            PricingDetailsPricePerKm.Text = Vehicles.ElementAt(vehicleIndex).PricePerKm.ToString();
            PricingDetailsPricePerDay.Text = Vehicles.ElementAt(vehicleIndex).PricePerDay.ToString();
            PricingDetailsFreeKmPerDay.Text = Vehicles.ElementAt(vehicleIndex).FreeKmPerDay.ToString();
            PricingDetailsFuelConsumption.Text = Vehicles.ElementAt(vehicleIndex).FuelConsumption.ToString();
            PricingDetailsFuelPrice.Text = FuelPrice.ToString();
        }


        // Calculate the total cost of the rental
        private void CalculatePrice()
        {
            double result = 10.177777f;
            double resultRounded = 0f;

            CarRentalVehicle vehicle;
            double distance;
            TimeSpan totalTimeRented;

            // Set the currently selected vehicle, if no vehicle is selected select the first
            if (VehicleTypeComboBox.SelectedIndex < 0) { VehicleTypeComboBox.SelectedItem = VehicleTypeComboBox.Items.First(); }
            vehicle = Vehicles.ElementAt(VehicleTypeComboBox.SelectedIndex);

            // Set the total distance driven, if no distance was given set it to 120, then calculate the cost of fuel for that distance, and finally adjust it for the free kilometers per day
            if (string.IsNullOrWhiteSpace(DistanceDrivenInput.Text)) { DistanceDrivenInput.Text = "120"; }
            distance = DistanceDrivenInput.Value;
            result = (vehicle.FuelConsumption / 100) * distance * FuelPrice;
            distance = Math.Clamp(distance - ((totalTimeRented.Days + 1) * vehicle.FreeKmPerDay), 0, 40075);

            // Get the time at the start and end of renting, if no time was given set the end-time to the current time or the start-time to 25 hours before the end-time, then pass the difference to totalTimeRented
            if (EndOfRentalCalendarDatePicker.Date == null | EndOfRentalTimePicker.Time == null) { SetEndOfRentalToCurrent(); }
            if (StartOfRentalCalendarDatePicker.Date == null | StartOfRentalTimePicker.Time == null)
            {
                DateTime dateTime = EndOfRentalCalendarDatePicker.Date.Value.DateTime + EndOfRentalTimePicker.Time;
                dateTime -= new TimeSpan(1, 1, 0, 0);
                StartOfRentalCalendarDatePicker.Date = dateTime.Date;
                StartOfRentalTimePicker.Time = dateTime.TimeOfDay;
            }
            totalTimeRented = (EndOfRentalCalendarDatePicker.Date.Value.DateTime + EndOfRentalTimePicker.Time) - (StartOfRentalCalendarDatePicker.Date.Value.DateTime + StartOfRentalTimePicker.Time);

            Debug.WriteLine(string.Format("CarRental: CalculatePrice distance = {0}", distance.ToString()));
            Debug.WriteLine(string.Format("CarRental: CalculatePrice totalTimeRented = {0}", totalTimeRented.ToString()));

            // Calculate the rest of the price
            result += (distance * vehicle.PricePerKm) + ((totalTimeRented.Days + 1) * vehicle.PricePerDay);
            Debug.WriteLine(string.Format("CarRental: CalculatePrice result = {0}", result.ToString()));

            // Round the result to two decimal places and display it on screen
            resultRounded = Math.Round(result * 100)/100;
            PriceOfRentalTextBlock.Text = resultRounded.ToString();
        }


        // Set the end-time of the rental period to the current time on this system/computer
        private void SetEndOfRentalToCurrentButton_Click(object sender, RoutedEventArgs e) { SetEndOfRentalToCurrent(); }
        private void SetEndOfRentalToCurrent()
        {
            EndOfRentalCalendarDatePicker.Date = DateTime.Now.Date;
            EndOfRentalTimePicker.Time = DateTime.Now.TimeOfDay;
        }


        private void VehicleTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPricingDetails(VehicleTypeComboBox.SelectedIndex);

            // use this instead if you don't want to explicitly name the sending ComboBox, not needed here as the sender is a single ComboBox
            /*
            ComboBox comboBox = sender as ComboBox;
            SetPricingDetails(comboBox.SelectedIndex);
            */
        }


        private void CalculatePriceButton_Click(object sender, RoutedEventArgs e)
        {
            CalculatePrice();
        }
    }
}
