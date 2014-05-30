using Kiss.Utils;
using System.IO;
using System.Web;
using System.Web.UI;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 控件基类，支持加载不同的ascx文件
    /// </summary>
    [ParseChildren(true), PersistChildren(false)]
    public class TemplatedControl : Control, INamingContainer, IContextAwaredControl
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

        JContext jc = JContext.Current;

        private IArea _site;
        public IArea CurrentSite { get { return _site ?? JContext.Current.Area; } set { _site = value; } }

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
                    if (OverrideSkinName || (UsedInMvc && ActionAsSkinName))// insure unique in a page
                    {
                        string viewResultSkin = jc.Items["__viewResult__"] as string;
                        if (StringUtil.HasText(viewResultSkin))
                        {
                            skinName = viewResultSkin;

                            // set used in mvc to false if skinname contains '/'
                            if (skinName.Contains("/"))
                                UsedInMvc = false;
                        }
                        else
                            skinName = jc.Navigation.Action;
                    }
                    else if (ActionAsSkinName)
                        skinName = jc.Navigation.Action;
                    else
                        skinName = GetType().Name;

                    if (string.IsNullOrEmpty(skinName))
                        skinName = GetType().Name;
                }

                return skinName;
            }
            set { skinName = value; }
        }

        public bool IsFile
        {
            get
            {
                return !(!jc.Items["__viewResult_IsFile__"].ToBoolean(true) && (OverrideSkinName || (UsedInMvc && ActionAsSkinName)));
            }
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
                string id = jc.Navigation.Id;
                if (id.Contains(":"))
                    id = id.Substring(id.IndexOf(":") + 1);

                return UsedInMvc ? string.Format("{0}{1}/", skinFilenamePrefix, id) :
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
            if (!IsFile) return;

            Controls.Clear();

            bool loaded = false;

            string skinFilename = GetSkinFileName(SkinName);

            string skinFile = GetSkinFileFullPath(GetSkinFolder(ThemeName), skinFilename);
            if (File.Exists(ServerUtil.MapPath(skinFile)))
            {
                jc.ViewData["_skinfile_"] = skinFile;
                loaded = LoadSkin(skinFile);
            }

            if (!loaded)
            {
                string format = ThemeName;

                if (format.IndexOf('.') != -1)
                    format = format.Split('.')[0];

                string default_formatskinFile = GetSkinFileFullPath(GetSkinFolder(format), skinFilename);
                if (File.Exists(ServerUtil.MapPath(default_formatskinFile)))
                {
                    jc.ViewData["_skinfile_"] = default_formatskinFile;
                    loaded = LoadSkin(default_formatskinFile);
                }

                string default_skinFile = null;

                string defaultTheme = MobileDetect.Instance.IsMobile ? "default_mobile" : "default";

                if (!loaded && !string.Equals(format, defaultTheme, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    default_skinFile = GetSkinFileFullPath(GetSkinFolder(defaultTheme), skinFilename);
                    if (File.Exists(ServerUtil.MapPath(default_skinFile)))
                    {
                        jc.ViewData["_skinfile_"] = default_skinFile;
                        loaded = LoadSkin(default_skinFile);
                    }
                }

                if (!loaded && ThrowExceptionOnSkinFileNotFound)
                    throw new WebException("Skin file not found in {0} nor in {1}{2}",
                        skinFile,
                        default_formatskinFile,
                        string.IsNullOrEmpty(default_skinFile) ? string.Empty : " nor in " + default_skinFile);

            }

            if (loaded)
                AttachChildControls();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Templated)
            {
                writer.Write(Util.Render(delegate(HtmlTextWriter w)
                {
                    if (!IsFile)
                    {
                        w.Write(jc.Items["__viewResult__"]);
                    }
                    else
                    {
                        base.Render(w);
                    }

                    if (jc.IsAjaxRequest)
                    {
                        w.Write("$!jc.render_lazy_include()");
                    }
                }));
            }
            else
            {
                if (!IsFile)
                {
                    writer.Write(jc.Items["__viewResult__"]);
                }
                else
                {
                    base.Render(writer);
                }
            }
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
                {
                    _themeName = MobileDetect.Instance.GetRealThemeName(CurrentSite);
                }
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

        private string GetSkinFolder(string theme)
        {
            CurrentSite = CurrentSite ?? jc.Area;

            if (!MobileDetect.Instance.IsMobile && jc.SiteConfig != null && !string.IsNullOrEmpty(CurrentSite.Layout))
            {
                string layout_url = string.Format("{0}/{1}/layouts/{2}/",
                    StringUtil.CombinUrl(CurrentSite.VirtualPath, CurrentSite.ThemeRoot),
                    theme,
                    CurrentSite.Layout);

                string layout_path = ServerUtil.MapPath(layout_url);

                if (Directory.Exists(layout_path) && File.Exists(Path.Combine(layout_path, GetSkinFileName(SkinName))))
                {
                    return layout_url;
                }
            }

            return string.Format(SkinFolderFormat,
                StringUtil.CombinUrl(CurrentSite.VirtualPath, CurrentSite.ThemeRoot),
                theme);
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

        public bool OverrideSkinName { get; set; }
    }
}
