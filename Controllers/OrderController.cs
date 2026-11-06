using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var username = User.Identity.Name;
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalPrice(),
                CustomerID = customer != null ? customer.CustomerID : 0,
                CustomerName = customer != null ? customer.CustomerName : "",
                CustomerPhone = customer != null ? customer.CustomerPhone : "",
                CustomerEmail = customer != null ? customer.CustomerEmail : "",
                ShippingAddress = customer != null ? customer.CustomerAddress : "",
                OrderDate = DateTime.Now,
                PaymentMethod = "COD",
                PaymentStatus = "Chờ xử lý"
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var customer = db.Customers.SingleOrDefault(c => c.Username == username);

                Order order = new Order
                {
                    CustomerID = customer != null ? customer.CustomerID : model.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalPrice(),
                    PaymentStatus = model.PaymentMethod == "COD" ? "Thanh toán tiền mặt" : "Đã thanh toán",
                    AddressDelivery = model.ShippingAddress // Khớp chính xác với cột AddressDelivery trong DB
                };

                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var item in cart.Items)
                {
                    OrderDetail detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    db.OrderDetails.Add(detail);
                }
                db.SaveChanges();

                cart.Clear();
                Session["Cart"] = cart;

                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalPrice();
            return View(model);
        }

        // GET: Order/OrderSuccess
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        // GET: Order/OrderHistory
        [Authorize]
        public ActionResult OrderHistory(string statusSearch)
        {
            var username = User.Identity.Name;
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null) return RedirectToAction("Login", "Account");

            var orders = db.Orders.Include(o => o.OrderDetails).Where(o => o.CustomerID == customer.CustomerID);

            if (!string.IsNullOrEmpty(statusSearch))
            {
                orders = orders.Where(o => o.PaymentStatus.Contains(statusSearch));
            }

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        // GET: Order/OrderDetailHistory/5
        [Authorize]
        public ActionResult OrderDetailHistory(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();

            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}