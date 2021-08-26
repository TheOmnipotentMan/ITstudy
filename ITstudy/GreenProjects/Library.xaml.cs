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
        /// A dictionary with some control meaasures and functions, only accepts both each category and key once
        /// </summary>
        private class CategoryDictionary
        {
            private Dictionary<ushort, string> _Categories;
            private Dictionary<string, ushort> _CategoriesInverse;

            private Stack<ushort> _RemovedKeys;

            public CategoryDictionary()
            {
                _Categories = new Dictionary<ushort, string>();
                _CategoriesInverse = new Dictionary<string, ushort>();
                _RemovedKeys = new Stack<ushort>();
            }

            /// <summary>
            /// Add a new category to the collection
            /// </summary>
            /// <param name="category">Category to add</param>
            /// <returns>True if succesfull. False if not, likely because category is already present</returns>
            public bool Add(string category)
            {
                if (_Categories.Count >= ushort.MaxValue) { Debug.WriteLine($"Library: CategoryDictionary.Add(string category) Dictionary has reached max size"); return false; }
                else if (_CategoriesInverse.ContainsKey(category)) { Debug.WriteLine($"Library: CategoryDictionary.Add(string category) category {category} is duplicate"); return false; }
                else
                {
                    ushort key = GetNextKey();
                    _Categories.Add(key, category);
                    _CategoriesInverse.Add(category, key);
                    return true;
                }
            }

            /// <summary>
            /// Remove a category from the collection
            /// </summary>
            /// <param name="category">Category to remove</param>
            /// <returns>True if succesfull. False if not, likely because category could not be found</returns>
            public bool Remove(string category)
            {
                ushort key;
                if (!_CategoriesInverse.TryGetValue(category, out key)) { Debug.WriteLine($"Library: CategoryDictionary.Remove(string category) category {category} not found"); return false; }
                else
                {
                    _RemovedKeys.Push(key);
                    _Categories.Remove(key);
                    _CategoriesInverse.Remove(category);
                    return true;
                }
            }

            /// <summary>
            /// Remove a category from the collection by key
            /// </summary>
            /// <param name="key">Key to remove</param>
            /// <returns>True if succesfull. False if not, likely because key could not be found</returns>
            public bool Remove(ushort key)
            {
                string category;
                if (!_Categories.TryGetValue(key, out category)) { Debug.WriteLine($"Library: CategoryDictionary.Remove(ushort key) key {key} not found"); return false; }
                else
                {
                    _RemovedKeys.Push(key);
                    _Categories.Remove(key);
                    _CategoriesInverse.Remove(category);
                    return true;
                }
            }

            /// <summary>
            /// Get the next free key for the dictionaries
            /// </summary>
            /// <returns></returns>
            private ushort GetNextKey()
            {
                if (_RemovedKeys.Count > 0)
                {
                    return _RemovedKeys.Pop();
                }
                else
                {
                    return (ushort)_Categories.Count();
                }
            }




            /// <summary>
            /// Determines whether the collection contains the speficied key
            /// </summary>
            /// <param name="key"></param>
            /// <returns>True if the collection contains the specified key, else false.</returns>
            public bool ContainsKey(ushort key)
            {
                return _Categories.ContainsKey(key);
            }

            /// <summary>
            /// Detemines whether the collection contains the specified category
            /// </summary>
            /// <param name="category"></param>
            /// <returns>True if the collection contains the specified category, else false.</returns>
            public bool ContainsCategory(string category)
            {
                return _CategoriesInverse.ContainsKey(category);
            }

            /// <summary>
            /// Get the value assosiated with the specified key
            /// </summary>
            /// <param name="category"></param>
            /// <param name="key"></param>
            /// <returns>True if the collection contains an element with the specified key, else false.</returns>
            public bool GetKey(string category, out ushort key)
            {
                return _CategoriesInverse.TryGetValue(category, out key);
            }

            /// <summary>
            /// Get the key associated with the specified value
            /// </summary>
            /// <param name="key"></param>
            /// <param name="category"></param>
            /// <returns>True if the collection contains an element with the specified value, else false.</returns>
            public bool GetCategory(ushort key, out string category)
            {
                return _Categories.TryGetValue(key, out category);
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
        /// All the possible categories for books in the inventory
        /// </summary>
        private CategoryDictionary Categories;




        public Library()
        {
            this.InitializeComponent();

            // Generate some content
            GenerateCategories();
            GenerateInventory();

            InventoryListView.ItemsSource = Inventory;
        }


        /// <summary>
        /// Populate the dictonary Categories with some categories
        /// </summary>
        private void GenerateCategories()
        {
            // Categories to generate, ordered alphabetically. Excerpt of, based on, https://en.wikipedia.org/wiki/List_of_writing_genres
            string[] categories = new string[]
            {
                "Academic", "Action", "Adventure",
                "Biography",
                "Children", "Classic", "Comedy", "Comic", "Crime",

                "Epic", "Essay",
                "Fantasy", "Folklore",

                "History", "Horror",



                
                "Manual", "Mystery",
                "Non Fiction", "Novel", "Novella",

                "Philosophy",

                "Religion", "Romance",
                "Satire", "Science Fiction", "Self Help", "Superhero", "Survival",
                "Thriller", "Travel",


                "Western",

                "Young Adult",

            };

            Categories = new CategoryDictionary();

            for (int i = 0; i < categories.Length; i++)
            {
                AddCategory(categories[i]);
            }            
        }


        /// <summary>
        /// Populate the list Inventory with some books, https://en.wikipedia.org/wiki/List_of_best-selling_books
        /// </summary>
        private void GenerateInventory()
        {
            Inventory = new List<Library_Book>();

            AddBook("The Hobbit", "J.R.R. Tolkien", "Geroge Allen & Unwin Ltd.", 1957, 2002, 3, 9022532003, "Fantasy");
            AddBook("Harry Potter and the Philosopher's Stone", "J.K. Rowling", "Bloomsbury", 1997, 1997, 1, 0747532699, "Fantasy");
            AddBook("The Little Prince", "Antoine de Saint-Exupéry", "Reynal & Hitchcock", 1943, 2020, 9, 0, "Novella");
            AddBook("Dream of the Red Chamber", "Cao Xueqin", "N/A", 1750, 2020, 255, 0, "Novel");
            AddBook("And Then There Were None", "Agatha Christie", "Collins Crime Club", 1939, 2020, 0, 0, "Mystery");

            AddBook("The Very Hungry Catepillar", "Eric Carle", "World Publishing Company", 1969, 2020, 99, 399226907, "Children");
        }


        /// <summary>
        /// Add a category to Categeries
        /// </summary>
        /// <param name="category"></param>
        private void AddCategory(string category)
        {
            try
            {
                Categories.Add(category);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine($"Library: GenerateCategories() encountered a duplicate. {category}");
            }
        }




        /// <summary>
        /// Add a book to Inventory. Handles category automatically given the desired category as a string
        /// </summary>
        /// <param name="title"></param>
        /// <param name="author"></param>
        /// <param name="publisher"></param>
        /// <param name="firstPrintYear"></param>
        /// <param name="printYear"></param>
        /// <param name="printNumber"></param>
        /// <param name="isbn"></param>
        /// <param name="category"></param>
        private void AddBook(string title, string author, string publisher, ushort firstPrintYear, ushort printYear, byte printNumber, ulong isbn, string category)
        {
            ushort key;
            if (Categories.GetKey(category, out key))
            {
                Inventory.Add(new Library_Book(title, author, publisher, firstPrintYear, printYear, printNumber, isbn, key, category));
            }
            else
            {
                Inventory.Add(new Library_Book(title, author, publisher, firstPrintYear, printYear, printNumber, isbn, ushort.MaxValue, "NotFound"));
            }
        }

    }
}
