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

using System.Collections.Specialized;



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

            private class CategoryLateFee
            {                
                /// <summary>
                /// The amount of money added to the late fee per period of time
                /// </summary>
                private double _LateFeeIncrement;
                /// <summary>
                /// The type of period how long a book of this category is allowed to be loaned for
                /// 0 = day, 1 = week, 2 = month, 3 = year
                /// </summary>
                private byte _AllowedPeriodType;
                /// <summary>
                /// The type of period after which a new FeeIncrement is added to the late fee.
                /// 0 = day, 1 = week, 2 = month, 3 = year
                /// </summary>
                private byte _LatePeriodType;
                /// <summary>
                /// The number of AllowedPeriodType durations this book can be loaned for
                /// </summary>
                private uint _AllowedPeriodLength;
                /// <summary>
                /// The number of LatePeriodType durations after which a new FeeIncrement is added to the late fee
                /// </summary>
                private uint _LatePeriodLength;
                /// <summary>
                /// Key that matches that of the category key stored in the dictionaries
                /// </summary>
                private ushort _Key;

                public CategoryLateFee(ushort key)
                {
                    _Key = key;
                    _LateFeeIncrement = 1;
                    _AllowedPeriodType = 2;
                    _AllowedPeriodLength = 1;
                    _LatePeriodType = 0;
                    _LatePeriodLength = 1;
                }

                /// <summary>
                /// Create a new Late Fee
                /// </summary>
                /// <param name="lateFee">Late fee, increment</param>
                /// <param name="allowedPeriodType">Type of period allowed, 0 = day, 1 = week, 2 = month, 3 = year</param>
                /// <param name="allowedPeriodLength">Length of period allowed</param>
                /// <param name="latePeriodType">Type of period late, 0 = day, 1 = week, 2 = month, 3 = year</param>
                /// <param name="latePeriodLength">Length of period late, late fee is incremented each time this is exceeded</param>
                public CategoryLateFee(ushort key, double lateFee, byte allowedPeriodType, uint allowedPeriodLength, byte latePeriodType, uint latePeriodLength)
                {
                    _Key = key;
                    _LateFeeIncrement = lateFee;
                    _AllowedPeriodType = allowedPeriodType;
                    _AllowedPeriodLength = Math.Clamp(allowedPeriodLength, 1, uint.MaxValue);
                    _LatePeriodType = latePeriodType;
                    _LatePeriodLength = Math.Clamp(latePeriodLength, 1, uint.MaxValue);
                }


                /// <summary>
                /// Set new values for this Late Fee
                /// </summary>
                /// <param name="lateFee">Late fee, increment</param>
                /// <param name="allowedPeriodType">Type of period allowed, 0 = day, 1 = week, 2 = month, 3 = year</param>
                /// <param name="allowedPeriodLength">Length of period allowed</param>
                /// <param name="latePeriodType">Type of period late, 0 = day, 1 = week, 2 = month, 3 = year</param>
                /// <param name="latePeriodLength">Length of period late, late fee is incremented each time this is exceeded</param>
                public void SetLateFee(double lateFee, byte allowedPeriodType, uint allowedPeriodLength, byte latePeriodType, uint latePeriodLength)
                {
                    _LateFeeIncrement = lateFee;
                    _AllowedPeriodType = allowedPeriodType;
                    _AllowedPeriodLength = Math.Clamp(allowedPeriodLength, 1, uint.MaxValue);
                    _LatePeriodType = latePeriodType;
                    _LatePeriodLength = Math.Clamp(latePeriodLength, 1, uint.MaxValue);
                }

                /// <summary>
                /// Get the Late Fee for this category
                /// </summary>
                /// <param name="loanOutDate"></param>
                /// <param name="returnDate"></param>
                /// <returns></returns>
                public double LateFee(DateTime loanOutDate, DateTime returnDate)
                {
                    if (!IsWithinAllowedPeriod(loanOutDate, returnDate))
                    {
                        loanOutDate = AdjustForAllowedPeriod(loanOutDate);
                        switch (_LatePeriodType)
                        {
                            // Days
                            case 0:
                                {
                                    return _LateFeeIncrement * Math.Ceiling((returnDate - loanOutDate).TotalDays / _LatePeriodLength);
                                }
                            // Weeks
                            case 1:
                                {
                                    return _LateFeeIncrement * Math.Ceiling((returnDate - loanOutDate).TotalDays / 7 / _LatePeriodLength);
                                }
                            // Months
                            case 2:
                                {
                                    return _LateFeeIncrement * Math.Ceiling((returnDate.Month - loanOutDate.Month + 12d * (returnDate.Year - loanOutDate.Year) + 1) / _LatePeriodLength);
                                }
                            // Years
                            case 3:
                                {
                                    return _LateFeeIncrement * ((returnDate.Year - loanOutDate.Year + 1) / _LatePeriodLength);
                                }
                            default: { Debug.WriteLine($"Library: CategoryLateFee.LateFee() did not find a matching case for _LatePeriodType {_LatePeriodType}"); return 0d; }
                        }
                    }
                    else
                    {
                        return 0d;
                    }
                }


                /// <summary>
                /// The key of the category of this late fee, identical to key in _Categories Dictionary
                /// </summary>
                public ushort Key
                {
                    get { return _Key; }
                }

                /// <summary>
                /// Check if the specified period is within the period books of this category are allowed to be loaned for
                /// </summary>
                /// <param name="loanOutDate"></param>
                /// <param name="returnDate"></param>
                /// <returns></returns>
                private bool IsWithinAllowedPeriod(DateTime loanOutDate, DateTime returnDate)
                {
                    switch (_AllowedPeriodType)
                    {
                        // Days, default
                        case 0:
                            {
                                return (returnDate - loanOutDate).TotalDays <= _AllowedPeriodLength;
                            }
                        // Weeks
                        case 1:
                            {
                                return (returnDate - loanOutDate).TotalDays / 7 <= _AllowedPeriodLength;
                            }
                        // Months
                        case 2:
                            {
                                return returnDate.Month - loanOutDate.Month + 12d * (returnDate.Year - loanOutDate.Year) <= _AllowedPeriodLength;
                            }
                        // Years
                        case 3:
                            {
                                return returnDate.Year - loanOutDate.Year <= _AllowedPeriodLength;
                            }
                        default: { Debug.WriteLine($"Library: CategoryLateFee.IsWithinAllowedPeriod() did not find a matching case for _AllowedPeriodType {_AllowedPeriodType}"); return true; }
                    }
                }

                /// <summary>
                /// Adjust a DateTime for the amount of time a book of this category can be on loan for
                /// </summary>
                /// <param name="loanOutDate"></param>
                /// <returns></returns>
                private DateTime AdjustForAllowedPeriod(DateTime loanOutDate)
                {
                    switch (_AllowedPeriodType)
                    {
                        // Days
                        case 0:
                            {
                                return loanOutDate.AddDays(_AllowedPeriodLength);
                            }
                        // Weeks
                        case 1:
                            {
                                return loanOutDate.AddDays(_AllowedPeriodLength * 7);
                            }
                        // Months
                        case 2:
                            {
                                return loanOutDate.AddMonths((int)_AllowedPeriodLength);
                            }
                        // Years
                        case 3:
                            {
                                return loanOutDate.AddYears((int)_AllowedPeriodLength);
                            }
                        default: { Debug.WriteLine($"Library: CategoryLateFee.llowedPeriod() did not find a matching case for _AllowedPeriodType {_AllowedPeriodType}"); return new DateTime(0, 1, 0); }
                    }
                }
            }




            private Dictionary<ushort, string> _Categories;
            private Dictionary<string, ushort> _CategoriesInverse;

            private List<CategoryLateFee> _LateFee;

            private Stack<ushort> _RemovedKeys;



            /// <summary>
            /// Create a new collection of categories. Call the Add method to add categories
            /// </summary>
            public CategoryDictionary()
            {
                _Categories = new Dictionary<ushort, string>();
                _CategoriesInverse = new Dictionary<string, ushort>();
                _LateFee = new List<CategoryLateFee>();
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
                    _LateFee.Add(new CategoryLateFee(key));
                    return true;
                }
            }

            /// <summary>
            /// Add a new category to the collection
            /// </summary>
            /// <param name="category">Category to add</param>
            /// <returns>True if succesfull. False if not, likely because category is already present</returns>
            public bool Add(string category, double lateFee, byte allowedPeriodType, uint allowedPeriodLength, byte latePeriodType, uint latePeriodLength)
            {
                if (_Categories.Count >= ushort.MaxValue) { Debug.WriteLine($"Library: CategoryDictionary.Add(string category) Dictionary has reached max size"); return false; }
                else if (_CategoriesInverse.ContainsKey(category)) { Debug.WriteLine($"Library: CategoryDictionary.Add(string category) category {category} is duplicate"); return false; }
                else
                {
                    ushort key = GetNextKey();
                    _Categories.Add(key, category);
                    _CategoriesInverse.Add(category, key);
                    _LateFee.Add(new CategoryLateFee(key, lateFee, allowedPeriodType, allowedPeriodLength, latePeriodType, latePeriodLength));
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
                    RemoveCategoryLateFee(key);
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
                    RemoveCategoryLateFee(key);
                    return true;
                }
            }





            /// <summary>
            /// Remove a CategoryLateFee from the LateFee list
            /// </summary>
            /// <param name="key">Key of late fee to remove</param>
            private void RemoveCategoryLateFee(ushort key)
            {
                for (int i = 0; i < _LateFee.Count; i++)
                {
                    if (_LateFee[i].Key == key)
                    {
                        _LateFee.RemoveAt(i);
                        i--;
                    }
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
            public bool TryGetKey(string category, out ushort key)
            {
                return _CategoriesInverse.TryGetValue(category, out key);
            }

            /// <summary>
            /// Get the key associated with the specified value
            /// </summary>
            /// <param name="key"></param>
            /// <param name="category"></param>
            /// <returns>True if the collection contains an element with the specified value, else false.</returns>
            public bool TryGetCategory(ushort key, out string category)
            {
                return _Categories.TryGetValue(key, out category);
            }

            /// <summary>
            /// Set new values for late fee of a category
            /// </summary>
            /// <param name="category">Which category to set new late fee values for</param>
            /// <param name="lateFee">The late fee increment</param>
            /// <param name="allowedPeriodType">Type of period allowed, 0 = day, 1 = week, 2 = month, 3 = year</param>
            /// <param name="allowedPeriodLength">Length of period allowed</param>
            /// <param name="latePeriodType">Type of period late, 0 = day, 1 = week, 2 = month, 3 = year</param>
            /// <param name="latePeriodLength">Lenght of period late, after which the total Late-Fee will be incremented by the value of lateFee</param>
            /// <returns>True if succesfull, false if not</returns>
            public bool SetLateFee(string category, double lateFee, byte allowedPeriodType, uint allowedPeriodLength, byte latePeriodType, uint latePeriodLength)
            {
                ushort key;
                if (_CategoriesInverse.TryGetValue(category, out key))
                {
                    _LateFee.Find(x => x.Key == key).SetLateFee(lateFee, allowedPeriodType, allowedPeriodLength, latePeriodType, latePeriodLength);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Get the late fee for a book given its category, the date is was loaned out and date it was returned
            /// </summary>
            /// <param name="category"></param>
            /// <param name="loanOutDate"></param>
            /// <param name="returnDate"></param>
            /// <returns></returns>
            public double GetLateFee(string category, DateTime loanOutDate, DateTime returnDate)
            {
                ushort key = 0;
                if (_CategoriesInverse.TryGetValue(category, out key))
                {
                    return _LateFee.Find(x => x.Key == key).LateFee(loanOutDate, returnDate);
                }
                else
                {
                    Debug.WriteLine($"Library: CategoryDictionary.GetLateFee() failed to find the category {category}");
                    return 0;
                }                
            }
        }

        /// <summary>
        /// Generates new and keeps track of removed/freed-up book codes
        /// </summary>
        private class BookCodeGenerator
        {
            private ushort _CurrentIndex;
            private Stack<ushort> _FreeCodes;

            public BookCodeGenerator(ushort startingIndex = 0)
            {
                _CurrentIndex = startingIndex;
                _FreeCodes = new Stack<ushort>();
            }

            /// <summary>
            /// Get a new book code
            /// </summary>
            /// <returns></returns>
            public ushort GetCode()
            {
                if (_FreeCodes.Count == 0)
                {
                    _CurrentIndex++;
                    return (ushort)(_CurrentIndex - 1);
                }
                else
                {
                    return _FreeCodes.Pop();
                }
            }

            /// <summary>
            /// Return a used book code, so it can be re-used for another book
            /// </summary>
            /// <param name="code"></param>
            public void ReturnCode(ushort code)
            {
                if (!_FreeCodes.Contains(code))
                {
                    _FreeCodes.Push(code);
                }
            }

            /// <summary>
            /// Get the current index number, the total number of book codes used up till now
            /// </summary>
            public ushort CurrentIndex
            {
                get { return _CurrentIndex; }
            }
        }






        // General Project info, to be displayed under PivotItem "Project Details"
        // Total time spent on this project
        string ProjectTimeSpent = "20:00";
        // Difficulty, general challenge when writing on a scale of 0 to 10, 0 being no effort and 10 being near impossible to completed with my current skill
        string ProjectChallenge = "5";
        // Date when this project was finished
        string ProjectDateFinished = "30/08/21";







        /// <summary>
        /// All the possible categories for books in the inventory
        /// </summary>
        private CategoryDictionary Categories;

        /// <summary>
        /// The inventory, the collection of books, of the library
        /// </summary>
        private List<Library_Book> Inventory;

        /// <summary>
        /// Returns a new, or available, book code. And stores any codes no longer in use. When a code is freed-up, because the book has been removed from the inventory, it has to be returned via ReturnCode(ushort code).
        /// </summary>
        private BookCodeGenerator CodeGenerator =  new BookCodeGenerator();
        
        /// <summary>
        /// Categories by which a book can be selected/searched for
        /// </summary>
        private enum SelectByCats { Code, Title };

        /// <summary>
        /// Possible states of the message that's displayed when trying the loan or return a book
        /// </summary>
        private enum DisplayMessageType { Normal, Error, Late };






        public Library()
        {
            this.InitializeComponent();

            // Generate some content
            GenerateCategories();
            GenerateInventory();

            FillSelectByComboBox();
            
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

            Categories.SetLateFee("Novel", 0.25, 2, 1, 0, 21);
            Categories.SetLateFee("Academic", 1, 0, 30, 1, 1);

        }


        /// <summary>
        /// Populate the list Inventory with some books, inspiration; https://en.wikipedia.org/wiki/List_of_best-selling_books
        /// </summary>
        private void GenerateInventory()
        {
            Inventory = new List<Library_Book>();

            AddBook("The Hobbit", "J.R.R. Tolkien", "Geroge Allen & Unwin Ltd.", 1957, 2002, 3, 9022532003, "Fantasy");
            AddBook("Harry Potter and the Philosopher's Stone", "J.K. Rowling", "Bloomsbury", 1997, 1997, 1, 0747532699, "Fantasy");
            AddBook("The Little Prince", "Antoine de Saint-Exupéry", "Reynal & Hitchcock", 1943, 2020, 9, 0, "Novella");
            AddBook("Dream of the Red Chamber", "Cao Xueqin", "N/A", 1750, 2020, 255, 0, "Novel");
            AddBook("And Then There Were None", "Agatha Christie", "Collins Crime Club", 1939, 2020, 0, 0, "Mystery");
            AddBook("The Lion, the Witch and the Wardrobe", "C.S. Lewis", "Geoffrey Bles", 1950, 2021, 54, 0, "Fantasy");
            AddBook("The Very Hungry Catepillar", "Eric Carle", "World Publishing Company", 1969, 2020, 99, 399226907, "Children");
            AddBook("Python Crash Course, 2nd Edition", "Eric Matthes", "William Pollock", 0, 2019, 6, 9781593279288, "Academic");
        }


        /// <summary>
        /// Fill the SelectBy ComboBox on the Library PivotItem with the categories specified in the SelectByCats enum
        /// </summary>
        private void FillSelectByComboBox()
        {
            foreach (string cat in Enum.GetNames(typeof(SelectByCats)))
            {
                SelectByComboBox.Items.Add(new ComboBoxItem() { Content = cat });
            }

            SelectByComboBox.SelectedItem = SelectByComboBox.Items.First();
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
            ushort catCode;
            if (Categories.TryGetKey(category, out catCode))
            {
                Inventory.Add(new Library_Book(title, author, publisher, catCode, CodeGenerator.GetCode(), firstPrintYear, printYear, printNumber, isbn, category));
            }
            else
            {
                Inventory.Add(new Library_Book(title, author, publisher, ushort.MaxValue, CodeGenerator.GetCode(), firstPrintYear, printYear, printNumber, isbn, "NotFound"));
            }
        }



        /// <summary>
        /// Loan out a book
        /// </summary>
        private void LoanOutBook()
        {
            string selectedBook = SelectedBookTextBox.Text;
            int selectBy = SelectByComboBox.SelectedIndex;

            DateTime date;
            if (BookDatePicker.SelectedDate == null)
            {
                date = DateTime.Now;
                BookDatePicker.Date = DateTime.Now;
            }
            else
            {
                date = BookDatePicker.Date.DateTime;
            }

                int index = -1;
            if (!TryGetBookIndex((SelectByCats)selectBy, selectedBook, out index))
            {
                // Error handled by TryGetBookIndex
            }
            else if (index == -1)
            {
                DisplayMessage("Book could not be found in inventory!", DisplayMessageType.Error);
            }

            else if (Inventory[index].IsOnLoan)
            {
                DisplayMessage("Book is already on loan.", DisplayMessageType.Error);
            }

            // Book was found succesfully
            else
            {
                // Debug.WriteLine($"Library: Book found");
                Inventory[index].LoanOutBook(date);
                DisplayMessage("Book loaned out succesfully.");
            }
        }


        /// <summary>
        /// Return a book
        /// </summary>
        private void ReturnBook()
        {
            string selectedBook = SelectedBookTextBox.Text;
            int selectBy = SelectByComboBox.SelectedIndex;

            DateTime date;
            if (BookDatePicker.SelectedDate == null)
            {
                date = DateTime.Now;
                BookDatePicker.Date = DateTime.Now;
            }
            else
            {
                date = BookDatePicker.Date.DateTime;
            }

            int index = -1;
            if (!TryGetBookIndex((SelectByCats)selectBy, selectedBook, out index))
            {
                // Error handled by TryGetBookIndex
            }
            else if (index == -1)
            {
                DisplayMessage("Book could not be found in inventory.", DisplayMessageType.Error);
            }

            else if (!Inventory[index].IsOnLoan)
            {
                DisplayMessage("Book is not on loan.", DisplayMessageType.Error);
            }

            // Book was found succesfully
            else
            {
                // Debug.WriteLine($"Library: Book found");
                DisplayMessage("Book Returned. Late fee = €" + Categories.GetLateFee(Inventory[index].PublicCategory, Inventory[index].LoanOutDate, date));
                Inventory[index].ReturnBook();
            }
        }


        /// <summary>
        /// Find the index of a book by a signature, like a title or code.
        /// </summary>
        /// <param name="cat">Category of the signature given</param>
        /// <param name="signature">Signature to search for</param>
        /// <param name="index">Index of the book, -1 if no book was found</param>
        /// <returns>True if search was completed, even when no book was found. False if search failed.</returns>
        private bool TryGetBookIndex(SelectByCats cat, string signature, out int index)
        {
            switch (cat)
            {
                case SelectByCats.Code:
                    {
                        ushort code;
                        if (ushort.TryParse(signature, out code))
                        {
                            index = GetBookIndexByCode(code);
                            return true;
                        }
                        else
                        {
                            DisplayMessage($"Failed to recognise a valid book code, make sure it is a number between 0 and {ushort.MaxValue}", DisplayMessageType.Error);
                            index = -1;
                            return false;
                        }
                    }
                case SelectByCats.Title:
                    {
                        index = GetBookIndexByTitle(signature);
                        return true;
                    }

                default:
                    {
                        Debug.WriteLine($"Library: GetBookIndex() does not have a case for {Enum.GetName(typeof(SelectByCats), cat)}");
                        index = -1;
                        return false;
                    }
            }
        }


        

        /// <summary>
        /// Get the index of a book by its code. Returns -1 if no book was found
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private int GetBookIndexByCode(ushort code)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                if (Inventory[i].Code == code)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the index of a book by its title. Returns -1 if no book was found
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private int GetBookIndexByTitle(string title)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                if (Inventory[i].Title == title)
                {
                    return i;
                }
            }
            return -1;
        }





        /// <summary>
        /// Verify if the information entered in the input fields in acceptable, display error messages to user if not
        /// </summary>
        /// <returns>True if input is acceptable, false if not</returns>
        private bool VerifyInputFields()
        {
            if (string.IsNullOrWhiteSpace(SelectedBookTextBox.Text))
            {
                DisplayMessage("No book was specified.", DisplayMessageType.Error);
                return false;
            }
            else if (SelectByComboBox.SelectedIndex < 0)
            {
                DisplayMessage("Please select a category by which to select a book.", DisplayMessageType.Error);
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Display a message to the user on the Library PivotItem
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        private void DisplayMessage(string message, DisplayMessageType type = DisplayMessageType.Normal)
        {
            switch (type)
            {
                case DisplayMessageType.Normal:
                    {
                        LoanOutReturnMessageTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
                        break;
                    }
                case DisplayMessageType.Error:
                    {
                        LoanOutReturnMessageTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;
                    }
                case DisplayMessageType.Late:
                    {
                        LoanOutReturnMessageTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Orange);
                        break;
                    }
                default:
                    {
                        Debug.WriteLine($"Library: DisplayMessage() case not recognised, {Enum.GetName(typeof(DisplayMessageType), type)}");
                        LoanOutReturnMessageTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Brown);
                        break;
                    }
            }

            LoanOutReturnMessageTextBlock.Text = message;
        }







        private void LoanOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (VerifyInputFields())
            {
                LoanOutBook();
            }
        }


        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (VerifyInputFields())
            {
                ReturnBook();
            }
        }


        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InventoryListView.ItemsSource = null;
            InventoryListView.ItemsSource = Inventory;
        }
    }
}
