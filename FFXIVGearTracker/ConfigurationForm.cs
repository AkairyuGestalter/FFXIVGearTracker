using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NCalc;

namespace FFXIVGearTracker
{
    public partial class ConfigurationForm : Form
    {
        private bool dmgFormValid = true;
        private bool critFormValid = true;

        public ConfigurationForm()
        {
            InitializeComponent();
            textBox1.Text = Common.DamageFormula;
            textBox2.Text = Common.CritFormula;
            numericUpDown1.Value = Common.HighestTurn;
            this.CancelButton = button2;
            this.AcceptButton = button1;

            textBox1.Validated += textBox1_Validated;
            textBox2.Validated += textBox2_Validated;
        }

        void textBox2_Validated(object sender, EventArgs e)
        {
            try
            {
                string critForm = textBox2.Text.Replace("CRITPCTMOD", "0").Replace("CRIT", "341");
                Expression expr = new Expression(critForm);
                double testVal = (double)expr.Evaluate();
                textBox2.BackColor = System.Drawing.SystemColors.Window;
                critFormValid = true;
                button1.Enabled = critFormValid & dmgFormValid;
            }
            catch
            {
                critFormValid = false;
                button1.Enabled = false;
                textBox2.BackColor = Color.Red;
            }
        }

        void textBox1_Validated(object sender, EventArgs e)
        {
            try
            {
                string dmgForm = textBox1.Text.Replace("WD", "40").Replace("DTR", "202").Replace("STAT", "400");
                Expression expr = new Expression(dmgForm);
                double testVal = (double)expr.Evaluate();
                textBox1.BackColor = System.Drawing.SystemColors.Window;
                dmgFormValid = true;
                button1.Enabled = critFormValid & dmgFormValid;
            }
            catch
            {
                dmgFormValid = false;
                button1.Enabled = false;
                textBox1.BackColor = Color.Red;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Common.DamageFormula = textBox1.Text;
            Common.CritFormula = textBox2.Text;
            Common.HighestTurn = (int)numericUpDown1.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}
