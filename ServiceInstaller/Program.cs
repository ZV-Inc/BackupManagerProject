using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Configuration.Install;

namespace ServiceInstaller
{
    class Program
    {
        static void Main()
        {
            //Получение имен всех установленных служб в системе.
            ServiceController[] _serviceController = ServiceController.GetServices();
            //Проверка на существование службы "FileSaverServiceHelper".
            bool ServiceIsInstalled = _serviceController.Any(s => s.ServiceName == "FileSaverServiceHelper");
            //Возвращает аргумент в зависимости если служба существует.
            string _installArgument = ServiceIsInstalled ? "--uninstall" : "--install";
            //Получает путь к каталогу из которого запускалась программа.
            var _folderPath = Directory.GetCurrentDirectory();

            switch (_installArgument)
            {
                case "--install":
                    try
                    {
                        //Запуск утилиты "InstallUtil.exe" с заданными параметрами. Для установки службы.
                        ManagedInstallerClass.InstallHelper(new string[] { $"{_folderPath}\\FileSaver.exe" });
                        MessageBox.Show($"Служба \"File Saver Helper\" установлена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;

                case "--uninstall":
                    try
                    {
                        //Запуск утилиты "InstallUtil.exe" с заданными параметрами. Для удаления службы.
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", $"{_folderPath}\\FileSaver.exe" });
                        MessageBox.Show($"Служба \"File Saver Helper\" удалена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
            }
        }
    }
}
