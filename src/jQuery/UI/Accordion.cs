using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Accordion : UIBase
    {
        #region props

        /// <summary>
        /// Selector for the active element. 
        /// Set to false to display none at start. Needs «collapsible: true».
        /// </summary>
        public string Active { get; set; }

        /// <summary>
        /// Choose your favorite animation, or disable them (set to false). 
        /// In addition to the default, 'bounceslide' and 'easeslide' are supported (both require the easing plugin).
        /// </summary>
        public string Animated { get; set; }

        /// <summary>
        /// If set, the highest content part is used as height reference for all other parts. 
        /// Provides more consistent animations.
        /// </summary>
        public bool AutoHeight { get; set; }

        /// <summary>
        /// If set, clears height and overflow styles after finishing animations. This enables accordions to work with dynamic content. 
        /// Won't work together with autoHeight.
        /// </summary>
        public bool ClearStyle { get; set; }

        /// <summary>
        /// Whether all the sections can be closed at once. 
        /// Allows collapsing the active section by the triggering event (click is the default).
        /// </summary>
        public bool Collapsible { get; set; }

        /// <summary>
        /// The event on which to trigger the accordion.
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// If set, the accordion completely fills the height of the parent element. Overrides autoheight.
        /// </summary>
        public bool FillSpace { get; set; }

        /// <summary>
        /// Selector for the header element.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Icons to use for headers. 
        /// Icons may be specified for 'header' and 'headerSelected'
        /// </summary>
        public string Icons { get; set; }

        /// <summary>
        /// If set, looks for the anchor that matches location.href and activates it. 
        /// Great for href-based state-saving. 
        /// Use navigationFilter to implement your own matcher.
        /// </summary>
        public bool Navigation { get; set; }

        /// <summary>
        /// Overwrite the default location.href-matching with your own matcher.
        /// </summary>
        public string NavigationFilter { get; set; }

        #endregion

        #region event

        public string onChange { get; set; }

        #endregion

        protected override void AppendJsIncludes()
        {
            JsIncludes.Add("ui.core");
            JsIncludes.Add("ui.widget");
            JsIncludes.Add("ui.accordion");
        }

        protected override void AppendJsBlock()
        {
            Js.Append("$(function() {");

            Js.AppendFormat("$('{0}').accordion", HtmlId);
            Js.Append("({");

            Js.AppendFormat("event:'{0}'", Event ?? "click");

            if (StringUtil.HasText(Active))
                Js.AppendFormat(",active:{0}", Active);
            else
                Js.AppendFormat(",active:{0}", JContext.Current.Navigation.Index);

            if (StringUtil.HasText(Animated))
                Js.AppendFormat(",animated:'{0}'", Animated);

            if (!AutoHeight)
                Js.Append(",autoHeight: false");

            if (ClearStyle)
                Js.Append(",clearStyle:true");

            if (Collapsible)
                Js.Append(",collapsible:true");

            if (FillSpace)
                Js.Append(",fillSpace:true");

            if (StringUtil.HasText(Header))
                Js.AppendFormat(",header:{0}", Header);

            if (StringUtil.HasText(Icons))
                Js.AppendFormat(",icons:{0}", Icons);

            if (Navigation)
                Js.Append(",navigation:true");

            if (StringUtil.HasText(NavigationFilter))
                Js.AppendFormat(",navigationFilter:{0}", NavigationFilter);

            if (StringUtil.HasText(onChange))
                Js.AppendFormat(",change:{0}", onChange);

            Js.Append("})});");

        }
    }
}
