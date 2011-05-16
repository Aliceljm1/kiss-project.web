
namespace Kiss.Web.Controls
{
    public class Resizable : UIBase
    {
        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "ui.core" );
            JsIncludes.Add ( "ui.resizable" );
        }

        protected override void AppendJsBlock ( )
        {
            Js.Append ( "$(function(){" );
            Js.AppendFormat ( "$('{0}')" , Selector );
            Js.Append ( ".resizable({" );


            Js.Append ( "})});" );
        }
    }
}
