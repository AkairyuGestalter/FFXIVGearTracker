namespace FFXIV.GearTracking.WinForms
{
	partial class PopOutGearTableForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
			this.PopOutGearDisplayGridView = new System.Windows.Forms.DataGridView();
			this.ClosePopFormButton = new System.Windows.Forms.Button();
			this.Owned = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.Item = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Turn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Slot = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.WDMG = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Stat = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VIT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PIE = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Acc = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DET = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Crit = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Speed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Parry = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CurrentVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ValPerCost = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.PopOutGearDisplayGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// PopOutGearDisplayGridView
			// 
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.PopOutGearDisplayGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.PopOutGearDisplayGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.PopOutGearDisplayGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Owned,
            this.Item,
            this.Turn,
            this.Slot,
            this.WDMG,
            this.Stat,
            this.VIT,
            this.PIE,
            this.Acc,
            this.DET,
            this.Crit,
            this.Speed,
            this.Parry,
            this.CurrentVal,
            this.ValPerCost});
			dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.PopOutGearDisplayGridView.DefaultCellStyle = dataGridViewCellStyle14;
			this.PopOutGearDisplayGridView.Location = new System.Drawing.Point(12, 41);
			this.PopOutGearDisplayGridView.Name = "PopOutGearDisplayGridView";
			this.PopOutGearDisplayGridView.RowHeadersVisible = false;
			this.PopOutGearDisplayGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.PopOutGearDisplayGridView.Size = new System.Drawing.Size(686, 502);
			this.PopOutGearDisplayGridView.TabIndex = 1;
			// 
			// ClosePopFormButton
			// 
			this.ClosePopFormButton.Location = new System.Drawing.Point(624, 12);
			this.ClosePopFormButton.Name = "ClosePopFormButton";
			this.ClosePopFormButton.Size = new System.Drawing.Size(74, 23);
			this.ClosePopFormButton.TabIndex = 7;
			this.ClosePopFormButton.Text = "Close";
			this.ClosePopFormButton.Click += new System.EventHandler(this.ClosePopFormButton_Click);
			// 
			// Owned
			// 
			this.Owned.HeaderText = "Own?";
			this.Owned.Name = "Owned";
			this.Owned.Width = 40;
			// 
			// Item
			// 
			this.Item.HeaderText = "Name";
			this.Item.Name = "Item";
			this.Item.ReadOnly = true;
			this.Item.Width = 150;
			// 
			// Turn
			// 
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Turn.DefaultCellStyle = dataGridViewCellStyle2;
			this.Turn.HeaderText = "Turn";
			this.Turn.Name = "Turn";
			this.Turn.ReadOnly = true;
			this.Turn.Width = 30;
			// 
			// Slot
			// 
			this.Slot.HeaderText = "Slot";
			this.Slot.Name = "Slot";
			this.Slot.ReadOnly = true;
			this.Slot.Width = 45;
			// 
			// WDMG
			// 
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.WDMG.DefaultCellStyle = dataGridViewCellStyle3;
			this.WDMG.HeaderText = "WD";
			this.WDMG.Name = "WDMG";
			this.WDMG.ReadOnly = true;
			this.WDMG.Width = 30;
			// 
			// Stat
			// 
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Stat.DefaultCellStyle = dataGridViewCellStyle4;
			this.Stat.HeaderText = "Main Stat";
			this.Stat.Name = "Stat";
			this.Stat.ReadOnly = true;
			this.Stat.Width = 35;
			// 
			// VIT
			// 
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.VIT.DefaultCellStyle = dataGridViewCellStyle5;
			this.VIT.HeaderText = "VIT";
			this.VIT.Name = "VIT";
			this.VIT.ReadOnly = true;
			this.VIT.Width = 30;
			// 
			// PIE
			// 
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.PIE.DefaultCellStyle = dataGridViewCellStyle6;
			this.PIE.HeaderText = "PIE";
			this.PIE.Name = "PIE";
			this.PIE.ReadOnly = true;
			this.PIE.Width = 30;
			// 
			// Acc
			// 
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Acc.DefaultCellStyle = dataGridViewCellStyle7;
			this.Acc.HeaderText = "Acc";
			this.Acc.Name = "Acc";
			this.Acc.ReadOnly = true;
			this.Acc.Width = 30;
			// 
			// DET
			// 
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.DET.DefaultCellStyle = dataGridViewCellStyle8;
			this.DET.HeaderText = "DET";
			this.DET.Name = "DET";
			this.DET.ReadOnly = true;
			this.DET.Width = 30;
			// 
			// Crit
			// 
			dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Crit.DefaultCellStyle = dataGridViewCellStyle9;
			this.Crit.HeaderText = "Crit";
			this.Crit.Name = "Crit";
			this.Crit.ReadOnly = true;
			this.Crit.Width = 30;
			// 
			// Speed
			// 
			dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Speed.DefaultCellStyle = dataGridViewCellStyle10;
			this.Speed.HeaderText = "Spd";
			this.Speed.Name = "Speed";
			this.Speed.ReadOnly = true;
			this.Speed.Width = 30;
			// 
			// Parry
			// 
			dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Parry.DefaultCellStyle = dataGridViewCellStyle11;
			this.Parry.HeaderText = "Parry";
			this.Parry.Name = "Parry";
			this.Parry.ReadOnly = true;
			this.Parry.Width = 30;
			// 
			// CurrentVal
			// 
			dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			dataGridViewCellStyle12.Format = "N3";
			this.CurrentVal.DefaultCellStyle = dataGridViewCellStyle12;
			this.CurrentVal.HeaderText = "Value";
			this.CurrentVal.Name = "CurrentVal";
			this.CurrentVal.ReadOnly = true;
			this.CurrentVal.Width = 60;
			// 
			// ValPerCost
			// 
			dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			dataGridViewCellStyle13.Format = "N6";
			this.ValPerCost.DefaultCellStyle = dataGridViewCellStyle13;
			this.ValPerCost.HeaderText = "dVal/ Cost";
			this.ValPerCost.Name = "ValPerCost";
			this.ValPerCost.ReadOnly = true;
			this.ValPerCost.Width = 60;
			// 
			// PopOutGearTableForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(710, 555);
			this.Controls.Add(this.ClosePopFormButton);
			this.Controls.Add(this.PopOutGearDisplayGridView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(726, 593);
			this.MinimumSize = new System.Drawing.Size(726, 593);
			this.Name = "PopOutGearTableForm";
			this.Text = "Gear List";
			((System.ComponentModel.ISupportInitialize)(this.PopOutGearDisplayGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView PopOutGearDisplayGridView;
		private System.Windows.Forms.Button ClosePopFormButton;
		private System.Windows.Forms.DataGridViewCheckBoxColumn Owned;
		private System.Windows.Forms.DataGridViewTextBoxColumn Item;
		private System.Windows.Forms.DataGridViewTextBoxColumn Turn;
		private System.Windows.Forms.DataGridViewTextBoxColumn Slot;
		private System.Windows.Forms.DataGridViewTextBoxColumn WDMG;
		private System.Windows.Forms.DataGridViewTextBoxColumn Stat;
		private System.Windows.Forms.DataGridViewTextBoxColumn VIT;
		private System.Windows.Forms.DataGridViewTextBoxColumn PIE;
		private System.Windows.Forms.DataGridViewTextBoxColumn Acc;
		private System.Windows.Forms.DataGridViewTextBoxColumn DET;
		private System.Windows.Forms.DataGridViewTextBoxColumn Crit;
		private System.Windows.Forms.DataGridViewTextBoxColumn Speed;
		private System.Windows.Forms.DataGridViewTextBoxColumn Parry;
		private System.Windows.Forms.DataGridViewTextBoxColumn CurrentVal;
		private System.Windows.Forms.DataGridViewTextBoxColumn ValPerCost;

	}
}