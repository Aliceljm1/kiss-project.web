using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using Kiss.Utils;
using Kiss.Web.UrlMapping;

namespace Kiss.Web.Area
{
    public class AreaUrlMappingProvider : IUrlMappingProvider
    {
        private const string kCACHE_KEY = "__AreaUrlMappingProvider_cache_key__";
        private UrlMappingConfig config;
        private Dictionary<string, UrlMappingItemCollection> _urlMappings = new Dictionary<string, UrlMappingItemCollection>();
        private Dictionary<string, Dictionary<int, NavigationItem>> _menuItems = new Dictionary<string, Dictionary<int, NavigationItem>>();
        private Dictionary<string, Dictionary<string, string>> _urls = new Dictionary<string, Dictionary<string, string>>();
        private UrlMappingItemCollection _manualGlobalRoutes = new UrlMappingItemCollection();
        private Dictionary<string, UrlMappingItemCollection> _manualItems = new Dictionary<string, UrlMappingItemCollection>();
        private static readonly object _synclock = new object();

        private ILogger _logger;
        private ILogger logger
        {
            get
            {
                if (_logger == null)
                    _logger = LogManager.GetLogger<AreaUrlMappingProvider>();
                return _logger;
            }
        }

        private DateTime _latestRefresh;

        private CacheDependency _fileDependency;

        public void Initialize(UrlMappingConfig config)
        {
            AssertUtils.ArgumentNotNull(config, "urlmappingConfig");

            this.config = config;

            RefreshUrlMappingData();
        }

        public DateTime LastRefreshTime
        {
            get { return _latestRefresh; }
        }

        public void RefreshUrlMappings()
        {
            RefreshUrlMappingData();
        }

        public UrlMappingItemCollection UrlMappings
        {
            get
            {
                RefreshUrlMappingData();

                ISite site = JContext.Current.Site;

                if (!_urlMappings.ContainsKey(site.SiteKey))
                {
                    logger.Info("routes not exist! site={0}", site.VirtualPath);
                    return new UrlMappingItemCollection();
                }

                if (_manualItems.ContainsKey(site.SiteKey))
                    return UrlMappingItemCollection.Combin(_manualGlobalRoutes, UrlMappingItemCollection.Combin(_manualItems[site.SiteKey], _urlMappings[site.SiteKey]));

                return UrlMappingItemCollection.Combin(_manualGlobalRoutes, _urlMappings[site.SiteKey]);
            }
        }

        public Dictionary<int, NavigationItem> MenuItems { get { return GetMenuItemsBySite(JContext.Current.Site); } }

        public Dictionary<int, NavigationItem> GetMenuItemsBySite(ISite site)
        {
            RefreshUrlMappingData();

            if (!_menuItems.ContainsKey(site.SiteKey))
            {
                logger.Info("menu not exist! site={0}", site.VirtualPath);
                return new Dictionary<int, NavigationItem>();
            }

            return _menuItems[site.SiteKey];
        }

        public Dictionary<string, string> GetUrlsBySite(ISite site)
        {
            RefreshUrlMappingData();

            if (!_urls.ContainsKey(site.SiteKey))
            {
                logger.Info("url not exist! site={0}", site.VirtualPath);
                return new Dictionary<string, string>();
            }

            return _urls[site.SiteKey];
        }

        public Dictionary<string, string> Urls { get { return GetUrlsBySite(JContext.Current.Site); } }

        protected void RefreshUrlMappingData()
        {
            if (HttpContext.Current.Cache[kCACHE_KEY] != null)
                return;

            lock (_synclock)
            {
                if (HttpContext.Current.Cache[kCACHE_KEY] != null) return;

                _urlMappings.Clear();
                _menuItems.Clear();
                _urls.Clear();

                // clear url mapping cache
                UrlMappingModule.Instance._caches.Clear();

                List<string> routefiles = new List<string>();

                string root = ServerUtil.MapPath("~");

                foreach (var item in AreaInitializer.Areas.Keys)
                {
                    if (item.Equals("/"))
                        routefiles.Add(Path.Combine(root, "App_Data" + Path.DirectorySeparatorChar + "routes.config"));
                    else
                        routefiles.Add(Path.Combine(root, item.Substring(1) + Path.DirectorySeparatorChar + "routes.config"));
                }

                foreach (var item in routefiles)
                {
                    string vp = Path.GetFileName(Path.GetDirectoryName(item)).ToLowerInvariant();
                    if (string.Equals(vp, "App_Data", StringComparison.InvariantCultureIgnoreCase))
                        vp = "/";
                    else
                        vp = "/" + vp;

                    if (!AreaInitializer.Areas.ContainsKey(vp))
                        throw new WebException("virtual path not found: {0}", vp);

                    ISite site = AreaInitializer.Areas[vp];

                    UrlMappingItemCollection routes = new UrlMappingItemCollection();
                    Dictionary<string, string> urls = new Dictionary<string, string>();
                    Dictionary<int, NavigationItem> menus = new Dictionary<int, NavigationItem>();

                    XmlUrlMappingProvider.ParseXml(item, routes, menus, urls, IncomingQueryStringBehavior.PassThrough);

                    _urlMappings[site.SiteKey] = routes;
                    _menuItems[site.SiteKey] = menus;
                    _urls[site.SiteKey] = urls;
                }

                _fileDependency = new CacheDependency(routefiles.ToArray());
                HttpRuntime.Cache.Insert(kCACHE_KEY, "dummyValue", _fileDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                _latestRefresh = DateTime.Now;
            }
        }

        public void Dispose()
        {
        }

        public void AddMapping(UrlMappingItem item)
        {
            _manualGlobalRoutes.Merge(item);
        }

        public void AddMapping(string siteKey, UrlMappingItem item)
        {
            UrlMappingItemCollection coll;

            if (_manualItems.ContainsKey(siteKey))
                coll = _manualItems[siteKey];
            else
            {
                coll = new UrlMappingItemCollection();
                _manualItems[siteKey] = coll;
            }

            coll.Merge(item);
        }
    }
}
