using System.ServiceProcess;
using System;
using System.Diagnostics;

namespace FileSaverHelper
{
    public partial class FileSaverServiceHelper : ServiceBase
    {
        public static string userName;

        EventLog ServiceLogger = new EventLog();

        public FileSaverServiceHelper()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                ServiceLogger.WriteEntry("Service helper started");

                userName = Environment.UserName;

                ServiceLogger.WriteEntry($"Service Helper:\n" +
                    $"User name: {userName}");
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Service Helper:\n" +
                    $"Exception: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            try
            {
                ServiceLogger.WriteEntry("Service helper stopped");
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Service Helper:\n" +
                    $"Exception: {ex.Message}");
            }
        }
    }
}
