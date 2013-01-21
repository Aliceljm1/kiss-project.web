using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Kiss.Web.Utils;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// minify inline style
    /// </summary>
    class InlineStyleMinifierStream : ReplaceFilterStream
    {
        private static readonly ILogger _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Match any <style> tag that does not have a src attribute, and include its inner content
        private static readonly Regex StyleWithoutSrc = new Regex(@"<styles?(?:(?!src).)*?/style>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        // Match the content within the matched <script> tag
        private static readonly Regex StyleContent = new Regex(@">[^>]*(.*)<",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        public InlineStyleMinifierStream(Stream stream)
            : base(stream)
        {
        }

        public override Regex ReplacePattern
        {
            get { return StyleContent; }
        }

        public override Regex SubjectPattern
        {
            get { return StyleWithoutSrc; }
        }

        public override string Replace(Match m)
        {
            if (string.IsNullOrEmpty(m.Value))
                return string.Empty;

            // Replace the angle brackets and minify the contents
            StringBuilder builder = new StringBuilder(">");
            try
            {
                var inner = m.Value.Substring(1, m.Value.Length - 2);

                builder.Append(CssMinifier.CssMinify(inner));
            }
            catch (Exception ex)
            {
                _log.Error("Error minifying inline CSS in {0}. style:{1}. exception:{2}",
                    HttpContext.Current.Request.RawUrl,
                    m.Value,
                    ex.Message);

                return m.Value;
            }
            builder.Append("<");
            return builder.ToString();
        }
    }
}