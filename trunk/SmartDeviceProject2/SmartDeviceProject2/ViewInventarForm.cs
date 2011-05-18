using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ViewInventarForm : Form
    {
        
        string _docId;
        byte _docType;

        public ViewInventarForm()
        {
            InitializeComponent();
        }

        public ViewInventarForm(
            string docId,
            byte docType)
            : this()
        {
            _docId = docId;
            _docType = docType;

        }


        private void ViewInventarForm_Load(object sender, EventArgs e)
        {
            this.listBox1.Font = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Regular);
            this.listBox1.Items.Clear();


            ScannedProductsDataSet.ScannedBarcodesRow [] rows
             = ActionsClass.Action.ScannedProducts.ScannedBarcodes.FindByDocIdAndDocType
                (_docId,
                _docType);

            if (rows != null &&
                rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    ProductsDataSet.ProductsTblRow r = 
                        ActionsClass.Action.GetProductRow(
                        rows[i].Barcode.ToString());
                    
                    if (r != null)
                    listBox1.Items.Add(
                        string.Format(
                        "{0:D13}|{1,12}|{2}",
                        rows[i].Barcode,
                        (r.ProductName.Length>12)?r.ProductName.Substring(0,12):
                        r.ProductName,
                        rows[i].FactQuantity.ToString()));
                    else
                        listBox1.Items.Add(
                        string.Format(
                        "{0:D13}|{1,12}|{2}",
                        rows[i].Barcode,
                        "Товар не найден",
                        rows[i].FactQuantity.ToString()));
                        
                }
            }

            this.Width = 238;
            this.Height = 295;
            listBox1.Focus();

            this.Refresh();


            
            
        }
        void InventarForm_Closed(object sender, System.EventArgs e)
        {
        }


        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
             e.KeyValue == 115)//YellowBtn
            {
                this.Close();
                return;
            }
        }
            

    }
}