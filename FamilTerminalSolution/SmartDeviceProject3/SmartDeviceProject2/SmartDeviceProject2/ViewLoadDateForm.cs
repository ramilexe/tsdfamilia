using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ViewLoadDateForm : Form
    {
        
        public ViewLoadDateForm()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(ViewLoadDateForm_KeyDown);
            this.button1.KeyDown += new KeyEventHandler(ViewLoadDateForm_KeyDown);
        }

        void ViewLoadDateForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                this.Close();
                return;
            }
        }


        private void ViewLoadDateForm_Load(object sender, EventArgs e)
        {
            ActionsClass.Action.OpenProducts();
            bool noerrors = true;
            string result = ActionsClass.Action.TestDB(out noerrors);

            ProductsDataSet.ProductsTblRow row =
                ActionsClass.Action.GetProductRow("0");
            this.label1.TextAlign = ContentAlignment.TopCenter;
            if (row != null)
            {
                this.label1.Text = string.Format(
                    row.ProductName);
            }
            else
            {
                this.label1.Text = "Нет данных о последней загрузке.";
            }
            ActionsClass.Action.CloseProducts();
            if (!noerrors)
            {

                this.listBox1.ForeColor = Color.Red;
                this.listBox1.Items.Add("Ошибки базы данных: ");
                foreach (string s in result.Split('\n'))
                {
                    this.listBox1.Items.Add(s);
                }
            }
            else
            {
                this.listBox1.ForeColor = Color.Green;
                this.listBox1.Items.Add("База данных исправна:");
                foreach (string s in result.Split('\n'))
                {
                    this.listBox1.Items.Add(s);
                }
            }
            this.listBox1.SelectedIndex = 0;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                this.Close();
                return;
            }
        }
    }
}