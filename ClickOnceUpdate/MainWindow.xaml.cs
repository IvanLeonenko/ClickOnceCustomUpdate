using System;
using System.Deployment.Application;
using System.Windows;
using ClickOnceUpdate.Updater;

namespace ClickOnceUpdate
{
    public partial class MainWindow
    {
        public SilentUpdater SilentUpdater { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SilentUpdater = ((SingleInstanceApplication)Application.Current).Updater;
            SilentUpdater.ProgressChanged += SilentUpdaterOnProgressChanged;
            DataContext = SilentUpdater;
        }

        private void SilentUpdaterOnProgressChanged(object sender, UpdateProgressChangedEventArgs updateProgressChangedEventArgs)
        {
            downloadStatus.Text = updateProgressChangedEventArgs.StatusString;
        }

        private void TextBlockLoaded(object sender, RoutedEventArgs e)
        {
            textBlock.Text = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
        }
        
        private void RestartButton(object sender, RoutedEventArgs e)
        {
            ((SingleInstanceApplication) Application.Current).Restart();
        }
    }
}
