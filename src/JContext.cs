using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using Kiss.Plugin;
using Kiss.Security;
using Kiss.Utils;
using Kiss.Web.Mvc;
using Kiss.Web.UrlMapping;

namespace Kiss.Web
{
    /// <summary>
    /// 存储了当前请求的一些信息
    /// </summary>
    public sealed class JContext
    {
        #region Private Containers

        //Generally expect 10 or less items
        private HybridDictionary _items = new HybridDictionary();
        private NameValueCollection _queryString = null;

        private HttpContext _httpContext = null;
        private DateTime requestStartTime = DateTime.Now;
        public DateTime RequestStartTime { get { return requestStartTime; } }
        private string _rawUrl;
        public string RawUrl { get { return _rawUrl; } }

        #endregion

        #region Initialize  and cnstr.'s

        /// <summary>
        /// Create/Instatiate items that will vary based on where this object 
        /// is first created
        /// 
        /// We could wire up Path, encoding, etc as necessary
        /// </summary>
        private void Initialize(NameValueCollection qs, string rawUrl)
        {
            _queryString = qs;
            _rawUrl = rawUrl;
        }

        /// <summary>
        /// cnst called when HttpContext is avaiable
        /// </summary>
        private JContext(HttpContext context, bool includeQS)
        {
            this._httpContext = context;

            if (includeQS)
                Initialize(new NameValueCollection(context.Request.QueryString), context.Request.RawUrl);
            else
                Initialize(null, context.Request.RawUrl);
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates a Context instance based on HttpContext. Generally, this
        /// method should be called via Begin_Request in an HttpModule
        /// </summary>
        public static JContext Create(HttpContext context)
        {
            JContext jcontext = new JContext(context, true);
            SaveContextToStore(jcontext);

            return jcontext;
        }

        #endregion

        #region Core Properties
        /// <summary>
        /// Simulates Context.Items and provides a per request/instance storage bag
        /// </summary>
        public IDictionary Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Provides direct access to the .Items property
        /// </summary>
        public object this[string key]
        {
            get
            {
                return this.Items[key];
            }
            set
            {
                this.Items[key] = value;
            }
        }

        /// <summary>
        /// Allows access to QueryString values
        /// </summary>
        public NameValueCollection QueryString
        {
            get { return _queryString; }
        }

        public HttpContext Context
        {
            get
            {
                return _httpContext;
            }
        }

        #endregion

        #region Helpers

        // *********************************************************************
        //  GetGuidFromQueryString
        //
        /// <summary>
        /// Retrieves a value from the query string and returns it as an int.
        /// </summary>
        // ***********************************************************************/
        public Guid GetGuidFromQueryString(string key)
        {
            Guid returnValue = Guid.Empty;
            string queryStringValue;

            // Attempt to get the value from the query string
            //
            queryStringValue = QueryString[key];

            // If we didn't find anything, just return
            //
            if (queryStringValue == null)
                return returnValue;

            // Found a value, attempt to conver to integer
            //
            try
            {

                // Special case if we find a # in the value
                //
                if (queryStringValue.IndexOf("#") > 0)
                    queryStringValue = queryStringValue.Substring(0, queryStringValue.IndexOf("#"));

                returnValue = new Guid(queryStringValue);
            }
            catch { }

            return returnValue;

        }

        // *********************************************************************
        //  GetIntFromQueryString
        //
        /// <summary>
        /// Retrieves a value from the query string and returns it as an int.
        /// </summary>
        // ***********************************************************************/
        public int GetIntFromQueryString(string key, int defaultValue)
        {
            string queryStringValue;


            // Attempt to get the value from the query string
            //
            queryStringValue = this.QueryString[key];

            // If we didn't find anything, just return
            //
            if (queryStringValue == null)
                return defaultValue;

            // Found a value, attempt to conver to integer
            //
            try
            {

                // Special case if we find a # in the value
                //
                if (queryStringValue.IndexOf("#") > 0)
                    queryStringValue = queryStringValue.Substring(0, queryStringValue.IndexOf("#"));

                defaultValue = Convert.ToInt32(queryStringValue);
            }
            catch { }

            return defaultValue;

        }

        #endregion

        #region Common QueryString Properties

        int pageIndex = -2;

        string orderBy = null;
        string returnUrl = null;

        private string queryText;
        public string QueryText
        {
            get
            {
                if (queryText == null)
                {
                    queryText = QueryString["q"];
                    if (StringUtil.IsNullOrEmpty(queryText))
                        queryText = null;
                }
                return queryText;
            }
            set { queryText = value; }
        }

        public string OrderBy
        {
            get
            {
                if (orderBy == null)
                    orderBy = QueryString["o"];

                return orderBy;
            }
            set { orderBy = value; }
        }

        public string ReturnUrl
        {
            get
            {
                if (returnUrl == null)
                    returnUrl = QueryString["returnUrl"];

                return returnUrl;
            }
            set { returnUrl = value; }
        }

        public int ObjId
        {
            get
            {
                return GetIntFromQueryString("Id", 0);
            }
        }

        public int PageIndex
        {
            get
            {
                if (pageIndex == -2)
                {
                    pageIndex = this.GetIntFromQueryString("page", GetIntFromQueryString("p", -1));
                    if (pageIndex != -1)
                        pageIndex = pageIndex - 1;
                    else if (pageIndex < 0)
                        pageIndex = 0;

                }
                return pageIndex;
            }
            set { pageIndex = value; }
        }


        #endregion

        #region State

        private static readonly string dataKey = "JContextStore";

        /// <summary>
        /// Returns the current instance of the CSContext from the ThreadData Slot. If one is not found and a valid HttpContext can be found,
        /// it will be used. Otherwise, an exception will be thrown. 
        /// </summary>
        public static JContext Current
        {
            get
            {
                HttpContext httpContext = HttpContext.Current;
                JContext context = null;
                if (httpContext != null)
                {
                    context = httpContext.Items[dataKey] as JContext;
                }
                else
                {
                    context = Thread.GetData(GetSlot()) as JContext;
                }

                if (context == null)
                {

                    if (httpContext == null)
                        throw new Exception("No JContext exists in the Current Application. AutoCreate fails since HttpContext.Current is not accessible.");

                    context = new JContext(httpContext, true);
                    SaveContextToStore(context);
                }
                return context;
            }
        }

        private static LocalDataStoreSlot GetSlot()
        {
            return Thread.GetNamedDataSlot(dataKey);
        }

        private static void SaveContextToStore(JContext context)
        {
            if (context.Context != null)
            {
                context.Context.Items[dataKey] = context;
            }
            else
            {
                Thread.SetData(GetSlot(), context);
            }
        }

        public static void Unload()
        {
            Thread.FreeNamedDataSlot(dataKey);
        }

        #endregion

        #region Navigation

        private NavigationInfo _navigation = new NavigationInfo();
        /// <summary>
        /// get navigation info
        /// </summary>
        public NavigationInfo Navigation { get { return _navigation; } internal set { _navigation = value; } }

        private List<NavigationItem> crumbs = new List<NavigationItem>();
        /// <summary>
        /// 面包屑导航
        /// </summary>
        public List<NavigationItem> Crumbs
        {
            get { return crumbs; }
            set { crumbs = value; }
        }

        /// <summary>
        /// 添加到面包屑导航
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        public void AddCrumb(string title, string url)
        {
            if (!StringUtil.HasText(title))
                return;

            crumbs.Add(new NavigationItem(title, url));
        }

        /// <summary>
        /// 添加到面包屑导航(无链接)
        /// </summary>
        /// <param name="title"></param>
        public void AddCrumb(string title)
        {
            AddCrumb(title, string.Empty);
        }

        #endregion

        #region ContextData

        /// <summary>
        /// get data in context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetContextData<T>(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return default(T);

            if (ContextData.Datas.ContainsKey(key))
                return (T)ContextData.Datas[key];

            return default(T);
        }

