#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-24
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-24		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.Collections;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;

namespace Kiss.Web
{
    /// <summary>
    /// A request context class that interacts with HttpContext.Current.
    /// </summary>
    public class WebRequestContext : IWebContext
    {
        /// <summary>Provides access to HttpContext.Current.</summary>
        protected virtual HttpContext CurrentHttpContext
        {
            get
            {
                if (HttpContext.Current == null)
                    throw new WebException("Tried to retrieve HttpContext.Current but it's null. This may happen when working outside a request or when doing stuff after the context has been recycled.");
                return HttpContext.Current;
            }
        }

        public bool IsWeb
        {
            get { return true; }
        }

        /// <summary>Gets a dictionary of request scoped items.</summary>
        public IDictionary RequestItems
        {
            get { return CurrentHttpContext.Items; }
        }

        /// <summary>The handler associated with the current request.</summary>
        public IHttpHandler Handler
        {
            get { return CurrentHttpContext.Handler; }
        }

        /// <summary>The current request object.</summary>
        public HttpRequest Request
        {
            get { return CurrentHttpContext.Request; }
        }

        /// <summary>The physical path on disk to the requested resource.</summary>
        public virtual string PhysicalPath
        {
            get { return Request.PhysicalPath; }
        }

        /// <summary>The host part of the requested url.</summary>
        public Url Url
        {
            get { return new Url(Request.Url.Scheme, Request.Url.Authority, Request.RawUrl); }
        }

        /// <summary>The current request object.</summary>
        public HttpResponse Response
        {
            get { return CurrentHttpContext.Response; }
        }

        /// <summary>Gets the current user in the web execution context.</summary>
        public IPrincipal User
        {
            get { return CurrentHttpContext.User; }
        }

        /// <summary>Converts a virtual url to an absolute url.</summary>
        /// <param name="virtualPath">The virtual url to make absolute.</param>
        /// <returns>The absolute url.</returns>
        public virtual string ToAbsolute(string virtualPath)
        {
            return Url.ToAbsolute(virtualPath);
        }

        /// <summary>Converts an absolute url to an app relative url.</summary>
        /// <param name="virtualPath">The absolute url to convert.</param>
        /// <returns>An app relative url.</returns>
        public virtual string ToAppRelative(string virtualPath)
        {
            if (virtualPath != null && virtualPath.StartsWith(Url.ApplicationPath, System.StringComparison.InvariantCultureIgnoreCase))
                return "~/" + virtualPath.Substring(Url.ApplicationPath.Length);
            return virtualPath;
        }

        /// <summary>Maps a virtual path to a physical disk path.</summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public string MapPath(string path)
        {
            return HostingEnvironment.MapPath(path);
        }

        /// <summary>Assigns a rewrite path.</summary>
        /// <param name="path">The path to the template that will handle the request.</param>
        public void RewritePath(string path)
        {
            CurrentHttpContext.RewritePath(path, false);
        }

        public void TransferRequest(string path)
        {
            string url = Url.Parse(path).AppendQuery("postback", Url.LocalUrl);
            CurrentHttpContext.Server.TransferRequest(url, true);
        }

        public void Close()
        {
        }
    }

    /// <summary>
    /// Represents a way to map a friendly url to a template.
    /// </summary>
    public enum RewriteMethod
    {
        /// <summary>Use HttpContext.RewriteRequest.</summary>
        RewriteRequest,
        /// <summary>Use HttpServerUtility.TransferRequest.</summary>
        TransferRequest,
        /// <summary>Do not rewrite the request. Someone else should pick it up and do something about it.</summary>
        None
    }
}
