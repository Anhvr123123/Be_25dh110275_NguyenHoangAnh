using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm sản phẩm theo từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDecription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            int pageNumber = page ?? 1;
            int pageSize = 6; // Số sản phẩm mỗi trang

            // Lấy danh sách dạng LINQ thô rồi Select sang đúng kiểu Product của Database
            model.FeaturedProducts = products.OrderByDescending(p => p.ProductID)
                                             .Take(10)
                                             .ToList();

            var listNew = products.OrderBy(p => p.ProductID).ToList();
            model.NewProducts = listNew.ToPagedList(pageNumber, pageSize);

            return View(model);
        }
        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Chỉ định rõ namespace của Model database
            Be_25dh110275_NguyenHoangAnh.Models.Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách sản phẩm liên quan cùng danh mục
            var relatedProducts = db.Products
                                    .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                                    .Take(4)
                                    .ToList();
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
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