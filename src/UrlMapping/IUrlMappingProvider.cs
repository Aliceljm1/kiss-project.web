using System;
using System.Collections.Generic;

namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// interface to get url related info.
    /// </summary>
    public interface IUrlMappingProvider : IDisposable
    {
        /// <summary>
        /// url mappings
        /// </summary>
        UrlMappingItemCollection UrlMappings { get; }

        /// <summary>
        /// menu items
        /// </summary>
        Dictionary<int, NavigationItem> MenuItems { get; }

        /// <summary>
        /// get menu items by site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Dictionary<int, NavigationItem> GetMenuItemsBySite(ISite site);

        UrlMappingItemCollection GetUrlsBySite(ISite site);

        /// <summary>
        /// refresh settings
        /// </summary>
        void RefreshUrlMappings();

        /// <summary>
        /// last refresh settings time
        /// </summary>
        DateTime LastRefreshTime { get; }

        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="config"></param>
        void Initialize(UrlMappingConfig config);

        /// <summary>
        /// add global routes
        /// </summary>
        /// <param name="item"></param>
        void AddMapping(UrlMappingItem item);

        /// <summary>
        /// add site only routes
        /// </summary>
        /// <param name="siteKey"></param>
        /// <param name="item"></param>
        void AddMapping(string siteKey, UrlMappingItem item);
    }
}
