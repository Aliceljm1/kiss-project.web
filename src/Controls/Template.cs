#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-05-22
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-05-22		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.Web.UI;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this control to render templated data
    /// </summary>
    public class Template : Control
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Util.Render(delegate(HtmlTextWriter w) { base.Render(w); }));
        }
    }

    /// <summary>
    /// use this page to render templated data
    /// </summary>
    public class TemplatePage : Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Util.Render(delegate(HtmlTextWriter w) { base.Render(w); }));
        }
    }
}
