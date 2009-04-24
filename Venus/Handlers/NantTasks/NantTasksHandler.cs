using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Azazel.FileSystem;
using Azazel.PluggingIn;
using Action=Azazel.FileSystem.Action;
using File=System.IO.File;

namespace Venus.Handlers.NantTasks {
    public class NantTasksHandler : FileHandler{
        private const string NantPathKey = "NantPath";
        private static readonly string ConfiguredNantPath = ConfigurationManager.AppSettings[NantPathKey];

        /* TODO: For now, you have to add something like this to the config file. Fix this.
       	<appSettings>
            <add key="NantPath" value="C:\Work\Trainline\trainline\Tracs\CommonBuild\nant\bin\NAnt.exe" />
	    </appSettings>
        */

        public override bool IsAvailable {
            get { return IsNantPathConfigured(); }
        }

        private static bool IsNantPathConfigured() {
            return ConfiguredNantPath != null && File.Exists(ConfiguredNantPath);
        }

        protected override bool Handles(FileInfo fileInfo) {
            return IsValidNantBuidFile(fileInfo);
        }

        protected override Actions ActionsFor(FileInfo buildFile) {
            var actions = new Actions();
            
            XmlNodeList targetNodes = GetAllTargets(buildFile);
            foreach (XmlNode targetNode in targetNodes) {
                actions.Add(new NantTargetAction(targetNode, ConfiguredNantPath, buildFile.FullName));
            }
            return actions;
        }

        private static XmlNodeList GetAllTargets(FileInfo fileInfo) {
            //TODO: Cache this?
            var buildFile = new XmlDocument();
            buildFile.Load(fileInfo.OpenRead());
            return buildFile.GetElementsByTagName("target");
        }

        private class NantTargetAction : Action {
            private readonly XmlNode targetNode;
            private readonly string nantPath;
            private readonly string buildFileName;

            public NantTargetAction(XmlNode targetNode, string nantPath, string buildFileName) {
                this.targetNode = targetNode;
                this.nantPath = nantPath;
                this.buildFileName = buildFileName;
            }

            public void Act() {
                //Calls Nant Directly, most of the time, not what we want    
                new Runner(new ProcessStartInfo(nantPath, string.Format("-buildfile:{0} {1}", buildFileName, TargetName))).AsyncStart();
                
//                TODO:Still working on this...
//                var processStartInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().GetExecutingFolder()+"\\CallNantWithPause.bat", BuildArguments());
//                new Runner(processStartInfo, false).AsyncStart();
            }

            private string BuildArguments() {
                return string.Format("{0} -buildfile:{1} {2}",nantPath, buildFileName, TargetName);
            }

            public string Name {
                get { return TargetName; }
            }

            private string TargetName {
                get { return targetNode.Attributes["name"].Value; }
            }
        }

        private static bool IsValidNantBuidFile(FileSystemInfo info) {
            return ".build".Equals(info.Extension, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}