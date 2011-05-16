using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Class : EffectBase
    {
        public string ClassName { get; set; }
        public string Callback { get; set; }

        public enum ClassAction
        {
            Add ,
            Remove
        }

        public ClassAction Action { get; set; }

        protected override void AppendEffectFunc ( )
        {
            Js.AppendFormat ( "$('{0}').{1}Class('{2}', {3}{4});" ,
                Selector ,
                Action.ToString ( ).ToLower ( ) ,
                ClassName ,
                Duration ,
                StringUtil.IsNullOrEmpty ( Callback ) ? string.Empty : "," + Callback );
        }
    }
}
