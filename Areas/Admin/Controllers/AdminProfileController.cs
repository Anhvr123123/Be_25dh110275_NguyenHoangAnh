
using System;
using System.Linq;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class AdminProfileController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private bool CheckAdminSession()
        {
            return Session["UserRole"] != null &&
                   Session["UserRole"].ToString().Trim() == "A" &&
                   Session["Username"] != null;
        }

        // GET: Admin/AdminProfile/Index
        public ActionResult Index()
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction(
                    "Login", "Account",
                    new { area = "Admin" });
            }

            string username = Session["Username"].ToString();

            var admin = db.Users.FirstOrDefault(u =>
                u.Username == username && u.UserRole == "A");

            if (admin == null)
            {
                return HttpNotFound();
            }

            return View(admin);
        }

        // GET: Admin/AdminProfile/Edit
        public ActionResult Edit()
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction(
                    "Login", "Account",
                    new { area = "Admin" });
            }

            string username = Session["Username"].ToString();

            var admin = db.Users.FirstOrDefault(u =>
                u.Username == username && u.UserRole == "A");

            if (admin == null)
            {
                return HttpNotFound();
            }

            return View(admin);
        }

        // POST: Admin/AdminProfile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            string CurrentPassword,
            string NewPassword,
            string ConfirmPassword)
        {
            if (!CheckAdminSession())
            {
                return RedirectToAction(
                    "Login", "Account",
                    new { area = "Admin" });
            }

            string username = Session["Username"].ToString();

            var admin = db.Users.FirstOrDefault(u =>
                u.Username == username && u.UserRole == "A");

            if (admin == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ViewBag.Error =
                    "Vui lòng nhập mật khẩu hiện tại!";
                return View(admin);
            }

            if (admin.Password == null ||
                admin.Password.Trim() != CurrentPassword)
            {
                ViewBag.Error =
                    "Mật khẩu hiện tại không chính xác!";
                return View(admin);
            }

            if (string.IsNullOrWhiteSpace(NewPassword) ||
                NewPassword.Length < 6)
            {
                ViewBag.Error =
                    "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return View(admin);
            }

            if (NewPassword.Length > 50)
            {
                ViewBag.Error =
                    "Mật khẩu không được quá 50 ký tự!";
                return View(admin);
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Error =
                    "Xác nhận mật khẩu không khớp!";
                return View(admin);
            }

            admin.Password = NewPassword;

            db.SaveChanges();

            TempData["Success"] =
                "Cập nhật tài khoản Admin thành công!";

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
