#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-11-21
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-11-21		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

namespace Kiss.Web.Ajax
{
    /// <summary>
    /// ajax method's param
    /// </summary>
    public class AjaxParam
    {
        /// <summary>
        /// param name
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// param type.
        /// </summary>
        public string ParamType { get; set; }

        public override string ToString()
        {
            return ParamName;
        }
    }
}
