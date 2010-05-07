#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-10-10
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-10-10		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// use this attribute to mark class as controller
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ControllerAttribute : Attribute
    {
        readonly string urlId;
        readonly Type modelType;

        public ControllerAttribute(string urlId)
        {
            this.urlId = urlId;
        }

        public ControllerAttribute(string urlId, Type modelType)
            : this(urlId)
        {
            this.modelType = modelType;
        }

        public ControllerAttribute(Type modelType)
            : this(modelType.Name, modelType)
        {
        }

        public string UrlId { get { return urlId; } }
        public Type ModelType { get { return modelType; } }

        public Type QcType { get; set; }

        public Type ServiceType { get; set; }
    }
}
