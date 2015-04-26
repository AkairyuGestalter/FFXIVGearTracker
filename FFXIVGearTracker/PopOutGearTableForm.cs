using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FFXIV.GearTracking.Core;

namespace FFXIV.GearTracking.WinForms
{
	public partial class PopOutGearTableForm : Form
	{
		private string slotFilter;
		private int currentJob;
		private int highTurnFilter;

		private Character activeChar;

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

		public PopOutGearTableForm()
		{
			InitializeComponent();

			slotFilter = "";
			currentJob = 0;
			highTurnFilter = Common.HighestTurn;

			CustomEvents.SlotFilterChangedEvent += PopOutGearTableForm_SlotFilterChanged;
			CustomEvents.HighestTurnFilterChangedEvent += PopOutGearTableForm_HighestTurnFilterChanged;
			CustomEvents.CharacterChangedEvent += PopOutGearTableForm_CharacterChanged;
			CustomEvents.CharacterUpdatedEvent += PopOutGearTableForm_CharacterUpdated;
			CustomEvents.ItemOwnedChangeEvent += CustomEvents_ItemOwnedChangeEvent;

			PopOutGearDisplayGridView.CellMouseUp+=PopOutGearDisplayGridView_CellMouseUp;
			PopOutGearDisplayGridView.CellValueChanged+=PopOutGearDisplayGridView_CellValueChanged;

			this.FormClosing += PopOutGearTableForm_FormClosing;
		}

		void CustomEvents_ItemOwnedChangeEvent(object sender, Item i, bool isOwned)
		{
			if (sender != this)
			{
				foreach (DataGridViewRow row in PopOutGearDisplayGridView.Rows)
				{
					try
					{
						if (i.name.Equals(((Item)row.Cells["Item"].Value).name))
						{
							row.Cells["Owned"].Value = isOwned;
						}
					}
					catch { }
				}
			}
		}

