
namespace Kiss.Web.Controls
{
    public class ToggleClass : EffectBase
    {
        public string ClassName { get; set; }

        protected override void AppendEffectFunc ( )
        {
            Js.AppendFormat ( "$('{0}').toggleClass('{1}', {2});" , Selector , ClassName ?? "class1" , Duration );
        }
    }
}
