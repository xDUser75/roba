using System.Web;

namespace Store.Core.Utils
{
    public class SystemUtils
    {
        public static string ServerUrl(HttpRequest Request) 
        {
            if (Request == null) return "";
            string applicationURL = Request.Url.Scheme+"://";
            applicationURL = applicationURL + Request.Url.Authority;
            applicationURL = applicationURL + Request.ApplicationPath;
            if (applicationURL.EndsWith("/")==false)
            {
                applicationURL = applicationURL + "/";
            }
            return HttpUtility.UrlEncode(applicationURL);
        }

    }
}