		void PopOutGearDisplayGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == PopOutGearDisplayGridView.Columns["Owned"].Index && e.RowIndex != -1)
			{
				CustomEvents.ChangeItemOwned(this, (Item)PopOutGearDisplayGridView.Rows[e.RowIndex].Cells["Item"].Value, (bool)PopOutGearDisplayGridView.Rows[e.RowIndex].Cells["Owned"].Value);
			}
		}

		void PopOutGearDisplayGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex == PopOutGearDisplayGridView.Columns["Owned"].Index && e.RowIndex != -1)
			{
				PopOutGearDisplayGridView.EndEdit();
			}
		}

		void PopOutGearTableForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			/*SlotFilterChanged = null;
			HighestTurnFilterChanged = null;
			CharacterChanged = null;
			CharacterUpdated = null;*/

		}

		void PopOutGearTableForm_CharacterUpdated(Character c)
		{
			UpdateCharacter(c);
		}

		void PopOutGearTableForm_CharacterChanged(Character c)
		{
			ChangeCharacter(c);
		}

		void PopOutGearTableForm_HighestTurnFilterChanged(int highTurn)
		{
			ChangeHighestTurnFilter(highTurn);
		}

		void PopOutGearTableForm_SlotFilterChanged(string slot)
		{
			ChangeSlotFilter(slot);
		}

		private void ChangeSlotFilter(string slot)
		{
			slotFilter = slot;
			FilterGear(slotFilter, highTurnFilter);
		}

		private void ChangeJob(int j)
		{
			currentJob = j;
			PopOutGearDisplayGridView.Rows.Clear();
			foreach (Item i in Common.gearDictionary.Values)
			{
				if (i.canEquip.Contains((Job)j))
				{
					PopOutGearDisplayGridView.Rows.Add(activeChar.ownedItems.Contains(i.name), i, i.sourceTurn, i.equipSlot, i.itemStats.weaponDamage, i.itemStats.mainStat, i.itemStats.vit, i.itemStats.pie, i.itemStats.acc, i.itemStats.det, i.itemStats.crit, i.itemStats.speed, i.itemStats.parry, Character.CalcGearVal(i, activeChar.currentWeights));
				}
			}
			FilterGear(slotFilter, highTurnFilter);
		}

		private void ChangeHighestTurnFilter(int highTurn)
		{
			highTurnFilter = highTurn;
			FilterGear(slotFilter, highTurnFilter);
		}

		private void ChangeCharacter(Character c)
		{
			activeChar = c;
			ChangeJob((int)activeChar.currentJob);
			PopGearValues();
		}

		private void UpdateCharacter(Character c)
		{
			activeChar = c;
			PopGearValues();
		}

		private void FilterGear(string slotFilter, int maxTurn)
		{
			foreach (DataGridViewRow row in PopOutGearDisplayGridView.Rows)
			{
				try
				{
					if ((slotFilter.Equals(((GearSlot)row.Cells["Slot"].Value).ToString()) || string.IsNullOrWhiteSpace(slotFilter)) && maxTurn >= (int)row.Cells["Turn"].Value)
					{
						row.Visible = true;
					}
					else
					{
						row.Visible = false;
					}
				}
				catch { }
			}
		}

		private void PopGearValues()
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new UpdGearValues(PopGearValues));
			}
			else
			{
				foreach (DataGridViewRow row in PopOutGearDisplayGridView.Rows)
				{
					try
					{
						Item tempItem = (Item)row.Cells["Item"].Value;
						Item currentItem = new Item();
						switch (tempItem.equipSlot)
						{
							case GearSlot.MainHand:
								currentItem.itemStats = activeChar.currentDamage[(int)activeChar.currentJob].mainHand.itemStats + activeChar.currentDamage[(int)activeChar.currentJob].offHand.itemStats;
								if (!tempItem.twoHand && !activeChar.currentDamage[(int)activeChar.currentJob].mainHand.twoHand)
								{
									tempItem.itemStats = tempItem.itemStats + activeChar.currentDamage[(int)activeChar.currentJob].offHand.itemStats;
								}
								break;
							case GearSlot.OffHand:
								if (!activeChar.currentDamage[(int)activeChar.currentJob].mainHand.twoHand)
								{
									currentItem = activeChar.currentDamage[(int)activeChar.currentJob].offHand;
								}
								break;
							case GearSlot.Head:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].head;
								break;
							case GearSlot.Body:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].body;
								break;
							case GearSlot.Hands:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].hands;
								break;
							case GearSlot.Waist:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].waist;
								break;
							case GearSlot.Legs:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].legs;
								break;
							case GearSlot.Feet:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].feet;
								break;
							case GearSlot.Neck:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].neck;
								break;
							case GearSlot.Ears:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].ears;
								break;
							case GearSlot.Wrists:
								currentItem = activeChar.currentDamage[(int)activeChar.currentJob].wrists;
								break;
							case GearSlot.Ring:
								if (tempItem.unique && (tempItem == activeChar.currentDamage[(int)activeChar.currentJob].leftRing || tempItem == activeChar.currentDamage[(int)activeChar.currentJob].rightRing))
								{
									currentItem = tempItem;
								}
								else if (Character.CalcGearVal(activeChar.currentDamage[(int)activeChar.currentJob].leftRing, activeChar.currentWeights) > Character.CalcGearVal(activeChar.currentDamage[(int)activeChar.currentJob].rightRing, activeChar.currentWeights))
								{
									currentItem = activeChar.currentDamage[(int)activeChar.currentJob].rightRing;
								}
								else
								{
									currentItem = activeChar.currentDamage[(int)activeChar.currentJob].leftRing;
								}
								break;
							default:
								break;
						}
						row.Cells["CurrentVal"].Value = Character.CalcGearVal(tempItem, activeChar.currentWeights);
						row.Cells["ValPerCost"].Value = activeChar.CalcGearDValPerTome(currentItem, tempItem, activeChar.currentWeights);
					}
					catch { }
				}
			}
		}

        private void ClosePopFormButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            CustomEvents.ClosePopOutForm();
        }
	}
}
