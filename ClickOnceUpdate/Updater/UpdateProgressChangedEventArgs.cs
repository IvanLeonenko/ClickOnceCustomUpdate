using System;
using System.Deployment.Application;

namespace ClickOnceUpdate.Updater
{
    public class UpdateProgressChangedEventArgs : EventArgs
    {
        public DeploymentProgressChangedEventArgs ProgressChangedEventArgs { get; private set; }
        public string StatusString { get; private set; }

        public UpdateProgressChangedEventArgs(DeploymentProgressChangedEventArgs args)
        {
            ProgressChangedEventArgs = args;
            StatusString = String.Format("Downloading: {0}. {1:D}K out of {2:D}K downloaded - {3:D}% complete", GetProgressString(args.State), args.BytesCompleted / 1024, args.BytesTotal / 1024, args.ProgressPercentage);
        }

        public override string ToString()
        {
            return StatusString;
        }

        private string GetProgressString(DeploymentProgressState state)
        {
            if (state == DeploymentProgressState.DownloadingApplicationFiles)
            {
                return "application files";
            }
            if (state == DeploymentProgressState.DownloadingApplicationInformation)
            {
                return "application manifest";
            }
            return "deployment manifest";
        }
    }
}
