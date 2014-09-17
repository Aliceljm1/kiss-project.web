using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kiss.Web.Area
{
    public class Host : IHost
    {
        public IArea CurrentArea
        {
            get
            {
                string virtualPath = getVirtualPath(HttpContext.Current.Request.Url.AbsolutePath);

                virtualPath = virtualPath.ToLowerInvariant();

                if (AreaInitializer.Areas.ContainsKey(virtualPath))
                    return AreaInitializer.Areas[virtualPath];

                return AreaConfig.Instance;
            }
        }

        private static string getVirtualPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return "/";

            string appPath = HttpRuntime.AppDomainAppVirtualPath;

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

        public IList<IArea> AllAreas
        {
            get
            {
                List<IArea> list = new List<IArea>();

                foreach (var item in AreaInitializer.Areas)
                {
                    list.Add(item.Value);
                }

                return list;
            }
        }

        public IArea GetByAreaKey(string siteKey)
        {
            return (from q in AllAreas
                    where q.AreaKey == siteKey
                    select q).FirstOrDefault();
        }
    }
}
