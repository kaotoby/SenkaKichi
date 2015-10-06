using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SenkaKichi.WcfService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
#if DEBUG
            WindowsService _service = new WindowsService();
            _service.OnDebug();
            Thread.Sleep(Timeout.Infinite);
#else
            ServiceBase.Run(new WindowsService());
#endif
        }
    }
}
