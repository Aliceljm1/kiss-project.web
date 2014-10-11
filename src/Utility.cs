using System.Web;
using Kiss.Utils;

namespace Kiss.Web
{
    public static class Utility
    {
        public static string FormatCssUrl(IArea site, string url)
        {
            return CombinHost(site, site.CssHost, url);
        }

        public static string FormatJsUrl(IArea site, string url)
        {
            return CombinHost(site, site.JsHost, url);
        }

        public static string FormatUrlWithDomain(IArea site, string url)
        {
            return CombinHost(site, site.Host, url);
        }

        private static string CombinHost(IArea site, string host, string relativeUrl)
        {
            return StringUtil.CombinUrl(host, site.VirtualPath, relativeUrl);
        }
    }
}
