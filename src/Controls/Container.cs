using Kiss.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// This control serves two distincts purposes:
    /// - it marks the location where the Master Page will be inserted into the Page
    /// - it contains the various Content sections that will be matched to the Master Page's
    ///   Region controls (based on their ID's).
    /// </summary>
    [ParseChildren(typeof(Content))]
    public class Container : PlaceHolder
    {
        #region props

        private string _themeMasterFile = "master.ascx";

        /// <summary>
        /// Master样式文件名
        /// </summary>
        public string ThemeMasterFile { get { return _themeMasterFile; } set { _themeMasterFile = value; } }

        private ISite _site;
        public ISite CurrentSite { get { if (_site == null)_site = JContext.Current.Site; return _site; } set { _site = value; } }

        /// <summary>
        /// Folder which contains the master files
        /// </summary>        
        private string themeFolder = null;
        private ISite container_site = null;
        public string ThemeFolder
        {
            get
            {
                if (themeFolder == null)
                    return string.Format("{0}/{1}/masters/", StringUtil.CombinUrl(CurrentSite.VirtualPath, CurrentSite.ThemeRoot), ThemeName);

                if (themeFolder.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
                {
                    container_site = SiteConfig.Instance;

                    if (themeFolder != "~")
                        container_site = JContext.Current.Host.GetBySiteKey(themeFolder.Substring(1));

                    if (container_site == null)
                        throw new WebException("site:{0} not exist!", themeFolder.Substring(1));

                    return string.Format("{0}/{1}/masters/", StringUtil.CombinUrl(container_site.VirtualPath, container_site.ThemeRoot), container_site.Theme);
                }

                return themeFolder;
            }
            set
            {
                themeFolder = value;
            }
        }

        /// <summary>
        /// Current user selected theme
        /// </summary>
        protected virtual string ThemeName
        {
            get
            {
                return CurrentSite.Theme;
            }
        }

        protected string ThemePath
        {
            get
            {
                return ThemeFolder + ThemeMasterFile;
            }
        }

        protected string DefaultThemePath
        {
            get
            {
                return string.Format("{0}/default/masters/{1}", StringUtil.CombinUrl(CurrentSite.VirtualPath, CurrentSite.ThemeRoot), ThemeMasterFile);
            }
        }

        private bool ThemeMasterExists
        {
            get
            {
                string filename = Context.Server.MapPath(ThemePath);
                return File.Exists(filename);
            }
        }

        private const string KEY_LastThemeMasterFile = "Kiss.lastThemeMasterFile";
        private string LastThemeMasterFile
        {
            get
            {
                return Context.Items[KEY_LastThemeMasterFile] as string;
            }
            set
            {
                Context.Items[KEY_LastThemeMasterFile] = value;
            }
        }

        private bool DefaultMasterExists
        {
            get
            {
                return File.Exists(Context.Server.MapPath(DefaultThemePath));
            }
        }

        protected List<Content> contents = new List<Content>();

        #endregion

        /// <summary>
        /// add content to content list. this method must called before OnInit method
        /// </summary>
        /// <param name="content"></param>
        public void AddContent(Content content)
        {
            contents.Add(content);
        }

        #region override

        /// <summary>
        /// 重载OnInit方法, 获取样式
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            string masterpagefile = string.Empty;

            if (ThemeMasterExists)
                masterpagefile = ThemePath;
            else if (DefaultMasterExists)
                masterpagefile = DefaultThemePath;
            else
                throw new KissException("The ThemeMasterFile {0} could not be found in the {1} or default theme directory", ThemeMasterFile, ThemeName);

            LoadMasterPage(masterpagefile);

            base.OnInit(e);
        }

        /// <summary>
        /// Only allows <see cref="Content"/> controls to be added.
        /// </summary>
        protected override void AddParsedSubObject(object obj)
        {
            if (obj is Content)
            {
                contents.Add(obj as Content);
            }
            else
            {
                throw new Exception("The ContentContainer control can only contain content controls");
            }
        }

        #endregion

        #region helper

        private void LoadMasterPage(string masterpagefile)
        {
            Control masterPage = Page.LoadControl(masterpagefile);

            bool isRootMaster = false;

            foreach (Control ctrl in masterPage.Controls)
            {
                // head control only appear once in root master file
                if (ctrl is Head)
                {
                    isRootMaster = true;
                    break;
                }
            }

            if (isRootMaster)
            {
                // auto add some controls here
                lock (masterPage.Controls.SyncRoot)
                {
                    JContext jc = JContext.Current;
                    if (!string.Equals(jc.DefaultSite["mode"], "custom", StringComparison.InvariantCultureIgnoreCase))
                    {
                        masterPage.Controls.AddAt(masterPage.Controls.Count - 1, new Include() { Js = "lazyinclude", Css = "common" });

                        masterPage.Controls.AddAt(masterPage.Controls.Count - 1, new UIBase() { ThemeStyleUrl = jc.DefaultSite["jqueryui_theme"] });

                        masterPage.Controls.AddAt(masterPage.Controls.Count - 1, new ControlPanel());
                    }

                    masterPage.Controls.AddAt(masterPage.Controls.Count - 1, new Scripts());
                }

                foreach (Control ctrl in masterPage.Controls)
                {
                    if (ctrl is MasterFileAwaredControl)
                        (ctrl as MasterFileAwaredControl).MasterPageFileName = ThemeMasterFile;

                    if (ctrl is IContextAwaredControl)
                        (ctrl as IContextAwaredControl).CurrentSite = container_site ?? CurrentSite;

                    // load head's child
                    if (ctrl is Head)
                    {
                        foreach (var item in ctrl.Controls)
                        {
                            if (item is IContextAwaredControl)
                                (item as IContextAwaredControl).CurrentSite = container_site ?? CurrentSite;
                        }
                    }

                    // force load child controls
                    int i = ctrl.Controls.Count;
                }
            }

            if (contents.Count > 0)
                FindRecur(contents);

            // save theme master file
            LastThemeMasterFile = ThemeMasterFile;

            Controls.Add(masterPage);
            MoveContentsIntoRegions();
        }

        private void FindRecur(IEnumerable ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if (ctrl is MasterFileAwaredControl)
                    (ctrl as MasterFileAwaredControl).MasterPageFileName = LastThemeMasterFile;

                if (ctrl is IContextAwaredControl)
                    (ctrl as IContextAwaredControl).CurrentSite = CurrentSite;

                if (ctrl.Controls.Count > 0)
                    FindRecur(ctrl.Controls);
            }
        }

        private void MoveContentsIntoRegions()
        {
            foreach (Content content in contents)
            {
                Control contentplaceholder = ContentPlaceHolder.Find(content.ID);

                if (contentplaceholder == null)
                    throw new KissException("Could not find matching region for content with ID '" + content.ID + "'");

                // Set the Content control's TemplateSourceDirectory to be the one from the
                // page.  Otherwise, it would end up using the one from the Master Pages user control,
                // which would be incorrect.
                content._templateSourceDirectory = TemplateSourceDirectory;

                if (!content.Append)
                    contentplaceholder.Controls.Clear();
                contentplaceholder.Controls.Add(content);
            }
        }

        #endregion
    }
}
