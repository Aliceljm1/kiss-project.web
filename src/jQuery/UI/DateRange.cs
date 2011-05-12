#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-05-18
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-05-18		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Text;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 用于选择一个范围内的日期
    /// </summary>
    public class DateRange : Datepicker
    {
        public string Min { get; set; }
        public string Max { get; set; }

        protected override void OnLoad ( EventArgs e )
        {
            HtmlId = Min + "," + Max;

            StringBuilder js = new StringBuilder ( );
            js.Append ( "function(input){" );

            js.AppendFormat ( "$('{0}').datepicker('option', 'maxDate', $('{1}').datepicker( 'getDate' ) );" , Min , Max );
            js.AppendFormat ( "$('{0}').datepicker('option', 'minDate', $('{1}').datepicker( 'getDate' ) );" , Max , Min );

            js.Append ( "}" );

            onBeforeShow = js.ToString ( );

            base.OnLoad ( e );
        }
    }
}
