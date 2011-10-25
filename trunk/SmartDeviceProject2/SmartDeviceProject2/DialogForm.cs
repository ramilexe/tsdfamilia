using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class DialogForm : Form
    {
        //string _line1;

        public string Line1
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }
        //string _line2;

        public string Line2
        {
            get { return label2.Text; }
            set { label2.Text = value; }
        }
       // string _line3;

        public string Line3
        {
            get { return label3.Text; }
            set { label3.Text = value; }
        }
        //string _caption;

        public string Caption
        {
            get { return this.Text; }
            set { this.Text = value; }
        }

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
            this.Refresh();
            button1.Focus();
        }
    }

    public class DialogFrm
    {
        private static DialogForm frm = null;

        public static DialogForm Dialog
        {

            get
            {
                if (frm == null)
                {
                    frm = new DialogForm();
                }
                return frm;

            }
        }

        public static System.Windows.Forms.DialogResult
            ShowMessage(string line1,
            string line2,
            string line3,
            string caption)
        {
            Dialog.Line1 = line1;
            Dialog.Line2 = line2;
            Dialog.Line3 = line3;
            Dialog.Caption = caption;
            Dialog.Refresh();
            return frm.ShowDialog();
        }
    }

}