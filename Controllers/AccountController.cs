
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;

using StoreUser =
    Be_25dh110275_NguyenHoangAnh.Models.User;

using StoreCustomer =
    Be_25dh110275_NguyenHoangAnh.Models.Customer;

using StoreDB =
    Be_25dh110275_NguyenHoangAnh.Models.MyStoreEntities;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class AccountController : Controller
    {
        private StoreDB db = new StoreDB();

        // Lấy tên tài khoản đang đăng nhập
        private string CurrentUsername()
        {
            return Convert.ToString(Session["Username"]);
        }

        // Lấy thông tin khách hàng
        private StoreCustomer CurrentCustomer()
        {
            string username = CurrentUsername();

            if (string.IsNullOrWhiteSpace(username))
                return null;

            return db.Customers.FirstOrDefault(c =>
                c.Username == username);
        }

        // Chuyển đến đăng nhập khi chưa có Session
        private ActionResult RequireLogin()
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    area = "",
                    returnUrl = Request.RawUrl
                });
        }

        // ==========================================
        // TRANG TÀI KHOẢN
        // ==========================================

        public ActionResult Index()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUsername()))
            {
                return RedirectToAction("ProfileInfo");
            }

            return RedirectToAction("Login");
        }

        // ==========================================
        // ĐĂNG KÝ
        // ==========================================

        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError(
                    "Username",
                    "Tên đăng nhập đã tồn tại.");

                return View(model);
            }

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    var user = new StoreUser
                    {
                        Username = model.Username,
                        Password = model.Password,
                        UserRole = "C"
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    var customer = new StoreCustomer
                    {
                        Username = model.Username,
                        CustomerName = model.CustomerName,
                        CustomerPhone = model.CustomerPhone,
                        CustomerEmail = model.CustomerEmail,
                        CustomerAddress = model.CustomerAddress
                    };

                    db.Customers.Add(customer);
                    db.SaveChanges();

                    transaction.Commit();

                    Session["Username"] = user.Username;
                    Session["UserRole"] = "C";

                    FormsAuthentication.SetAuthCookie(
                        user.Username, false);

                    TempData["Success"] =
                        "Đăng ký tài khoản thành công!";

                    return RedirectToAction(
                        "Index", "Home",
                        new { area = "" });
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "",
                        "Đăng ký thất bại. Vui lòng thử lại.");

                    return View(model);
                }
            }
        }

        // ==========================================
        // ĐĂNG NHẬP
        // ==========================================

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            LoginVM model,
            string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u =>
                u.Username == model.Username &&
                u.Password == model.Password &&
                u.UserRole == "C");

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }

            Session["Username"] = user.Username;
            Session["UserRole"] = "C";

            FormsAuthentication.SetAuthCookie(
                user.Username, false);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(
                "Index", "Home",
                new { area = "" });
        }

        // ==========================================
        // CHUYỂN HƯỚNG PROFILE
        // ==========================================

        [HttpGet]
        [ActionName("Profile")]
        public ActionResult CustomerProfile()
        {
            return RedirectToAction("ProfileInfo");
        }

        // ==========================================
        // THÔNG TIN TÀI KHOẢN
        // ==========================================

        [HttpGet]
        public ActionResult ProfileInfo()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
                return RequireLogin();

            var customer = CurrentCustomer();

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // ==========================================
        // CẬP NHẬT THÔNG TIN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(
            string CustomerPhone,
            string CustomerEmail,
            string CustomerAddress)
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
                return RequireLogin();

            var customer = CurrentCustomer();

            if (customer == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(CustomerPhone))
            {
                TempData["Error"] =
                    "Vui lòng nhập số điện thoại.";

                return RedirectToAction("ProfileInfo");
            }

            if (CustomerPhone.Trim().Length > 15)
            {
                TempData["Error"] =
                    "Số điện thoại không được quá 15 ký tự.";

                return RedirectToAction("ProfileInfo");
            }

            customer.CustomerPhone = CustomerPhone.Trim();

            customer.CustomerEmail =
                string.IsNullOrWhiteSpace(CustomerEmail)
                    ? null
                    : CustomerEmail.Trim();

            customer.CustomerAddress =
                string.IsNullOrWhiteSpace(CustomerAddress)
                    ? null
                    : CustomerAddress.Trim();

            try
            {
                db.SaveChanges();

                TempData["Success"] =
                    "Cập nhật thông tin tài khoản thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Không thể lưu thông tin. Vui lòng thử lại.";
            }

            return RedirectToAction("ProfileInfo");
        }

        // ==========================================
        // GET: ĐỔI MẬT KHẨU
        // ==========================================

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername()))
                return RequireLogin();

            return View();
        }

        // ==========================================
        // POST: ĐỔI MẬT KHẨU
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(
            string oldPassword,
            string newPassword,
            string confirmPassword)
        {
            string username = CurrentUsername();

            if (string.IsNullOrWhiteSpace(username))
            {
                return RequireLogin();
            }

            var user = db.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.UserRole == "C");

            if (user == null)
                return HttpNotFound();

            // Kiểm tra mật khẩu cũ
            if (string.IsNullOrWhiteSpace(oldPassword) ||
                (user.Password ?? "").TrimEnd() != oldPassword)
            {
                ViewBag.Error =
                    "Mật khẩu cũ không chính xác.";

                return View();
            }

            // Kiểm tra mật khẩu mới
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error =
                    "Vui lòng nhập mật khẩu mới.";

                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error =
                    "Mật khẩu mới phải có ít nhất 6 ký tự.";

                return View();
            }

            // Kiểm tra xác nhận mật khẩu
            if (newPassword != confirmPassword)
            {
                ViewBag.Error =
                    "Xác nhận mật khẩu mới không khớp.";

                return View();
            }

            // Không cho dùng lại mật khẩu cũ
            if (newPassword == oldPassword)
            {
                ViewBag.Error =
                    "Mật khẩu mới phải khác mật khẩu cũ.";

                return View();
            }

            // Lưu mật khẩu mới
            try
            {
                user.Password = newPassword;
                db.SaveChanges();

                TempData["Success"] =
                    "Đổi mật khẩu thành công.";

                // Quay về thông tin tài khoản
                return RedirectToAction(
                    "ProfileInfo",
                    "Account",
                    new { area = "" });
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "Không thể cập nhật mật khẩu. Vui lòng thử lại.";

                return View();
            }
        }

        // ==========================================
        // ĐĂNG XUẤT
        // ==========================================

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Index", "Home",
                new { area = "" });
        }

        // GIẢI PHÓNG DATABASE
        
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
