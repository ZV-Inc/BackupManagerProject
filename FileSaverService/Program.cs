using System;
using System.Linq;
using System.ServiceProcess;

namespace FileSaverService
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            bool serviceExists;
            try
            {
                if (serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == "FileSaverService"))
                {
                    var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", appPath });
                }
                else
                {
                    var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { appPath });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка:\n{ex.Message}");
            }
            /*
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new FileSaverService()
            };
            ServiceBase.Run(ServicesToRun);
            */
        }
    }
}
