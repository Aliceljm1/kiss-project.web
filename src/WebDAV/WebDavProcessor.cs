using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Kiss.Web.WebDAV.BaseClasses;
using Kiss.Web.WebDAV.Interfaces;

namespace Kiss.Web.WebDAV
{
    /// <summary>
    /// WebDav Framework entry point for Dav request processing
    /// </summary>
    internal class WebDavProcessor
    {
        private static readonly ILogger logger = LogManager.GetLogger<DavMethodBase>();

        /// <summary>
        /// Initializes a new instance of the WebDavProcessor class
        /// </summary>
        static WebDavProcessor()
        {
            WebDavProcessor.RequestCache = new List<CacheInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the WebDavProcessor class
        /// </summary>
        public WebDavProcessor() : this(Assembly.GetCallingAssembly()) { }

        /// <summary>
        /// Initializes a new instance of the WebDavProcessor class
        /// </summary>
        /// <param name="davSourceAssembly">Assembly containing custom DAV Method implementation </param>
        public WebDavProcessor(Assembly davSourceAssembly)
        {
            this.DavSourceAssembly = davSourceAssembly;
            this.DavMethodList = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);

            //Use reflection to identify the map the Map the current 
            foreach (Type _objectType in this.DavSourceAssembly.GetTypes())
            {
                string _davMethod = null;

                if (_objectType.BaseType == typeof(DavOptionsBase))
                    _davMethod = "OPTIONS";
                else if (_objectType.BaseType == typeof(DavMKColBase))
                    _davMethod = "MKCOL";
                else if (_objectType.BaseType == typeof(DavPropFindBase))
                    _davMethod = "PROPFIND";
                else if (_objectType.BaseType == typeof(DavHeadBase))
                    _davMethod = "HEAD";
                else if (_objectType.BaseType == typeof(DavDeleteBase))
                    _davMethod = "DELETE";
                else if (_objectType.BaseType == typeof(DavMoveBase))
                    _davMethod = "MOVE";
                else if (_objectType.BaseType == typeof(DavCopyBase))
                    _davMethod = "COPY";
                else if (_objectType.BaseType == typeof(DavPutBase))
                    _davMethod = "PUT";
                else if (_objectType.BaseType == typeof(DavGetBase))
                    _davMethod = "GET";
                else if (_objectType.BaseType == typeof(DavLockBase))
                    _davMethod = "LOCK";
                else if (_objectType.BaseType == typeof(DavUnlockBase))
                    _davMethod = "UNLOCK";
                else if (_objectType.BaseType == typeof(DavPropPatchBase))
                    _davMethod = "PROPPATCH";
                else if (_objectType.BaseType == typeof(DavVersionControlBase))
                    _davMethod = "VERSION-CONTROL";
                else if (_objectType.BaseType == typeof(DavReportBase))
                    _davMethod = "REPORT";

                if (_davMethod != null)
                {
                    if (this.DavMethodList.ContainsKey(_davMethod))
                        throw new WebDavException("Duplicate objects for " + _davMethod + " found. There should only up to 1 object implementing each of the base DavMethod classes.");

                    this.DavMethodList[_davMethod] = _objectType.ToString();
                }
            }
        }

        /// <summary>
        /// WebDav Framework method for Dav request processing
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <remarks>
        ///		Process all requests... will return 501 if the requested method is not implemented
        /// </remarks>
        public void ProcessRequest(HttpApplication httpApplication)
        {
            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication", InternalFunctions.GetResourceString("ArgumentNullException", "HttpApplication"));

            //Set the status code to Method Not Allowed by default
            int _statusCode = 405;

            string _httpMethod = httpApplication.Request.HttpMethod;

            logger.Debug("Processing HttpMethod " + _httpMethod);
            //try
            {
                if (this.DavMethodList.ContainsKey(_httpMethod))
                {
                    httpApplication.Response.Clear();
                    httpApplication.Response.ClearContent();

                    DavMethodBase _davMethodBase = this.DavSourceAssembly.CreateInstance(this.DavMethodList[_httpMethod]) as DavMethodBase;
                    if (_davMethodBase != null)
                    {
                        _davMethodBase.HttpApplication = httpApplication;

                        //Purge all old cache items 
                        WebDavProcessor.RequestCache.RemoveAll(e => e.ExpirationDate < DateTime.Now);

                        ICacheableDavResponse _cacheableResponse = _davMethodBase as ICacheableDavResponse;
                        if (_cacheableResponse != null)
                        {
                            CacheInfo _cacheItem = WebDavProcessor.RequestCache
                                            .Where(e => e.Verb == _httpMethod && e.CacheKey == _cacheableResponse.CacheKey)
                                            .FirstOrDefault();

                            if (_cacheItem != null)
                            {
                                _davMethodBase.SetResponseXml(_cacheItem.ResponseXML);
                                _davMethodBase.SendResponseXML();
                                _statusCode = _cacheItem.StatusCode;
                            }
                            else
                            {
                                _statusCode = ((IDavRequest)_davMethodBase).ProcessRequest();

                                //Set the cache
                                if (_cacheableResponse.CacheTTL > 0)
                                {
                                    //Cache the info for now to improve speed
                                    _cacheItem = new CacheInfo()
                                    {
                                        Verb = _httpMethod,
                                        CacheKey = _cacheableResponse.CacheKey,
                                        ExpirationDate = DateTime.Now.AddSeconds(_cacheableResponse.CacheTTL),
                                        ResponseXML = _davMethodBase.ResponseXml,
                                        StatusCode = _statusCode
                                    };

                                    //TODO: do we need locking?
                                    WebDavProcessor.RequestCache.Add(_cacheItem);
                                }
                            }
                        }
                        else
                            _statusCode = ((IDavRequest)_davMethodBase).ProcessRequest();
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    InternalFunctions.WriteDebugLog("Error processing HttpMethod " + _httpMethod + 
            //        Environment.NewLine + "Message: " + ex.Message);
            //}

            logger.Debug("Completed processing HttpMethod " + _httpMethod + " status code returned: " + _statusCode);

            if (this.DavMethodList.ContainsKey(_httpMethod))
            {
                httpApplication.Response.StatusCode = _statusCode;

                //Perhaps implement...
                //httpApplication.Request.InputStream.Close();				
            }
        }

        #region Private Properties / Classes

        private class CacheInfo
        {
            /// <summary>
            /// Http Verb
            /// </summary>
            public string CacheKey { get; set; }
            public string Verb { get; set; }
            public int StatusCode { get; set; }
            public DateTime ExpirationDate { get; set; }
            public string ResponseXML { get; set; }
        }

        /// <summary>
        /// Dav Source Assembly
        /// </summary>
        private Assembly DavSourceAssembly { get; set; }

        private static List<CacheInfo> RequestCache { get; set; }

        /// <summary>
        /// Keeps track of all the inherited DavOptionsBase members
        /// </summary>
        private SortedList<string, string> DavMethodList { get; set; }

        #endregion
    }
}