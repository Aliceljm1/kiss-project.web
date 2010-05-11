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

        /// <summary>
        /// 是否只到默认目录下查找皮肤
        /// </summary>
        public bool Shared { get; set; }

        private string skinFilenamePrefix = string.Empty;
        public string SkinFileNamePrefix
        {
            get
            {
                return UsedInMvc && !Shared ? string.Format("{0}{1}/", skinFilenamePrefix, JContext.Current.Navigation.Id) :
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

            // 加载自定义ThemeName的
            if (SkinFileExists)
                loaded = LoadSkin(SkinFile);

            if (!loaded && DefaultSkinFileExists)
                loaded = LoadDefaultThemedControl();

            if (!loaded && ThrowExceptionOnSkinFileNotFound)
                throw new WebException("Skin file not found in " + SkinFile + " nor in " + DefaultSkinFile);

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

        /// <summary>
        /// 创建默认的皮肤控件文件名——类名
        /// </summary>
        protected virtual string SkinFileName
        {
            get
            {
                return GetSkinFileName(SkinName);
            }
        }

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
        /// 控件目录
        /// </summary>
        protected virtual string SkinFolder
        {
            get
            {
                if (Shared)
                    return GetSkinFolder("default");

                return GetSkinFolder(ThemeName);
            }
        }

        /// <summary>
        /// 获取要加载的皮肤控件文件
        /// </summary>
        protected string SkinFile { get { return GetSkinFileFullPath(SkinFolder, SkinFileName); } }

        /// <summary>
        /// 默认皮肤控件文件路径
        /// </summary>
        protected virtual string DefaultSkinFile
        {
            get
            {
                return GetSkinFileFullPath(GetSkinFolder("default"), SkinFileName);
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

        protected string GetSkinFileName(string name)
        {
            string prefix = SkinFileNamePrefix;
            if (prefix.StartsWith("/") && prefix.EndsWith("/"))
                prefix = string.Empty;
            return string.Format("{1}{0}{2}.ascx", name, prefix, SkinFileNamePostfix);
        }

        private const string SkinFolderFormat = "{0}/{1}/skins/";

        private static string GetSkinFolder(string theme)
        {
            return string.Format(SkinFolderFormat, JContext.Current.Site.ThemeRoot, theme);
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

        private bool LoadDefaultThemedControl()
        {
            return LoadSkin(this.DefaultSkinFile);
        }

        /// <summary>
        /// 皮肤控件文件是否存在
        /// </summary>
        private bool SkinFileExists
        {
            get
            {
                return ServerUtil.FileExists(SkinFile);
            }
        }

        /// <summary>
        /// 默认皮肤文件是否存在
        /// </summary>
        private bool DefaultSkinFileExists
        {
            get
            {
                return ServerUtil.FileExists(DefaultSkinFile);
            }
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
