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

        public MenuType Type { get; set; }

        public string Key { get; set; }

        public string ModelKey { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            FilterEventArgs e = new FilterEventArgs();
            e.Type = Type;
            e.Items = GetDataSource(Type, Key);

            OnFilter(e);

            JContext.Current.ViewData[ModelKey ?? "menu"] = e.Items;

            base.Render(writer);
        }

        public static List<NavigationItem> GetDataSource(MenuType type)
        {
            return GetDataSource(type, null);
        }

        public static List<NavigationItem> GetDataSource(MenuType type, string key)
        {
            JContext jc = JContext.Current;

            List<NavigationItem> list = new List<NavigationItem>();
            int index = JContext.Current.Navigation.Index;
            int subIndex = JContext.Current.Navigation.SubIndex;

            Dictionary<int, NavigationItem> Items = UrlMappingModule.Instance.Provider.MenuItems;

            List<int> keys;
            int key_index;

            switch (type)
            {
                case MenuType.TopLevel:
                    keys = new List<int>(Items.Keys);

                    foreach (int i in Items.Keys)
                    {
                        if (!Items[i].Visible)
                            continue;

                        NavigationItem item = Items[i].Clone() as NavigationItem;
                        item.Selected = index == i;
                        item.Url = GetUrl(jc, item.Url);

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

                        foreach (int i in subItems.Keys)
                        {
                            if (!subItems[i].Visible)
                                continue;

                            NavigationItem item = subItems[i].Clone() as NavigationItem;
                            item.Selected = subIndex == i;
                            item.Url = GetUrl(jc, item.Url);

                            key_index = keys.IndexOf(i);

                            item.IsFirst = key_index == 0 || subItems[keys[key_index - 1]].IsSeparator;
                            item.IsLast = key_index == subItems.Count - 1 || subItems[keys[key_index + 1]].IsSeparator;

                            list.Add(item);
                        }
                    }
                    break;
                case MenuType.Cascade:

                    keys = new List<int>(Items.Keys);

                    foreach (int i in keys)
                    {
                        if (!Items[i].Visible) continue;

                        NavigationItem item = Items[i].Clone() as NavigationItem;
                        item.Selected = index == i;
                        item.Url = GetUrl(jc, item.Url);
                        item.SubItems = new List<NavigationItem>();

                        key_index = keys.IndexOf(i);
                        item.IsFirst = key_index == 0 || Items[keys[key_index - 1]].IsSeparator;
                        item.IsLast = key_index == Items.Count - 1 || Items[keys[key_index + 1]].IsSeparator;

                        Dictionary<int, NavigationItem> children = Items[i].Children;
                        List<int> sub_keys = new List<int>(children.Keys);
                        foreach (int j in children.Keys)
                        {
                            if (!children[j].Visible) continue;
                            NavigationItem subItem = children[j].Clone() as NavigationItem;
                            subItem.Selected = item.Selected && subIndex == j;
                            subItem.Url = GetUrl(jc, subItem.Url);

                            key_index = sub_keys.IndexOf(j);

                            subItem.IsFirst = key_index == 0 || children[sub_keys[key_index - 1]].IsSeparator;
                            subItem.IsLast = key_index == children.Count - 1 || children[sub_keys[key_index + 1]].IsSeparator;

                            item.SubItems.Add(subItem);
                        }

                        list.Add(item);
                    }
                    break;
                case MenuType.Self:
                    ISite site = JContext.Current.Site;
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
                            Title = i.Title,
                            Desc = i.Desc
                        };
                        nav.SetSerializerData(sd);
                        list.Add(nav);
                    }
                    break;
                default:
                    break;
            }

            return list;
        }

        private static string GetUrl(JContext jc, string url)
        {
            if (StringUtil.IsNullOrEmpty(url))
                return "#";

            if (url.Contains("://"))
                return url;
            else if (url.StartsWith("~"))
                return ServerUtil.ResolveUrl(url);

            return jc.CombinUrl(url);
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
            Custom = 4
        }
    }
}
