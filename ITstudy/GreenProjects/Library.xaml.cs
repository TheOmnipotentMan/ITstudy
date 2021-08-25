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
    /// Library book-renting and return system
    /// </summary>
    public sealed partial class Library : Page
    {

        /// <summary>
        /// Alternative for the Categories list, use this instead of the string
        /// </summary>
        private class Category
        {
            public class CategoryCode : IComparable
            {
                private ushort _Code;

                public CategoryCode(ushort code)
                {
                    _Code = Math.Clamp(code, ushort.MinValue, (ushort)9999);
                }

                public int CompareTo(object obj)
                {
                    if (obj == null) { return 1; }
                    if (obj != null)
                    {
                        CategoryCode otherCatCode = obj as CategoryCode;
                        return this._Code.CompareTo(otherCatCode._Code);
                    }
                    else
                    {
                        throw new ArgumentException("PlaceAnOrder_Item: Object is not a Year!");
                    }
                }

                public ushort Code
                {
                    get { return _Code; }
                }
            }

            private string _Name;
            private CategoryCode _Code;

            public Category(string name, ushort code)
            {
                _Name = name;
                _Code = new CategoryCode(code);
            }

            public string Name
            {
                get { return _Name; }
            }

            public ushort Code
            {
                get { return _Code.Code; }
            }
        }







        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "00:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "x";
        // Date when this project was finished
        string ProjectDateFinished = "00/00/21";









        /// <summary>
        /// The inventory, the collection of books, of the library
        /// </summary>
        private List<Library_Book> Inventory;


        /// <summary>
        /// All possible categories of books in the inventory. as soon as any item has been added to inventory, DO NOT SORT!
        /// </summary>
        private List<string> Categories; // TODO replace with dictionary




        public Library()
        {
            this.InitializeComponent();

            GenerateCategories();

            GenerateInventory();
        }


        private void GenerateCategories()
        {
            Categories = new List<string>();

            Categories.Add("Academic");
            Categories.Add("Children");
            Categories.Add("Crime");
            Categories.Add("Epic");
            Categories.Add("Fantasy");
            Categories.Add("History");
            Categories.Add("Romance");
            Categories.Add("Science Fiction");
            Categories.Add("Thriller");
        }


        private void GenerateInventory()
        {
            Inventory = new List<Library_Book>();

            Inventory.Add(new Library_Book("The Very Hungry Catepillar", "Eric Carle", "World Publishing Company", 1969, 2020, 99, 0399226907, 1));
        }

    }
}
