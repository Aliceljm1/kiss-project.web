using System.Web;
using Kiss.Utils;

namespace Kiss.Web
{
    public static class Utility
    {
        public static string FormatCssUrl(ISite site, string url)
        {
            return CombinHost(site, site.CssHost, url);
        }

        public static string FormatJsUrl(ISite site, string url)
        {
            return CombinHost(site, site.JsHost, url);
        }

        public static string FormatUrlWithDomain(ISite site, string url)
        {
            return CombinHost(site, site.Host, url);
        }

        private static string CombinHost(ISite site, string host, string relativeUrl)
        {
            string domain = HttpContext.Current == null ? host : HttpContext.Current.IsDebuggingEnabled ? string.Empty : host;

            return StringUtil.CombinUrl(domain, StringUtil.CombinUrl(site.VirtualPath, relativeUrl));
        }
    }
}
