using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;
using Kiss.Utils;
using Kiss.Utils.Web;
using Kiss.Web.Utils;

namespace Kiss.Web
{
    /// <summary>
    /// Module that handles resources like (javascript css image) using
    /// GZip and some basic code optimizations
    /// 
    /// This module should be used in conjunction with
    /// ClientScriptProxy.RegisterClientScriptResource which sets
    /// up the proper URL formatting required for this module to
    /// handle requests. Format is:
    /// <![CDATA[
    /// res.axd?r=ResourceName&t=FullAssemblyName&z=css
    /// ]]>
    /// The type parameter can be omitted if the resource lives
    /// in this assembly
    /// </summary>
    public class ResourceHandler : IHttpHandler
    {
        private const string PRENAMESPACE = "Kiss.Web.";

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

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;

            string contentType = Request.QueryString["z"];
            if (contentType == "0")
                contentType = "text/css";
            else if (contentType == "1")
                contentType = "application/x-javascript";

            bool shorturl = StringUtil.HasText(Request.QueryString["su"]);

            // *** Start by checking whether GZip is supported by client
            bool useGZip = RequestUtil.SupportGZip() && (string.Equals(contentType, "text/css") || string.Equals(contentType, "application/x-javascript"));

            // *** Create a cachekey and check whether it exists
            string CacheKey = Request.QueryString.ToString() + useGZip.ToString();

            byte[] Output = context.Cache[CacheKey] as byte[];
            if (Output != null)
            {
                // read cache and send to client
                SendOutput(contentType, Output, useGZip);
                return;
            }

            // *** Retrieve information about resource embedded
            // *** Values are base64 encoded
            string ResourceTypeName = Request.QueryString["t"];
            if (!string.IsNullOrEmpty(ResourceTypeName))
                ResourceTypeName = Encoding.ASCII.GetString(Convert.FromBase64String(ResourceTypeName));

            string resource = Request.QueryString["r"];
            if (string.IsNullOrEmpty(resource))
            {
                SendErrorResponse("Invalid Resource");
                return;
            }
            resource = Encoding.ASCII.GetString(Convert.FromBase64String(resource));

            if (shorturl)
            {
                if (StringUtil.HasText(ResourceTypeName))
                    ResourceTypeName = PRENAMESPACE + ResourceTypeName;
                resource = ResourceTypeName + "." + resource;
            }

            // *** Try to locate the assembly that houses the Resource
            Assembly resourceAssembly = null;

            // *** If no type is passed use the current assembly - otherwise
            // *** run through the loaded assemblies and try to find assembly
            if (string.IsNullOrEmpty(ResourceTypeName))
                resourceAssembly = this.GetType().Assembly;
            else
            {
                resourceAssembly = FindAssembly(ResourceTypeName);
                if (resourceAssembly == null)
                {
                    SendErrorResponse("Invalid Type Information");
                    return;
                }
            }

            if (!GetResource(resourceAssembly, resource, ref useGZip, ref Output))
            {
                SendErrorResponse("Error!");
                return;
            }

            // Add into the cache
            HttpRuntime.Cache.Insert(CacheKey, Output, null, DateTime.MaxValue, Cache.NoSlidingExpiration);

            // Write out to Response object with appropriate Client Cache settings
            this.SendOutput(contentType, Output, useGZip);
        }

        private static Dictionary<string, Assembly> asmcaches = new Dictionary<string, Assembly>();

        private static Assembly FindAssembly(string ResourceTypeName)
        {

            if (asmcaches.ContainsKey(ResourceTypeName))
                return asmcaches[ResourceTypeName];

            Assembly assembly = null;
            ITypeFinder typefinder = ServiceLocator.Instance.Resolve<ITypeFinder>();
            if (typefinder != null)
                assembly = typefinder.FindAssembly(ResourceTypeName);
            else
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.FullName == ResourceTypeName || asm.GetName().Name == ResourceTypeName)
                    {
                        assembly = asm;
                        break;
                    }
                }

                assembly = Assembly.Load(ResourceTypeName);
            }

            if (assembly != null)
                asmcaches[ResourceTypeName] = assembly;

            return assembly;
        }

        #endregion

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
                    url = HttpContext.Current.IsDebuggingEnabled && !site.CombinCss ? StringUtil.CombinUrl(site.VirtualPath, "res.axd?r=") : Utility.FormatCssUrl("res.axd?r=");
                    version = site.CssVersion;
                    break;
                case ".js":
                    site = JContext.Current.Site;
                    url = HttpContext.Current.IsDebuggingEnabled && !site.CombinJs ? StringUtil.CombinUrl(site.VirtualPath, "res.axd?r=") : Utility.FormatJsUrl("res.axd?r=");
                    version = site.JsVersion;
                    break;
                default:
                    url = "res.axd?r=";
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
            if (StringUtil.HasText(version))
                url += ("&v=" + version);

            if (shorturl)
                url += "&su=1";

            return url;
        }
        #region helper

        private static bool GetResource(Assembly assembly, string resource, ref bool useGzip, ref byte[] output)
        {
            string extension = StringUtil.GetExtension(resource);

            switch (extension)
            {
                case ".js":
                case ".css":
                    string content = string.Empty;
                    using (Stream st = assembly.GetManifestResourceStream(resource))
                    {
                        if (st == null)
                            return false;
                        StreamReader sr = new StreamReader(st, Encoding.UTF8);
                        content = sr.ReadToEnd();
                    }

                    content = Optimize(extension, content);

                    // Now we're ready to create out output
                    if (useGzip)
                        output = GZipUtil.GZipMemory(content);
                    else
                    {
                        output = Encoding.UTF8.GetBytes(content);
                    }
                    return true;
                default:
                    useGzip = false;
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        if (stream == null)
                        {
                            logger.Error("resource: {0} not found!", resource);
                            return false;
                        }

                        if (stream.CanRead && stream.CanSeek)
                        {
                            output = new byte[stream.Length];
                            stream.Position = 0;
                            stream.Read(output, 0, (int)stream.Length);
                        }
                    }
                    return true;
            }
        }

        // Optimize the script by removing comment lines 
        // and stripping spaces
        private static string Optimize(string extension, string content)
        {
            if (!HttpContext.Current.IsDebuggingEnabled)
            {
                switch (extension)
                {
                    case ".js":
                        return new JavaScriptMinifier().MinifyString(content);
                    case ".css":
                        return CssMinifier.CssMinify(content);
                    default:
                        break;
                }
            }

            return content;
        }

        /// <summary>
        /// Returns an error response to the client. Generates a 404 error
        /// </summary>
        /// <param name="Message"></param>
        private void SendErrorResponse(string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                Message = "Invalid Web Resource";

            HttpContext Context = HttpContext.Current;

            Context.Response.StatusCode = 404;
            Context.Response.StatusDescription = Message;
            Context.Response.End();
        }

        /// <summary>
        /// Sends the output to the client using appropriate cache settings.
        /// Content should be already encoded and ready to be sent as binary.
        /// </summary>
        private void SendOutput(string contentType, byte[] Output, bool useGZip)
        {
            HttpResponse Response = HttpContext.Current.Response;

            Response.ContentType = contentType;

            if (useGZip)
                Response.AppendHeader("Content-Encoding", "gzip");

            if (!HttpContext.Current.IsDebuggingEnabled)
            {
                ServerUtil.AddCache(60 * 24 * 30);
            }

            Response.BinaryWrite(Output);
            Response.End();
        }

        #endregion
    }
}
