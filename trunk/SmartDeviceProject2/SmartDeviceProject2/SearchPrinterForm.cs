using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class SearchPrinterForm : Form
    {
        System.Collections.Specialized.StringDictionary stringDict =
            new System.Collections.Specialized.StringDictionary();

        
        public SearchPrinterForm()
        {
            InitializeComponent();

            try
            {
                BTPrintClass.PrintClass.Disconnect();
            }
            catch { }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    try
                    {
                        BTPrintClass.PrintClass.SetDefaultDevice(listBox1.SelectedItem.ToString());
                        BTPrintClass.PrintClass.TestPrint3(listBox1.SelectedItem.ToString());
                    }
                    catch { }
                    this.Close();
                }
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
            catch {
                this.Close();
            }
            


        }

        private void SearchPrinterForm_Load(object sender, EventArgs e)
        {
            this.listBox1.Focus();
            //BTPrintClass.PrintClass.BTPrinterInit();
            BTPrintClass.PrintClass.SearchDevices();
            stringDict = BTPrintClass.PrintClass.GetFoundedDevicesDict();

            foreach (string s in stringDict.Keys)
            {
                this.listBox1.Items.Add(s);
            }
            this.Refresh();



        }

        private void SearchPrinterForm_Deactivate(object sender, EventArgs e)
        {

        }

        private void SearchPrinterForm_Closing(object sender, CancelEventArgs e)
        {
            BTPrintClass.PrintClass.Disconnect();
        }
    }
}