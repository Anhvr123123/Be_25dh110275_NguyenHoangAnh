
using Be_25dh110275_NguyenHoangAnh.Models;
using Be_25dh110275_NguyenHoangAnh.Models.ViewModel;
using PagedList;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using Product = Be_25dh110275_NguyenHoangAnh.Models.Product;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // DANH SÁCH SẢN PHẨM
        public ActionResult Index(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            int? page)
        {
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDecription.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p =>
                    p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p =>
                    p.ProductPrice <= maxPrice.Value);
            }

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;

                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;

                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;

                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;

                default:
                    products = products.OrderByDescending(p => p.ProductID);
                    break;
            }

            int pageNumber = page ?? 1;
            int pageSize = 10;

            var model = new ProductSearchVM
            {
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                Products = products.ToPagedList(pageNumber, pageSize)
            };

            return View(model);
        }

        // THÊM SẢN PHẨM - GET
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName");

            return View();
        }

        // THÊM SẢN PHẨM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            Product product,
            HttpPostedFileBase UploadImage)
        {
            if (UploadImage != null && UploadImage.ContentLength > 0)
            {
                string imageName = SaveImage(UploadImage);

                if (imageName == null)
                {
                    ModelState.AddModelError(
                        "ProductImage",
                        "Ảnh không hợp lệ.");
                }
                else
                {
                    product.ProductImage = imageName;
                    ModelState.Remove("ProductImage");
                }
            }

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID);

            return View(product);
        }

        // SỬA SẢN PHẨM - GET
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id.Value);

            if (product == null)
                return HttpNotFound();

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID);

            return View(product);
        }

        // SỬA SẢN PHẨM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            Product product,
            HttpPostedFileBase UploadImage)
        {
            Product oldProduct = db.Products.Find(product.ProductID);

            if (oldProduct == null)
                return HttpNotFound();

            string imageName = product.ProductImage;

            if (UploadImage != null && UploadImage.ContentLength > 0)
            {
                imageName = SaveImage(UploadImage);

                if (imageName == null)
                {
                    ModelState.AddModelError(
                        "ProductImage",
                        "Ảnh không hợp lệ.");
                }
            }

            if (ModelState.IsValid)
            {
                oldProduct.CategoryID = product.CategoryID;
                oldProduct.ProductName = product.ProductName;
                oldProduct.ProductDecription = product.ProductDecription;
                oldProduct.ProductPrice = product.ProductPrice;

                if (!string.IsNullOrWhiteSpace(imageName))
                {
                    oldProduct.ProductImage = imageName;
                }

                db.SaveChanges();

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID);

            return View(product);
        }

        // CHI TIẾT SẢN PHẨM
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // XÓA SẢN PHẨM - GET
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // XÓA SẢN PHẨM - POST
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            try
            {
                db.Products.Remove(product);
                db.SaveChanges();

                TempData["Success"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Không thể xóa sản phẩm vì có dữ liệu liên quan.";
            }

            return RedirectToAction("Index");
        }

        // LƯU ẢNH
        private string SaveImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            if (file.ContentLength > 5 * 1024 * 1024)
                return null;

            string extension = Path.GetExtension(file.FileName).ToLower();

            string[] allowedExtensions =
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp"
            };

            if (!allowedExtensions.Contains(extension))
                return null;

            string folder = Server.MapPath("~/Images/products/");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString("N") + extension;

            string path = Path.Combine(folder, fileName);

            file.SaveAs(path);

            return fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
