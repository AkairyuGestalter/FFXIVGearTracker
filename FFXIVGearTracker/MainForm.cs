using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using NCalc;
using FFXIV.GearTracking.Core;
using FFXIV.GearTracking.Simulation;

namespace FFXIV.GearTracking.WinForms
{
    public partial class MainForm : Form
    {
        private Character activeChar;
        public Character GetActiveChar
        {
            get
            {
                return activeChar;
            }
        }
        PopOutGearTableForm popForm = new PopOutGearTableForm();

        private Dictionary<int, GearSet> accSets;
        Thread calcThread;
        private bool dmgFormValid = false, healFormValid = false, critFormValid = false, parryFormValid = false, speedFormValid = false;
        private bool previousVisibleState;

        public MainForm()
        {
            InitializeComponent();
            foreach (Job j in Enum.GetValues(typeof(Job)))
            {
                JobSelect.Items.Add(j);
                ((DataGridViewComboBoxColumn)GearEditGridView.Columns["Job"]).Items.Add(j.ToString());
                EditGearJobFilterComboBox.Items.Add(j.ToString());
            }
            foreach (Character c in Common.charDictionary.Values)
            {
                CharacterSelect.Items.Add(c);
            }
            foreach (GearSlot g in Enum.GetValues(typeof(GearSlot)))
            {
                GearSlotFilter.Items.Add(g.ToString());
                ((DataGridViewComboBoxColumn)GearEditGridView.Columns["EditSlot"]).Items.Add(g.ToString());
                EditGearSlotFilterComboBox.Items.Add(g.ToString());
            }
            SimWeightsCheckBox.Checked = Common.SimulateWeights;

            CharacterSelect.SelectedIndexChanged += CharacterSelect_SelectedIndexChanged;
            GearDisplayGridView.CellMouseUp += GearDisplayGridView_CellMouseUp;
            GearDisplayGridView.CellValueChanged += GearDisplayGridView_CellValueChanged;

            #region EditGearTab
            EditGearCancelButton_Click(this, null);
            #endregion

            #region EditFoodTab
            EditFoodResetButton_Click(this, null);
            #endregion

            #region EditConfigTab
            EditDamageFormTextBox.Text = Common.DamageFormula;
            EditHealingFormTextBox.Text = Common.HealingFormula;
            EditCritFormTextBox.Text = Common.CritFormula;
            EditParryFormTextBox.Text = Common.ParryFormula;
            EditVITPerSTR.Value = (decimal)Common.VitPerSTR;
            EditHighestTurn.Value = (decimal)Common.HighestTurn;
            #endregion

            CustomEvents.ItemOwnedChangeEvent += CustomEvents_ItemOwnedChangeEvent;
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
            this.Shown += MainForm_Shown;
        }

        void CustomEvents_ItemOwnedChangeEvent(object sender, Item i, bool isOwned)
        {
            if (isOwned && !activeChar.ownedItems.Contains(i.name))
            {
                activeChar.ownedItems.Add(i.name);
            }
            else if (!isOwned && activeChar.ownedItems.Contains(i.name))
            {
                activeChar.ownedItems.Remove(i.name);
            }
            if (sender != this)
            {
                foreach (DataGridViewRow row in GearDisplayGridView.Rows)
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

        void GearDisplayGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == GearDisplayGridView.Columns["Owned"].Index && e.RowIndex != -1)
            {
                CustomEvents.ChangeItemOwned(this, (Item)GearDisplayGridView.Rows[e.RowIndex].Cells["Item"].Value, (bool)GearDisplayGridView.Rows[e.RowIndex].Cells["Owned"].Value);
            }
        }

