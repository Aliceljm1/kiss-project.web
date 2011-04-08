using Kiss.Plugin;

namespace Kiss.Web.UrlMapping
{
    [AutoInit(Title = "Url Route", Priority = 4)]
    public class UrlMappingInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, PluginSetting setting)
        {
            if (!setting.Enable) return;            

            UrlMappingModule module = new UrlMappingModule();
            module.Start();
            sl.AddComponentInstance(module);
        }
    }
}
