using Be_25dh110275_NguyenHoangAnh.Models; // Đảm bảo đúng namespace chứa Model và DbContext của bạn
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Controllers
{
    public class ProductsController : Controller
    {
        // Khởi tạo DbContext kết nối cơ sở dữ liệu (thay MyDbContext bằng tên DbContext thực tế của bạn nếu khác)
        // Thay MyDbContext bằng tên DbContext thực tế tìm được ở trên
        private MyStoreEntities db = new MyStoreEntities();
        // GET: Products/Index
        public ActionResult Index()
        {
            // Lấy danh sách sản phẩm kèm theo thông tin bảng Category (Eager Loading)
            var products = db.Products.Include("Category").ToList();
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                    ImageFile.SaveAs(path);
                    product.ProductImage = fileName;
                }
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }
        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Lấy sản phẩm liên quan (nếu muốn)
            var relatedProducts = db.Products
                                    .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                                    .Take(4)
                                    .ToList();
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }
    }
}