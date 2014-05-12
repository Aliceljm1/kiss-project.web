using Kiss.Plugin;
using Kiss.Utils;

namespace Kiss.Web.Optimization
{
    [AutoInit]
    class OptimizationInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (!setting.Enable) return;

            OptimizationModule cm = new OptimizationModule();
            cm.Start();
            sl.AddComponentInstance(cm);

            if (setting["css_sprite"].ToBoolean())
                ImageOptimizations.AddCacheDependencies(ServerUtil.MapPath("~/"), true);
        }
    }
}
