using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Be_25dh110275_NguyenHoangAnh.Models.ViewModel
{
    public class CheckoutVM
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }
        public string CustomerAddress { get; set; }

        public List<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}