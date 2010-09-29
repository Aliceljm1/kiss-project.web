using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using Kiss.Utils;
using Kiss.Web.Utils;

namespace Kiss.Web
{
    /// <summary>
    /// 用于合并js，css输出
    /// </summary>
    public class ResourceCombineHandler : IHttpHandler
    {
        private readonly static TimeSpan CACHE_DURATION = TimeSpan.FromDays(30);

        private static ILogger _logger;
        private static ILogger logger
        {
            get
            {
                if (_logger == null)
                    _logger = LogManager.GetLogger<ResourceHandler>();

                return _logger;
            }
        }

        #region IHttpHandler Members

        /// <summary>
        /// <see cref="IHttpHandler.IsReusable"/>
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// <see cref="IHttpHandler.ProcessRequest"/>
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;

            // Read setName, contentType and version. All are required. They are
            // used as cache key
            string files = request["f"] ?? string.Empty;
            string contentType = request["t"] ?? string.Empty;
            string version = request["v"] ?? string.Empty;

            // Decide if browser supports compressed response
            bool isCompressed = RequestUtil.SupportGZip();

            // If the set has already been cached, write the response directly from
            // cache. Otherwise generate the response and cache it
            if (!this.WriteFromCache(context, files, version, isCompressed, contentType))
            {
                using (MemoryStream memoryStream = new MemoryStream(5000))
                {
                    // Decide regular stream or GZipStream based on whether the response
                    // can be cached or not
                    using (Stream writer = isCompressed ?
                        (Stream)(new GZipStream(memoryStream, CompressionMode.Compress)) :
                        memoryStream)
                    {
                        // Load the files defined in querystring and process each file                       
                        string[] fileNames = files.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries);

                        int index = request.Url.OriginalString.IndexOf("rescombiner.axd", StringComparison.InvariantCultureIgnoreCase);
                        if (index == -1)
                            return;

                        string path = Utility.FormatCssUrl(string.Empty).TrimEnd('/');

                        StringBuilder sb = new StringBuilder();
                        foreach (string fileName in fileNames)
                        {
                            byte[] fileBytes = GetFileBytes(context, fileName.Trim());
                            if (fileBytes == null || fileBytes.Length == 0)
                                continue;

                            string str = Encoding.UTF8.GetString(fileBytes);

                            sb.Append(str);
                        }

                        string totalstr = null;
                        if (string.Equals(contentType, "text/css"))
                        {
                            //if (fileName.Contains("res.axd"))
                            //    str = Regex.Replace(str, @"url\s*\(\s*([^:]+?)\s*\)", string.Format("url({0}$1)", path));
                            totalstr = CssMinifier.CssMinify(sb.ToString());
                        }
                        else if (string.Equals(contentType, "text/javascript"))
                        {
                            try
                            {
                                totalstr = new JsMin().MinifyString(sb.ToString());
                            }
                            catch (Exception ex)
                            {
                                logger.Warn(ExceptionUtil.WriteException(ex));
                                totalstr = sb.ToString();
                            }
                        }
                        if (StringUtil.HasText(totalstr))
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(totalstr);
                            writer.Write(buffer, 0, buffer.Length);
                        }
                        writer.Close();
                    }

                    // Cache the combined response so that it can be directly written
                    // in subsequent calls 
                    byte[] responseBytes = memoryStream.ToArray();
                    context.Cache.Insert(GetCacheKey(files, version, isCompressed),
                        responseBytes, null, Cache.NoAbsoluteExpiration,
                        CACHE_DURATION);

                    logger.Debug("refresh cache! url: {0}", request.Url.AbsoluteUri);

                    // Generate the response
                    this.WriteBytes(responseBytes, context, isCompressed, contentType);
                }
            }
        }

        private byte[] GetFileBytes(HttpContext context, string virtualPath)
        {
            try
            {
                ISite site = JContext.Current.Site;
                if (virtualPath.StartsWith("/"))
                {
                    string path = virtualPath;
                    int index = path.IndexOf(site.VirtualPath);
                    if (index != -1)
                        path = path.Substring(index + site.VirtualPath.Length);

                    if (path.StartsWith("themes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        path = path.Substring(6);

                        path = string.Concat(VirtualPathUtility.ToAbsolute(site.ThemeRoot), path);
                    }
                    else
                        path = StringUtil.CombinUrl(site.VirtualPath, path);

                    virtualPath = string.Format("{0}://{1}{2}",
                        context.Request.Url.Scheme,
                        context.Request.Url.Authority,
                        path);
                }

                if (virtualPath.Contains("://"))
                {
                    using (WebClient client = new WebClient())
                    {
                        return client.DownloadData(virtualPath);
                    }
                }
                else
                {
                    string physicalPath = context.Server.MapPath(virtualPath);
                    return File.ReadAllBytes(physicalPath);
                }
            }
            catch
            {
                logger.Error("file: " + virtualPath + " is not found");
            }

            return null;
        }

        private bool WriteFromCache(HttpContext context, string setName, string version,
            bool isCompressed, string contentType)
        {
            byte[] responseBytes = context.Cache[GetCacheKey(setName, version, isCompressed)] as byte[];

            if (null == responseBytes || 0 == responseBytes.Length)
                return false;

            this.WriteBytes(responseBytes, context, isCompressed, contentType);
            return true;
        }

        private void WriteBytes(byte[] bytes, HttpContext context,
            bool isCompressed, string contentType)
        {
            if (bytes == null || bytes.Length == 0)
                return;
            HttpResponse response = context.Response;

            response.AppendHeader("Content-Length", bytes.Length.ToString());
            response.ContentType = contentType;
            if (isCompressed)
                response.AppendHeader("Content-Encoding", "gzip");

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.Add(CACHE_DURATION));
            context.Response.Cache.SetMaxAge(CACHE_DURATION);
            context.Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.Flush();
        }

        #endregion

        private string GetCacheKey(string setName, string version, bool isCompressed)
        {
            return "rescombiner." + setName + "." + version + "." + isCompressed;
        }
    }
}
