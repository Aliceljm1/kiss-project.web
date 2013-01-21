#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-23
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-23		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections.Specialized;

namespace Kiss.Web.Utils
{
    /// <summary>
    /// util class for http content type
    /// </summary>
    public static class ContentTypeUtil
    {
        private static readonly StringDictionary contentType = new StringDictionary();

        static ContentTypeUtil()
        {
            contentType.Add("csv", "application/vnd.ms-excel");
            contentType.Add("css", "text/css");
            contentType.Add("js", "text/javascript");
            contentType.Add("doc", "application/msword");
            contentType.Add("gif", "image/gif");
            contentType.Add("bmp", "image/bmp");
            contentType.Add("htm", "text/html");
            contentType.Add("html", "text/html");
            contentType.Add("jpeg", "image/jpeg");
            contentType.Add("jpg", "image/jpeg");
            contentType.Add("pdf", "application/pdf");
            contentType.Add("png", "image/png");
            contentType.Add("ppt", "application/vnd.ms-powerpoint");
            contentType.Add("rtf", "application/msword");
            contentType.Add("txt", "text/plain");
            contentType.Add("xls", "application/vnd.ms-excel");
            contentType.Add("xml", "text/xml");
            contentType.Add("wmv", "video/x-ms-wmv");
            contentType.Add("wma", "video/x-ms-wmv");
            contentType.Add("mpeg", "video/mpeg");
            contentType.Add("mpg", "video/mpeg");
            contentType.Add("mpa", "video/mpeg");
            contentType.Add("mpe", "video/mpeg");
            contentType.Add("mov", "video/quicktime");
            contentType.Add("qt", "video/quicktime");
            contentType.Add("avi", "video/x-msvideo");
            contentType.Add("asf", "video/x-ms-asf");
            contentType.Add("asr", "video/x-ms-asf");
            contentType.Add("asx", "video/x-ms-asf");
            contentType.Add("swf", "application/x-shockwave-flash");
        }

        public static string GetContentType(string extension)
        {
            extension = extension.Trim('.');

            if (contentType.ContainsKey(extension))
                return contentType[extension];

            throw new NotSupportedException();
        }
    }
}
