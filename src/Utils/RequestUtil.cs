#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-08
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-08		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.Web;

namespace Kiss.Web.Utils
{
    /// <summary>
    /// some util method of client like browser, ip
    /// </summary>
    public static class RequestUtil
    {
        /// <summary>
        /// checking whether GZip is supported by client
        /// </summary>
        /// <returns></returns>
        public static bool SupportGZip()
        {
            string acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

            return !string.IsNullOrEmpty(acceptEncoding)
                && acceptEncoding.ToLower().IndexOf("gzip") > -1;
        }

        /// <summary>
        /// get user's ip address
        /// </summary>
        public static string IpAddress
        {
            get
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
        }

        /// <summary>
        /// checking if browser is ie 6
        /// </summary>
        public static bool IsIE6
        {
            get
            {
                HttpBrowserCapabilities browser = HttpContext.Current.Request.Browser;
                return browser.Browser == "IE" && browser.MajorVersion <= 6;
            }
        }
    }
}
