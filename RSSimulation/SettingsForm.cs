using System;
using System.Windows.Forms;

namespace RSSimulation
{
    public partial class SettingsForm : Form
    {
        public DateTime SimulationTime { get; set; }
        public int SimulationSpeed { get; set; }
        public int RSCounts {  get; set; }
        public int EFrameBuiltTime { get; set; }
        public int SalvageTime { get; set; }
        public float DozerSpeed { get; set; }
        public float LCCSpeed { get; set; }
        public int LCCLoadingTime { get; set; }
        public int LCCDropTime { get; set; }
        public int LCCCounts { get; set; }
        public SettingsForm()
        {
            InitializeComponent();
        }
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            SimulationTimeTxt.Value = this.SimulationTime;
            SimulationSpeedTxt.Value = this.SimulationSpeed;
            RSCountsTxt.Value = this.RSCounts;
            EFrameTimeTxt.Value = this.EFrameBuiltTime;
            SalvageTimeTxt.Value =this.SalvageTime;
            DozerSpeedTxt.Value = Convert.ToDecimal(this.DozerSpeed);
            LCCSpeedTxt.Value = Convert.ToDecimal(this.LCCSpeed);
            LCCLoadingTimeTxt.Value = this.LCCLoadingTime;
            LCCDropTimeTxt.Value = this.LCCDropTime;
            LCCCountsTxt.Value = this.LCCCounts;
        }
        private void onOKBtn(object sender, EventArgs e)
        {
            this.SimulationTime = SimulationTimeTxt.Value;
            this.SimulationSpeed = (int)SimulationSpeedTxt.Value;
            this.RSCounts = (int)RSCountsTxt.Value;
            this.EFrameBuiltTime = (int)EFrameTimeTxt.Value;
            this.SalvageTime = (int)SalvageTimeTxt.Value;
            this.DozerSpeed = (float)DozerSpeedTxt.Value;
            this.LCCSpeed = (float)LCCSpeedTxt.Value;
            this.LCCLoadingTime = (int)LCCLoadingTimeTxt.Value;
            this.LCCDropTime = (int)LCCDropTimeTxt.Value;
            this.LCCCounts = (int)LCCCountsTxt.Value;

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
