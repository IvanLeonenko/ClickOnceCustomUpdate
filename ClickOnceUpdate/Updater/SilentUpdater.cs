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
            applicationDeployment.UpdateCompleted += UpdateCompleted;
            applicationDeployment.UpdateProgressChanged += UpdateProgressChanged;
            timer.Elapsed += (sender, args) =>
                                 {
                                     if (processing)
                                         return;
                                     processing = true;
                                     try
                                     {
                                         if (applicationDeployment.CheckForUpdate(false))
                                             applicationDeployment.UpdateAsync();
                                         else
                                             processing = false;
                                     }
                                     catch(Exception ex)
                                     {
                                         Debug.WriteLine("Check for update failed. " + ex.Message);
                                         processing = false;
                                     }
                                 };
            timer.Start();
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
