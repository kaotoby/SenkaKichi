using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SenkaKichi.WcfService
{
    public partial class WindowsService : ServiceBase
    {
        public ServiceHost serviceHost = null;
        private static readonly ILog log = LogManager.GetLogger(typeof(WindowsService).FullName);

        public WindowsService() {
            InitializeComponent();
        }

        public void OnDebug() {
            OnStart(null);
        }

        protected override void OnStart(string[] args) {
            log.Info("[Started] SenkaKichi.WcfService");
            if (serviceHost != null) {
                serviceHost.Close();
            }
            ServiceManager.Current.StartTimer();
            // Create a ServiceHost and provide the base address.
            serviceHost = new ServiceHost(typeof(Service));

            // Open the ServiceHostBase to create listeners and start listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop() {
            log.Warn("[Stopped] SenkaKichi.WcfService");

            if (serviceHost != null) {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
