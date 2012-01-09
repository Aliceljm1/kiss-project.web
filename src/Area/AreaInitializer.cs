using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Kiss.Plugin;
using Kiss.Utils;
using Kiss.Web.Mvc;
using Kiss.Web.UrlMapping;

namespace Kiss.Web.Area
{
    [AutoInit(Title = "Area", Priority = 8)]
    public class AreaInitializer : IPluginInitializer
    {
        const string kCACHE_KEY = "__AreaInitializer_cache_key__";
        internal static readonly Dictionary<string, SiteConfig> Areas = new Dictionary<string, SiteConfig>();
        private static readonly ILogger logger = LogManager.GetLogger<AreaInitializer>();
        private CacheDependency _fileDependency;
        private static readonly List<string> IGNORES_DIR = new List<string>() { "app_data", "bin", "app_browser", "app_code", "app_globalresources", "app_localresources", "app_themes", "app_webreferences" };

        #region IPluginInitializer Members

        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (!setting.Enable)
            {
                sl.AddComponent("kiss.XmlUrlMappingProvider", typeof(IUrlMappingProvider), typeof(XmlUrlMappingProvider));
                sl.AddComponent("kiss.defaultHost", typeof(IHost), typeof(Kiss.Web.Host));

                return;
            }

            sl.AddComponent("kiss.Areahost", typeof(IHost), typeof(Host));
            sl.AddComponent("kiss.AreaUrlMappingProvider", typeof(IUrlMappingProvider), typeof(AreaUrlMappingProvider));

            Areas.Add(@"/", SiteConfig.Instance);

            ControllerResolver resolver = ControllerResolver.Instance;

            List<string> files = load_areas(resolver);

            _fileDependency = new CacheDependency(files.ToArray());
            HttpRuntime.Cache.Insert(kCACHE_KEY, "dummyValue", _fileDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, (k, v, r) =>
            {
                File.SetLastWriteTime(ServerUtil.MapPath("~/Web.config"), DateTime.Now);
            });

            logger.Debug("AreaInitializer done.");
        }

        private static List<string> load_areas(ControllerResolver resolver)
        {
            List<string> monitor_paths = new List<string>();

            List<string> privateBins = new List<string>() { "bin" };
#if !MONO
            var m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

            foreach (var dir in Directory.GetDirectories(ServerUtil.MapPath("~")))
            {
                string areaName = Path.GetFileName(dir).ToLowerInvariant();

                if (IGNORES_DIR.Contains(areaName))
                    continue;

                // check if the dir is a valid area
                string configfile = Path.Combine(dir, "area.config");
                if (!File.Exists(configfile))
                    continue;

                // load area config
                XmlDocument xml = new XmlDocument();
                xml.Load(configfile);

                SiteConfig config = SiteConfig.GetConfig(xml.DocumentElement);
                config.VP = "/" + areaName;
                config.SiteKey = areaName;

                Areas.Add(@"/" + areaName, config);

                monitor_paths.Add(configfile);

                // load assemblies
                string bindir = Path.Combine(dir, "bin");

                if (Directory.Exists(bindir))
                {
                    privateBins.Add(bindir);

                    monitor_paths.Add(bindir);

#if !MONO
                    // hack !!!
                    if (m != null && funsion != null)
                    {
                        m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", privateBins.Join(";") });
                        m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "SHADOW_COPY_DIRS", privateBins.Join(";") });
                    }
#endif

                    List<Assembly> assemblies = new List<Assembly>();

                    foreach (var item in Directory.GetFiles(bindir, "*.dll", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
#if MONO
                            assemblies.Add(Assembly.Load(File.ReadAllBytes(item)));
#else
                            assemblies.Add(AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(item)));
#endif
                        }
                        catch (BadImageFormatException)
                        {
                        }
                    }

                    Dictionary<string, Type> types = new Dictionary<string, Type>();
                    foreach (var asm in assemblies)
                    {
                        foreach (var item in resolver.GetsControllerFromAssembly(asm))
                        {
                            types[item.Key] = item.Value;
                        }
                    }
                    resolver.SetSiteControllers(areaName, types);
                }
            }

            return monitor_paths;
        }

        #endregion
    }
}
