using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Timers;

namespace ClickOnceUpdate.Updater
{
    public class SilentUpdater : INotifyPropertyChanged
    {
        private readonly ApplicationDeployment applicationDeployment;
        private readonly Timer timer = new Timer(60000);
        private bool processing;

        public event EventHandler<UpdateProgressChangedEventArgs> ProgressChanged;
        public event EventHandler<EventArgs> Completed;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool updateAvailable;
        public bool UpdateAvailable
        {
            get { return updateAvailable; }
            private set { updateAvailable = value; OnPropertyChanged("UpdateAvailable"); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCompleted()
        {
            var handler = Completed;
            if (handler != null) handler(this, null);
        }

        private void OnProgressChanged(UpdateProgressChangedEventArgs e)
        {
            var handler = ProgressChanged;
            if (handler != null) handler(this, e);
        }

        public SilentUpdater()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
                return;
            applicationDeployment = ApplicationDeployment.CurrentDeployment;
            applicationDeployment.CheckForUpdateCompleted += CheckForUpdateCompleted;
            applicationDeployment.CheckForUpdateProgressChanged += CheckForUpdateProgressChanged;
            applicationDeployment.UpdateCompleted += UpdateCompleted;
            applicationDeployment.UpdateProgressChanged += UpdateProgressChanged;
            timer.Elapsed += (sender, args) =>
                                 {
                                     if (processing)
                                         return;
                                     processing = true;
                                     applicationDeployment.CheckForUpdateAsync();
                                 };
            timer.Start();
        }

        void CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            //String.Format("Downloading: {0}. {1:D}K of {2:D}K downloaded.", GetProgressString(e.State), e.BytesCompleted / 1024, e.BytesTotal / 1024);
        }

        void CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled || !e.UpdateAvailable)
            {
                Debug.WriteLine("Check for update failed.");
                processing = false;
                return;
            }
            applicationDeployment.UpdateAsync();
        }

        void UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            OnProgressChanged(new UpdateProgressChangedEventArgs(e));
        }

        void UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            processing = false;
            if (e.Cancelled || e.Error != null)
            {
                Debug.WriteLine("Could not install the latest version of the application.");
                return;
            }
            UpdateAvailable = true;
            OnCompleted();
        }
    }
}
