using Kiss.Utils;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Kiss.Web.Controls
{
    public class ControlPanel : Control
    {
        public ControlPanel()
        {
            JContext jc = JContext.Current;
            if (jc.User == null) return;

            isSiteAdmin = jc.User.HasPermission("site.control_panel")
                && (!jc.Area["support_mulit_site"].ToBoolean() || jc.User.IsInSite(jc.SiteId));
        }

        private bool isSiteAdmin = false;

        private List<IControlPanelItemRenderer> renderers = new List<IControlPanelItemRenderer>();

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (!isSiteAdmin) return;

            foreach (var cpItem in Plugin.Plugins.GetPlugins<ControlPanelItemAttribute>(JContext.Current.User))
            {
                Object obj = Activator.CreateInstance(cpItem.Decorates);
                if (obj is Control)
                    Controls.Add(obj as Control);

                renderers.Add(obj as IControlPanelItemRenderer);
            }

            if (renderers.Count == 0) return;

            Include include = new Include();
            include.Js = "cookie";
            Controls.Add(include);

            ClientScriptProxy.Current.RegisterCssResource(GetType(),
                                "Kiss.Web.jQuery.cp.c.css");
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!isSiteAdmin || renderers.Count == 0) { base.Render(writer); return; }

            writer.Write("<div id='_g_sc' class='sc opened' path='{0}'><div class='scContent'><div class='controlPanel'><div class='plugins'>", JContext.Current.Area.VirtualPath);

            foreach (var renderer in renderers)
            {
                renderer.RenderItem(writer);
            }

            writer.Write("</div></div><span class='close' title='关闭控制面板'>«</span><span class='open' title='打开控制面板'>»</span></div></div>");

            ClientScriptProxy.Current.RegisterJsResource(writer, GetType(), "Kiss.Web.jQuery.cp.j.js");

            base.Render(writer);
        }
    }
}
