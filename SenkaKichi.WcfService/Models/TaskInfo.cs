using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SenkaKichi.WcfService.Models
{
    public abstract class TaskInfo
    {
        public DateTime NextRunTime { get; set; }

        public SortedSet<DateTime> Schedule { get; private set; }

        public Thread CurrentThread { get; private set; }

        public TaskInfo(IEnumerable<DateTime> schedule) {
            Schedule = new SortedSet<DateTime>(schedule);
        }

        public TaskInfo(TimeSpan interval, int startHour, int startMinute, int startSecond) :
            this(interval, startHour, startMinute, startSecond, true) { }

        public TaskInfo(TimeSpan interval, int startHour, int startMinute, int startSecond, bool runNow) {
            List<DateTime> schedule = new List<DateTime>();
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, startHour, startMinute, startSecond);
            do {
                schedule.Add(start);
                start += interval;
            } while (now.Day == start.Day);
            Schedule = new SortedSet<DateTime>(schedule);
            if (runNow) NextRunTime = DateTime.Now;
            else NextRunTime = Schedule.FirstOrDefault(d => d > NextRunTime);
        }

        protected static ServiceManager Manager {
            get {
                return ServiceManager.Current;
            }
        }

        public void Run() {
            DateTime next = Schedule.FirstOrDefault(d => d > NextRunTime);
            if (next == default(DateTime)) {
                Schedule = new SortedSet<DateTime>(Schedule.Select(c => c.AddDays(1)));
                NextRunTime = Schedule.First();
            } else {
                NextRunTime = next;
            }

            CurrentThread = new Thread(Main);
            CurrentThread.Start();
        }

        protected abstract void Main();
    }
}