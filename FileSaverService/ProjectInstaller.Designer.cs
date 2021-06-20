
namespace FileSaverService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.FileSaverServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.FileSaverServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // FileSaverServiceProcessInstaller
            // 
            this.FileSaverServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.FileSaverServiceProcessInstaller.Password = null;
            this.FileSaverServiceProcessInstaller.Username = null;
            // 
            // FileSaverServiceInstaller
            // 
            this.FileSaverServiceInstaller.Description = "File Saver Service V0.4.5";
            this.FileSaverServiceInstaller.DisplayName = "File Saver Service";
            this.FileSaverServiceInstaller.ServiceName = "FileSaverService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.FileSaverServiceProcessInstaller,
            this.FileSaverServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller FileSaverServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller FileSaverServiceInstaller;
    }
}