#region File Comment
//+-------------------------------------------------------------------+
//+ FileName: 	    ContentPlaceHolder.cs
//+ File Created:   20090820
//+-------------------------------------------------------------------+
//+ Purpose:        The control marks a place holder for content in a master page.
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 20090820        ZHLI Comment Created
//+-------------------------------------------------------------------+
//+-------------------------------------------------------------------+
#endregion

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// The control marks a place holder for content in a master page.
    /// </summary>
    public class ContentPlaceHolder : PlaceHolder, INamingContainer
    {
        private const string contextKey = "__Kiss.contentplaceholders__";

        #region override

        /// <summary>
        /// Overrides <see cref="Control.ID"/> to register regions as they are created.
        /// </summary>
        public override string ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = value;
                Register ( );
            }
        }

        #endregion

        #region helper

        private void Register ( )
        {
            if ( HttpContext.Current != null )
            {
                HttpContext.Current.Items[ GetKey ( ID ) ] = this;
            }
        }

        private static string GetKey ( string id )
        {
            return contextKey + id;
        }

        internal static ContentPlaceHolder Find ( string id )
        {
            if ( HttpContext.Current == null ) return null;
            return HttpContext.Current.Items[ GetKey ( id ) ] as ContentPlaceHolder;
        }

        #endregion
    }
}
