using System.Collections.Generic;
using System.Reflection;

namespace Kiss.Web
{
    /// <summary>
    /// Provides information about types in the current web application. 
    /// Optionally this class can look at all assemblies in the bin folder.
    /// </summary>
    public class WebAppTypeFinder : AppDomainTypeFinder
    {
        private IWebContext webContext;
        private bool binFolderAssembliesLoaded = false;

        public WebAppTypeFinder(IWebContext webContext)
        {
            this.webContext = webContext;
        }

        #region Methods

        public override IList<Assembly> GetAssemblies()
        {
            List<string> addedAssemblyNames = new List<string>();
            List<Assembly> assemblies = new List<Assembly>();

            AddAssembliesInAppDomain(addedAssemblyNames, assemblies);

            if (!binFolderAssembliesLoaded)
            {
                binFolderAssembliesLoaded = true;
                LoadMatchingAssemblies(addedAssemblyNames, webContext.MapPath("~/bin"));
            }

            return assemblies;
        }

        #endregion
    }
}
