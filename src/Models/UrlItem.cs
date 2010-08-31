using System;
using System.Collections.Generic;
using System.Linq;

namespace Kiss.Web
{
    [Serializable]
    [OriginalName("gUrlItem")]
    public class UrlItem : QueryObject<UrlItem, int>
    {
        [PK]
        override public int Id { get { return base.Id; } set { base.Id = value; } }

        public string SiteKey { get; set; }

        public int MenuId { get; set; }

        public int MenuItemId { get; set; }

        public string Name { get; set; }
        public string Template { get; set; }
        public string Href { get; set; }

        public string ControllerId { get; set; }
        public string Action { get; set; }

        public string Title { get; set; }

        public int SortOrder { get; set; }

        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

        private ExtendedAttributes _extAttrs;
        [Ignore]
        public ExtendedAttributes ExtAttrs
        {
            get
            {
                if (_extAttrs == null)
                {
                    _extAttrs = new ExtendedAttributes();
                    _extAttrs.SetData(PropertyName, PropertyValue);
                }
                return _extAttrs;
            }
        }

        [Ignore]
        public string this[string key]
        {
            get
            {
                if (ExtAttrs.ExtendedAttributesCount == 0)
                    ExtAttrs.SetData(PropertyName, PropertyValue);
                return ExtAttrs.GetExtendedAttribute(key);
            }
            set
            {
                ExtAttrs.SetExtendedAttribute(key, value);
            }
        }

        public void SerializeExtAttrs()
        {
            SerializerData sd = ExtAttrs.GetSerializerData();

            PropertyName = sd.Keys;
            PropertyValue = sd.Values;
        }

        public override string ToString()
        {
            return Template;
        }

        #region api

        public static List<UrlItem> GetsAll(string siteKey)
        {
            return (from q in Query
                    where q.SiteKey == siteKey
                    orderby q.MenuId ascending, q.MenuItemId ascending, q.SortOrder ascending
                    select q).ToList();
        }

        public static List<UrlItem> GetsAll()
        {
            return (from q in Query
                    orderby q.SiteKey, q.MenuId ascending, q.MenuItemId ascending, q.SortOrder ascending
                    select q).ToList();
        }

        public static List<UrlItem> GetsByMenuItem(MenuItem mi)
        {
            return (from ui in Query
                    where ui.MenuId == mi.MenuId && ui.MenuItemId == mi.Id
                    orderby ui.SortOrder ascending
                    select ui).ToList();
        }

        public static List<UrlItem> GetsByMenuId(int menuId)
        {
            return (from ui in Query
                    where ui.MenuId == menuId
                    select ui).ToList();
        }

        public static UrlItem GetByAction(string controller, string action)
        {
            return (from q in Query
                    where q.ControllerId == controller && q.Action == action
                    select q).FirstOrDefault();
        }

        public static List<UrlItem> GetsNoParent(int menuId)
        {
            return (from q in Query
                    where q.MenuId == menuId && q.MenuItemId == 0
                    orderby q.SortOrder ascending
                    select q).ToList();
        }

        #endregion
    }
}
