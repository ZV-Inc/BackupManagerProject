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
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new FileSaverService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
