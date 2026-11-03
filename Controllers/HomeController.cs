
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();


        public ActionResult Index(string searchTerm)
        {
            using (var db = new MyStoreEntities())
            {
                var query = db.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.ProductName.Contains(searchTerm));
                }

                ViewBag.FeaturedProducts = query
                    .OrderByDescending(p => p.ProductID)
                    .Take(8)
                    .ToList();

                ViewBag.SearchTerm = searchTerm;
            }

            return View();
        }

        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var categories = db.Categories.ToList();
            return PartialView("CategoryMenuPV", categories);
        }

        public ActionResult ProductList(int? categoryId, int? page)
        {
            int pageNumber = page ?? 1;

            if (pageNumber < 1)
                pageNumber = 1;

            int pageSize = 8;

            var products = db.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                var category = db.Categories.Find(categoryId.Value);

                if (category == null)
                    return HttpNotFound();

                products = products.Where(p =>
                    p.CategoryID == categoryId.Value);

                ViewBag.CategoryName = category.CategoryName;
            }
            else
            {
                ViewBag.CategoryName = "Tất cả sản phẩm";
            }

            ViewBag.CategoryId = categoryId;

            var result = products
                .OrderBy(p => p.ProductID)
                .ToPagedList(pageNumber, pageSize);

            return View(result);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Be_25dh110275_NguyenHoangAnh.Models.Product
                product = db.Products.Find(id.Value);

            if (product == null)
                return HttpNotFound();

            var relatedProducts = db.Products
                .Where(p =>
                    p.CategoryID == product.CategoryID &&
                    p.ProductID != product.ProductID)
                .OrderBy(p => p.ProductID)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
