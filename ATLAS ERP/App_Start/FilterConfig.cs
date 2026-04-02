using System.Web.Mvc;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new GlobalExceptionFilter());
        }
    }
}