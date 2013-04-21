using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ClickOnceUpdate.Updater;

namespace ClickOnceUpdate
{
    public class SingleInstanceApplication : Application
    {
        private Mutex instanceMutex;
        public SilentUpdater Updater { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            instanceMutex = new Mutex(true, @"Local\" + Assembly.GetExecutingAssembly().GetType().GUID, out createdNew);
            if (!createdNew)
            {
                instanceMutex = null;
                Current.Shutdown();
                return;
            }

            Updater = new SilentUpdater();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ReleaseMutex();
            base.OnExit(e);
        }

        private void ReleaseMutex()
        {
            if (instanceMutex == null) 
                return;
            instanceMutex.ReleaseMutex();
            instanceMutex.Close();
            instanceMutex = null;
        }

        public void Restart()
        {
            //var shortcutFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".appref-ms");
            //CreateClickOnceShortcut(tmpFile);

            var shortcutFile = GetShortcutPath();
            var proc = new Process { StartInfo = { FileName = shortcutFile, UseShellExecute = true } };
            
            ReleaseMutex();
            proc.Start();
            Current.Shutdown();
        }

        public static string GetShortcutPath()
        {
            return String.Format(@"{0}\{1}\{2}.appref-ms", Environment.GetFolderPath(Environment.SpecialFolder.Programs), GetPublisher(), GetDeploymentInfo().Name.Replace(".application", ""));
        }

        public static string GetPublisher()
        {
            XDocument xDocument;
            using (var memoryStream = new MemoryStream(AppDomain.CurrentDomain.ActivationContext.DeploymentManifestBytes))
            using (var xmlTextReader = new XmlTextReader(memoryStream))
                xDocument = XDocument.Load(xmlTextReader);

            if (xDocument.Root == null)
                return null;
            
            var description = xDocument.Root.Elements().First(e => e.Name.LocalName == "description");
            var publisher = description.Attributes().First(a => a.Name.LocalName == "publisher");
            return publisher.Value;
        }

        private static ApplicationId GetDeploymentInfo()
        {
            var appSecurityInfo = new System.Security.Policy.ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            return appSecurityInfo.DeploymentId;
        }

        private static void CreateClickOnceShortcut(string location)
        {
            var updateLocation = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation;
            var deploymentInfo = GetDeploymentInfo();
            using (var shortcutFile = new StreamWriter(location, false, Encoding.Unicode))
            {
                shortcutFile.Write(String.Format(@"{0}#{1}, Culture=neutral, PublicKeyToken=",
                                    updateLocation.ToString().Replace(" ", "%20"),
                                    deploymentInfo.Name.Replace(" ", "%20")));
                foreach (var b in deploymentInfo.PublicKeyToken)
                    shortcutFile.Write("{0:x2}", b);
                shortcutFile.Write(String.Format(", processorArchitecture={0}", deploymentInfo.ProcessorArchitecture));
                shortcutFile.Close();
            }
        }
    }
}
