using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WarframeLauncher
{
    public partial class PasswordBox : Form
    {
        public PasswordBox()
        {
            InitializeComponent();
        }

        private void PasswordBox_Load(object sender, EventArgs e)
        {
            buttonClear.Enabled = !(String.IsNullOrEmpty(Properties.Settings.Default.GamePassword));
        }

        private void SaveNewPassword(string newPassword)
        {
            Properties.Settings.Default.GamePassword = textPasswordBox.Text;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }

        private void textPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveNewPassword(textPasswordBox.Text);
            } else if (e.KeyCode == Keys.Escape)
            {
                // Exit without doing anything
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveNewPassword(textPasswordBox.Text);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            SaveNewPassword("");
        }
    }
}
