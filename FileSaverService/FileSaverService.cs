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
        public string EndFolder;

        public int SaveFileTime;
        public int FolderVersion = 1;

        List<string> SaveFileList = new List<string>();

        FileSaverService fileSaverService = new FileSaverService();

        //Импорт advapi32.dll для корректной работы SetServiceStatus
        [DllImport("advapi32.dll", SetLastError = true)]

        //Установка статуса сервиса
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public FileSaverService()
        {
            try
            {
                InitializeComponent();

                //Указатели на существующий лог в "Просмотр событий"
                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                //Очистка лога
                ServiceLogger.Clear();

                //Нужно реализовать "Загрузку"
                DefaultSaveFileDirectory = $"C:/FSSaves/";

                string[] SaveReader = File.ReadAllLines(DefaultSaveFileDirectory + "FSSave.txt"); //FSSave должен получать название файла из "Загрузки" (Загруженного файла)

                int found = 0;

                //Разделение информации из файла сохранения
                foreach (string s in SaveReader)
                {
                    found = s.IndexOf(": ");
                    //Разделение и добавление информации в лист (добавление только информации после занка ": ")
                    SaveFileList.Add(s.Substring(found + 2));
                }

                //Лист имеет 5 индексов:
                //0 индекс — дата и время сохранения
                //1 и 2 индексы — папки указанные в файле сохранения
                //3 индекс — выбранный промежуток между сохранениями
                //4 индекс — пользователь, от имени которого сделано сохранение
                StartDir = SaveFileList[1]; //Начальная директория
                EndDir = SaveFileList[2]; //Конечная директория

                //Информация о выбранном времени
                switch (SaveFileList[3].Substring(0, 2))
                {
                    case "1 ":
                        SaveFileTime = 3600000; //3 600 000 ms (1 час)
                        break;
                    case "6 ":
                        SaveFileTime = 21600000; //21 600 000 ms (6 часов)
                        break;
                    case "12":
                        SaveFileTime = 43200000; //43 200 000 ms (12 часов)
                        break;
                    case "24":
                        SaveFileTime = 86400000; //86 400 000 ms (24 часа)
                        break;
                }

                //Проверка указанных в файле сохранения путей. Если меньше или равно 4 знакам, то операция прервётся
                if (StartDir.Length <= 4 || EndDir.Length <= 4)
                {
                    ServiceLogger.WriteEntry($"Не верно указаны папки или путь к ним слишком короткий." +
                        $"\nНачальная папка: {StartDir}" +
                        $"\nКонечная папка: {EndDir}");
                    fileSaverService.Stop();
                    return;
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
                //Обновление состояния службы до "Start Pending".
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                StartDir = SaveFileList[1];
                EndDir = SaveFileList[2];

                //Если лога с текущими указателями не существует, то создаёт его
                if (!EventLog.SourceExists("FileSaverServiceSource"))
                {
                    EventLog.CreateEventSource("FileSaverServiceSource", "FileSaverServiceLog");
                }

                ServiceLogger.WriteEntry($"Сервис запустился с параметрами: \n" +
                    $"Начальная папка: {StartDir}\n" +
                    $"Конечная папка: {EndDir}\n" +
                    $"Промежуток времени: {SaveFileList[3]}");

                //Устанавливаем таймер
                Timer timer = new Timer();
                timer.Interval = SaveFileTime;
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
                timer.Start();

                ServiceLogger.WriteEntry($"Таймер запущен с промежутком: {SaveFileTime} ms ({SaveFileList[3]})");

                //Обновление состояния службы до "Running".
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Исключение в методе \u0022OnStart\u0022: " + ex.Message +
                    $"Начальная директория: \u0022{StartDir}\u0022 \n" +
                    $"Конечная директория: \u0022{EndDir}\u0022");
            }
        }

        protected override void OnStop()
        {
            try
            {
                //Обновиление состояния службы до "Stop Pending".
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                ServiceLogger.WriteEntry("Сервис остановлен.");

                // Update the service state to Stopped.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Исключение в методе \u0022OnStop\u0022: " + ex.Message);
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                //Разделение текущей даты на 2 элемента массива "ДАТА и ВРЕМЯ" (Первый элемент — "01.01.2021". Второй элемент "00:00:00")
                string[] dateNow = DateTime.Now.ToString().Split(' ');

                if (!Directory.Exists(EndDir))
                {
                    ServiceLogger.WriteEntry($"Папка \u0022{EndDir}\u0022 не существует. Попытка создать...");
                    DirectoryWork.DirectoryCreate(EndDir);

                    if (Directory.Exists(EndDir))
                    {
                        ServiceLogger.WriteEntry($"Папка \u0022{EndDir}\u0022 создана.");
                    }
                }
                else
                {
                    ServiceLogger.WriteEntry($"Папка \u0022{EndDir}\u0022 уже существует.");
                }

                //Имя папки где будут храниться скопированные файлы
            m1: EndFolder = EndDir + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                if (Directory.Exists(EndFolder))
                {
                    ServiceLogger.WriteEntry($"Папка \u0022{EndFolder}\u0022 уже существует.");

                    FolderVersion++;

                    goto m1;
                }
                else
                {
                    EndFolder = EndDir + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                    ServiceLogger.WriteEntry($"Попытка создать папку \u0022{EndFolder}\u0022");

                    //Создание папки
                    DirectoryWork.DirectoryCreate(EndFolder);

                    ServiceLogger.WriteEntry($"Папка \u0022{EndFolder}\u0022 создана.");
                    ServiceLogger.WriteEntry($"Попытка начать копирование из \u0022{StartDir}\u0022 в \u0022{EndFolder}\u0022...");

                    //Запуск метода копирования
                    DirectoryWork.DirectoryCopy(StartDir, EndFolder, true);

                    FolderVersion = 1;

                    ServiceLogger.WriteEntry("Копирование успешно.");
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry("Исключение в методе \u0022OnTimer\u0022: " + ex.Message);
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
