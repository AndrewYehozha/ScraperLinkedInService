namespace SSPLinkedInService
{
    partial class SSPProjectInstaller
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
            this.SSPServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SSPServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // SSPServiceProcessInstaller
            // 
            this.SSPServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.SSPServiceProcessInstaller.Password = null;
            this.SSPServiceProcessInstaller.Username = null;
            // 
            // SSPServiceInstaller
            // 
            this.SSPServiceInstaller.DelayedAutoStart = true;
            this.SSPServiceInstaller.Description = "Search suitable profiles service";
            this.SSPServiceInstaller.DisplayName = "SSPProjectInstaller";
            this.SSPServiceInstaller.ServiceName = "SSPProjectInstaller";
            this.SSPServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // SSPProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SSPServiceProcessInstaller,
            this.SSPServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller SSPServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller SSPServiceInstaller;
    }
}