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
	public partial class GearEditForm : Form
	{
		public GearEditForm()
		{
			InitializeComponent();
			this.FormClosing += GearEditForm_FormClosing;
			foreach (Job j in Enum.GetValues(typeof(Job)))
			{
				((DataGridViewComboBoxColumn)dataGridView1.Columns["Job"]).Items.Add(j.ToString());
				comboBox1.Items.Add(j.ToString());
			}
			foreach (GearSlot g in Enum.GetValues(typeof(GearSlot)))
			{
				((DataGridViewComboBoxColumn)dataGridView1.Columns["Slot"]).Items.Add(g.ToString());
				comboBox2.Items.Add(g.ToString());
			}
			foreach (Item i in Common.allGear)
			{
				foreach (Job j in i.canEquip)
				{
					//name, ilvl, unique, job, slot, twohand?, wdmg, stat, vit, acc, det, crit, speed
					dataGridView1.Rows.Add(i.name, i.itemStats.itemLevel, i.unique.ToString(), j.ToString(), i.equipSlot.ToString(), i.twoHand.ToString(), i.itemStats.weaponDamage, i.itemStats.mainStat, i.itemStats.vit, i.itemStats.pie, i.itemStats.acc, i.itemStats.det, i.itemStats.crit, i.itemStats.speed, i.itemStats.parry, i.sourceTurn, i.tomeTier, i.tomeCost);
				}
			}

			dataGridView1.DataError += dataGridView1_DataError;
		}

		void GearEditForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult == DialogResult.OK)
			{
				Common.allGear.Clear();
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					Item i = new Item();
					i.name = (string)row.Cells["Item"].Value;
					if (!string.IsNullOrWhiteSpace(i.name))
					{
						bool itemFound = false;
						foreach (Item i2 in Common.allGear)
						{
							if (i2.name.Equals(i.name))
							{
								itemFound = true;
								try
								{
									i2.canEquip.Add((FFXIVGearTracker.Job)Enum.Parse(typeof(FFXIVGearTracker.Job), (string)row.Cells["Job"].Value));
								}
								catch { }
							}
						}
						if (!itemFound)
						{
							try
							{
								i.itemStats.itemLevel = GetCellAsInt(row.Cells["ItemLevel"]);
								i.unique = GetCellAsBool(row.Cells["IsUniqueItem"]);
								i.canEquip = new List<FFXIVGearTracker.Job>();
								i.canEquip.Add((FFXIVGearTracker.Job)Enum.Parse(typeof(FFXIVGearTracker.Job), (string)row.Cells["Job"].Value));
								i.equipSlot = (GearSlot)Enum.Parse(typeof(GearSlot), (string)row.Cells["Slot"].Value);
								i.twoHand = GetCellAsBool(row.Cells["IsTwoHand"]);
								i.itemStats.weaponDamage = GetCellAsInt(row.Cells["WDMG"]);
								i.itemStats.mainStat = GetCellAsInt(row.Cells["Stat"]);
								i.itemStats.vit = GetCellAsInt(row.Cells["VIT"]);
								i.itemStats.pie = GetCellAsInt(row.Cells["PIE"]);
								i.itemStats.det = GetCellAsInt(row.Cells["DET"]);
								i.itemStats.acc = GetCellAsInt(row.Cells["Accuracy"]);
								i.itemStats.crit = GetCellAsInt(row.Cells["Crit"]);
								i.itemStats.speed = GetCellAsInt(row.Cells["Speed"]);
								i.itemStats.parry = GetCellAsInt(row.Cells["Parry"]);
								i.sourceTurn = GetCellAsInt(row.Cells["Turn"]);
								i.tomeTier = GetCellAsDouble(row.Cells["TomeTier"]);
								i.tomeCost = GetCellAsInt(row.Cells["TomeCost"]);
								i.relicTier = GetCellAsDouble(row.Cells["RelicTier"]);
								Common.allGear.Add(i);
							}
							catch { }
						}
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

		private double GetCellAsDouble(DataGridViewCell cell)
		{
			double tempval;
			try
			{
				tempval = (double)cell.Value;
				return tempval;
			}
			catch
			{
				try
				{
					if (double.TryParse((string)cell.Value, out tempval))
					{
						return tempval;
					}
				}
				catch { }
			}
			return 0;
		}

		private bool GetCellAsBool(DataGridViewCell cell)
		{
			bool tempval;
			try
			{
				tempval = (bool)cell.Value;
				return tempval;
			}
			catch
			{
				try
				{
					if (bool.TryParse((string)cell.Value, out tempval))
					{
						return tempval;
					}
				}
				catch { }
			}
			return false;
		}

		void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{

		}

		private void button3_Click(object sender, EventArgs e)
		{
			//cancel
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//accept
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			FilterView((string)comboBox1.SelectedItem, (string)comboBox2.SelectedItem);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			comboBox1.SelectedIndex = -1;
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			FilterView((string)comboBox1.SelectedItem, (string)comboBox2.SelectedItem);
		}

		private void FilterView(string jobFilter, string slotFilter)
		{
			try
			{
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					if ((((string)row.Cells["Job"].Value).Equals(jobFilter) || string.IsNullOrWhiteSpace(jobFilter)) && (((string)row.Cells["Slot"].Value).Equals(slotFilter) || string.IsNullOrWhiteSpace(slotFilter)))
					{
						row.Visible = true;
					}
					else
					{
						row.Visible = false;
					}
				}
			}
			catch
			{
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			comboBox2.SelectedIndex = -1;
		}
	}
}
