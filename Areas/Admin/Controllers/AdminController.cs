using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Admin/Index
        public ActionResult Index()
        {
            // Thống kê dữ liệu cho Dashboard
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalCategories = db.Categories.Count();

            return View();
        }
    }
}