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
                    return new JsMin().MinifyString(content);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<JavascriptMinifierStream>().Error("error minify script:{0}. Exception:{1}", HttpContext.Current.Request.RawUrl, ex.Message);
                }

                return content;
            };
        }
    }
}