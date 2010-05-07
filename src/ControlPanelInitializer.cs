using Kiss.Plugin;
using Kiss.Security;

namespace Kiss.Web
{
    [AutoInit(Title = "显示控制面板")]
    public class ControlPanelInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, PluginSetting setting)
        {
            IUserService userservice = null;
            try
            {
                userservice = sl.Resolve<IUserService>();
            }
            catch
            {
            }

            if (userservice == null)
                return;

            if (setting.Enable)// add site.control_panel permission
                userservice.AddPermissionModule("site", "站点管理", "control_panel|显示控制面板");
            else
                userservice.RemovePermissionModule("site", "control_panel");
        }
    }
}
