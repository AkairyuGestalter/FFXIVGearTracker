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
	public partial class AddCharacterForm : System.Windows.Forms.Form
	{
		public String CharacterName { get; set; }

		public AddCharacterForm()
		{
			InitializeComponent();
		}

		private void CharAcceptButton_Click(object sender, EventArgs e)
		{
			CharacterName = CharacterNameTextBox.Text;
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Hide();
		}

		private void CharCancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Hide();
		}
	}
}
