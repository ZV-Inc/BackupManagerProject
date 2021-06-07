using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
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
        ServiceController serviceController = new ServiceController("FileSaverServiceName");
        //Получает название лога и его ресурс, для обращения к логу программы.
        EventLog ServiceLogger = new EventLog("FileSaverServiceLog", ".", "FileSaverServiceSource");
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
                //ServiceController получает имена всех служб.
                ServiceController.GetServices();

                //Получение информации о дисках в системе.
                DriveInfo[] driveInfo = DriveInfo.GetDrives();

                //Запись каждого диска в массив строк.
                foreach (DriveInfo strings in driveInfo)
                {
                    DiskList.Items.Add(strings.Name);
                }
                //Проверка на наличие директории.
                if (registryKey.GetValue("Start Directory") == null || registryKey.GetValue("End Directory") == null || registryKey.GetValue("Time span") == null)
                {
                    using (registryKey.OpenSubKey(@"Software\WOW6432Node\FileSaver"))
                    {
                        registryKey.SetValue("Start Directory", "");
                        registryKey.SetValue("End Directory", "");
                        registryKey.SetValue("Time span", "");
                    }
                }

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
                    serviceController.ServiceName = "FileSaverServiceName";
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
                serviceController.ServiceName = "FileSaverServiceName";
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

                MakeBackup(StartDirectory, EndDirectory);
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
                DiskInfoTextBox.Text = $"Свободное пространство: {driveInfo.AvailableFreeSpace / 1024 / 1024} MB\n"
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
        async private void MakeBackup(string StartDir, string EndDir)
        {
            try
            {
                string EndFolder;
                string dateTime = DateTime.Now.ToString().Split(' ')[0];

                int FolderVersion = 1;

                await Task.Run(() =>
                        {
                            DirectoryWork.DirectoryCreate(EndDir);

                        m1: EndFolder = EndDir + "\\" + "Backup-" + dateTime + $"-[{FolderVersion}]";

                            if (Directory.Exists(EndFolder))
                            {
                                FolderVersion++;
                                goto m1;
                            }
                            else
                            {
                                EndFolder = EndDir + "\\" + "Backup-" + dateTime + $"-[{FolderVersion}]";

                                DirectoryWork.DirectoryCreate(EndFolder);

                                DirectoryWork.DirectoryCopy(StartDir, EndFolder, true);

                                FolderVersion = 1;
                            }
                        });

                ProgressBarAsync.IsIndeterminate = false;
                ProgressBarAsync.Value = 100;
                System.Windows.Forms.MessageBox.Show("Копирование успешно.");
                MainWindowManager.IsEnabled = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Во время копирования файлов произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                MainWindowManager.IsEnabled = true;
            }
        }
    }
}
