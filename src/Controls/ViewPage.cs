using System.Web.UI;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this page to render templated data
    /// </summary>
    public class ViewPage : Page
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
