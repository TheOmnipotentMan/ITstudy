using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// TODO remove if the app is ever finished
using System.Diagnostics;




namespace ITstudy.GreenProjects
{

    /// <summary>
    /// Library book
    /// </summary>
    struct Library_Book
    {

        // Public values for UI list element, display purposes
        public string PublicTitle;
        public string PublicAuthor;
        public string PublicPublisher;
        public string PublicISBN;
        public string PublicCategory;
        public string PublicIsAvailable;
        public string PublicLoanDate;



        // Private values, actual data accessable through get methods
        private string _Title;
        private string _Author;
        private string _Publisher;

        private ushort _CategoryCode;

        private ushort _FirstPrintYear;
        private ushort _PrintYear;
        private byte _PrintNumber;

        private ulong _ISBN;

        private bool _IsOnLoan;
        private DateTime _LoanOutDate;

        private List<string> _SubCats;






        /// <summary>
        /// Create a new library book
        /// </summary>
        /// <param name="title"></param>
        /// <param name="author"></param>
        /// <param name="publisher"></param>
        /// <param name="firstPrintYear">The year this book was first printed & published</param>
        /// <param name="printNumber">This books printing run, eg 4th print</param>
        /// <param name="printYear">The year this version or print was made</param>
        /// <param name="isbn">International Standard Book Number</param>
        public Library_Book(string title, string author, string publisher, ushort firstPrintYear, ushort printYear, byte printNumber, ulong isbn, ushort genreCategory, string publicGenreCategory = "")
        {
            _Title = title;
            _Author = author;
            _Publisher = publisher;

            _CategoryCode = genreCategory;

            _FirstPrintYear = firstPrintYear;
            _PrintYear = printYear;
            _PrintNumber = printNumber;

            _ISBN = (isbn >= 1e14) ? (ulong)(isbn % 1e14) : isbn;

            _IsOnLoan = false;
            _LoanOutDate = DateTime.MaxValue;

            _SubCats = new List<string>();

            PublicTitle = _Title;
            PublicAuthor = _Author;
            PublicPublisher = _Publisher;
            PublicISBN = _ISBN.ToString("D13");
            PublicCategory = (publicGenreCategory == "") ? _CategoryCode.ToString() : publicGenreCategory;
            PublicIsAvailable = (_IsOnLoan) ? "On Loan" : "Available";
            PublicLoanDate = (_LoanOutDate == DateTime.MaxValue) ? "xx-xx-xxxx" : _LoanOutDate.Day.ToString("D2") + "-" + _LoanOutDate.Month.ToString("D2") + "-" + _LoanOutDate.Year.ToString("D4");
        }

        







        /// <summary>
        /// Loan out this book
        /// </summary>
        /// <param name="loanStartDate">The date on which this book is being loaned out</param>
        /// <returns>True if succesfull, false if not because book is already on loan</returns>
        public bool LoanOutBook(DateTime loanStartDate)
        {
            if (!_IsOnLoan)
            {
                _IsOnLoan = true;
                _LoanOutDate = loanStartDate;
                UpdatePublicLoanDate();
                return true;
            }
            else
            {
                return false;
            }            
        }

        /// <summary>
        /// Return a book from loan
        /// </summary>
        /// <returns>True if succesfull, false if not because book was not loaned out</returns>
        public bool ReturnBook()
        {
            if (_IsOnLoan)
            {
                _IsOnLoan = false;
                _LoanOutDate = DateTime.MaxValue;
                UpdatePublicLoanDate();
                return true;
            }
            else
            {
                return false;
            }
        }








        /// <summary>
        /// Update the PublicLoadDate to represent the actual _LoanOutDate
        /// </summary>
        private void UpdatePublicLoanDate()
        {
            PublicLoanDate = (_LoanOutDate == DateTime.MaxValue) ? "xx-xx-xxxx" : _LoanOutDate.Day.ToString("D2") + "-" + _LoanOutDate.Month.ToString("D2") + "-" + _LoanOutDate.Year.ToString("D4");
        }

        public void SetPublicCategory(string category)
        {
            PublicCategory = category;
        }

        
        /// <summary>
        /// Title of the book
        /// </summary>
        public string Title
        {
            get { return _Title; }
        }

        /// <summary>
        /// Author of the book
        /// </summary>
        public string Author
        {
            get { return _Author; }
        }

        /// <summary>
        /// Publisher of the book
        /// </summary>
        public string Publisher
        {
            get { return _Publisher; }
        }

        /// <summary>
        /// Genre or Category number
        /// </summary>
        public ushort GenreCategory
        {
            get { return _CategoryCode; }
        }

        /// <summary>
        /// Year the book was first printed
        /// </summary>
        public ushort FirstPrintYear
        {
            get { return _FirstPrintYear; }
        }

        /// <summary>
        /// Year this version of the book was printed
        /// </summary>
        public ushort PrintYear
        {
            get { return _PrintYear; }
        }

        /// <summary>
        /// Print number of this book
        /// </summary>
        public byte PrintNumber
        {
            get { return _PrintNumber; }
        }

        /// <summary>
        /// International Standard Book Number
        /// </summary>
        public ulong ISBN
        {
            get { return _ISBN; }
        }

        /// <summary>
        /// Is this book on loan or not
        /// </summary>
        public bool IsOnLoan
        {
            get { return _IsOnLoan; }
        }

        /// <summary>
        /// The date on which this book was loaned out. If book is not loaned out returns DateTiem.MaxValue
        /// </summary>
        public DateTime LoanOutDate
        {
            get { return _LoanOutDate; }
        }

        /// <summary>
        /// The list of sub-categories relevant to this book
        /// </summary>
        public List<String> SubCategories
        {
            get { return _SubCats; }
            set { _SubCats = new List<string>(value); }
        }

        /// <summary>
        /// Add a single sub-category to the list of sub-categories
        /// </summary>
        /// <param name="category">The sub-category to add</param>
        public void AddSubCategory(string category)
        {
            if (!_SubCats.Contains(category))
            {
                _SubCats.Add(category);
                SortSubCatsList();
            }            
        }

        /// <summary>
        /// Remove a sub-category from this book
        /// </summary>
        /// <param name="category">The sub-category to remove</param>
        public void RemoveSubCategory(string category)
        {
            for (int i = 0; i < _SubCats.Count; i++)
            {
                if (category == _SubCats[i])
                {
                    _SubCats.RemoveAt(i);
                    SortSubCatsList();
                    break;
                }
            }
        }

        /// <summary>
        /// Is a sub-category present for this book
        /// </summary>
        /// <param name="category">The sub-category to find</param>
        /// <returns>True is sub-cat is found, false if not</returns>
        public bool IsSubCategoryPresent(string category)
        {
            return _SubCats.Contains(category);
        }

        /// <summary>
        /// Sort the current _SubCats List with default sort()
        /// </summary>
        private void SortSubCatsList()
        {
            _SubCats.Sort();
        }
    }
}
