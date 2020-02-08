namespace ScraperLinkedInService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ScraperLinkedInProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ScraperLinkedInInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ScraperLinkedInProcessInstaller
            // 
            this.ScraperLinkedInProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.ScraperLinkedInProcessInstaller.Password = null;
            this.ScraperLinkedInProcessInstaller.Username = null;
            // 
            // ScraperLinkedInInstaller
            // 
            this.ScraperLinkedInInstaller.DelayedAutoStart = true;
            this.ScraperLinkedInInstaller.DisplayName = "ScraperLinkedInService";
            this.ScraperLinkedInInstaller.ServiceName = "ScraperLinkedInService";
            this.ScraperLinkedInInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ScraperLinkedInProcessInstaller,
            this.ScraperLinkedInInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ScraperLinkedInProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ScraperLinkedInInstaller;
    }
}