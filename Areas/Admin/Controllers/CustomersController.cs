using Be_25dh110275_NguyenHoangAnh.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index(string searchTerm)
        {
            var customers = db.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm) || c.CustomerEmail.Contains(searchTerm) || c.Username.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;
            return View(customers.ToList());
        }

        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Admin/Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,CustomerName,CustomerEmail,CustomerPhone,CustomerAddress")] Customer customer, string Password)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ModelState.AddModelError("", "Lỗi dữ liệu đầu vào: " + string.Join(" | ", errors));
                return View(customer);
            }

            try
            {
                string uName = customer.Username;

                // 1. Kiểm tra xem Username đã tồn tại trong bảng User chưa (Dùng FirstOrDefault để khớp kiểu chuỗi)
                var existingUser = db.Users.FirstOrDefault(u => u.Username == uName);
                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        Username = customer.Username,
                        Password = !string.IsNullOrEmpty(Password) ? Password : "123",
                        UserRole = "C"
                    };
                    db.Users.Add(newUser);
                    db.SaveChanges(); // Lưu User trước
                }

                // 2. Kiểm tra xem Username đã tồn tại trong bảng Customer chưa (Tránh trùng lặp)
                if (db.Customers.Any(c => c.Username == uName))
                {
                    ModelState.AddModelError("", "Tên đăng nhập này đã tồn tại trong danh sách khách hàng!");
                    return View(customer);
                }

                // 3. Lưu Customer
                db.Customers.Add(customer);
                db.SaveChanges(); // Lưu Customer sau

                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"{x.PropertyName}: {x.ErrorMessage}"));

                ModelState.AddModelError("", "Lỗi Validation Database: " + errorMessages);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.GetBaseException().Message);
            }

            return View(customer);
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,CustomerName,CustomerEmail,CustomerPhone,CustomerAddress,Username")] Customer customer, string Password)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;

                if (!string.IsNullOrEmpty(Password))
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == customer.Username);
                    if (user != null)
                    {
                        user.Password = Password;
                        db.Entry(user).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Customer customer = db.Customers.Find(id);
                db.Customers.Remove(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Không thể xóa khách hàng này vì có dữ liệu liên quan! Lỗi: " + ex.Message);
                Customer customer = db.Customers.Find(id);
                return View(customer);
            }
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