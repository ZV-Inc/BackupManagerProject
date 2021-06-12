using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Win32;

namespace FileSaverInterface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string StartDirectory;
        public string EndDirectory;
        //Создание раздела в реестре.
        RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\FileSaver");
        //Получает имя нужного сериса для работы.
        ServiceController serviceController = new ServiceController("FileSaverService");
        //Получает название лога и его ресурс, для обращения к логу программы.
        EventLog ServiceLogger = new EventLog();
        //Вызывает диалоговое окно с выбором папки.
        FolderBrowserDialog browserDialog = new FolderBrowserDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Если журнал с ссылкой не существует, то создаёт его
                if (!EventLog.SourceExists("FileSaverServiceSource"))
                {
                    EventLog.CreateEventSource("FileSaverServiceSource", "FileSaverServiceLog");
                }

                //ServiceController получает имена всех служб.
                ServiceController.GetServices();

                //Получение информации о дисках в системе.
                DriveInfo[] driveInfo = DriveInfo.GetDrives();

                //Запись каждого диска в массив строк.
                foreach (DriveInfo strings in driveInfo)
                {
                    DiskList.Items.Add(strings.Name);
                }
                //Проверка на наличие директорий.
                if (registryKey.GetValue("Start Directory") == null || registryKey.GetValue("End Directory") == null || registryKey.GetValue("Time span") == null)
                {
                    using (registryKey.OpenSubKey(@"Software\WOW6432Node\FileSaver"))
                    {
                        registryKey.SetValue("Start Directory", "");
                        registryKey.SetValue("End Directory", "");
                        registryKey.SetValue("Time span", "");
                    }
                }

                OutputTypeComboBox.SelectedIndex = 0;
                DiskList.SelectedIndex = 0;
                ComboBoxTime.SelectedIndex = 0;
                RegistryInfoTextBox_Changed();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка при загрузке программы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработчик событий кнопки "обзор" для стартовой директории.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewStart.IsEnabled = false;
                
                browserDialog.ShowDialog();

                ViewStartTextBox.Text = browserDialog.SelectedPath;

                ViewStart.IsEnabled = true;

                StartDirectory = ViewStartTextBox.Text;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При выборе начальной директории произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработчик событий кнопки "обзор" для конечной директории.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewEnd.IsEnabled = false;

                browserDialog.ShowDialog();

                ViewEndTextBox.Text = browserDialog.SelectedPath;

                ViewEnd.IsEnabled = true;

                EndDirectory = ViewEndTextBox.Text;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При выборе конечной дериктории произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработчик событий для кнопки "Запустить сервис".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                // Проверка параметров на наличие заданых значений.
                if (registryKey.GetValue("Start Directory") == null || registryKey.GetValue("End Directory") == null || registryKey.GetValue("Time span") == null)
                {
                    System.Windows.Forms.MessageBox.Show($"Не удалось найти сохраненные папки или одно из значений пустое.\n\n" +
                        $"Start Directory: {StartDirectory}\n" +
                        $"End Directory: {EndDirectory}\n" +
                        $"Time span: {registryKey.GetValue("Time span")}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    serviceController.ServiceName = "FileSaverService";
                    serviceController.Refresh();
                    //Если служба уже запущена будет выведена ошибка.
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        System.Windows.Forms.MessageBox.Show($"Не удалось запустить службу. Служба {serviceController.DisplayName} уже запущена.\n\n" +
                            $"Текущий статус службы: {serviceController.Status}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        serviceController.Refresh();
                    }
                    else
                    {
                        serviceController.Start();
                        serviceController.Refresh();
                        System.Windows.Forms.MessageBox.Show($"Служба запущена.\n" +
                            $"Текущий статус службы: {serviceController.Status}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ServiceLogger.WriteEntry("Служба запущена с помощью программы");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке запустить службу произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработка события кнопки "Остановить сервис".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serviceController.ServiceName = "FileSaverService";
                serviceController.Refresh();
                //Если сервис уже остановлен, будет выведена ошибка.
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    System.Windows.Forms.MessageBox.Show($"Не удалось остановить службу. Служба {serviceController.DisplayName} в данный момент остановлена.\n\n" +
                        $"Текущий статус службы: {serviceController.Status}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    serviceController.Refresh();
                }
                else
                {
                    serviceController.Stop();
                    serviceController.Refresh();
                    System.Windows.Forms.MessageBox.Show($"Служба остановлена.\n" +
                        $"Текущий статус службы: {serviceController.Status}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ServiceLogger.WriteEntry("Служба остановлена с помощью программы");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке остановить службу произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Кнопка сохранения текущих настроек бэкапа в директорию.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartDirectory = ViewStartTextBox.Text;
                EndDirectory = ViewEndTextBox.Text;

                if (!Directory.Exists(StartDirectory) || StartDirectory.Length <= 4)
                {
                    System.Windows.Forms.MessageBox.Show($"Директория {StartDirectory} не найдена или указана неверно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Directory.Exists(EndDirectory) || EndDirectory.Length <= 4)
                {
                    System.Windows.Forms.MessageBox.Show($"Директория {EndDirectory} не найдена или указана неверно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                registryKey.SetValue("Start Directory", StartDirectory, RegistryValueKind.String);
                registryKey.SetValue("End Directory", EndDirectory, RegistryValueKind.String);
                registryKey.SetValue("Time span", ComboBoxTime.Text, RegistryValueKind.String);

                System.Windows.Forms.MessageBox.Show($"Сохранение успешно. Сохраненные параметры:\n" +
                    $"Start Directory: {StartDirectory}\n" + //Информация получаемая в эту строчку должа быть из реестра
                    $"End Directory: {EndDirectory}\n" +//Информация получаемая в эту строчку должа быть из реестра
                    $"Time span: {ComboBoxTime.Text}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);//Информация получаемая в эту строчку должа быть из реестра

                RegistryInfoTextBox_Changed();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке сохранить файл произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработчик событий кнопки бэкап.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartDirectory = registryKey.GetValue("Start Directory").ToString();
                EndDirectory = registryKey.GetValue("End Directory").ToString();

                MainWindowManager.IsEnabled = false;
                ProgressBarAsync.IsIndeterminate = true;
                ProgressBarAsync.Value = 0;

                switch (OutputTypeComboBox.SelectedIndex)
                {
                    case 0:
                        ZipArviceBackupType(StartDirectory, EndDirectory);
                        break;

                    case 1:
                        OverwriteFolderBackupType(StartDirectory, EndDirectory);
                        break;

                    case 2:
                        SingeFolderBackupType(StartDirectory, EndDirectory);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке начать бэкап произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Кнопка вызова Help файла.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Help.ShowHelp(null, "DBMHELP.chm");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При открытии файла помощи произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Кнопка вызова информации "О программе".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutProgramButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Ссылка на дочернее окно
                AboutProgram aboutProgram = new AboutProgram();

                //Показать дочернее окно
                aboutProgram.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При открытии окна \u0022О программе\u0022 произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработчик событий проверки дисков на наличие, и характеристики.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DiskInfoTextBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Вывод информации о дисках в TextBox.
                DriveInfo driveInfo = new DriveInfo(DiskList.SelectedItem.ToString());
                DiskInfoTextBox.Text = $"Свободное пространство: {driveInfo.AvailableFreeSpace / 1048576} MB\n"
                    + $"Общий размер: {driveInfo.TotalSize / 1024 / 1024} MB\n"
                    + $"Формат устройства: {driveInfo.DriveFormat}\n"
                    + $"Тип устройства: {driveInfo.DriveType}\n"
                    + $"Готовность: {driveInfo.IsReady}\n"
                    + $"Имя: {driveInfo.Name}\n"
                    + $"Корневой каталог: {driveInfo.RootDirectory}\n"
                    + $"Метка тома: {driveInfo.VolumeLabel}";
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При загрузке информации о дисках произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Обработчик событий проверки регистра, вывод данных регистра
        /// </summary>
        private void RegistryInfoTextBox_Changed()
        {
            try
            {
                if (registryKey.GetValue("Start Directory").ToString().Length == 0)
                {
                    RegistryInfoTextBox.Text = $"Не удалось получить данные (Возможно данные отсутствуют)";
                }
                else
                {
                    RegistryInfoTextBox.Text = $"Начальная директория:\n {registryKey.GetValue("Start Directory")}\n" +
                  $"Конечная директория:\n {registryKey.GetValue("End Directory")}\n" +
                  $"Промежуток времени:\n {registryKey.GetValue("Time span")}";
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При загрузке информации из реестра произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        /// <summary>
        /// Обработчик событий кнопки "Выход". Закрывает программу.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitProgrammButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("При попытке закрыть программу произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Метод создания бэкапа, выполняет копирование.
        /// </summary>
        /// <param name="StartDir"></param>
        /// <param name="EndDir"></param>

        async private void ZipArviceBackupType(string StartDir, string EndDir)
        {
            try
            {
                await Task.Run(() =>
                {
                    string EndZip;
                    string dateTime = DateTime.Now.ToString().Split(' ')[0];

                    int ZipVersion = 1;

                    if (!Directory.Exists(EndDir))
                    {
                        DirectoryWork.DirectoryCreate(EndDir);
                    }

                m1: EndZip = EndDir + "\\" + "Backup-" + dateTime + $"-[{ZipVersion}]";

                    if (File.Exists($"{EndZip}.zip"))
                    {
                        ZipVersion++;
                        goto m1;
                    }
                    else
                    {
                        EndZip = EndDir + "\\" + "Backup-" + dateTime + $"-[{ZipVersion}]";

                        ZipFile.CreateFromDirectory(StartDir, $"{EndZip}.zip");

                        ZipVersion = 1;
                    }
                });

                MainWindowManager.IsEnabled = true;
                ProgressBarAsync.IsIndeterminate = false;
                ProgressBarAsync.Value = 100;
                System.Windows.Forms.MessageBox.Show("Создание Zip Архива успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке создать архив произошла ошибка:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        async private void OverwriteFolderBackupType(string StartDir, string EndDir)
        {
            try
            {
                await Task.Run(() =>
                {
                    string EndOverwriteFolder;
                    string dateTime = DateTime.Now.ToString().Split(' ')[0];

                    int OverwriteFolderVersion = 1;

                    if (!Directory.Exists(EndDir))
                    {
                        DirectoryWork.DirectoryCreate(EndDir);
                    }

                m1: EndOverwriteFolder = EndDir + "\\" + "Backup-" + dateTime + $"-[{OverwriteFolderVersion}]";

                    if (Directory.Exists(EndOverwriteFolder))
                    {
                        OverwriteFolderVersion++;
                        goto m1;
                    }
                    else
                    {
                        EndOverwriteFolder = EndDir + "\\" + "Backup-" + dateTime + $"-[{OverwriteFolderVersion}]";

                        DirectoryWork.DirectoryCreate(EndOverwriteFolder);
                        DirectoryWork.DirectoryCopy(StartDir, EndOverwriteFolder, true);

                        OverwriteFolderVersion = 1;
                    }
                });

                MainWindowManager.IsEnabled = true;
                ProgressBarAsync.IsIndeterminate = false;
                ProgressBarAsync.Value = 100;
                System.Windows.Forms.MessageBox.Show("Папка создана успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке создать архив произошла ошибка:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        async private void SingeFolderBackupType(string StartDir, string EndDir)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(EndDir))
                    {
                        Directory.Delete(EndDir, true);
                        DirectoryWork.DirectoryCreate(EndDir);
                        DirectoryWork.DirectoryCopy(StartDir, EndDir, true);
                    }
                    else
                    {
                        DirectoryWork.DirectoryCreate(EndDir);
                        DirectoryWork.DirectoryCopy(StartDir, EndDir, true);
                    }
                });

                MainWindowManager.IsEnabled = true;
                ProgressBarAsync.IsIndeterminate = false;
                ProgressBarAsync.Value = 100;
                System.Windows.Forms.MessageBox.Show("Папка создана успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке создать архив произошла ошибка:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
