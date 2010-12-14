using System;
using System.Collections.Generic;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 导航面包屑
    /// </summary>
    public class Crumb : TemplatedControl
    {
        /// <summary>
        /// load menu
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            NavigationInfo nav = JContext.Current.Navigation;
            if (!nav.OK) return;

            List<NavigationItem> items = Menu.GetDataSource(JContext.Current.Site, Menu.MenuType.Cascade, string.Empty);

            NavigationItem item1 = items[nav.Index];

            JContext.Current.Crumbs.Insert(0, new NavigationItem() { Title = item1.Title, Url = item1.Url });

            if (nav.SubIndex > -1)
            {
                NavigationItem item2 = item1.SubItems[nav.SubIndex];

                JContext.Current.Crumbs.Insert(1, new NavigationItem() { Title = item2.Title, Url = item2.Url });
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!JContext.Current.Navigation.OK) return;
            JContext.Current.Crumbs[JContext.Current.Crumbs.Count - 1].IsLast = true;
        }
    }
}
