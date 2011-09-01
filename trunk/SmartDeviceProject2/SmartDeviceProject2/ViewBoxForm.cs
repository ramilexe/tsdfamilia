using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ViewBoxForm : Form
    {
        Dictionary<long,ListBoxItem> items =
            new Dictionary<long,ListBoxItem>();

        string _docId;
        byte _docType;

        public ViewBoxForm()
        {
            InitializeComponent();
        }

        public ViewBoxForm(
            string docId,
            byte docType)
            : this()
        {
            _docId = docId;
            _docType = docType;

            this.listBox1.Font = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Regular);
            this.listBox1.Items.Clear();

        }


        private void ViewInventarForm_Load(object sender, EventArgs e)
        {
            ScannedProductsDataSet.ScannedBarcodesRow [] rows
             = ActionsClass.Action.FindByDocIdAndDocType
                (_docId,
                _docType);
            items.Add(-1, new FirstListBoxItem());

            if (rows != null &&
                rows.Length > 0)
            {
                
                for (int i = 0; i < rows.Length; i++)
                {
                    ProductsDataSet.ProductsTblRow r = 
                        ActionsClass.Action.GetProductRow(
                        rows[i].Barcode.ToString());

                    
                    
                    if (r != null)
                    /*listBox1.Items.Add(
                        string.Format(
                        "{0:D13}|{1,12}|{2}",
                        rows[i].Barcode,
                        (r.ProductName.Length>12)?r.ProductName.Substring(0,12):
                        r.ProductName,
                        rows[i].FactQuantity.ToString()));
                     */
                    {
                        if (items.ContainsKey(rows[i].Barcode))
                            items[rows[i].Barcode].FactQuantity += rows[i].FactQuantity;
                        else
                            items.Add(rows[i].Barcode,
                                new ListBoxItem(
                                    rows[i].Barcode,
                                    r.ProductName,
                                    rows[i].FactQuantity,
                                    rows[i].PlanQuanity));
                    }
                    else
                        if (items.ContainsKey(rows[i].Barcode))
                            items[rows[i].Barcode].FactQuantity += rows[i].FactQuantity;
                        else
                            items.Add(rows[i].Barcode,
                                new ListBoxItem(
                                    rows[i].Barcode,
                                    "Товар не найден",
                                    rows[i].FactQuantity,
                                    rows[i].PlanQuanity));

                        /*
                        listBox1.Items.Add(
                        string.Format(
                        "{0:D13}|{1,12}|{2}",
                        rows[i].Barcode,
                        "Товар не найден",
                        rows[i].FactQuantity.ToString()));*/
                        
                }
            }
            foreach (long bk in items.Keys)
            {
                listBox1.Items.Add(items[bk]);
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

        
        public class ListBoxItem
        {
            public long Barcode;
            public string ProductName;
            public int FactQuantity;
            public int PlanQuantity;

            public ListBoxItem(long _Barcode, string _ProductName, int _FactQuantity, int _planQuantity)
            {
                Barcode = _Barcode;
                ProductName = _ProductName;
                FactQuantity = _FactQuantity;
                PlanQuantity = _planQuantity;
            }
            public override string ToString()
            {
                return string.Format(
                        "{0:D13}|{1,10}|{2,2}|{3,2}",
                        Barcode,
                        (ProductName.Length > 10) ? ProductName.Substring(0, 10) :
                            ProductName,
                        FactQuantity.ToString(),
                        PlanQuantity);
            }
        }

         public class FirstListBoxItem:ListBoxItem
        {

            public FirstListBoxItem():
                base(0,string.Empty,0,0)
             {

             }

            
            public override string ToString()
            {
                return string.Format(
                        "{0,13}|{1,10}|{2}|{3}",
                        "Штрихкод",
                        "Код тов",
                        "Факт",
                        "План");
            }
        }
    
    }
}