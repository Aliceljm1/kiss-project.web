using Kiss.Utils;
using System.Collections.Generic;

namespace Kiss.Web.Controls
{
    public class Sortable : UIBase
    {
        public string ConnectWith { get; set; }
        public string Handle { get; set; }

        protected override void AppendJsIncludes()
        {
            JsIncludes.Add("ui.core");
            JsIncludes.Add("ui.widget");
            JsIncludes.Add("ui.mouse");
            JsIncludes.Add("ui.sortable");
        }

        protected override void AppendJsBlock()
        {
            Js.Append("$(function(){");
            Js.AppendFormat("$('{0}')", Selector);
            Js.Append(".sortable({");

            List<string> list = new List<string>();

            if (StringUtil.HasText(ConnectWith))
                list.Add(string.Format("connectWith:'{0}'", ConnectWith));

            if (StringUtil.HasText(Handle))
                list.Add(string.Format("handle:'{0}'", Handle));

            Js.Append(string.Join(",", list.ToArray()));

            Js.Append("});");

            Js.AppendFormat("$('{0}').disableSelection();", Selector);

            Js.Append("});");
        }
    }
}
