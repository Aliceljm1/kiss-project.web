using System;
using System.Collections.Generic;
using System.Web.UI;
using Kiss.Utils;
using Kiss.Web.UrlMapping;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// menu data will read from urlmapping config or custom menu.
    /// </summary>
    public class Menu : TemplatedControl
    {
        public class FilterEventArgs : EventArgs
        {
            public static readonly new FilterEventArgs Empty = new FilterEventArgs();

            public MenuType Type { get; internal set; }

            public ISite Site { get; internal set; }

            public List<NavigationItem> Items { get; set; }
        }

        public static event EventHandler<FilterEventArgs> Filter;

        protected virtual void OnFilter(FilterEventArgs e)
        {
            EventHandler<FilterEventArgs> handler = Filter;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public static event EventHandler<FilterEventArgs> BeforeFilter;

        static void OnBeforeFilter(FilterEventArgs e)
        {
            EventHandler<FilterEventArgs> handler = BeforeFilter;

            if (handler != null)
            {
                handler(null, e);
            }
        }

        public MenuType Type { get; set; }

        public string Key { get; set; }

        public string ModelKey { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            FilterEventArgs e = new FilterEventArgs();
            e.Type = Type;
            e.Items = GetDataSource(Type, Key);
            e.Site = CurrentSite;

            OnFilter(e);

            JContext.Current.ViewData[ModelKey ?? "menu"] = e.Items;

            base.Render(writer);
        }

        public List<NavigationItem> GetDataSource(MenuType type)
        {
            return GetDataSource(type, null);
        }

        public List<NavigationItem> GetDataSource(MenuType type, string key)
        {
            return GetDataSource(CurrentSite, type, key);
        }

        public static List<NavigationItem> GetDataSource(ISite site, MenuType type, string key)
        {
            JContext jc = JContext.Current;

            List<NavigationItem> list = new List<NavigationItem>();

            int index = jc.Navigation.Index;
            int subIndex = jc.Navigation.SubIndex;
            int subsubIndex = jc.Navigation.SubsubIndex;

            Dictionary<int, NavigationItem> Items = UrlMappingModule.Instance.Provider.GetMenuItemsBySite(site);

            string currentSiteKey = jc.Site.SiteKey;

            // set menu index of root site
            if (site.SiteKey != currentSiteKey)
            {
                foreach (var k in Items.Keys)
                {
                    if (string.Equals(Items[k].Name, currentSiteKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        index = k;
                    }

                    foreach (var k2 in Items[k].Children.Keys)
                    {
                        if (string.Equals(Items[k].Children[k2].Name, currentSiteKey, StringComparison.InvariantCultureIgnoreCase))
                        {
                            index = k;
                            subIndex = k2;
                        }
                    }
                }
            }

            List<int> keys;
            int key_index;

            switch (type)
            {
                case MenuType.TopLevel:
                    keys = new List<int>(Items.Keys);

                    foreach (int i in Items.Keys)
                    {
                        NavigationItem item = Items[i].Clone() as NavigationItem;
                        item.Selected = index == i;
                        item.Url = GetUrl(site, item.Url);

                        key_index = keys.IndexOf(i);

                        item.IsFirst = key_index == 0 || Items[keys[key_index - 1]].IsSeparator;
                        item.IsLast = key_index == Items.Count - 1 || Items[keys[key_index + 1]].IsSeparator;

                        list.Add(item);
                    }
                    break;
                case MenuType.SubLevel:
                    if (Items.ContainsKey(index))
                    {
                        Dictionary<int, NavigationItem> subItems = Items[index].Children;

                        keys = new List<int>(subItems.Keys);

                        foreach (int j in subItems.Keys)
                        {
                            NavigationItem subItem = subItems[j].Clone() as NavigationItem;
                            subItem.Selected = subIndex == j;
                            subItem.Url = GetUrl(site, subItem.Url);
                            subItem.SubItems = new List<NavigationItem>();

                            key_index = keys.IndexOf(j);

                            subItem.IsFirst = key_index == 0 || subItems[keys[key_index - 1]].IsSeparator;
                            subItem.IsLast = key_index == subItems.Count - 1 || subItems[keys[key_index + 1]].IsSeparator;

                            Dictionary<int, NavigationItem> subsub = Items[index].Children[j].Children;
                            List<int> subsub_keys = new List<int>(subsub.Keys);
                            foreach (int k in subsub.Keys)
                            {
                                NavigationItem subsubItem = subsub[k].Clone() as NavigationItem;
                                subsubItem.Selected = subItem.Selected && subsubIndex == k;
                                subsubItem.Url = GetUrl(site, subsubItem.Url);

                                key_index = subsub_keys.IndexOf(k);

                                subsubItem.IsFirst = key_index == 0 || subsub[subsub_keys[key_index - 1]].IsSeparator;
                                subsubItem.IsLast = key_index == subsub.Count - 1 || subsub[subsub_keys[key_index + 1]].IsSeparator;

                                subItem.SubItems.Add(subsubItem);
                            }

                            list.Add(subItem);
                        }
                    }
                    break;
                case MenuType.SubsubLevel:
                    if (Items.ContainsKey(index) && Items[index].Children.ContainsKey(subIndex))
                    {
                        Dictionary<int, NavigationItem> subsub = Items[index].Children[subIndex].Children;
                        List<int> subsub_keys = new List<int>(subsub.Keys);
                        foreach (int k in subsub.Keys)
                        {
                            NavigationItem subsubItem = subsub[k].Clone() as NavigationItem;
                            subsubItem.Selected = subsubIndex == k;
                            subsubItem.Url = GetUrl(site, subsubItem.Url);

                            key_index = subsub_keys.IndexOf(k);

                            subsubItem.IsFirst = key_index == 0 || subsub[subsub_keys[key_index - 1]].IsSeparator;
                            subsubItem.IsLast = key_index == subsub.Count - 1 || subsub[subsub_keys[key_index + 1]].IsSeparator;

                            list.Add(subsubItem);
                        }
                    }
                    break;
                case MenuType.Cascade:

                    keys = new List<int>(Items.Keys);

                    foreach (int i in keys)
                    {
                        NavigationItem item = Items[i].Clone() as NavigationItem;
                        item.Selected = index == i;
                        item.Url = GetUrl(site, item.Url);
                        item.SubItems = new List<NavigationItem>();

                        key_index = keys.IndexOf(i);
                        item.IsFirst = key_index == 0 || Items[keys[key_index - 1]].IsSeparator;
                        item.IsLast = key_index == Items.Count - 1 || Items[keys[key_index + 1]].IsSeparator;

                        Dictionary<int, NavigationItem> sub = Items[i].Children;
                        List<int> sub_keys = new List<int>(sub.Keys);
                        foreach (int j in sub.Keys)
                        {
                            NavigationItem subItem = sub[j].Clone() as NavigationItem;
                            subItem.Selected = item.Selected && subIndex == j;
                            subItem.Url = GetUrl(site, subItem.Url);
                            subItem.SubItems = new List<NavigationItem>();

                            key_index = sub_keys.IndexOf(j);

                            subItem.IsFirst = key_index == 0 || sub[sub_keys[key_index - 1]].IsSeparator;
                            subItem.IsLast = key_index == sub.Count - 1 || sub[sub_keys[key_index + 1]].IsSeparator;

                            Dictionary<int, NavigationItem> subsub = Items[i].Children[j].Children;
                            List<int> subsub_keys = new List<int>(subsub.Keys);
                            foreach (int k in subsub.Keys)
                            {
                                NavigationItem subsubItem = subsub[k].Clone() as NavigationItem;
                                subsubItem.Selected = subItem.Selected && subsubIndex == k;
                                subsubItem.Url = GetUrl(site, subsubItem.Url);

                                key_index = subsub_keys.IndexOf(k);

                                subsubItem.IsFirst = key_index == 0 || subsub[subsub_keys[key_index - 1]].IsSeparator;
                                subsubItem.IsLast = key_index == subsub.Count - 1 || subsub[subsub_keys[key_index + 1]].IsSeparator;

                                subItem.SubItems.Add(subsubItem);
                            }

                            item.SubItems.Add(subItem);
                        }

                        list.Add(item);
                    }
                    break;
                case MenuType.Self:
                    List<UrlMappingItem> items = UrlMappingModule.Instance.Provider.UrlMappings.FindAll(delegate(UrlMappingItem item)
                    {
                        if (StringUtil.HasText(key))
                            return item.Index == index && item.SubIndex == subIndex && item["key"] == key;
                        else
                            return item.Index == index && item.SubIndex == subIndex;
                    });
                    items.Sort();
                    foreach (UrlMappingItem i in items)
                    {
                        SerializerData sd = i.GetSerializerData();
                        NavigationItem nav = new NavigationItem()
                        {
                            Selected = (i.SelfIndex == JContext.Current.Navigation.Url.SelfIndex),
                            Url = StringUtil.CombinUrl(site.VirtualPath, i.UrlTemplate.Replace("[page]", "1")),
                            Title = i.Title
                        };
                        nav.SetSerializerData(sd);
                        list.Add(nav);
                    }
                    break;
                default:
                    break;
            }

            FilterEventArgs e = new FilterEventArgs();
            e.Type = type;
            e.Items = list;
            e.Site = site;

            OnBeforeFilter(e);

            return e.Items;
        }

        private static string GetUrl(ISite site, string url)
        {
            if (StringUtil.IsNullOrEmpty(url))
                return "#";

            if (url.Contains("://"))
                return url;
            else if (url.StartsWith("~"))
                return ServerUtil.ResolveUrl(url);

            return StringUtil.CombinUrl(site.VirtualPath, url);
        }

        public enum MenuType
        {
            /// <summary>
            /// top level
            /// </summary>
            TopLevel = 0,
            /// <summary>
            /// sub level
            /// </summary>
            SubLevel = 1,
            /// <summary>
            /// cascade 
            /// </summary>
            Cascade = 2,
            /// <summary>
            /// page
            /// </summary>
            Self = 3,
            /// <summary>
            /// custom menu
            /// </summary>
            Custom = 4,

            SubsubLevel = 5
        }
    }
}
