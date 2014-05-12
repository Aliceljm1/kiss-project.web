#region File Comment
//+-------------------------------------------------------------------+
//+ FileName: 	    Content.cs
//+ File Created:   20090820
//+-------------------------------------------------------------------+
//+ Purpose:        Content∂®“Â
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 20090820          ZHLI Comment Created
//+-------------------------------------------------------------------+
//+-------------------------------------------------------------------+
#endregion

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// This control contains the content for a particular contentplaceholder
    /// </summary>
    public class Content : PlaceHolder
    {
        internal string _templateSourceDirectory;

        /// <summary>
        /// Overrides <see cref="Control.TemplateSourceDirectory"/>.
        /// </summary>
        public override string TemplateSourceDirectory
        {
            get
            {
                return _templateSourceDirectory;
            }
        }

        public bool Append { get; set; }
    }
}
