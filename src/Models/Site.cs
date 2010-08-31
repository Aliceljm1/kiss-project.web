using System;
using System.Collections.Generic;
using System.Linq;
using Kiss.Web;

namespace Kiss.Web
{
    [Serializable]
    [OriginalName("gSite")]
    public class Site : QueryObject<Site, int>, ISite
    {
        [PK]
        override public int Id { get { return base.Id; } set { base.Id = value; } }

        public string SiteKey { get; set; }

        /// <summary>
        /// The domain name plus and any port information, e.g. jiliKiss.com:80
        /// </summary>
        public string Authority { get; set; }

        public string Title { get; set; }

        public bool CombinCss { get; set; }

        public bool CombinJs { get; set; }

        public string CssHost { get; set; }

        public string ThemeRoot { get; set; }

        public string CssRoot { get; set; }

        public string CssVersion { get; set; }

        public string DefaultTheme { get; set; }

        public string FavIcon { get; set; }

        public string Host { get; set; }

        public string JsHost { get; set; }

        public string JsVersion { get; set; }

        public string RawAdditionalHeader { get; set; }

        public string VirtualPath { get; set; }

        public int MenuId { get; set; }

        public string Category { get; set; }

        public string Footer { get; set; }

        public string ErrorPage { get; set; }

        public string UserKey { get; set; }
        public string DeptKey { get; set; }
        public string RoleKey { get; set; }

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
            return Title;
        }

        public List<DictSchema> GetSchema(string type)
        {
            return DictSchema.GetsByType(Id, type);
        }

        public DictSchema GetSchema(string type, string name)
        {
            return DictSchema.GetByName(Id, type, name);
        }

        #region method

        public static Site GetByAuthority(string authority)
        {
            return (from s in Query
                    where s.Authority == authority
                    select s).FirstOrDefault();
        }

        public static List<Site> GetsByAuthority(string authority)
        {
            return (from s in Query
                    where s.Authority == authority || s.Authority == "*"
                    orderby s.Authority
                    select s).ToList();
        }

        public static List<Site> FindAll(string siteKey)
        {
            return (from q in Query
                    where q.SiteKey == siteKey
                    select q).ToList();
        }

        #endregion
    }
}
