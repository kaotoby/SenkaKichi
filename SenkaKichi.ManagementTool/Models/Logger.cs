using System;
using System.Collections.Generic;

namespace SenkaKichi.ManagementTool
{
    public class Logger
    {
        public List<string> Logs { get { return _logs; } }
        public string LogText { get { return string.Join("\r\n", _logs); } }
        public delegate void LogChangedHandler(object sender, LogEventArgs e);
        public event LogChangedHandler OnLogChanged;

        private List<string> _logs;

        public Logger() {
            _logs = new List<string>();
        }

        private void LogChanged(string log) {
            // Make sure someone is listening to event
            if (OnLogChanged == null) return;

            LogEventArgs args = new LogEventArgs(log);
            OnLogChanged(this, args);
        }

        public void Debug(string msg) {
            Debug(msg, null);
        }

        public void Debug(string msg, Exception ex) {
            Log("DEBUG", msg, ex);
        }

        public void Info(string msg) {
            Info(msg, null);
        }

        public void Info(string msg, Exception ex) {
            Log("INFO", msg, ex);
        }

        public void Warn(string msg) {
            Warn(msg, null);
        }

        public void Warn(string msg, Exception ex) {
            Log("WARN", msg, ex);
        }

        public void Error(string msg) {
            Error(msg, null);
        }

        public void Error(string msg, Exception ex) {
            Log("ERROR", msg, ex);
        }

        public void Log(string level, string msg, Exception ex) {
            string log = string.Format("{0} [{1}] {2}{3}", DateTime.Now.ToString("HH:mm:ss"), level, ex == null ? "" : ex.ToString() + "\r\n", msg);
            _logs.Add(log);
            LogChanged(log);
        }
    }

    public class LogEventArgs : EventArgs
    {
        public string NewLog { get; private set; }

        public LogEventArgs(string status) {
            NewLog = status;
        }
    }
}
