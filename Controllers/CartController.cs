using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Lấy giỏ hàng từ Session (Đảm bảo khởi tạo và lưu trữ chuẩn xác)
        private Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // GET: Cart/Index2
        public ActionResult Index2(int? page)
        {
            var cart = GetCart();
            var products = db.Products.ToList();
            var similarProducts = new List<Be_25dh110275_NguyenHoangAnh.Models.Product>();

            if (cart.Items != null && cart.Items.Any())
            {
                similarProducts = products.Where(p => cart.Items.Any(ci => ci.Category == p.Category.CategoryName)
                    && !cart.Items.Any(ci => ci.ProductID == p.ProductID)).ToList();
            }

            int pageNumber = page ?? 1;
            int pageSize = cart.PageSize;

            cart.SimilarProducts = similarProducts.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);
            return View(cart);
        }

        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                var cart = GetCart();
                cart.AddItem(
                    product.ProductID,
                    product.ProductImage,
                    product.ProductName,
                    product.ProductPrice,
                    quantity,
                    product.Category != null ? product.Category.CategoryName : "Khác"
                );
                Session["Cart"] = cart; // Lưu lại session
            }
            return RedirectToAction("Index2");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            cart.RemoveItem(id);
            Session["Cart"] = cart; // Lưu lại session
            return RedirectToAction("Index2");
        }

        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            return RedirectToAction("Index2");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                Session["Cart"] = cart; // Bắt buộc gán lại Session để lưu số lượng mới
            }
            return RedirectToAction("Index2");
        }
    }
}