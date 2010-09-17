using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using Kiss.Utils;
using Kiss.Web.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 该控件将输出合并的js
    /// </summary>
    [PersistChildren( true ), ParseChildren( false )]
    public class Scripts : Control
    {
        const string ScriptKey = "__scripts__";

        /// <summary>
        /// 重载Control的Render方法
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render( HtmlTextWriter writer )
        {
            RenderScripts( writer );

            base.Render( writer );
        }

        /// <summary>
        /// 输出脚本
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void RenderScripts( HtmlTextWriter writer )
        {
            string jsversion = JContext.Current.Site.JsVersion;

            List<string> urls = new List<string>();
            List<string> blocks = new List<string>();

            Queue queue = Context.Items[ ScriptKey ] as Queue;

            if( queue != null && queue.Count > 0 )
            {
                IEnumerator ie = queue.GetEnumerator();
                while( ie.MoveNext() )
                {
                    ScriptQueueItem si = ( ScriptQueueItem )ie.Current;

                    if( si.IsScriptBlock )
                        blocks.Add( si.Script );
                    else
                        urls.Add( si.Script );
                }
            }

            if( urls.Count > 0 )
            {
                writer.Write(
                    string.Format( "<script src='{0}' type='text/javascript'></script>",
                        Utility.FormatJsUrl( string.Format( "rescombiner.axd?f={0}&t=text/javascript&v={1}",
                                                            ServerUtil.UrlEncode( StringUtil.CollectionToCommaDelimitedString( urls ) ),
                                                            jsversion ) ) ) );
            }

            if( blocks.Count > 0 )
            {
                string blockCacheKey = GetBlockScriptCacheKey( jsversion );

                string block =  Context.Cache.Get( blockCacheKey ) as string;
                if( StringUtil.IsNullOrEmpty( block ) )
                {
                    block = string.Format( "<script type='text/javascript'>{0}</script>", new JavaScriptMinifier().MinifyString( StringUtil.CollectionToDelimitedString( blocks, "", string.Empty ) ) );
                    Context.Cache.Insert( blockCacheKey, block, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays( 60 ) );
                }
                writer.Write( block );
            }
        }

        /// <summary>
        /// 添加脚本链接
        /// </summary>
        /// <param name="url"></param>
        public static void AddRes( string url )
        {
            AddScript( url, false, HttpContext.Current );
        }

        /// <summary>
        /// 添加脚本块
        /// </summary>
        /// <param name="script"></param>
        public static void AddBlock( string script )
        {
            AddScript( script, true, HttpContext.Current );
        }

        /// <summary>
        /// 去除当前页面的脚本块缓存
        /// </summary>
        public static void ClearCache()
        {
            HttpContext.Current.Cache.Remove( GetBlockScriptCacheKey( JContext.Current.Site.JsVersion ) );
        }

        private static void AddScript( string script, bool isblock, HttpContext context )
        {
            Queue scriptQueue = context.Items[ ScriptKey ] as Queue;
            if( scriptQueue == null )
            {
                scriptQueue = new Queue();
                context.Items[ ScriptKey ] = scriptQueue;
            }

            scriptQueue.Enqueue( new ScriptQueueItem( script, isblock ) );
        }

        private static string GetBlockScriptCacheKey( string jsversion )
        {
            return string.Format( "Kiss.web.controls.scripts.block.{0}.{1}",
                HttpContext.Current.Request.RawUrl,
                jsversion );
        }

        internal class ScriptQueueItem
        {
            public bool IsScriptBlock { get; set; }
            public string Script { get; set; }

            public ScriptQueueItem( string script, bool isblock )
            {
                Script = script;
                IsScriptBlock = isblock;
            }
        }
    }
}
