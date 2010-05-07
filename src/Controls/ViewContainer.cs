#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-24
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-24		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

namespace Kiss.Web.Controls
{
    public class ViewContainer : TemplatedControl
    {
        #region fields / properties

        private string _view;

        public string View
        {
            get { return _view; }
            set { _view = value; }
        }

        #endregion

        #region override

        protected override string SkinFolder
        {
            get
            {
                return string.Format ( "~/Themes/{0}/Views/", ThemeName );
            }
        }

        protected override string SkinFileName
        {
            get
            {
                return string.Format ( "{0}.ascx", View );
            }
        }

        #endregion
    }
}
