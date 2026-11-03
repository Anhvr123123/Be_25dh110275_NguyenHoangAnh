
using System;
using System.Linq;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private bool IsAdmin()
        {
            return Session["UserRole"] != null &&
                   Session["UserRole"].ToString() == "A";
        }

        // ĐĂNG NHẬP - GET
        [HttpGet]
        public ActionResult Login()
        {
            if (IsAdmin())
            {
                return RedirectToAction(
                    "Index", "Home",
                    new { area = "Admin" });
            }

            return View();
        }

        // ĐĂNG NHẬP - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";

                return View();
            }

            var adminUser = db.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password &&
                u.UserRole == "A");

            if (adminUser == null)
            {
                ViewBag.Error =
                    "Tên đăng nhập hoặc mật khẩu Admin không đúng!";

                return View();
            }

            Session["User"] = adminUser;
            Session["Username"] = adminUser.Username;
            Session["UserRole"] = "A";

            return RedirectToAction(
                "Index", "Home",
                new { area = "Admin" });
        }

        // ĐĂNG KÝ - GET
        [HttpGet]
        public ActionResult Register()
        {
            if (db.Users.Any(u => u.UserRole == "A") &&
                !IsAdmin())
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // ĐĂNG KÝ - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            string Username,
            string Password,
            string ConfirmPassword)
        {
            if (db.Users.Any(u => u.UserRole == "A") &&
                !IsAdmin())
            {
                ViewBag.Error =
                    "Bạn không có quyền tạo tài khoản Admin.";

                return View();
            }

            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            if (db.Users.Any(u => u.Username == Username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            var newAdmin = new User
            {
                Username = Username,
                Password = Password,
                UserRole = "A"
            };

            db.Users.Add(newAdmin);
            db.SaveChanges();

            TempData["Success"] =
                "Tạo tài khoản Admin thành công!";

            return RedirectToAction("Login");
        }

        // THÔNG TIN TÀI KHOẢN

        [HttpGet]
        public new ActionResult Profile()

        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            string username = Session["Username"] == null
                ? ""
                : Session["Username"].ToString();

            var admin = db.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.UserRole == "A");

            if (admin == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(admin);
        }

        // ĐĂNG XUẤT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Login", "Account",
                new { area = "Admin" });
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
