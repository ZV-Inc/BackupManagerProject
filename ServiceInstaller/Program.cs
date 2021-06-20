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
            //Возвращает true или false в зависимости от существования службы.
            string _serviceIsInstalled = ServiceController.GetServices().Any(s => s.ServiceName == "FileSaverService") ? "--uninstall" : "--install";
            //Получает путь к каталогу из которого запускалась программа.
            var _servicePath = Directory.GetCurrentDirectory() + "\\FileSaverService.exe";

            switch (_serviceIsInstalled)
            {
                case "--install":
                    try
                    {
                        //Запуск утилиты "InstallUtil.exe" с заданными параметрами. Для установки службы.
                        ManagedInstallerClass.InstallHelper(new string[] { _servicePath });
                        MessageBox.Show($"Служба \"File Saver Service\" установлена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", _servicePath });
                        MessageBox.Show($"Служба \"File Saver Service\" удалена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
