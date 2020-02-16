using System.ServiceProcess;

namespace ScraperLinkedInService
{
    public partial class Service : ServiceBase
    {

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnShutdown()
        {
            OnStop();
        }

        protected override void OnStop()
        {

        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
        }
    }
}
