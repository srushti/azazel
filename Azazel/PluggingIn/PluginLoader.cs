using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using Azazel.FileSystem;
using File=Azazel.FileSystem.File;

namespace Azazel.PluggingIn {
    public class PluginLoader {
        private static readonly Assemblies assemblies = new Assemblies();
        private static readonly CharacterPlugins characterPlugins = new CharacterPlugins();
        private static readonly LaunchablePlugins launchablePlugins = new LaunchablePlugins();

        static PluginLoader() {
            characterPlugins = new CharacterPlugins();
            launchablePlugins = new LaunchablePlugins();
            var executableLocation = ConfigurationManager.AppSettings["executableLocation"];
            if (string.IsNullOrEmpty(executableLocation))
                executableLocation = new File(Assembly.GetExecutingAssembly().Location).ParentFolder.FullName;
            var pluginAssemblyFiles = new Folder(Path.Combine(executableLocation, "plugins")).GetFiles("*.dll");
            for (var i = 0; i < pluginAssemblyFiles.Count; i++) {
                var assembly = Assembly.LoadFrom(((File) pluginAssemblyFiles.Get(i)).FullName);
                assemblies.Add(assembly);
                new List<Type>(assembly.GetTypes()).ForEach(type => AddPlugin(type, typeof (LaunchablePlugin), launchablePlugins));
                new List<Type>(assembly.GetTypes()).ForEach(type => AddPlugin(type, typeof (CharacterPlugin), characterPlugins));
            }
            launchablePlugins.Add(SelfPlugin.INSTANCE);
        }

        internal virtual Assembly[] Assemblies {
            get { return assemblies.ToArray(); }
        }

        public CharacterPlugins CharacterPlugins {
            get { return characterPlugins; }
        }

        public LaunchablePlugins LaunchablePlugins {
            get { return launchablePlugins; }
        }

        private static void AddPlugin<T>(Type obj, Type pluginType, ICollection<T> plugins) where T : Plugin {
            if (!obj.IsClass || obj.IsAbstract || !pluginType.IsAssignableFrom(obj)) return;
            try {
                var plugin = (T) Activator.CreateInstance(obj, true);
                if (plugin.IsAvailable) plugins.Add(plugin);
            }
            catch (Exception) {
                MessageBox.Show("Could not load plugin : " + obj, "Error loading plugin", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal class Assemblies : List<Assembly> {}
}