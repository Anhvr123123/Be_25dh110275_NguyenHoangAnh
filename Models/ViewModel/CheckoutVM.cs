using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Be_25dh110275_NguyenHoangAnh.Models.ViewModel
{
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }

        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }

        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string ShippingMethod { get; set; }
    }
}