        void GearDisplayGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == GearDisplayGridView.Columns["Owned"].Index && e.RowIndex != -1)
            {
                GearDisplayGridView.EndEdit();
            }
        }

        void MainForm_Shown(object sender, EventArgs e)
        {
            PopOutGearTableForm popForm = new PopOutGearTableForm();
            //popForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
            if (Common.GearTablePoppedOut /*&& CustomEvents.ClosePopOutFormEvent == null*/)
            {
                PopOutGearButton_Click(this, null);
            }
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            #region CharacterGearTab
            GearDisplayGridView.Columns["Item"].ValueType = typeof(Item);
            if (CharacterSelect.Items.Count > 0)
            {
                CharacterSelect.SelectedIndex = 0;
            }
            GearSetsTabControl.SelectedTab = currentGearTab;
            MainTabControl.SelectedTab = tabPage1;
            HideShowGearBox();
            #endregion

            #region EditGearTab
            #endregion

            #region EditFoodTab
            #endregion

            #region EditConfigTab
            #endregion
        }

        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.charDictionary.Clear();
            foreach (Character c in CharacterSelect.Items)
            {
                Common.charDictionary.Add(c.charName, c);
            }
            if (calcThread != null)
            {
                if (calcThread.IsAlive)
                {
                    calcThread.Abort();
                }
            }
        }

        #region CharacterGearTab
        void CharacterSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            activeChar = (Character)CharacterSelect.SelectedItem;
            PopulateForm();
        }

        private void PopulateForm()
        {
            GearSet ideal;
            try
            {
                ideal = activeChar.idealDamage[(int)activeChar.currentJob];
            }
            catch
            {
                ideal = new GearSet();
                ideal.baseStats = activeChar.baseStats[(int)activeChar.currentJob];
            }
            if (activeChar.ownedDamage == null)
            {
                activeChar.ownedDamage = new GearSet[activeChar.currentDamage.Length];
                for (int i = 0; i < activeChar.ownedDamage.Length; i++)
                {
                    activeChar.ownedDamage[i] = new GearSet();
                    activeChar.ownedDamage[i].baseStats = activeChar.currentDamage[i].baseStats;
                }
                activeChar.ownedCoilFoodA = new GearSet[activeChar.currentCoilFoodA.Length];
                for (int i = 0; i < activeChar.ownedCoilFoodA.Length; i++)
                {
                    activeChar.ownedCoilFoodA[i] = new GearSet();
                    activeChar.ownedCoilFoodA[i].baseStats = activeChar.currentCoilFoodA[i].baseStats;
                }
                activeChar.ownedCoilFoodB = new GearSet[activeChar.currentCoilFoodB.Length];
                for (int i = 0; i < activeChar.ownedCoilFoodB.Length; i++)
                {
                    activeChar.ownedCoilFoodB[i] = new GearSet();
                    activeChar.ownedCoilFoodB[i].baseStats = activeChar.currentCoilFoodB[i].baseStats;
                }
                activeChar.ownedItems = new List<string>();
            }
            if (activeChar.ownedAccReqListA == null)
            {
                activeChar.ownedAccReqListA = (int[])activeChar.accuracyNeeds.Clone();
                activeChar.ownedAccReqListB = (int[])activeChar.accuracyNeeds.Clone();
            }
            activeChar.currentWeights = ideal.gearWeights;
            HighestTurn.Value = Math.Min(activeChar.clearedTurn, Common.HighestTurn);
            JobSelect.SelectedValue = activeChar.currentJob;
            JobSelect.Text = activeChar.currentJob.ToString();
            HighestTurn.Value = activeChar.clearedTurn;
            AccuracyRequirement.Value = activeChar.accuracyNeeds[(int)activeChar.currentJob];
            SpdBreakPoint.Value = Common.speedBreakPoints[(int)activeChar.currentJob];
            SpdBreakCheckBox.Checked = Common.UseSpeedBreakPoint;
            OwnedAAccReq.Value = activeChar.ownedAccReqListA[(int)activeChar.currentJob];
            OwnedBAccReq.Value = activeChar.ownedAccReqListB[(int)activeChar.currentJob];
            ProgressionTomeTier.Value = (decimal)activeChar.tomeTier[(int)activeChar.currentJob];
            ProgressionRelicTier.Value = (decimal)activeChar.relicTier[(int)activeChar.currentJob];

            GearDisplayGridView.Rows.Clear();
            foreach (Control box in currentGearTab.Controls)
            {
                if (box is GroupBox)
                {
                    foreach (Control c in ((GroupBox)box).Controls)
                    {
                        if (c is ComboBox)
                        {
                            ((ComboBox)c).Items.Clear();
                        }
                        else if (c is TextBox)
                        {
                            ((TextBox)c).Text = "";
                        }
                    }
                }
            }
            foreach (Control box in ownedGearTab.Controls)
            {
                if (box is GroupBox)
                {
                    foreach (Control c in ((GroupBox)box).Controls)
                    {
                        if (c is ComboBox)
                        {
                            ((ComboBox)c).Items.Clear();
                        }
                        else if (c is TextBox)
                        {
                            ((TextBox)c).Text = "";
                        }
                    }
                }
            }
            foreach (Control box in progressionGearTab.Controls)
            {
                if (box is GroupBox)
                {
                    foreach (Control c in ((GroupBox)box).Controls)
                    {
                        if (c is ComboBox)
                        {
                            ((ComboBox)c).Items.Clear();
                        }
                        else if (c is TextBox)
                        {
                            ((TextBox)c).Text = "";
                        }
                    }
                }
            }
            foreach (Control box in idealGearTab.Controls)
            {
                if (box is GroupBox)
                {
                    foreach (Control c in ((GroupBox)box).Controls)
                    {
                        if (c is ComboBox)
                        {
                            ((ComboBox)c).Items.Clear();
                        }
                        else if (c is TextBox)
                        {
                            ((TextBox)c).Text = "";
                        }
                    }
                }
            }

            Item tempItm = new Item();
            tempItm.name = "None";
            tempItm.equipSlot = GearSlot.OffHand;
            CurrentOffHand.Items.Add(tempItm);
            CurrentOffHandA.Items.Add(tempItm);
            CurrentOffHandB.Items.Add(tempItm);

            foreach (Item i in Common.gearDictionary.Values)
            {
                if (i.canEquip.Contains(activeChar.currentJob))
                {
                    GearDisplayGridView.Rows.Add(activeChar.ownedItems.Contains(i.name), i, i.sourceTurn, i.equipSlot, i.itemStats.weaponDamage, i.itemStats.mainStat, i.itemStats.vit, i.itemStats.pie, i.itemStats.acc, i.itemStats.det, i.itemStats.crit, i.itemStats.speed, i.itemStats.parry, Character.CalcGearVal(i, activeChar.currentWeights));
                    switch (i.equipSlot)
                    {
                        case GearSlot.MainHand:
                            CurrentWeapon.Items.Add(i);
                            CurrentWeaponA.Items.Add(i);
                            CurrentWeaponB.Items.Add(i);
                            break;
                        case GearSlot.OffHand:
                            CurrentOffHand.Items.Add(i);
                            CurrentOffHandA.Items.Add(i);
                            CurrentOffHandB.Items.Add(i);
                            break;
                        case GearSlot.Head:
                            CurrentHead.Items.Add(i);
                            CurrentHeadA.Items.Add(i);
                            CurrentHeadB.Items.Add(i);
                            break;
                        case GearSlot.Body:
                            CurrentBody.Items.Add(i);
                            CurrentBodyA.Items.Add(i);
                            CurrentBodyB.Items.Add(i);
                            break;
                        case GearSlot.Hands:
                            CurrentHands.Items.Add(i);
                            CurrentHandsA.Items.Add(i);
                            CurrentHandsB.Items.Add(i);
                            break;
                        case GearSlot.Waist:
                            CurrentWaist.Items.Add(i);
                            CurrentWaistA.Items.Add(i);
                            CurrentWaistB.Items.Add(i);
                            break;
                        case GearSlot.Legs:
                            CurrentLegs.Items.Add(i);
                            CurrentLegsA.Items.Add(i);
                            CurrentLegsB.Items.Add(i);
                            break;
                        case GearSlot.Feet:
                            CurrentFeet.Items.Add(i);
                            CurrentFeetA.Items.Add(i);
                            CurrentFeetB.Items.Add(i);
                            break;
                        case GearSlot.Neck:
                            CurrentNeck.Items.Add(i);
                            CurrentNeckA.Items.Add(i);
                            CurrentNeckB.Items.Add(i);
                            break;
                        case GearSlot.Ears:
                            CurrentEarring.Items.Add(i);
                            CurrentEarringA.Items.Add(i);
                            CurrentEarringB.Items.Add(i);
                            break;
                        case GearSlot.Wrists:
                            CurrentWrists.Items.Add(i);
                            CurrentWristsA.Items.Add(i);
                            CurrentWristsB.Items.Add(i);
                            break;
                        case GearSlot.Ring:
                            CurrentLRing.Items.Add(i);
                            CurrentRRing.Items.Add(i);
                            CurrentLRingA.Items.Add(i);
                            CurrentRRingA.Items.Add(i);
                            CurrentLRingB.Items.Add(i);
                            CurrentRRingB.Items.Add(i);
                            break;
                        default:
                            break;
                    }
                }
            }

            PopFood();

            try
            {
                PopBaseStats(activeChar.baseStats[(int)activeChar.currentJob]);
                PopGearSet(CurrentDamageGroup, activeChar.currentDamage[(int)activeChar.currentJob]);
                PopGearSet(CurrentCompareAGroup, activeChar.currentCoilFoodA[(int)activeChar.currentJob]);
                PopGearSet(CurrentCompareBGroup, activeChar.currentCoilFoodB[(int)activeChar.currentJob]);
                PopGearSet(OwnedDamageGroup, activeChar.ownedDamage[(int)activeChar.currentJob]);
                PopGearSet(OwnedCompareAGroup, activeChar.ownedCoilFoodA[(int)activeChar.currentJob]);
                PopGearSet(OwnedCompareBGroup, activeChar.ownedCoilFoodB[(int)activeChar.currentJob]);
                PopGearSet(ProgressionDamageGroup, activeChar.progressionDamage[(int)activeChar.currentJob]);
                PopGearSet(ProgressionCompareAGroup, activeChar.progressionCoilFoodA[(int)activeChar.currentJob]);
                PopGearSet(ProgressionCompareBGroup, activeChar.progressionCoilFoodB[(int)activeChar.currentJob]);
                PopGearSet(IdealDamageGroup, activeChar.idealDamage[(int)activeChar.currentJob]);
                PopGearSet(IdealCompareAGroup, activeChar.idealCoilFoodA[(int)activeChar.currentJob]);
                PopGearSet(IdealCompareBGroup, activeChar.idealCoilFoodB[(int)activeChar.currentJob]);
                PopGearValues();
                CustomEvents.ChangeCharacter(activeChar);
                FilterGear((GearSlotFilter.SelectedItem != null ? (string)GearSlotFilter.SelectedItem : ""), (HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
            }
            catch { }
        }

        private void PopFood()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new PopFoodBoxes(PopFood));
            }
            else
            {
                CurrentFood.Items.Clear();
                CurrentFood.Items.Add(new Food());
                OwnedFood.Items.Clear();
                OwnedFood.Items.Add(new Food());
                ProgressionFood.Items.Clear();
                ProgressionFood.Items.Add(new Food());
                IdealFood.Items.Clear();
                IdealFood.Items.Add(new Food());
                CurrentFoodA.Items.Clear();
                CurrentFoodA.Items.Add(new Food());
                OwnedFoodA.Items.Clear();
                OwnedFoodA.Items.Add(new Food());
                ProgressionFoodA.Items.Clear();
                ProgressionFoodA.Items.Add(new Food());
                IdealFoodA.Items.Clear();
                IdealFoodA.Items.Add(new Food());
                CurrentFoodB.Items.Clear();
                CurrentFoodB.Items.Add(new Food());
                OwnedFoodB.Items.Clear();
                OwnedFoodB.Items.Add(new Food());
                ProgressionFoodB.Items.Clear();
                ProgressionFoodB.Items.Add(new Food());
                IdealFoodB.Items.Clear();
                IdealFoodB.Items.Add(new Food());

                foreach (Food f in Common.foodDictionary.Values)
                {
                    CurrentFood.Items.Add(f);
                    OwnedFood.Items.Add(f);
                    ProgressionFood.Items.Add(f);
                    IdealFood.Items.Add(f);
                    CurrentFoodA.Items.Add(f);
                    OwnedFoodA.Items.Add(f);
                    ProgressionFoodA.Items.Add(f);
                    IdealFoodA.Items.Add(f);
                    CurrentFoodB.Items.Add(f);
                    OwnedFoodB.Items.Add(f);
                    ProgressionFoodB.Items.Add(f);
                    IdealFoodB.Items.Add(f);
                }
                if (activeChar != null)
                {
                    CurrentFood.Text = activeChar.currentDamage[(int)activeChar.currentJob].meal.name;
                    CurrentFoodA.Text = activeChar.currentCoilFoodA[(int)activeChar.currentJob].meal.name;
                    CurrentFoodB.Text = activeChar.currentCoilFoodB[(int)activeChar.currentJob].meal.name;

                    OwnedFood.Text = activeChar.ownedDamage[(int)activeChar.currentJob].meal.name;
                    OwnedFoodA.Text = activeChar.ownedCoilFoodA[(int)activeChar.currentJob].meal.name;
                    OwnedFoodB.Text = activeChar.ownedCoilFoodB[(int)activeChar.currentJob].meal.name;

                    ProgressionFood.Text = activeChar.progressionDamage[(int)activeChar.currentJob].meal.name;
                    ProgressionFoodA.Text = activeChar.progressionCoilFoodA[(int)activeChar.currentJob].meal.name;
                    ProgressionFoodB.Text = activeChar.progressionCoilFoodB[(int)activeChar.currentJob].meal.name;

                    IdealFood.SelectedIndexChanged -= IdealFood_SelectedIndexChanged;
                    IdealFood.Text = activeChar.idealDamage[(int)activeChar.currentJob].meal.name;
                    IdealFood.SelectedIndexChanged += IdealFood_SelectedIndexChanged;
                    IdealFoodA.Text = activeChar.idealCoilFoodA[(int)activeChar.currentJob].meal.name;
                    IdealFoodB.Text = activeChar.idealCoilFoodB[(int)activeChar.currentJob].meal.name;
                }
            }
        }

        private void PopBaseStats(Statistics baseStats)
        {
            BaseMainStat.ValueChanged -= BaseMainStat_ValueChanged;
            BaseVIT.ValueChanged -= BaseVIT_ValueChanged;
            BasePiety.ValueChanged -= BasePiety_ValueChanged;
            BaseDET.ValueChanged -= BaseDET_ValueChanged;
            BaseCrit.ValueChanged -= BaseCrit_ValueChanged;
            BaseAccuracy.ValueChanged -= BaseAccuracy_ValueChanged;
            BaseSpeed.ValueChanged -= BaseSpeed_ValueChanged;
            BaseParry.ValueChanged -= BaseParry_ValueChanged;

            BaseMainStat.Value = baseStats.mainStat;
            BaseVIT.Value = baseStats.vit;
            BasePiety.Value = baseStats.pie;
            BaseDET.Value = baseStats.det;
            BaseCrit.Value = baseStats.crit;
            BaseAccuracy.Value = baseStats.acc;
            BaseSpeed.Value = baseStats.speed;
            BaseParry.Value = baseStats.parry;

            BaseMainStat.ValueChanged += BaseMainStat_ValueChanged;
            BaseVIT.ValueChanged += BaseVIT_ValueChanged;
            BasePiety.ValueChanged += BasePiety_ValueChanged;
            BaseDET.ValueChanged += BaseDET_ValueChanged;
            BaseCrit.ValueChanged += BaseCrit_ValueChanged;
            BaseAccuracy.ValueChanged += BaseAccuracy_ValueChanged;
            BaseSpeed.ValueChanged += BaseSpeed_ValueChanged;
            BaseParry.ValueChanged += BaseParry_ValueChanged;
        }

        private void PopGearValues()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdGearValues(PopGearValues));
            }
            else
            {
                GearSet ideal;
                try
                {
                    ideal = activeChar.idealDamage[(int)activeChar.currentJob];
                }
                catch
                {
                    ideal = new GearSet();
                    ideal.baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                }
                activeChar.currentWeights = ideal.gearWeights;
                foreach (DataGridViewRow row in GearDisplayGridView.Rows)
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

        private void PopGearSet(GroupBox gearBox, GearSet set)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new PopGearSetBox(PopGearSet), new object[] { gearBox, set });
            }
            else
            {
                set.CalcGearStats();
                set.CalcTotalStats();
                if (gearBox.Name == "IdealDamageGroup")
                {
                    weightLabel.Text = "Weights:\n" + set.gearWeights.ToString(activeChar.currentJob);
                }
                foreach (Control c in gearBox.Controls)
                {
                    if (c.Name.Contains("Weapon"))
                    {
                        c.Text = set.mainHand.ToString();
                        if (c is TextBox)
                        {
                            if (set.mainHand.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("OffHand"))
                    {
                        c.Text = set.offHand.ToString();
                        if (c is TextBox)
                        {
                            if (set.offHand.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                        else if (c is ComboBox)
                        {
                            c.Enabled = !set.mainHand.twoHand;
                        }
                    }
                    else if (c.Name.Contains("Head"))
                    {
                        c.Text = set.head.ToString();
                        if (c is TextBox)
                        {
                            if (set.head.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Body"))
                    {
                        c.Text = set.body.ToString();
                        if (c is TextBox)
                        {
                            if (set.body.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Hands"))
                    {
                        c.Text = set.hands.ToString();
                        if (c is TextBox)
                        {
                            if (set.hands.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Waist"))
                    {
                        c.Text = set.waist.ToString();
                        if (c is TextBox)
                        {
                            if (set.waist.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Legs"))
                    {
                        c.Text = set.legs.ToString();
                        if (c is TextBox)
                        {
                            if (set.legs.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Feet"))
                    {
                        c.Text = set.feet.ToString();
                        if (c is TextBox)
                        {
                            if (set.feet.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Neck"))
                    {
                        c.Text = set.neck.ToString();
                        if (c is TextBox)
                        {
                            if (set.neck.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Earring"))
                    {
                        c.Text = set.ears.ToString();
                        if (c is TextBox)
                        {
                            if (set.ears.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Wrist"))
                    {
                        c.Text = set.wrists.ToString();
                        if (c is TextBox)
                        {
                            if (set.wrists.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("LRing"))
                    {
                        c.Text = set.leftRing.ToString();
                        if (c is TextBox)
                        {
                            if (set.leftRing.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                        else if (c is ComboBox && set.rightRing.unique && ((ComboBox)c).Items.Contains(set.rightRing))
                        {
                            ((ComboBox)c).Items.Remove(set.rightRing);
                        }
                    }
                    else if (c.Name.Contains("RRing"))
                    {
                        c.Text = set.rightRing.ToString();
                        if (c is TextBox)
                        {
                            if (set.rightRing.itemStats.acc > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                        else if (c is ComboBox && set.leftRing.unique && ((ComboBox)c).Items.Contains(set.leftRing))
                        {
                            ((ComboBox)c).Items.Remove(set.leftRing);
                        }
                    }
                    else if (c.Name.Contains("Food"))
                    {
                        c.Text = set.meal.ToString();
                        if (c is TextBox)
                        {
                            if (set.meal.accCap > 0)
                            {
                                ((TextBox)c).BackColor = Color.Green;
                                ((TextBox)c).ForeColor = Color.White;
                            }
                            else
                            {
                                ((TextBox)c).BackColor = System.Drawing.SystemColors.Control;
                                ((TextBox)c).ForeColor = Color.Black;
                            }
                        }
                    }
                    else if (c.Name.Contains("Accuracy"))
                    {
                        ((TextBox)c).Text = set.totalStats.acc.ToString();
                    }
                    else if (c.Name.Contains("Value"))
                    {
                        ((TextBox)c).Text = set.totalStats.Value(activeChar.currentWeights).ToString();
                    }
                    if (!(c.Name.Contains("Accuracy") || c.Name.Contains("Value")))
                    {
                        toolTip1.SetToolTip(c, c.Text);
                    }
                }
                toolTip1.SetToolTip(gearBox, set.totalStats.ToString());
            }
        }

        private void JobSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            activeChar.currentJob = (Job)JobSelect.SelectedItem;
            if (activeChar.baseStats.Length <= (int)activeChar.currentJob)
            {
                GearSet[] newCurrentDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newCurrentCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newCurrentCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newOwnedDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newOwnedCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newOwnedCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newProgressionDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newProgressionCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newProgressionCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newIdealDamage = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newIdealCoilFoodA = new GearSet[Enum.GetValues(typeof(Job)).Length];
                GearSet[] newIdealCoilFoodB = new GearSet[Enum.GetValues(typeof(Job)).Length];
                Statistics[] newBaseStats = new Statistics[Enum.GetValues(typeof(Job)).Length];
                double[] newTomeTier = new double[Enum.GetValues(typeof(Job)).Length];
                int[] newRelicTier = new int[Enum.GetValues(typeof(Job)).Length];
                int[] newAccuracyRequirements = new int[Enum.GetValues(typeof(Job)).Length];
                int[] newOwnedAccReqListA = new int[Enum.GetValues(typeof(Job)).Length];
                int[] newOwnedAccReqListB = new int[Enum.GetValues(typeof(Job)).Length];
                int[] newAccuracyNeeds = new int[Enum.GetValues(typeof(Job)).Length];
                int[] newSpeedBreakPoints = new int[Enum.GetValues(typeof(Job)).Length];
                for (int i = 0; i < activeChar.currentDamage.Length; i++)
                {
                    newCurrentDamage[i] = activeChar.currentDamage[i];
                    newCurrentCoilFoodA[i] = activeChar.currentCoilFoodA[i];
                    newCurrentCoilFoodB[i] = activeChar.currentCoilFoodB[i];
                    newOwnedDamage[i] = activeChar.ownedDamage[i];
                    newOwnedCoilFoodA[i] = activeChar.ownedCoilFoodA[i];
                    newOwnedCoilFoodB[i] = activeChar.ownedCoilFoodB[i];
                    newProgressionDamage[i] = activeChar.progressionDamage[i];
                    newProgressionCoilFoodA[i] = activeChar.progressionCoilFoodA[i];
                    newProgressionCoilFoodB[i] = activeChar.progressionCoilFoodB[i];
                    newIdealDamage[i] = activeChar.idealDamage[i];
                    newIdealCoilFoodA[i] = activeChar.idealCoilFoodA[i];
                    newIdealCoilFoodB[i] = activeChar.idealCoilFoodB[i];
                    newBaseStats[i] = activeChar.baseStats[i];
                    newTomeTier[i] = activeChar.tomeTier[i];
                    newRelicTier[i] = activeChar.relicTier[i];
                    newAccuracyRequirements[i] = Common.accuracyRequirements[i];
                    newAccuracyNeeds[i] = activeChar.accuracyNeeds[i];
                    newOwnedAccReqListA[i] = activeChar.ownedAccReqListA[i];
                    newOwnedAccReqListB[i] = activeChar.ownedAccReqListB[i];
                    newSpeedBreakPoints[i] = Common.speedBreakPoints[i];
                }
                newCurrentDamage[(int)activeChar.currentJob] = new GearSet();
                newCurrentCoilFoodA[(int)activeChar.currentJob] = new GearSet();
                newCurrentCoilFoodB[(int)activeChar.currentJob] = new GearSet();
                newOwnedDamage[(int)activeChar.currentJob] = new GearSet();
                newOwnedCoilFoodA[(int)activeChar.currentJob] = new GearSet();
                newOwnedCoilFoodB[(int)activeChar.currentJob] = new GearSet();
                newProgressionDamage[(int)activeChar.currentJob] = new GearSet();
                newProgressionCoilFoodA[(int)activeChar.currentJob] = new GearSet();
                newProgressionCoilFoodB[(int)activeChar.currentJob] = new GearSet();
                newIdealDamage[(int)activeChar.currentJob] = new GearSet();
                newIdealCoilFoodA[(int)activeChar.currentJob] = new GearSet();
                newIdealCoilFoodB[(int)activeChar.currentJob] = new GearSet();
                newBaseStats[(int)activeChar.currentJob] = new Statistics();
                newTomeTier[(int)activeChar.currentJob] = 0;
                newRelicTier[(int)activeChar.currentJob] = 0;
                try
                {
                    newAccuracyRequirements[(int)activeChar.currentJob] = Common.accuracyRequirements[(int)activeChar.currentJob];
                    newAccuracyNeeds[(int)activeChar.currentJob] = Common.accuracyRequirements[(int)activeChar.currentJob];
                    newOwnedAccReqListA[(int)activeChar.currentJob] = Common.accuracyRequirements[(int)activeChar.currentJob];
                    newOwnedAccReqListB[(int)activeChar.currentJob] = Common.accuracyRequirements[(int)activeChar.currentJob];
                    newSpeedBreakPoints[(int)activeChar.currentJob] = Common.accuracyRequirements[(int)activeChar.currentJob];
                }
                catch
                {
                    newAccuracyRequirements[(int)activeChar.currentJob] = 341;
                    newAccuracyNeeds[(int)activeChar.currentJob] = 341;
                    newOwnedAccReqListA[(int)activeChar.currentJob] = 341;
                    newOwnedAccReqListB[(int)activeChar.currentJob] = 341;
                    newSpeedBreakPoints[(int)activeChar.currentJob] = 341;
                }

                activeChar.currentDamage = newCurrentDamage;
                activeChar.currentCoilFoodA = newCurrentCoilFoodA;
                activeChar.currentCoilFoodB = newCurrentCoilFoodB;
                activeChar.ownedDamage = newOwnedDamage;
                activeChar.ownedCoilFoodA = newOwnedCoilFoodA;
                activeChar.ownedCoilFoodB = newOwnedCoilFoodB;
                activeChar.progressionDamage = newProgressionDamage;
                activeChar.progressionCoilFoodA = newProgressionCoilFoodA;
                activeChar.progressionCoilFoodB = newProgressionCoilFoodB;
                activeChar.idealDamage = newIdealDamage;
                activeChar.idealCoilFoodA = newIdealCoilFoodA;
                activeChar.idealCoilFoodB = newIdealCoilFoodB;
                activeChar.baseStats = newBaseStats;
                activeChar.tomeTier = newTomeTier;
                activeChar.relicTier = newRelicTier;
                activeChar.accuracyNeeds = newAccuracyNeeds;
                activeChar.ownedAccReqListA = newOwnedAccReqListA;
                activeChar.ownedAccReqListB = newOwnedAccReqListB;
                Common.accuracyRequirements = newAccuracyRequirements;
                Common.speedBreakPoints = newSpeedBreakPoints;
            }
            if (activeChar.baseStats[(int)activeChar.currentJob].mainStat == 0)
            {
                activeChar.SetBaseStats(new Statistics(0, 0, (int)BaseMainStat.Value, (int)BaseVIT.Value, (int)BaseDET.Value, (int)BaseCrit.Value, (int)BaseSpeed.Value, (int)BaseAccuracy.Value, (int)BasePiety.Value, (int)BaseParry.Value), activeChar.currentJob);
            }
            PopulateForm();
        }

        private void AddCharButton_Click(object sender, EventArgs e)
        {
            AddCharacterForm newCharForm = new AddCharacterForm();
            if (newCharForm.ShowDialog() == DialogResult.OK)
            {
                Character newChar = new Character();
                newChar.charName = newCharForm.CharacterName;
                if (!Common.charDictionary.Keys.Contains(newChar.charName))
                {
                    Common.charDictionary.Add(newChar.charName, newChar);
                    activeChar = newChar;
                    //activeChar.baseStats = new Statistics(0, 0, (int)BaseMainStat.Value, (int)BaseVIT.Value, (int)BaseDET.Value, (int)BaseCrit.Value, (int)BaseSpeed.Value, (int)BaseAccuracy.Value);
                    CharacterSelect.Items.Add(activeChar);
                    CharacterSelect.SelectedIndex = CharacterSelect.Items.IndexOf(activeChar);
                    JobSelect.SelectedIndex = JobSelect.Items.IndexOf(activeChar.currentJob);
                }
            }
        }

        private void DeleteCharButton_Click(object sender, EventArgs e)
        {
            int tempIndex = CharacterSelect.SelectedIndex;
            Common.charDictionary.Remove(((Character)CharacterSelect.SelectedItem).charName);
            CharacterSelect.Items.Remove(CharacterSelect.SelectedItem);
            try
            {
                CharacterSelect.SelectedItem = CharacterSelect.Items[tempIndex];
                PopulateForm();
            }
            catch
            {
                activeChar = null;
                ClearForm();
            }
        }

        private void ClearForm()
        {
            CharacterSelect.Text = "";
        }

        private void RecalcProgression_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.progressionDamage[(int)activeChar.currentJob], ProgressionDamageGroup, activeChar.clearedTurn, 0, activeChar.tomeTier[(int)activeChar.currentJob], false, activeChar.relicTier[(int)activeChar.currentJob], (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcProgressionA_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.progressionCoilFoodA[(int)activeChar.currentJob], ProgressionCompareAGroup, activeChar.clearedTurn, activeChar.accuracyNeeds[(int)activeChar.currentJob], activeChar.tomeTier[(int)activeChar.currentJob], false, activeChar.relicTier[(int)activeChar.currentJob], (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcProgressionB_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.progressionCoilFoodB[(int)activeChar.currentJob], ProgressionCompareBGroup, activeChar.clearedTurn, activeChar.accuracyNeeds[(int)activeChar.currentJob], activeChar.tomeTier[(int)activeChar.currentJob], false, activeChar.relicTier[(int)activeChar.currentJob], (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcIdeal_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.idealDamage[(int)activeChar.currentJob], IdealDamageGroup, Common.HighestTurn, 0, (LimitIdealTomeTier.Checked ? activeChar.tomeTier[(int)activeChar.currentJob] : -1), false, (LimitIdealRelicTier.Checked ? activeChar.relicTier[(int)activeChar.currentJob] : -1), (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcIdealA_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.idealCoilFoodA[(int)activeChar.currentJob], IdealCompareAGroup, Common.HighestTurn, activeChar.accuracyNeeds[(int)activeChar.currentJob], (LimitIdealTomeTier.Checked ? activeChar.tomeTier[(int)activeChar.currentJob] : -1), false, (LimitIdealRelicTier.Checked ? activeChar.relicTier[(int)activeChar.currentJob] : -1), (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcIdealB_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.idealCoilFoodB[(int)activeChar.currentJob], IdealCompareBGroup, Common.HighestTurn, activeChar.accuracyNeeds[(int)activeChar.currentJob], (LimitIdealTomeTier.Checked ? activeChar.tomeTier[(int)activeChar.currentJob] : -1), false, (LimitIdealRelicTier.Checked ? activeChar.relicTier[(int)activeChar.currentJob] : -1), (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcOwned_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.ownedDamage[(int)activeChar.currentJob], OwnedDamageGroup, Common.HighestTurn, 0, (LimitIdealTomeTier.Checked ? activeChar.tomeTier[(int)activeChar.currentJob] : -1), true, (LimitIdealRelicTier.Checked ? activeChar.relicTier[(int)activeChar.currentJob] : -1), (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcOwnedA_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.ownedCoilFoodA[(int)activeChar.currentJob], OwnedCompareAGroup, Common.HighestTurn, (OwnedSpecificAccCheckbox.Checked ? activeChar.ownedAccReqListA[(int)activeChar.currentJob] : activeChar.accuracyNeeds[(int)activeChar.currentJob]), (LimitIdealTomeTier.Checked ? activeChar.tomeTier[(int)activeChar.currentJob] : -1), true, (LimitIdealRelicTier.Checked ? activeChar.relicTier[(int)activeChar.currentJob] : -1), (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalcOwnedB_Click(object sender, EventArgs e)
        {
            calcThread = new Thread(() => RecalculateGearSet(ref activeChar.ownedCoilFoodB[(int)activeChar.currentJob], OwnedCompareBGroup, Common.HighestTurn, (OwnedSpecificAccCheckbox.Checked ? activeChar.ownedAccReqListB[(int)activeChar.currentJob] : activeChar.accuracyNeeds[(int)activeChar.currentJob]), (LimitIdealTomeTier.Checked ? activeChar.tomeTier[(int)activeChar.currentJob] : -1), true, (LimitIdealRelicTier.Checked ? activeChar.relicTier[(int)activeChar.currentJob] : -1), (Common.UseSpeedBreakPoint ? Common.speedBreakPoints[(int)activeChar.currentJob] : 341)));
            calcThread.Start();
        }

        private void RecalculateGearSet(ref GearSet startingSet, GroupBox popBox, int turnLimit, int accuracyReq, double tomeTierLimit, bool ownedGearOnly, int relicTierLimit, int speedBreakPoint = 341)
        {
            ActivateButtons(false);
            StartProgressBar();

            startingSet = CalcGearSet(activeChar.currentJob, startingSet, turnLimit, accuracyReq, tomeTierLimit, ownedGearOnly, relicTierLimit, speedBreakPoint);
            if (popBox.Name == "IdealDamageGroup")
            {
                activeChar.currentWeights = activeChar.idealDamage[(int)activeChar.currentJob].gearWeights;
                UpdGearValDisplay((int)activeChar.currentJob);
                PopGearValues();
                CustomEvents.UpdateCharacter(activeChar);
            }
            PopGearSet(popBox, startingSet);
            SelectTab((TabPage)popBox.Parent);

            CloseProgressBar();
            ActivateButtons(true);
        }

        private void StartProgressBar()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new StartProgressBar(StartProgressBar));
            }
            else
            {
                if (!progressBar1.Visible)
                {
                    progressBar1.Maximum = 100;
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    progressBar1.Visible = true;
                    progressBar1.MarqueeAnimationSpeed = 50;
                }
            }
        }

        private void SetProgressBarMaximum(int max)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetProgressBarMax(SetProgressBarMaximum), new object[] { max });
            }
            else
            {
                progressBar1.Maximum = max;
                progressBar1.Style = ProgressBarStyle.Continuous;
            }
        }

        private void IncrementProgressBar()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new IncrementProgressBar(IncrementProgressBar));
            }
            else
            {
                if (progressBar1.Value < progressBar1.Maximum)
                {
                    progressBar1.Value++;
                }
            }
        }

        private void CloseProgressBar()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new StopProgressBar(CloseProgressBar));
            }
            else
            {
                progressBar1.Visible = false;
            }
        }

        private void ActivateButtons(bool active)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ActivateCalcButtons(ActivateButtons), new object[] { active });
            }
            else
            {
                CharacterSelect.Enabled = active;
                JobSelect.Enabled = active;
                DeleteCharButton.Enabled = active;
                AddCharButton.Enabled = active;
                RecalcOwned.Enabled = active;
                RecalcOwnedA.Enabled = active;
                RecalcOwnedB.Enabled = active;
                RecalcProgression.Enabled = active;
                RecalcProgressionA.Enabled = active;
                RecalcProgressionB.Enabled = active;
                RecalcIdeal.Enabled = active;
                RecalcIdealA.Enabled = active;
                RecalcIdealB.Enabled = active;
            }
        }

        private void SelectTab(TabPage tab)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SelectTabPage(SelectTab), new object[] { tab });
            }
            else
            {
                GearSetsTabControl.SelectedTab = tab;
            }
        }


        private void IdealFood_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.idealDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)IdealFood.SelectedItem)
            {
                activeSet.meal = (Food)IdealFood.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                /*StatWeights idealWeights = activeSet.CalcStatWeights(activeChar.currentJob, Common.SimulateWeights);
                activeChar.currentWeights = idealWeights;
                weightLabel.Text = "Weights:\n" + idealWeights.ToString(activeChar.currentJob);
                UpdGearValDisplay((int)activeChar.currentJob);
                CurrentValue.Text = activeChar.currentDamage[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();
                CurrentValueA.Text = activeChar.currentCoilFoodA[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();
                CurrentValueB.Text = activeChar.currentCoilFoodB[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();
                ProgressionValue.Text = activeChar.progressionDamage[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();
                ProgressionValueA.Text = activeChar.progressionCoilFoodA[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();
                ProgressionValueB.Text = activeChar.progressionCoilFoodB[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();*/
                IdealValue.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                /*				IdealValueA.Text = activeChar.idealCoilFoodA[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();
                                IdealValueB.Text = activeChar.idealCoilFoodB[(int)activeChar.currentJob].totalStats.Value(idealWeights).ToString();*/
                IdealAccuracy.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(IdealDamageGroup, activeSet.totalStats.ToString());
                activeChar.idealDamage[(int)activeChar.currentJob] = activeSet;
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void IdealFoodA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.idealCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)IdealFoodA.SelectedItem)
            {
                activeSet.meal = (Food)IdealFoodA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                IdealValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                IdealAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(IdealCompareAGroup, activeSet.totalStats.ToString());
                activeChar.idealCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void IdealFoodB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.idealCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)IdealFoodB.SelectedItem)
            {
                activeSet.meal = (Food)IdealFoodB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                IdealValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                IdealAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(IdealCompareBGroup, activeSet.totalStats.ToString());
                activeChar.idealCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void OwnedFood_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.ownedDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)OwnedFood.SelectedItem)
            {
                activeSet.meal = (Food)OwnedFood.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                OwnedValue.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                OwnedAccuracy.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(OwnedDamageGroup, activeSet.totalStats.ToString());
                activeChar.ownedDamage[(int)activeChar.currentJob] = activeSet;
            }

        }

        private void OwnedFoodA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.ownedCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)OwnedFoodA.SelectedItem)
            {
                activeSet.meal = (Food)OwnedFoodA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                OwnedValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                OwnedAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(OwnedCompareAGroup, activeSet.totalStats.ToString());
                activeChar.ownedCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void OwnedFoodB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.ownedCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)OwnedFoodB.SelectedItem)
            {
                activeSet.meal = (Food)OwnedFoodB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                OwnedValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                OwnedAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(OwnedCompareBGroup, activeSet.totalStats.ToString());
                activeChar.ownedCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void ProgressionFood_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.progressionDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)ProgressionFood.SelectedItem)
            {
                activeSet.meal = (Food)ProgressionFood.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                ProgressionValue.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                ProgressionAccuracy.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(ProgressionDamageGroup, activeSet.totalStats.ToString());
                activeChar.progressionDamage[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void ProgressionFoodA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.progressionCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)ProgressionFoodA.SelectedItem)
            {
                activeSet.meal = (Food)ProgressionFoodA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                ProgressionValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                ProgressionAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(ProgressionCompareAGroup, activeSet.totalStats.ToString());
                activeChar.progressionCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void ProgressionFoodB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.progressionCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)ProgressionFoodB.SelectedItem)
            {
                activeSet.meal = (Food)ProgressionFoodB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                ProgressionValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                ProgressionAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(ProgressionCompareBGroup, activeSet.totalStats.ToString());
                activeChar.progressionCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.mainHand != (Item)CurrentWeapon.SelectedItem)
            {
                activeSet.mainHand = (Item)CurrentWeapon.SelectedItem;
                if (activeSet.mainHand.twoHand)
                {
                    activeSet.offHand = new Item();
                    CurrentOffHand.Text = "";
                    CurrentOffHand.Enabled = false;
                }
                else
                {
                    CurrentOffHand.Enabled = true;
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentHead_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.head != (Item)CurrentHead.SelectedItem)
            {
                activeSet.head = (Item)CurrentHead.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentBody_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.body != (Item)CurrentBody.SelectedItem)
            {
                activeSet.body = (Item)CurrentBody.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentHands_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.hands != (Item)CurrentHands.SelectedItem)
            {
                activeSet.hands = (Item)CurrentHands.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentWaist_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.waist != (Item)CurrentWaist.SelectedItem)
            {
                activeSet.waist = (Item)CurrentWaist.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentLegs_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.legs != (Item)CurrentLegs.SelectedItem)
            {
                activeSet.legs = (Item)CurrentLegs.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentFeet_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.feet != (Item)CurrentFeet.SelectedItem)
            {
                activeSet.feet = (Item)CurrentFeet.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentOffHand_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.offHand != (Item)CurrentOffHand.SelectedItem)
            {
                activeSet.offHand = (Item)CurrentOffHand.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentNeck_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.neck != (Item)CurrentNeck.SelectedItem)
            {
                activeSet.neck = (Item)CurrentNeck.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentEarring_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.ears != (Item)CurrentEarring.SelectedItem)
            {
                activeSet.ears = (Item)CurrentEarring.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentWrists_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.wrists != (Item)CurrentWrists.SelectedItem)
            {
                activeSet.wrists = (Item)CurrentWrists.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentLRing_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.leftRing != (Item)CurrentLRing.SelectedItem)
            {
                if (!CurrentRRing.Items.Contains(activeSet.leftRing) && !string.IsNullOrWhiteSpace(activeSet.leftRing.name))
                {
                    CurrentRRing.Items.Add(activeSet.leftRing);
                }
                activeSet.leftRing = (Item)CurrentLRing.SelectedItem;
                if (activeSet.leftRing.unique && CurrentRRing.Items.Contains(activeSet.leftRing))
                {
                    CurrentRRing.Items.Remove(activeSet.leftRing);
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentRRing_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.rightRing != (Item)CurrentRRing.SelectedItem)
            {
                if (!CurrentLRing.Items.Contains(activeSet.rightRing) && !string.IsNullOrWhiteSpace(activeSet.rightRing.name))
                {
                    CurrentLRing.Items.Add(activeSet.rightRing);
                }
                activeSet.rightRing = (Item)CurrentRRing.SelectedItem;
                if (activeSet.rightRing.unique && CurrentLRing.Items.Contains(activeSet.rightRing))
                {
                    CurrentLRing.Items.Remove(activeSet.rightRing);
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentFood_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentDamage[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)CurrentFood.SelectedItem)
            {
                activeSet.meal = (Food)CurrentFood.SelectedItem;
                activeSet.CalcTotalStats();
                activeChar.currentDamage[(int)activeChar.currentJob] = activeSet;
                toolTip1.SetToolTip(CurrentDamageGroup, activeSet.totalStats.ToString());
                PopGearValues();
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void CurrentWeaponA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.mainHand != (Item)CurrentWeaponA.SelectedItem)
            {
                activeSet.mainHand = (Item)CurrentWeaponA.SelectedItem;
                if (activeSet.mainHand.twoHand)
                {
                    activeSet.offHand = new Item();
                    CurrentOffHandA.SelectedItem = new Item();
                    CurrentOffHandA.Text = "";
                    CurrentOffHandA.Enabled = false;
                }
                else
                {
                    CurrentOffHandA.Enabled = true;
                }
                activeSet.mainHand = (Item)CurrentWeaponA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentHeadA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.head != (Item)CurrentHeadA.SelectedItem)
            {
                activeSet.head = (Item)CurrentHeadA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentBodyA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.body != (Item)CurrentBodyA.SelectedItem)
            {
                activeSet.body = (Item)CurrentBodyA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentHandsA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.hands != (Item)CurrentHandsA.SelectedItem)
            {
                activeSet.hands = (Item)CurrentHandsA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentWaistA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.waist != (Item)CurrentWaistA.SelectedItem)
            {
                activeSet.waist = (Item)CurrentWaistA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentLegsA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.legs != (Item)CurrentLegsA.SelectedItem)
            {
                activeSet.legs = (Item)CurrentLegsA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentFeetA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.feet != (Item)CurrentFeetA.SelectedItem)
            {
                activeSet.feet = (Item)CurrentFeetA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentOffHandA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.offHand != (Item)CurrentOffHandA.SelectedItem)
            {
                activeSet.offHand = (Item)CurrentOffHandA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentNeckA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.neck != (Item)CurrentNeckA.SelectedItem)
            {
                activeSet.neck = (Item)CurrentNeckA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentEarringA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.ears != (Item)CurrentEarringA.SelectedItem)
            {
                activeSet.ears = (Item)CurrentEarringA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentWristsA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.wrists != (Item)CurrentWristsA.SelectedItem)
            {
                activeSet.wrists = (Item)CurrentWristsA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentLRingA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.leftRing != (Item)CurrentLRingA.SelectedItem)
            {
                if (!CurrentRRingA.Items.Contains(activeSet.leftRing) && !string.IsNullOrWhiteSpace(activeSet.leftRing.name))
                {
                    CurrentRRingA.Items.Add(activeSet.leftRing);
                }
                activeSet.leftRing = (Item)CurrentLRingA.SelectedItem;
                if (activeSet.leftRing.unique && CurrentRRingA.Items.Contains(activeSet.leftRing))
                {
                    CurrentRRingA.Items.Remove(activeSet.leftRing);
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentRRingA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.rightRing != (Item)CurrentRRingA.SelectedItem)
            {
                if (!CurrentLRingA.Items.Contains(activeSet.rightRing) && !string.IsNullOrWhiteSpace(activeSet.rightRing.name))
                {
                    CurrentLRingA.Items.Add(activeSet.rightRing);
                }
                activeSet.rightRing = (Item)CurrentRRingA.SelectedItem;
                if (activeSet.rightRing.unique && CurrentLRingA.Items.Contains(activeSet.rightRing))
                {
                    CurrentLRingA.Items.Remove(activeSet.rightRing);
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentFoodA_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodA[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)CurrentFoodA.SelectedItem)
            {
                activeSet.meal = (Food)CurrentFoodA.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueA.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareAGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodA[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentWeaponB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.mainHand != (Item)CurrentWeaponB.SelectedItem)
            {
                activeSet.mainHand = (Item)CurrentWeaponB.SelectedItem;
                if (activeSet.mainHand.twoHand)
                {
                    activeSet.offHand = new Item();
                    CurrentOffHandB.Text = "";
                    CurrentOffHandB.Enabled = false;
                }
                else
                {
                    CurrentOffHandB.Enabled = true;
                }
                activeSet.mainHand = (Item)CurrentWeaponB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentHeadB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.head != (Item)CurrentHeadB.SelectedItem)
            {
                activeSet.head = (Item)CurrentHeadB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentBodyB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.body != (Item)CurrentBodyB.SelectedItem)
            {
                activeSet.body = (Item)CurrentBodyB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentHandsB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.hands != (Item)CurrentHandsB.SelectedItem)
            {
                activeSet.hands = (Item)CurrentHandsB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentWaistB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.waist != (Item)CurrentWaistB.SelectedItem)
            {
                activeSet.waist = (Item)CurrentWaistB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentLegsB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.legs != (Item)CurrentLegsB.SelectedItem)
            {
                activeSet.legs = (Item)CurrentLegsB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentFeetB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.feet != (Item)CurrentFeetB.SelectedItem)
            {
                activeSet.feet = (Item)CurrentFeetB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentOffHandB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.offHand != (Item)CurrentOffHandB.SelectedItem)
            {
                activeSet.offHand = (Item)CurrentOffHandB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentNeckB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.neck != (Item)CurrentNeckB.SelectedItem)
            {
                activeSet.neck = (Item)CurrentNeckB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentEarringB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.ears != (Item)CurrentEarringB.SelectedItem)
            {
                activeSet.ears = (Item)CurrentEarringB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentWristsB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.wrists != (Item)CurrentWristsB.SelectedItem)
            {
                activeSet.wrists = (Item)CurrentWristsB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentLRingB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.leftRing != (Item)CurrentLRingB.SelectedItem)
            {
                if (!CurrentRRingB.Items.Contains(activeSet.leftRing) && !string.IsNullOrWhiteSpace(activeSet.leftRing.name))
                {
                    CurrentRRingB.Items.Add(activeSet.leftRing);
                }
                activeSet.leftRing = (Item)CurrentLRingB.SelectedItem;
                if (activeSet.leftRing.unique && CurrentRRingB.Items.Contains(activeSet.leftRing))
                {
                    CurrentRRingB.Items.Remove(activeSet.leftRing);
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentRRingB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.rightRing != (Item)CurrentRRingB.SelectedItem)
            {
                if (!CurrentLRingB.Items.Contains(activeSet.rightRing) && !string.IsNullOrWhiteSpace(activeSet.rightRing.name))
                {
                    CurrentLRingB.Items.Add(activeSet.rightRing);
                }
                activeSet.rightRing = (Item)CurrentRRingB.SelectedItem;
                if (activeSet.rightRing.unique && CurrentLRingB.Items.Contains(activeSet.rightRing))
                {
                    CurrentLRingB.Items.Remove(activeSet.rightRing);
                }
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void CurrentFoodB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GearSet activeSet;
            try
            {
                activeSet = activeChar.currentCoilFoodB[(int)activeChar.currentJob];
            }
            catch
            {
                activeSet = new GearSet(activeChar.baseStats[(int)activeChar.currentJob]);
            }
            if (activeSet.meal != (Food)CurrentFoodB.SelectedItem)
            {
                activeSet.meal = (Food)CurrentFoodB.SelectedItem;
                activeSet.CalcGearStats();
                activeSet.CalcTotalStats();
                CurrentValueB.Text = activeSet.totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeSet.totalStats.acc.ToString();
                toolTip1.SetToolTip(CurrentCompareBGroup, activeSet.totalStats.ToString());
                activeChar.currentCoilFoodB[(int)activeChar.currentJob] = activeSet;
            }
        }

        private void UpdGearValDisplay(int jobIndex)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdGearSetValues(UpdGearValDisplay), new object[] { jobIndex });
            }
            else
            {
                CurrentAccuracy.Text = activeChar.currentDamage[jobIndex].totalStats.acc.ToString();
                CurrentValue.Text = activeChar.currentDamage[jobIndex].totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyA.Text = activeChar.currentCoilFoodA[jobIndex].totalStats.acc.ToString();
                CurrentValueA.Text = activeChar.currentCoilFoodA[jobIndex].totalStats.Value(activeChar.currentWeights).ToString();
                CurrentAccuracyB.Text = activeChar.currentCoilFoodB[jobIndex].totalStats.acc.ToString();
                CurrentValueB.Text = activeChar.currentCoilFoodB[jobIndex].totalStats.Value(activeChar.currentWeights).ToString();

                ProgressionAccuracy.Text = activeChar.progressionDamage[jobIndex].totalStats.acc.ToString();
                ProgressionValue.Text = activeChar.progressionDamage[jobIndex].totalStats.Value(activeChar.progressionDamage[jobIndex].gearWeights).ToString();
                ProgressionAccuracyA.Text = activeChar.progressionCoilFoodA[jobIndex].totalStats.acc.ToString();
                ProgressionValueA.Text = activeChar.progressionCoilFoodA[jobIndex].totalStats.Value(activeChar.progressionDamage[jobIndex].gearWeights).ToString();
                ProgressionAccuracyB.Text = activeChar.progressionCoilFoodB[jobIndex].totalStats.acc.ToString();
                ProgressionValueB.Text = activeChar.progressionCoilFoodB[jobIndex].totalStats.Value(activeChar.progressionDamage[jobIndex].gearWeights).ToString();

                IdealAccuracy.Text = activeChar.idealDamage[jobIndex].totalStats.acc.ToString();
                IdealValue.Text = activeChar.idealDamage[jobIndex].totalStats.Value(activeChar.idealDamage[jobIndex].gearWeights).ToString();
                IdealAccuracyA.Text = activeChar.idealCoilFoodA[jobIndex].totalStats.acc.ToString();
                IdealValueA.Text = activeChar.idealCoilFoodA[jobIndex].totalStats.Value(activeChar.idealDamage[jobIndex].gearWeights).ToString();
                IdealAccuracyB.Text = activeChar.idealCoilFoodB[jobIndex].totalStats.acc.ToString();
                IdealValueB.Text = activeChar.idealCoilFoodB[jobIndex].totalStats.Value(activeChar.idealDamage[jobIndex].gearWeights).ToString();
            }
        }

        private void UpdBaseStats(int jobIndex)
        {
            try
            {
                activeChar.currentDamage[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.currentDamage[jobIndex].CalcTotalStats();
                activeChar.currentCoilFoodA[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.currentCoilFoodA[jobIndex].CalcTotalStats();
                activeChar.currentCoilFoodB[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.currentCoilFoodB[jobIndex].CalcTotalStats();

                activeChar.progressionDamage[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.progressionDamage[jobIndex].CalcTotalStats();
                activeChar.progressionCoilFoodA[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.progressionCoilFoodA[jobIndex].CalcTotalStats();
                activeChar.progressionCoilFoodB[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.progressionCoilFoodB[jobIndex].CalcTotalStats();

                activeChar.idealDamage[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.idealDamage[jobIndex].CalcTotalStats();
                activeChar.idealCoilFoodA[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.idealCoilFoodA[jobIndex].CalcTotalStats();
                activeChar.idealCoilFoodB[jobIndex].baseStats = activeChar.baseStats[(int)activeChar.currentJob];
                activeChar.idealCoilFoodB[jobIndex].CalcTotalStats();

                //activeChar.currentWeights = activeChar.idealDamage[jobIndex].CalcStatWeights(activeChar.currentJob, Common.SimulateWeights);
                UpdGearValDisplay(jobIndex);
                PopGearValues();
                CustomEvents.UpdateCharacter(activeChar);
            }
            catch { }
        }

        private void BaseMainStat_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].mainStat = (int)BaseMainStat.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BaseVIT_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].vit = (int)BaseVIT.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BaseAccuracy_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].acc = (int)BaseAccuracy.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BaseDET_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].det = (int)BaseDET.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BaseCrit_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].crit = (int)BaseCrit.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BaseSpeed_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].speed = (int)BaseSpeed.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BasePiety_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].pie = (int)BasePiety.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void BaseParry_ValueChanged(object sender, EventArgs e)
        {
            activeChar.baseStats[(int)activeChar.currentJob].parry = (int)BaseParry.Value;
            UpdBaseStats((int)activeChar.currentJob);
        }

        private void HighestTurn_ValueChanged(object sender, EventArgs e)
        {
            activeChar.clearedTurn = (int)HighestTurn.Value;
        }

        private GearSet CalcGearSet(Job j, GearSet startSet, int turnLimit, int accReq, double tomeTierLimit, bool ownedGearOnly, double relicTierLimit, int speedBreakPoint)
        {
            //Load applicable gear into slot-specific lists
            List<Item> mainHands = new List<Item>();
            List<Item> offHands = new List<Item>();
            List<Item> heads = new List<Item>();
            List<Item> bodies = new List<Item>();
            List<Item> hands = new List<Item>();
            List<Item> waists = new List<Item>();
            List<Item> legs = new List<Item>();
            List<Item> feet = new List<Item>();
            List<Item> necks = new List<Item>();
            List<Item> ears = new List<Item>();
            List<Item> wrists = new List<Item>();
            List<Item> rings = new List<Item>();

            int maxiLevel = 0;
            foreach (Item i in Common.gearDictionary.Values)
            {
                if (i.canEquip.Contains(j) && (i.sourceTurn <= turnLimit || activeChar.ownedItems.Contains(i.name)) && (i.tomeTier <= tomeTierLimit || tomeTierLimit == -1) && (i.relicTier <= relicTierLimit || relicTierLimit == -1) &&
                    (!ownedGearOnly || activeChar.ownedItems.Contains(i.name)))
                {
                    if (maxiLevel < i.itemStats.itemLevel)
                    {
                        maxiLevel = i.itemStats.itemLevel;
                    }
                    switch (i.equipSlot)
                    {
                        case GearSlot.MainHand:
                            mainHands.Add(i);
                            break;
                        case GearSlot.OffHand:
                            offHands.Add(i);
                            break;
                        case GearSlot.Head:
                            heads.Add(i);
                            break;
                        case GearSlot.Body:
                            bodies.Add(i);
                            break;
                        case GearSlot.Hands:
                            hands.Add(i);
                            break;
                        case GearSlot.Waist:
                            waists.Add(i);
                            break;
                        case GearSlot.Legs:
                            legs.Add(i);
                            break;
                        case GearSlot.Feet:
                            feet.Add(i);
                            break;
                        case GearSlot.Neck:
                            necks.Add(i);
                            break;
                        case GearSlot.Ears:
                            ears.Add(i);
                            break;
                        case GearSlot.Wrists:
                            wrists.Add(i);
                            break;
                        case GearSlot.Ring:
                            rings.Add(i);
                            break;
                        default:
                            break;
                    }
                }
            }

            // Optimize for damage value first
            bool changesMade;
            bool changesThisSlot = false;
            int iterations = 0;
            GearSet tempSet;
            if (turnLimit < Common.HighestTurn || tomeTierLimit > -1 || relicTierLimit > -1 || accReq > 341 || ownedGearOnly)
            {
                tempSet = activeChar.idealDamage[(int)j].Clone();
            }
            else
            {
                tempSet = startSet.Clone();
            }
            if (tempSet.baseStats.mainStat == 0)
            {
                tempSet.baseStats = activeChar.baseStats[(int)j];
            }
            tempSet.meal = new Food();
            if (!(turnLimit < Common.HighestTurn || tomeTierLimit > -1 || relicTierLimit > -1 || accReq > 341 || ownedGearOnly))
            {
                tempSet.gearWeights = Calculation.CalcStatWeights(j, tempSet.totalStats, Common.SimulateWeights);
            }
            GearSet bestSet = tempSet.Clone();
            if (!mainHands.Contains(bestSet.mainHand))
            {
                bestSet.mainHand = new Item();
            }
            if (bestSet.offHand.itemStats.mainStat > 0 && (!offHands.Contains(bestSet.offHand) || bestSet.mainHand.twoHand))
            {
                bestSet.offHand = new Item();
            }
            if (!heads.Contains(bestSet.head))
            {
                bestSet.head = new Item();
            }
            if (!bodies.Contains(bestSet.body))
            {
                bestSet.body = new Item();
            }
            if (!hands.Contains(bestSet.hands))
            {
                bestSet.hands = new Item();
            }
            if (!waists.Contains(bestSet.waist))
            {
                bestSet.waist = new Item();
            }
            if (!legs.Contains(bestSet.legs))
            {
                bestSet.legs = new Item();
            }
            if (!feet.Contains(bestSet.feet))
            {
                bestSet.feet = new Item();
            }
            if (!necks.Contains(bestSet.neck))
            {
                bestSet.neck = new Item();
            }
            if (!ears.Contains(bestSet.ears))
            {
                bestSet.ears = new Item();
            }
            if (!wrists.Contains(bestSet.wrists))
            {
                bestSet.wrists = new Item();
            }
            if (!rings.Contains(bestSet.rightRing) || (bestSet.rightRing.unique && bestSet.rightRing == bestSet.leftRing))
            {
                bestSet.rightRing = new Item();
            }
            if (!rings.Contains(bestSet.leftRing) || (bestSet.leftRing.unique && bestSet.leftRing == bestSet.rightRing))
            {
                bestSet.leftRing = new Item();
            }
            bestSet.CalcGearStats();
            bestSet.CalcTotalStats();

            GearSet prevSet = tempSet.Clone();
            GearSet prevSet2 = prevSet.Clone();
            do
            {
                changesMade = false;
                changesThisSlot = false;
                foreach (Item i in mainHands)
                {
                    Item tempOffHand = tempSet.offHand;
                    if (!i.twoHand && tempSet.mainHand.twoHand)
                    {
                        foreach (Item i2 in offHands)
                        {
                            if (!offHands.Contains(tempOffHand) || i2.itemStats.Value(tempSet.gearWeights) > tempOffHand.itemStats.Value(tempSet.gearWeights))
                            {
                                tempOffHand = i2;
                            }
                        }
                        if (!mainHands.Contains(tempSet.mainHand) || (i.itemStats + tempOffHand.itemStats).Value(tempSet.gearWeights) > (tempSet.mainHand.itemStats + tempSet.offHand.itemStats).Value(tempSet.gearWeights))
                        {
                            tempSet.mainHand = i;
                            tempSet.offHand = tempOffHand;
                            changesThisSlot = (tempSet.mainHand != prevSet.mainHand || tempSet.offHand != prevSet.offHand);
                        }
                    }
                    else
                    {
                        if (!mainHands.Contains(tempSet.mainHand) || i.itemStats.Value(tempSet.gearWeights) > tempSet.mainHand.itemStats.Value(tempSet.gearWeights))
                        {
                            tempSet.mainHand = i;
                            tempSet.offHand = new Item();
                            changesThisSlot = tempSet.mainHand != prevSet.mainHand;
                        }
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                if (!tempSet.mainHand.twoHand)
                {
                    foreach (Item i in offHands)
                    {
                        if (!offHands.Contains(tempSet.offHand) || i.itemStats.Value(tempSet.gearWeights) > tempSet.offHand.itemStats.Value(tempSet.gearWeights))
                        {
                            tempSet.offHand = i;
                            changesThisSlot = tempSet.offHand != prevSet.offHand;
                        }
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in heads)
                {
                    if (!heads.Contains(tempSet.head) || i.itemStats.Value(tempSet.gearWeights) > tempSet.head.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.head = i;
                        changesThisSlot = tempSet.head != prevSet.head;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in bodies)
                {
                    if (!bodies.Contains(tempSet.body) || i.itemStats.Value(tempSet.gearWeights) > tempSet.body.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.body = i;
                        changesThisSlot = tempSet.body != prevSet.body;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in hands)
                {
                    if (!hands.Contains(tempSet.hands) || i.itemStats.Value(tempSet.gearWeights) > tempSet.hands.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.hands = i;
                        changesThisSlot = tempSet.hands != prevSet.hands;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in waists)
                {
                    if (!waists.Contains(tempSet.waist) || i.itemStats.Value(tempSet.gearWeights) > tempSet.waist.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.waist = i;
                        changesThisSlot = tempSet.waist != prevSet.waist;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in legs)
                {
                    if (!legs.Contains(tempSet.legs) || i.itemStats.Value(tempSet.gearWeights) > tempSet.legs.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.legs = i;
                        changesThisSlot = tempSet.legs != prevSet.legs;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in feet)
                {
                    if (!feet.Contains(tempSet.feet) || i.itemStats.Value(tempSet.gearWeights) > tempSet.feet.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.feet = i;
                        changesThisSlot = tempSet.feet != prevSet.feet;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in necks)
                {
                    if (!necks.Contains(tempSet.neck) || i.itemStats.Value(tempSet.gearWeights) > tempSet.neck.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.neck = i;
                        changesThisSlot = tempSet.neck != prevSet.neck;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in ears)
                {
                    if (!ears.Contains(tempSet.ears) || i.itemStats.Value(tempSet.gearWeights) > tempSet.ears.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.ears = i;
                        changesThisSlot = tempSet.ears != prevSet.ears;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in wrists)
                {
                    if (!wrists.Contains(tempSet.wrists) || i.itemStats.Value(tempSet.gearWeights) > tempSet.wrists.itemStats.Value(tempSet.gearWeights))
                    {
                        tempSet.wrists = i;
                        changesThisSlot = tempSet.wrists != prevSet.wrists;
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in rings)
                {
                    if ((!rings.Contains(tempSet.leftRing) || i.itemStats.Value(tempSet.gearWeights) > tempSet.leftRing.itemStats.Value(tempSet.gearWeights)) &&
                        (i.unique ? !i.name.Equals(tempSet.rightRing.name) : true))
                    {
                        tempSet.leftRing = i;
                        changesThisSlot = (tempSet.leftRing != prevSet.leftRing || tempSet.rightRing != prevSet.rightRing);
                    }
                }
                changesMade = changesMade || changesThisSlot;
                changesThisSlot = false;
                foreach (Item i in rings)
                {
                    if ((!rings.Contains(tempSet.rightRing) || i.itemStats.Value(tempSet.gearWeights) > tempSet.rightRing.itemStats.Value(tempSet.gearWeights)) &&
                        (i.unique ? !i.name.Equals(tempSet.leftRing.name) : true))
                    {
                        tempSet.rightRing = i;
                        changesThisSlot = (tempSet.leftRing != prevSet.leftRing || tempSet.rightRing != prevSet.rightRing);
                    }
                }
                changesMade = changesMade || changesThisSlot;

                if (changesMade)
                {
                    tempSet.CalcGearStats();
                    tempSet.CalcTotalStats();
                    if (tempSet.IsEqual(bestSet))
                    {
                        tempSet.gearWeights = bestSet.gearWeights;
                        iterations++;
                        continue;
                    }
                    else if (tempSet.IsEqual(prevSet2))
                    {
                        tempSet.gearWeights = prevSet2.gearWeights;
                        iterations++;
                        continue;
                    }
                    else if (!(turnLimit < Common.HighestTurn || tomeTierLimit > -1 || relicTierLimit > -1 || accReq > 341 || ownedGearOnly))
                    {
                        tempSet.gearWeights = Calculation.CalcStatWeights(j, tempSet.totalStats, Common.SimulateWeights);
                    }
                    if (tempSet.totalStats.Value(tempSet.gearWeights) > bestSet.totalStats.Value(tempSet.gearWeights) && tempSet.totalStats.Value(bestSet.gearWeights) > bestSet.totalStats.Value(bestSet.gearWeights))
                    {
                        if (!tempSet.IsEqual(prevSet2))
                        {
                            bestSet = tempSet.Clone();
                            iterations = 0;
                        }
                        else
                        {
                            iterations++;
                        }
                    }
                    else
                    {
                        StatWeights avgWeights = tempSet.gearWeights + bestSet.gearWeights;
                        tempSet.gearWeights = avgWeights;
                        bestSet.gearWeights = avgWeights;
                        if (tempSet.totalStats.Value(avgWeights) > bestSet.totalStats.Value(avgWeights) && !tempSet.IsEqual(prevSet2))
                        {
                            bestSet = tempSet.Clone();
                            iterations = 0;
                        }
                        else
                        {
                            iterations++;
                        }
                    }
                }
                prevSet2 = prevSet.Clone();
                prevSet = tempSet.Clone();
            } while (changesMade && iterations < 5);

            bestSet.meal = startSet.meal;
            tempSet = bestSet.Clone();
            bool needSpeed = (speedBreakPoint > 341 && tempSet.totalStats.speed < speedBreakPoint);
            bool needAcc = tempSet.totalStats.acc < accReq;
            if (needAcc || needSpeed)
            {
                maxiLevel = maxiLevel / 10 * 10;

                List<Item> optItems = new List<Item>();
                foreach (Item i in mainHands)
                {
                    if (i.itemStats.itemLevel >= tempSet.mainHand.itemStats.itemLevel || ownedGearOnly) // exclude lower-ilevel weapons, we should always use the highest ones available to us.
                    {
                        if (!i.twoHand)
                        {
                            foreach (Item i2 in offHands)
                            {
                                if ((needAcc && (i.itemStats + i2.itemStats).acc > (tempSet.mainHand.itemStats + tempSet.offHand.itemStats).acc) ||
                                    (needSpeed && (i.itemStats + i2.itemStats).speed > (tempSet.mainHand.itemStats + tempSet.offHand.itemStats).speed))
                                {
                                    if (!optItems.Contains(i))
                                    {
                                        optItems.Add(i);
                                    }
                                    if (!optItems.Contains(i2))
                                    {
                                        optItems.Add(i);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((needAcc && (i.itemStats.acc > tempSet.mainHand.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.mainHand.itemStats.speed))
                                && !optItems.Contains(i))
                            {
                                optItems.Add(i);
                            }
                        }
                    }
                }
                foreach (Item i in heads)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.head.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.head.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in bodies)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.body.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.body.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in hands)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.hands.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.hands.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in waists)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.waist.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.waist.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in legs)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.legs.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.legs.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in feet)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.feet.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.feet.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in necks)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.neck.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.neck.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in ears)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.ears.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.ears.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in wrists)
                {
                    if (((needAcc && i.itemStats.acc > tempSet.wrists.itemStats.acc) || (needSpeed && i.itemStats.speed > tempSet.wrists.itemStats.speed))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel < 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                foreach (Item i in rings)
                {
                    if (((needAcc && (i.itemStats.acc > tempSet.leftRing.itemStats.acc || i.itemStats.acc > tempSet.rightRing.itemStats.acc)) ||
                        (needSpeed && (i.itemStats.speed > tempSet.leftRing.itemStats.speed || i.itemStats.speed > tempSet.rightRing.itemStats.speed)))
                        && !optItems.Contains(i) && (maxiLevel - i.itemStats.itemLevel <= 20 || ownedGearOnly))
                    {
                        optItems.Add(i);
                    }
                }
                optItems = DeduplicateItems(optItems);
                accSets = new Dictionary<int, GearSet>();
                StatWeights weights = bestSet.gearWeights;
                SetProgressBarMaximum(EstAccSetCount(optItems));
                tempSet = OptimizeAccuracy(j, tempSet.Clone(), new GearSet(), accReq, optItems, optItems.ToList<Item>(), 0, weights, speedBreakPoint);
                /*if (tempSet.totalStats.acc <= bestSet.totalStats.acc)
                {
                    tempSet = bestSet;
                }*/
            }
            return tempSet;
        }

        private int EstAccSetCount(List<Item> accItems)
        {
            int[] slotCounts = new int[typeof(GearSlot).GetEnumValues().Length];
            for (int i = 0; i < slotCounts.Length; i++)
            {
                slotCounts[i] = 0;
            }
            foreach (Item i in accItems)
            {
                slotCounts[(int)i.equipSlot]++;
            }
            int setCount = 1;
            for (int i = 0; i < slotCounts.Length; i++)
            {
                if ((int)GearSlot.Ring == i)
                {
                    setCount *= Choose(slotCounts[i] + 2, 2);
                }
                else
                {
                    setCount *= Choose(slotCounts[i] + 1, 1);
                }
            }
            return setCount;
        }

        private int Choose(int n, int k)
        {
            int result = 1;

            for (int i = Math.Max(k, n - k) + 1; i <= n; ++i)
                result *= i;

            for (int i = 2; i <= Math.Min(k, n - k); ++i)
                result /= i;

            return result;
        }

        private List<Item> DeduplicateItems(List<Item> items)
        {
            List<Item> removeItems = new List<Item>();
            foreach (Item i in items)
            {
                if (i.equipSlot != GearSlot.Ring && !removeItems.Contains(i))
                {
                    for (int index = items.IndexOf(i) + 1; index < items.Count; index++)
                    {
                        if (i.tomeTier > items[index].tomeTier && items[index].name.Contains(i.name))
                        {
                            removeItems.Add(items[index]);
                            break;
                        }
                        else if (items[index].tomeTier > i.tomeTier && items[index].name.Contains(i.name))
                        {
                            removeItems.Add(i);
                            break;
                        }
                    }
                }
            }
            foreach (Item i in removeItems)
            {
                items.Remove(i);
            }
            return items;
        }

        public GearSet OptimizeAccuracy(Job j, GearSet startingSet, GearSet bestSet, int accNeed, List<Item> optItems, List<Item> currentItems, int index, StatWeights weights, int speedBreakPoint = 341)
        {
            IncrementProgressBar();
            try
            {
                GearSet tempSet;
                startingSet.CalcGearStats();
                startingSet.CalcTotalStats();
                if (startingSet.totalStats.acc >= accNeed && startingSet.totalStats.speed >= speedBreakPoint /*|| currentItems.Count == 0*/)
                {
                    if (accSets.ContainsKey(index))
                    {
                        accSets.TryGetValue(index, out tempSet);
                        if (startingSet.totalStats.Value(weights) > tempSet.gearStats.Value(weights))
                        {
                            accSets.Remove(index);
                            accSets.Add(index, startingSet);
                            return startingSet;
                        }
                        return tempSet;
                    }
                    else
                    {
                        accSets.Add(index, startingSet);
                        return startingSet;
                    }
                }
                else
                {
                    if (accSets.TryGetValue(index, out tempSet))
                    {
                        return tempSet;
                    }

                    if (currentItems.Count > 0)
                    {
                        List<Item> activeItems = currentItems.ToList<Item>();
                        foreach (Item i in currentItems)
                        {
                            tempSet = startingSet.Clone();
                            switch (i.equipSlot)
                            {
                                case GearSlot.MainHand:
                                    tempSet.mainHand = i;
                                    if (!i.twoHand)
                                    {
                                        tempSet.offHand = new Item();
                                    }
                                    break;
                                case GearSlot.Head:
                                    tempSet.head = i;
                                    break;
                                case GearSlot.Body:
                                    tempSet.body = i;
                                    break;
                                case GearSlot.Hands:
                                    tempSet.hands = i;
                                    break;
                                case GearSlot.Waist:
                                    tempSet.waist = i;
                                    break;
                                case GearSlot.Legs:
                                    tempSet.legs = i;
                                    break;
                                case GearSlot.Feet:
                                    tempSet.feet = i;
                                    break;
                                case GearSlot.Neck:
                                    tempSet.neck = i;
                                    break;
                                case GearSlot.Ears:
                                    tempSet.ears = i;
                                    break;
                                case GearSlot.Wrists:
                                    tempSet.wrists = i;
                                    break;
                                default:
                                    break;
                            }
                            if (i.equipSlot == GearSlot.MainHand && !i.twoHand)
                            {
                                foreach (Item i2 in optItems)
                                {
                                    if (i2.equipSlot == GearSlot.OffHand)
                                    {
                                        GearSet tempSet2 = tempSet.Clone();
                                        tempSet2.offHand = i2;
                                        activeItems.Remove(i);
                                        List<Item> tempAccItems = activeItems.ToList<Item>();
                                        tempAccItems.Remove(i);
                                        tempAccItems.Remove(i2);
                                        foreach (Item remItem in optItems)
                                        {
                                            if (remItem.equipSlot == GearSlot.MainHand || remItem.equipSlot == GearSlot.OffHand)
                                            {
                                                tempAccItems.Remove(remItem);
                                            }
                                        }
                                        tempSet2 = OptimizeAccuracy(j, tempSet2.Clone(), bestSet, accNeed, optItems, tempAccItems, index + (int)Math.Pow(2, optItems.IndexOf(i)), weights, speedBreakPoint);
                                        if (tempSet2.Value(weights) > bestSet.Value(weights) && tempSet2.totalStats.acc > accNeed && tempSet2.totalStats.speed > speedBreakPoint)
                                        {
                                            bestSet = tempSet2;
                                        }
                                    }
                                }
                            }
                            else if (i.equipSlot == GearSlot.Ring)
                            {
                                activeItems.Remove(i);
                                List<Item> tempAccItems = activeItems.ToList<Item>();
                                if (i.unique)
                                {
                                    if (tempSet.rightRing != i)
                                    {
                                        GearSet tempSet2 = tempSet.Clone();
                                        tempSet2.leftRing = i;
                                        tempSet2 = OptimizeAccuracy(j, tempSet2.Clone(), bestSet, accNeed, optItems, tempAccItems, index + (int)Math.Pow(2, optItems.IndexOf(i)), weights, speedBreakPoint);
                                        if (tempSet2.Value(weights) > bestSet.Value(weights) && tempSet2.totalStats.acc > accNeed && tempSet2.totalStats.speed > speedBreakPoint)
                                        {
                                            bestSet = tempSet2;
                                        }
                                    }
                                    if (tempSet.leftRing != i)
                                    {
                                        GearSet tempSet2 = tempSet.Clone();
                                        tempSet2.rightRing = i;
                                        tempSet2 = OptimizeAccuracy(j, tempSet2.Clone(), bestSet, accNeed, optItems, tempAccItems, index + (int)Math.Pow(2, optItems.IndexOf(i)), weights, speedBreakPoint);
                                        if (tempSet2.Value(weights) > bestSet.Value(weights) && tempSet2.totalStats.acc > accNeed && tempSet2.totalStats.speed > speedBreakPoint)
                                        {
                                            bestSet = tempSet2;
                                        }
                                    }
                                }
                                else
                                {
                                    GearSet tempSet2 = tempSet.Clone();
                                    tempSet2.leftRing = i;
                                    tempSet2 = OptimizeAccuracy(j, tempSet2.Clone(), bestSet, accNeed, optItems, tempAccItems, index + (int)Math.Pow(2, optItems.IndexOf(i)), weights, speedBreakPoint);
                                    if (tempSet2.Value(weights) > bestSet.Value(weights) && tempSet2.totalStats.acc > accNeed && tempSet2.totalStats.speed > speedBreakPoint)
                                    {
                                        bestSet = tempSet2;
                                    }

                                    tempSet2 = tempSet.Clone();
                                    tempSet2.rightRing = i;
                                    tempSet2 = OptimizeAccuracy(j, tempSet2.Clone(), bestSet, accNeed, optItems, tempAccItems, index + (int)Math.Pow(2, optItems.IndexOf(i)), weights, speedBreakPoint);
                                    if (tempSet2.Value(weights) > bestSet.Value(weights) && tempSet2.totalStats.acc > accNeed && tempSet2.totalStats.speed > speedBreakPoint)
                                    {
                                        bestSet = tempSet2;
                                    }
                                }
                            }
                            else
                            {
                                activeItems.Remove(i);
                                List<Item> tempAccItems = activeItems.ToList<Item>();
                                foreach (Item remItem in optItems)
                                {
                                    if (remItem.equipSlot == i.equipSlot)
                                    {
                                        tempAccItems.Remove(remItem);
                                    }
                                }
                                tempSet = OptimizeAccuracy(j, tempSet.Clone(), bestSet, accNeed, optItems, tempAccItems, index + (int)Math.Pow(2, optItems.IndexOf(i)), weights, speedBreakPoint);
                                if (tempSet.Value(weights) > bestSet.Value(weights) && tempSet.totalStats.acc > accNeed && tempSet.totalStats.speed > speedBreakPoint)
                                {
                                    bestSet = tempSet;
                                }
                            }
                        }
                        if (accSets.ContainsKey(index))
                        {
                            accSets.TryGetValue(index, out tempSet);
                            if (bestSet.totalStats.Value(weights) > tempSet.gearStats.Value(weights))
                            {
                                accSets.Remove(index);
                                accSets.Add(index, bestSet);
                                return bestSet;
                            }
                            IncrementProgressBar();
                            return tempSet;
                        }
                        else
                        {
                            if ((tempSet.totalStats.acc > bestSet.totalStats.acc && tempSet.totalStats.acc < accNeed) || (tempSet.totalStats.speed > bestSet.totalStats.speed && tempSet.totalStats.speed < speedBreakPoint))
                            {
                                return tempSet;
                            }
                            return bestSet;
                        }
                    }
                    else if ((startingSet.totalStats.acc > bestSet.totalStats.acc) || (startingSet.totalStats.speed > bestSet.totalStats.speed && startingSet.totalStats.speed < speedBreakPoint))
                    {
                        return startingSet;
                    }
                    else
                    {
                        return bestSet;
                    }
                }
            }
            catch (Exception ex)
            {
                return startingSet;
            }
        }

        private void AccuracyRequirement_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                activeChar.accuracyNeeds[(int)activeChar.currentJob] = (int)AccuracyRequirement.Value;
                Common.accuracyRequirements[(int)activeChar.currentJob] = (int)AccuracyRequirement.Value;
            }
            catch { }
        }

        private void ClearGearSlotFilter_Click(object sender, EventArgs e)
        {
            GearSlotFilter.SelectedItem = null;
            FilterGear("", (HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
            CustomEvents.ChangeSlotFilter("");
        }

        private void GearSlotFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GearSlotFilter.SelectedItem != null)
            {
                FilterGear((string)GearSlotFilter.SelectedItem, (HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
                CustomEvents.ChangeSlotFilter((string)GearSlotFilter.SelectedItem);
            }
            else
            {
                FilterGear("", (HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
                CustomEvents.ChangeSlotFilter("");
            }
        }

        private void FilterGear(string slotFilter, int maxTurn)
        {
            foreach (DataGridViewRow row in GearDisplayGridView.Rows)
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

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveFile))
            {
                openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                openFileDialog1.InitialDirectory = Properties.Settings.Default.SaveFile.Substring(0, Properties.Settings.Default.SaveFile.LastIndexOf('\\'));
                openFileDialog1.FileName = "";
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.SaveFile = openFileDialog1.FileName;
                Common.Load(Properties.Settings.Default.SaveFile);
                activeChar = null;
                EditGearCancelButton_Click(sender, e);
                EditFoodResetButton_Click(sender, e);
                CharacterSelect.Items.Clear();
                foreach (Character c in Common.charDictionary.Values)
                {
                    CharacterSelect.Items.Add(c);
                }
                if (CharacterSelect.Items.Count > 0)
                {
                    CharacterSelect.SelectedIndex = 0;
                }
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveFile))
            {
                SaveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                Common.Save(Properties.Settings.Default.SaveFile);
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveFile))
            {
                saveFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Properties.Settings.Default.SaveFile.Substring(0, Properties.Settings.Default.SaveFile.LastIndexOf('\\'));
                saveFileDialog1.FileName = Properties.Settings.Default.SaveFile.Substring(Properties.Settings.Default.SaveFile.LastIndexOf('\\') + 1);
            }
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.SaveFile = saveFileDialog1.FileName;
                SaveToolStripMenuItem_Click(sender, e);
            }
        }

        private void EditGearExportButton_Click(object sender, EventArgs e)
        {

        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Properties.Settings.Default.SaveFile.Substring(0, Properties.Settings.Default.SaveFile.LastIndexOf('\\'));
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ExportImport import = new ExportImport();
                import.Import(openFileDialog1.FileName);
            }
        }

        private void importFoodToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ProgressionTomeTier_ValueChanged(object sender, EventArgs e)
        {
            activeChar.tomeTier[(int)activeChar.currentJob] = (double)ProgressionTomeTier.Value;
        }


        private void ProgressionRelicTier_ValueChanged(object sender, EventArgs e)
        {
            activeChar.relicTier[(int)activeChar.currentJob] = (int)ProgressionRelicTier.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (GearSlotFilter.SelectedItem != null)
            {
                FilterGear((string)GearSlotFilter.SelectedItem, (HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
            }
            else
            {
                FilterGear("", (HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
            }
            CustomEvents.ChangeHighestTurnFilter((HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
        }

        private void PopOutGearButton_Click(object sender, EventArgs e)
        {
            previousVisibleState = Common.GearTableVisible;
            if (Common.GearTableVisible)
            {
                HideShowButton_Click(sender, e);
            }
            HideShowButton.Enabled = false;
            PopOutGearButton.Enabled = false;
            CustomEvents.ClosePopOutFormEvent += MainForm_ClosePopOutForm;
            popForm.Show();
            //CustomEvents.ChangeCharacter(activeChar);
            //CustomEvents.ChangeSlotFilter((GearSlotFilter.SelectedItem != null ? (string)GearSlotFilter.SelectedItem : ""));
            //CustomEvents.ChangeHighestTurnFilter((HideHigherTurnGearCheckBox.Checked ? activeChar.clearedTurn : Common.HighestTurn));
            Common.GearTablePoppedOut = true;
        }

        void MainForm_ClosePopOutForm()
        {
            CustomEvents.ClosePopOutFormEvent -= MainForm_ClosePopOutForm;
            Common.GearTableVisible = previousVisibleState;
            HideShowGearBox();
            Common.GearTablePoppedOut = false;
            PopOutGearButton.Enabled = true;
            HideShowButton.Enabled = true;
        }

        private void HideShowButton_Click(object sender, EventArgs e)
        {
            Common.GearTableVisible = !Common.GearTableVisible;
            HideShowGearBox();
        }

        private void HideShowGearBox()
        {
            if (!Common.GearTableVisible)
            {
                HideShowButton.Text = "Show";
                groupBox5.Visible = false;
                MainTabControl.Width = MainTabControl.MinimumSize.Width;
                this.Width = this.MinimumSize.Width;
            }
            else
            {
                HideShowButton.Text = "Hide";
                groupBox5.Visible = true;
                MainTabControl.Width = MainTabControl.MaximumSize.Width;
                this.Width = this.MaximumSize.Width;
            }
        }
        #endregion

        #region EditGearTab
        private void EditGearJobFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterEditGearView((string)EditGearJobFilterComboBox.SelectedItem, (string)EditGearSlotFilterComboBox.SelectedItem);
        }

        private void EditGearClearJobFilterButton_Click(object sender, EventArgs e)
        {
            EditGearJobFilterComboBox.SelectedIndex = -1;
        }

        private void EditGearSlotFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterEditGearView((string)EditGearJobFilterComboBox.SelectedItem, (string)EditGearSlotFilterComboBox.SelectedItem);
        }

        private void EditGearClearSlotFilterButton_Click(object sender, EventArgs e)
        {
            EditGearSlotFilterComboBox.SelectedIndex = -1;
        }

        private void EditGearAcceptButton_Click(object sender, EventArgs e)
        {
            Common.gearDictionary.Clear();
            //GearEditGridView.Rows[0].
            foreach (DataGridViewRow row in GearEditGridView.Rows)
            {
                try
                {
                    Item i = new Item();
                    i.name = (string)row.Cells["EditItem"].Value;
                    if (!string.IsNullOrWhiteSpace(i.name))
                    {
                        if (Common.gearDictionary.ContainsKey(i.name))
                        {
                            Item i2 = Common.gearDictionary[i.name];
                            try
                            {
                                Job j = (Job)Enum.Parse(typeof(Job), (string)row.Cells["Job"].Value);
                                if (!i2.canEquip.Contains(j))
                                {
                                    i2.canEquip.Add(j);
                                    Common.gearDictionary.Remove(i2.name);
                                    Common.gearDictionary.Add(i2.name, i2);
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                i.itemStats.itemLevel = GetCellAsInt(row.Cells["ItemLevel"]);
                                i.unique = GetCellAsBool(row.Cells["IsUniqueItem"]);
                                i.canEquip = new List<Job>();
                                i.canEquip.Add((Job)Enum.Parse(typeof(Job), (string)row.Cells["Job"].Value));
                                i.equipSlot = (GearSlot)Enum.Parse(typeof(GearSlot), (string)row.Cells["EditSlot"].Value);
                                i.twoHand = GetCellAsBool(row.Cells["IsTwoHand"]);
                                i.itemStats.weaponDamage = GetCellAsInt(row.Cells["EditWDMG"]);
                                i.itemStats.autoAttackDelay = GetCellAsDouble(row.Cells["Delay"]);
                                i.itemStats.blockRate = GetCellAsInt(row.Cells["EditBlockRate"]);
                                i.itemStats.blockStrength = GetCellAsInt(row.Cells["EditBlockStrength"]);
                                i.itemStats.mainStat = GetCellAsInt(row.Cells["EditStat"]);
                                i.itemStats.vit = GetCellAsInt(row.Cells["EditVIT"]);
                                i.itemStats.pie = GetCellAsInt(row.Cells["EditPIE"]);
                                i.itemStats.det = GetCellAsInt(row.Cells["EditDET"]);
                                i.itemStats.acc = GetCellAsInt(row.Cells["Accuracy"]);
                                i.itemStats.crit = GetCellAsInt(row.Cells["EditCrit"]);
                                i.itemStats.speed = GetCellAsInt(row.Cells["EditSpeed"]);
                                i.itemStats.parry = GetCellAsInt(row.Cells["EditParry"]);
                                i.sourceTurn = GetCellAsInt(row.Cells["CoilTurn"]);
                                i.tomeTier = GetCellAsDouble(row.Cells["TomeTier"]);
                                i.tomeCost = GetCellAsInt(row.Cells["TomeCost"]);
                                i.relicTier = GetCellAsDouble(row.Cells["RelicTier"]);
                                Common.gearDictionary.Add(i.name, i);
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            if (activeChar != null)
            {
                PopulateForm();
            }
            EditGearCancelButton_Click(sender, e);
        }

        private void EditGearCancelButton_Click(object sender, EventArgs e)
        {
            GearEditGridView.Rows.Clear();
            foreach (Item i in Common.gearDictionary.Values)
            {
                foreach (Job j in i.canEquip)
                {
                    //name, ilvl, unique, job, slot, twohand?, wdmg, stat, vit, acc, det, crit, speed
                    GearEditGridView.Rows.Add(i.name, i.itemStats.itemLevel, i.unique.ToString(), j.ToString(), i.equipSlot.ToString(), i.twoHand.ToString(), i.itemStats.weaponDamage, i.itemStats.autoAttackDelay, i.itemStats.blockRate, i.itemStats.blockStrength, i.itemStats.mainStat, i.itemStats.vit, i.itemStats.acc, i.itemStats.det, i.itemStats.crit, i.itemStats.speed, i.itemStats.pie, i.itemStats.parry, i.sourceTurn, i.tomeTier, i.tomeCost, i.relicTier);
                }
            }
            FilterEditGearView((string)EditGearJobFilterComboBox.SelectedItem, (string)EditGearSlotFilterComboBox.SelectedItem);
        }

        private void FilterEditGearView(string jobFilter, string slotFilter)
        {
            try
            {
                foreach (DataGridViewRow row in GearEditGridView.Rows)
                {
                    if ((((string)row.Cells["Job"].Value).Equals(jobFilter) || string.IsNullOrWhiteSpace(jobFilter)) && (((string)row.Cells["EditSlot"].Value).Equals(slotFilter) || string.IsNullOrWhiteSpace(slotFilter)))
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
        #endregion



        #region EditFoodTab
        private void EditFoodAcceptButton_Click(object sender, EventArgs e)
        {
            Common.foodDictionary.Clear();
            foreach (DataGridViewRow row in FoodEditGridView.Rows)
            {
                string name = (string)row.Cells["Food"].Value;
                if (!string.IsNullOrWhiteSpace(name) && !Common.foodDictionary.Keys.Contains(name))
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
                    Common.foodDictionary.Add(name, new Food(name, vitmod, vitmax, accmod, accmax, detmod, detmax, critmod, critmax, spdmod, spdmax, piemod, piemax, parrymod, parrymax));
                }
            }
            PopFood();
            if (activeChar != null)
            {
                UpdGearValDisplay((int)activeChar.currentJob);
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void EditFoodResetButton_Click(object sender, EventArgs e)
        {
            FoodEditGridView.Rows.Clear();
            foreach (Food f in Common.foodDictionary.Values)
            {
                FoodEditGridView.Rows.Add(f.name, (int)(f.vitPct * 100), f.vitCap, (int)(f.accPct * 100), f.accCap, (int)(f.detPct * 100), f.detCap, (int)(f.critPct * 100), f.critCap, (int)(f.speedPct * 100), f.speedCap, (int)(f.piePct * 100), f.pieCap, (int)(f.parryPct * 100), f.parryCap);
            }
        }
        #endregion

        #region EditConfigTab
        private void EditDamageFormTextBox_Validated(object sender, EventArgs e)
        {
            try
            {
                string dmgForm = EditDamageFormTextBox.Text.Replace("WD", "40").Replace("DTR", "202").Replace("STAT", "400");
                Expression expr = new Expression(dmgForm);
                double testVal = (double)expr.Evaluate();
                EditDamageFormTextBox.BackColor = System.Drawing.SystemColors.Window;
                dmgFormValid = true;
                EditConfigAcceptButton.Enabled = critFormValid && dmgFormValid && healFormValid && parryFormValid && speedFormValid;
            }
            catch
            {
                dmgFormValid = false;
                EditConfigAcceptButton.Enabled = false;
                EditDamageFormTextBox.BackColor = Color.Red;
            }
        }

        private void EditCritFormTextBox_Validated(object sender, System.EventArgs e)
        {
            try
            {
                string critForm = EditCritFormTextBox.Text.Replace("CRITPCTMOD", "0").Replace("CRIT", "341");
                Expression expr = new Expression(critForm);
                double testVal = (double)expr.Evaluate();
                EditCritFormTextBox.BackColor = System.Drawing.SystemColors.Window;
                critFormValid = true;
                EditConfigAcceptButton.Enabled = critFormValid && dmgFormValid && healFormValid && parryFormValid && speedFormValid;
            }
            catch
            {
                critFormValid = false;
                EditConfigAcceptButton.Enabled = false;
                EditCritFormTextBox.BackColor = Color.Red;
            }
        }

        private void EditHealingFormTextBox_Validated(object sender, EventArgs e)
        {
            try
            {
                string healForm = EditHealingFormTextBox.Text.Replace("WD", "40").Replace("DTR", "202").Replace("MND", "400");
                Expression expr = new Expression(healForm);
                double testVal = (double)expr.Evaluate();
                EditHealingFormTextBox.BackColor = System.Drawing.SystemColors.Window;
                healFormValid = true;
                EditConfigAcceptButton.Enabled = critFormValid && dmgFormValid && healFormValid && parryFormValid && speedFormValid;
            }
            catch
            {
                healFormValid = false;
                EditConfigAcceptButton.Enabled = false;
                EditHealingFormTextBox.BackColor = Color.Red;
            }
        }

        private void EditParryFormTextBox_Validated(object sender, EventArgs e)
        {
            try
            {
                string parryForm = EditParryFormTextBox.Text.Replace("PARRY", "341");
                Expression expr = new Expression(parryForm);
                double testVal = (double)expr.Evaluate();
                EditParryFormTextBox.BackColor = System.Drawing.SystemColors.Window;
                parryFormValid = true;
                EditConfigAcceptButton.Enabled = critFormValid && dmgFormValid && healFormValid && parryFormValid && speedFormValid;
            }
            catch
            {
                parryFormValid = false;
                EditConfigAcceptButton.Enabled = false;
                EditParryFormTextBox.BackColor = Color.Red;
            }
        }

        private void EditConfigResetButton_Click(object sender, EventArgs e)
        {
            EditVITPerSTR.Value = (decimal)Common.VitPerSTR;
            EditHighestTurn.Value = (decimal)Common.HighestTurn;

            EditDamageFormTextBox.Text = Common.DamageFormula;
            EditDamageFormTextBox_Validated(sender, e);
            EditHealingFormTextBox.Text = Common.HealingFormula;
            EditHealingFormTextBox_Validated(sender, e);
            EditCritFormTextBox.Text = Common.CritFormula;
            EditCritFormTextBox_Validated(sender, e);
            EditParryFormTextBox.Text = Common.ParryFormula;
            EditParryFormTextBox_Validated(sender, e);
            EditSpeedFormTextBox.Text = Common.SpdReductionFormula;
            EditSpeedFormTextBox_Validated(sender, e);
        }

        private void EditConfigAcceptButton_Click(object sender, EventArgs e)
        {
            Common.DamageFormula = EditDamageFormTextBox.Text;
            Common.HealingFormula = EditHealingFormTextBox.Text;
            Common.CritFormula = EditCritFormTextBox.Text;
            Common.ParryFormula = EditParryFormTextBox.Text;
            Common.SpdReductionFormula = EditSpeedFormTextBox.Text;

            Common.VitPerSTR = (double)EditVITPerSTR.Value;
            Common.HighestTurn = (int)EditHighestTurn.Value;

            if (activeChar != null)
            {
                activeChar.idealDamage[(int)activeChar.currentJob].gearWeights = Calculation.CalcStatWeights(activeChar.currentJob, activeChar.idealDamage[(int)activeChar.currentJob].totalStats, Common.SimulateWeights);
                activeChar.currentWeights = activeChar.idealDamage[(int)activeChar.currentJob].gearWeights;
                weightLabel.Text = "Weights:\n" + activeChar.currentWeights.ToString(activeChar.currentJob);
                UpdGearValDisplay((int)activeChar.currentJob);
                PopGearValues();
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void EditSpeedFormTextBox_Validated(object sender, EventArgs e)
        {
            try
            {
                string speedForm = EditSpeedFormTextBox.Text.Replace("SPEED", "400");
                Expression expr = new Expression(speedForm);
                double testVal = (double)expr.Evaluate();
                EditSpeedFormTextBox.BackColor = System.Drawing.SystemColors.Window;
                speedFormValid = true;
                EditConfigAcceptButton.Enabled = critFormValid && dmgFormValid && healFormValid && parryFormValid && speedFormValid;
            }
            catch
            {
                speedFormValid = false;
                EditConfigAcceptButton.Enabled = false;
                EditSpeedFormTextBox.BackColor = Color.Red;
            }
        }
        #endregion

        private void EditConfigRestoreButton_Click(object sender, EventArgs e)
        {
            Common.VitPerSTR = Common.DefaultVitPerSTR;
            Common.HighestTurn = Common.DefaultHighestTurn;

            Common.DamageFormula = Common.DefaultDamageFormula;
            Common.AutoAttackDamageFormula = Common.DefaultAutoAttackDamageFormula;
            Common.HealingFormula = Common.DefaultHealingFormula;
            Common.CritFormula = Common.DefaultCritFormula;
            Common.SpdReductionFormula = Common.DefaultSpdReductionFormula;
            Common.ParryFormula = Common.DefaultParryFormula;

            SimWeightsCheckBox.Checked = Common.DefaultSimulateWeights;

            EditConfigResetButton_Click(sender, e);

            if (activeChar != null)
            {
                activeChar.idealDamage[(int)activeChar.currentJob].gearWeights = Calculation.CalcStatWeights(activeChar.currentJob, activeChar.idealDamage[(int)activeChar.currentJob].totalStats, Common.SimulateWeights);
                activeChar.currentWeights = activeChar.idealDamage[(int)activeChar.currentJob].gearWeights;
                UpdGearValDisplay((int)activeChar.currentJob);
                PopGearValues();
                CustomEvents.UpdateCharacter(activeChar);
            }
        }

        private void SimWeightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Common.SimulateWeights = SimWeightsCheckBox.Checked;
        }

        private void OwnedSpecificAccCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            OwnedAAccReq.Enabled = OwnedSpecificAccCheckbox.Checked;
            OwnedBAccReq.Enabled = OwnedSpecificAccCheckbox.Checked;
        }

        private void OwnedAAccReq_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                activeChar.ownedAccReqListA[(int)activeChar.currentJob] = (int)OwnedAAccReq.Value;
            }
            catch { }
        }

        private void OwnedBAccReq_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                activeChar.ownedAccReqListB[(int)activeChar.currentJob] = (int)OwnedBAccReq.Value;
            }
            catch { }
        }

        private void SpdBreakCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Common.UseSpeedBreakPoint = SpdBreakCheckBox.Checked;
            SpdBreakPoint.Enabled = Common.UseSpeedBreakPoint;
        }

        private void SpdBreakPoint_ValueChanged(object sender, EventArgs e)
        {
            Common.speedBreakPoints[(int)activeChar.currentJob] = (int)SpdBreakPoint.Value;
        }
    }
}

