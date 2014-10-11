using System;
using System.Collections.Generic;

namespace Kiss.Web.Ajax
{
    public class AjaxClass : ICloneable
    {
        public string Id { get; set; }

        public string TypeString { get; set; }

        public Type Type { get; set; }

        private List<AjaxMethod> _methods = new List<AjaxMethod>();
        public List<AjaxMethod> Methods
        {
            get { return _methods; }
            set { _methods = value; }
        }

        public string Key
        {
            get
            {
                if (Type != null)
                    return Type.FullName;

                return TypeString;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            AjaxClass c = new AjaxClass();
            c.Id = this.Id;
            c.TypeString = this.TypeString;

            return c;
        }

        #endregion
    }
}
