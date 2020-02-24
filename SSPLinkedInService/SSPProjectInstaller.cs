using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace SSPLinkedInService
{
    [RunInstaller(true)]
    public partial class SSPProjectInstaller : System.Configuration.Install.Installer
    {
        public SSPProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
