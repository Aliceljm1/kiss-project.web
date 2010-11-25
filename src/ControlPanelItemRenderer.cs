using System;
using System.Web.UI;
using Kiss.Plugin;
using Kiss.Security;
using Kiss.Utils;

namespace Kiss.Web
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ControlPanelItemAttribute : PluginAttribute
    {
        public override bool IsAuthorized(Principal user)
        {
            if (StringUtil.HasText(Permission))
                return user.HasPermission(Permission);

            return true;
        }

        public string Permission { get; set; }
    }

    public interface IControlPanelItemRenderer
    {
        void RenderItem(HtmlTextWriter writer);
    }
}
