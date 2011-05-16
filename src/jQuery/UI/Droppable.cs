
namespace Kiss.Web.Controls
{
    public class Droppable : UIBase
    {
        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "ui.core" );
            JsIncludes.Add ( "ui.droppable" );
        }

        protected override void AppendJsBlock ( )
        {
            Js.Append ( "$(function(){" );
            Js.AppendFormat ( "$('{0}')" , Selector );
            Js.Append ( ".droppable({" );


            Js.Append ( "})});" );
        }
    }
}
