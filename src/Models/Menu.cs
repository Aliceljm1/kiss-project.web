using System;
using System.Collections.Generic;
using System.Linq;

namespace Kiss.Web
{
    [Serializable]
    [OriginalName("gMenu")]
    public class Menu : QueryObject<Menu, int>
    {
        [PK]
        override public int Id { get { return base.Id; } set { base.Id = value; } }

        public string SiteKey { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }

        #region api

        public static List<Menu> GetsAll(string siteKey)
        {
            return (from q in Query
                    where q.SiteKey == siteKey
                    select q).ToList();
        }

        public static List<Menu> GetsAll()
        {
            return (from q in Query
                    orderby q.SiteKey
                    select q).ToList();
        }

        public static List<Menu> FindAll(string sitekey)
        {
            return (from q in Query
                    where q.SiteKey == sitekey
                    select q).ToList();
        }

        public static int Copy(string menuTitle, int oldMenuId, string siteKey)
        {
            // create menu
            Menu menu = new Menu();
            menu.Title = menuTitle;
            menu.SiteKey = siteKey;
            Query.Add(menu);
            Query.SubmitChanges();

            Dictionary<int, MenuItem> cache = new Dictionary<int, MenuItem>();

            // copy menuitems
            List<MenuItem> menuItems = new List<MenuItem>();
            foreach (MenuItem mi in MenuItem.GetsByMenuId(oldMenuId))
            {
                cache[mi.Id] = mi;

                mi.Id = 0;
                mi.MenuId = menu.Id;

                menuItems.Add(mi);
            }
            MenuItem.Query.AddRange(menuItems);
            MenuItem.Query.SubmitChanges();

            // copy pages
            List<UrlItem> urls = new List<UrlItem>();
            foreach (UrlItem url in UrlItem.GetsByMenuId(oldMenuId))
            {
                url.Id = 0;
                url.MenuId = menu.Id;
                url.MenuItemId = cache[url.MenuItemId].Id;

                urls.Add(url);
            }
            UrlItem.Query.AddRange(urls);
            UrlItem.Query.SubmitChanges();

            return menu.Id;
        }

        #endregion
    }
}
