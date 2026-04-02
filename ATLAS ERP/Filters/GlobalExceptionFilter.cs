using System.Web.Mvc;

namespace ATLAS_ERP.Filters
{
    public class GlobalExceptionFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary
                {
                    { "Mensagem", "Ocorreu um erro inesperado. Tente novamente." },
                    { "Detalhe", filterContext.Exception.Message }
                }
            };
        }
    }
}