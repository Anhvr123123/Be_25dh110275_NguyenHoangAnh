
using System;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Kiểm tra quyền Admin
        private bool CheckAdminSession()
        {
            if (Session["UserRole"] == null ||
                Session["AdminUsername"] == null)
            {
                return false;
            }

            string username = Session["AdminUsername"].ToString();

            if (Session["UserRole"].ToString() != "A")
            {
                return false;
            }

            // Kiểm tra tài khoản vẫn có quyền Admin trong CSDL
            return db.Users.Any(u =>
                u.Username == username &&
                u.UserRole == "A");
        }

        // Chuyển về đăng nhập Admin
        private ActionResult AdminLogin()
        {
            return RedirectToAction(
                "Login",
                "Account",
                new { area = "Admin" });
        }

        // GET: Admin/Admin/Index
        public ActionResult Index()
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalCategories = db.Categories.Count();

            return View();
        }

        // GET: Admin/Admin/Orders
        public ActionResult Orders()
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            var orders = db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Admin/Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id.Value);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Admin/Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Find(id.Value);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            int OrderID,
            string PaymentStatus,
            string AddressDelivery)
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            var order = db.Orders.Find(OrderID);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(PaymentStatus))
            {
                ModelState.AddModelError(
                    "PaymentStatus",
                    "Vui lòng chọn trạng thái thanh toán.");
            }

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            order.PaymentStatus = PaymentStatus;
            order.AddressDelivery = AddressDelivery;

            db.SaveChanges();

            TempData["Success"] =
                "Cập nhật đơn hàng thành công!";

            return RedirectToAction("Orders");
        }

        // GET: Admin/Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderID == id.Value);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/Admin/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!CheckAdminSession())
            {
                return AdminLogin();
            }

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.OrderDetails.RemoveRange(order.OrderDetails);

                    db.Orders.Remove(order);
                    db.SaveChanges();

                    transaction.Commit();

                    TempData["Success"] =
                        "Xóa đơn hàng thành công!";
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    TempData["Error"] =
                        "Không thể xóa đơn hàng!";
                }
            }

            return RedirectToAction("Orders");
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
