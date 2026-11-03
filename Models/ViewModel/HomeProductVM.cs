using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using Be_25dh110275_NguyenHoangAnh.Models;

namespace Be_25dh110275_NguyenHoangAnh.Models.ViewModel
{
    public class HomeProductVM
    {
        // Tiêu chí tìm kiếm, mô tả hoặc chủng loại sản phẩm[cite: 13]
        public string SearchTerm { get; set; }

        // Các thuộc tính hỗ trợ phân trang[cite: 13]
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 10;

        public List<Be_25dh110275_NguyenHoangAnh.Models.Product> FeaturedProducts { get; set; }
        public IPagedList<Be_25dh110275_NguyenHoangAnh.Models.Product> NewProducts { get; set; }
    }
}