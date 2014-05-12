#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-11-11
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-11-11		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;

namespace Kiss.Web.Ajax
{
    /// <summary>
    /// use this attribute to mark controller method a ajax method
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, Inherited = true, AllowMultiple = false )]
    public sealed class AjaxMethodAttribute : Attribute
    {
        public AjaxMethodType Type { get; set; }
        public int CacheMinutes { get; set; }
        public AjaxServerExceptionAction OnExceptionAction { get; set; }
        public string OnExceptionParameter { get; set; }

        public AjaxMethodAttribute()
        {
            CacheMinutes = -1;
            Type = AjaxMethodType.Post;
            OnExceptionAction = AjaxServerExceptionAction.JSEval;
            OnExceptionParameter = "alert('Error！')";
        }
    }

    [Flags]
    public enum AjaxMethodType
    {
        Get = 0,
        Post = 1
    }
}