        #endregion

        #region User

        private Principal _user;
        /// <summary>
        /// current user
        /// </summary>
        public Principal User
        {
            get
            {
                if (_user == null)
                    _user = Context.User as Principal;

                return _user;
            }
        }

        /// <summary>
        /// 用户是否通过认证
        /// </summary>
        public bool IsAuth
        {
            get
            {
                return Context.User.Identity.IsAuthenticated;
            }
        }

        /// <summary>
        /// 当前用户名
        /// </summary>
        public string UserName
        {
            get
            {
                return Context.User.Identity.Name;
            }
        }

        #endregion

        /// <summary>
        /// 当前请求是否是ajax
        /// </summary>
        public bool IsAjaxRequest { get; internal set; }

        public bool IsAsync { get; internal set; }
        /// <summary>
        /// 是否是提交表单
        /// </summary>
        public bool IsPost
        {
            get
            {
                return Context.Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        #region Design

        ///// <summary>
        ///// 是否处于设计模式
        ///// </summary>
        //public bool IsDesignMode { get; set; }

        private List<string> designableSections = new List<string>();
        public List<string> DesignableSections { get { return designableSections; } }

        #endregion

        #region mvc

        #region ViewData

        private Dictionary<string, object> _viewData = new Dictionary<string, object>();
        /// <summary>
        /// datas used in mvc style binding
        /// </summary>
        public Dictionary<string, object> ViewData { get { return _viewData; } }

        /// <summary>
        /// get datas in mvc style binding
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetViewData(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return null;

            if (_viewData.ContainsKey(key))
                return _viewData[key];

            return null;
        }

        /// <summary>
        /// get datas in mvc style binding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetViewData<T>(string key)
        {
            object obj = GetViewData(key);
            if (obj == null)
                return default(T);

            return (T)obj;
        }

        #endregion

        public ControllerContainer ControllerContainer
        {
            get
            {
                MvcModule module = ServiceLocator.Instance.Resolve("Kiss.mvc") as MvcModule;
                if (module != null)
                    return module.Container;

                return null;
            }
        }

        /// <summary>
        /// current mvc controller
        /// </summary>
        public Controller Controller { get; set; }

        #endregion

        private string referrer;
        public string Referrer
        {
            get
            {
                if (referrer == null)
                {
                    Uri refer = Context.Request.UrlReferrer;
                    if (refer != null)
                        referrer = refer.AbsoluteUri;
                    else
                        referrer = string.Empty;
                }
                return referrer;
            }
        }

        private NameValueCollection _referrerQueryString;
        public NameValueCollection ReferrerQueryString
        {
            get
            {
                if (_referrerQueryString == null)
                {
                    Uri refer = Context.Request.UrlReferrer;
                    if (refer != null)
                        _referrerQueryString = StringUtil.DelimitedEquation2NVCollection("&", refer.Query.Trim('?'));
                    else
                        _referrerQueryString = new NameValueCollection();
                }

                return _referrerQueryString;
            }
        }

        private IUrlMappingProvider _urlmapping;
        public IUrlMappingProvider UrlMapping
        {
            get
            {
                if (_urlmapping == null)
                    _urlmapping = UrlMappingModule.Instance.Provider;
                return _urlmapping;
            }
        }

        /// <summary>
        /// get url by UrlMapping name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetUrl(string name)
        {
            if (UrlMapping.Urls.Count == 0)
                return string.Empty;

            if (UrlMapping.Urls.ContainsKey(name))
                return StringUtil.CombinUrl(JContext.Current.Site.VirtualPath, UrlMapping.Urls[name]);

            return string.Empty;
        }

        /// <summary>
        /// get url by UrlMapping name, and replace
        /// </summary>
        /// <param name="name"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public string GetUrl(string name, string replace)
        {
            NameValueCollection nv = StringUtil.CommaDelimitedEquation2NVCollection(replace);

            string url = GetUrl(name);

            foreach (string key in nv.Keys)
            {
                url = url.Replace("[" + key + "]", nv[key]);
            }

            return url;
        }

        /// <summary>
        /// get url by UrlMapping name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="withDomain"></param>
        /// <returns></returns>
        public string GetUrl(string name, bool withDomain)
        {
            if (UrlMapping.Urls.ContainsKey(name))
                return withDomain ? Utility.FormatUrlWithDomain(UrlMapping.Urls[name]) : UrlMapping.Urls[name];
            return string.Empty;
        }

        /// <summary>
        /// combin baseurl with current site's virtual path
        /// </summary>
        /// <param name="baseurl"></param>
        /// <returns></returns>
        public string CombinUrl(string baseurl)
        {
            return StringUtil.CombinUrl(Site.VirtualPath, baseurl);
        }

        /// <summary>
        /// CombinUrl的简写方法
        /// </summary>
        public string url(string baseUrl)
        {
            return CombinUrl(baseUrl);
        }

        private string _theme;
        /// <summary>
        /// current theme
        /// </summary>
        public string Theme
        {
            get
            {
                if (StringUtil.IsNullOrEmpty(_theme))
                    _theme = QueryString["theme"];
                if (StringUtil.IsNullOrEmpty(_theme))
                    _theme = Site.DefaultTheme;
                return _theme;
            }
            set { _theme = value; }
        }

        #region Engine

        private ISite _site;
        /// <summary>
        /// get current site
        /// </summary>
        public ISite Site
        {
            get
            {
                if (_site == null)
                {
                    PluginSetting setting = PluginSettings.Get<HostInitializer>();

                    if (setting.Enable && StringUtil.HasText(setting["type"]))
                    {
                        IHost host = null;
                        try
                        {
                            host = ServiceLocator.Instance.Resolve<IHost>();
                        }
                        catch (Exception ex)
                        {
                            throw new WebException("site farm component is not supported!", ex);
                        }

                        _site = host.CurrentSite;
                    }
                    else
                    {
                        _site = SiteConfig.Instance;
                    }
                }

                return _site;
            }
        }

        #endregion

        #region utils

        public string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        public string HtmlEncode(string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        public string ThemePath
        {
            get
            {
                return CombinUrl(string.Format("/themes/{0}", Theme));
            }
        }

        #endregion
    }
}
