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
    /// minify inline javascript
    /// </summary>
    class InlineJavascriptMinifierStream : ReplaceFilterStream
    {
        private static readonly ILogger _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Match any <script> tag that does not have a src attribute, and include its inner content
        private static readonly Regex ScriptWithoutSrc = new Regex(@"<script(?:(?!src).)*?/script>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        // Match the content within the matched <style> tag
        private static readonly Regex ScriptContent = new Regex(@">[^>]*(.*)<",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        public InlineJavascriptMinifierStream(Stream stream)
            : base(stream)
        {
        }

        public override Regex ReplacePattern
        {
            get { return ScriptContent; }
        }

        public override Regex SubjectPattern
        {
            get { return ScriptWithoutSrc; }
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
                builder.Append(new Kiss.Web.Utils.ajaxmin.Minifier().MinifyJavaScript((inner)));
            }
            catch (Exception ex)
            {
                _log.Error("Error minifying inline javascript in {0}. script:{1}. exception:{2}",
                    HttpContext.Current.Request.Url,
                    m.Value,
                    ex.Message);
                return m.Value;
            }
            builder.Append("<");
            return builder.ToString();
        }
    }
}