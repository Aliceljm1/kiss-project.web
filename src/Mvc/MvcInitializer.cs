using System;
using Kiss.Plugin;

namespace Kiss.Web.Mvc
{
    [AutoInit(Title = "mvc", Priority = 0)]
    public class MvcInitializer : IPluginInitializer
    {
        const string KEY = "Kiss.mvc";

        #region IPluginInitializer Members

        public void Init(ServiceLocator sl, PluginSetting setting)
        {
            if (!setting.Enable) return;

            string type = setting["type"] ?? string.Empty;

            switch (type.ToLower())
            {
                case "":
                    sl.AddComponent(KEY, typeof(MvcModule));
                    break;
                default:
                    sl.AddComponent(KEY, Type.GetType(type, true, true));
                    break;
            }

            object obj = sl.Resolve(KEY);
            if (obj is IStartable)
                (obj as IStartable).Start();
        }

        #endregion
    }
}
