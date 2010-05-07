#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-08
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-08		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.ComponentModel;
using System.Web.UI;

namespace Kiss.Web.Controls
{
    [PersistChildren ( false ) , ParseChildren ( true )]
    public class LoginView : Control
    {
        #region props

        [
        Browsable ( false ) ,
        DefaultValue ( null ) ,
        Description ( "匿名用户的模板" ) ,
        PersistenceMode ( PersistenceMode.InnerProperty )
        ]
        public ITemplate AnonymousTemplate { get; set; }

        [
        Browsable ( false ) ,
        DefaultValue ( null ) ,
        Description ( "登录用户的模板" ) ,
        PersistenceMode ( PersistenceMode.InnerProperty )
        ]
        public ITemplate AuthedTemplate { get; set; }

        #endregion

        protected override void CreateChildControls ( )
        {
            base.CreateChildControls ( );

            Control container = new Control ( );
            if ( JContext.Current.IsAuth && AuthedTemplate != null )
            {
                AuthedTemplate.InstantiateIn ( container );
                Controls.Add ( container );
            }
            else if ( !JContext.Current.IsAuth && AnonymousTemplate != null )
            {
                AnonymousTemplate.InstantiateIn ( container );
                Controls.Add ( container );
            }
            else
            {
                Visible = false;
            }

            ChildControlsCreated = true;
        }
    }
}
