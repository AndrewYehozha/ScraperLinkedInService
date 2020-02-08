using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ScraperLinkedInService
{
    public partial class Service : ServiceBase
    {
        private Scraper scraper;

        public Service()
        {
            scraper = new Scraper();
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
            scraper.Close();
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
        }
    }
}
