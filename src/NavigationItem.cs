using System;
using System.Collections.Generic;

namespace Kiss.Web
{
    public class NavigationItem : ExtendedAttributes, ICloneable
    {
        #region fields / props

        public string Name { get; set; }

        public string Title { get; set; }

        public bool Selected { get; set; }

        public string Url { get; set; }

        public List<NavigationItem> SubItems { get; set; }

        public bool HasChild
        {
            get
            {
                return SubItems != null && SubItems.Count > 0;
            }
        }

        public bool IsSeparator
        {
            get { return string.Equals(Name, "separator", StringComparison.InvariantCultureIgnoreCase); }
        }

        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }

        public Dictionary<int, NavigationItem> Children { get; set; }

        #endregion

        #region ctor

        public NavigationItem()
        {
        }

        public NavigationItem(string displayName, string url)
        {
            Title = displayName;
            Url = url;
        }

        public NavigationItem(string name, string displayName, bool selected)
        {
            Name = name;
            Title = displayName;
            Selected = selected;
        }

        public NavigationItem(string name, string displayName, bool selected,string url)
            : this(name, displayName, selected)
        {
            Url = url;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            NavigationItem item = new NavigationItem();
            item.Title = this.Title;
            item.Url = this.Url;
            item.Name = this.Name;
            item.SetSerializerData(this.GetSerializerData());

            return item;
        }

        #endregion
    }
}
