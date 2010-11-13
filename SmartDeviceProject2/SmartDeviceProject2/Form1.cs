using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //try
            //{
            //    //BTPrintClass p = new BTPrintClass()

            //    BTPrintClass.PrintClass.OnSetError += new BTPrintClass.SetError(PrintClass_OnSetError);
            //    BTPrintClass.PrintClass.BTPrinterInit();
            //    BTPrintClass.PrintClass.SearchDevices();
            //    Calib.BluetoothLibNet.BTST_DEVICEINFO[] devices =
            //        BTPrintClass.PrintClass.GetFoundedDevices();
            //    /*System.Collections.Specialized.StringDictionary dict =
            //        BTPrintClass.PrintClass.GetFoundedDevicesDict();*/

            //    BTPrintClass.PrintClass.ConnToPrinter(devices[0]);
            //    string label1 = "***** TEST PRINT CS.NET *****" + "\r" + "\n"
            //        + "IT-600 or DT-X11" + "\r" + "\n"
            //        + "CASIO" + "\r" + "\n"
            //        + "www.casio.co.jp/English/system/" + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "CASIO Computer Co., Ltd." + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "6-2, Hon-machi 1-chome" + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "Shibuya-ku, Tokyo 151-8543, Japan" + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "Tel.: +81-3-5334-4771" + "\r" + "\n"
            //        + "Fax.: +81-3-5334-4656" + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "\r" + "\n"
            //        + "\r" + "\n";

            //    BTPrintClass.PrintClass.Print(label1);
            //}
            //finally
            //{
            //    BTPrintClass.PrintClass.Disconnect();
            //}
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            for (int i = 0;  i < this.Controls.Count; i++)
            {
                if (this.Controls[i].Name.IndexOf("button")>=0)
                {
                    System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)this.Controls[i];
                    b.Click += new EventHandler(button_Click);
                    b.KeyDown += new KeyEventHandler(b_KeyDown);
                    b.GotFocus += new EventHandler(b_GotFocus);
                    b.LostFocus += new EventHandler(b_LostFocus);
                    b.KeyPress += new KeyPressEventHandler(b_KeyPress);
                    System.Drawing.Font f =
                        new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);

                    b.Font = f;
                }
            }
            

            
        }

        void b_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        void b_LostFocus(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)sender;

            b.BackColor = Color.PaleGreen;
            System.Drawing.Font f =
                new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);

            b.Font = f;
            
        }

        void b_GotFocus(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b
             = (System.Windows.Forms.Button)sender;

            b.BackColor = Color.Plum;

            System.Drawing.Font f =
                new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold|FontStyle.Underline);

            b.Font = f;
            
        }

        void b_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is System.Windows.Forms.Button)
            {
                if (e.KeyValue == 13)
                {
                    System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)sender;

                    if (b.Name.IndexOf("button") >= 0)
                    {
                        string id = b.Name.Replace("button", "");
                        MenuEvents(id[0]);
                    }
                }
                else
                {
                    MenuEvents(e.KeyValue);
                }
            }
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void button_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)sender;

            if (b.Name.IndexOf("button") >= 0)
            {
                string id = b.Name.Replace("button", "");
                MenuEvents(id[0]);

            }
        }

        void PrintClass_OnSetError(string text)
        {
            //throw new NotImplementedException();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            MenuEvents(e.KeyChar);
        }

        void MenuEvents(char menuId)
        {
            MenuEvents(Convert.ToInt32(menuId));
        }

        void MenuEvents(int menuId)
        {

            switch (menuId)
            {

                case 48: Application.Exit(); break;
                case 49: { 
                    using (ViewProductForm frm = new ViewProductForm()) 
                    { 
                        frm.ShowDialog(); 
                    } 
                    break; 
                }

                default:
                    {
                        return;
                    }

            }
        }

        private void FillData()
        {


        }
    }
}