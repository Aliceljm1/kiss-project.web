using System;
using System.IO;
using System.Web;

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
                    return new Kiss.Web.Utils.ajaxmin.Minifier().MinifyStyleSheet(content);
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