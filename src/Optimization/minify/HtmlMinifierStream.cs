using System.IO;
using System.Text.RegularExpressions;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// minify html
    /// </summary>
    class HtmlMinifierStream : HttpOutputFilter
    {
        private static readonly Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}", RegexOptions.Compiled);

        /// <summary>
        /// Creates a new Minify Filter for the provided stream
        /// </summary>
        public HtmlMinifierStream(Stream stream)
            : base(stream)
        {
            TransformString += (content) =>
            {
                //TODO translate the HTML minifier from http://code.google.com/p/minify/ and use that
                return reg.Replace(content, string.Empty);
            };
        }
    }
}