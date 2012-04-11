using Kiss.Plugin;
using Kiss.Web.UrlMapping;

namespace Kiss.Web.Resources
{
    [AutoInit(Title = "Resource")]
    public class ResourceInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (!setting.Enable) return;

            IUrlMappingProvider urlMapping = sl.Resolve<IUrlMappingProvider>();

            UrlMappingItem item = UrlMapping.Utility.CreateTemplatedMappingItem("_res.aspx");
            item.Id = "_res_";
            item.Action = "proc";
            item.Index = -1;
            item.SubIndex = -1;
            item.SubsubIndex = -1;

            urlMapping.AddMapping(item);

            item = UrlMapping.Utility.CreateTemplatedMappingItem("[]_resc.aspx");
            item.Id = "_resc_";
            item.Action = "proc";
            item.Index = -1;
            item.SubIndex = -1;
            item.SubsubIndex = -1;

            urlMapping.AddMapping(item);
        }
    }
}
