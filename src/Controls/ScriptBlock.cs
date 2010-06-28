using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// This control contains optional javascript for the page
    /// </summary>
    public class ScriptBlock : PlaceHolder
    {
        private string _script;
        protected override void AddParsedSubObject( object obj )
        {
            if( JContext.Current.Site.CombinJs )
            {
                LiteralControl lit = obj as LiteralControl;
                _script = lit.Text;

                _script = _script.Replace( "<script type=\"text/javascript\">", string.Empty );
                _script = _script.Replace( "</script>", string.Empty );
            }
            else
                base.AddParsedSubObject( obj );
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if( StringUtil.HasText( _script ) )
                Scripts.AddBlock( _script );
        }
    }
}
