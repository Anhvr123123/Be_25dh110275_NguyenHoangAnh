using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.Items.Sum(x => x.TotalPrice)
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
                var cart = Session["Cart"] as List<CartItem>;
                if (cart == null || !cart.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }

                try
                {
                    // 1. Tạo đơn hàng mới khớp hoàn toàn với CSDL của bạn
                    Order order = new Order();
                    order.OrderDate = DateTime.Now;
                    order.PaymentStatus = "Chưa thanh toán";
                    order.AddressDelivery = model.CustomerAddress;

                    // SỬA TỪ ProductPrice THÀNH Price (hoặc UnitPrice tùy class CartItem)
                    order.TotalAmount = cart.Sum(x => x.Quantity * x.UnitPrice);

                    // Nếu người dùng đã đăng nhập, bạn có thể gán CustomerID từ Session hoặc User
                    var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                    if (user != null)
                    {
                        var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
                        if (customer != null)
                        {
                            order.CustomerID = customer.CustomerID;
                        }
                    }

                    db.Orders.Add(order);
                    db.SaveChanges(); // Lưu để sinh ra OrderID tự động tăng

                    // 2. Thêm chi tiết đơn hàng
                    foreach (var item in cart)
                    {
                        OrderDetail detail = new OrderDetail();
                        detail.OrderID = order.OrderID;
                        detail.ProductID = item.ProductID;
                        detail.Quantity = item.Quantity;

                        // SỬA TỪ ProductPrice THÀNH Price
                        detail.UnitPrice = item.UnitPrice;

                        db.OrderDetails.Add(detail);
                    }
                    db.SaveChanges();

                    // 3. Xóa giỏ hàng sau khi đặt thành công
                    Session["Cart"] = null;

                    // Chuyển hướng kèm theo ID đơn hàng vừa tạo để hiển thị trang thành công
                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thanh toán: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: Order/MyOrder
        [Authorize]
        public ActionResult MyOrder()
        {
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = db.Orders.Where(o => o.CustomerID == customer.CustomerID)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();

            return View(orders);
        }

        // GET: Order/OrderSuccess/5 (Hiển thị chi tiết đơn hàng thành công)
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }
    }
}