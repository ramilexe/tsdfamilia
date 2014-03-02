using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class WaitForm : Form
    {
        public event EndLoadDelegate onEndLoad;

        public WaitForm()
        {
            InitializeComponent();
            this.TopMost = true;
            OutRefresh += new OnRefresh(WaitForm_OutRefresh);
            //onEndLoad += new EndLoadDelegate(WaitForm_onEndLoad);
        }

        public void WaitForm_OutRefresh()
        {
            if (this.InvokeRequired)
            {
                OnRefresh del = new OnRefresh(WaitForm_OutRefresh);
                this.Invoke(del);
            }
            else
                this.Refresh();
        }

        public delegate void OnRefresh();
        public event OnRefresh OutRefresh;

        
        private void WaitForm_Load(object sender, EventArgs e)
        {
            this.Width = 235;
            this.Height = 295;
            this.timer1.Enabled = true;

            

        }

        //void WaitForm_onEndLoad()
        //{
        //    if (this.InvokeRequired)
        //    {
        //        EndLoadDelegate del = new EndLoadDelegate(WaitForm_onEndLoad);
        //        this.Invoke(del);
        //    }
        //    else
        //    {
        //         timer1.Enabled = false;
        //         this.Close();
        //    }
        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                
                if (this.progressBar1.Value >= 99)
                    this.progressBar1.Value = 0;
                else
                    this.progressBar1.Value++;
                Application.DoEvents();
                this.Activate();
                this.Refresh();
                
            }
            catch { }
        }
    }
}