using Kiss.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 该控件将输出合并的js
    /// </summary>
    [PersistChildren(true), ParseChildren(false), NonVisualControl(true)]
    class Scripts : Control, IContextAwaredControl
    {
        const string ScriptKey = "__scripts__";

        private IArea _site;
        public IArea CurrentSite { get { return _site ?? JContext.Current.Area; } set { _site = value; } }

        /// <summary>
        /// 重载Control的Render方法
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            RenderScripts(writer);

            writer.Write("$!jc.render_lazy_include()");
        }

        /// <summary>
        /// 输出脚本
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void RenderScripts(HtmlTextWriter writer)
        {
            List<string> urls = new List<string>();
            List<string> blocks = new List<string>();
            List<string> combineUrls = new List<string>();

            Queue queue = Context.Items[ScriptKey] as Queue;

            if (queue != null && queue.Count > 0)
            {
                IEnumerator ie = queue.GetEnumerator();
                while (ie.MoveNext())
                {
                    ScriptQueueItem si = (ScriptQueueItem)ie.Current;

                    if (si.IsScriptBlock)
                        blocks.Add(si.Script);
                    else if (si.IsCombine)
                        combineUrls.Add(si.Script);
                    else
                        urls.Add(si.Script);
                }
            }

            // no combined url
            foreach (var url in urls)
            {
                writer.Write("<script src='{0}' type='text/javascript'></script>", url);
            }

            Dictionary<string, List<string>> di = new Dictionary<string, List<string>>();

            // combined 
            if (combineUrls.Count > 0)
            {
                foreach (string str in combineUrls)
                {
                    string url = str.Replace(AreaConfig.Instance.VirtualPath, "/").TrimStart('/');

                    string path = "/";

                    int index = url.IndexOf("/");

                    if (index != -1)
                        path = url.Substring(0, index + 1);

                    if (!di.ContainsKey(path))
                        di[path] = new List<string>();

                    di[path].Add(url);
                }

                int ps = 6;

                foreach (var item in di)
                {
                    for (int i = 0; i < (int)Math.Ceiling(item.Value.Count * 1.0 / ps); i++)
                    {
                        List<string> list = new List<string>();

                        for (int j = 0; j < ps; j++)
                        {
                            int index = i * ps + j;

                            list.Add(item.Value[index]);

                            if (index == item.Value.Count - 1)
                                break;
                        }

                        writer.Write(string.Format("<script src='{0}' type='text/javascript'></script>",
                            Utility.FormatJsUrl(AreaConfig.Instance, string.Format("{2}_resc.aspx?f={0}&t=text/javascript&v={1}",
                                                                ServerUtil.UrlEncode(StringUtil.CollectionToCommaDelimitedString(list)),
                                                                AreaConfig.Instance.JsVersion,
                                                                StringUtil.CombinUrl(AreaConfig.Instance.VirtualPath, item.Key)))));
                    }
                }
            }

            // script blocks
            if (blocks.Count > 0)
            {
                writer.Write("<script type='text/javascript'>{0}</script>", StringUtil.CollectionToDelimitedString(blocks, " ", string.Empty));
            }
        }

        /// <summary>
        /// 添加脚本链接
        /// </summary>
        /// <param name="url"></param>
        public static void AddRes(string url, bool isCombine)
        {
            AddScript(url, false, isCombine, HttpContext.Current);
        }

        /// <summary>
        /// 添加脚本块
        /// </summary>
        /// <param name="script"></param>
        public static void AddBlock(string script)
        {
            AddScript(script, true, false, HttpContext.Current);
        }

        private static void AddScript(string script, bool isblock, bool isCombine, HttpContext context)
        {
            Queue scriptQueue = context.Items[ScriptKey] as Queue;
            if (scriptQueue == null)
            {
                scriptQueue = new Queue();
                context.Items[ScriptKey] = scriptQueue;
            }

            scriptQueue.Enqueue(new ScriptQueueItem(script, isblock, isCombine));
        }

        internal class ScriptQueueItem
        {
            public bool IsCombine { get; set; }
            public bool IsScriptBlock { get; set; }
            public string Script { get; set; }

            public ScriptQueueItem(string script, bool isblock, bool isCombine)
            {
                Script = script;
                IsScriptBlock = isblock;
                IsCombine = isCombine;
            }
        }
    }
}
