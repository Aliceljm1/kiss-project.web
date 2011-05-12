using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Progressbar : UIBase
    {
        public int Value { get; set; }
        public string onChange { get; set; }

        protected override void Render ( HtmlTextWriter writer )
        {
            writer.Write ( string.Format ( "<div id='{0}'></div>" ,
                HtmlId ) );

            base.Render ( writer );
        }

        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "ui.core" );
            JsIncludes.Add ( "ui.progressbar" );
        }

        protected override void AppendJsBlock ( )
        {
            Js.AppendFormat ( "$('{0}')" , HtmlId );
            Js.Append ( ".progressbar({" );
            Js.AppendFormat ( "value: {0}" , Value );

            if ( StringUtil.HasText ( onChange ) )
                Js.AppendFormat ( ",change:{0}" , onChange );

            Js.Append ( "});" );
        }
    }
}
