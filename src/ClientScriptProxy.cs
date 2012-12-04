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

        #region JS
        /// <summary>
        /// Loads the appropriate jScript library out of the scripts directory
        /// </summary>
        /// <param name="control"></param>
        public void LoadjQuery(HtmlTextWriter writer)
        {
            if (JContext.Current.IsAjaxRequest)
                return;

            string url = Resources.Utility.GetResourceUrl(GetType(), "Kiss.Web.jQuery.js", true);

            if (IsScriptRended(url))
                return;

            SetScriptRended(url);

            writer.WriteLine("<script src='{0}' type='text/javascript'></script>", url);
        }

        public void RegisterJsResource(HtmlTextWriter writer, string resourceName)
        {
            RegisterJsResource(writer, GetType(), resourceName, false);
        }

        /// <summary>
        /// Returns a WebResource or ScriptResource URL for script resources that are to be
        /// embedded as script includes.
        /// </summary>
        public void RegisterJsResource(HtmlTextWriter writer, Type type, string resourceName)
        {
            RegisterJsResource(writer, type, resourceName, false);
        }

        public void RegisterJsResource(HtmlTextWriter writer, string assemblyName, string resourceName)
        {
            RegisterJsResource(writer, assemblyName, resourceName, false);
        }

        public void RegisterJsResource(HtmlTextWriter writer, Type type, string resourceName, bool noCombine)
        {
            RegisterJsResource(writer, type.Assembly.GetName().Name, resourceName, noCombine);
        }

        public void RegisterJsResource(HtmlTextWriter writer, string assemblyName, string resourceName, bool noCombin)
        {
            RegisterJs(writer, Resources.Utility.GetResourceUrl(assemblyName, resourceName), noCombin);
        }

        public void RegisterJs(HtmlTextWriter writer, string url)
        {
            RegisterJs(writer, url, false);
        }

        public void RegisterJs(HtmlTextWriter writer, string url, bool noCombine)
        {
            if (IsScriptRended(url))
                return;

            SetScriptRended(url);

            Scripts.AddRes(url, !noCombine && JContext.Current.Site.CombineJs);
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
                if (!noCombin && JContext.Current.Site.CombineJs)
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

            ISite site = JContext.Current.Site;

            if (!site.CombineCss)
            {
                if (url.Contains("?"))
                    url += ("&v=" + site.CssVersion);
                else
                    url += ("?v=" + site.CssVersion);
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
                Head.AddStyle(url);
            else
                Head.AddStyle(StringUtil.CombinUrl(baseUrl, url));
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