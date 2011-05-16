
namespace Kiss.Web.Controls
{
    public class SwitchClass : EffectBase
    {
        public string Class1 { get; set; }
        public string Class2 { get; set; }

        protected override void AppendEffectFunc ( )
        {
            Js.AppendFormat ( "$('{0}').switchClass('{1}', '{2}', {3});" ,
                Selector ,
                Class1 ,
                Class2 ,
                Duration );
        }
    }
}
