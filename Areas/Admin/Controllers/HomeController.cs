using Be_25dh110275_NguyenHoangAnh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home/Index (Trang quản trị Admin)
        public ActionResult Index()
        {
            // Thống kê số lượng khớp hoàn toàn với View Dashboard Admin
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalOrders = db.Orders.Count();

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}