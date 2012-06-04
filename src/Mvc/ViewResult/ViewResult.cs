using Kiss.Web.Controls;

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
            if (jc.IsPost && !jc.RenderContent)
            {
                jc.Context.Response.Write(new TemplatedControl()
                {
                    SkinName = ViewName,
                    UsedInMvc = !ViewName.StartsWith("/"),
                    Templated = true
                }.Execute());
            }
            else
            {
                jc.Items["__viewResult__"] = ViewName;
            }
        }
    }
}
