
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;
using StoreProduct = Be_25dh110275_NguyenHoangAnh.Models.Product;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private Cart GetCart()
        {
            var cart = Session["Cart"] as Cart;

            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }

            return cart;
        }

        // GET: Cart
        public ActionResult Index()
        {
            return RedirectToAction("Index2");
        }

        // GET: Cart/Index2
        [HttpGet]
        public ActionResult Index2(int? page)
        {
            var cart = GetCart();

            var similarProducts = new List<StoreProduct>();

            if (cart.Items != null && cart.Items.Any())
            {
                var categoryNames = cart.Items
                    .Select(i => i.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

                var cartProductIds = cart.Items
                    .Select(i => i.ProductID)
                    .ToList();

                similarProducts = db.Products
                    .Include("Category")
                    .Where(p =>
                        !cartProductIds.Contains(p.ProductID) &&
                        p.Category != null &&
                        categoryNames.Contains(
                            p.Category.CategoryName))
                    .OrderBy(p => p.ProductID)
                    .ToList();
            }

            int pageNumber = page.GetValueOrDefault(1);

            if (pageNumber < 1)
                pageNumber = 1;

            int pageSize = cart.PageSize;

            if (pageSize < 1)
                pageSize = 8;

            cart.SimilarProducts = similarProducts
                .ToPagedList(pageNumber, pageSize);

            return View(cart);
        }

        // GET: Cart/AddToCart
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products
                .Include("Category")
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
                return HttpNotFound();

            if (quantity < 1)
                quantity = 1;

            var cart = GetCart();

            string categoryName = "Khác";

            if (product.Category != null)
                categoryName = product.Category.CategoryName;

            cart.AddItem(
                product.ProductID,
                product.ProductImage,
                product.ProductName,
                product.ProductPrice,
                quantity,
                categoryName
            );

            Session["Cart"] = cart;

            return RedirectToAction("Index2");
        }

        // GET: Cart/RemoveFromCart
        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();

            cart.RemoveItem(id);

            Session["Cart"] = cart;

            return RedirectToAction("Index2");
        }

        // GET: Cart/ClearCart
        public ActionResult ClearCart()
        {
            Session["Cart"] = null;

            return RedirectToAction("Index2");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();

            if (quantity < 1)
                quantity = 1;

            cart.UpdateQuantity(id, quantity);

            Session["Cart"] = cart;

            return RedirectToAction("Index2");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
