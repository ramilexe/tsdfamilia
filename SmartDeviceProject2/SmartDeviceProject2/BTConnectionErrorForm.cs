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
            BTPrintClass.PrintClass.SetStatusEvent("Open BTConnectionErrorForm form");
            InitializeComponent();
        }


        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.Yes;
                BTPrintClass.PrintClass.SetStatusEvent("BTConnectionErrorForm form - choose Yes");
                this.Close();
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.No;
                BTPrintClass.PrintClass.SetStatusEvent("BTConnectionErrorForm form - choose No");
                this.Close();
            }
        }

        private void BTConnectionErrorForm_Load(object sender, EventArgs e)
        {
            button1.Focus();
        }
    }
}