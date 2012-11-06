using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
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

        private readonly string tmpFileName = Guid.NewGuid() + ".appref-ms";
        private const string tmpFileContent = "http://localhost/clickonceupdate/clickonceupdate.application#ClickOnceUpdate.application, Culture=neutral, PublicKeyToken=ac4e6db76b06dfa4, processorArchitecture=msil";

        public void Restart()
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), tmpFileName);
            File.WriteAllText(tmpFile, tmpFileContent, Encoding.Unicode);
            var proc = new Process { StartInfo = { FileName = tmpFile, UseShellExecute = true} };
            
            ReleaseMutex();
            proc.Start();
            Current.Shutdown();
        }
    }
}
