using System.Collections.Generic;
using System.Web;
using System;

namespace Kiss.Web.Area
{
    public class Host : IHost
    {
        public ISite CurrentSite
        {
            get
            {
                string virtualPath = getVirtualPath(HttpContext.Current.Request.Url.AbsolutePath);

                virtualPath = virtualPath.ToLowerInvariant();

                if (AreaInitializer.Areas.ContainsKey(virtualPath))
                    return AreaInitializer.Areas[virtualPath];

                return SiteConfig.Instance;
            }
        }

        private static string getVirtualPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return "/";

            string appPath = HttpContext.Current.Request.ApplicationPath;

            if (appPath != "/")
            {
                if (string.Equals(appPath, absolutePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    absolutePath = "/";
                }
                else
                {
                    var i = absolutePath.IndexOf(appPath);
                    if (i == 0)
                        absolutePath = absolutePath.Substring(appPath.Length);
                }
            }

            string vp;

            if (absolutePath.LastIndexOf('/') == 0)
                vp = "/";
            else
                vp = absolutePath.Substring(0, absolutePath.Substring(1).IndexOf('/') + 1);

            return vp;
        }

        public IList<ISite> AllSites
        {
            get
            {
                List<ISite> list = new List<ISite>();

                foreach (var item in AreaInitializer.Areas)
                {
                    list.Add(item.Value);
                }

                return list;
            }
        }
    }
}
