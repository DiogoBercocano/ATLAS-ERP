using System.Web;
using System.Web.Mvc;

namespace ATLAS_ERP.Infrastructure
{
    public static class CspHtmlHelper
    {
        /// <summary>
        /// Returns the per-request CSP nonce so Razor views can inject it into script tags:
        ///   &lt;script nonce="@Html.CspNonce()"&gt;
        /// The nonce is generated once per request in SecurityHeadersModule.BeginRequest.
        /// </summary>
        public static IHtmlString CspNonce(this HtmlHelper helper)
        {
            var nonce = HttpContext.Current?.Items["CspNonce"] as string;
            return new HtmlString(nonce ?? "");
        }
    }
}
