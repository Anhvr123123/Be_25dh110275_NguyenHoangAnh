
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // ==========================================
        // LẤY USERNAME ĐANG ĐĂNG NHẬP
        // ==========================================
        private string CurrentUsername()
        {
            return Convert.ToString(Session["Username"]);
        }

        // ==========================================
        // LẤY THÔNG TIN KHÁCH HÀNG
        // ==========================================
        private Customer CurrentCustomer()
        {
            string username = CurrentUsername();

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return db.Customers.FirstOrDefault(c =>
                c.Username == username);
        }

        // ==========================================
        // CHUYỂN ĐẾN TRANG ĐĂNG NHẬP
        // ==========================================
        private ActionResult RedirectToLogin(string actionName)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    area = "",
                    returnUrl = Url.Action(
                        actionName,
                        "Order",
                        new { area = "" })
                });
        }

        // ==========================================
        // GET: ORDER/CHECKOUT
        // HIỂN THỊ TRANG THANH TOÁN
        // ==========================================
        [HttpGet]
        public ActionResult Checkout()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
            {
                return RedirectToLogin("Checkout");
            }

            var cart = Session["Cart"] as Cart;

            if (cart == null ||
                cart.Items == null ||
                !cart.Items.Any())
            {
                TempData["Message"] =
                    "Giỏ hàng của bạn đang trống.";

                return RedirectToAction(
                    "Index2",
                    "Cart",
                    new { area = "" });
            }

            var customer = CurrentCustomer();

            if (customer == null)
            {
                TempData["Message"] =
                    "Không tìm thấy thông tin khách hàng.";

                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "" });
            }

            var model = new CheckoutVM
            {
                CustomerID = customer.CustomerID,

                CustomerName = customer.CustomerName,

                CustomerPhone = customer.CustomerPhone,

                CustomerEmail = customer.CustomerEmail,

                ShippingAddress = customer.CustomerAddress,

                CartItems = cart.Items.ToList(),

                TotalAmount = cart.TotalPrice(),

                OrderDate = DateTime.Now,

                PaymentMethod = "COD",

                PaymentStatus = "Chưa thanh toán"
            };

            return View(model);
        }

        // ==========================================
        // POST: ORDER/CHECKOUT
        // XÁC NHẬN ĐẶT HÀNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            // Kiểm tra đăng nhập
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
            {
                return RedirectToLogin("Checkout");
            }

            // Kiểm tra giỏ hàng
            var cart = Session["Cart"] as Cart;

            if (cart == null ||
                cart.Items == null ||
                !cart.Items.Any())
            {
                TempData["Message"] =
                    "Giỏ hàng của bạn đang trống.";

                return RedirectToAction(
                    "Index2",
                    "Cart",
                    new { area = "" });
            }

            // Lấy khách hàng
            var customer = CurrentCustomer();

            if (customer == null)
            {
                TempData["Message"] =
                    "Không tìm thấy thông tin khách hàng.";

                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "" });
            }

            if (model == null)
            {
                model = new CheckoutVM();
            }

            // ======================================
            // DỮ LIỆU LẤY TỪ SERVER
            // ======================================

            model.CustomerID = customer.CustomerID;
            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalPrice();
            model.OrderDate = DateTime.Now;

            ModelState.Remove("CustomerID");
            ModelState.Remove("CartItems");
            ModelState.Remove("TotalAmount");
            ModelState.Remove("OrderDate");
            ModelState.Remove("PaymentStatus");

            // ======================================
            // KIỂM TRA THÔNG TIN GIAO HÀNG
            // ======================================

            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                ModelState.AddModelError(
                    "CustomerName",
                    "Vui lòng nhập họ tên người nhận.");
            }

            if (string.IsNullOrWhiteSpace(model.CustomerPhone))
            {
                ModelState.AddModelError(
                    "CustomerPhone",
                    "Vui lòng nhập số điện thoại.");
            }

            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                ModelState.AddModelError(
                    "ShippingAddress",
                    "Vui lòng nhập địa chỉ giao hàng.");
            }

            // ======================================
            // KIỂM TRA PHƯƠNG THỨC THANH TOÁN
            // ======================================

            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Vui lòng chọn phương thức thanh toán.");
            }
            else if (model.PaymentMethod != "COD" &&
                     model.PaymentMethod != "BANK")
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Phương thức thanh toán không hợp lệ.");
            }

            // ======================================
            // XÁC ĐỊNH TRẠNG THÁI THANH TOÁN
            // ======================================

            if (model.PaymentMethod == "BANK")
            {
                model.PaymentStatus = "Chờ chuyển khoản";
            }
            else
            {
                model.PaymentStatus = "Chưa thanh toán";
            }

            // ======================================
            // KIỂM TRA GIỎ HÀNG
            // ======================================

            if (cart.Items.Any(i => i.Quantity <= 0))
            {
                ModelState.AddModelError(
                    "",
                    "Số lượng sản phẩm không hợp lệ.");
            }

            if (cart.TotalPrice() <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Tổng tiền đơn hàng không hợp lệ.");
            }

            // Nếu có lỗi, hiển thị lại form
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ======================================
            // LƯU ĐƠN HÀNG
            // ======================================

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    var order = new Order
                    {
                        CustomerID = customer.CustomerID,

                        OrderDate = DateTime.Now,

                        TotalAmount = cart.TotalPrice(),

                        PaymentStatus = model.PaymentStatus,

                        AddressDelivery =
                            model.ShippingAddress.Trim()
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    int orderId = order.OrderID;

                    // ==================================
                    // LƯU CHI TIẾT ĐƠN HÀNG
                    // ==================================

                    foreach (var item in cart.Items)
                    {
                        var detail = new OrderDetail
                        {
                            OrderID = orderId,

                            ProductID = item.ProductID,

                            Quantity = item.Quantity,

                            UnitPrice = item.UnitPrice
                        };

                        db.OrderDetails.Add(detail);
                    }

                    db.SaveChanges();

                    transaction.Commit();

                    // Xóa giỏ hàng
                    Session["Cart"] = null;

                    if (model.PaymentMethod == "BANK")
                    {
                        TempData["Success"] =
                            "Đặt hàng thành công. " +
                            "Đơn hàng đang chờ chuyển khoản.";
                    }
                    else
                    {
                        TempData["Success"] =
                            "Đặt hàng thành công. " +
                            "Bạn sẽ thanh toán khi nhận hàng.";
                    }

                    // Chuyển đến trang thành công
                    return RedirectToAction(
                        "OrderSuccess",
                        "Order",
                        new
                        {
                            area = "",
                            id = orderId
                        });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(
                        "Lỗi đặt hàng: " + ex.ToString());

                    ModelState.AddModelError(
                        "",
                        "Đặt hàng thất bại. " +
                        "Vui lòng kiểm tra và thử lại.");
                }
            }

            return View(model);
        }

        // ==========================================
        // GET: ORDER/ORDERSUCCESS
        // TRANG ĐẶT HÀNG THÀNH CÔNG
        // ==========================================
        [HttpGet]
        public ActionResult OrderSuccess(int id)
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
            {
                return RedirectToLogin("OrderHistory");
            }

            var customer = CurrentCustomer();

            if (customer == null)
            {
                return HttpNotFound();
            }

            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(o =>
                    o.OrderID == id &&
                    o.CustomerID == customer.CustomerID);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // ==========================================
        // GET: ORDER/ORDERHISTORY
        // LỊCH SỬ ĐƠN HÀNG
        // ==========================================
        [HttpGet]
        public ActionResult OrderHistory(string statusSearch)
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
            {
                return RedirectToLogin("OrderHistory");
            }

            var customer = CurrentCustomer();

            if (customer == null)
            {
                TempData["Message"] =
                    "Không tìm thấy thông tin khách hàng.";

                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "" });
            }

            var orders = db.Orders
                .Include(o => o.OrderDetails)
                .Where(o =>
                    o.CustomerID == customer.CustomerID);

            // Lọc theo trạng thái thanh toán
            if (!string.IsNullOrWhiteSpace(statusSearch))
            {
                orders = orders.Where(o =>
                    o.PaymentStatus.Contains(statusSearch));
            }

            return View(
                orders
                    .OrderByDescending(o => o.OrderDate)
                    .ToList());
        }

        // ==========================================
        // GET: ORDER/ORDERDETAILHISTORY
        // CHI TIẾT ĐƠN HÀNG
        // ==========================================
        [HttpGet]
        public ActionResult OrderDetailHistory(int id)
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
            {
                return RedirectToLogin("OrderHistory");
            }

            var customer = CurrentCustomer();

            if (customer == null)
            {
                return HttpNotFound();
            }

            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(o =>
                    o.OrderID == id &&
                    o.CustomerID == customer.CustomerID);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // ==========================================
        // GIẢI PHÓNG DATABASE
        // ==========================================
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
