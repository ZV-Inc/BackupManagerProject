using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public string SaveFile;
        public string SaveFileDirectory;
        public string StartDirectory;
        public string EndDirectory;
        public string BackupFolderDateName;
        public string DefaultSaveFileDirectory;
        public string EndFolder;

        public int FolderVersion = 1;

        ServiceController serviceController = new ServiceController("FileSaverServiceName");
        SaveFileDialog saveFileDialog = new SaveFileDialog();
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

                SaveFileDirectory = $"C:/FSSaves/";

                if (!Directory.Exists(SaveFileDirectory))
                {
                    Directory.CreateDirectory(SaveFileDirectory);
                }

                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveFileDialog.FileName = "FSSave.txt";
                saveFileDialog.DefaultExt = ".txt";
                saveFileDialog.InitialDirectory = SaveFileDirectory;
                saveFileDialog.Title = "Выберите путь сохранения файла";

                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                //Получение информации о дисках в системе
                DriveInfo[] driveInfo = DriveInfo.GetDrives();

                //Запись каждого диска в массив строк
                foreach (DriveInfo strings in driveInfo)
                {
                    DiskList.Items.Add(strings.Name);
                }

                DiskList.SelectedIndex = 0;
                ComboBoxTime.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка при загрузке приложения: {ex.Message}");
            }
        }

        private void lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                System.Windows.Forms.MessageBox.Show($"При выборе начальной папки произошла ошибка: {ex.Message}");
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
                System.Windows.Forms.MessageBox.Show($"При выборе конечной папки произошла ошибка: {ex.Message}");
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
                System.Windows.Forms.MessageBox.Show($"При открытии файла помощи произошла ошибка: {ex.Message}");
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
                System.Windows.Forms.MessageBox.Show($"При открытии окна \u0022О программе\u0022 произошла ошибка: {ex.Message}");
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(SaveFileDirectory + "FSSave.txt"))
                {
                    System.Windows.Forms.MessageBox.Show($"Файл сохранения не найден. Невозможно запустить службу. Убедитесь что в папке \u0022{SaveFileDirectory}\u0022 есть файл \u0022FSSave.txt\u0022.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
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
                System.Windows.Forms.MessageBox.Show($"При попытке запустить службу произошла ошибка: {ex.Message}");
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
                System.Windows.Forms.MessageBox.Show($"При попытке остановить службу произошла ошибка: {ex.Message}");
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

                DialogResult dialogResult = saveFileDialog.ShowDialog();

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    SaveFile = saveFileDialog.FileName.ToString();
                }

                if (dialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                StreamWriter streamWriter = new StreamWriter(SaveFile);

                //Write a line of text
                streamWriter.WriteLine($"Date and time: {DateTime.Now}\n" +
                    $"Start folder: {StartDirectory}\n" +
                    $"End folder: {EndDirectory}\n" +
                    $"Selected time span: {ComboBoxTime.Text}");
                //Close the file
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке сохранить файл произошла ошибка: {ex.Message}");
            }
        }

        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DefaultSaveFileDirectory = $"C:/FSSaves/";

                List<string> SaveFileList = new List<string>();

                string[] SaveReader = File.ReadAllLines(DefaultSaveFileDirectory + "FSSave.txt");
                string[] DateNow = DateTime.Now.ToString().Split();

                string StartDir;
                string EndDir;

                int found = 0;

                foreach (string s in SaveReader)
                {
                    found = s.IndexOf(": ");
                    SaveFileList.Add(s.Substring(found + 2));
                }

                StartDir = SaveFileList[1];
                EndDir = SaveFileList[2];

                MainWindowManager.IsEnabled = false;
                ProgressBarAsync.IsIndeterminate = true;
                ProgressBarAsync.Value = 0;

                MakeBackup(DateNow, StartDir, EndDir);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке начать бэкап произошла ошибка: {ex.Message}");
            }
        }

        async private void MakeBackup(string[] dateNow, string StartDir, string EndDir)
        {
            try
            {
                await Task.Run(() =>
                {
                    BackupFolderDateName = DateTime.Now.ToString();

                    DirectoryWork.DirectoryCreate(EndDir);

                m1: EndFolder = EndDir + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

                    if (Directory.Exists(EndFolder))
                    {
                        FolderVersion++;
                        goto m1;
                    }
                    else
                    {
                        EndFolder = EndDir + "\\" + "Backup-" + dateNow[0] + $"-[{FolderVersion}]";

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
                System.Windows.Forms.MessageBox.Show("Во время копирования файлов произошла ошибка: " + ex.Message);

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
                System.Windows.Forms.MessageBox.Show("При попытке закрыть программу произошла ошибка: " + ex.Message);
            }
        }
    }
}
