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
        private Dictionary<ISite, UrlMappingItemCollection> _urlMappings = new Dictionary<ISite, UrlMappingItemCollection>();
        private Dictionary<ISite, Dictionary<int, NavigationItem>> _menuItems = new Dictionary<ISite, Dictionary<int, NavigationItem>>();
        private Dictionary<ISite, Dictionary<string, string>> _urls = new Dictionary<ISite, Dictionary<string, string>>();
        private UrlMappingItemCollection _manualGlobalRoutes = new UrlMappingItemCollection();
        private Dictionary<string, UrlMappingItemCollection> _manualItems = new Dictionary<string, UrlMappingItemCollection>();

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
                if (HttpContext.Current.Cache[kCACHE_KEY] == null)
                    RefreshUrlMappingData();

                ISite site = JContext.Current.Site;

                if (!_urlMappings.ContainsKey(site))
                {
                    logger.Info("routes not exist! site={0}", site.VirtualPath);
                    return new UrlMappingItemCollection();
                }

                if (_manualItems.ContainsKey(site.SiteKey))
                    return UrlMappingItemCollection.Combin(_manualGlobalRoutes, UrlMappingItemCollection.Combin(_manualItems[site.SiteKey], _urlMappings[site]));

                return UrlMappingItemCollection.Combin(_manualGlobalRoutes, _urlMappings[site]);
            }
        }

        public Dictionary<int, NavigationItem> MenuItems { get { return GetMenuItemsBySite(JContext.Current.Site); } }

        public Dictionary<int, NavigationItem> GetMenuItemsBySite(ISite site)
        {
            if (HttpContext.Current.Cache[kCACHE_KEY] == null)
                RefreshUrlMappingData();

            if (!_menuItems.ContainsKey(site))
            {
                logger.Info("menu not exist! site={0}", site.VirtualPath);
                return new Dictionary<int, NavigationItem>();
            }

            return _menuItems[site];
        }

        public Dictionary<string, string> GetUrlsBySite(ISite site)
        {
            if (HttpContext.Current.Cache[kCACHE_KEY] == null)
                RefreshUrlMappingData();

            if (!_urls.ContainsKey(site))
            {
                logger.Info("url not exist! site={0}", site.VirtualPath);
                return new Dictionary<string, string>();
            }

            return _urls[site];
        }

        public Dictionary<string, string> Urls { get { return GetUrlsBySite(JContext.Current.Site); } }

        protected void RefreshUrlMappingData()
        {
            _urlMappings.Clear();
            _menuItems.Clear();
            _urls.Clear();

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

                _urlMappings[site] = routes;
                _menuItems[site] = menus;
                _urls[site] = urls;
            }

            _fileDependency = new CacheDependency(routefiles.ToArray());
            HttpRuntime.Cache.Insert(kCACHE_KEY, "dummyValue", _fileDependency);

            _latestRefresh = DateTime.Now;
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
