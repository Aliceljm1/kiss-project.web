using System.Web;
using Kiss.Utils;

namespace Kiss.Web
{
    public static class Utility
    {
        public static string FormatCssUrl(string url)
        {
            return CombinHost(JContext.Current.Site.CssHost, url);
        }

        public static string FormatJsUrl(string url)
        {
            return CombinHost(JContext.Current.Site.JsHost, url);
        }

        public static string FormatUrlWithDomain(string url)
        {
            return CombinHost(JContext.Current.Site.Host, url);
        }

        public static string StylePath
        {
            get
            {
                JContext jc = JContext.Current;
                return FormatCssUrl(string.Format(jc.Site.CssRoot, jc.Theme));
            }
        }

        private static string CombinHost(string host, string relativeUrl)
        {
            string domain = HttpContext.Current == null ? host : HttpContext.Current.IsDebuggingEnabled ? string.Empty : host;

            return StringUtil.CombinUrl(domain, StringUtil.CombinUrl(JContext.Current.Site.VirtualPath, relativeUrl));
        }
    }
}
