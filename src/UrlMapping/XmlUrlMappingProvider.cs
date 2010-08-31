using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Kiss.Utils;

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

        #region IUrlMappingProvider Members

        /// <summary>
        /// Provides the <see cref="UrlMappingModule" /> with an internally-cached
        /// list of URL templates and redirection mappings processed from items
        /// in an XML file.
        /// </summary>
        /// <returns>The collection of URL redirection mappings</returns>
        UrlMappingItemCollection IUrlMappingProvider.UrlMappings
        {
            get
            {
                // if we aren't using Dependencies, then return the collection that 
                // was generated upon initialization
                if (!config.UseDependency)
                    return _coll;

                // if we are using a dependency, check to see if we have a 
                // valid collection already processed in cache
                if (HttpContext.Current.Cache[kCACHE_KEY] != null)
                    return _coll;

                // if not, we need to refresh the url mappings from sql
                RefreshUrlMappingData();
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

        public Dictionary<int, NavigationItem> GetMenuItemsBySite(ISite site)
        {
            return MenuItems;
        }

        private Dictionary<string, string> _urls = new Dictionary<string, string>();
        public Dictionary<string, string> Urls
        {
            get { return _urls; }
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

            string file = HttpContext.Current.Server.MapPath(config.UrlMappingFile);

            ParseXml(file, _coll, MenuItems, Urls, config.IncomingQueryStringBehavior);

            _coll.Merge(_manualAdded);

            // if we're using a file dependency, generate it now
            if (config.UseDependency)
            {
                try
                {
                    string dir = Path.GetDirectoryName(file);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    _fileDependency = new CacheDependency(file);
                    HttpContext.Current.Cache.Insert(kCACHE_KEY, "dummyValue", _fileDependency);
                }
                catch (Exception ex)
                {
                    throw new UrlMappingException("There is an XmlUrlMappingModule error. create cache dependency failed.", ex);
                }
            }

            // remember the refresh time
            _latestRefresh = DateTime.Now;
        }

        private static NavigationInfo GetMenuInfo(XmlNode node, Dictionary<int, NavigationItem> menuItems)
        {
            NavigationInfo menuItem = new NavigationInfo() { Index = -1, SubIndex = -1 };

            XmlNode subMenuNode, menuNode;

            XmlNode pNode = node.ParentNode;
            if (pNode == null || pNode.Name != "menu")
                return menuItem;

            // get parent node's title
            menuItem.Title = XmlUtil.GetStringAttribute(pNode, "title", string.Empty);
            menuItem.Desc = XmlUtil.GetStringAttribute(pNode, "desc", string.Empty);

            if (pNode.ParentNode == null || pNode.ParentNode.Name != "menu")
            {
                menuNode = pNode;
                subMenuNode = null;
            }
            else
            {
                subMenuNode = pNode;
                menuNode = pNode.ParentNode;
            }

            menuItem.Index = XmlUtil.GetIntAttribute(menuNode, "index", XmlUtil.FindElementIndex(menuNode));
            if (menuItem.Index == -1)
                return menuItem;

            if (!menuItems.ContainsKey(menuItem.Index))
            {
                NavigationItem item = GetMenuItem(menuNode);
                item.Children = new Dictionary<int, NavigationItem>();

                menuItems[menuItem.Index] = item;
            }

            if (subMenuNode != null)
            {
                menuItem.SubIndex = XmlUtil.GetIntAttribute(subMenuNode, "index", XmlUtil.FindElementIndex(subMenuNode as XmlElement));
                if (menuItem.SubIndex == -1)
                    return menuItem;

                NavigationItem item = menuItems[menuItem.Index];
                if (!item.Children.ContainsKey(menuItem.SubIndex))
                {
                    NavigationItem subItem = GetMenuItem(subMenuNode);

                    item.Children[menuItem.SubIndex] = subItem;
                }
            }
            else
            {
                menuItem.SubIndex = -1;
            }

            return menuItem;
        }

        private static NavigationItem GetMenuItem(XmlNode node)
        {
            NavigationItem item = new NavigationItem();

            item.Name = XmlUtil.GetStringAttribute(node, "id", null);
            item.Url = XmlUtil.GetStringAttribute(node, "url", string.Empty);
            item.Title = XmlUtil.GetStringAttribute(node, "title", string.Empty);
            item.Icon = XmlUtil.GetStringAttribute(node, "icon", null);

            foreach (XmlAttribute attr in node.Attributes)
            {
                item[attr.Name] = attr.Value;
            }

            return item;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (config.UseDependency)
            {
                _fileDependency.Dispose();
            }
        }

        #endregion

        public void AddMapping(UrlMappingItem item)
        {
            AddMapping(SiteConfig.Instance.SiteKey, item);
        }

        public void AddMapping(string siteKey, UrlMappingItem item)
        {
            _manualAdded.Add(item);
            _coll.Merge(item);
        }

        public static void ParseXml(string file, UrlMappingItemCollection routes, Dictionary<int, NavigationItem> menuItems, Dictionary<string, string> urls, IncomingQueryStringBehavior incomingQueryStringBehavior)
        {
            if (File.Exists(file))
            {
                // parse the given xml file to retrieve the listing of URL items;
                // the xml file should include tags in the form:
                // <url name="" template="" href="" />
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
                urls.Clear();

                UrlMappingItem lastItem = null;

                // parse the file for <urlMapping> tags
                foreach (XmlNode node in xml.SelectNodes("//url"))
                {
                    // retrieve name, urlTemplate, and redirection attributes;
                    // ensure urlTemplate and redirection are present
                    string name = XmlUtil.GetStringAttribute(node, "name", string.Empty);
                    string urlTemplate = XmlUtil.GetStringAttribute(node, "template", string.Empty);
                    string redirection = Utility.GetHref(XmlUtil.GetStringAttribute(node, "href", string.Empty));

                    if (string.IsNullOrEmpty(urlTemplate))
                        throw new UrlMappingException("There is an XmlUrlMappingModule error.  All <url> tags in the mapping file require a 'template' attribute.");

                    // still here, we can create the item and add to the collection
                    UrlMappingItem item
                          = Utility.CreateTemplatedMappingItem(
                            name, urlTemplate, redirection, incomingQueryStringBehavior
                           );
                    item.UrlTemplate = urlTemplate;

                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        item[attr.Name] = attr.Value;
                    }

                    NavigationInfo menuItem = GetMenuInfo(node, menuItems);

                    // get info from menu
                    item.Index = menuItem.Index;
                    item.SubIndex = menuItem.SubIndex;
                    item.Title = XmlUtil.GetStringAttribute(node, "title", menuItem.Title);
                    item.Desc = XmlUtil.GetStringAttribute(node, "desc", menuItem.Desc);

                    item.Id = XmlUtil.GetStringAttribute(node, "id", null);
                    item.Action = XmlUtil.GetStringAttribute(node, "action", null);

                    routes.Add(item);

                    if (StringUtil.HasText(item.Name))
                        urls.Add(item.Name, item.UrlTemplate);

                    // set self index
                    if (lastItem == null)
                    {
                        item.SelfIndex = 0;
                    }
                    else
                    {
                        if (lastItem.Index == item.Index && lastItem.SubIndex == item.SubIndex)
                            item.SelfIndex = lastItem.SelfIndex + 1;
                        else
                            item.SelfIndex = 0;
                    }
                    lastItem = item;
                }
            }
        }
    }
}
