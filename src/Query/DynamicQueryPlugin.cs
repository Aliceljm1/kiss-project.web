using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Kiss.Query;
using Kiss.Utils;

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

                    foreach (ISite site in ServiceLocator.Instance.Resolve<IHost>().AllSites)
                    {
                        string dir;
                        string filename;
                        List<string> files = new List<string>();

                        if (site.SiteKey == SiteConfig.Instance.SiteKey)// default site
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

                                id = string.Format(FORMAT, site.SiteKey, id.ToLower());

                                _logger.Debug("RESULT: query id:{0}", id);

                                qc_dict[id] = new Qc()
                                {
                                    Id = id,
                                    Field = XmlUtil.GetStringAttribute(node, "field", string.Empty),
                                    AllowedOrderbyColumns = XmlUtil.GetStringAttribute(node, "allowedOrderbyColumns", string.Empty),
                                    Orderby = XmlUtil.GetStringAttribute(node, "orderby", string.Empty),
                                    Where = node.InnerText,
                                    PageSize = XmlUtil.GetIntAttribute(node, "pageSize", -1)
                                };

                                foreach (XmlAttribute attr in node.Attributes)
                                {
                                    qc_dict[id][attr.Name] = attr.Value;
                                }
                            }

                            _logger.Debug("end parse query file: {0}.", item);
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
            if (qId == null)
                qId = jc.Navigation.ToString();

            if (string.IsNullOrEmpty(qId))
                return;

            qc = GetById(jc.Site, string.Format("{0}.{1}.{2}", qId, e.Method, e.DbProviderName));

            if (qc == null)
                qc = GetById(jc.Site, string.Format("{0}.{1}", qId, e.Method));

            if (qc == null)
                qc = GetById(jc.Site, string.Format("{0}.{1}", qId, e.DbProviderName));

            if (qc == null)
                qc = GetById(jc.Site, qId);

            if (qc == null)
                return;

            if (qc.PageSize > -1 && q.PageSize == -1)
                q.PageSize = qc.PageSize;

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

            if (StringUtil.HasText(qc.AllowedOrderbyColumns))
                q.AllowedOrderbyColumns.AddRange(StringUtil.CommaDelimitedListToStringArray(qc.AllowedOrderbyColumns));

            if (StringUtil.HasText(qc.Orderby))
            {
                foreach (string oderby in StringUtil.CommaDelimitedListToStringArray(qc.Orderby))
                {
                    q.AddOrderby(oderby.TrimStart('-'), !oderby.StartsWith("-"));
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

                q.Parameters.Clear();

                Match m = Regex.Match(sql, @"@\w+");
                while (m.Success)
                {
                    string param_name = m.Value.Substring(1).Trim();

                    q.Parameters[param_name] = q[param_name];
                    m = m.NextMatch();
                }

                if (StringUtil.HasText(sql))
                    q.WhereClause = sql;
            }
        }

        Qc GetById(ISite site, string id)
        {
            if (HttpContext.Current.Cache[kCACHE_KEY] == null)
                Refresh();

            string key = string.Format(FORMAT, site.SiteKey, id.ToLower());

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
