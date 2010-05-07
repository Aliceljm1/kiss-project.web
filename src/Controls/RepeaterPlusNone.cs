
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 扩展了Repeater, 增加了空数据模板
    /// </summary>
    public class RepeaterPlusNone : Repeater
    {
        #region fields / properties

        [
        Browsable(false),
        DefaultValue(null),
        Description("数据源为空时显示的模板"),
        PersistenceMode(PersistenceMode.InnerProperty),
        ]
        public virtual ITemplate NoneTemplate
        {
            get { return _noneTemplate; }
            set { _noneTemplate = value; }
        }
        private ITemplate _noneTemplate;

        public bool ShowHeaderFooterOnNone { get; set; }

        public string DataKey { get; set; }

        // static
        private static readonly object EventNoneItemsDataBound = new object();

        #endregion

        #region override

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (StringUtil.HasText(DataKey))
            {
                DataSource = JContext.Current.GetViewData(DataKey ?? "list");
                DataBind();
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            if ((Items.Count == 0) && (NoneTemplate != null))
            {
                this.Controls.Clear();

                if (ShowHeaderFooterOnNone && (this.HeaderTemplate != null))
                {
                    RepeaterItem headerItem = this.CreateItem(-1, ListItemType.Header);
                    RepeaterItemEventArgs headerArgs = new RepeaterItemEventArgs(headerItem);
                    this.InitializeItem(headerItem);
                    this.OnItemCreated(headerArgs);
                    this.Controls.Add(headerItem);
                    headerItem.DataBind();
                    this.OnItemDataBound(headerArgs);
                }

                // Process the NoneTemplate
                RepeaterItem noneItem = new RepeaterItem(-1, ListItemType.Item);
                RepeaterItemEventArgs noneArgs = new RepeaterItemEventArgs(noneItem);
                NoneTemplate.InstantiateIn(noneItem);
                this.OnItemCreated(noneArgs);
                this.Controls.Add(noneItem);
                OnNoneItemsDataBound(noneArgs);

                if (ShowHeaderFooterOnNone && (this.FooterTemplate != null))
                {
                    RepeaterItem footerItem = this.CreateItem(-1, ListItemType.Footer);
                    RepeaterItemEventArgs footerArgs = new RepeaterItemEventArgs(footerItem);
                    this.InitializeItem(footerItem);
                    this.OnItemCreated(footerArgs);
                    this.Controls.Add(footerItem);
                    footerItem.DataBind();
                    this.OnItemDataBound(footerArgs);
                }

                this.ChildControlsCreated = true;
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// This event is called when no items were found in the data source.
        /// </summary>
        public event RepeaterItemEventHandler NoneItemsDataBound
        {
            add { base.Events.AddHandler(EventNoneItemsDataBound, value); }
            remove { base.Events.RemoveHandler(EventNoneItemsDataBound, value); }
        }

        /// <summary>
        /// This method is called to invoke the <see cref="NoneItemsDataBound"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> to be passed to the event.</param>
        protected virtual void OnNoneItemsDataBound(RepeaterItemEventArgs e)
        {
            RepeaterItemEventHandler handler = (RepeaterItemEventHandler)base.Events[EventNoneItemsDataBound];
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}
