using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace FileSaverInterface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string StartDirectory;
        public string EndDirectory;
        public string BackupFolderDateName;
        public string EndFolder;

        public int FolderVersion = 1;

        RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\FileSaver");
        ServiceController serviceController = new ServiceController("FileSaverServiceName");
        EventLog ServiceLogger = new EventLog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceController.GetServices();

                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                //Получение информации о дисках в системе
                DriveInfo[] driveInfo = DriveInfo.GetDrives();

                //Запись каждого диска в массив строк
                foreach (DriveInfo strings in driveInfo)
                {
                    DiskList.Items.Add(strings.Name);
                }
                if (registryKey.GetValue("Start Directory",))
                {

                }

                using (registryKey.OpenSubKey(@"Software\WOW6432Node\FileSaver"))
                {
                    registryKey.SetValue("Start Directory", "");
                    registryKey.SetValue("End Directory", "");
                    registryKey.SetValue("Selected time span", "");
                }

                DiskList.SelectedIndex = 0;
                ComboBoxTime.SelectedIndex = 0;
                RegistryInfoTextBox_Changed();
                
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка при загрузке приложения: {ex.Message}");
            }
        }

        private void DiskInfoTextBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Вывод информации о дисках в TextBox
                DiskInfoTextBox.Text = "";
                DriveInfo driveInfo = new DriveInfo(DiskList.SelectedItem.ToString());
                DiskInfoTextBox.Text = "Свободное пространство: " + driveInfo.AvailableFreeSpace / 1024 / 1024 + " MB\n"
                    + "Общий размер: " + driveInfo.TotalSize / 1024 / 1024 + " MB\n"
                    + "Формат устройства: " + driveInfo.DriveFormat + "\n"
                    + "Тип устройства: " + driveInfo.DriveType + "\n"
                    + "Готовность: " + driveInfo.IsReady + "\n"
                    + "Имя " + driveInfo.Name
                    + "\nКорневой каталог: " + driveInfo.RootDirectory +
                    "\nМетка тома: " + driveInfo.VolumeLabel;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При загрузке информации о дисках произошла ошибка: {ex.Message}");
            }
        }

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
                  $"Промежуток времени:\n {registryKey.GetValue("Selected time span")}";
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При загрузке информации из реестра произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void OverViewStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog browserDialog = new FolderBrowserDialog();

                OverViewStart.IsEnabled = false;

                browserDialog.ShowDialog();

                OverViewStartTextBox.Text = browserDialog.SelectedPath;

                OverViewStart.IsEnabled = true;

                StartDirectory = OverViewStartTextBox.Text;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При выборе начальной директории произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OverViewEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var browserDialog = new FolderBrowserDialog();

                OverViewEnd.IsEnabled = false;

                browserDialog.ShowDialog();

                OverViewEndTextBox.Text = browserDialog.SelectedPath;

                OverViewEnd.IsEnabled = true;

                EndDirectory = OverViewEndTextBox.Text;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При выборе конечной дериктории произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (registryKey.GetValue("Start Directory") == null || registryKey.GetValue("End Directory") == null || registryKey.GetValue("Selected time span") == null)
                {
                    System.Windows.Forms.MessageBox.Show($"Не удалось найти сохраненные папки или одно из значений пустое.\n\n" +
                        $"Start Directory: {StartDirectory}\n" +
                        $"End Directory: {EndDirectory}\n" +
                        $"Selected time span: {registryKey.GetValue("Selected time span")}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    serviceController.ServiceName = "FileSaverServiceName";
                    serviceController.Refresh();

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

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serviceController.ServiceName = "FileSaverServiceName";
                serviceController.Refresh();

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

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartDirectory = OverViewStartTextBox.Text;
                EndDirectory = OverViewEndTextBox.Text;

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
                registryKey.SetValue("Selected time span", ComboBoxTime.Text, RegistryValueKind.String);

                System.Windows.Forms.MessageBox.Show($"Сохранение успешно. Сохраненные параметры:\n" +
                    $"Start Directory: {StartDirectory}\n" + //Информация получаемая в эту строчку должа быть из реестра
                    $"End Directory: {EndDirectory}\n" +//Информация получаемая в эту строчку должа быть из реестра
                    $"Selected time span: {ComboBoxTime.Text}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);//Информация получаемая в эту строчку должа быть из реестра

                RegistryInfoTextBox_Changed();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке сохранить файл произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string DateNow = DateTime.Now.ToString().Split(' ')[0];

                StartDirectory = registryKey.GetValue("Start Directory").ToString();
                EndDirectory = registryKey.GetValue("End Directory").ToString();

                MainWindowManager.IsEnabled = false;
                ProgressBarAsync.IsIndeterminate = true;
                ProgressBarAsync.Value = 0;

                MakeBackup(DateNow, StartDirectory, EndDirectory);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке начать бэкап произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        async private void MakeBackup(string dateNow, string StartDir, string EndDir)
        {
            try
            {
                await Task.Run(() =>
                {
                    BackupFolderDateName = DateTime.Now.ToString();

                    DirectoryWork.DirectoryCreate(EndDir);

                m1: EndFolder = EndDir + "\\" + "Backup-" + dateNow + $"-[{FolderVersion}]";

                    if (Directory.Exists(EndFolder))
                    {
                        FolderVersion++;
                        goto m1;
                    }
                    else
                    {
                        EndFolder = EndDir + "\\" + "Backup-" + dateNow + $"-[{FolderVersion}]";

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
    }
}
