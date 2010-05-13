using System;

namespace Kiss.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class AsyncAttribute : Attribute
    {
    }
}
