using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
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

        AboutProgramm aboutProgramm = new AboutProgramm();
        ServiceController serviceController = new ServiceController("FileSaverServiceName");
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        EventLog ServiceLogger = new EventLog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            ServiceController.GetServices();
        }

        private void lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void OverViewStart_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();

            OverViewStart.IsEnabled = false;

            browserDialog.ShowDialog();

            OverViewStartTextBox.Text = browserDialog.SelectedPath;

            OverViewStart.IsEnabled = true;

            StartDirectory = OverViewStartTextBox.Text;
        }

        private void OverViewEnd_Click(object sender, RoutedEventArgs e)
        {
            var browserDialog = new FolderBrowserDialog();

            OverViewEnd.IsEnabled = false;

            browserDialog.ShowDialog();

            OverViewEndTextBox.Text = browserDialog.SelectedPath;

            OverViewEnd.IsEnabled = true;

            EndDirectory = OverViewEndTextBox.Text;
        }

        private void ButtonHelp_Click_1(object sender, RoutedEventArgs e)
        {
            Help.ShowHelp(null, "DBMHELP.chm");
        }

        private void AboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            //Ссылка на дочернее окно
            AboutUsWindow aboutUsWindow = new AboutUsWindow();

            //Показать дочернее окно
            aboutUsWindow.ShowDialog();
        }

        private void AboutProgramm_Click(object sender, RoutedEventArgs e)
        {
            aboutProgramm.ShowDialog();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(SaveFileDirectory + "FSSave.txt"))
            {
                System.Windows.Forms.MessageBox.Show($"Save file don't exists. Can't start a service. Make save file in folder \u0022{SaveFileDirectory}\u0022 to start service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }
            else
            {
                serviceController.ServiceName = "FileSaverServiceName";
                serviceController.Refresh();

                if (!Directory.Exists(StartDirectory) || StartDirectory.Length <= 3)
                {
                    System.Windows.Forms.MessageBox.Show($"Директория {StartDirectory} не найдена или указана неверно", "Ошибка");
                    return;
                }

                if (!Directory.Exists(EndDirectory) || EndDirectory.Length <= 3)
                {
                    System.Windows.Forms.MessageBox.Show($"Директория {EndDirectory} не найдена или указана неверно", "Ошибка");
                    return;
                }

                try
                {
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        System.Windows.Forms.MessageBox.Show($"Служба {serviceController.DisplayName} уже запущена.\n" +
                            $"Service state: {serviceController.Status}", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        serviceController.Refresh();
                    }
                    else
                    {
                        serviceController.Start();
                        serviceController.Refresh();
                        System.Windows.Forms.MessageBox.Show($"Service sarted.\n" +
                            $"Service State: {serviceController.Status}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ServiceLogger.WriteEntry("Service Started from interface");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Some Exception: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            serviceController.ServiceName = "FileSaverServiceName";
            serviceController.Refresh();

            try
            {
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    System.Windows.Forms.MessageBox.Show($"Не удалось остановить службу {serviceController.DisplayName}. В данный момент она не запущена.\n" +
                        $"Service state: {serviceController.Status}", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    serviceController.Refresh();
                }
                else
                {
                    serviceController.Stop();
                    serviceController.Refresh();
                    System.Windows.Forms.MessageBox.Show($"Service stopped.\n" +
                        $"Service State: {serviceController.Status}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ServiceLogger.WriteEntry("Service Stopped from interface");
                }
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Some Exception: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                    $"Start folder: {OverViewStartTextBox.Text}\n" +
                    $"End folder: {OverViewEndTextBox.Text}\n" +
                    $"Selected time span: {ComboBoxTime.Text}\n" +
                    $"Current user: {Environment.UserName}");
                //Close the file
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Exception: " + ex.Message, "Error");
            }
        }

        private void ExitProgrammButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }
    }
}
