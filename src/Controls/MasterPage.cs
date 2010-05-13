using System;
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

        protected override void OnPreInit(EventArgs e)
        {
            string masterFile = Context.Request.QueryString["MasterFile"];
            Container container = new Container();
            if (StringUtil.HasText(masterFile))
                container.ThemeMasterFile = masterFile + ".ascx";

            Controls.Add(container);

            base.OnPreInit(e);
        }

        JContext jc;
        Mvc.MvcModule module;
        Action<JContext, Mvc.MvcModule> action = delegate(JContext jc, Mvc.MvcModule module) { module.invoker.InvokeAction(jc); };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            jc = JContext.Current;

            module = ServiceLocator.Instance.Resolve<Mvc.MvcModule>();

            if (jc.IsAsync)
                RegisterAsyncTask(new PageAsyncTask(BeginAsyncOperation, EndAsyncOperation, TimeoutAsyncOperation, null));
        }

        IAsyncResult BeginAsyncOperation(object sender, EventArgs e, AsyncCallback cb, object state)
        {
            return action.BeginInvoke(jc, module, cb, state);
        }

        void EndAsyncOperation(IAsyncResult ar)
        {
            action.EndInvoke(ar);
        }

        void TimeoutAsyncOperation(IAsyncResult ar) { }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Templated)
                writer.Write(Util.Render(delegate(HtmlTextWriter w) { base.Render(w); }));
            else
                base.Render(writer);
        }
    }
}
