
namespace Kiss.Web.Controls
{
    public class Draggable : UIBase
    {
        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "ui.core" );
            JsIncludes.Add ( "ui.draggable" );
        }

        protected override void AppendJsBlock ( )
        {
            Js.Append ( "$(function(){" );
            Js.AppendFormat ( "$('{0}')" , HtmlId );
            Js.Append ( ".draggable({" );


            Js.Append ( "})});" );
        }
    }
}
