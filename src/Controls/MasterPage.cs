#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-11-06
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-11-06		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this page to render master file directly
    /// </summary>
    public class MasterPage : Page
    {
        /// <summary>
        /// 是否启用模板引擎渲染
        /// </summary>
        public bool Templated { get; set; }

        protected override void OnPreInit( EventArgs e )
        {
            string masterFile = Context.Request.QueryString[ "MasterFile" ];
            Container container = new Container();
            if( StringUtil.HasText( masterFile ) )
                container.ThemeMasterFile = masterFile + ".ascx";

            Controls.Add( container );

            base.OnPreInit( e );
        }

        protected override void Render( HtmlTextWriter writer )
        {
            if( Templated )
                writer.Write( Util.Render( delegate( HtmlTextWriter w ) { base.Render( w ); } ) );
            else
                base.Render( writer );
        }
    }
}
