using System;
using System.Windows.Forms;

namespace RSSimulation
{
    public partial class SettingsForm : Form
    {
        public bool needReDraw = false;
        public SettingsForm()
        {
            InitializeComponent();
        }
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            SimulationTimeTxt.Value = Global.SimulationTime;
            SimulationSpeedTxt.Value = Global.SimulationSpeed;
            RSCountsTxt.Value = Global.RSCounts;
            EFrameTimeTxt.Value = Global.EFrameBuiltTime;
            SalvageTimeTxt.Value =Global.SalvageTime;
            DozerSpeedTxt.Value = Convert.ToDecimal(Global.DozerSpeed);
            LCCSpeedTxt.Value = Convert.ToDecimal(Global.LCCSpeed);
            LCCLoadingTimeTxt.Value = Global.LCCLoadingTime;
            LCCDropTimeTxt.Value = Global.LCCDropTime;
            LCCCountsTxt.Value = Global.LCCCounts;
            needReDraw = false;
        }
        private void onOKBtn(object sender, EventArgs e)
        {
            Global.SimulationTime = SimulationTimeTxt.Value;
            Global.SimulationSpeed = (int)SimulationSpeedTxt.Value;
            Global.EFrameBuiltTime = (int)EFrameTimeTxt.Value;
            Global.SalvageTime = (int)SalvageTimeTxt.Value;
            Global.DozerSpeed = (float)DozerSpeedTxt.Value;
            Global.LCCSpeed = (float)LCCSpeedTxt.Value;
            Global.LCCLoadingTime = (int)LCCLoadingTimeTxt.Value;
            Global.LCCDropTime = (int)LCCDropTimeTxt.Value;

            if (Global.LCCCounts != (int)LCCCountsTxt.Value)
            {
                Global.LCCCounts = (int)LCCCountsTxt.Value;
                Global._lcc = new Carrier[Global.LCCCounts];
                for (int i = 0; i < Global.LCCCounts; i++)
                {
                    Global._lcc[i] = new Carrier();
                }
                Global.InitializeLCCs();
            }
            if (Global.RSCounts != (int)RSCountsTxt.Value)
            {
                Global.RSCounts = (int)RSCountsTxt.Value;
                if (Global.RSCounts > 0 && Global.RSCounts < 150)
                {
                    Global._normalRSList.Clear();
                    Global._salvageRSList.Clear();
                    for (int i = 1; i <= Global.RSCounts; i++)
                    {
                        Global._normalRSList.Add(i);
                    }
                }
                Global.InitializeRSandDozer();
            }

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
