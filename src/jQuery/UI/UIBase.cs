using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// jquery ui base class
    /// </summary>
    public class UIBase : Control
    {
        private ClientScriptProxy _proxy = ClientScriptProxy.Current;
        /// <summary>
        /// client script proxy
        /// </summary>
        public ClientScriptProxy Proxy
        {
            get { return _proxy; }
        }

        public string Selector { get; set; }

        protected List<string> JsIncludes { get; private set; }
        protected StringBuilder Js { get; private set; }

        public string ThemeStyleUrl { get { return Context.Items["_jqueryUI_ThemeStyleUrl"] as string; } set { Context.Items["_jqueryUI_ThemeStyleUrl"] = value; } }

        #region ctor

        public UIBase()
        {
            JsIncludes = new List<string>();
            Js = new StringBuilder();
        }

        #endregion

        #region override

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // register style
            if (string.IsNullOrEmpty(ThemeStyleUrl))
                Proxy.RegisterCssResource("Kiss.Web", "Kiss.Web.jQuery.UI.themes.smoothness.style.css");
            else
                Proxy.RegisterCss(JContext.Current.url(ThemeStyleUrl));
        }

        protected override void Render(HtmlTextWriter writer)
        {
            //base.Render(writer);

            // 在ajax环境下，不引用js libs
            if (!JContext.Current.IsAjaxRequest)
            {
                AppendJsIncludes();
                RegisterJsInclucde(writer);
            }

            if (StringUtil.IsNullOrEmpty(Selector))
                return;

            AppendJsBlock();

            // render js block
            if (Js.Length > 0)
            {
                ClientScriptProxy.Current.RegisterJsBlock(writer,
                    Guid.NewGuid().ToString(),
                    Js.ToString(),
                    true,
                    JContext.Current.IsAjaxRequest);
            }
        }

        #endregion

        protected virtual void AppendJsIncludes() { }
        protected virtual void AppendJsBlock() { }

        private void RegisterJsInclucde(HtmlTextWriter writer)
        {
            Proxy.LoadjQuery(writer);

            // register js resource
            foreach (string script in JsIncludes)
            {
                string js = script;
                if (!js.EndsWith(".js"))
                    js = string.Format("Kiss.Web.jQuery.UI.{0}.js", js);

                Proxy.RegisterJsResource(writer,
                    typeof(UIBase),
                    js,
                    JContext.Current.IsAjaxRequest);
            }
        }
    }
}
