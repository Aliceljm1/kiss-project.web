using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// Button enhances standard form elements like button, input of type submit or reset or anchors to themable buttons with appropiate mouseover and active styles.
    /// In addition to basic push buttons, radio buttons and checkboxes (inputs of type radio and checkbox) can be converted to buttons: Their associated label is styled to appear as the button, while the underlying input is updated on click.
    /// In order to group radio buttons, Button also provides an additional widget-method, called Buttonset. Its used by selecting a container element (which contains the radio buttons) and calling buttonset(). Buttonset will also provide visual grouping, and therefore should be used whenever you have a group of buttons. It works by selecting all descendents and applying button() to them. You can enable and disable a buttonset, which will enable and disable all contained buttons. Destroying a buttonset also calls the button's destroy method.
    /// When using an input of type button, submit or reset, support is limited to plain text labels with no icons.
    /// </summary>
    public class Button : UIBase
    {
        /// <summary>
        /// Disables (true) or enables (false) the button. Can be set when initialising (first creating) the button.
        /// </summary>
        public bool disabled { get; set; }
        /// <summary>
        /// Whether to show any text - when set to false (display no text), icons (see icons option) must be enabled, otherwise it'll be ignored.
        /// </summary>
        public bool text { get; set; }
        /// <summary>
        /// Icons to display, with or without text (see text option). The primary icon is displayed by default on the left of the label text, the secondary by default is on the right. Value for the primary and secondary properties must be a classname (String), eg. "ui-icon-gear". For using only one icon: icons: {primary:'ui-icon-locked'}. For using two icons: icons: {primary:'ui-icon-gear',secondary:'ui-icon-triangle-1-s'}
        /// </summary>
        public string icons { get; set; }
        /// <summary>
        /// Text to show on the button. When not specified (null), the element's html content is used, or its value attribute when it's an input element of type submit or reset; or the html content of the associated label element if its an input of type radio or checkbox
        /// </summary>
        public string label { get; set; }

        public Button()
        {
            text = true;
        }

        protected override void AppendJsIncludes()
        {
            JsIncludes.Add("ui.core");
            JsIncludes.Add("ui.widget");
            JsIncludes.Add("ui.button");
        }

        protected override void AppendJsBlock()
        {
            Js.Append("$(function() {");

            Js.AppendFormat("$('{0}').button", Selector);
            Js.Append("({");

            Js.AppendFormat("disabled:{0}", disabled.ToString().ToLower());

            if (!text)
                Js.Append(",text:false");

            if (StringUtil.HasText(icons))
                Js.AppendFormat(",icons:{0}", icons);

            if (StringUtil.HasText(label))
                Js.AppendFormat(",label:'{0}'", label);

            Js.Append("})});");
        }
    }
}
