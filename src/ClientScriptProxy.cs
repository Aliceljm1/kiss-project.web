
using System;
using System.Web;
using System.Web.UI;
using Kiss.Utils;
using Kiss.Web.Controls;

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

        public void RegisterJsResource(HtmlTextWriter writer, Type type, string resourceName, bool noCombin)
        {
            RegisterJsResource(writer, type.Assembly.GetName().Name, resourceName, noCombin);
        }

        public void RegisterJsResource(HtmlTextWriter writer, string assemblyName, string resourceName, bool noCombin)
        {
            if (IsScriptRended(resourceName))
                return;

            SetScriptRended(resourceName);

            RegisterClientScript(writer, ResourceHandler.GetResourceUrl(assemblyName, resourceName), noCombin);
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
                if (!noCombin && JContext.Current.Site.CombinJs)
                {
                    Scripts.AddBlock(script);
                    return;
                }
                else
                    script = string.Format("<script type='text/javascript'>{0}</script>", script);
            }

            writer.Write(script);
        }

        public void RegisterCss(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (IsScriptRended(url))
                return;

            SetScriptRended(url);

            Head.AddStyle(url);
        }

        public void RegisterCssResource(Type type, string resourceName, string baseUrl)
        {
            RegisterCssResource(type.Assembly.GetName().Name, resourceName, baseUrl);
        }

        public void RegisterCssResource(string assemblyName, string resourceName, string baseUrl)
        {
            if (IsScriptRended(resourceName))
                return;

            SetScriptRended(resourceName);

            if (string.IsNullOrEmpty(baseUrl))
                Head.AddStyle(ResourceHandler.GetResourceUrl(assemblyName, resourceName));
            else
                Head.AddStyle(StringUtil.CombinUrl(baseUrl, ResourceHandler.GetResourceUrl(assemblyName, resourceName)));
        }

        public void RegisterCssResource(string assemblyName, string resourceName)
        {
            RegisterCssResource(assemblyName, resourceName, null);
        }

        public void RegisterCssResource(Type type, string resourceName)
        {
            RegisterCssResource(type, resourceName, null);
        }

        /// <summary>
        /// Returns a WebResource URL for non script resources
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public string GetWebResourceUrl(Control control, Type type, string resourceName)
        {
            return ResourceHandler.GetResourceUrl(type, resourceName);
        }

        public void RegisterCssBlock(string css, string key)
        {
            if (IsScriptRended(key))
                return;

            SetScriptRended(key);

            Head.AddRawContent(string.Format("<style type='text/css'>{0}</style>", css), HttpContext.Current);
        }

        public void RegisterClientScript(HtmlTextWriter writer, string url, bool noCombin)
        {
            if (!noCombin && JContext.Current.Site.CombinJs)
                Scripts.AddRes(url);
            else
                writer.Write("<script src='{0}' type='text/javascript'></script>", url);
        }

        public void RegisterClientScript(HtmlTextWriter writer, string url)
        {
            RegisterClientScript(writer, url, false);
        }

        #region help

        private bool IsScriptRended(string key)
        {
            bool? b = HttpContext.Current.Items["client_" + key] as bool?;

            return b != null && b.Value;
        }

        private void SetScriptRended(string key)
        {
            HttpContext.Current.Items["client_" + key] = true;
        }

        #endregion
    }
}