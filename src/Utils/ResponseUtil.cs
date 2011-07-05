using System.Web;
using Kiss.Utils;

namespace Kiss.Web.Utils
{
    /// <summary>
    /// some util method of <see cref="System.Web.HttpResponse"/>
    /// </summary>
    public static class ResponseUtil
    {
        /// <summary>
        /// output object in json
        /// </summary>
        /// <param name="Response"></param>
        /// <param name="result"></param>
        public static void OutputJson(HttpResponse Response, object result)
        {
            OutputJson(Response, result, 0);
        }

        /// <summary>
        /// output object in json
        /// </summary>
        /// <param name="Response"></param>
        /// <param name="result"></param>
        /// <param name="cahceMinutes"></param>
        public static void OutputJson(HttpResponse Response, object result, int cahceMinutes)
        {
            OutputJson(Response, result, cahceMinutes, string.Empty);
        }

        /// <summary>
        /// output object in json
        /// </summary>
        public static void OutputJson(HttpResponse response, object result, int cahceMinutes, string jsonp)
        {
            bool isJsonp = StringUtil.HasText(jsonp);

            response.ContentType = isJsonp ? "text/plain" : "text/html";

            if (!isJsonp)
                ServerUtil.AddCache(response, cahceMinutes);
            else
                ServerUtil.AddCache(response, -1);

            string r = new Kiss.Json.JavaScriptSerializer().Serialize(result);

            if (isJsonp)
                r = string.Format("{0}({1})", jsonp, r);

            response.Write(r);
        }        
    }
}
