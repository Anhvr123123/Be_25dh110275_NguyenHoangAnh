using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace Be_25dh110275_NguyenHoangAnh.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items => items;

        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;
        public IPagedList<Be_25dh110275_NguyenHoangAnh.Models.Product> SimilarProducts { get; set; }

        // Tổng tiền toàn giỏ hàng
        public decimal TotalPrice()
        {
            return items.Sum(i => i.UnitPrice * i.Quantity);
        }

        public void AddItem(int productId, string productImage, string productName, decimal unitPrice, int quantity, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);
            if (existingItem == null)
            {
                items.Add(new CartItem { ProductID = productId, ProductImage = productImage, ProductName = productName, UnitPrice = unitPrice, Quantity = quantity, Category = category });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public void Clear()
        {
            items.Clear();
        }

        // Cập nhật số lượng chuẩn xác
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    items.Remove(item);
                }
            }
        }
    }
}