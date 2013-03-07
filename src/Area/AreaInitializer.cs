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
        internal static readonly Dictionary<string, AreaConfig> Areas = new Dictionary<string, AreaConfig>();
        private static readonly ILogger logger = LogManager.GetLogger<AreaInitializer>();
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

            Areas.Add(@"/", AreaConfig.Instance);

            ControllerResolver resolver = ControllerResolver.Instance;

            load_areas(resolver);

            HttpRuntime.Cache.Insert(kCACHE_KEY, "dummyValue", null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

            logger.Debug("AreaInitializer done.");
        }

        private static void load_areas(ControllerResolver resolver)
        {
            List<string> privateBins = new List<string>() { "bin" };

            MethodInfo m = null, funsion = null;

            // check if i am running under mono at runtime. bad coding style
            bool isMono = Type.GetType("Mono.Runtime") != null;

            if (!isMono)
            {
                m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
                funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            }

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

                AreaConfig config = AreaConfig.GetConfig(xml.DocumentElement);
                config.VP = "/" + areaName;
                config.AreaKey = areaName;

                Areas.Add(@"/" + areaName, config);

                // load assemblies
                string bindir = Path.Combine(dir, "bin");

                if (Directory.Exists(bindir))
                {
                    privateBins.Add(bindir);

                    if (!isMono)
                    {
                        // hack !!!
                        if (m != null && funsion != null)
                        {
                            m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", privateBins.Join(";") });
                            m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "SHADOW_COPY_DIRS", privateBins.Join(";") });
                        }
                    }

                    List<Assembly> assemblies = new List<Assembly>();

                    foreach (var item in Directory.GetFiles(bindir, "*.dll", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            if (isMono)
                                assemblies.Add(Assembly.Load(File.ReadAllBytes(item)));
                            else
                                assemblies.Add(AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(item)));
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
        }

        #endregion
    }
}
