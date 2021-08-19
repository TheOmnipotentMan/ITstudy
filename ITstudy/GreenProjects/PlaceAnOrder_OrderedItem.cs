using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITstudy.GreenProjects
{
     /// <summary>
     /// An item that has been ordered from the inventory of PlaceAnOrder
     /// </summary>
    class PlaceAnOrder_OrderedItem
    {
        public string Name = "EMPTY";
        public string Code = "EMPTY";
        public string BasePrice = "EMPTY";
        public string Discount = "EMPTY";
        public string Price = "EMPTY";

        public PlaceAnOrder_OrderedItem() { }

        public PlaceAnOrder_OrderedItem(string name, string code, string basePrice, string discount, string price)
        {
            Name = name;
            Code = code;
            BasePrice = basePrice;
            Discount = discount;
            Price = price;
        }
    }
}
