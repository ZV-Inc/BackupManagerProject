using System;
using System.ServiceProcess;

namespace FileSaverService
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args !=null && args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "--install":
                            try
                            {
                                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { appPath });
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            break;

                        case "--uninstall":
                            try
                            {
                                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] {"/u", appPath });
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            break;
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new FileSaverService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
