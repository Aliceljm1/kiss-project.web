using System;
using System.Collections.Generic;
using System.Linq;

namespace Kiss.Web
{
    [Serializable]
    [OriginalName("gMenuItem")]
    public class MenuItem : QueryObject<MenuItem, int>
    {
        [PK]
        override public int Id { get { return base.Id; } set { base.Id = value; } }

        public string SiteKey { get; set; }

        public int MenuId { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Icon { get; set; }

        public int Index { get; set; }
        public int SubIndex { get; set; }

        public string ControllerId { get; set; }

        public bool Visiable { get; set; }

        private Menu _menu;
        [Ignore]
        public Menu Menu
        {
            get
            {
                if (_menu == null)
                    _menu = Menu.Get(MenuId);
                return _menu;
            }
        }

        [Ignore]
        public MenuItem Parent { get; set; }

        public override string ToString()
        {
            return Title;
        }

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

        #region api

        public List<MenuItem> GetsAll(string siteKey)
        {
            return (from q in Query
                    where q.SiteKey == siteKey
                    orderby q.MenuId ascending, q.Index ascending, q.SubIndex ascending
                    select q).ToList();
        }

        public static List<MenuItem> GetsAll()
        {
            return (from q in Query
                    orderby q.SiteKey, q.MenuId ascending, q.Index ascending, q.SubIndex ascending
                    select q).ToList();
        }

        public static List<MenuItem> GetsByMenuId(int menuId)
        {
            List<MenuItem> list = (from q in Query
                                   where q.MenuId == menuId
                                   orderby q.Index, q.SubIndex
                                   select q).ToList();

            foreach (var item in list)
            {
                if (item.SubIndex > -1)
                {
                    item.Parent = list.Find(delegate(MenuItem mi) { return mi.Index == item.Index && mi.SubIndex == -1; });
                }
            }

            return list;
        }

        public static List<MenuItem> GetsTopByMenuId(int menuId)
        {
            return (from mi in Query
                    where mi.MenuId == menuId && mi.SubIndex == -1
                    orderby mi.Index
                    select mi).ToList();
        }

        public static List<MenuItem> GetsSubByMenuId(MenuItem menuItem)
        {
            return (from mi in Query
                    where mi.MenuId == menuItem.MenuId && mi.Index == menuItem.Index && mi.SubIndex > -1
                    orderby mi.SubIndex
                    select mi).ToList();
        }

        #endregion
    }
}
