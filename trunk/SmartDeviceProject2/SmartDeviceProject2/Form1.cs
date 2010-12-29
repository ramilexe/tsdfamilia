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
        public static event DatabaseChanged OnDatabaseChaned;
        //ProductsDataSet _products = ActionsClass.Action.Products;
        //ScannedProductsDataSet _scannedProducts = ActionsClass.Action.ScannedProducts;

        System.Threading.Timer tmr =
            new System.Threading.Timer(new System.Threading.TimerCallback(OnTimer)
                , null
                , System.Threading.Timeout.Infinite
                , System.Threading.Timeout.Infinite);
        private static DateTime lastCreationDatabaseTime = DateTime.Now;
        
        private static void OnTimer(object state)
        {
            /*DateTime dt = System.IO.File.GetCreationTime("Products.sdf");
            if (dt != lastCreationDatabaseTime)
            {
                if (OnDatabaseChaned != null)
                    OnDatabaseChaned();
            }*/
                
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
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
            if (Program.Default.EnableExit != 1)
                button0.Enabled = false;
            else
                button0.Enabled = true;

            ActionsClass.Action.LoadScannedData();
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
                if (e.KeyCode == Keys.Escape)
                {
                    Application.Exit();
                }
                if (e.KeyValue == 13 || e.KeyCode == Keys.Enter)
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
            try
            {
                switch (menuId)
                {

                    case 48:
                        {
                            if (Program.Default.EnableExit == 1)
                                Application.Exit(); 
                            break;
                        }
                    case 49:
                        {
                            using (ViewProductForm frm = new ViewProductForm())
                            {
                                frm.ShowDialog();
                            }
                            break;
                        }
                    case 53:
                        {
                            using (SettingsForm frm = new SettingsForm())
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
            catch (Exception err)
            {
                MessageBox.Show(err.Message
                    , "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
            this.Refresh();
        }

        private void FillData()
        {


        }

        
    }
}