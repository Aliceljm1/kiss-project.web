using Kiss.Query;
using Kiss.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Xml;

namespace Kiss.Web.Query
{
    public class DynamicQueryPlugin
    {
        Dictionary<string, Qc> qc_dict = new Dictionary<string, Qc>();
        const string kCACHE_KEY = "__DynamicQueryPlugin_cache_key__";

        const string FORMAT = "{0}.{1}";
        private static readonly ILogger _logger = LogManager.GetLogger<DynamicQueryPlugin>();
        private static readonly object _synclock = new object();

        public void Start()
        {
            QueryCondition.BeforeQuery += QueryCondition_BeforeQuery;
        }

        private void Refresh()
        {
            lock (_synclock)
            {
                if (HttpContext.Current.Cache[kCACHE_KEY] == null)
                {
                    qc_dict.Clear();

                    List<string> filenames = new List<string>();

                    foreach (IArea site in ServiceLocator.Instance.Resolve<IHost>().AllAreas)
                    {
                        string dir;
                        string filename;
                        List<string> files = new List<string>();

                        if (site.AreaKey == AreaConfig.Instance.AreaKey)// default site
                            dir = ServerUtil.MapPath("~/App_Data");
                        else
                            dir = ServerUtil.MapPath(site.VirtualPath);

                        filename = Path.Combine(dir, "query.config");

                        if (!Directory.Exists(dir))
                            continue;

                        if (File.Exists(filename))
                            files.Add(filename);

                        // add filename like "query.post.config"
                        files.AddRange(Directory.GetFiles(dir, "query.*.config", SearchOption.TopDirectoryOnly));

                        filenames.AddRange(files);

                        // prase xml
                        foreach (var item in files)
                        {
                            _logger.Debug("begin parse query file: {0}.", item);

                            XmlDocument doc = new XmlDocument();
                            doc.Load(item);

                            foreach (XmlNode node in doc.DocumentElement.SelectNodes("//query"))
                            {
                                string id = XmlUtil.GetStringAttribute(node, "id", string.Empty);
                                if (string.IsNullOrEmpty(id))
                                    continue;

                                id = string.Format(FORMAT, site.AreaKey, id.ToLower());

                                _logger.Debug("RESULT: query id:{0}", id);

                                Qc qc = new Qc()
                                {
                                    Id = id,
                                    Field = XmlUtil.GetStringAttribute(node, "field", string.Empty),
                                    AllowedOrderbyColumns = XmlUtil.GetStringAttribute(node, "allowedOrderbyColumns", string.Empty),
                                    Orderby = XmlUtil.GetStringAttribute(node, "orderby", string.Empty),
                                    Where = node.InnerText.Trim(),
                                    PageSize = XmlUtil.GetIntAttribute(node, "pageSize", -1)
                                };

                                if (string.IsNullOrEmpty(qc.Field))
                                {
                                    // 如果field是设置在单独的节点
                                    XmlNode n = node.SelectSingleNode("field");
                                    if (n != null)
                                    {
                                        qc.Field = n.InnerText.Trim();

                                        n = node.SelectSingleNode("where");
                                        if (n != null)
                                            qc.Where = n.InnerText.Trim();
                                    }
                                }

                                qc_dict[id] = qc;

                                foreach (XmlAttribute attr in node.Attributes)
                                {
                                    qc_dict[id][attr.Name] = attr.Value;
                                }
                            }

                            _logger.Debug("end parse query file: {0}.", item);
                        }
                    }

                    // load from database
                    foreach (var item in (from q in DictSchema.CreateContext(true)
                                          where q.Type == "dynamic_query" && q.IsValid == true
                                          select q).ToList())
                    {
                        if (string.IsNullOrEmpty(item.Name)) continue;

                        string id = string.Format(FORMAT,
                            string.IsNullOrEmpty(item.ParentId) ? "default" : item.ParentId,
                            item.Name.ToLowerInvariant());

                        var qc = qc_dict[id] = new Qc()
                        {
                            Id = id,
                            Field = item.Prop1,
                            AllowedOrderbyColumns = item.Prop2,
                            Orderby = item.Prop3,
                            Where = item.Prop4,
                            PageSize = item.Prop5.ToInt(-1)
                        };

                        foreach (string key in item.ExtAttrs.Keys)
                        {
                            qc[key] = item[key];
                        }
                    }

                    CacheDependency fileDependency = new CacheDependency(filenames.ToArray());
                    HttpContext.Current.Cache.Insert(kCACHE_KEY, "dummyValue", fileDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                }
            }
        }

        void QueryCondition_BeforeQuery(object sender, QueryCondition.BeforeQueryEventArgs e)
        {
            QueryCondition q = sender as QueryCondition;

            JContext jc = JContext.Current;

            Qc qc = null;

            string qId = q.Id;
            string sitekey = jc.Area.AreaKey;

            if (qId == null)
                qId = jc.Navigation.ToString();
            else
            {
                if (qId.Contains(":"))
                {
                    string[] ar = StringUtil.Split(qId, ":", true, true);
                    sitekey = ar[0];
                    qId = ar[1];
                }
            }

            if (string.IsNullOrEmpty(qId))
                return;

            qc = GetById(sitekey, string.Format("{0}.{1}.{2}", qId, e.Method, e.DbProviderName));

            if (qc == null)
                qc = GetById(sitekey, string.Format("{0}.{1}", qId, e.Method));

            if (qc == null)
                qc = GetById(sitekey, string.Format("{0}.{1}", qId, e.DbProviderName));

            if (qc == null)
                qc = GetById(sitekey, qId);

            if (qc == null)
            {
                _logger.Warn("query:{0} not found!", q.Id);
                return;
            }

            if (qc.PageSize > -1 && q.PageSize == -1)
                q.PageSize = qc.PageSize;

            q.Parameters.Clear();

            if ((string.IsNullOrEmpty(q.TableField) || q.TableField == "*" || q.EventFiredTimes > 1) && StringUtil.HasText(qc.Field))
            {
                if (qc.Field.Contains("$"))
                {
                    using (StringWriter writer = new StringWriter())
                    {
                        Dictionary<string, object> di = new Dictionary<string, object>(jc.ViewData);
                        di["this"] = sender;

                        ServiceLocator.Instance.Resolve<ITemplateEngine>().Process(di,
                                   string.Empty,
                                   writer,
                                   qc.Field);

                        q.TableField = writer.GetStringBuilder().ToString();
                    }
                }
                else
                {
                    q.TableField = qc.Field;
                }
            }

            // 解析field里的@参数
            Match m = Regex.Match(q.TableField, @"@\w+");
            while (m.Success)
            {
                string param_name = m.Value.Substring(1).Trim();

                if (q[param_name] != null)
                    q.Parameters[param_name] = q[param_name];
                m = m.NextMatch();
            }

            if (StringUtil.HasText(qc.AllowedOrderbyColumns))
                q.AllowedOrderbyColumns.AddRange(StringUtil.CommaDelimitedListToStringArray(qc.AllowedOrderbyColumns));

            if (StringUtil.HasText(qc.Orderby))
            {
                List<string> ls = new List<string>(StringUtil.CommaDelimitedListToStringArray(qc.Orderby));

                if (q.IsAddOrderBy2First)
                {
                    ls.Reverse();

                    foreach (string oderby in ls)
                    {
                        q.InsertOrderby(0, oderby.TrimStart('-'), !oderby.StartsWith("-"));
                    }
                }
                else
                {
                    foreach (string oderby in ls)
                    {
                        q.AddOrderby(oderby.TrimStart('-'), !oderby.StartsWith("-"));
                    }
                }
            }

            foreach (string key in qc.Keys)
            {
                q[key] = qc[key];
            }

            using (StringWriter writer = new StringWriter())
            {
                Dictionary<string, object> di = new Dictionary<string, object>(jc.ViewData);
                di["this"] = sender;

                ServiceLocator.Instance.Resolve<ITemplateEngine>().Process(di,
                           string.Empty,
                           writer,
                           qc.Where);

                string sql = Regex.Replace(writer.GetStringBuilder().ToString(), @"\s{1,}|\t|\r|\n", " ");

                // 解析where里的@参数
                m = Regex.Match(sql, @"@\w+");
                while (m.Success)
                {
                    string param_name = m.Value.Substring(1).Trim();

                    if (q[param_name] != null)
                        q.Parameters[param_name] = q[param_name];

                    m = m.NextMatch();
                }

                if (StringUtil.HasText(sql))
                    q.WhereClause = sql;
            }
        }

        Qc GetById(string siteKey, string id)
        {
            if (HttpContext.Current.Cache[kCACHE_KEY] == null)
                Refresh();

            string key = string.Format(FORMAT, siteKey, id.ToLower());

            if (qc_dict.ContainsKey(key))
                return qc_dict[key];

            return null;
        }
    }

    class Qc : ExtendedAttributes
    {
        public string Id { get; set; }
        public string Field { get; set; }
        public string AllowedOrderbyColumns { get; set; }
        public string Where { get; set; }
        public int PageSize { get; set; }
        public string Orderby { get; set; }
    }
}
