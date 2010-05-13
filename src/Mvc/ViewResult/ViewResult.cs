
namespace Kiss.Web.Mvc
{
    public class ViewResult : ActionResult
    {
        public ViewResult()
        {
        }

        public ViewResult(string view)
        {
            ViewName = view;
        }

        public string ViewName { get; set; }

        public override void ExecuteResult(JContext jc)
        {
            jc.Items["__viewResult__"] = ViewName;
        }
    }
}
