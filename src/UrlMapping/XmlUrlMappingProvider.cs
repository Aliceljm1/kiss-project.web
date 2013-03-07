using Kiss.Utils;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Xml;

namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// url mapping config in xml
    /// </summary>
    public class XmlUrlMappingProvider : IUrlMappingProvider
    {
        private const string kCACHE_KEY = "__XmlUrlMappingProvider_cache_key__";

        private UrlMappingConfig config;
        private UrlMappingItemCollection _coll = new UrlMappingItemCollection();
        private UrlMappingItemCollection _manualAdded = new UrlMappingItemCollection();
        private CacheDependency _fileDependency;

        // to help with debugging, provide a property that indicates when the urlmapping
        // data was last refreshed
        private DateTime _latestRefresh;

        private static readonly object sync_lock = new object();

        #region IUrlMappingProvider Members

        /// <summary>
        /// Provides the <see cref="UrlMappingModule" /> with an internally-cached
        /// list of URL templates and redirection mappings processed from items
        /// in an XML file.
        /// </summary>
        /// <returns>The collection of URL redirection mappings</returns>
        public UrlMappingItemCollection UrlMappings
        {
            get
            {
                // we are using a dependency, check to see if we have a 
                // valid collection already processed in cache
                if (HttpContext.Current.Cache[kCACHE_KEY] != null)
                    return _coll;

                // double check lock
                lock (sync_lock)
                {
                    if (HttpContext.Current.Cache[kCACHE_KEY] != null)
                        return _coll;

                    // refresh the url mappings
                    RefreshUrlMappingData();
                }

                return _coll;
            }
        }

        /// <summary>
        /// Accepts a configuration object from the <see cref="UrlMappingModule"/>
        /// and initializes the provider.
        /// </summary>
        /// <param name="config">
        /// the configuration settings typed as a <c>UrlMappingProviderConfiguration</c> object; 
        /// the actual object type may be a subclass of <c>UrlMappingProviderConfiguration</c>.
        /// </param>
        void IUrlMappingProvider.Initialize(UrlMappingConfig config)
        {
            if (config == null)
                throw new ProviderException("Invalid UrlMappingProvider config.");

            // remember configuration settings
            this.config = config;

            // initialize the url mappings
            RefreshUrlMappingData();
        }

        /// <summary>
        /// Implements the IUrlMappingProvider method to refresh internally-cached
        /// URL mappings.
        /// </summary>
        void IUrlMappingProvider.RefreshUrlMappings()
        {
            RefreshUrlMappingData();
        }

        /// <summary>
        /// Returns the date and time the provider most recently refreshed its
        /// data
        /// </summary>
        /// <returns>the most recent refresh time</returns>
        DateTime IUrlMappingProvider.LastRefreshTime { get { return _latestRefresh; } }

        private Dictionary<int, NavigationItem> _menuItems = new Dictionary<int, NavigationItem>();
        public Dictionary<int, NavigationItem> MenuItems
        {
            get
            {
                return _menuItems;
            }
        }

        public Dictionary<int, NavigationItem> GetMenuItemsBySite(IArea site)
        {
            return MenuItems;
        }

        public UrlMappingItemCollection GetUrlsBySite(IArea site)
        {
            return UrlMappings;
        }

        #endregion

        /// <summary>
        /// Refreshes the internally-cached collection of URL templates and redirection mappings.
        /// </summary>
        protected void RefreshUrlMappingData()
        {
            if (_coll != null)
                _coll.Clear();
            else
                _coll = new UrlMappingItemCollection();

            // clear url mapping cache
            UrlMappingModule.Instance._caches.Clear();

            string file = HttpContext.Current.Server.MapPath(config.UrlMappingFile);

            ParseXml(file, _coll, MenuItems, config.IncomingQueryStringBehavior);

            _coll.Merge(_manualAdded);

            // using a file dependency, generate it now
            if (File.Exists(file))
            {
                _fileDependency = new CacheDependency(file);
                HttpContext.Current.Cache.Insert(kCACHE_KEY, "dummyValue", _fileDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }

            // remember the refresh time
            _latestRefresh = DateTime.Now;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (_fileDependency != null)
                _fileDependency.Dispose();
        }

        #endregion

        public void AddMapping(UrlMappingItem item)
        {
            AddMapping(AreaConfig.Instance.AreaKey, item);
        }

        public void AddMapping(string siteKey, UrlMappingItem item)
        {
            _manualAdded.Add(item);
            _coll.Merge(item);
        }

        public static void ParseXml(string file, UrlMappingItemCollection routes, Dictionary<int, NavigationItem> menuItems, IncomingQueryStringBehavior incomingQueryStringBehavior)
        {
            if (!File.Exists(file))
                return;

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(file);
            }
            catch (Exception ex)
            {
                throw new UrlMappingException("The error occurred while loading the route files.  A virtual path is required and the file must be well-formed.", ex);
            }

            menuItems.Clear();

            for (int i = 0; i < xml.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode node = xml.DocumentElement.ChildNodes[i];

                if (node.Name == "menu")
                {
                    NavigationItem menuItem = getMenuItem(node);
                    menuItem.Children = new Dictionary<int, NavigationItem>();

                    menuItems[i] = menuItem;

                    for (int j = 0; j < node.ChildNodes.Count; j++)
                    {
                        XmlNode subNode = node.ChildNodes[j];

                        if (subNode.Name == "menu")
                        {
                            NavigationItem sub_menuItem = getMenuItem(subNode);
                            sub_menuItem.Children = new Dictionary<int, NavigationItem>();

                            menuItems[i].Children[j] = sub_menuItem;

                            for (int k = 0; k < subNode.ChildNodes.Count; k++)
                            {
                                XmlNode subsubNode = subNode.ChildNodes[k];

                                if (subsubNode.Name == "url")
                                {
                                    UrlMappingItem url = getUrlInfo(subsubNode, sub_menuItem, i, j, -1, incomingQueryStringBehavior);

                                    routes.Add(url);
                                }
                                else if (subsubNode.Name == "menu")
                                {
                                    NavigationItem subsub_menuItem = getMenuItem(subsubNode);
                                    sub_menuItem.Children[k] = subsub_menuItem;

                                    foreach (XmlNode last_node in subsubNode.ChildNodes)
                                    {
                                        if (last_node.Name == "url")
                                        {
                                            UrlMappingItem url = getUrlInfo(last_node, subsub_menuItem, i, j, k, incomingQueryStringBehavior);

                                            routes.Add(url);
                                        }
                                    }
                                }
                            }

                        }
                        else if (subNode.Name == "url")
                        {
                            UrlMappingItem url = getUrlInfo(subNode, menuItem, i, -1, -1, incomingQueryStringBehavior);

                            routes.Add(url);
                        }
                    }
                }
                else if (node.Name == "url")
                {
                    UrlMappingItem url = getUrlInfo(node, new NavigationItem(), -1, -1, -1, incomingQueryStringBehavior);

                    routes.Add(url);
                }
            }
        }

        private static UrlMappingItem getUrlInfo(XmlNode node, NavigationItem menuItem, int index, int subIndex, int subsubIndex, IncomingQueryStringBehavior incomingQueryStringBehavior)
        {
            string name = XmlUtil.GetStringAttribute(node, "name", string.Empty);
            string urlTemplate = XmlUtil.GetStringAttribute(node, "template", string.Empty);

            if (string.IsNullOrEmpty(urlTemplate))
                throw new UrlMappingException("There is an XmlUrlMappingModule error.  All <url> tags in the mapping file require a 'template' attribute.");

            string redirection = Utility.GetHref(XmlUtil.GetStringAttribute(node, "href", string.Empty));

            // still here, we can create the item and add to the collection
            UrlMappingItem item
                  = Utility.CreateTemplatedMappingItem(
                    name, urlTemplate, redirection, incomingQueryStringBehavior
                   );
            item.UrlTemplate = urlTemplate;

            // set custom attributes
            foreach (XmlAttribute attr in node.Attributes)
            {
                item[attr.Name] = attr.Value;
            }

            if (XmlUtil.GetStringAttribute(node, "index", string.Empty) == "?")
                item.Index = null;
            else
                item.Index = index;

            if (XmlUtil.GetStringAttribute(node, "subindex", string.Empty) == "?")
                item.SubIndex = null;
            else
                item.SubIndex = subIndex;

            if (XmlUtil.GetStringAttribute(node, "subsubindex", string.Empty) == "?")
                item.SubsubIndex = null;
            else
                item.SubsubIndex = subsubIndex;

            item.Title = XmlUtil.GetStringAttribute(node, "title", menuItem.Title);

            item.Id = XmlUtil.GetStringAttribute(node, "id", null);
            item.Action = XmlUtil.GetStringAttribute(node, "action", null);

            return item;
        }

        private static NavigationItem getMenuItem(XmlNode node)
        {
            NavigationItem item = new NavigationItem();

            item.Name = XmlUtil.GetStringAttribute(node, "id", null);
            item.Url = XmlUtil.GetStringAttribute(node, "url", string.Empty);
            item.Title = XmlUtil.GetStringAttribute(node, "title", string.Empty);

            foreach (XmlAttribute attr in node.Attributes)
            {
                item[attr.Name] = attr.Value;
            }

            return item;
        }
    }
}
