using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// 定义了映射关系, 描述了url该怎么样重定向
    /// </summary>
    public class UrlMappingItem : ExtendedAttributes, IComparable<UrlMappingItem>
    {
        #region ctor

        public UrlMappingItem()
        {
        }

        public UrlMappingItem(string name, Regex urlTarget, string redirection)
        {
            Name = name;
            UrlTarget = urlTarget;
            Redirection = redirection;
        }

        #endregion

        #region props

        /// <summary>
        /// 标识对象类型
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 映射名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 映射方式
        /// </summary>
        public Regex UrlTarget { get; set; }

        /// <summary>
        /// 重定向的url
        /// </summary>
        public string Redirection { get; set; }

        public string UrlTemplate { get; set; }

        public int Index { get; set; }

        public int SubIndex { get; set; }

        /// <summary>
        /// 标识自己在菜单中的index
        /// </summary>
        public int SelfIndex { get; set; }

        public string Title { get; set; }

        public string Desc { get; set; }

        public string Action { get; set; }

        #endregion

        #region IComparable<UrlMappingItem> Members

        public int CompareTo(UrlMappingItem other)
        {
            return SelfIndex.CompareTo(other.SelfIndex);
        }

        #endregion
    }

    public class UrlMappingItemCollection : List<UrlMappingItem>
    {
        public void Merge(UrlMappingItem item)
        {
            Merge(new UrlMappingItemCollection() { item });
        }

        public void Merge(UrlMappingItemCollection items)
        {
            UrlMappingItemCollection col = Combin(items, this);
            Clear();
            AddRange(col);
        }

        public static UrlMappingItemCollection Combin(UrlMappingItemCollection first, UrlMappingItemCollection second)
        {
            UrlMappingItemCollection col = new UrlMappingItemCollection();

            // first things first
            foreach (var item in first)
            {
                var configed = second.FindByAction(item.Id, item.Action, item.UrlTemplate);
                if (configed == null)
                    col.Add(item);
                else
                {
                    if (second.Remove(configed))
                        col.Add(configed);
                    else
                        col.Add(item);
                }
            }

            col.AddRange(second);

            return col;
        }

        public UrlMappingItem FindByAction(string controller, string action, string urltemplate)
        {
            return Find(delegate(UrlMappingItem item)
            {
                if (item == null) return false;
                return item.Id == controller && item.Action == action && item.UrlTemplate == urltemplate;
            });
        }
    }
}
