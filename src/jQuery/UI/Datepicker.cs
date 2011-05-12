#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-05-18
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-05-18		zhli Comment Created
//+-------------------------------------------------------------------+
//+ 2009-05-19		zhli add Duration, ShowAnim properties
//+-------------------------------------------------------------------+
#endregion

using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Datepicker : UIBase
    {
        /// <summary>
        /// The format for parsed and displayed dates.
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Allows you to change the month by selecting from a drop-down list. 
        /// You can enable this feature by setting the attribute to true.
        /// </summary>
        public bool ChangeMonth { get; set; }

        /// <summary>
        /// Allows you to change the year by selecting from a drop-down list. 
        /// You can enable this feature by setting the attribute to true.
        /// </summary>
        public bool ChangeYear { get; set; }

        /// <summary>
        /// Have the datepicker appear automatically when the field receives focus ('focus'), 
        /// appear only when a button is clicked ('button'), 
        /// or appear when either event takes place ('both').
        /// </summary>
        public string ShowOn { get; set; }

        /// <summary>
        /// The URL for the popup button image. 
        /// If set, button text becomes the alt value and is not directly displayed
        /// </summary>
        public string ButtonImage { get; set; }

        /// <summary>
        /// Set to true to place an image after the field to use as the trigger 
        /// without it appearing on a button.
        /// </summary>
        public bool ButtonImageOnly { get; set; }

        /// <summary>
        /// Set a maximum selectable date via a Date object, 
        /// or a number of days from today (e.g. +7) 
        /// or a string of values and periods ('y' for years, 'm' for months, 'w' for weeks, 'd' for days, e.g. '+1m +1w'), 
        /// or null for no limit.
        /// </summary>
        public string MaxDate { get; set; }

        /// <summary>
        /// Set a minimum selectable date via a Date object, 
        /// or a number of days from today (e.g. +7) 
        /// or a string of values and periods ('y' for years, 'm' for months, 'w' for weeks, 'd' for days, e.g. '-1y -1m'), 
        /// or null for no limit.
        /// </summary>
        public string MinDate { get; set; }

        /// <summary>
        /// Set the name of the animation used to show/hide the datepicker. 
        /// Use 'show' (the default), 'slideDown', 'fadeIn', 
        /// or any of the show/hide jQuery UI effects.
        /// </summary>
        public string ShowAnim { get; set; }

        /// <summary>
        /// Control the speed at which the datepicker appears, 
        /// it may be a time in milliseconds, 
        /// a string representing one of the three predefined speeds ("slow", "normal", "fast"), 
        /// or '' for immediately.
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Can be a function that takes an input field and current datepicker instance 
        /// and returns an options object to update the datepicker with. 
        /// It is called just before the datepicker is displayed.
        /// </summary>
        public string onBeforeShow { get; set; }

        /// <summary>
        /// The function takes a date as a parameter 
        /// and must return an array with [0] equal to true/false indicating 
        /// whether or not this date is selectable, [1] equal to a CSS class name(s) or 
        /// '' for the default presentation and [2] an optional popup tooltip for this date. 
        /// It is called for each day in the datepicker before is it displayed.
        /// </summary>
        public string onBeforeShowDay { get; set; }

        /// <summary>
        /// Allows you to define your own event when the datepicker moves to a new month and/or year. 
        /// The function receives the selected year, month and the datepicker instance as parameters. 
        /// this refers to the associated input field.
        /// </summary>
        public string onChangeMonthYear { get; set; }

        /// <summary>
        /// Allows you to define your own event when the datepicker is closed, whether or not a date is selected. 
        /// The function receives the selected date as a Date and the datepicker instance as parameters. 
        /// this refers to the associated input field.
        /// </summary>
        public string onClose { get; set; }

        /// <summary>
        /// Allows you to define your own event when the datepicker is selected. 
        /// The function receives the selected date(s) as text and the datepicker instance as parameters. 
        /// this refers to the associated input field.
        /// </summary>
        public string onSelect { get; set; }

        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "ui.core" );
            JsIncludes.Add ( "ui.datepicker" );
        }

        protected override void AppendJsBlock ( )
        {
            Js.Append ( "$(function(){" );
            Js.AppendFormat ( "$('{0}')" , HtmlId );
            Js.Append ( ".datepicker({" );

            Js.AppendFormat ( "changeMonth:{0}" , StringUtil.ToJsBoolean ( ChangeMonth ) );

            if ( ChangeYear )
                Js.Append ( ",changeYear:true" );

            if ( StringUtil.HasText ( DateFormat ) )
                Js.AppendFormat ( ",dateFormat:'{0}'" , DateFormat );

            if ( StringUtil.HasText ( ShowOn ) )
                Js.AppendFormat ( ",showOn:'{0}'" , ShowOn );

            if ( StringUtil.HasText ( ButtonImage ) )
                Js.AppendFormat ( ",buttonImage:'{0}'" , ButtonImage );

            if ( ButtonImageOnly )
                Js.Append ( ",buttonImageOnly:true" );

            if ( StringUtil.HasText ( MaxDate ) )
                Js.AppendFormat ( ",maxDate:'{0}'" , MaxDate );

            if ( StringUtil.HasText ( MinDate ) )
                Js.AppendFormat ( ",minDate:'{0}'" , MinDate );

            if ( StringUtil.HasText ( ShowAnim ) )
                Js.AppendFormat ( ",showAnim:'{0}'" , ShowAnim );

            if ( StringUtil.HasText ( Duration ) )
                Js.AppendFormat ( ",duration:'{0}'" , Duration );

            if ( StringUtil.HasText ( onBeforeShow ) )
                Js.AppendFormat ( ",beforeShow:{0}" , onBeforeShow );

            if ( StringUtil.HasText ( onBeforeShowDay ) )
                Js.AppendFormat ( ",beforeShowDay:{0}" , onBeforeShowDay );

            if ( StringUtil.HasText ( onChangeMonthYear ) )
                Js.AppendFormat ( ",onChangeMonthYear:{0}" , onChangeMonthYear );

            if ( StringUtil.HasText ( onClose ) )
                Js.AppendFormat ( ",onClose:{0}" , onClose );

            if ( StringUtil.HasText ( onSelect ) )
                Js.AppendFormat ( ",onSelect:{0}" , onSelect );

            Js.Append ( "})});" );
        }
    }
}
