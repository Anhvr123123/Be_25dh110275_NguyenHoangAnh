
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home/Index
        public ActionResult Index()
        {
            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "A")
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "Admin" });
            }

            ViewBag.AdminUsername = Session["AdminUsername"];

            return View();
        }
    }
}
