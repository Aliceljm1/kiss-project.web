using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Kiss.Plugin;
using Kiss.Utils;

namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// 重定向
    /// </summary>
    public class UrlMappingModule : IStartable, IStoppable
    {
        #region props

        #region events

        /// <summary>
        /// Occurs when the url is matched
        /// </summary>
        public static event EventHandler<EventArgs> UrlMatched;

        /// <summary>
        /// Raises the Saved event.
        /// </summary>
        public void OnUrlMatched()
        {
            if (UrlMatched != null)
            {
                UrlMatched(this, EventArgs.Empty);
            }
        }

        #endregion

        public static UrlMappingModule Instance { get; private set; }

        private IUrlMappingProvider _provider;
        public IUrlMappingProvider Provider { get { return _provider; } }

        private const string kCONTEXTITEMS_RAWURLKEY = "__UrlMappingModule_RawUrl__";
        private const string kCONTEXTITEMS_ADDEDQSKEY = "__UrlMappingModule_AddedQS__";

        private NoMatchAction _noMatchAction;
        private string _noMatchRedirectPage;
        private bool _automaticallyUpdateFormAction;
        private IncomingQueryStringBehavior _qsBehavior;
        private List<string> _ignoreExtensions;
        private List<string> _allowExtensions;
        private UrlProcessingEvent _processingEvent;

        readonly EventBroker broker;

        #endregion

        #region ctor

        public UrlMappingModule()
        {
            broker = EventBroker.Instance;
            Instance = this;
        }

        #endregion

        public NameValueCollection GetMappedQueryString(string urlRequested)
        {
            urlRequested = GetUrlRequested(urlRequested);
            foreach (UrlMappingItem item in _provider.UrlMappings ?? new UrlMappingItemCollection())
            {
                Match match = item.UrlTarget.Match(urlRequested);

                if (match.Success)
                {
                    // do we want to add querystring parameters for dynamic mappings?
                    NameValueCollection qs = new NameValueCollection();
                    if (match.Groups.Count > 1)
                    {
                        for (int i = 1; i < match.Groups.Count; i++)
                            qs.Add(item.UrlTarget.GroupNameFromNumber(i), match.Groups[i].Value);
                    }

                    JContext.Current.Navigation.Set(item);

                    return qs;
                }
            }

            return new NameValueCollection();
        }

        void context_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app != null)
            {
                Page page = app.Context.Handler as Page;
                if (page != null)
                {
                    if (HttpContext.Current.Items[kCONTEXTITEMS_RAWURLKEY] != null)
                    {
                        page.PreRender += new EventHandler(page_PreRender);
                    }
                }
            }
        }

        void page_PreRender(object sender, EventArgs e)
        {
            object o = HttpContext.Current.Items[kCONTEXTITEMS_RAWURLKEY];
            if (o != null)
            {
                string rawPath = o.ToString();

                string qs = string.Empty;

                if (rawPath.Contains("?"))
                {
                    int index = rawPath.IndexOf("?");
                    qs = _qsBehavior == IncomingQueryStringBehavior.Ingore ? string.Empty : rawPath.Substring(index + 1);
                    rawPath = rawPath.Remove(index);
                }

                HttpContext.Current.RewritePath(rawPath, string.Empty, qs, false);
            }
        }

        void ProcessUrl(object sender, EventArgs e)
        {
            HttpApplication app = (sender as HttpApplication);
            if (app == null)
                return;

            string urlRequested = GetUrlRequested(app.Request);

            if (!CheckExtension(urlRequested))
                return;

            // inspect the request and perform redirection as necessary
            // start by getting the mapping items from the provider
            NameValueCollection qs;
            string newPath;
            if (Match(app.Request, urlRequested, out newPath, out qs))
            {
                RerouteRequest(app, newPath, qs, app.Request.QueryString);
            }
            else
            {
                switch (_noMatchAction)
                {
                    case NoMatchAction.PassThrough:
                        // do nothing; allow the request to continut to be processed normally;                                
                        break;
                    case NoMatchAction.Redirect:
                        RerouteRequest(app, _noMatchRedirectPage, null, app.Request.QueryString);
                        break;
                    case NoMatchAction.Return404:
                        app.Response.StatusCode = 404;
                        app.Response.StatusDescription = "File not found.";
                        app.Response.End();
                        break;
                    case NoMatchAction.ThrowException:
                        throw new UrlMappingException("No UrlMappingModule match found for url '" + urlRequested + "'.");
                }
            }

            SetViewData();
        }

        #region protected

        protected void RerouteRequest(HttpApplication app, string newPath, NameValueCollection qs, NameValueCollection incomingQS)
        {
            // signal to the future page handler that we rerouted from a different URL
            HttpContext.Current.Items.Add(kCONTEXTITEMS_RAWURLKEY, HttpContext.Current.Request.RawUrl);
            HttpContext.Current.Items.Add(kCONTEXTITEMS_ADDEDQSKEY, qs);

            NameValueCollection urlQueryString = null;
            if (newPath.Contains("?"))
            {
                Url url = new Url(newPath);
                if (StringUtil.HasText(url.Query))
                {
                    urlQueryString = StringUtil.CommaDelimitedEquation2NVCollection(url.Query);
                    urlQueryString.Remove("kissMasterFile");
                }
            }

            // apply the querystring to the path
            newPath = ApplyQueryString(newPath, qs);

            JContext.Current.QueryString.Clear();
            JContext.Current.QueryString.Add(qs);
            if (urlQueryString != null && urlQueryString.HasKeys())
                JContext.Current.QueryString.Add(urlQueryString);

            // if configured, apply the incoming query string variables too
            if (_qsBehavior == IncomingQueryStringBehavior.PassThrough)
            {
                NameValueCollection _qs = new NameValueCollection(incomingQS);
                _qs.Remove("kissMasterFile");
                newPath = ApplyQueryString(newPath, _qs);
                JContext.Current.QueryString.Add(_qs);
            }

            // perform the redirection
            if (newPath.StartsWith("~/"))
                HttpContext.Current.RewritePath(newPath, false);
            else if (newPath.StartsWith("/"))
                app.Response.Redirect(newPath);
            else if (newPath.StartsWith("http:") || newPath.StartsWith("https:"))
            {
                app.Response.Status = "301 Moved Permanently";
                app.Response.AddHeader("Location", newPath);
                app.Response.End();
            }
            else
                // otherwise, treat it as a local file and force the virtual path
                HttpContext.Current.RewritePath("~/" + newPath, false);
        }

        protected string ApplyQueryString(string path, NameValueCollection qs)
        {
            // append the given querystring items to the given path
            if (qs != null)
            {
                for (int i = 0; i < qs.Count; i++)
                {
                    if ((i == 0) && !path.Contains("?"))
                        path += string.Format("?{0}={1}", qs.GetKey(i), ServerUtil.UrlEncode(qs[i]));
                    else
                        path += string.Format("&{0}={1}", qs.GetKey(i), ServerUtil.UrlEncode(qs[i]));
                }
            }

            return path;
        }

        #endregion

        #region Initalize

        private void Initalize()
        {
            UrlMappingConfig config = UrlMappingConfig.Instance;

            _provider = ServiceLocator.Instance.Resolve<IUrlMappingProvider>();

            _noMatchAction = config.NoMatchAction;
            _noMatchRedirectPage = config.NoMatchRedirectUrl;
            _automaticallyUpdateFormAction = config.AutoUpdateFormAction;
            _qsBehavior = config.IncomingQueryStringBehavior;
            _processingEvent = config.UrlProcessingEvent;

            _ignoreExtensions = new List<string>(config.IgnoreExtensions.Split(new char[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries));
            for (int i = 0; i < _ignoreExtensions.Count; i++)
            {
                _ignoreExtensions[i] = _ignoreExtensions[i].Trim().ToLower();
            }

            _allowExtensions = new List<string>(config.AllowExtensions.Split(new char[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries));
            for (int i = 0; i < _allowExtensions.Count; i++)
            {
                _allowExtensions[i] = _allowExtensions[i].Trim().ToLower();
            }

            if (_provider != null)
            {
                _provider.Initialize(config);
            }

            // save to config
            config.Provider = _provider;
        }

        #endregion

        public void Start()
        {
            Initalize();

            switch (_processingEvent)
            {
                case UrlProcessingEvent.BeginRequest:
                    broker.BeginRequest += ProcessUrl;
                    break;
                case UrlProcessingEvent.AuthenticateRequest:
                    broker.AuthenticateRequest += ProcessUrl;
                    break;
                case UrlProcessingEvent.AuthorizeRequest:
                    broker.AuthorizeRequest += ProcessUrl;
                    break;
                default:
                    break;
            }

            if (_automaticallyUpdateFormAction)
            {
                broker.PostMapRequestHandler += context_PostMapRequestHandler;
            }
        }

        public void Stop()
        {
            switch (_processingEvent)
            {
                case UrlProcessingEvent.BeginRequest:
                    broker.BeginRequest -= ProcessUrl;
                    break;
                case UrlProcessingEvent.AuthenticateRequest:
                    broker.AuthenticateRequest -= ProcessUrl;
                    break;
                case UrlProcessingEvent.AuthorizeRequest:
                    broker.AuthorizeRequest -= ProcessUrl;
                    break;
                default:
                    break;
            }

            if (_automaticallyUpdateFormAction)
            {
                broker.PostMapRequestHandler -= context_PostMapRequestHandler;
            }
        }

        private string GetUrlRequested(HttpRequest request)
        {
            // if we want to include the queryString in pattern matching, use RawUrl
            // otherwise use Path
            string rawUrl = (_qsBehavior == IncomingQueryStringBehavior.Include ? request.RawUrl : request.Path);

            // identify the string to pattern match; this should not include
            // "~/" or "/" at the front but otherwise should be an application-relative
            // reference
            return GetUrlRequested(rawUrl);
        }

        private string GetUrlRequested(string url)
        {
            string virtualPath = JContext.Current.Site.VirtualPath;
            string urlRequested = string.Empty;
            if (virtualPath != "/")
                urlRequested = url.ToLower().Replace(virtualPath, string.Empty);
            else
                urlRequested = url;

            if (urlRequested.EndsWith("/"))
                urlRequested += "default.aspx";

            return urlRequested.Trim('/');
        }

        private bool CheckExtension(string url)
        {
            // should the module ignore this request, based on the extension?
            string extension = StringUtil.GetExtension(url.ToLower());

            if (StringUtil.IsNullOrEmpty(extension))
                return true;

            if (_allowExtensions.Count > 0)
                return _allowExtensions.Contains(extension);
            else
                return !_ignoreExtensions.Contains(extension);
        }

        internal static void SetViewData()
        {
            JContext jc = JContext.Current;
            jc.ViewData["jc"] = jc;
            foreach (string key in ContextData.Datas.Keys)
            {
                jc.ViewData[key] = ContextData.Datas[key];
            }
        }

        private bool Match(HttpRequest request, string urlRequested, out string newPath, out NameValueCollection qs)
        {
            qs = new NameValueCollection();
            JContext jc = JContext.Current;
            foreach (UrlMappingItem item in _provider.UrlMappings ?? new UrlMappingItemCollection())
            {
                Match match = item.UrlTarget.Match(urlRequested);

                bool s = match.Success;

                if (s)
                {
                    // add querystring parameters for dynamic mappings
                    for (int i = 1; i < match.Groups.Count; i++)
                        qs.Add(item.UrlTarget.GroupNameFromNumber(i), match.Groups[i].Value);
                }
                else if (StringUtil.IsNullOrEmpty(jc.Navigation.LanguageCode))
                {
                    Regex regex = new Regex(string.Format(@"^(?<lang>[a-zA-z\-]*)/{0}", item.UrlTarget.ToString().Substring(1)));

                    match = regex.Match(urlRequested);
                    s = match.Success;
                    if (s)
                    {
                        jc.Navigation.LanguageCode = match.Groups[1].Value;

                        // add querystring parameters for dynamic mappings
                        for (int i = 2; i < match.Groups.Count; i++)
                            qs.Add(item.UrlTarget.GroupNameFromNumber(i), match.Groups[i].Value);
                    }
                }

                if (s)
                {
                    jc.Navigation.Set(item);

                    OnUrlMatched();

                    newPath = item.Redirection;

                    return true;
                }
            }
            newPath = string.Empty;
            return false;
        }
    }
}
