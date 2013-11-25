using Kiss.Plugin;

namespace Kiss.Web.Query
{
    [AutoInit]
    public class DynamicQueryInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (!setting.Enable) return;

            DynamicQueryPlugin plugin = new DynamicQueryPlugin();
            plugin.Start();

            sl.AddComponentInstance<DynamicQueryPlugin>(plugin);
        }
    }
}
