using System.Collections.Generic;
using System.Collections.Specialized;

namespace Kiss.Web
{
    public interface ITextProcesser
    {
        string ToCode(string content, NameValueCollection param, out List<string> refIds);

        string ToHtml(JContext jc, string code);
    }
}
