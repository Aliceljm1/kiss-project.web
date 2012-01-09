using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Kiss.Config;
using Kiss.Utils;
using Kiss.Web.Utils;

namespace Kiss.Web
{
    [ConfigNode("site", Desc = "站点")]
    public class SiteConfig : ConfigBase, ISite
    {
        public static SiteConfig Instance { get { return GetConfig<SiteConfig>(); } }

        public static SiteConfig GetConfig(XmlNode node)
        {
            return GetConfig<SiteConfig>(node, false);
        }

        #region props

        [ConfigProp("title", ConfigPropAttribute.DataType.String, Desc = "页面标题")]
        public string Title { get; private set; }

        [ConfigProp("defaultTheme", ConfigPropAttribute.DataType.String, DefaultValue = "default", Desc = "默认theme")]
        public string DefaultTheme { get; private set; }

        private string _vp;
        [ConfigProp("virtualPath", Desc = "虚拟目录", DefaultValue = "")]
        public string VP { get { return _vp; } set { _vp = value; _virtualPath = null; } }

        [ConfigProp("host", ConfigPropAttribute.DataType.String, Desc = "域名")]
        public string Host { get; private set; }

        [ConfigProp("cssRoot", ConfigPropAttribute.DataType.String, DefaultValue = "themes/{0}/", Desc = "样式文件夹路径")]
        public string CssRoot { get; private set; }

        [ConfigProp("themeRoot", ConfigPropAttribute.DataType.String, DefaultValue = "themes", Desc = "皮肤根目录")]
        public string ThemeRoot { get; private set; }

        [ConfigProp("combinCss", ConfigPropAttribute.DataType.Boolean, DefaultValue = false, Desc = "合并样式")]
        public bool CombineCss { get; private set; }

        [ConfigProp("cssversion", ConfigPropAttribute.DataType.String, DefaultValue = "1", Desc = "样式版本")]
        public string CssVersion { get; private set; }

        [ConfigProp("cssHost", ConfigPropAttribute.DataType.String, Desc = "样式根url")]
        public string CssHost { get; private set; }

        [ConfigProp("jsversion", ConfigPropAttribute.DataType.String, DefaultValue = "1", Desc = "js版本")]
        public string JsVersion { get; private set; }

        [ConfigProp("combinJs", ConfigPropAttribute.DataType.Boolean, DefaultValue = false, Desc = "合并js")]
        public bool CombineJs { get; private set; }

        [ConfigProp("jsHost", ConfigPropAttribute.DataType.String, Desc = "js根url")]
        public string JsHost { get; private set; }

        [ConfigProp("favIcon", ConfigPropAttribute.DataType.String, Desc = "fav icon")]
        public string FavIcon { get; private set; }

        [ConfigProp("generator", ConfigPropAttribute.DataType.String, DefaultValue = "TXTEK.com", Desc = "generator")]
        public string Generator { get; private set; }

        [ConfigProp("rawAdditionalHeader", ConfigPropAttribute.DataType.String, Desc = "任意的http头")]
        public string RawAdditionalHeader { get; private set; }

        [ConfigProp("searchMetaKeywords", ConfigPropAttribute.DataType.String, Desc = "meta keyword")]
        public string SearchMetaKeywords { get; private set; }

        [ConfigProp("searchMetaDescription", ConfigPropAttribute.DataType.String, Desc = "meta description")]
        public string SearchMetaDescription { get; private set; }

        [ConfigProp("siteKey", ConfigPropAttribute.DataType.String, DefaultValue = "default")]
        public string SiteKey { get; set; }

        public string ErrorPage { get; private set; }

        /// <summary>
        /// jquery ui theme
        /// </summary>
        [ConfigProp("jqueryui", DefaultValue = "smoothness", Options = "blacktie,blitzer,cupertino,darkhive,darkness,dotluv,eggplant,excitebike,flick,hotsneaks,humanity,lefrog,lightness,mintchoc,overcast,peppergrinder,redmond,smoothness,southstreet,start,sunny,swankypurse,trontastic,vader", Desc = "jquery ui theme")]
        public string jQueryUI { get; private set; }

        internal static readonly Dictionary<int, NavigationItem> menu = new Dictionary<int, NavigationItem>();
        public Dictionary<int, NavigationItem> MenuItems { get { return menu; } }

        #endregion

        protected override void LoadValuesFromConfigurationXml(XmlNode node)
        {
            base.LoadValuesFromConfigurationXml(node);

            ErrorPage = string.Empty;
            XmlNode error_node = node.SelectSingleNode("errorPage");
            if (error_node != null)
                ErrorPage = error_node.Value;
        }

        public string Authority
        {
            get { return string.Empty; }
        }

        public List<DictSchema> GetSchema(string type)
        {
            throw new NotSupportedException("single site doesn't support dict schema!");
        }

        public DictSchema GetSchema(string type, string name)
        {
            throw new NotSupportedException("single site doesn't support dict schema!");
        }

        private string _virtualPath;

        /// <summary>
        /// 虚拟目录
        /// </summary>
        /// <remarks>只能在http上下文中调用</remarks>
        public string VirtualPath
        {
            get
            {
                if (_virtualPath == null)
                {
                    _virtualPath = StringUtil.CombinUrl(HttpRuntime.AppDomainAppVirtualPath, VP);

                    if (_virtualPath != "/" && !_virtualPath.EndsWith("/"))
                        _virtualPath += "/";
                }

                return _virtualPath;
            }
        }

        public string Theme
        {
            get
            {
                string t = JContext.Current.Items["SiteConfig.Theme"] as string;

                if (string.IsNullOrEmpty(t))
                    t = DefaultTheme;

                return t;
            }
            set
            {
                JContext.Current.Items["SiteConfig.Theme"] = value;
            }
        }
    }
}
