using System.ServiceProcess;

namespace FileSaver
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
                new ServiceHelper()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
