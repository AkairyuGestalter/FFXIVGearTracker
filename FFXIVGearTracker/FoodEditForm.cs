using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFXIVGearTracker
{
    public partial class FoodEditForm : Form
    {
        public FoodEditForm()
        {
            InitializeComponent();
            foreach (Food f in Common.allFood)
            {
				dataGridView1.Rows.Add(f.name, (int)(f.vitPct * 100), f.vitCap, (int)(f.accPct * 100), f.accCap, (int)(f.detPct * 100), f.detCap, (int)(f.critPct * 100), f.critCap, (int)(f.speedPct * 100), f.speedCap, (int)(f.piePct * 100), f.pieCap, (int)(f.parryPct * 100), f.parryCap);
            }
            this.FormClosing += FoodEditForm_FormClosing;
        }

        void FoodEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                Common.allFood.Clear();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    string name = (string)row.Cells["Food"].Value;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        double vitmod = GetCellAsInt(row.Cells["VitPct"]) / 100.0;
                        int vitmax = GetCellAsInt(row.Cells["VitCap"]);
                        double accmod = GetCellAsInt(row.Cells["AccPct"]) / 100.0;
                        int accmax = GetCellAsInt(row.Cells["AccCap"]);
                        double detmod = GetCellAsInt(row.Cells["DetPct"]) / 100.0;
                        int detmax = GetCellAsInt(row.Cells["DetCap"]);
                        double critmod = GetCellAsInt(row.Cells["CritPct"]) / 100.0;
                        int critmax = GetCellAsInt(row.Cells["CritCap"]);
                        double spdmod = GetCellAsInt(row.Cells["SpdPct"]) / 100.0;
                        int spdmax = GetCellAsInt(row.Cells["SpdCap"]);
						double piemod = GetCellAsInt(row.Cells["PiePct"]) / 100.0;
						int piemax = GetCellAsInt(row.Cells["PieMax"]);
						double parrymod = GetCellAsInt(row.Cells["ParryPct"]);
						int parrymax = GetCellAsInt(row.Cells["ParryMax"]);
                        Common.allFood.Add(new Food(name, vitmod, vitmax, accmod, accmax, detmod, detmax, critmod, critmax, spdmod, spdmax, piemod, piemax, parrymod, parrymax));
                    }
                }
            }
        }

        private int GetCellAsInt(DataGridViewCell cell)
        {
            int tempval;
            try
            {
                tempval = (int)cell.Value;
                return tempval;
            }
            catch
            {
                try
                {
                    if (int.TryParse((string)cell.Value, out tempval))
                    {
                        return tempval;
                    }
                }
                catch { }
            }
            return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
    }
}
