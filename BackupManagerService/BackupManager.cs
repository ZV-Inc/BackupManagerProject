using System;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using BackupManagerLib;
using Microsoft.Win32;

namespace BackupManagerService
{
    public partial class BackupManagerService : ServiceBase
    {
        private string _startDirectory;
        private string _endDirectory;
        private string _timeSpan;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public BackupManagerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                if (!EventLog.SourceExists("FileSaverServiceSource"))
                {
                    EventLog.CreateEventSource("FileSaverServiceSource", "FileSaverServiceLog");
                }

                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                int msTimeSpan = 0;

                ServiceLogger.Clear();

                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\FileSaver"))
                {
                    _startDirectory = registryKey.GetValue("Start Directory").ToString();
                    _endDirectory = registryKey.GetValue("End Directory").ToString();
                    _timeSpan = registryKey.GetValue("Time span").ToString();
                }

                switch (_timeSpan.Substring(0, 2))
                {
                    case "1 ":
                        msTimeSpan = 60000;
                        break;
                    case "6 ":
                        msTimeSpan = 21600000;
                        break;
                    case "12":
                        msTimeSpan = 43200000;
                        break;
                    case "24":
                        msTimeSpan = 86400000;
                        break;
                }

                if (_startDirectory.Length <= 4 || _endDirectory.Length <= 4)
                {
                    ServiceLogger.WriteEntry($"Не верно указаны каталоги или путь к ним слишком короткий.\n" +
                        $"\nНачальная директория: {_startDirectory}" +
                        $"\nКонечная директория: {_endDirectory}");
                    return;
                }

                ServiceLogger.WriteEntry($"Служба запустилась с параметрами:\n" +
                    $"Начальная директория: {_startDirectory}\n" +
                    $"Конечная директория: {_endDirectory}\n" +
                    $"Промежуток: {msTimeSpan} ms (1 минута)");

                Timer timer = new Timer();
                timer.Interval = msTimeSpan;
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
                timer.Start();

                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Исключение в методе \"OnStart\":\n" + ex.Message +
                    $"\nНачальная директория: \"{_startDirectory}\"" +
                    $"\nКонечная директория: \"{_endDirectory}\"" +
                    $"\nПромежуток: {_timeSpan}");
            }
        }

        protected override void OnStop()
        {
            try
            {
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                ServiceLogger.WriteEntry("Служба остановлена.");

                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Исключение в методе \"OnStop\":\n" + ex.Message);
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                DirectoryExtensions directoryExtensions = new DirectoryExtensions();
                string[] dateNow = DateTime.Now.ToString().Split(' ');
                string EndFolder;
                int FolderVersion = 1;

                if (!Directory.Exists(_endDirectory))
                {
                    ServiceLogger.WriteEntry($"Директория \"{_endDirectory}\" не найдена. Попытка создать...");

                    directoryExtensions.DirectoryCreate(_endDirectory);

                    if (Directory.Exists(_endDirectory))
                    {
                        ServiceLogger.WriteEntry($"Директория \"{_endDirectory}\" создана.");
                    }
                }
                else
                {
                    ServiceLogger.WriteEntry($"Директория \"{_endDirectory}\" уже существует.");
                }

            m1: EndFolder = _endDirectory + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                if (Directory.Exists(EndFolder))
                {
                    ServiceLogger.WriteEntry($"Директория \"{EndFolder}\" уже существует.");

                    FolderVersion++;

                    goto m1;
                }
                else
                {
                    EndFolder = _endDirectory + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                    ServiceLogger.WriteEntry($"Попытка создать каталог \"{EndFolder}\"");

                    directoryExtensions.DirectoryCreate(EndFolder);

                    ServiceLogger.WriteEntry($"Каталог \"{EndFolder}\" создан.");
                    ServiceLogger.WriteEntry($"Попытка начать копирование из каталога \"{_startDirectory}\" в каталог \"{EndFolder}\"...");

                    directoryExtensions.DirectoryCopy(_startDirectory, EndFolder, true);

                    FolderVersion = 1;

                    ServiceLogger.WriteEntry("Копирование успешно.");
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Исключение в методе \"OnTimer\":\n" + ex.Message);
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