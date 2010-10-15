using System;
using System.Collections.Generic;
using System.Linq;

namespace Kiss.Web
{
    [Serializable]
    [OriginalName("gSite")]
    public class Site : QueryObject<Site, int>
    {
        [PK]
        override public int Id { get { return base.Id; } set { base.Id = value; } }

        public string SiteKey { get; set; }

        /// <summary>
        /// The domain name plus and any port information, e.g. yourdomain.com:80
        /// </summary>
        public string Authority { get; set; }

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
