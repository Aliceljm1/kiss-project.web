using System;
using System.Web.SessionState;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this page to render master file directly
    /// </summary>
    public class MasterPage : Page, IRequiresSessionState
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
