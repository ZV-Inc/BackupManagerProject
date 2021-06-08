//StartDirectory — Начальная директория (Директория с файлами\каталогами)
//EndDriectory — Конечная директория (Директория с каталогами копий)
//EndFolder — Название конечного каталога

using System;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace FileSaverService
{
    /// <summary>
    /// Основной класс службы, где прописана вся логика его работы.
    /// </summary>
    public partial class FileSaverService : ServiceBase
    {
        public string StartDirectory;
        public string EndDirectory;
        public string TimeSpan;

        //Импорт advapi32.dll для работы SetServiceStatus.
        [DllImport("advapi32.dll", SetLastError = true)]

        //Установка статуса служба
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        /// <summary>
        /// Метод, где инициализируется служба.
        /// </summary>
        public FileSaverService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Происходит при запуске службы.
        /// </summary>
        /// <param name="args">Аргумент необходимый для парвильной работы метода.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                //Обновление состояния службы до "Start Pending".
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                //TimeSpan заданный в миллисекундах (мс)
                int msTimeSpan = 0;

                //Если журнал с текущим именем не существует, то создаёт его
                if (!EventLog.SourceExists("FileSaverServiceSource"))
                {
                    EventLog.CreateEventSource("FileSaverServiceSource", "FileSaverServiceLog");
                }

                //Указатели на существующий журнал в "Просмотр событий".
                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                //Очистка журнала.
                ServiceLogger.Clear();

                //Открывает указанный раздел реестра.
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\FileSaver"))
                {
                    //Получение занчений из реестра.
                    StartDirectory = registryKey.GetValue("Start Directory").ToString();
                    EndDirectory = registryKey.GetValue("End Directory").ToString();
                    TimeSpan = registryKey.GetValue("Time span").ToString();
                }

                ServiceLogger.WriteEntry($"Start Directory: {StartDirectory}\n" +
                    $"End Directory: {EndDirectory}\n" +
                    $"Time span: {TimeSpan}");

                //Информация о выбранном времени.
                switch (TimeSpan.Substring(0, 2))
                {
                    case "1 ":
                        msTimeSpan = 3600000; //3 600 000 ms (1 час)
                        break;
                    case "6 ":
                        msTimeSpan = 21600000; //21 600 000 ms (6 часов)
                        break;
                    case "12":
                        msTimeSpan = 43200000; //43 200 000 ms (12 часов)
                        break;
                    case "24":
                        msTimeSpan = 86400000; //86 400 000 ms (24 часа)
                        break;
                }

                //Проверка указанных, в файле сохранения, путей. Если меньше или равно 4 знакам, то операция прервётся
                if (StartDirectory.Length <= 4 || EndDirectory.Length <= 4)
                {
                    ServiceLogger.WriteEntry($"Не верно указаны каталоги или путь к ним слишком короткий.\n" +
                        $"\nНачальная директория: {StartDirectory}" +
                        $"\nКонечная директория: {EndDirectory}");
                    return;
                }

                ServiceLogger.WriteEntry($"Служба запустилась с параметрами:\n" +
                    $"Начальная директория: {StartDirectory}\n" +
                    $"Конечная директория: {EndDirectory}\n" +
                    $"Промежуток: {msTimeSpan} ms ({TimeSpan})");

                //Устанавливаем таймер.
                Timer timer = new Timer();
                timer.Interval = msTimeSpan;
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
                timer.Start();

                //Обновление состояния службы до "Running".
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Исключение в методе \"OnStart\":\n" + ex.Message +
                    $"\nНачальная директория: \"{StartDirectory}\"" +
                    $"\nКонечная директория: \"{EndDirectory}\"" +
                    $"\nПромежуток: {TimeSpan}");
            }
        }

        /// <summary>
        /// Происходит во время остановки службы.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                //Обновиление состояния службы до "Stop Pending".
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                ServiceLogger.WriteEntry("Служба остановлена.");

                //Обновления состояния службы до "Stopped".
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Исключение в методе \"OnStop\":\n" + ex.Message);
            }
        }

        /// <summary>
        /// Метод вызываемый во время срабатывания таймера. Здесь прописана оснавная логика копирования файлов.
        /// </summary>
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                //Разделение текущей даты на 2 элемента массива "ДАТА и ВРЕМЯ" (Первый элемент — "01.01.2021". Второй элемент "00:00:00")
                string[] dateNow = DateTime.Now.ToString().Split(' ');

                string EndFolder;

                int FolderVersion = 1;

                //Проверка, существует ли директория End Directory.
                if (!Directory.Exists(EndDirectory))
                {
                    ServiceLogger.WriteEntry($"Директория \"{EndDirectory}\" не найдена. Попытка создать...");

                    DirectoryWork.DirectoryCreate(EndDirectory);

                    if (Directory.Exists(EndDirectory))
                    {
                        ServiceLogger.WriteEntry($"Директория \"{EndDirectory}\" создана.");
                    }
                }
                else
                {
                    ServiceLogger.WriteEntry($"Директория \"{EndDirectory}\" уже существует.");
                }

            //Имя каталога, где будут храниться скопированные файлы и покаталоги
            m1: EndFolder = EndDirectory + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                if (Directory.Exists(EndFolder))
                {
                    ServiceLogger.WriteEntry($"Директория \"{EndFolder}\" уже существует.");

                    FolderVersion++;

                    goto m1;
                }
                else
                {
                    //Присвоение нового имени с увеличеным индексом "FolderVersion"
                    EndFolder = EndDirectory + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                    ServiceLogger.WriteEntry($"Попытка создать каталог \"{EndFolder}\"");

                    DirectoryWork.DirectoryCreate(EndFolder);

                    ServiceLogger.WriteEntry($"каталога \"{EndFolder}\" создана.");
                    ServiceLogger.WriteEntry($"Попытка начать копирование из каталога \"{StartDirectory}\" в каталог \"{EndFolder}\"...");

                    DirectoryWork.DirectoryCopy(StartDirectory, EndFolder, true);

                    FolderVersion = 1;

                    ServiceLogger.WriteEntry("Копирование успешно.");
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Исключение в методе \"OnTimer\":\n" + ex.Message);
            }
        }

        /// <summary>
        /// Типы перечеслений состояний службы.
        /// </summary>
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

        /// <summary>
        /// Структура в которой содержится информация о состоянии службы.
        /// </summary>
        //Последовательно помещает элементы структуры в памяти.
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
