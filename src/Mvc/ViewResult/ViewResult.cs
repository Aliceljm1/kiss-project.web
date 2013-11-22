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
            IsFile = true;
        }

        public ViewResult(string view, bool isFile)
        {
            ViewName = view;
            IsFile = isFile;
        }

        public string ViewName { get; set; }
        public bool IsFile { get; set; }

        public override void ExecuteResult(JContext jc)
        {
            jc.Items["__viewResult__"] = ViewName;
            jc.Items["__viewResult_IsFile__"] = IsFile;

            if (!jc.RenderContent)
            {
                jc.Context.Response.Write(new TemplatedControl()
                {
                    SkinName = ViewName,
                    UsedInMvc = !ViewName.StartsWith("/"),
                    OverrideSkinName = true,
                    Templated = true
                }.Execute());
            }
        }
    }
}
