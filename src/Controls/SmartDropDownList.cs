using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Kiss.Web.Controls
{
    public class SmartDropDownList : DropDownList
    {
        /// <summary>
        /// 将控件的内容呈现到指定的编写器中
        /// </summary>
        /// <param name="writer">writer</param>
        protected override void RenderContents ( HtmlTextWriter writer )
        {
            //base.RenderContents(writer);
            OptionGroupRenderContents ( writer );
        }

        /// <summary>
        /// 呈现Option或OptionGroup
        /// </summary>
        /// <param name="writer">writer</param>
        private void OptionGroupRenderContents ( HtmlTextWriter writer )
        {
            foreach ( ListItem li in this.Items )
            {
                // 如果没有optgroup属性则呈现Option
                if ( li.Value != this.OptionGroupValue )
                {
                    // 呈现Option
                    RenderListItem ( li , writer );
                }
                // 如果有optgroup属性则呈现OptionGroup
                else
                {
                    //Ie6不支持
                    //if (writerEndTag)
                    // 呈现OptionGroup的EndTag
                    //    OptionGroupEndTag(writer);
                    //else
                    //    writerEndTag = true;

                    // 呈现OptionGroup的BeginTag
                    OptionGroupBeginTag ( li , writer );
                    OptionGroupEndTag ( writer );//add for Ie6.0
                }
            }
            //Ie6不支持
            //if (writerEndTag)
            // 呈现OptionGroup的EndTag
            //    OptionGroupEndTag(writer);
        }

        /// <summary>
        /// 呈现OptionGroup的BeginTag
        /// </summary>
        /// <param name="li">OptionGroup数据项</param>
        /// <param name="writer">writer</param>
        private void OptionGroupBeginTag ( ListItem li , HtmlTextWriter writer )
        {
            writer.WriteBeginTag ( "optgroup" );

            // 写入OptionGroup的label
            writer.WriteAttribute ( "label" , li.Text );

            foreach ( string key in li.Attributes.Keys )
            {
                // 写入OptionGroup的其它属性
                writer.WriteAttribute ( key , li.Attributes[ key ] );
            }
            writer.Write ( HtmlTextWriter.TagRightChar );
            writer.WriteLine ( );
        }

        /// <summary>
        /// 呈现OptionGroup的EndTag
        /// </summary>
        /// <param name="writer">writer</param>
        private void OptionGroupEndTag ( HtmlTextWriter writer )
        {
            writer.WriteEndTag ( "optgroup" );
            writer.WriteLine ( );
        }

        /// <summary>
        /// 呈现Option
        /// </summary>
        /// <param name="li">Option数据项</param>
        /// <param name="writer">writer</param>
        private void RenderListItem ( ListItem li , HtmlTextWriter writer )
        {
            writer.WriteBeginTag ( "option" );

            // 写入Option的Value
            writer.WriteAttribute ( "value" , li.Value , true );

            if ( li.Selected )
            {
                // 如果该Option被选中则写入selected
                writer.WriteAttribute ( "selected" , "selected" , false );
            }

            foreach ( string key in li.Attributes.Keys )
            {
                // 写入Option的其它属性
                writer.WriteAttribute ( key , li.Attributes[ key ] );
            }

            writer.Write ( HtmlTextWriter.TagRightChar );

            // 写入Option的Text
            HttpUtility.HtmlEncode ( li.Text , writer );

            writer.WriteEndTag ( "option" );
            writer.WriteLine ( );
        }

        /// <summary>
        /// 用于添加SmartDropDownList的分组项的ListItem的Value值
        /// </summary>
        [
        Browsable ( true ) ,
        Description ( "用于添加DropDownList的分组项的ListItem的Value值" ) ,
        Category ( "扩展" )
        ]
        public virtual string OptionGroupValue
        {
            get
            {
                string s = ( string ) ViewState[ "OptionGroupValue" ];

                return ( s == null ) ? "optgroup" : s;
            }
            set
            {
                ViewState[ "OptionGroupValue" ] = value;
            }
        }

    }
}

