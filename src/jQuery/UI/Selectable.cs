#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-05-21
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-05-21		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Selectable : UIBase
    {
        #region props

        /// <summary>
        /// This determines whether to refresh (recalculate) the position and size of each selectee at the beginning of each select operation. 
        /// If you have many many items, 
        /// you may want to set this to false and call the refresh method manually.
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// Prevents selecting if you start on elements matching the selector.
        /// </summary>
        public string Cancel { get; set; }

        /// <summary>
        /// Time in milliseconds to define when the selecting should start. 
        /// It helps preventing unwanted selections when clicking on an element.
        /// </summary>
        public int Delay { get; set; }

        public string Filter { get; set; }

        #endregion

        #region events

        public string onSelected { get; set; }
        public string onSelecting { get; set; }
        public string onStart { get; set; }
        public string onStop { get; set; }
        public string onUnselected { get; set; }
        public string onUnselecting { get; set; }

        #endregion

        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "ui.core" );
            JsIncludes.Add ( "ui.selectable" );
        }

        protected override void AppendJsBlock ( )
        {
            Js.Append ( "$(function(){" );
            Js.AppendFormat ( "$('{0}')" , Selector );
            Js.Append ( ".selectable({" );

            Js.AppendFormat ( "autoRefresh:{0}" , StringUtil.ToJsBoolean ( AutoRefresh ) );

            if ( StringUtil.HasText ( Cancel ) )
                Js.AppendFormat ( ",cancel:'{0}'" , Cancel );

            if ( Delay > 0 )
                Js.AppendFormat ( ",delay:{0}" , Delay );

            if ( StringUtil.HasText ( Filter ) )
                Js.AppendFormat ( ",filter:'{0}'" , Filter );

            if ( StringUtil.HasText ( onSelected ) )
                Js.AppendFormat ( ",selected: {0}" , onSelected );
            if ( StringUtil.HasText ( onSelecting ) )
                Js.AppendFormat ( ",selecting: {0}" , onSelecting );
            if ( StringUtil.HasText ( onStart ) )
                Js.AppendFormat ( ",start: {0}" , onStart );
            if ( StringUtil.HasText ( onStop ) )
                Js.AppendFormat ( ",stop: {0}" , onStop );
            if ( StringUtil.HasText ( onUnselected ) )
                Js.AppendFormat ( ",unselected: {0}" , onUnselected );
            if ( StringUtil.HasText ( onUnselecting ) )
                Js.AppendFormat ( ",unselecting: {0}" , onUnselecting );

            Js.Append ( "})});" );
        }
    }
}
