namespace FFXIVGearTracker
{
	partial class AddCharacterForm
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
			this.CharacterNameTextBox = new System.Windows.Forms.TextBox();
			this.CharAcceptButton = new System.Windows.Forms.Button();
			this.CharCancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// CharacterNameTextBox
			// 
			this.CharacterNameTextBox.Location = new System.Drawing.Point(12, 12);
			this.CharacterNameTextBox.Name = "CharacterNameTextBox";
			this.CharacterNameTextBox.Size = new System.Drawing.Size(232, 20);
			this.CharacterNameTextBox.TabIndex = 0;
			// 
			// CharAcceptButton
			// 
			this.CharAcceptButton.Location = new System.Drawing.Point(12, 43);
			this.CharAcceptButton.Name = "CharAcceptButton";
			this.CharAcceptButton.Size = new System.Drawing.Size(113, 25);
			this.CharAcceptButton.TabIndex = 1;
			this.CharAcceptButton.Text = "&Accept";
			this.CharAcceptButton.Click += new System.EventHandler(this.CharAcceptButton_Click);
			// 
			// CharCancelButton
			// 
			this.CharCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CharCancelButton.Location = new System.Drawing.Point(131, 43);
			this.CharCancelButton.Name = "CharCancelButton";
			this.CharCancelButton.Size = new System.Drawing.Size(113, 25);
			this.CharCancelButton.TabIndex = 2;
			this.CharCancelButton.Text = "&Cancel";
			this.CharCancelButton.Click += new System.EventHandler(this.CharCancelButton_Click);
			// 
			// AddCharacterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(259, 81);
			this.ControlBox = false;
			this.Controls.Add(this.CharCancelButton);
			this.Controls.Add(this.CharAcceptButton);
			this.Controls.Add(this.CharacterNameTextBox);
			this.Name = "AddCharacterForm";
			this.Text = "Enter Character Name";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox CharacterNameTextBox;
		private System.Windows.Forms.Button CharAcceptButton;
		private System.Windows.Forms.Button CharCancelButton;
	}
}