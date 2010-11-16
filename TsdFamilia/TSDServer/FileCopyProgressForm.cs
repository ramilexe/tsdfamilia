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
        public FileCopyProgressForm()
        {
            InitializeComponent();
        }

        public void SetProgress(long total, long current)
        {

            progressBar1.Maximum = 100;
            progressBar1.Value = (int)(100.0 * current / total);

        }
    }
}
