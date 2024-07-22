using System;
using System.Windows.Forms;

namespace RSSimulation
{
    public partial class SalvageForm : Form
    {
        public bool needReDraw = false;
        private bool _isLoading = false;
        public SalvageForm()
        {
            InitializeComponent();
        }
        private void SalvageForm_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            RSCountsTxt.Value = Global.RSCounts;
            listBoxAvailable.Items.Clear();
            listBoxUnavailable.Items.Clear();
            for (int i = 0; i < Global._normalRSList.Count; i++)
            {
                listBoxAvailable.Items.Add("Roof Support #" + Global._normalRSList[i].ToString());
            }

            for (int i = 0; i < Global._salvageRSList.Count; i++)
            {
                listBoxUnavailable.Items.Add("Roof Support #" + Global._salvageRSList[i].ToString());
            }
            _isLoading = false;
        }
        private void btnMoveRight_Click(object sender, EventArgs e)
        {
            if (listBoxAvailable.SelectedIndex < 0) return;
            var value = listBoxAvailable.SelectedItem.ToString();
            listBoxAvailable.Items.RemoveAt(listBoxAvailable.SelectedIndex);
            listBoxUnavailable.Items.Add(value);
        }

        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            if (listBoxUnavailable.SelectedIndex < 0) return;
            int value = int.Parse(listBoxUnavailable.SelectedItem.ToString().Substring(14));
            listBoxUnavailable.Items.RemoveAt(listBoxUnavailable.SelectedIndex);
            for (int i = 0; i < listBoxAvailable.Items.Count; i++)
            {
                int temp = int.Parse(listBoxAvailable.Items[i].ToString().Substring(14));
                if (temp > value)
                {
                    listBoxAvailable.Items.Insert(i, "Roof Support #" + value.ToString());
                    return;
                }
            }
            listBoxAvailable.Items.Add("Roof Support #" + value);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            Global._normalRSList.Clear();
            Global._salvageRSList.Clear();
            for (int i = 0; i < listBoxAvailable.Items.Count; i ++)
            {
                Global._normalRSList.Add(int.Parse(listBoxAvailable.Items[i].ToString().Substring(14)));
            }

            for (int i = 0; i < listBoxUnavailable.Items.Count; i++)
            {
                Global._salvageRSList.Add(int.Parse(listBoxUnavailable.Items[i].ToString().Substring(14)));
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void RSCountsTxt_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            Global.RSCounts = (int)RSCountsTxt.Value;
            Global._normalRSList.Clear();
            Global._salvageRSList.Clear();
            listBoxAvailable.Items.Clear();
            listBoxUnavailable.Items.Clear();
            for (int i = 1; i <= Global.RSCounts; i++)
            {
                Global._normalRSList.Add(i);
                listBoxAvailable.Items.Add("Roof Support #" + i.ToString());
            }
            needReDraw = true;
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            int seled = listBoxUnavailable.SelectedIndex;
            if (seled <= 0) return;
            var item = listBoxUnavailable.SelectedItem;
            listBoxUnavailable.Items.RemoveAt(seled);
            listBoxUnavailable.Items.Insert(seled - 1, item);
            listBoxUnavailable.SelectedIndex = seled - 1;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            int seled = listBoxUnavailable.SelectedIndex;
            if (seled < 0 || seled >= listBoxUnavailable.Items.Count) return;
            var item = listBoxUnavailable.SelectedItem;
            listBoxUnavailable.Items.RemoveAt(seled);
            listBoxUnavailable.Items.Insert(seled + 1, item);
            listBoxUnavailable.SelectedIndex = seled + 1;
        }

        private void listBoxAvailable_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxAvailable.SelectedIndex < 0) return;
            var value = listBoxAvailable.SelectedItem.ToString();
            listBoxAvailable.Items.RemoveAt(listBoxAvailable.SelectedIndex);
            listBoxUnavailable.Items.Add(value);
        }

        private void listBoxUnavailable_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxUnavailable.SelectedIndex < 0) return;
            int value = int.Parse(listBoxUnavailable.SelectedItem.ToString().Substring(14));
            listBoxUnavailable.Items.RemoveAt(listBoxUnavailable.SelectedIndex);
            for (int i = 0; i < listBoxAvailable.Items.Count; i++)
            {
                int temp = int.Parse(listBoxAvailable.Items[i].ToString().Substring(14));
                if (temp > value)
                {
                    listBoxAvailable.Items.Insert(i, "Roof Support #" + value.ToString());
                    return;
                }
            }
            listBoxAvailable.Items.Add("Roof Support #" + value);
        }
    }
}
