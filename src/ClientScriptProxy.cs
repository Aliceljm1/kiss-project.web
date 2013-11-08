using Kiss.Utils;
using Kiss.Web.Controls;
using System;
using System.Web;
using System.Web.UI;

namespace Kiss.Web
{
    /// <summary>
    /// use this class to add js,css to current page
    /// </summary>
    public class ClientScriptProxy
    {
        /// <summary>
        /// Current instance of this class which should always be used to 
        /// access this object. There are no public constructors to
        /// ensure the reference is used as a Singleton to further
        /// ensure that all scripts are written to the same clientscript
        /// manager.
        /// </summary>
        public static ClientScriptProxy Current
        {
            get
            {
                if (Singleton<ClientScriptProxy>.Instance == null)
                {
                    Singleton<ClientScriptProxy>.Instance = new ClientScriptProxy();
                }

                return Singleton<ClientScriptProxy>.Instance;
            }
        }

        /// <summary>
        /// No public constructor - use ClientScriptProxy.Current to
        /// get an instance to ensure you once have one instance per
        /// page active.
        /// </summary>
        protected ClientScriptProxy()
        {
        }

        /// <summary>
        /// 依赖的资源名称，多个资源名称使用逗号分隔
        /// </summary>
        /// <param name="resourceName"></param>
        public void Require(IArea area, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return;

            JContext jc = JContext.Current;

            foreach (var res in StringUtil.Split(resourceName, StringUtil.Comma, true, true))
            {
                if (res == "jquery.js")
                {
                    Head.RequireJquery = true;
                    continue;
                }

                string resourceUrl = string.Empty;

                string key = string.Format("define_{0}", res);

                if (area != null && !string.IsNullOrEmpty(area[key]))
                {
                    resourceUrl = jc.CombinUrl(area, area[key], false);
                }
                else
                {
                    foreach (IArea item in jc.Host.AllAreas)
                    {
                        if (area != null && item.AreaKey == area.AreaKey) continue;

                        if (!string.IsNullOrEmpty(item[key]))
                        {
                            resourceUrl = jc.CombinUrl(item, item[key], false);
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(resourceUrl))
                    {
                        if (res == "kiss.js" || res == "kiss.css" || res == "jqueryui.js" || res == "jqueryui.css")
                        {
                            resourceUrl = Resources.Utility.GetResourceUrl(GetType(), "Kiss.Web.jQuery." + res, true);
                        }
                        else
                        {
                            throw new WebException("未找到{0}的定义。", res);
                        }
                    }
                }

                if (res.EndsWith(".js"))
                    RegisterJs(resourceUrl);
                else if (res.EndsWith(".css"))
                    RegisterCss(resourceUrl);
            }
        }

        #region JS
        public void RegisterJsResource(string resourceName)
        {
            RegisterJsResource(GetType(), resourceName, false);
        }

        /// <summary>
        /// Returns a WebResource or ScriptResource URL for script resources that are to be
        /// embedded as script includes.
        /// </summary>
        public void RegisterJsResource(Type type, string resourceName)
        {
            RegisterJsResource(type, resourceName, false);
        }

        public void RegisterJsResource(string assemblyName, string resourceName)
        {
            RegisterJsResource(assemblyName, resourceName, false);
        }

        public void RegisterJsResource(Type type, string resourceName, bool noCombine)
        {
            RegisterJsResource(type.Assembly.GetName().Name, resourceName, noCombine);
        }

        public void RegisterJsResource(string assemblyName, string resourceName, bool noCombin)
        {
            RegisterJs(Resources.Utility.GetResourceUrl(assemblyName, resourceName), noCombin);
        }

        public void RegisterJs(string url)
        {
            RegisterJs(url, false);
        }

        public void RegisterJs(string url, bool noCombine)
        {
            if (IsScriptRended(url))
                return;

            SetScriptRended(url);

            Scripts.AddRes(url, !noCombine && JContext.Current.Area.CombineJs);
        }

        public void RegisterJsBlock(HtmlTextWriter writer, string key, string script, bool addScriptTags)
        {
            RegisterJsBlock(writer, key, script, addScriptTags, false);
        }

        /// <summary>
        /// Registers a client script block in the page.
        /// </summary>
        public void RegisterJsBlock(HtmlTextWriter writer, string key, string script, bool addScriptTags, bool noCombin)
        {
            if (IsScriptRended(key))
                return;

            SetScriptRended(key);

            if (addScriptTags)
            {
                if (!noCombin && JContext.Current.Area.CombineJs)
                {
                    Scripts.AddBlock(script);
                    return;
                }
                else
                    script = string.Format("<script type='text/javascript'>{0}</script>", script);
            }

            writer.Write(script);
        }
        #endregion

        #region CSS
        public void RegisterCss(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (IsScriptRended(url))
                return;

            SetScriptRended(url);

            IArea site = JContext.Current.Area;

            if (!site.CombineCss)
            {
                if (url.Contains("?"))
                    url += ("&v=" + AreaConfig.Instance.CssVersion);
                else
                    url += ("?v=" + AreaConfig.Instance.CssVersion);
            }

            Head.AddStyle(url);
        }

        public void RegisterCssResource(string resourceName)
        {
            RegisterCssResource(GetType(), resourceName, null);
        }

        public void RegisterCssResource(Type type, string resourceName)
        {
            RegisterCssResource(type, resourceName, null);
        }

        public void RegisterCssResource(Type type, string resourceName, string baseUrl)
        {
            RegisterCssResource(type.Assembly.GetName().Name, resourceName, baseUrl);
        }

        public void RegisterCssResource(string assemblyName, string resourceName)
        {
            RegisterCssResource(assemblyName, resourceName, null);
        }

        public void RegisterCssResource(string assemblyName, string resourceName, string baseUrl)
        {
            string url = Resources.Utility.GetResourceUrl(assemblyName, resourceName);

            if (IsScriptRended(url))
                return;

            SetScriptRended(url);

            if (string.IsNullOrEmpty(baseUrl))
                Head.AddStyle(JContext.Current.Area, url, "all", HttpContext.Current, StyleRelativePosition.First, true);
            else
                Head.AddStyle(JContext.Current.Area, StringUtil.CombinUrl(baseUrl, url), "all", HttpContext.Current, StyleRelativePosition.First, true);
        }

        public void RegisterCssBlock(string css, string key)
        {
            if (IsScriptRended(key))
                return;

            SetScriptRended(key);

            Head.AddRawContent(string.Format("<style type='text/css'>{0}</style>", css), HttpContext.Current);
        }
        #endregion

        /// <summary>
        /// Returns a WebResource URL for non script resources
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public string GetWebResourceUrl(Control control, Type type, string resourceName)
        {
            return Resources.Utility.GetResourceUrl(type, resourceName);
        }

        #region help

        public bool IsScriptRended(string key)
        {
            bool? b = HttpContext.Current.Items["client_" + key] as bool?;

            return b != null && b.Value;
        }

        public void SetScriptRended(string key)
        {
            HttpContext.Current.Items["client_" + key] = true;
        }

        #endregion
    }
}