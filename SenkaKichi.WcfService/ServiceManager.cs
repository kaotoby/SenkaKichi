using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenkaKichi.DbModels;
using SenkaKichi.OAuthApi.Twitter;
using SenkaKichi.WcfService.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace SenkaKichi.WcfService
{
    public partial class ServiceManager
    {
        public Dictionary<int, ServerInfo> Servers { get; private set; }
        public Dictionary<string, TaskInfo> Tasks { get; private set; }
        public SenkaContext Database { get; private set; }
        public TwitterApiManager TwitterManager { get; private set; }
        public DateInfo DateInfo { get; set; }
        public static ServiceManager Current {
            get {
                if (_current == null) {
                    _current = new ServiceManager();
                }
                return _current;
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ServiceManager).FullName);
        private static ServiceManager _current;
        private Timer _timer;

        public ServiceManager() {
            #region Configure Task

            Tasks = new Dictionary<string, TaskInfo> {
                { "UpdateSenka" , new UpdateSenkaTask(new TimeSpan(12, 0, 0), 3, 0, 2) }
            };

            Tasks["UpdateSenka"].NextRunTime = DateTime.Now;

            #endregion

            Database = new SenkaContext();
            TwitterManager = new TwitterApiManager(Database);
        }

        public void StartTimer() {
            Servers = Database.Servers
                .Include(server => server.ServerAuthorize)
                .ToArray()
                .ToDictionary(server => (int)server.ServerId, server => new ServerInfo(server));

            _timer = new Timer(TimerCallback, null, 3000, 1000);
        }

        private void TimerCallback(object state) {
            var taskToRun = Tasks.Where(t => DateTime.Now > t.Value.NextRunTime);
            foreach (var task in taskToRun) {
                log.Debug(string.Format("[Task Trigger] {0}", task.Key));
                task.Value.Run();
            }
        }
    }
}