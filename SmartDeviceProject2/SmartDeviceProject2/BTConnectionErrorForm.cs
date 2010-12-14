using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class BTConnectionErrorForm : Form
    {
        public BTConnectionErrorForm()
        {
            InitializeComponent();
        }


        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.No;
                this.Close();
            }
        }

        private void BTConnectionErrorForm_Load(object sender, EventArgs e)
        {
            button1.Focus();
        }
    }
}