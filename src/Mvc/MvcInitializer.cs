using Kiss.Plugin;

namespace Kiss.Web.Mvc
{
    [AutoInit(Title = "mvc")]
    public class MvcInitializer : IPluginInitializer
    {
        #region IPluginInitializer Members

        public void Init(ServiceLocator sl, PluginSetting setting)
        {
            if (!setting.Enable) return;

            MvcModule mvcModule = new MvcModule();
            mvcModule.Start();
            sl.AddComponentInstance(mvcModule);
        }

        #endregion
    }
}
