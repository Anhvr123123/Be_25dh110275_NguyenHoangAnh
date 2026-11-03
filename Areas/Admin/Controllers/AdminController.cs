using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.admin.Controllers
{
    public class AdminController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Hàm hỗ trợ kiểm tra Session quyền Admin
        private bool CheckAdminSession()
        {
            var userSession = Session["User"] ?? Session["Account"] ?? Session["Username"];
            if (userSession == null && !User.Identity.IsAuthenticated)
            {
                return false;
            }
            return true;
        }

        // GET: admin/Admin/Index (Trang tổng quan Admin)
        public ActionResult Index()
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Thống kê dữ liệu cho Dashboard
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalCategories = db.Categories.Count();

            return View();
        }

        // GET: admin/Admin/Orders (Danh sách đơn hàng)
        public ActionResult Orders()
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var orders = db.Orders.Include(o => o.Customer)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();

            return View(orders);
        }

        // GET: admin/Admin/Details/5 (Chi tiết đơn hàng)
        public ActionResult Details(int? id)
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Include(o => o.Customer)
                                 .Include(o => o.OrderDetails.Select(od => od.Product))
                                 .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: admin/Admin/Edit/5 (Chỉnh sửa trạng thái đơn hàng)
        public ActionResult Edit(int? id)
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: admin/Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Orders");
            }

            return View(order);
        }

        // GET: admin/Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Include(o => o.Customer).SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: admin/Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var order = db.Orders.Find(id);
            if (order != null)
            {
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                foreach (var detail in orderDetails)
                {
                    db.OrderDetails.Remove(detail);
                }

                db.Orders.Remove(order);
                db.SaveChanges();
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