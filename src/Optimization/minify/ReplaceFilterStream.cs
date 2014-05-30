using System.IO;
using System.Text.RegularExpressions;

namespace Kiss.Web.Optimization
{
    abstract class ReplaceFilterStream : HttpOutputFilter
    {
        public ReplaceFilterStream(Stream stream)
            : base(stream)
        {
            TransformString += (content) =>
            {
                return Find(content);
            };
        }

        private string Find(string html)
        {
            return SubjectPattern.Replace(html, Found);
        }

        private string Found(Match m)
        {
            return ReplacePattern.Replace(m.Value, Replace);
        }

        public abstract Regex SubjectPattern { get; }
        public abstract Regex ReplacePattern { get; }
        public abstract string Replace(Match m);
    }
}
