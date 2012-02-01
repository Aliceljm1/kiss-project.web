using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this page to render master file directly
    /// </summary>
    public class MasterPage : Page
    {
        /// <summary>
        /// 是否启用模板引擎渲染
        /// </summary>
        public bool Templated { get; set; }

        private bool hasMasterFile = false;

        protected override void OnPreInit(EventArgs e)
        {
            string masterFile = Context.Items[UrlMapping.UrlMappingModule.kCONTEXTITEMS_MASTERPAGEKEY] as string;

            if (StringUtil.HasText(masterFile))
            {
                hasMasterFile = true;

                if (JContext.Current.RenderContent)
                {
                    Container container = new Container();
                    container.ThemeMasterFile = masterFile + ".ascx";

                    Controls.Add(container);
                }
            }

            base.OnPreInit(e);
        }

        JContext jc;
        Action<JContext, Mvc.MvcModule> action = delegate(JContext jc, Mvc.MvcModule module) { module.invoker.InvokeAction(jc); };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            jc = JContext.Current;

            if (jc.IsAsync)
                RegisterAsyncTask(new PageAsyncTask(BeginAsyncOperation, EndAsyncOperation, TimeoutAsyncOperation, null));
        }

        IAsyncResult BeginAsyncOperation(object sender, EventArgs e, AsyncCallback cb, object state)
        {
            return action.BeginInvoke(jc,
                ServiceLocator.Instance.Resolve<Mvc.MvcModule>(),
                cb,
                state);
        }

        void EndAsyncOperation(IAsyncResult ar)
        {
            action.EndInvoke(ar);
        }

        void TimeoutAsyncOperation(IAsyncResult ar) { }

        protected override void Render(HtmlTextWriter writer)
        {
            ContentType = Context.Items["_ContentType_"] as string ?? ContentType;

            if (Templated && hasMasterFile && JContext.Current.RenderContent)
                writer.Write(Util.Render(delegate(HtmlTextWriter w) { base.Render(w); }));
            else
                base.Render(writer);
        }
    }
}
