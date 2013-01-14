using Kiss.Plugin;
using Kiss.Utils;
using Kiss.Web.UrlMapping;
using System.Collections.Generic;
using System.Linq;

namespace Kiss.Web.Dynamic
{
    [AutoInit]
    public class DynamicPageInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (!setting.Enable) return;

            IUrlMappingProvider provider = sl.Resolve<IUrlMappingProvider>();

            List<DictSchema> routes = (from q in DictSchema.CreateContext(true)
                                       where q.Type == "routes"
                                       orderby q.SortOrder ascending
                                       select q).ToList();

            foreach (var item in routes)
            {
                UrlMappingItem urlmapping = UrlMapping.Utility.CreateTemplatedMappingItem(item.Name);
                urlmapping.Title = item.Title;
                urlmapping.Redirection = UrlMapping.Utility.GetHref(item.Description);
                urlmapping.Id = item.Prop1;
                urlmapping.Action = item.Prop2;
                urlmapping.Index = item.Prop3.ToInt(-1);
                urlmapping.SubIndex = item.Prop4.ToInt(-1);
                urlmapping.SubsubIndex = item.Prop5.ToInt(-1);

                if (string.IsNullOrEmpty(item.Category))
                    provider.AddMapping(urlmapping);
                else
                    provider.AddMapping(item.Category, urlmapping);
            }
        }
    }
}
