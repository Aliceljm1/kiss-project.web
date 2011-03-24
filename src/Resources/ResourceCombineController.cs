using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using Kiss.Utils;
using Kiss.Web.Mvc;

namespace Kiss.Web.Resources
{
    /// <summary>
    /// 用于合并js，css输出
    /// </summary>
    [Controller("_resc_")]
    class ResourceCombineController : Controller
    {
        private readonly static TimeSpan CACHE_DURATION = TimeSpan.FromDays(30);

        void proc()
        {
            HttpRequest request = httpContext.Request;

            // Read setName, contentType and version. All are required. They are
            // used as cache key
            string files = request["f"] ?? string.Empty;
            string contentType = request["t"] ?? string.Empty;
            string version = request["v"] ?? string.Empty;

            // If the set has already been cached, write the response directly from
            // cache. Otherwise generate the response and cache it
            if (!WriteFromCache(httpContext, files, version, contentType))
            {
                // Load the files defined in querystring and process each file                       
                string[] fileNames = files.Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);

                StringBuilder sb = new StringBuilder();
                foreach (string fileName in fileNames)
                {
                    string str = GetFileContent(httpContext, fileName.Trim());
                    if (string.IsNullOrEmpty(str))
                        continue;

                    sb.AppendLine(str);
                }

                if (sb.Length == 0) return;

                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

                // Cache the combined response so that it can be directly written
                // in subsequent calls 
                httpContext.Cache.Insert(GetCacheKey(files, version),
                    buffer, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

                logger.Debug("refresh combined url: {0}", request.Url.AbsoluteUri);

                // Generate the response
                WriteBytes(buffer, httpContext.Response, contentType);
            }
        }

        private string GetFileContent(HttpContext context, string virtualPath)
        {
            string content = string.Empty;

            try
            {
                JContext jc = JContext.Current;
                ISite site = jc.Site;
                if (virtualPath.StartsWith("/"))
                {
                    string path = virtualPath;
                    int index = path.IndexOf(site.VirtualPath);
                    if (index != -1)
                        path = path.Substring(index + site.VirtualPath.Length);

                    if (path.StartsWith("themes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        path = path.Substring(6);

                        path = string.Concat(VirtualPathUtility.ToAbsolute(jc.CombinUrl(site.ThemeRoot)), path);
                    }
                    else
                        path = StringUtil.CombinUrl(site.VirtualPath, path);

                    virtualPath = string.Format("{0}://{1}{2}",
                        context.Request.Url.Scheme,
                        jc.SiteConfig == null ? context.Request.Url.Authority : jc.SiteConfig.Authority,
                        path);
                }

                if (virtualPath.Contains("://"))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        content = client.DownloadString(virtualPath);
                    }
                }
                else
                {
                    string physicalPath = context.Server.MapPath(virtualPath);
                    content = File.ReadAllText(physicalPath, Encoding.UTF8);
                }
            }
            catch
            {
                logger.Error("file: " + virtualPath + " is not found");
            }

            return content;
        }

        private bool WriteFromCache(HttpContext context, string setName, string version, string contentType)
        {
            byte[] responseBytes = context.Cache[GetCacheKey(setName, version)] as byte[];

            if (null == responseBytes || 0 == responseBytes.Length)
                return false;

            WriteBytes(responseBytes, context.Response, contentType);
            return true;
        }

        private void WriteBytes(byte[] bytes, HttpResponse response, string contentType)
        {
            if (bytes == null || bytes.Length == 0)
                return;
            
            ContentType = contentType;

            ServerUtil.AddCache(60 * 24 * 90);

            response.BinaryWrite(bytes);
        }

        private string GetCacheKey(string setName, string version)
        {
            return "rescombiner." + setName + "." + version;
        }
    }
}
