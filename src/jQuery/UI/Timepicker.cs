using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Timepicker : UIBase
    {
        protected override void AppendJsIncludes()
        {
            JsIncludes.Add("ui.core");
            JsIncludes.Add("ui.widget");
            JsIncludes.Add("ui.mouse");
            JsIncludes.Add("ui.slider");
            JsIncludes.Add("ui.datepicker");
            JsIncludes.Add("addon.timepicker");
        }        
    }
}
