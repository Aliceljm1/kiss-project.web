#region File Comment
//+-------------------------------------------------------------------+
//+ FileName: 	    EffectBase.cs
//+ File Created:   20090811
//+-------------------------------------------------------------------+
//+ Purpose:        EffectBase定义
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 20090811        ZHLI Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections.Generic;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    [ParseChildren ( typeof ( List<Trigger> ), ChildrenAsProperties = true, DefaultProperty = "Triggers" ),
    PersistChildren ( false )]
    public abstract class EffectBase : UIBase
    {
        /// <summary>
        /// Duration in second
        /// </summary>
        private int _duration = 1000;
        public int Duration { get { return _duration <= 0 ? 1000 : _duration; } set { _duration = value * 1000; } }

        private string _funcName;
        public string FuncName
        {
            get
            {
                if ( StringUtil.IsNullOrEmpty ( _funcName ) )
                    _funcName = "effect" + DateTime.Now.Millisecond;
                return _funcName;
            }
            set { _funcName = value; }
        }

        public List<Trigger> Triggers { get; set; }

        /// <summary>
        /// 是否加载样式文件
        /// </summary>
        public bool IncludeCss { get; set; }

        protected string TriggersId
        {
            get
            {
                return StringUtil.CollectionToCommaDelimitedString ( StringUtil.ToStringArray<Trigger> ( Triggers.ToArray ( ), delegate ( Trigger t ) { return "#" + t.TriggerID; } ) );
            }
        }

        protected override void OnLoad ( EventArgs e )
        {
            if ( IncludeCss )
                Proxy.RegisterCssResource ( typeof ( UIBase ),
                    string.Format ( "Kiss.Web.jQuery.UI.Res.themes.{0}.style.css", SiteConfig.Instance.jQueryUI ) );
        }

        protected override void AppendJsIncludes ( )
        {
            JsIncludes.Add ( "effects.core" );
        }

        protected override void AppendJsBlock ( )
        {
            if (StringUtil.IsNullOrEmpty(HtmlId))
                return;

            Js.AppendFormat ( ";var {0} = function()", FuncName );
            Js.Append ( "{" );
            AppendEffectFunc ( );
            Js.Append ( "};" );

            if ( Triggers != null && Triggers.Count > 0 )
            {
                Js.Append ( "$(function() {" );

                foreach ( Trigger trigger in Triggers )
                {
                    if ( StringUtil.IsNullOrEmpty ( trigger.TriggerID ) )
                        continue;

                    Js.AppendFormat ( "$('{0}').{1}(function()", trigger.TriggerID, trigger.Event ?? "click" );
                    Js.Append ( "{" );
                    Js.AppendFormat ( "{0}();", FuncName );
                    Js.Append ( "});" );
                }

                Js.Append ( "});" );
            }
        }

        protected abstract void AppendEffectFunc ( );
    }

    public class Trigger
    {
        public string TriggerID { get; set; }
        public string Event { get; set; }
    }
}
