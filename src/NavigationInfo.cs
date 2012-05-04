using System;
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
        public int SubsubIndex { get; set; }

        private string _title;
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_title) && ParentMenu != null)
                    _title = ParentMenu.Title;

                if (!string.IsNullOrEmpty(_title) && _title.Contains("$"))
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

        public string Name { get; set; }
        public bool OK { get; private set; }

        public NavigationItem ParentMenu
        {
            get
            {
                if (!OK) return null;

                if (SubsubIndex >= 0)
                    return Provider.MenuItems[Index].Children[SubIndex].Children[SubsubIndex];

                if (SubIndex >= 0)
                    return Provider.MenuItems[Index].Children[SubIndex];

                if (Index >= 0)
                    return Provider.MenuItems[Index];

                return null;
            }
        }

        public List<NavigationItem> Crumbs
        {
            get
            {
                List<NavigationItem> list = new List<NavigationItem>();

                if (!OK) return list;

                if (Index >= 0)
                    list.Add(Provider.MenuItems[Index].Clone() as NavigationItem);

                if (SubIndex >= 0)
                    list.Add(Provider.MenuItems[Index].Children[SubIndex].Clone() as NavigationItem);

                if (SubsubIndex >= 0)
                    list.Add(Provider.MenuItems[Index].Children[SubIndex].Children[SubsubIndex].Clone() as NavigationItem);

                return list;
            }
        }

        protected IUrlMappingProvider Provider
        {
            get
            {
                return UrlMappingModule.Instance.Provider;
            }
        }

        public UrlMapping.UrlMappingItem Url { get; private set; }

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

        public bool Set(UrlMapping.UrlMappingItem item, string requesturl)
        {
            Url = item;

            string url;

            if (item.Index == null || item.SubIndex == null || item.SubsubIndex == null)
            {
                Dictionary<int, NavigationItem> menuItems = UrlMapping.UrlMappingModule.Instance.Provider.MenuItems;

                if (item.Index == null)
                {
                    double max = 0;
                    int maxi = 0, maxj = 0, maxk = 0;
                    foreach (int i in menuItems.Keys)
                    {
                        double d = 0;

                        foreach (int j in menuItems[i].Children.Keys)
                        {
                            NavigationItem ni = menuItems[i].Children[j];

                            foreach (int k in ni.Children.Keys)
                            {
                                NavigationItem nii = ni.Children[k];

                                url = trimUrl(nii.Url);
                                if (string.IsNullOrEmpty(url)) continue;

                                d = StringUtil.Similarity(requesturl, url);
                                if (d > max)
                                {
                                    max = d;

                                    maxi = i;
                                    maxj = j;
                                    maxk = k;
                                }
                            }

                            url = trimUrl(ni.Url);
                            if (string.IsNullOrEmpty(url))
                                continue;

                            d = StringUtil.Similarity(requesturl, url);
                            if (d > max)
                            {
                                max = d;

                                maxi = i;
                                maxj = j;
                                maxk = -1;
                            }
                        }

                        url = trimUrl(menuItems[i].Url);
                        if (string.IsNullOrEmpty(url))
                            continue;

                        d = StringUtil.Similarity(requesturl, url);
                        if (d > max)
                        {
                            max = d;

                            maxi = i;
                            maxj = -1;
                            maxk = -1;
                        }
                    }

                    if (max > 0)
                    {
                        Index = maxi;
                        SubIndex = maxj;
                        SubsubIndex = maxk;
                    }
                    else
                        return false;
                }
                else if (item.SubIndex == null)
                {
                    Index = item.Index.Value;

                    double max = 0;
                    int maxj = 0, maxk = 0;

                    double d = 0;

                    foreach (int j in menuItems[Index].Children.Keys)
                    {
                        NavigationItem ni = menuItems[Index].Children[j];

                        foreach (int k in ni.Children.Keys)
                        {
                            NavigationItem nii = ni.Children[k];

                            url = trimUrl(nii.Url);
                            if (string.IsNullOrEmpty(url)) continue;

                            d = StringUtil.Similarity(requesturl, url);
                            if (d > max)
                            {
                                max = d;

                                maxj = j;
                                maxk = k;
                            }
                        }

                        url = trimUrl(ni.Url);
                        if (string.IsNullOrEmpty(url))
                            continue;

                        d = StringUtil.Similarity(requesturl, url);
                        if (d > max)
                        {
                            max = d;

                            maxj = j;
                            maxk = -1;
                        }
                    }

                    if (max > 0)
                    {
                        SubIndex = maxj;
                        SubsubIndex = maxk;
                    }
                    else
                        return false;
                }
                else
                {
                    double max = 0;
                    int maxk = 0;

                    double d = 0;

                    NavigationItem ni = menuItems[Index].Children[SubIndex];

                    foreach (int k in ni.Children.Keys)
                    {
                        NavigationItem nii = ni.Children[k];

                        url = trimUrl(nii.Url);
                        if (string.IsNullOrEmpty(url)) continue;

                        d = StringUtil.Similarity(requesturl, url);
                        if (d > max)
                        {
                            max = d;

                            maxk = k;
                        }
                    }

                    if (max > 0)
                    {
                        SubsubIndex = maxk;
                    }
                    else
                        return false;
                }
            }
            else
            {
                Index = item.Index.Value;
                SubIndex = item.SubIndex.Value;
                SubsubIndex = item.SubsubIndex.Value;
            }

            Title = item.Title;
            Name = item.Name;

            foreach (string key in Url.Keys)
            {
                SetExtendedAttribute(key, Url[key]);
            }

            OK = true;

            return true;
        }

        static string trimUrl(string url)
        {
            return new Url(url).Path.TrimStart('/');
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Id, Action);
        }
    }
}
