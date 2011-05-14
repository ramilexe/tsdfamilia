using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class DialogForm : Form
    {
        public DialogForm()
        {
            BTPrintClass.PrintClass.SetStatusEvent("Open DialogForm form");
            InitializeComponent();
        }

        public DialogForm(
            string line1,
            string line2,
            string line3,
            string caption
            )
            : this()
        {
            label1.Text = line1;
            label2.Text = line2;
            label3.Text = line3;
            this.Text = caption;
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.Yes;
                BTPrintClass.PrintClass.SetStatusEvent("DialogForm form - choose Yes");
                this.Close();
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.No;
                BTPrintClass.PrintClass.SetStatusEvent("DialogForm form - choose No");
                this.Close();
            }
        }

        private void BTConnectionErrorForm_Load(object sender, EventArgs e)
        {
            button1.Focus();
        }
    }
}