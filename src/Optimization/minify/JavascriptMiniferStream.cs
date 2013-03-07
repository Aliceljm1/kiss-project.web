using Kiss.Utils;
using Kiss.Web.Utils;
using System;
using System.IO;
using System.Web;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// minify javascript
    /// </summary>
    class JavascriptMinifierStream : HttpOutputFilter
    {
        /// <summary>
        /// Creates a new Minify Filter for the provided stream
        /// </summary>
        public JavascriptMinifierStream(Stream stream)
            : base(stream)
        {
            TransformString += (content) =>
            {
                try
                {
                    return new Kiss.Web.Utils.ajaxmin.Minifier().MinifyJavaScript(content);
                }
                catch (Exception ex)
                {
                    ILogger logger = LogManager.GetLogger<JavascriptMinifierStream>();
                    logger.Error("error minify script:{0}.", HttpContext.Current.Request.RawUrl);
                    logger.Error(ExceptionUtil.WriteException(ex));
                }

                return content;
            };
        }
    }
}