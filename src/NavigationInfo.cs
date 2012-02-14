﻿using System;
using System.Collections.Generic;
using System.IO;
using Kiss.Utils;
using Kiss.Web.UrlMapping;

namespace Kiss.Web
{
    /// <summary>
    /// navigation info
    /// </summary>
    public class NavigationInfo : ExtendedAttributes
    {
        public int Index { get; set; }
        public int SubIndex { get; set; }

        private string _title;
        public string Title
        {
            get
            {
                if (StringUtil.HasText(_title) && _title.Contains("$"))
                {
                    ITemplateEngine te = ServiceLocator.Instance.Resolve<ITemplateEngine>();
                    using (StringWriter sw = new StringWriter())
                    {
                        te.Process(JContext.Current.ViewData, "title", sw, _title);
                        _title = sw.GetStringBuilder().ToString();
                    }
                }

                return _title;
            }
            set { _title = value; }
        }
        public string Desc { get; set; }

        public string Name { get; set; }
        public bool OK { get; private set; }

        protected IUrlMappingProvider Provider
        {
            get
            {
                return UrlMappingModule.Instance.Provider;
            }
        }

        public UrlMapping.UrlMappingItem Url { get; private set; }

        /// <summary>
        /// top menu
        /// </summary>
        public NavigationItem Menu
        {
            get
            {
                if (!Provider.MenuItems.ContainsKey(Index))
                    return null;

                return Provider.MenuItems[Index];
            }
        }

        /// <summary>
        /// sub menu
        /// </summary>
        public NavigationItem SubMenu
        {
            get
            {
                if (Menu == null)
                    return null;

                if (Menu.Children != null && Menu.Children.ContainsKey(SubIndex))
                    return Menu.Children[SubIndex];

                return null;
            }
        }

        /// <summary>
        /// parent menu. sub menu or top menu
        /// </summary>
        public NavigationItem ParentMenu
        {
            get
            {
                return SubMenu ?? Menu;
            }
        }

        private string _id;
        /// <summary>
        /// current url's id
        /// </summary>
        public string Id
        {
            get
            {
                if (_id == null)
                {
                    if (!OK)
                        return string.Empty;

                    if (StringUtil.IsNullOrEmpty(Url.Id))
                    {
                        if (ParentMenu != null)
                            _id = ParentMenu.Name;

                        if (StringUtil.IsNullOrEmpty(_id))
                        {
                            int index = Url.UrlTemplate.IndexOf("/");
                            if (index == -1)
                                _id = Url.UrlTemplate;
                            else
                                _id = Url.UrlTemplate.Substring(0, index);
                        }
                    }
                    else
                    {
                        if (Url.Id.StartsWith("?"))
                        {
                            string k = Url.Id.Substring(1);
                            _id = JContext.Current.QueryString[k];
                            if (StringUtil.IsNullOrEmpty(_id))
                                _id = JContext.Current.Context.Request.Params[k];
                        }
                        else
                            _id = Url.Id;
                    }
                }

                return _id;
            }
        }

        public string LanguageCode { get; set; }

        private string _action;
        public string Action
        {
            get
            {
                if (_action == null)
                {
                    if (!OK)
                        _action = string.Empty;
                    else if (StringUtil.IsNullOrEmpty(Url.Action))
                    {
                        string url = Url.UrlTemplate;
                        int index = url.IndexOf(Id, StringComparison.InvariantCultureIgnoreCase);

                        if (index == -1)
                        {
                            index = url.IndexOf("/");
                            if (index == -1)
                                _action = string.Empty;
                            else
                                url = url.Remove(0, index).Trim('/');
                        }
                        else
                        {
                            url = url.Substring(index + Id.Length).Trim('/');
                        }

                        index = url.IndexOf("/");
                        if (index == -1)
                            _action = url;
                        else
                            _action = url.Substring(0, index);
                    }
                    else
                    {
                        if (Url.Action.StartsWith("?"))
                        {
                            string k = Url.Action.Substring(1);
                            _action = JContext.Current.QueryString[k];
                            if (StringUtil.IsNullOrEmpty(_action))
                                _action = JContext.Current.Context.Request.Params[k];
                        }
                        else
                            _action = Url.Action;
                    }

                    if (StringUtil.IsNullOrEmpty(_action))
                        _action = "index";

                    int dotindex = _action.IndexOf(".");
                    if (dotindex != -1)
                        _action = _action.Remove(dotindex);

                    if (string.Equals(_action, "default", StringComparison.InvariantCultureIgnoreCase))
                        _action = "index";
                }

                return _action;
            }
        }

        /// <summary>
        /// current url's icon
        /// </summary>
        public string Icon
        {
            get
            {
                if (!Provider.MenuItems.ContainsKey(Index))
                    return string.Empty;

                NavigationItem item = Provider.MenuItems[Index];

                if (item.Children != null && item.Children.ContainsKey(SubIndex))
                {
                    NavigationItem subItem = item.Children[SubIndex];
                    return subItem.Icon ?? item.Icon;
                }

                return item.Icon;
            }
        }

        public bool Set(UrlMapping.UrlMappingItem item, string requesturl)
        {
            Url = item;

            string url;

            if (item.Index == null || item.SubIndex == null)
            {
                Dictionary<int, NavigationItem> menuItems = UrlMapping.UrlMappingModule.Instance.Provider.MenuItems;

                if (item.Index == null)
                {
                    double max = 0;
                    int maxi = 0, maxj = 0;
                    foreach (int i in menuItems.Keys)
                    {
                        double d = 0;
                        if (menuItems[i].Children != null && menuItems[i].Children.Count > 0)
                        {
                            foreach (int j in menuItems[i].Children.Keys)
                            {
                                NavigationItem ni = menuItems[i].Children[j];

                                url = ni.Url.TrimStart('/');
                                if (string.IsNullOrEmpty(url))
                                    continue;
                                d = StringUtil.Similarity(requesturl, url);
                                if (d > max)
                                {
                                    max = d;

                                    maxi = i;
                                    maxj = j;
                                }
                            }
                        }

                        url = menuItems[i].Url.TrimStart('/');
                        if (string.IsNullOrEmpty(url))
                            continue;
                        d = StringUtil.Similarity(requesturl, url);
                        if (d > max)
                        {
                            max = d;

                            maxi = i;
                            maxj = -1;
                        }
                    }

                    if (max > 0)
                    {
                        Index = maxi;
                        SubIndex = maxj;
                    }
                    else
                        return false;
                }
                else
                {
                    Index = item.Index.Value;

                    if (item.SubIndex == null)
                    {
                        int i = 0;
                        double max = 0;
                        int maxi = 0;
                        foreach (NavigationItem ni in menuItems[item.Index.Value].Children.Values)
                        {
                            url = ni.Url.TrimStart('/');
                            if (string.IsNullOrEmpty(url))
                                continue;
                            double d = StringUtil.Similarity(requesturl, ni.Url);
                            if (d > max)
                            {
                                max = d;
                                maxi = i;
                            }

                            i++;
                        }

                        if (max > 0)
                            SubIndex = maxi;
                        else
                            return false;
                    }
                }
            }
            else
            {
                Index = item.Index.Value;
                SubIndex = item.SubIndex.Value;
            }
            Title = item.Title;
            Desc = item.Desc;
            Name = item.Name;

            foreach (string key in Url.Keys)
            {
                SetExtendedAttribute(key, Url[key]);
            }

            OK = true;

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Id, Action);
        }
    }
}
