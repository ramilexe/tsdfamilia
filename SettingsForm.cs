using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }


        private void SettingsForm_Load(object sender, EventArgs e)
        {
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (this.Controls[i].Name.IndexOf("button") >= 0)
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
            this.Refresh();
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
                new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold | FontStyle.Underline);

            b.Font = f;

        }
        void b_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is System.Windows.Forms.Button)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
                if (e.KeyCode == Keys.Enter)//e.KeyValue == 13)
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

                case 48: this.DialogResult = DialogResult.OK;  this.Close(); break;
                case 49:
                    {
                        using (SearchPrinterForm srchFrm = new SearchPrinterForm())
                        {
                            srchFrm.ShowDialog();
                        }
                        this.Refresh();
                        break;
                    }
                case 50:
                    {
                        
                        using (AcceptClearDataForm frm =
                            new AcceptClearDataForm())
                        {
                            if (frm.ShowDialog() == DialogResult.Yes)
                            {
                                string result = "";
                                try
                                {
                                    ActionsClass.Action.ClearScannedData();
                                    result = "Ранее отсканированные данные очищены";
                                }
                                catch (Exception err)
                                {
                                    result = string.Format("Ошибка очистки базы данных \n{0}", err.Message);
                                }
                                using (EndClearDataForm endFrm =
                                    new EndClearDataForm(result))
                                {
                                    endFrm.ShowDialog();
                                }
                            }
                        }
                        
                        break;
                    }
                case 51:
                    {
                        using (ViewLoadDateForm srchFrm = new ViewLoadDateForm())
                        {
                            srchFrm.ShowDialog();
                        }
                        this.Refresh();
                        break;
                    }


                default:
                    {
                        return;
                    }

            }
        }
    }
}