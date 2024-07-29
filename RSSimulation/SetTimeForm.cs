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
    public partial class SetTimeForm : Form
    {
        public int estimateTime;
        public SetTimeForm()
        {
            InitializeComponent();
        }
        private void SetTimeForm_Load(object sender, EventArgs e)
        {
            txtHour.Value = 3;
            txtMin.Value = 0;
            txtSec.Value = 0;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            estimateTime = (int)txtHour.Value * 3600 + (int)txtMin.Value * 60 + (int)txtSec.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
