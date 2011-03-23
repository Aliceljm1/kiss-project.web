using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;
using Kiss.Utils;
using Kiss.Web.Mvc;

namespace Kiss.Web.Resources
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
    /// _res.aspx?r=ResourceName&t=FullAssemblyName&z=0
    /// ]]>
    /// The type parameter can be omitted if the resource lives
    /// in this assembly
    /// </summary>
    [Controller("_res_")]
    class ResourceController : Controller
    {
        void proc()
        {
            HttpRequest request = httpContext.Request;
            HttpResponse response = httpContext.Response;

            string contentType = request.QueryString["z"];
            if (contentType == "0")
                contentType = "text/css";
            else if (contentType == "1")
                contentType = "application/x-javascript";

            bool shorturl = StringUtil.HasText(request.QueryString["su"]);

            // *** Create a cachekey and check whether it exists
            string CacheKey = request.QueryString.ToString();

            byte[] output = httpContext.Cache[CacheKey] as byte[];
            if (output != null)
            {
                // read cache and send to client
                SendOutput(response, contentType, output);
                return;
            }

            // *** Retrieve information about resource embedded
            // *** Values are base64 encoded
            string ResourceTypeName = request.QueryString["t"];
            if (!string.IsNullOrEmpty(ResourceTypeName))
                ResourceTypeName = Encoding.ASCII.GetString(Convert.FromBase64String(ResourceTypeName));

            string resource = request.QueryString["r"];
            if (string.IsNullOrEmpty(resource))
            {
                SendErrorResponse(response, "Invalid Resource");
                return;
            }
            resource = Encoding.ASCII.GetString(Convert.FromBase64String(resource));

            if (shorturl)
            {
                if (StringUtil.HasText(ResourceTypeName))
                    ResourceTypeName = Utility.PRENAMESPACE + ResourceTypeName;
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
                    SendErrorResponse(response, "Invalid Type Information");
                    return;
                }
            }

            output = ResourceUtil.LoadBufferFromAssembly(resourceAssembly, resource);

            if (output == null)
            {
                logger.Error("resource: {0} not found!", resource);
                SendErrorResponse(response, "Error!");
            }
            else
            {
                // Add into the cache
                HttpRuntime.Cache.Insert(CacheKey, output, null, DateTime.MaxValue, Cache.NoSlidingExpiration);

                // Write out to Response object with appropriate Client Cache settings
                SendOutput(response, contentType, output);
            }
        }

        private static readonly Dictionary<string, Assembly> asmcaches = new Dictionary<string, Assembly>();

        private static Assembly FindAssembly(string resourceTypeName)
        {
            if (asmcaches.ContainsKey(resourceTypeName))
                return asmcaches[resourceTypeName];

            Assembly assembly = null;

            lock (asmcaches)
            {
                if (asmcaches.ContainsKey(resourceTypeName))
                    return asmcaches[resourceTypeName];

                ITypeFinder typefinder = ServiceLocator.Instance.Resolve<ITypeFinder>();
                if (typefinder != null)
                    assembly = typefinder.FindAssembly(resourceTypeName);
                else
                {
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (asm.FullName == resourceTypeName || asm.GetName().Name == resourceTypeName)
                        {
                            assembly = asm;
                            break;
                        }
                    }

                    assembly = Assembly.Load(resourceTypeName);
                }

                if (assembly != null)
                    asmcaches[resourceTypeName] = assembly;
            }

            return assembly;
        }

        #region helper

        private void SendErrorResponse(HttpResponse response, string message)
        {
            if (!string.IsNullOrEmpty(message))
                message = "Invalid Web Resource";

            response.StatusCode = 404;
            response.StatusDescription = message;
            response.End();
        }

        private void SendOutput(HttpResponse response, string contentType, byte[] Output)
        {
            response.ContentType = contentType;

            ServerUtil.AddCache(60 * 24 * 90);

            response.BinaryWrite(Output);
            response.End();
        }

        #endregion
    }
}
