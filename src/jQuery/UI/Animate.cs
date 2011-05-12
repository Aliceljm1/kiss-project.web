
namespace Kiss.Web.Controls
{
    public class Animate : EffectBase
    {
        public string ToStyle { get; set; }

        protected override void AppendEffectFunc ( )
        {
            Js.AppendFormat ( "$('{0}').animate({1}, {2});" , HtmlId , ToStyle ?? "{backgroundColor: '#aa0000', color: '#fff', width: 500}" , Duration );
        }
    }
}
