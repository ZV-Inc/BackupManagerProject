using System.ServiceProcess;

namespace BackupManagerService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new BackupManagerService()
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
