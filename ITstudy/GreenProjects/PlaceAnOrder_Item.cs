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
    /// Item within the inventory of PlaceAnOrder 
    /// Contains a Name and item Codes
    /// </summary>
    public class PlaceAnOrder_Item
    {
        /// <summary>
        /// The code of an item in the inventory of PlaceAnOrder
        /// 
        /// </summary>
        public class ItemCode : IComparable
        {
            private string _Code;

            public ItemCode(int code)
            {
                _Code = code.ToString("D6");
            }

            public ItemCode(string code)
            {
                if (code.Length != 6)
                {
                    _Code = "ErrLen!";
                    Debug.WriteLine($"PlaceAnOrder_Item.ItemCode: constructor recieve code of invalid length, expected = 6, recieved = {code.Length}. code = {code}");
                }
                else
                {
                    bool allNumbers = true;
                    foreach (char c in code)
                    {
                        if (!char.IsDigit(c)) { allNumbers = false; }
                    }

                    if (allNumbers)
                    {
                        _Code = code;
                    }
                    else
                    {
                        _Code = "ErrNum";
                        Debug.WriteLine($"PlaceAnOrder_Item: PlaceAnOrder_Item(string name, string code) recieve code with invalid character(s), not all are decimal digits: code = {code}");
                    }
                }
            }

            public int CompareTo(object obj)
            {
                if (obj == null) { return 1; }
                if (obj != null)
                {
                    ItemCode otherItem = obj as ItemCode;
                    return this._Code.CompareTo(otherItem._Code);
                }
                else
                {
                    throw new ArgumentException("PlaceAnOrder_Item: Object is not a Year!");
                }
            }

            public string CodeString
            {
                get { return _Code; }
            }

            public int CodeInt
            {
                get { return int.Parse(_Code); }
            }

            public int Year
            {
                get { return int.Parse(_Code.Substring(0, 4)); }
            }

            public int TrackerCode
            {
                get { return int.Parse(_Code.Substring(4, 2)); }
            }
        }






        public string Name;
        public string CodeString;
        public string PriceString;

        public ItemCode Code = new ItemCode(-1);

        private double _Price = 0;


        





        public PlaceAnOrder_Item(string name, int year, int item, double price = 100d)
        {
            Name = name;

            year = Math.Clamp(year, 0, 9999);
            item = Math.Clamp(item, 0, 99);

            Code = new ItemCode(year * 100 + item);

            CodeString = Code.CodeString;

            _Price = Math.Round(price, 2);
            PriceString = _Price.ToString("N2");
        }

        public PlaceAnOrder_Item(string name, int code, double price)
        {
            Name = name;

            code = Math.Clamp(code, 0, 999999);
            Code = new ItemCode(code);
            CodeString = Code.CodeString;

            _Price = Math.Round(price, 2);
            PriceString = _Price.ToString("N2");
        }

        public PlaceAnOrder_Item(string name, string code, double price)
        {
            Name = name;

            Code = new ItemCode(code);
            CodeString = Code.CodeString;

            _Price = Math.Round(price, 2);
            PriceString = _Price.ToString("N2");        
        }




        public int CodeInt()
        {
            return Code.CodeInt;
        }

        public int Year()
        {
            return Code.Year;
        }

        public int TrackerCode()
        {
            return Code.TrackerCode;
        }

        public double Price()
        {
            return _Price;
        }



    }
}
