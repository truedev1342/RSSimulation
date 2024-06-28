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
    public partial class SalvageForm : Form
    {

        public List<int> _available = new List<int>();
        public List<int> _unavailable = new List<int>();
        public int RSCounts = 0;
        private bool _isLoading = false;
        public SalvageForm()
        {
            InitializeComponent();
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
            listBoxAvailable.Items.Add(value);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _available.Clear();
            for (int i = 0; i < listBoxAvailable.Items.Count; i ++)
            {
                _available.Add(int.Parse(listBoxAvailable.Items[i].ToString().Substring(14)));
            }

            for (int i = 0; i < listBoxUnavailable.Items.Count; i++)
            {
                _unavailable.Add(int.Parse(listBoxUnavailable.Items[i].ToString().Substring(14)));
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SalvageForm_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            RSCountsTxt.Value = RSCounts;
            listBoxAvailable.Items.Clear();
            listBoxUnavailable.Items.Clear();
            for (int i = 0; i < _available.Count; i ++)
            {
                listBoxAvailable.Items.Add("Roof Support #" + _available[i].ToString());
            }

            for (int i = 0; i < _unavailable.Count; i++)
            {
                listBoxUnavailable.Items.Add("Roof Support #" + _unavailable[i].ToString());
            }
            _isLoading = false;
        }

        private void RSCountsTxt_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            RSCounts = (int)RSCountsTxt.Value;
            _available.Clear();
            _unavailable.Clear();
            listBoxAvailable.Items.Clear();
            listBoxUnavailable.Items.Clear();
            for (int i = 1; i <= RSCounts; i++)
            {
                _available.Add(i);
                listBoxAvailable.Items.Add("Roof Support #" + i.ToString());
            }
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
    }
}
