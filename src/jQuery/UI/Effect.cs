using System;
using System.Collections.Generic;
using System.Text;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Effect : EffectBase
    {
        public enum EffectName
        {
            Blind ,
            Bounce ,
            Clip ,
            Drop ,
            Explode ,
            Fold ,
            Highlight ,
            Puff ,
            Pulsate ,
            Scale ,
            Shake ,
            Size ,
            Slide ,
            Transfer
        }

        public EffectName Name { get; set; }

        private string _options;
        public string Options
        {
            get
            {

                if ( StringUtil.HasText ( _options ) )
                    return _options;

                if ( Name == EffectName.Scale )
                    return "{percent: 0}";

                if ( Name == EffectName.Size )
                    return "{to:{width: 200,height: 60}}";

                if ( Name == EffectName.Transfer )
                    return "{" + string.Format ( "to: '{0}', className: 'ui-effects-transfer'" , TriggersId ) + "}";

                return "{}";
            }
            set { _options = value; }
        }

        public string Callback { get; set; }

        public enum ActionType
        {
            Toggle ,
            Show ,
            Hide
        }

        public ActionType Action { get; set; }

        protected override void AppendJsIncludes ( )
        {
            base.AppendJsIncludes ( );

            JsIncludes.Add ( "effects." + Name.ToString ( ).ToLower ( ) );
        }

        protected override void AppendEffectFunc ( )
        {
            Js.AppendFormat ( "$('{0}').{1}('{2}',{3},{4}{5});" ,
                HtmlId ,
                Action.ToString ( ).ToLower ( ) ,
                Name.ToString ( ).ToLower ( ) ,
                Options ,
                Duration ,
                StringUtil.IsNullOrEmpty ( Callback ) ? string.Empty : "," + Callback );
        }
    }
}
