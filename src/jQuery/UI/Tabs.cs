using System.Collections.Generic;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Tabs : UIBase
    {
        #region props

        /// <summary>
        /// Additional Ajax options to consider when loading tab content (see $.ajax).
        /// </summary>
        public string AjaxOptions { get; set; }

        /// <summary>
        /// Whether or not to cache remote tabs content
        /// e.g. load only once or with every click. Cached content is being lazy loaded, 
        /// e.g once and only once for the first click. 
        /// Note that to prevent the actual Ajax requests from being cached by the browser you need to provide an extra cache: false flag to ajaxOptions.
        /// </summary>
        public bool Cache { get; set; }

        /// <summary>
        /// Set to true to allow an already selected tab to become unselected again upon reselection.
        /// </summary>
        public bool Collapsible { get; set; }

        /// <summary>
        /// Store the latest selected tab in a cookie. 
        /// The cookie is then used to determine the initially selected tab if the selected option is not defined. 
        /// Requires cookie plugin. 
        /// The object needs to have key/value pairs of the form the cookie plugin expects as options. 
        /// Available options (example): { expires: 7, path: '/', domain: 'jquery.com', secure: true }. 
        /// Since jQuery UI 1.7 it is also possible to define the cookie name being used via name property.
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// An array containing the position of the tabs (zero-based index) that should be disabled on initialization.
        /// </summary>
        public string Disabled { get; set; }

        /// <summary>
        /// The type of event to be used for selecting a tab.
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Enable animations for hiding and showing tab panels. 
        /// The duration option can be a string representing one of the three predefined speeds ("slow", "normal", "fast") 
        /// or the duration in milliseconds to run an animation (default is "normal").
        /// </summary>
        public string Fx { get; set; }

        /// <summary>
        /// If the remote tab, its anchor element that is, has no title attribute to generate an id from, 
        /// an id/fragment identifier is created from this prefix and a unique id returned by $.data(el), for example "ui-tabs-54".
        /// </summary>
        public string IdPrefix { get; set; }

        /// <summary>
        /// HTML template from which a new tab panel is created in case of adding a tab with the add method or when creating a panel for a remote tab on the fly.
        /// </summary>
        public string PanelTemplate { get; set; }

        /// <summary>
        /// Zero-based index of the tab to be selected on initialization. To set all tabs to nselected pass -1 as value.
        /// </summary>
        public int Selected { get; set; }

        /// <summary>
        /// The HTML content of this string is shown in a tab title while remote content is loading. 
        /// Pass in empty string to deactivate that behavior.
        /// </summary>
        public string Spinner { get; set; }

        /// <summary>
        /// HTML template from which a new tab is created and added. 
        /// The placeholders #{href} and #{label} are replaced with the url and tab label that are passed as arguments to the add method.
        /// </summary>
        public string TabTemplate { get; set; }

        public bool Sortable { get; set; }

        public bool Vertical { get; set; }

        #endregion

        #region events

        /// <summary>
        /// This event is triggered when clicking a tab.
        /// </summary>
        public string onSelect { get; set; }

        /// <summary>
        /// This event is triggered after the content of a remote tab has been loaded.
        /// </summary>
        public string onLoad { get; set; }

        /// <summary>
        /// This event is triggered when a tab is shown.
        /// </summary>
        public string onShow { get; set; }

        /// <summary>
        /// This event is triggered when a tab is added.
        /// </summary>
        public string onAdd { get; set; }

        /// <summary>
        /// This event is triggered when a tab is removed.
        /// </summary>
        public string onRemove { get; set; }

        /// <summary>
        /// This event is triggered when a tab is enabled.
        /// </summary>
        public string onEnable { get; set; }

        /// <summary>
        /// This event is triggered when a tab is disabled.
        /// </summary>
        public string onDisable { get; set; }

        #endregion

        protected override void AppendJsIncludes()
        {
            List<string> list = new List<string>() {
                "ui.core",
                "ui.widget",
                "ui.tabs"
            };

            if (Sortable)
                list.Add("ui.sortable");

            JsIncludes.AddRange(list);
        }

        protected override void AppendJsBlock()
        {
            Js.Append("$(function() {");

            Js.AppendFormat("$('{0}').tabs", Selector);
            Js.Append("({");

            Js.AppendFormat("event:'{0}'", Event ?? "click");

            if (StringUtil.HasText(AjaxOptions))
                Js.AppendFormat(",ajaxOptions:{0}", AjaxOptions);

            if (Cache)
                Js.Append(",cache:true");

            if (Collapsible)
                Js.Append(",collapsible:true");

            if (StringUtil.HasText(Cookie))
                Js.AppendFormat(",cookie:{0}", Cookie);

            if (StringUtil.HasText(Disabled))
                Js.AppendFormat(",disabled:{0}", Disabled);

            if (StringUtil.HasText(Fx))
                Js.AppendFormat(",fx:{0}", Fx);

            if (StringUtil.HasText(IdPrefix))
                Js.AppendFormat(",idPrefix:'{0}'", IdPrefix);

            if (StringUtil.HasText(PanelTemplate))
                Js.AppendFormat(",panelTemplate:\"{0}\"", PanelTemplate);

            if (Selected > 0)
                Js.AppendFormat(",selected:{0}", Selected);

            if (StringUtil.HasText(Spinner))
                Js.AppendFormat(",spinner:'{0}'", Spinner);

            if (StringUtil.HasText(TabTemplate))
                Js.AppendFormat(",tabTemplate:\"{0}\"", TabTemplate);

            if (StringUtil.HasText(onSelect))
                Js.AppendFormat(",select:{0}", onSelect);

            if (StringUtil.HasText(onLoad))
                Js.AppendFormat(",load:{0}", onLoad);

            if (StringUtil.HasText(onShow))
                Js.AppendFormat(",show:{0}", onShow);

            if (StringUtil.HasText(onAdd))
                Js.AppendFormat(",add:{0}", onAdd);

            if (StringUtil.HasText(onRemove))
                Js.AppendFormat(",remove:{0}", onRemove);

            if (StringUtil.HasText(onEnable))
                Js.AppendFormat(",enable:{0}", onEnable);

            if (StringUtil.HasText(onDisable))
                Js.AppendFormat(",disable:{0}", onDisable);

            Js.Append("})");

            if (Sortable)
                Js.AppendFormat(".find('.ui-tabs-nav').sortable({0})", Vertical ? "{axis:'y'}" : "{axis:'x'}");

            if (Vertical)
            {
                Js.Append(".addClass('ui-tabs-vertical ui-helper-clearfix');");
                Js.AppendFormat("$('{0} li').removeClass('ui-corner-top').addClass('ui-corner-left');", Selector);
            }

            Js.Append("});");
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (StringUtil.HasText(Cookie))
                ClientScriptProxy.Current.RegisterJsResource(writer, "Kiss.Web.jQuery.cookie.js");
        }
    }
}
