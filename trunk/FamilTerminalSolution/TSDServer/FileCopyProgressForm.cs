using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class FileCopyProgressForm : Form
    {
        public string FormCaption
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }
        public FileCopyProgressForm()
        {
            InitializeComponent();
        }

        public void SetProgress(long total, long current)
        {

            progressBar1.Maximum = 100;
            progressBar1.Value = (int)(100.0 * current / total);
            if (current >= total)
            {
                this.DialogResult = DialogResult.OK;
            }

        }
        public void SetError(long total, long current, Exception err)
        {

            this.DialogResult = DialogResult.Abort;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите остановить копирование данных на терминал ?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Abort;
            }
        }
    }
}
