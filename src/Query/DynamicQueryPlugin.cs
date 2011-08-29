using System;
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
        CacheDependency _fileDependency;
        const string FORMAT = "{0}.{1}";

        public void Start()
        {
            Refresh();

            QueryCondition.BeforeQuery += QueryCondition_BeforeQuery;
        }

        private void Refresh()
        {
            qc_dict.Clear();

            List<string> filenames = new List<string>();

            foreach (ISite site in ServiceLocator.Instance.Resolve<IHost>().AllSites)
            {
                string filename;

                if (site.SiteKey == SiteConfig.Instance.SiteKey)// default site
                    filename = ServerUtil.MapPath("~/App_Data");
                else
                    filename = ServerUtil.MapPath(site.VirtualPath);

                filename = Path.Combine(filename, "query.config");

                if (!File.Exists(filename))
                    continue;

                filenames.Add(filename);

                // prase xml
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);

                foreach (XmlNode node in doc.DocumentElement.SelectNodes("//query"))
                {
                    string id = XmlUtil.GetStringAttribute(node, "id", string.Empty);
                    if (string.IsNullOrEmpty(id))
                        continue;

                    id = string.Format(FORMAT, site.SiteKey, id.ToLower());

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
            }

            _fileDependency = new CacheDependency(filenames.ToArray());
            HttpContext.Current.Cache.Insert(kCACHE_KEY, "dummyValue", _fileDependency);
        }

        void QueryCondition_BeforeQuery(object sender, QueryCondition.BeforeQueryEventArgs e)
        {
            QueryCondition q = sender as QueryCondition;

            JContext jc = JContext.Current;

            Qc qc = null;
            if (string.IsNullOrEmpty(q.Id))
            {
                qc = GetById(jc.Site, string.Concat(jc.Navigation.Id, ".", jc.Navigation.Action, ".", e.Method));

                if (qc == null)
                    qc = GetById(jc.Site, string.Concat(jc.Navigation.Id, ".", jc.Navigation.Action));
                else
                    q.EnableFireEventMulti = true;
            }
            else
                qc = GetById(jc.Site, q.Id);

            if (qc == null)
                return;

            if (qc.PageSize > -1 && q.PageSize == -1)
                q.PageSize = qc.PageSize;

            if (StringUtil.HasText(qc.Field))
                q.TableField = qc.Field;

            if (StringUtil.HasText(qc.AllowedOrderbyColumns))
                q.AllowedOrderbyColumns.AddRange(StringUtil.CommaDelimitedListToStringArray(qc.AllowedOrderbyColumns));

            if (StringUtil.HasText(qc.Orderby))
            {
                foreach (string oderby in StringUtil.CommaDelimitedListToStringArray(qc.Orderby))
                {
                    q.AddOrderby(oderby.TrimStart('-'), !oderby.StartsWith("-"));
                }
            }

            q.SetSerializerData(qc.GetSerializerData());

            using (StringWriter writer = new StringWriter())
            {
                Dictionary<string, object> di = new Dictionary<string, object>();
                di.Add("this", sender);
                di.Add("jc", jc);
                di.Add("utils", Utils.Instance);

                ServiceLocator.Instance.Resolve<ITemplateEngine>().Process(di,
                           string.Empty,
                           writer,
                           qc.Where);

                string sql = Regex.Replace(writer.GetStringBuilder().ToString(), @"\s{1,}|\t|\r|\n", " ");

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

    public class Utils
    {
        public static readonly Utils Instance = new Utils();

        public bool hasText(string str)
        {
            return StringUtil.HasText(str);
        }

        public bool isDateTime(string str)
        {
            if (StringUtil.IsNullOrEmpty(str)) return false;

            DateTime dt;

            bool valid = DateTime.TryParse(str, out dt);

            if (valid)
            {
                valid = dt < DateTime.MaxValue && dt > DateTime.MinValue;
            }

            return valid;
        }

        public int toInt(string str)
        {
            return str.ToInt();
        }

        public DateTime toDateTime(string str)
        {
            return DateTime.Parse(str);
        }
    }
}
