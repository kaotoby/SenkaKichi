using SenkaKichi.DbModels;
using SenkaKichi.ManagementTool.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenkaKichi.ManagementTool
{
    public partial class ManagementFrom : Form
    {
        public static Logger Log {
            get {
                if (_log == null) {
                    _log = new Logger();
                }
                return _log;
            }
        }

        private static Logger _log;
        private SenkaContext _db;
        private ServiceClient _client;
        private bool inprogress = false;

        public ManagementFrom() {
            InitializeComponent();
        }

        private void ManagementFrom_Load(object sender, EventArgs e) {
            Log.OnLogChanged += Log_OnLogChanged;
            _db = new SenkaContext();
            _client = new ServiceClient();
        }

        void Log_OnLogChanged(object sender, LogEventArgs e) {
            FromHelper.AppendText(this, textBoxLog, e.NewLog + "\r\n");
        }

        private async void buttonForceUpdateData_Click(object sender, EventArgs e) {
            if (!inprogress) {
                inprogress = true;
                string serverId = ShowInputDialog("Enter Server Id");
                if (!string.IsNullOrEmpty(serverId)) {
                    var result = await _client.UpdateSenkaDataAsync(int.Parse(serverId));
                    Log.Debug("Update Senka Data: " + result.ToString());
                }
                inprogress = false;
            }
        }

        private async void buttonUpdateIp_Click(object sender, EventArgs e) {
            if (!inprogress) {
                inprogress = true;
                var result = await _client.UpdateServerIpAddressAsync();
                Log.Debug("Update Ip: " + result.ToString());
                inprogress = false;
            }
        }

        private string ShowInputDialog(string text) {
            InputDialog inputDialog = new InputDialog();

            string result = null;
            if (inputDialog.ShowDialog("Input", text, this) == DialogResult.OK) {
                result = inputDialog.textBox1.Text;
            }

            inputDialog.Dispose();
            return result;
        }

        private DateTime ShowDateTimeDialog(string text) {
            DateTimeDialog datetimeDialog = new DateTimeDialog();
            datetimeDialog.dateTimePicker1.Value = DateTime.Now;

            DateTime result = default(DateTime);
            if (datetimeDialog.ShowDialog("Input", text, this) == DialogResult.OK) {
                result = datetimeDialog.dateTimePicker1.Value;
            }

            datetimeDialog.Dispose();
            return result;
        }

        private async void buttonUpdateStart_Click(object sender, EventArgs e) {
            if (!inprogress) {
                inprogress = true;
                var info = await _db.ServerMaintenances
                    .OrderByDescending(m => m.Id)
                    .FirstOrDefaultAsync();
                Log.Debug(info.ToString());
                DateTime start = ShowDateTimeDialog("Enter start time");
                inprogress = false;
            }
        }

        private void buttonUpdateEnd_Click(object sender, EventArgs e) {

        }

        private void buttonAddMaintenance_Click(object sender, EventArgs e) {

        }

        private async void buttonTest_Click(object sender, EventArgs e) {
            Log.Debug("Pressed");
            SenkaKichi.OAuthApi.Twitter.TwitterApiManager mamger = new SenkaKichi.OAuthApi.Twitter.TwitterApiManager(new SenkaContext());
            try {
                Log.Debug("ready!");
                await mamger.PostStatusesUpdateAsync(1, Guid.NewGuid().ToString("N") + "\ntest2 #test");
                Log.Debug("OK");
            } catch (Exception ex) {
                Log.Debug("Fail", ex);
            }
        }
    }
}
