using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Windows.Controls;
using Microsoft.Win32;
using BackupManagerLib;
using System.ComponentModel;

namespace BackupManagerInterface
{
    public partial class MainWindow : Window
    {
        private string _startDirectory;
        private string _endDirectory;

        private RegistryKey _registryKey = Registry.LocalMachine.CreateSubKey(@"Software\WOW6432Node\BackupManager");
        private ServiceController _serviceController = new ServiceController("BackupManagerService");
        private EventLog _serviceLogger = new EventLog();
        private FolderBrowserDialog _browserDialog = new FolderBrowserDialog();
        private BackupType _backupTypes = new BackupType();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!EventLog.SourceExists("BackupManagerServiceSource"))
                {
                    EventLog.CreateEventSource("BackupManagerServiceSource", "BackupManagerServiceLog");
                }

                ServiceController.GetServices();
                DriveInfo[] driveInfo = DriveInfo.GetDrives();

                foreach (DriveInfo strings in driveInfo)
                {
                    DiskList.Items.Add(strings.Name);
                }

                if (_registryKey.GetValue("Start Directory") == null || _registryKey.GetValue("End Directory") == null || _registryKey.GetValue("Time span") == null)
                {
                    using (_registryKey.OpenSubKey(@"Software\WOW6432Node\BackupManager"))
                    {
                        _registryKey.SetValue("Start Directory", "");
                        _registryKey.SetValue("End Directory", "");
                        _registryKey.SetValue("Time span", "");
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

        private void ViewStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewStart.IsEnabled = false;

                _browserDialog.ShowDialog();

                ViewStartTextBox.Text = _browserDialog.SelectedPath;

                ViewStart.IsEnabled = true;

                _startDirectory = ViewStartTextBox.Text;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При выборе начальной директории произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewEnd.IsEnabled = false;

                _browserDialog.ShowDialog();

                ViewEndTextBox.Text = _browserDialog.SelectedPath;

                ViewEnd.IsEnabled = true;

                _endDirectory = ViewEndTextBox.Text;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При выборе конечной дериктории произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_registryKey.GetValue("Start Directory") == null || _registryKey.GetValue("End Directory") == null || _registryKey.GetValue("Time span") == null)
                {
                    System.Windows.Forms.MessageBox.Show($"Не удалось найти сохраненные папки или одно из значений пустое.\n\n" +
                        $"Start Directory: {_startDirectory}\n" +
                        $"End Directory: {_endDirectory}\n" +
                        $"Time span: {_registryKey.GetValue("Time span")}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _serviceController.ServiceName = "BackupManagerService";
                    _serviceController.Refresh();

                    if (_serviceController.Status == ServiceControllerStatus.Running)
                    {
                        System.Windows.Forms.MessageBox.Show($"Не удалось запустить службу. Служба {_serviceController.DisplayName} уже запущена.\n\n" +
                            $"Текущий статус службы: {_serviceController.Status}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _serviceController.Refresh();
                    }
                    else
                    {
                        _serviceController.Start();
                        _serviceController.Refresh();
                        System.Windows.Forms.MessageBox.Show($"Служба запущена.\n" +
                            $"Текущий статус службы: {_serviceController.Status}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _serviceLogger.WriteEntry("Служба запущена с помощью программы");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке запустить службу произошла ошибка: {ex.Message}\n\nВозможно служба не установлена, для её установки используйте \"ServiceInstaller.exe\"", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _serviceController.ServiceName = "BackupManagerService";
                _serviceController.Refresh();

                if (_serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    System.Windows.Forms.MessageBox.Show($"Не удалось остановить службу. Служба {_serviceController.DisplayName} в данный момент остановлена.\n\n" +
                        $"Текущий статус службы: {_serviceController.Status}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _serviceController.Refresh();
                }
                else
                {
                    _serviceController.Stop();
                    _serviceController.Refresh();
                    System.Windows.Forms.MessageBox.Show($"Служба остановлена.\n" +
                        $"Текущий статус службы: {_serviceController.Status}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _serviceLogger.WriteEntry("Служба остановлена с помощью программы");
                }
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке запустить службу произошла ошибка: {ex.Message}\n\nВозможно служба не установлена, для её установки используйте \"ServiceInstaller.exe\"", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _startDirectory = ViewStartTextBox.Text;
                _endDirectory = ViewEndTextBox.Text;

                if (!Directory.Exists(_startDirectory) || _startDirectory.Length <= 4)
                {
                    System.Windows.Forms.MessageBox.Show($"Директория {_startDirectory} не найдена или указана неверно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Directory.Exists(_endDirectory) || _endDirectory.Length <= 4)
                {
                    System.Windows.Forms.MessageBox.Show($"Директория {_endDirectory} не найдена или указана неверно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _registryKey.SetValue("Start Directory", _startDirectory, RegistryValueKind.String);
                _registryKey.SetValue("End Directory", _endDirectory, RegistryValueKind.String);
                _registryKey.SetValue("Time span", ComboBoxTime.Text, RegistryValueKind.String);

                System.Windows.Forms.MessageBox.Show($"Сохранение успешно. Сохраненные параметры:\n" +
                    $"Start Directory: {_startDirectory}\n" +
                    $"End Directory: {_endDirectory}\n" +
                    $"Time span: {ComboBoxTime.Text}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                _startDirectory = _registryKey.GetValue("Start Directory").ToString();
                _endDirectory = _registryKey.GetValue("End Directory").ToString();

                MainWindowManager.IsEnabled = false;
                ProgressBarAsync.IsIndeterminate = true;
                ProgressBarAsync.Value = 0;

                switch (OutputTypeComboBox.SelectedIndex)
                {
                    case 0:
                        _backupTypes.ZipArviceBackupType(_startDirectory, _endDirectory);

                        MainWindowManager.IsEnabled = true;
                        ProgressBarAsync.IsIndeterminate = false;
                        ProgressBarAsync.Value = 100;
                        System.Windows.Forms.MessageBox.Show("Создание Zip Архива успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;

                    case 1:
                        _backupTypes.OverwriteFolderBackupType(_startDirectory, _endDirectory);

                        MainWindowManager.IsEnabled = true;
                        ProgressBarAsync.IsIndeterminate = false;
                        ProgressBarAsync.Value = 100;
                        System.Windows.Forms.MessageBox.Show("Папка создана успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;

                    case 2:
                        _backupTypes.SingeFolderBackupType(_startDirectory, _endDirectory);

                        MainWindowManager.IsEnabled = true;
                        ProgressBarAsync.IsIndeterminate = false;
                        ProgressBarAsync.Value = 100;
                        System.Windows.Forms.MessageBox.Show("Папка создана успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При попытке начать бэкап произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                AboutProgram aboutProgram = new AboutProgram();
                aboutProgram.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При открытии окна \u0022О программе\u0022 произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DiskInfoTextBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
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

        private void RegistryInfoTextBox_Changed()
        {
            try
            {
                if (_registryKey.GetValue("Start Directory").ToString().Length == 0)
                {
                    RegistryInfoTextBox.Text = $"Отсутствуют.";
                }
                else
                {
                    RegistryInfoTextBox.Text = $"Начальная директория:\n {_registryKey.GetValue("Start Directory")}\n" +
                  $"Конечная директория:\n {_registryKey.GetValue("End Directory")}\n" +
                  $"Промежуток времени:\n {_registryKey.GetValue("Time span")}";
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"При загрузке информации из реестра произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
