using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
