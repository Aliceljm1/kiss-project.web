using Kiss.Plugin;
using Kiss.Security;
using Kiss.Utils;
using System;
using System.Web.UI;

namespace Kiss.Web
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ControlPanelItemAttribute : PluginAttribute
    {
        public override bool IsAuthorized(Principal user)
        {
            JContext jc = JContext.Current;
            IArea area = jc.Area;

            string per = Permission;

            if (area["support_multi_site"].ToBoolean())
            {
                per = string.Format("{0}@{1}", per, jc.SiteId);
            }

            if (user != null && StringUtil.HasText(per))
                return user.HasPermission(per);

            return true;
        }

        public string Permission { get; set; }
    }

    public interface IControlPanelItemRenderer
    {
        void RenderItem(HtmlTextWriter writer);
    }
}
