#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-24
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-24		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

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
        private bool ensureBinFolderAssembliesLoaded = true;
        private bool binFolderAssembliesLoaded = false;

        public WebAppTypeFinder ( IWebContext webContext )
        {
            this.webContext = webContext;
        }

        #region props

        /// <summary>
        /// Gets or sets wether assemblies in the bin folder of the web application 
        /// should be specificly checked for beeing loaded on application load. 
        /// This is need in situations where plugins need to be loaded in the AppDomain after the application been reloaded.
        /// </summary>
        public bool EnsureBinFolderAssembliesLoaded
        {
            get { return ensureBinFolderAssembliesLoaded; }
            set { ensureBinFolderAssembliesLoaded = value; }
        }

        #endregion

        #region Methods

        public override IList<Assembly> GetAssemblies ( )
        {
            if ( EnsureBinFolderAssembliesLoaded && !binFolderAssembliesLoaded )
            {
                binFolderAssembliesLoaded = true;
                LoadMatchingAssemblies ( webContext.MapPath ( "~/bin" ) );
            }

            return base.GetAssemblies ( );
        }

        #endregion
    }
}
