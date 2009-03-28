using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using Azazel.FileSystem;
using Azazel.Logging;
using File=Azazel.FileSystem.File;

namespace Azazel.PluggingIn {
    public class PluginLoader {
        private readonly Assemblies assemblies = new Assemblies();
        private readonly CharacterPlugins characterPlugins = new CharacterPlugins();
        private readonly LaunchablePlugins launchablePlugins = new LaunchablePlugins();

        public PluginLoader(SelfPlugin selfPlugin) {
            characterPlugins = new CharacterPlugins();
            launchablePlugins = new LaunchablePlugins();
            var executableLocation = ConfigurationManager.AppSettings["executableLocation"];
            var executingAssembly = Assembly.GetExecutingAssembly();
            if (string.IsNullOrEmpty(executableLocation)) executableLocation = new File(executingAssembly.Location).ParentFolder.FullName;
            var pluginAssemblyFiles = new Folder(Path.Combine(executableLocation, "plugins")).GetFiles("*.dll");
            LogManager.WriteLog("Started loading plugins");
            for (var i = 0; i < pluginAssemblyFiles.Count; i++) {
                var assembly = Assembly.LoadFrom(((File) pluginAssemblyFiles.Get(i)).FullName);
                if (new List<AssemblyName>(assembly.GetReferencedAssemblies()).Exists(name => name.FullName == executingAssembly.FullName)) {
                    assemblies.Add(assembly);
                    new List<Type>(assembly.GetTypes()).ForEach(type => AddPlugin(type, typeof (LaunchablePlugin), launchablePlugins));
                    new List<Type>(assembly.GetTypes()).ForEach(type => AddPlugin(type, typeof (CharacterPlugin), characterPlugins));
                }
            }
            LogManager.WriteLog("Done loading plugins");
            launchablePlugins.Add(selfPlugin);
        }

        public Assembly[] Assemblies {
            get { return assemblies.ToArray(); }
        }

        public CharacterPlugins CharacterPlugins {
            get { return characterPlugins; }
        }

        public LaunchablePlugins LaunchablePlugins {
            get { return launchablePlugins; }
        }

        private static void AddPlugin<T>(Type type, Type pluginType, ICollection<T> plugins) where T : Plugin {
            if (!type.IsClass || type.IsAbstract || !pluginType.IsAssignableFrom(type)) return;
            try {
                LogManager.WriteLog("Loading plugin " + type);
                var plugin = (T) Activator.CreateInstance(type, true);
                if (plugin.IsAvailable) plugins.Add(plugin);
            }
            catch (Exception) {
                MessageBox.Show("Could not load plugin : " + type, "Error loading plugin", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal class Assemblies : List<Assembly> {}
}