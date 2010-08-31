using System;
using Kiss.Plugin;

namespace Kiss.Web.Mvc
{
    [AutoInit(Title = "mvc", Priority = 0)]
    public class MvcInitializer : IPluginInitializer
    {
        //const string KEY = "Kiss.mvc";

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
