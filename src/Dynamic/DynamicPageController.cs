using Kiss.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kiss.Web.Dynamic
{
    [Controller("_page")]
    class DynamicPageController : Controller
    {
        ActionResult index()
        {
            Regex r = new Regex(@"(\[[a-zA-Z0-9_]*\])");

            List<string> tokens = new List<string>();

            foreach (Match item in r.Matches(jc.Navigation.Url.UrlTemplate))
            {
                if (!item.Success && item.Value.Length == 2) continue;

                tokens.Add(item.Value.Substring(1, item.Value.Length - 2));
            }

            List<string> values = new List<string>();

            foreach (var token in tokens)
            {
                if (!string.IsNullOrEmpty(jc.Params[token]))
                    values.Add(jc.Params[token]);
            }

            DictSchema schema = (from q in DictSchema.CreateContext(true)
                                 where q.Type == "pages" && q.SiteId == jc.SiteId && q.Category == jc.Site.SiteKey && q.Prop2 == jc.Navigation.Url.UrlTemplate && q.Name == values.FirstOrDefault()
                                 select q).FirstOrDefault();

            if (tokens.Count > 0 && schema == null)
            {
                httpContext.Response.StatusCode = 404;
                httpContext.Response.StatusDescription = "File not found.";
                httpContext.Response.End();

                return new EmptyResult();
            }

            string skinname = jc.Navigation.Action;

            if (schema != null)
            {
                ViewData["page"] = schema;

                if (!string.IsNullOrEmpty(schema.Prop1))
                {
                    skinname = schema.Prop1;
                }
            }
            else
            {
                DictSchema route = (from q in DictSchema.CreateContext(true)
                                    where q.Type == "routes" && q.Category == jc.Site.SiteKey && q.Name == jc.Navigation.Url.UrlTemplate
                                    select q).FirstOrDefault();

                if (route != null && !string.IsNullOrEmpty(route["skinname"]))
                    skinname = route["skinname"];
            }

            return new ViewResult(skinname);
        }
    }
}
