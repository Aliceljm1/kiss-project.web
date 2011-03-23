using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kiss.Utils;
using System.Web;
using Kiss.Web.Utils;

namespace Kiss.Web.Resources
{
    public static class Utility
    {
        internal const string PRENAMESPACE = "Kiss.Web.";

        public static string GetResourceUrl(Type type, string resourceName)
        {
            return GetResourceUrl(type, resourceName, resourceName.StartsWith(PRENAMESPACE));
        }

        public static string GetResourceUrl(string resourceFullName, bool shorturl)
        {
            if (StringUtil.IsNullOrEmpty(resourceFullName))
                return string.Empty;

            string[] strs = StringUtil.Split(resourceFullName, StringUtil.Comma, true, true);

            if (strs.Length != 2)
                return string.Empty;

            return GetResourceUrl(strs[1], strs[0], shorturl);
        }

        public static string GetResourceUrl(Type type, string resourceName, bool shorturl)
        {
            return GetResourceUrl(type.Assembly.GetName().Name, resourceName, shorturl);
        }

        public static string GetResourceUrl(string assemblyName, string resourceName)
        {
            return GetResourceUrl(assemblyName, resourceName, assemblyName.StartsWith(PRENAMESPACE, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetResourceUrl(string assemblyName, string resourceName, bool shorturl)
        {
            string extension = StringUtil.GetExtension(resourceName);

            string url = null;
            string version = string.Empty;

            ISite site = null;
            switch (extension)
            {
                case ".css":
                    site = JContext.Current.Site;
                    url = HttpContext.Current.IsDebuggingEnabled && !site.CombinCss ? StringUtil.CombinUrl(site.VirtualPath, "_res.aspx?r=") : Web.Utility.FormatCssUrl(site, "_res.aspx?r=");
                    version = site.CssVersion;
                    break;
                case ".js":
                    site = JContext.Current.Site;
                    url = HttpContext.Current.IsDebuggingEnabled && !site.CombinJs ? StringUtil.CombinUrl(site.VirtualPath, "_res.aspx?r=") : Web.Utility.FormatJsUrl(site, "_res.aspx?r=");
                    version = site.JsVersion;
                    break;
                default:
                    url = "_res.aspx?r=";
                    break;
            }

            if (shorturl)
            {
                assemblyName = assemblyName.Substring(PRENAMESPACE.Length);
                resourceName = resourceName.Substring(PRENAMESPACE.Length).Substring(assemblyName.Length + 1);
            }

            url += Convert.ToBase64String(Encoding.ASCII.GetBytes(resourceName)) +
                                            "&t=" +
                                            Convert.ToBase64String(Encoding.ASCII.GetBytes(assemblyName));

            string contentType = ContentTypeUtil.GetContentType(extension);

            // short the url
            if (contentType == "text/css")
                contentType = "0";
            else if (contentType == "application/x-javascript")
                contentType = "1";

            url += ("&z=" + contentType);

            if (StringUtil.HasText(version) && site != null && !site.CombinCss)
                url += ("&v=" + version);

            if (shorturl)
                url += "&su=1";

            return url;
        }

    }
}
