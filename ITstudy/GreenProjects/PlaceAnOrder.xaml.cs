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

    public sealed partial class PlaceAnOrder : Page
    {

        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "08:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "4";
        // Date when this project was finished
        string ProjectDateFinished = "19/08/21";




        List<PlaceAnOrder_Item> Inventory;

        double TotalPrice;

        int CurrentYear = 2021;






        public PlaceAnOrder()
        {
            this.InitializeComponent();

            GenerateInventory();
        }


        /// <summary>
        /// Generate the Inventory to be used
        /// </summary>
        private void GenerateInventory()
        {
            Inventory = new List<PlaceAnOrder_Item>();
            Inventory.Add(new PlaceAnOrder_Item("Item01", 2007, 01));
            Inventory.Add(new PlaceAnOrder_Item("Item02", 2020, 10, 60d));
            Inventory.Add(new PlaceAnOrder_Item("Item03", 1234, 20, 10000d));
            Inventory.Add(new PlaceAnOrder_Item("Item04", 2013, 32, 240d));
            Inventory.Add(new PlaceAnOrder_Item("Item05", 2021, 02, -20d));
            Inventory.Add(new PlaceAnOrder_Item("Item06", 2018, 63));
            Inventory.Add(new PlaceAnOrder_Item("Item07", 0000, 00));
            Inventory.Add(new PlaceAnOrder_Item("Item08", 2021, 15, 50d));
            Inventory.Add(new PlaceAnOrder_Item("Item09", 2020, 13, 40d));

            SortInventoryByCode();

            InventoryListView.ItemsSource = Inventory;

            // Set default order
            OrderInputTextBox.Text = "200701 202010 201863 202102 202115";
        }


        /// <summary>
        /// Sort invertory by code, ascending
        /// https://docs.microsoft.com/en-us/dotnet/api/system.icomparable?view=net-5.0 & https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
        /// </summary>
        private void SortInventoryByCode()
        {
            Inventory.Sort((x, y) => x.Code.CompareTo(y.Code));
        }


        /// <summary>
        /// Process the order(s). If no order was provided, defaults to the value of OrderInputTextBox.Text, ie the input field for the user.
        /// Generates a list with all (recognised) orders and displays it on screen with relevant information
        /// </summary>
        /// <param name="orders">The orders to process, defaults to the value of OrderInputTextBox.Text</param>
        private void ProcessOrder(string orders = "")
        {
            // If no specific order string was given, copy the text from the input field OrderInputTextBox
            if (orders == "") { orders = OrderInputTextBox.Text; }

            TotalPrice = 0;

            // Create a new list that will hold all the individual orders
            List<PlaceAnOrder_OrderedItem> Orders = new List<PlaceAnOrder_OrderedItem>();

            Debug.WriteLine($"PlaceAnOrder: ProcessOrder() codes in order = {orders.Length / 7 + 1}");

            // Go over the order string in steps of 7 (a single order is 6 chars/digits long, with one additional char at the end for seperation)
            for (int i1 = 0; i1 < orders.Length; i1+=7)
            {
                // Try to parse the substring of orders, ie an order, to the integer 'code'. And if this is succesfull, continue
                int code;
                if (int.TryParse(orders.Substring(i1, 6), out code))
                {
                    /* OLD, now located in seperate method FindIndexBinary()
                    // Do a binary search of Inventory for the item that matches to the given order
                    int index = Inventory.Count / 2;
                    Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search starting, max iterations = {Math.Ceiling(Math.Log(Inventory.Count, 2))}");
                    for (int i2 = 0; i2 < Math.Ceiling(Math.Log(Inventory.Count, 2)); i2++)
                    {
                        Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search, strart loop {i2}, index = {index}");
                        // If the code matches one in the inventory, add it to the orders with relevant information
                        if (code == Inventory[index].CodeInt())
                        {
                            i2 = Inventory.Count;
                            Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search found index = {index}");
                            Orders.Add(new PlaceAnOrder_OrderedItem());
                            Orders.Last().Name = Inventory[index].Name;
                            Orders.Last().Code = Inventory[index].Code.CodeString;
                            Orders.Last().BasePrice = Inventory[index].PriceString;
                            Orders.Last().Discount = GetDiscountPercentage(index);
                            Orders.Last().Price = GetDiscountPrice(index);
                        }
                        else if (code < Inventory[index].CodeInt())
                        {
                            Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search index too high");
                            index -= Math.Clamp(Inventory.Count / (int)Math.Pow(2, i2 + 2), 1, int.MaxValue);
                        }
                        else if (code > Inventory[index].CodeInt())
                        {
                            Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search index too low");
                            index += Math.Clamp(Inventory.Count / (int)Math.Pow(2, i2 + 2), 1, int.MaxValue);
                        }
                        // Item can not be found, should never be reached because one of the above statements will always be true
                        else
                        {
                            i2 = Inventory.Count;
                            index = -1;
                            Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search failed to compare numbers!");
                        }
                        Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search, end loop {i2}, index = {index}");
                    }
                    */

                    // Try and find a matching item
                    int index = FindIndexBinary(code);

                    // If a match was found, add it to the orders list
                    if (index >= 0)
                    {
                        Orders.Add(new PlaceAnOrder_OrderedItem());
                        Orders.Last().Name = Inventory[index].Name;
                        Orders.Last().Code = Inventory[index].Code.CodeString;
                        Orders.Last().BasePrice = Inventory[index].PriceString;
                        Orders.Last().Discount = GetDiscountPercentage(index);
                        Orders.Last().Price = GetDiscountPrice(index);
                    }                    
                }
            }

            // If orders were recognised, display the info
            if (Orders.Count > 0)
            {
                // Display the individual orders and their details
                OrderDetailsListView.ItemsSource = Orders;
                // Display total price
                TotalPriceTextBlock.Text = Math.Round(TotalPrice, 2).ToString("N2");                
            }            

        }


        /// <summary>
        /// Find the matching item of the code, via binary search, and return its index. Returns -1 if no match was found
        /// </summary>
        /// <param name="code">The code of the item to try and find in the Inventory</param>
        /// <returns>Index of item in Inventory, -1 if nothing was found</returns>
        private int FindIndexBinary(int code)
        {
            Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search starting, searching for {code}, max iterations = {Math.Ceiling(Math.Log(Inventory.Count, 2))}");

            int index = Inventory.Count / 2;
            for (int i2 = 0; i2 < Math.Ceiling(Math.Log(Inventory.Count, 2)); i2++)
            {
                Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search, strart loop {i2}, index = {index}");
                // If the code matches one in the inventory, end the loop
                if (code == Inventory[index].CodeInt())
                {
                    Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search found index = {index}");
                    return index;
                }
                else if (code < Inventory[index].CodeInt())
                {
                    Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search index too high");
                    index -= Math.Clamp(Inventory.Count / (int)Math.Pow(2, i2 + 2), 1, int.MaxValue);
                }
                else if (code > Inventory[index].CodeInt())
                {
                    Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search index too low");
                    index += Math.Clamp(Inventory.Count / (int)Math.Pow(2, i2 + 2), 1, int.MaxValue);
                }
                // Item can not be found, should never be reached because one of the above statements will always be true
                else
                {
                    i2 = Inventory.Count;
                    Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search failed to compare numbers!");
                }
                Debug.WriteLine($"PlaceAnOrder: ProcessOrder() binary search, end loop {i2}, index = {index}");
            }

            return -1;
        }


        /// <summary>
        /// Get the total discount percentage for an item
        /// </summary>
        /// <param name="index">index of item in inventory</param>
        /// <returns>total discount on item in percentage as a string</returns>
        private string GetDiscountPercentage(int index)
        {
            return (GetDiscountYear(Inventory[index].Year()) + GetDiscountTracker(Inventory[index].TrackerCode())).ToString() + " %";
        }


        /// <summary>
        /// Get the discounted price of an item.
        /// Also adds the discounted price to the total price
        /// </summary>
        /// <param name="index">index of item in inventory</param>
        /// <returns>Discount-price as a string</returns>
        private string GetDiscountPrice(int index)
        {
            Debug.WriteLine($"PlaceAnOrder: GetDiscountPrice() price = {Inventory[index].Price()}, multiplying by {1 - ((GetDiscountYear(Inventory[index].Year()) + GetDiscountTracker(Inventory[index].TrackerCode())) / 100d)}");

            // Add the price of the item to the total cost as well
            TotalPrice += Inventory[index].Price() * (1 - ((GetDiscountYear(Inventory[index].Year()) + GetDiscountTracker(Inventory[index].TrackerCode())) / 100d));

            return "€ " + (Inventory[index].Price() * (1 - ((GetDiscountYear(Inventory[index].Year()) + GetDiscountTracker(Inventory[index].TrackerCode())) / 100d))).ToString("N2");
        }


        /// <summary>
        /// Get the discount percentage of an item for the year it was produced
        /// 25% >= 5 years, 10% >= 3 years, 5% >= 2 years. 
        /// </summary>
        /// <param name="year">year of production, should be between 0 & 9999</param>
        /// <returns>discount percentage based on year</returns>
        private int GetDiscountYear(int year)
        {
            // Production year >= than 5 years ago
            if ((CurrentYear - year) >= 5)
            {
                return 25;
            }
            // Production year >= than 3 years ago
            else if ((CurrentYear - year) >= 3)
            {
                return 10;
            }
            // Production year >= than 2 years ago
            else if ((CurrentYear - year) >= 2)
            {
                return 5;
            }

            // Else no discount
            else { return 0; }
        }

        /// <summary>
        /// Get the discount percentage of an item for its tracker code
        /// If the first number of the tracker code is even, the discount is 2%, else it's 0.
        /// </summary>
        /// <param name="tracker">tracker code, should be between 0 & 99</param>
        /// <returns>discount percentage based on tracker code</returns>
        private int GetDiscountTracker(int tracker)
        {
            return (((tracker - (tracker % 10)) / 10) % 2 == 0) ? 2 : 0;
        }



        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            ProcessOrder();
        }
    }
}
