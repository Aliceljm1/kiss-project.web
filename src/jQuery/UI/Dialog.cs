#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-08
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-08		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.Collections.Generic;
using System.Web.UI;
using Kiss.Utils;
using Kiss.Web.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 对话框
    /// </summary>    
    public class Dialog : UIBase
    {
        #region props

        public bool AutoOpen { get; set; }

        private bool _closeOnEscapse = true;
        public bool CloseOnEscapse { get { return _closeOnEscapse; } set { _closeOnEscapse = value; } }
        public string DialogClass { get; set; }
        public bool Draggable { get; set; }
        public int Height { get; set; }
        public string Hide { get; set; }
        public int MaxHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MinHeight { get; set; }
        public int MinWidth { get; set; }

        private bool _modal = true;
        public bool Modal { get { return _modal; } set { _modal = value; } }
        public string Position { get; set; }
        public bool Resizable { get; set; }
        public string Show { get; set; }
        public bool Stack { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public int ZIndex { get; set; }

        public string onBeforeclose { get; set; }
        public string onOpen { get; set; }
        public string onFocus { get; set; }
        public string onDragstart { get; set; }
        public string onDrag { get; set; }
        public string onDragstop { get; set; }
        public string onResizeStart { get; set; }
        public string onResize { get; set; }
        public string onResizeStop { get; set; }
        public string onClose { get; set; }

        #endregion

        protected override void AppendJsIncludes()
        {
            List<string> list = new List<string>() { 
                "ui.core",
                "ui.widget",                
                "ui.position",
                "ui.dialog"
            };

            if (Resizable || Draggable)
                list.Add("ui.mouse");

            if (Resizable)
                list.Add("ui.resizable");

            if (Draggable)
                list.Add("ui.draggable");

            if (RequestUtil.IsIE6)
                list.Add("Kiss.Web.jQuery.bgiframe.js");

            list.Add("Kiss.Web.jQuery.dialogutil.js");

            JsIncludes.AddRange(list);
        }

        protected override void AppendJsBlock()
        {
            Js.Append("jQuery(function(){");
            Js.AppendFormat("jQuery('{0}')", Selector);
            Js.Append(".dialog({");

            Js.AppendFormat("autoOpen: {0}", AutoOpen.ToString().ToLower());

            if (RequestUtil.IsIE6)
                Js.Append(",bgiframe:true");

            if (!CloseOnEscapse)
                Js.Append(",closeOnEscape:false");

            if (StringUtil.HasText(DialogClass))
                Js.AppendFormat(",dialogClass:'{0}'", DialogClass);

            if (!Draggable)
                Js.Append(",draggable:false");

            if (Height > 0)
                Js.AppendFormat(",height: {0}", Height);

            if (StringUtil.HasText(Hide))
                Js.AppendFormat(",hide: '{0}'", Hide);

            if (MaxHeight > 0)
                Js.AppendFormat(",maxHeight: {0}", MaxHeight);

            if (MaxWidth > 0)
                Js.AppendFormat(",maxWidth: {0}", MaxWidth);

            if (MinHeight > 0)
                Js.AppendFormat(",minHeight: {0}", MinHeight);

            if (MinWidth > 0)
                Js.AppendFormat(",minWidth: {0}", MinWidth);

            if (Modal)
                Js.Append(",modal: true");

            if (StringUtil.HasText(Position))
                Js.AppendFormat(",position:{0}", Position);

            if (!Resizable)
                Js.Append(",resizable:false");

            if (StringUtil.HasText(Show))
                Js.AppendFormat(",show: '{0}'", Show);

            if (!Stack)
                Js.Append(",stack:false");

            if (StringUtil.HasText(Title))
                Js.AppendFormat(",title: '{0}'", Title);

            if (Width > 0)
                Js.AppendFormat(",width: {0}", Width);

            if (ZIndex > 0)
                Js.AppendFormat(", zIndex: {0}", ZIndex);

            if (StringUtil.HasText(onBeforeclose))
                Js.AppendFormat(",beforeclose: {0}", onBeforeclose);

            if (StringUtil.HasText(onOpen))
                Js.AppendFormat(",open: {0}", onOpen);

            if (StringUtil.HasText(onFocus))
                Js.AppendFormat(",focus: {0}", onFocus);

            if (StringUtil.HasText(onDragstart))
                Js.AppendFormat(",dragStart: {0}", onDragstart);

            if (StringUtil.HasText(onDrag))
                Js.AppendFormat(",drag: {0}", onDrag);

            if (StringUtil.HasText(onDragstop))
                Js.AppendFormat(",dragStop: {0}", onDragstop);

            if (StringUtil.HasText(onResizeStart))
                Js.AppendFormat(",resizeStart: {0}", onResizeStart);

            if (StringUtil.HasText(onResize))
                Js.AppendFormat(",resize: {0}", onResize);

            if (StringUtil.HasText(onResizeStop))
                Js.AppendFormat(",resizeStop: {0}", onResizeStop);

            if (StringUtil.HasText(onClose))
                Js.AppendFormat(",close: {0}", onClose);

            Js.Append("});");

            Js.AppendFormat("dialogutil('{0}');", Selector);

            Js.Append("});");
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(string.Format("<div id='{0}' style='display:none;'>", Selector.TrimStart('#')));

            RenderChildren(writer);

            writer.Write("</div>");

            base.Render(writer);
        }
    }
}
