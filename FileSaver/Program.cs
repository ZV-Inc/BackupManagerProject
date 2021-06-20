using System;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace FileSaver
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            try
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                        new ServiceHelper()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Исключение:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
