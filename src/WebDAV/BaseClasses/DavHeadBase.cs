using System;
using System.Web;

namespace Kiss.Web.WebDAV.BaseClasses
{
    /// <summary>
    /// Dav Head Framework Base Class
    /// </summary>
    /// <remarks>
    ///		RFC2518 Compliant
    ///		
    ///		<code>
    ///		The ProcessDavRequest event must follow the following rules addressed in RFC2518
    ///			http://www.webdav.org/specs/rfc2518.html#rfc.section.8.4
    ///			
    ///		- If the requested resource does not exist the method MUST fail with:
    ///			
    ///					base.AbortRequest(ServerResponseCode.NotFound)
    ///		</code>
    ///		
    ///		<code>
    ///			Returns ServerResponseCode.OK when successful
    ///		</code>
    ///		<seealso cref="DavMethodBase.ServerResponseCode"/>
    ///		<seealso cref="DavMethodBase.AbortRequest(System.Enum)"/>
    /// </remarks>		
    public abstract class DavHeadBase : DavMethodBase
    {
        /// <summary>
        /// Dav Head Framework Base Class
        /// </summary>
        protected DavHeadBase()
        {
            this.ValidateDavRequest += new DavRequestValidator(DavHeadBase_ValidateDavRequest);
            this.InternalProcessDavRequest += new DavInternalProcessHandler(DavHeadBase_InternalProcessDavRequest);

            this.ResponseCache = HttpCacheability.NoCache;
            this.ResponseCacheExpiration = DateTime.MinValue;
        }

        /// <summary>
        /// Head Resource
        /// </summary>
        protected DavResourceBase Resource { get; set; }

        /// <summary>
        /// Output response cacheability.
        /// </summary>
        /// <remarks>HttpCacheability.NoCache by default</remarks>
        protected HttpCacheability ResponseCache { get; set; }

        /// <summary>
        /// Output response cache expiration
        /// </summary>
        /// <remarks>No expiration by default</remarks>
        protected DateTime ResponseCacheExpiration { get; set; }

        private int DavHeadBase_ValidateDavRequest(object sender, EventArgs e)
        {
            if (base.RequestLength != 0)
                return (int)ServerResponseCode.BadRequest;

            return (int)ServerResponseCode.Ok;
        }

        private int DavHeadBase_InternalProcessDavRequest(object sender, EventArgs e)
        {
            base.HttpApplication.Response.Cache.SetCacheability(ResponseCache);

            if (ResponseCacheExpiration != DateTime.MinValue)
                this.HttpApplication.Response.Cache.SetExpires(ResponseCacheExpiration);

            if (this.Resource != null)
            {
                HttpApplication.Context.Items["_ContentType_"] = this.Resource.ContentType;
                HttpApplication.Response.AddHeader("Content-Type", this.Resource.ContentType);
                HttpApplication.Response.AddHeader("Content-Length", this.Resource.ContentLength.ToString());
                HttpApplication.Response.AddHeader("Last-Modified", this.Resource.LastModified.ToString("r"));
            }

            return (int)ServerResponseCode.Ok;
        }
    }
}