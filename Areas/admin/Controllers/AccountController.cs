using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.admin.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: admin/Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: admin/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!";
                return View();
            }

            var adminUser = db.Users.FirstOrDefault(u => u.Username == username && u.Password == password && u.UserRole == "A");

            if (adminUser != null)
            {
                Session["User"] = adminUser;
                return RedirectToAction("Index", "Home", new { area = "admin" });
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu Admin không đúng!";
            return View();
        }

        // GET: admin/Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: admin/Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string Username, string Password)
        {
            try
            {
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!";
                    return View();
                }

                // Kiểm tra trùng lặp username
                var check = db.Users.FirstOrDefault(u => u.Username == Username);
                if (check != null)
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại trong hệ thống!";
                    return View();
                }

                // Sử dụng ADO.NET thuần để insert thẳng vào bảng User trong CSDL, bỏ qua hoàn toàn lỗi Validation của EF
                string connString = System.Configuration.ConfigurationManager.ConnectionStrings["MyStoreEntities"].ConnectionString;

                // Trích xuất chuỗi kết nối ADO.NET từ Entity Framework connection string nếu cần, hoặc dùng trực tiếp connection string của SQL
                // Hoặc lấy trực tiếp từ db.Database.Connection.ConnectionString:
                using (var conn = db.Database.Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO [User] (Username, Password, UserRole) VALUES (@username, @password, @role)";
                        cmd.Parameters.Add(new SqlParameter("@username", Username));
                        cmd.Parameters.Add(new SqlParameter("@password", Password));
                        cmd.Parameters.Add(new SqlParameter("@role", "A"));
                        cmd.ExecuteNonQuery();
                    }
                }

                // Đăng ký thành công -> Chuyển hướng về trang đăng nhập Admin
                return RedirectToAction("Login", "Account", new { area = "admin" });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View();
            }
        }

        // GET: admin/Account/Logout
        public ActionResult Logout()
        {
            Session["User"] = null;
            return RedirectToAction("Login", "Account", new { area = "admin" });
        }
    }
}