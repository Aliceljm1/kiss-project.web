using System;
using System.Web.UI;
using Kiss.Plugin;
using Kiss.Security;

namespace Kiss.Web
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ControlPanelItemAttribute : PluginAttribute
    {
        public override bool IsAuthorized(Principal user)
        {
            return user.HasPermission(Permission);
        }

        public string Permission { get; set; }
    }

    public interface IControlPanelItemRenderer
    {
        void RenderItem(HtmlTextWriter writer);
    }
}
