using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: admin/Orders/Index
        public ActionResult Index(string searchString, string status)
        {
            var orders = db.Orders.AsQueryable();

            // Tìm kiếm theo mã đơn hàng hoặc tên khách hàng (nếu có nhập)
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderID.ToString().Contains(searchString) || o.Customer.CustomerName.Contains(searchString));
            }

            // Lọc theo trạng thái thanh toán hoặc trạng thái đơn hàng
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.PaymentStatus == status);
            }

            var result = orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(result);
        }

        // GET: admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: admin/Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: admin/Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                var existingOrder = db.Orders.Find(order.OrderID);
                if (existingOrder != null)
                {
                    existingOrder.PaymentStatus = order.PaymentStatus;
                    // Cập nhật thêm các trường khác nếu cần thiết (ví dụ: trạng thái giao hàng)

                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: admin/Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);
            if (order != null)
            {
                // Xóa các chi tiết đơn hàng trước để tránh lỗi khóa ngoại (Foreign Key)
                db.OrderDetails.RemoveRange(order.OrderDetails);

                // Xóa đơn hàng
                db.Orders.Remove(order);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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