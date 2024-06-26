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
        public SalvageForm()
        {
            InitializeComponent();
        }

        private void btnMoveRight_Click(object sender, EventArgs e)
        {
            if (listBoxAvailable.SelectedIndex < 0) return;
            var value = listBoxAvailable.SelectedItem.ToString();
            listBoxAvailable.Items.RemoveAt(listBoxAvailable.SelectedIndex);
            for (int i = 0; i < listBoxUnavailable.Items.Count; i ++)
            {
                int temp = int.Parse(listBoxUnavailable.Items[i].ToString());
                if (temp > int.Parse(value))
                {
                    listBoxUnavailable.Items.Insert(i, value);
                    return;
                }
            }
            listBoxUnavailable.Items.Add(value);
        }

        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            if (listBoxUnavailable.SelectedIndex < 0) return;
            var value = listBoxUnavailable.SelectedItem.ToString();
            listBoxUnavailable.Items.RemoveAt(listBoxUnavailable.SelectedIndex);
            for (int i = 0; i < listBoxAvailable.Items.Count; i++)
            {
                int temp = int.Parse(listBoxAvailable.Items[i].ToString());
                if (temp > int.Parse(value))
                {
                    listBoxAvailable.Items.Insert(i, value);
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
                _available.Add(int.Parse(listBoxAvailable.Items[i].ToString()));
            }

            for (int i = 0; i < listBoxUnavailable.Items.Count; i++)
            {
                _unavailable.Add(int.Parse(listBoxUnavailable.Items[i].ToString()));
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
            listBoxAvailable.Items.Clear();
            listBoxUnavailable.Items.Clear();
            for (int i = 0; i < _available.Count; i ++)
            {
                listBoxAvailable.Items.Add(_available[i].ToString());
            }

            for (int i = 0; i < _unavailable.Count; i++)
            {
                listBoxUnavailable.Items.Add(_unavailable[i].ToString());
            }
        }
    }
}
