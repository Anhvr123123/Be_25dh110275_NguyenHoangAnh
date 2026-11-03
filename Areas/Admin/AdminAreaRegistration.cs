using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Be_25dh110275_NguyenHoangAnh.Areas.Admin.Controllers" } // Bắt buộc phải có dòng này để định tuyến tìm thấy ProductsController
            );
        }
    }
}