using System;
using Kiss.Plugin;
using Kiss.Utils;

namespace Kiss.Web
{
    [AutoInit(Title = "web farm support", Priority = 8)]
    public class HostInitializer : IPluginInitializer
    {
        #region IPluginInitializer Members

        public void Init(ServiceLocator sl, PluginSetting setting)
        {
            if (!setting.Enable) return;

            string type = setting["type"];

            if (StringUtil.HasText(type))
                sl.AddComponent("kiss.host", typeof(IHost), Type.GetType(type, true, true));
        }

        #endregion
    }
}
