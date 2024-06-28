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
        public int RSCounts {  get; set; }
        public int EFrameBuiltTime { get; set; }
        public int SimulationSpeed { get; set; }
        public DateTime SimulationTime { get; set; }
        public SettingsForm()
        {
            InitializeComponent();
        }
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            EFrameTimeTxt.Value = this.EFrameBuiltTime;
            SimulationSpeedTxt.Value = this.SimulationSpeed;
            RSCountsTxt.Value = this.RSCounts;
            SimulationTimeTxt.Value = this.SimulationTime;
        }
        private void onOKBtn(object sender, EventArgs e)
        {
            this.RSCounts = (int)RSCountsTxt.Value;
            this.EFrameBuiltTime = (int)EFrameTimeTxt.Value;
            this.SimulationSpeed = (int)SimulationSpeedTxt.Value;
            this.SimulationTime = SimulationTimeTxt.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void onCancelBtn(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }        
    }
}
