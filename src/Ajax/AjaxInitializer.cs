using Kiss.Plugin;
using Kiss.Web.UrlMapping;

namespace Kiss.Web.Ajax
{
    [AutoInit(Title = "Ajax")]
    public class AjaxInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (!setting.Enable) return;

            IUrlMappingProvider urlMapping = sl.Resolve<IUrlMappingProvider>();

            UrlMappingItem item = UrlMapping.Utility.CreateTemplatedMappingItem("ajax.aspx");
            item.Id = "_ajax_";
            item.Action = "proc";
            item.Index = -1;
            item.SubIndex = -1;

            urlMapping.AddMapping(item);
        }
    }
}
