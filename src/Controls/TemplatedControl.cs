using System.Web;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 控件基类，支持加载不同的ascx文件
    /// </summary>
    [ParseChildren(true), PersistChildren(false)]
    public class TemplatedControl : Control, INamingContainer
    {
        const string SkinFolderFormat = "{0}/{1}/skins/";

        #region ctor

        public TemplatedControl()
        {
        }

        public TemplatedControl(string skinname)
        {
            SkinName = skinname;
            UsedInMvc = true;
            Templated = true;
        }

        #endregion

        #region props

        private string skinName;
        /// <summary>
        /// 皮肤控件名
        /// </summary>        
        public string SkinName
        {
            get
            {
                if (StringUtil.IsNullOrEmpty(skinName))
                {
                    if (UsedInMvc && ActionAsSkinName)// insure unique in a page
                    {
                        string viewResultSkin = JContext.Current.Items["__viewResult__"] as string;
                        if (StringUtil.HasText(viewResultSkin))
                            skinName = viewResultSkin;
                        else
                            skinName = JContext.Current.Navigation.Action;
                    }
                    else
                        skinName = GetType().Name;
                }

                return skinName;
            }
            set { skinName = value; }
        }

        /// <summary>
        /// user action name as skin name
        /// </summary>
        public bool ActionAsSkinName { get; set; }

        /// <summary>
        /// use template engine to render
        /// </summary>
        public bool Templated { get; set; }

        private string skinFilenamePrefix = string.Empty;
        public string SkinFileNamePrefix
        {
            get
            {
                return UsedInMvc ? string.Format("{0}{1}/", skinFilenamePrefix, JContext.Current.Navigation.Id) :
                    skinFilenamePrefix;
            }
            set
            {
                skinFilenamePrefix = value;
            }
        }
        public string SkinFileNamePostfix { get; set; }

        private bool throwExceptionOnSkinFileNotFound = true;
        /// <summary>
        /// 皮肤文件不存在时是否抛出异常
        /// </summary>
        public bool ThrowExceptionOnSkinFileNotFound { get { return throwExceptionOnSkinFileNotFound; } set { throwExceptionOnSkinFileNotFound = value; } }

        #endregion

        #region override

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        public override Control FindControl(string id)
        {
            Control ctrl = base.FindControl(id);
            if (ctrl == null && this.Controls.Count == 1)
            {
                ctrl = this.Controls[0].FindControl(id);
            }
            return ctrl;
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            bool loaded = false;

            string skinFilename = GetSkinFileName(SkinName);

            string skinFile = GetSkinFileFullPath(GetSkinFolder(ThemeName), skinFilename);

            // 加载自定义ThemeName,lang的skin
            if (ServerUtil.FileExists(skinFile))
                loaded = LoadSkin(skinFile);

            string defaultlang_skinFile = GetSkinFileFullPath(GetSkinFolder(ThemeName, false), skinFilename);

            if (!loaded && ServerUtil.FileExists(defaultlang_skinFile))
                loaded = LoadSkin(defaultlang_skinFile);

            string default_skinFile = GetSkinFileFullPath(GetSkinFolder("default", false), skinFilename);
            if (!loaded && ServerUtil.FileExists(default_skinFile))
                loaded = LoadSkin(default_skinFile);

            if (!loaded && ThrowExceptionOnSkinFileNotFound)
                throw new WebException("Skin file not found in " + skinFile + " nor in " + defaultlang_skinFile + " nor in " + default_skinFile);

            if (loaded)
                AttachChildControls();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Templated)
                writer.Write(Util.Render(delegate(HtmlTextWriter w) { base.Render(w); }));
            else
                base.Render(writer);
        }

        #endregion

        #region virtual

        /// <summary>
        /// skin is used in mvc.
        /// </summary>
        public bool UsedInMvc { get; set; }

        private string _themeName;
        /// <summary>
        /// 获取当前访问用户的皮肤名
        /// </summary>
        protected virtual string ThemeName
        {
            get
            {
                if (StringUtil.IsNullOrEmpty(_themeName))
                    _themeName = JContext.Current.Site.DefaultTheme;
                return _themeName;
            }
        }

        /// <summary>
        /// 重写该方法加载皮肤控件
        /// </summary>
        /// <remarks>
        /// 只有当非默认的皮肤使用时才会用到
        /// </remarks>
        protected virtual void AttachChildControls() { }

        #endregion

        #region helper

        protected virtual string GetSkinFileName(string name)
        {
            string prefix = SkinFileNamePrefix;
            if (prefix.StartsWith("/") && prefix.EndsWith("/"))
                prefix = string.Empty;
            return string.Format("{1}{0}{2}.ascx", name, prefix, SkinFileNamePostfix);
        }

        private static string GetSkinFolder(string theme)
        {
            return GetSkinFolder(theme, true);
        }

        private static string GetSkinFolder(string theme, bool lang)
        {
            JContext jc = JContext.Current;

            return string.Format(SkinFolderFormat,
                jc.Site.ThemeRoot,
                !lang || StringUtil.IsNullOrEmpty(jc.Navigation.LanguageCode) ? theme : string.Format("{0}-{1}", theme, jc.Navigation.LanguageCode));
        }

        private static string GetSkinFileFullPath(string folder, string skinFile)
        {
            const string s = "/";

            if (!string.IsNullOrEmpty(folder) && !folder.EndsWith(s))
            {
                folder += s;
            }

            return folder + skinFile;
        }

        private bool LoadSkin(string skinPath)
        {
            Page page = HttpContext.Current.CurrentHandler as Page;
            if (page != null)
            {
                Control skin = page.LoadControl(skinPath);
                this.Controls.Add(skin);

                return true;
            }

            return false;
        }

        #endregion

        public string Execute()
        {
            System.Web.UI.Page p = new Page();
            p.Controls.Add(this);

            return ServerUtil.ExecutePage(p);
        }
    }
}
