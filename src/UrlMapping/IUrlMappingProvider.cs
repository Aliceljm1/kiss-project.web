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

        /// <summary>
        /// url mapping name, template dictionary
        /// </summary>
        Dictionary<string, string> Urls { get; }

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

        void AddMapping(UrlMappingItem item);
    }
}
