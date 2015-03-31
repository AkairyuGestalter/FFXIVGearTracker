namespace FFXIVGearTracker
{
    partial class FoodEditForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Food = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VitPct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VitCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AccPct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AccCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DetPct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DetCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CritPct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CritCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SpdPct = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SpdCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Food,
            this.VitPct,
            this.VitCap,
            this.AccPct,
            this.AccCap,
            this.DetPct,
            this.DetCap,
            this.CritPct,
            this.CritCap,
            this.SpdPct,
            this.SpdCap});
            this.dataGridView1.Location = new System.Drawing.Point(12, 41);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridView1.Size = new System.Drawing.Size(937, 492);
            this.dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Accept";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(874, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Food
            // 
            this.Food.HeaderText = "Food";
            this.Food.Name = "Food";
            this.Food.Width = 150;
            // 
            // VitPct
            // 
            dataGridViewCellStyle21.Format = "N0";
            dataGridViewCellStyle21.NullValue = null;
            this.VitPct.DefaultCellStyle = dataGridViewCellStyle21;
            this.VitPct.HeaderText = "Vit %";
            this.VitPct.Name = "VitPct";
            this.VitPct.Width = 72;
            // 
            // VitCap
            // 
            dataGridViewCellStyle22.Format = "N0";
            dataGridViewCellStyle22.NullValue = null;
            this.VitCap.DefaultCellStyle = dataGridViewCellStyle22;
            this.VitCap.HeaderText = "Vit Max";
            this.VitCap.Name = "VitCap";
            this.VitCap.Width = 72;
            // 
            // AccPct
            // 
            dataGridViewCellStyle23.Format = "N0";
            this.AccPct.DefaultCellStyle = dataGridViewCellStyle23;
            this.AccPct.HeaderText = "Acc %";
            this.AccPct.Name = "AccPct";
            this.AccPct.Width = 72;
            // 
            // AccCap
            // 
            dataGridViewCellStyle24.Format = "N0";
            this.AccCap.DefaultCellStyle = dataGridViewCellStyle24;
            this.AccCap.HeaderText = "Acc Max";
            this.AccCap.Name = "AccCap";
            this.AccCap.Width = 72;
            // 
            // DetPct
            // 
            dataGridViewCellStyle25.Format = "N0";
            this.DetPct.DefaultCellStyle = dataGridViewCellStyle25;
            this.DetPct.HeaderText = "Det %";
            this.DetPct.Name = "DetPct";
            this.DetPct.Width = 72;
            // 
            // DetCap
            // 
            dataGridViewCellStyle26.Format = "N0";
            this.DetCap.DefaultCellStyle = dataGridViewCellStyle26;
            this.DetCap.HeaderText = "Det Max";
            this.DetCap.Name = "DetCap";
            this.DetCap.Width = 72;
            // 
            // CritPct
            // 
            dataGridViewCellStyle27.Format = "N0";
            this.CritPct.DefaultCellStyle = dataGridViewCellStyle27;
            this.CritPct.HeaderText = "Crit %";
            this.CritPct.Name = "CritPct";
            this.CritPct.Width = 72;
            // 
            // CritCap
            // 
            dataGridViewCellStyle28.Format = "N0";
            this.CritCap.DefaultCellStyle = dataGridViewCellStyle28;
            this.CritCap.HeaderText = "Crit Max";
            this.CritCap.Name = "CritCap";
            this.CritCap.Width = 72;
            // 
            // SpdPct
            // 
            dataGridViewCellStyle29.Format = "N0";
            this.SpdPct.DefaultCellStyle = dataGridViewCellStyle29;
            this.SpdPct.HeaderText = "Spd %";
            this.SpdPct.Name = "SpdPct";
            this.SpdPct.Width = 72;
            // 
            // SpdCap
            // 
            dataGridViewCellStyle30.Format = "N0";
            this.SpdCap.DefaultCellStyle = dataGridViewCellStyle30;
            this.SpdCap.HeaderText = "Spd Max";
            this.SpdCap.Name = "SpdCap";
            this.SpdCap.Width = 72;
            // 
            // FoodEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 547);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "FoodEditForm";
            this.Text = "FoodEditForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Food;
        private System.Windows.Forms.DataGridViewTextBoxColumn VitPct;
        private System.Windows.Forms.DataGridViewTextBoxColumn VitCap;
        private System.Windows.Forms.DataGridViewTextBoxColumn AccPct;
        private System.Windows.Forms.DataGridViewTextBoxColumn AccCap;
        private System.Windows.Forms.DataGridViewTextBoxColumn DetPct;
        private System.Windows.Forms.DataGridViewTextBoxColumn DetCap;
        private System.Windows.Forms.DataGridViewTextBoxColumn CritPct;
        private System.Windows.Forms.DataGridViewTextBoxColumn CritCap;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpdPct;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpdCap;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}