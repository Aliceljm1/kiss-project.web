using System;
using System.IO;
using Kiss.Utils;
using Kiss.Web.UrlMapping;

namespace Kiss.Web
{
    /// <summary>
    /// navigation info
    /// </summary>
    public class NavigationInfo
    {
        public int Index { get; set; }
        public int SubIndex { get; set; }

        private string _title;
        public string Title
        {
            get
            {
                if (StringUtil.HasText(_title) && _title.StartsWith("$"))
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

        public string this[string key]
        {
            get
            {
                if (Url != null)
                    return Url[key];
                return string.Empty;
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
                        _id = string.Empty;
                    else if (StringUtil.IsNullOrEmpty(Url.Id))
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
                        _id = Url.Id;
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
                            string act = Url.Action.Substring(1);
                            JContext jc = JContext.Current;
                            _action = jc.QueryString[act];
                            if (StringUtil.IsNullOrEmpty(_action))
                                _action = jc.Context.Request.Form[act];
                        }
                        else
                            _action = Url.Action;
                    }

                    if (StringUtil.IsNullOrEmpty(_action))
                        _action = "index";

                    int dotindex = _action.IndexOf(".");
                    if (dotindex != -1)
                        _action = _action.Remove(dotindex);
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

        public void Set(UrlMapping.UrlMappingItem item)
        {
            OK = true;
            Url = item;

            Index = item.Index;
            SubIndex = item.SubIndex;
            Title = item.Title;
            Desc = item.Desc;
            Name = item.Name;
        }
    }
}
