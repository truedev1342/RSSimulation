using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSSimulation
{
    public partial class SettingsForm : Form
    {
        public int _rsNum {  get; set; }
        public int _necessaryTime { get; set; }
        public int _scale { get; set; }
        public DateTime _startTime { get; set; }
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this._rsNum = (int)inputRSNum.Value;
            this._necessaryTime = (int)inputNecessaryTime.Value;
            this._scale = (int)inputScale.Value;
            this._startTime = inputStartTime.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            inputNecessaryTime.Value = this._necessaryTime;
            inputScale.Value = this._scale;
            inputRSNum.Value = this._rsNum;
            inputStartTime.Value = this._startTime;
        }
    }
}
