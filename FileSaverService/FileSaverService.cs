using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;

namespace FileSaverService
{
    public partial class FileSaverService : ServiceBase
    {
        public string StartDir;
        public string EndDir;
        public string DefaultSaveFileDirectory;
        public string EndDirectoryDateName;
        public int SaveFileTime;

        List<string> newList = new List<string>();

        [DllImport("advapi32.dll", SetLastError = true)]

        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public FileSaverService()
        {
            try
            {
                InitializeComponent();

                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                ServiceLogger.Clear();

                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                DefaultSaveFileDirectory = $"C:/FSSaves/"; //Exception on save file: Не удалось найти часть пути "C:\Users\СИСТЕМА\Documents\FSSaves\Save.txt".

                string[] FileReader = File.ReadAllLines(DefaultSaveFileDirectory + "FSSave.txt");

                int found = 0;

                foreach (string s in FileReader)
                {
                    found = s.IndexOf(": ");
                    newList.Add(s.Substring(found + 2));
                }

                StartDir = newList[1];
                EndDir = newList[2];

                switch (newList[3].Substring(0, 2))
                {
                    case "1 ":
                        SaveFileTime = 3600000; //3 600 000 ms (1 hour)
                        break;
                    case "6 ":
                        SaveFileTime = 21600000; //21 600 000 ms (6 hours)
                        break;
                    case "12":
                        SaveFileTime = 43200000; //43 200 000 ms (12 hours)
                        break;
                    case "24":
                        SaveFileTime = 86400000; //86 400 000 ms (24 hours)
                        break;
                }

                if (StartDir.Length <= 4 || EndDir.Length <= 4)
                {
                    StartDir = "C:/Logs";
                    EndDir = "E:/LogsBackup";
                }
                else
                {
                    StartDir = newList[1];
                    EndDir = newList[2];
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Exception on initialize step: \n" + ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // Update the service state to Start Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                StartDir = newList[1];
                EndDir = newList[2];

                if (!EventLog.SourceExists("FileSaverServiceSource"))
                {
                    EventLog.CreateEventSource("FileSaverServiceSource", "FileSaverServiceLog");
                }

                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                ServiceLogger.WriteEntry($"Service started with this parameters: \n" +
                    $"Start folder: {StartDir}\n" +
                    $"End folder: {EndDir}\n" +
                    $"Time span: {newList[3]}");

                // Set up a timer that triggers every minute.
                Timer timer = new Timer();
                timer.Interval = SaveFileTime; // 180 seconds
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
                timer.Start();

                ServiceLogger.WriteEntry($"Timer Sarted. Time span: {SaveFileTime} ms ({newList[3]})");

                // Update the service state to Running.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"OnStart directories exception: " + ex.Message +
                    $"\n\n Directories:\n   " +
                    $"Start Directory: \u0022{StartDir}\u0022 \n   " +
                    $"End Directory: \u0022{EndDir}\u0022");
            }
        }

        protected override void OnStop()
        {
            try
            {
                // Update the service state to Stop Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                ServiceLogger.WriteEntry("Service OnStopped");

                // Update the service state to Stopped.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("OnStop exception: " + ex.Message);
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                EndDirectoryDateName = DateTime.Now.ToString();

                StartDir = newList[1];
                EndDir = newList[2];

                // TODO: Insert monitoring activities here.
                ServiceLogger.WriteEntry($"Start Directory: {StartDir}");
                ServiceLogger.WriteEntry($"End Directory: {EndDir}");
                ServiceLogger.WriteEntry("Main operations started");
                ServiceLogger.WriteEntry($"Checked if {EndDir} exists");

                if (!Directory.Exists(EndDir))
                {
                    ServiceLogger.WriteEntry($"Folder {EndDir} don't exists. Try create.");
                    DirectoryWork.DirectoryCreate(EndDir);
                    ServiceLogger.WriteEntry($"Create {EndDir} ended.");
                }
                else
                {
                    ServiceLogger.WriteEntry($"Directory {EndDir} already exists");
                    ServiceLogger.WriteEntry($"Try to clear {EndDir}");

                    Directory.Delete(EndDir, true);
                    DirectoryWork.DirectoryCreate(EndDir);

                    ServiceLogger.WriteEntry($"Folder {EndDir} cleared");
                }

                if (!Directory.Exists(EndDir))
                {
                    ServiceLogger.WriteEntry($"Folder {EndDir} don't exists. Try create.");
                    DirectoryWork.DirectoryCreate(EndDir);
                    ServiceLogger.WriteEntry($"Create {EndDir} ended.");
                }

                ServiceLogger.WriteEntry("Starting copy method");

                ServiceLogger.WriteEntry($"Start Directory: {StartDir}");
                ServiceLogger.WriteEntry($"End Directory: {EndDir}");

                DirectoryWork.DirectoryCopy(StartDir, EndDir, true);

                ServiceLogger.WriteEntry("Copy method ended");
                ServiceLogger.WriteEntry("All operation has ended");
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("OnTimer exception: " + ex.Message);
            }
        }

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
    }
}
