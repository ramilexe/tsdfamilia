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
        }
    }
}