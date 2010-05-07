using System.Web.UI;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this control to render template data in a usercontrol
    /// </summary>
    public class ViewUserControl : UserControl
    {
        /// <summary>
        /// 是否启用模板渲染
        /// </summary>
        public bool EnableTemplate { get; set; }

        protected override void Render ( HtmlTextWriter writer )
        {
            if ( EnableTemplate )
                writer.Write ( Util.Render ( delegate ( HtmlTextWriter w ) { base.Render ( w ); } ) );
            else
                base.Render ( writer );
        }
    }
}
