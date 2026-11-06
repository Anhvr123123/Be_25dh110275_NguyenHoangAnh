using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Be_25dh110275_NguyenHoangAnh.Models.ViewModel
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; } // Sửa lại thành get; set; chuẩn
        public int Quantity { get; set; }
        public string Category { get; set; }

        // Bổ sung thuộc tính tính thành tiền cho từng sản phẩm
        public decimal TotalPrice
        {
            get { return UnitPrice * Quantity; }
        }
    }
}