using Kiss.Plugin;

namespace Kiss.Web.UrlMapping
{
    [AutoInit(Title = "url routing", Priority = 4)]
    public class UrlMappingInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, PluginSetting setting)
        {
            if (!setting.Enable) return;

            sl.AddComponent("kiss.XmlUrlMappingProvider", typeof(IUrlMappingProvider), typeof(XmlUrlMappingProvider));
            sl.AddComponent("kiss.defaultHost", typeof(IHost), typeof(Host));

            UrlMappingModule module = new UrlMappingModule();
            module.Start();
            sl.AddComponentInstance(module);
        }
    }
}
