namespace PosInfoCollection
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.PosInfoCollectionServiceInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // PosInfoCollectionServiceInstaller
            // 
            this.PosInfoCollectionServiceInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.PosInfoCollectionServiceInstaller.Password = null;
            this.PosInfoCollectionServiceInstaller.Username = null;
            // 
            // ServiceInstaller
            // 
            this.ServiceInstaller.Description = "POS机信息收集服务";
            this.ServiceInstaller.DisplayName = "PosInfoCollection";
            this.ServiceInstaller.ServiceName = "PosInfoCollection";
            this.ServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.PosInfoCollectionServiceInstaller,
            this.ServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller PosInfoCollectionServiceInstaller;
        private System.ServiceProcess.ServiceInstaller ServiceInstaller;
    }
}