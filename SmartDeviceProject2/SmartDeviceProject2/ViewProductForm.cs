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
        ScanClass scaner = new ScanClass();
        TSDServer.CustomEncodingClass MyEncoder = new TSDServer.CustomEncodingClass();
        public ViewProductForm()
        {
            InitializeComponent();
            
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
                label5.Text = "";
                label6.Text = "";
                label7.Text = "";
                label8.Text = "";

                label5.Text = barcode;

                ProductsDataSet.ProductsTblRow row =
                    GetProductRow(barcode);
                if (row != null)
                {

                    //Graphics g = Graphics.(textBox1.);
                    //SizeF f = g.MeasureString("String to fit", textBox1.Font);
                    if (row.ProductName.Length > 15)
                    {
                        label5.Text = row.ProductName.Substring(0, 15);
                        label6.Text = row.ProductName.Substring(15);
                    }
                    else
                        label5.Text = row.ProductName;

                    label7.Text = row.NewPrice.ToString();
                    label8.Text = row.OldPrice.ToString();
                    label9.Text = row.Article;

                }
                else
                {
                    label5.Text = "...Товар не найден...";
                }
            }
        }
        private void ViewProductForm_Load(object sender, EventArgs e)
        {
            scaner.InitScan();
            scaner.OnScanned += new Scanned(Scanned);

            foreach (Control c in this.Controls)
            {
                c.GotFocus += new EventHandler(c_GotFocus);
                c.LostFocus += new EventHandler(c_LostFocus);
            }
            /*this.textBox1.Focus();
            lastColor = this.textBox1.BackColor;*/
            
        }

        void c_LostFocus(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = lastColor;
        }

        void c_GotFocus(object sender, EventArgs e)
        {
            //lastColor = this.textBox1.BackColor;
            ((Control)sender).BackColor = Color.Plum;
        }

        private void ViewProductForm_Closing(object sender, CancelEventArgs e)
        {
            
            scaner.OnScanned = null;
            scaner.StopScan();
        }

        private ProductsDataSet.ProductsTblRow GetProductRow(string barcode)
        {
            ProductsDataSet.ProductsTblRow row = _products.ProductsTbl.FindByBarcode(long.Parse(barcode));
            if (row == null)
            {
                using (ProductsDataSetTableAdapters.ProductsBinTblTableAdapter ta
                    = new Familia.TSDClient.ProductsDataSetTableAdapters.ProductsBinTblTableAdapter())
                {
                    
                    ProductsDataSet.ProductsBinTblDataTable table =
                        ta.GetDataByBarcode(long.Parse(barcode));
                    if (table.Rows.Count > 0)
                    {
                        row = _products.ProductsTbl.NewProductsTblRow();
                        for (int i = 0; i < _products.ProductsTbl.Columns.Count; i++)
                        {
                            if (table[0][i] != null &&
                            table[0][i] != System.DBNull.Value)
                            {
                                if (table.Columns[i].DataType == typeof(byte[]))
                                {
                                    byte[] buffer = (byte[])table[0][i];
                                    //byte[] newBuf = System.Text.Encoding.Convert(Encoding.GetEncoding(866),
                                    //    Encoding.Default, buffer);
                                    row[i] = MyEncoder.GetString(buffer); //System.Text.Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                                    
                                 //   row[i] = System.Text.Encoding.Default.GetString(
                                 //   (byte[])table[0][i], 0, ((byte[])table[0][i]).Length);

                                }
                                else
                                {
                                    row[i] = table[0][i];
                                }
                                
                            }
                                //row.Article = System.Text.Encoding.Default.GetString(
                                //    table[0].Article, 0, table[0].Article.Length);
                        }
                        /*if (table[0]["Artilce"] != null &&
                            table[0]["Artilce"] != System.DBNull.Value)
                            row.Article = System.Text.Encoding.Default.GetString(
                                table[0].Article, 0, table[0].Article.Length);

                        row.Barcode = table[0].Barcode;
                        row.Country = System.Text.Encoding.Default.GetString(
                            table[0].Country, 0, table[0].Country.Length);
                        row.DiscountRate = System.Text.Encoding.Default.GetString(
                            table[0].DiscountRate, 0, table[0].DiscountRate.Length);
                        row.NavCode = System.Text.Encoding.Default.GetString(
                            table[0].NavCode, 0, table[0].NavCode.Length);
                        row.NewPrice = table[0].NewPrice;
                        row.OldPrice = table[0].OldPrice;
                        row.ProductName = System.Text.Encoding.Default.GetString(
                            table[0].ProdName, 0, table[0].ProdName.Length);
                        row.ProjectNumber = System.Text.Encoding.Default.GetString(
                            table[0].ProjectNumber, 0, table[0].ProjectNumber.Length);
                        row.PurchasePrice = table[0].PurchasePrice;
                        row.ReturnDate = table[0].ReturnDate;
                        row.Structure = System.Text.Encoding.Default.GetString(
                            table[0].Structure, 0, table[0].Structure.Length);
                        row.TransferDate = table[0].TransferDate;*/

                        _products.ProductsTbl.AddProductsTblRow(row);
                        _products.AcceptChanges();

                        return row;

                    }
                    else
                        return null;
                }
            }
            else
                return row;

        }

        private void closeFrmBtn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            if (e.KeyValue == 27)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            
        }

        private void printBtn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 27)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
 
        }

        private void searchBtn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 27)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}