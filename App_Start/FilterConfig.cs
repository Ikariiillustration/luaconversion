using System.Web;
using System.Web.Mvc;
using project2.Data;
using project2.Models;

namespace project2
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
