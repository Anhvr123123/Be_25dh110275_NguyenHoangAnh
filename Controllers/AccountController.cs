using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.Find(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                Be_25dh110275_NguyenHoangAnh.Models.User newUser = new Be_25dh110275_NguyenHoangAnh.Models.User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "C"
                };
                db.Users.Add(newUser);

                Customer customer = new Customer
                {
                    Username = model.Username,
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    CustomerAddress = model.CustomerAddress
                };
                db.Customers.Add(customer);

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"{validationError.PropertyName}: {validationError.ErrorMessage}");
                        }
                    }
                }
            }
            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    Session["Username"] = user.Username;
                    Session["Role"] = user.UserRole;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
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