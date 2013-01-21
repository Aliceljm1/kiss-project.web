using System;
using System.IO;
using System.Web;
using Kiss.Web.Utils;
using System.Text.RegularExpressions;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// style minify
    /// </summary>
    class StyleMinifierStream : HttpOutputFilter
    {
        /// <summary>
        /// Creates a new Minify Filter for the provided stream
        /// </summary>
        public StyleMinifierStream(Stream stream)
            : base(stream)
        {
            TransformString += (content) =>
            {
                try
                {
                    content = content.Replace("\n", "");
                    content = Regex.Replace(content, @"\s+", " ");
                    content = Regex.Replace(content, @"\s*:\s*", ":");
                    content = Regex.Replace(content, @"\s*\,\s*", ",");
                    content = Regex.Replace(content, @"\s*\{\s*", "{");
                    content = Regex.Replace(content, @"\s*\}\s*", "}");
                    content = Regex.Replace(content, @"\s*\;\s*", ";");

                    return content;
                    //return CssMinifier.CssMinify(content);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<StyleMinifierStream>().Error("error minify css:{0}. Exception:{1}", HttpContext.Current.Request.RawUrl, ex.Message);
                }

                return content;
            };
        }
    }
}