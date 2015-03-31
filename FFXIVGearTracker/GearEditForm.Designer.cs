namespace FFXIVGearTracker
{
    partial class GearEditForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.button4 = new System.Windows.Forms.Button();
			this.Item = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ItemLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsUniqueItem = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Job = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Slot = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.IsTwoHand = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.WDMG = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Stat = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VIT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PIE = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Accuracy = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DET = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Crit = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Speed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Parry = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Turn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TomeTier = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TomeCost = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(67, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Filter By Job:";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(85, 12);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(121, 21);
			this.comboBox1.TabIndex = 1;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(212, 10);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "ClearFilter";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// dataGridView1
			// 
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Item,
            this.ItemLevel,
            this.IsUniqueItem,
            this.Job,
            this.Slot,
            this.IsTwoHand,
            this.WDMG,
            this.Stat,
            this.VIT,
            this.PIE,
            this.Accuracy,
            this.DET,
            this.Crit,
            this.Speed,
            this.Parry,
            this.Turn,
            this.TomeTier,
            this.TomeCost});
			this.dataGridView1.Location = new System.Drawing.Point(15, 39);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(1213, 522);
			this.dataGridView1.TabIndex = 3;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(878, 10);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 4;
			this.button2.Text = "Accept";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(959, 10);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 5;
			this.button3.Text = "Cancel";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(310, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(68, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Filter By Slot:";
			// 
			// comboBox2
			// 
			this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Location = new System.Drawing.Point(384, 12);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(121, 21);
			this.comboBox2.TabIndex = 7;
			this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(511, 10);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 8;
			this.button4.Text = "ClearFilter";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// Item
			// 
			this.Item.HeaderText = "Name";
			this.Item.Name = "Item";
			this.Item.Width = 150;
			// 
			// ItemLevel
			// 
			dataGridViewCellStyle1.Format = "N0";
			dataGridViewCellStyle1.NullValue = null;
			this.ItemLevel.DefaultCellStyle = dataGridViewCellStyle1;
			this.ItemLevel.HeaderText = "iLvl";
			this.ItemLevel.Name = "ItemLevel";
			this.ItemLevel.Width = 50;
			// 
			// IsUniqueItem
			// 
			this.IsUniqueItem.HeaderText = "Unique?";
			this.IsUniqueItem.Items.AddRange(new object[] {
            "True",
            "False"});
			this.IsUniqueItem.Name = "IsUniqueItem";
			this.IsUniqueItem.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.IsUniqueItem.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.IsUniqueItem.Width = 75;
			// 
			// Job
			// 
			this.Job.HeaderText = "Job";
			this.Job.Name = "Job";
			// 
			// Slot
			// 
			this.Slot.HeaderText = "Slot";
			this.Slot.Name = "Slot";
			this.Slot.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Slot.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// IsTwoHand
			// 
			this.IsTwoHand.HeaderText = "Two Hand?";
			this.IsTwoHand.Items.AddRange(new object[] {
            "True",
            "False"});
			this.IsTwoHand.Name = "IsTwoHand";
			this.IsTwoHand.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.IsTwoHand.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.IsTwoHand.Width = 75;
			// 
			// WDMG
			// 
			dataGridViewCellStyle2.Format = "N0";
			dataGridViewCellStyle2.NullValue = null;
			this.WDMG.DefaultCellStyle = dataGridViewCellStyle2;
			this.WDMG.HeaderText = "WDMG";
			this.WDMG.Name = "WDMG";
			this.WDMG.Width = 50;
			// 
			// Stat
			// 
			dataGridViewCellStyle3.Format = "N0";
			dataGridViewCellStyle3.NullValue = null;
			this.Stat.DefaultCellStyle = dataGridViewCellStyle3;
			this.Stat.HeaderText = "MainStat";
			this.Stat.Name = "Stat";
			this.Stat.Width = 50;
			// 
			// VIT
			// 
			dataGridViewCellStyle4.Format = "N0";
			dataGridViewCellStyle4.NullValue = null;
			this.VIT.DefaultCellStyle = dataGridViewCellStyle4;
			this.VIT.HeaderText = "VIT";
			this.VIT.Name = "VIT";
			this.VIT.Width = 50;
			// 
			// PIE
			// 
			dataGridViewCellStyle5.Format = "N0";
			this.PIE.DefaultCellStyle = dataGridViewCellStyle5;
			this.PIE.HeaderText = "PIE";
			this.PIE.Name = "PIE";
			this.PIE.Width = 50;
			// 
			// Accuracy
			// 
			dataGridViewCellStyle6.Format = "N0";
			dataGridViewCellStyle6.NullValue = null;
			this.Accuracy.DefaultCellStyle = dataGridViewCellStyle6;
			this.Accuracy.HeaderText = "Acc";
			this.Accuracy.Name = "Accuracy";
			this.Accuracy.Width = 50;
			// 
			// DET
			// 
			dataGridViewCellStyle7.Format = "N0";
			dataGridViewCellStyle7.NullValue = null;
			this.DET.DefaultCellStyle = dataGridViewCellStyle7;
			this.DET.HeaderText = "DET";
			this.DET.Name = "DET";
			this.DET.Width = 50;
			// 
			// Crit
			// 
			dataGridViewCellStyle8.Format = "N0";
			dataGridViewCellStyle8.NullValue = null;
			this.Crit.DefaultCellStyle = dataGridViewCellStyle8;
			this.Crit.HeaderText = "Crit";
			this.Crit.Name = "Crit";
			this.Crit.Width = 50;
			// 
			// Speed
			// 
			dataGridViewCellStyle9.Format = "N0";
			this.Speed.DefaultCellStyle = dataGridViewCellStyle9;
			this.Speed.HeaderText = "Speed";
			this.Speed.Name = "Speed";
			this.Speed.Width = 50;
			// 
			// Parry
			// 
			dataGridViewCellStyle10.Format = "N0";
			this.Parry.DefaultCellStyle = dataGridViewCellStyle10;
			this.Parry.HeaderText = "Parry";
			this.Parry.Name = "Parry";
			this.Parry.Width = 50;
			// 
			// Turn
			// 
			dataGridViewCellStyle11.Format = "N0";
			dataGridViewCellStyle11.NullValue = null;
			this.Turn.DefaultCellStyle = dataGridViewCellStyle11;
			this.Turn.HeaderText = "Coil Turn";
			this.Turn.Name = "Turn";
			this.Turn.Width = 50;
			// 
			// TomeTier
			// 
			dataGridViewCellStyle12.Format = "N1";
			this.TomeTier.DefaultCellStyle = dataGridViewCellStyle12;
			this.TomeTier.HeaderText = "Tome Tier";
			this.TomeTier.Name = "TomeTier";
			this.TomeTier.Width = 50;
			// 
			// TomeCost
			// 
			dataGridViewCellStyle13.Format = "N0";
			dataGridViewCellStyle13.NullValue = null;
			this.TomeCost.DefaultCellStyle = dataGridViewCellStyle13;
			this.TomeCost.HeaderText = "Tome Cost";
			this.TomeCost.Name = "TomeCost";
			this.TomeCost.Width = 50;
			// 
			// GearEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1243, 576);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.label1);
			this.Name = "GearEditForm";
			this.Text = "GearEditForm";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.DataGridViewTextBoxColumn Item;
		private System.Windows.Forms.DataGridViewTextBoxColumn ItemLevel;
		private System.Windows.Forms.DataGridViewComboBoxColumn IsUniqueItem;
		private System.Windows.Forms.DataGridViewComboBoxColumn Job;
		private System.Windows.Forms.DataGridViewComboBoxColumn Slot;
		private System.Windows.Forms.DataGridViewComboBoxColumn IsTwoHand;
		private System.Windows.Forms.DataGridViewTextBoxColumn WDMG;
		private System.Windows.Forms.DataGridViewTextBoxColumn Stat;
		private System.Windows.Forms.DataGridViewTextBoxColumn VIT;
		private System.Windows.Forms.DataGridViewTextBoxColumn PIE;
		private System.Windows.Forms.DataGridViewTextBoxColumn Accuracy;
		private System.Windows.Forms.DataGridViewTextBoxColumn DET;
		private System.Windows.Forms.DataGridViewTextBoxColumn Crit;
		private System.Windows.Forms.DataGridViewTextBoxColumn Speed;
		private System.Windows.Forms.DataGridViewTextBoxColumn Parry;
		private System.Windows.Forms.DataGridViewTextBoxColumn Turn;
		private System.Windows.Forms.DataGridViewTextBoxColumn TomeTier;
		private System.Windows.Forms.DataGridViewTextBoxColumn TomeCost;
    }
}