using Kiss.Utils;
using System.Web.UI;

namespace Kiss.Web.Controls
{
    public class Slider : UIBase
    {
        #region props

        public bool Animate { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public bool Vertical { get; set; }
        public string Range { get; set; }
        public int Step { get; set; }
        public int Value { get; set; }
        public string Values { get; set; }

        #endregion

        #region event

        public string onStart { get; set; }
        public string onSlide { get; set; }
        public string onChange { get; set; }
        public string onStop { get; set; }

        #endregion

        protected override void AppendJsIncludes()
        {
            JsIncludes.Add("ui.core");
            JsIncludes.Add("ui.widget");
            JsIncludes.Add("ui.mouse");
            JsIncludes.Add("ui.slider");
        }

        protected override void AppendJsBlock()
        {
            Js.Append("$(function() {");

            Js.AppendFormat("$('{0}').slider(", Selector);
            Js.Append("{");

            Js.AppendFormat("animate:{0}", Animate.ToString().ToLower());

            if (Max > 0)
                Js.AppendFormat(",max:{0}", Max);

            if (Min > 0)
                Js.AppendFormat(",min:{0}", Min);

            if (Vertical)
                Js.Append(",orientation:'vertical'");

            if (StringUtil.HasText(Range))
                Js.AppendFormat(",range:{0}", Range);

            if (Step > 0)
                Js.AppendFormat(",step:{0}", Step);

            if (Value > 0)
                Js.AppendFormat(",value:{0}", Value);

            if (StringUtil.HasText(Values))
                Js.AppendFormat(",values:{0}", Values);

            if (StringUtil.HasText(onStart))
                Js.AppendFormat(",start:{0}", onStart);

            if (StringUtil.HasText(onSlide))
                Js.AppendFormat(",slide:{0}", onSlide);

            if (StringUtil.HasText(onChange))
                Js.AppendFormat(",change:{0}", onChange);

            if (StringUtil.HasText(onStop))
                Js.AppendFormat(",stop:{0}", onStart);

            Js.Append("});");

            Js.Append("});");
        }
    }
}
