using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenkaKichi.ManagementTool
{
    public partial class InputDialog : Form
    {
        public InputDialog() {
            InitializeComponent();
        }

        public DialogResult ShowDialog(string caption, string text, IWin32Window owner) {
            this.Text = caption;
            label1.Text = text;
            return this.ShowDialog(owner);
        }
    }
}
