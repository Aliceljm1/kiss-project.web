﻿#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-08
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-08		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.Text;
using System.Web;
using Kiss.Utils;
using Newtonsoft.Json;

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
        /// <param name="Response"></param>
        /// <param name="result"></param>
        /// <param name="cahceMinutes"></param>
        /// <param name="jsonp"></param>
        public static void OutputJson(HttpResponse Response, object result, int cahceMinutes, string jsonp)
        {
            bool isJsonp = StringUtil.HasText(jsonp);

            Response.ContentType = isJsonp ? "text/plain" : "text/html";

            if (!isJsonp)
                ServerUtil.AddCache(cahceMinutes);
            else
                ServerUtil.AddCache(-1);

            string r;

            if (result is string)
                r = JavaScriptConvert.ToString(result);
            else if (result is bool)
                r = result.ToString().ToLower();
            else if (result == null)
                r = JavaScriptConvert.Null;
            else if (result is int)
                r = JavaScriptConvert.ToString(result);
            else
                r = JavaScriptConvert.SerializeObject(result);

            if (isJsonp)
                r = string.Format("{0}({1})", jsonp, r);

            //if (RequestUtil.SupportGZip())
            //{
            //    Response.AppendHeader("Content-Encoding", "gzip");
            //    byte[] buffer = GZipUtil.GZipMemory(r, Encoding.UTF8);
            //    Response.BinaryWrite(buffer);
            //}
            //else
            //{
                Response.Write(r);
           // }

            Response.End();
        }
    }
}
