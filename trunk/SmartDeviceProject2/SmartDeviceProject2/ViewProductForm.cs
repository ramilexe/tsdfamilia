using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class ViewProductForm : Form
    {
        ProductsDataSet _products;
        ScannedProductsDataSet _scannedProducts;
        Color lastColor;

        public ViewProductForm()
        {
            InitializeComponent();
            Program.scaner.InitScan();
        }

        public ViewProductForm(ProductsDataSet products, ScannedProductsDataSet scannedProducts):this()
        {
            _products = products;
            _scannedProducts = scannedProducts;
        }


        private void closeFrmBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        void Scanned(string barcode)
        {
            if (this.InvokeRequired)
            {
                Familia.TSDClient.Scanned del = new Scanned(Scanned);
                this.Invoke(del, barcode);
            }
            else
            {
                textBox5.Text = "";
                this.textBox1.Text = barcode;

                ProductsDataSet.ProductsTblRow row =
                    GetProductRow(barcode);
                if (row != null)
                {

                    //Graphics g = Graphics.(textBox1.);
                    //SizeF f = g.MeasureString("String to fit", textBox1.Font);
                    if (row.ProductName.Length > 15)
                    {
                        textBox1.Text = row.ProductName.Substring(0, 15);
                        textBox5.Text = row.ProductName.Substring(15);
                    }
                    else
                        textBox1.Text = row.ProductName;

                    textBox2.Text = row.NewPrice.ToString();
                    textBox3.Text = row.OldPrice.ToString();
                    textBox4.Text = row.Article;

                }
                else
                {
                    textBox5.Text = "...Товар не найден...";
                }
            }
        }
        private void ViewProductForm_Load(object sender, EventArgs e)
        {
            Program.scaner.ResumeScan();
            Program.scaner.OnScanned += new Scanned(Scanned);

            foreach (Control c in this.Controls)
            {
                c.GotFocus += new EventHandler(c_GotFocus);
                c.LostFocus += new EventHandler(c_LostFocus);
            }
            this.textBox1.Focus();
            lastColor = this.textBox1.BackColor;
            
        }

        void c_LostFocus(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = lastColor;
        }

        void c_GotFocus(object sender, EventArgs e)
        {
            lastColor = this.textBox1.BackColor;
            ((Control)sender).BackColor = Color.Plum;
        }

        private void ViewProductForm_Closing(object sender, CancelEventArgs e)
        {
            Program.scaner.PauseScan();
            Program.scaner.OnScanned = null;

        }

        private ProductsDataSet.ProductsTblRow GetProductRow(string barcode)
        {
            using (ProductsDataSetTableAdapters.ProductsTblTableAdapter ta
                = new Familia.TSDClient.ProductsDataSetTableAdapters.ProductsTblTableAdapter())
            {

                ProductsDataSet.ProductsTblDataTable table = 
                    ta.GetDataByBarcode(barcode);
                if (table.Rows.Count > 0)
                    return table[0];
                else
                    return null;
            }

        }

        private void closeFrmBtn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void printBtn_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}