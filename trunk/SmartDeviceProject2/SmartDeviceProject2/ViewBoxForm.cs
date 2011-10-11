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
        
        Dictionary<long,
            ProductsDataSet.ProductsTblRow> products =
            new Dictionary<long, ProductsDataSet.ProductsTblRow>();

        string _docId;
        byte _docType;

        ProductsDataSet.DocsTblRow[] docsRows = null;
        ScannedProductsDataSet.ScannedBarcodesRow[] rows = null;

        public ViewBoxForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Конструктор для формы просмотра ттн
        /// </summary>
        /// <param name="docId">штрихкод короба</param>
        /// <param name="docType">тип документа BoxWProducts=10</param>
        public ViewBoxForm(
            string docId,
            byte docType)
            : this()
        {
            _docId = docId;
            _docType = docType;

            this.listBox1.Font = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Regular);
            this.listBox1.Items.Clear();

            //this.treeView1.Font = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Regular);
            //this.treeView1.Nodes.Clear();

        }


        private void ViewInventarForm_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        void InventarForm_Closed(object sender, System.EventArgs e)
        {
        }

        private void RefreshData()
        {
            products.Clear();
            items.Clear();
            listBox1.Items.Clear();

            rows = ActionsClass.Action.FindByDocIdAndDocType(_docId,_docType);

            docsRows = ActionsClass.Action.GetDataByDocIdAndType(_docId, _docType);

            //items.Add(-1, new FirstListBoxItem());

            OpenNETCF.Windows.Forms.ListItem li0 = listBox1.Items.Add(new FirstListBoxItem().ToString());
            li0.Font = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Regular);
            label2.Font = new Font(FontFamily.GenericMonospace, 8f, FontStyle.Regular);

            if (docsRows != null &&
                docsRows.Length > 0)
            {


                for (int i = 0; i < docsRows.Length; i++)
                {
                    
                    ProductsDataSet.ProductsTblRow r =
                        ActionsClass.Action.GetProductRowByNavCode(
                            docsRows[i].NavCode);

                    if (!products.ContainsKey(r.Barcode))
                        products.Add(r.Barcode,r);
                    //rows[i].Barcode.ToString());


                    //ScannedProductsDataSet.ScannedBarcodesRow scanRow = null;




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
                        if (rows != null)
                        {
                            for (int j = 0; j < rows.Length; j++)
                            {

                                if (rows[j].Barcode == r.Barcode)
                                {
                                    //scanRow = rows[j];

                                    if (items.ContainsKey(r.Barcode))
                                    {
                                        items[r.Barcode].FactQuantity += rows[j].FactQuantity;
                                        if (items[r.Barcode].FactQuantity == rows[j].PlanQuanity)
                                            items[r.Barcode].ForeColor = Color.Green;
                                        else
                                            if (items[r.Barcode].FactQuantity > rows[j].PlanQuanity)
                                                items[r.Barcode].ForeColor = Color.Red;

                                    }
                                    else
                                    {
                                        items.Add(r.Barcode,
                                            new ListBoxItem(
                                                r.Barcode,
                                                r.ProductName,
                                                rows[j].FactQuantity,
                                                docsRows[i].Quantity));

                                        /*
                                        if (items[r.Barcode].FactQuantity == rows[j].PlanQuanity)
                                            items[r.Barcode].ForeColor = Color.Green;
                                        else
                                            if (items[r.Barcode].FactQuantity > rows[j].PlanQuanity)
                                                items[r.Barcode].ForeColor = Color.Red;*/

                                    }
                                }
                            }
                        }
                        else
                        {
                            items.Add(r.Barcode,
                                new ListBoxItem(
                                    r.Barcode,
                                    r.ProductName,
                                    0,
                                    docsRows[i].Quantity));
                        }

                        /*if (scanRow != null)
                        {//есть отсканированые, товар найден
                            if (items.ContainsKey(r.Barcode))
                                items[r.Barcode].FactQuantity += scanRow.FactQuantity;
                            else
                                items.Add(r.Barcode,
                                    new ListBoxItem(
                                        r.Barcode,
                                        r.ProductName,
                                        scanRow.FactQuantity,
                                        docsRows[i].Quantity));
                        }
                        else
                        {*/
                        //нет отсканированных, товар найден
                        if (!items.ContainsKey(r.Barcode))
                            //items[r.Barcode].FactQuantity += scanRow.FactQuantity;
                            //else
                            items.Add(r.Barcode,
                                new ListBoxItem(
                                    r.Barcode,
                                    r.ProductName,
                                    0,
                                    docsRows[i].Quantity));
                        //}
                    }
                    /*else
                     * //отсканированный товар не найден в табл.продуктов - мало вероятно. не должно быть
                        if (items.ContainsKey(rows[i].Barcode))
                            items[rows[i].Barcode].FactQuantity += rows[i].FactQuantity;
                        else
                            items.Add(rows[i].Barcode,
                                new ListBoxItem(
                                    rows[i].Barcode,
                                    "Товар не найден",
                                    rows[i].FactQuantity,
                                    rows[i].PlanQuanity));*/

                    /*
                    listBox1.Items.Add(
                    string.Format(
                    "{0:D13}|{1,12}|{2}",
                    rows[i].Barcode,
                    "Товар не найден",
                    rows[i].FactQuantity.ToString()));*/

                }
            }

            Dictionary<long, ListBoxItem> itemsNew =
                new Dictionary<long, ListBoxItem>();

            if (rows != null)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    if (itemsNew.ContainsKey(rows[i].Barcode))
                    {
                        itemsNew[rows[i].Barcode].FactQuantity += rows[i].FactQuantity;
                        /*
                        if (itemsNew[rows[i].Barcode].FactQuantity == rows[i].PlanQuanity)
                            itemsNew[rows[i].Barcode].ForeColor = Color.Green;
                        else
                            if (itemsNew[rows[i].Barcode].FactQuantity > rows[i].PlanQuanity)
                                itemsNew[rows[i].Barcode].ForeColor = Color.Red;
                        */

                    }
                    else
                    {
                        ProductsDataSet.ProductsTblRow r =
                            ActionsClass.Action.GetProductRow(rows[i].Barcode.ToString());

                        if (!products.ContainsKey(r.Barcode))
                            products.Add(r.Barcode,r);

                        itemsNew.Add(r.Barcode,
                            new ListBoxItem(
                                r.Barcode,
                                r.ProductName,
                                rows[i].FactQuantity,
                                0));

                        //itemsNew[r.Barcode].ForeColor = Color.Red;
                    }
                }
            }

            int planQ = 0;
            int factQ = 0;
            foreach (long bk in items.Keys)
            {
                OpenNETCF.Windows.Forms.ListItem li = listBox1.Items.Add(items[bk].ToString());

                li.ForeColor = items[bk].ForeColor;

                //TreeNode tn = treeView1.Nodes.Add(items[bk].ToString());
                if (items[bk].PlanQuantity == items[bk].FactQuantity)
                    li.ForeColor = Color.Green;

                if (items[bk].FactQuantity > items[bk].PlanQuantity)
                    li.ForeColor = Color.Red;


                planQ += items[bk].PlanQuantity;
                factQ += items[bk].FactQuantity;
            }

            foreach (long bk in itemsNew.Keys)
            {
                if (!items.ContainsKey(bk))
                {
                    OpenNETCF.Windows.Forms.ListItem li = listBox1.Items.Add(itemsNew[bk].ToString());
                    //TreeNode tn = treeView1.Nodes.Add(itemsNew[bk].ToString());
                    li.ForeColor = itemsNew[bk].ForeColor;

                    if (itemsNew[bk].PlanQuantity == itemsNew[bk].FactQuantity)
                        li.ForeColor = Color.Green;

                    if (itemsNew[bk].FactQuantity > itemsNew[bk].PlanQuantity)
                        li.ForeColor = Color.Red;

                    planQ += itemsNew[bk].PlanQuantity;
                    factQ += itemsNew[bk].FactQuantity;

                }
            }
            this.label2.Text =
                string.Format(
                        "{0,13}|{1,10}|{2,2}|{3,2}",
                        " ",
                        "Итого:",
                        factQ.ToString(),
                        planQ.ToString());

            //listBox1.Items.Add(new EndListBoxItem(factQ, planQ).ToString());

            //treeView1.Nodes.Add(new EndListBoxItem(factQ, planQ).ToString());


            this.Width = 238;
            this.Height = 295;
            listBox1.Focus();

            this.Refresh();

        }
        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
             e.KeyValue == 115)//YellowBtn
            {
                e.Handled = true;
                this.Close();
                return;
            }
            if (e.KeyValue == 18)//RedBtn
            {
                if (rows != null && 
                    listBox1.SelectedIndex > 0 &&
                    listBox1.SelectedIndex < listBox1.Items.Count)
                {
                     OpenNETCF.Windows.Forms.ListItem li = 
                        listBox1.Items[listBox1.SelectedIndex];

                     ListBoxItem lbi = new ListBoxItem(li.Text);

                     FChangeQtyForm.MForm.Barcode = lbi.Barcode.ToString();
                     FChangeQtyForm.MForm.ProductName = lbi.ProductName.ToString();
                     FChangeQtyForm.MForm.FactQuantity = lbi.FactQuantity;
                     FChangeQtyForm.MForm.PlanQuantity = lbi.PlanQuantity;
                     DialogResult dr = FChangeQtyForm.MForm.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        int diff = FChangeQtyForm.MForm.NewQuantity -
                            FChangeQtyForm.MForm.FactQuantity;

                        //ScannedProductsDataSet.ScannedBarcodesRow scannedRow = null;
                        List<ScannedProductsDataSet.ScannedBarcodesRow > scannedList = 
                            new List<ScannedProductsDataSet.ScannedBarcodesRow>();
                        for (int i=0;i<rows.Length;i++)
                        {
                            if (rows[i].Barcode == lbi.Barcode)
                                scannedList.Add(rows[i]);
                            //break;
                        }
                        
                        

                        if (FChangeQtyForm.MForm.NewQuantity >= 0)
                        {
                            ProductsDataSet.ProductsTblRow r = products[lbi.Barcode];
                            ProductsDataSet.DocsTblRow docRow = null;
                            foreach (ProductsDataSet.DocsTblRow d in docsRows)
                            {
                                if (d.NavCode == r.NavCode)
                                {
                                    docRow = d;
                                    break;
                                }
                            }
                            if (diff < 0)
                            {
                                foreach (ScannedProductsDataSet.ScannedBarcodesRow s in scannedList)
                                {
                                    if (s.FactQuantity >= Math.Abs(diff))
                                    {
                                        ActionsClass.Action.ChangeQtyPosition(docRow, s, diff);
                                        //s.FactQuantity += diff;
                                        break;
                                    }
                                    else
                                    {
                                        int sgn = Math.Sign(diff);
                                        diff = (Math.Abs(diff) - s.FactQuantity) * sgn;
                                        ActionsClass.Action.ChangeQtyPosition(docRow, s, sgn * s.FactQuantity);

                                    }
                                }
                            }
                            else //diff >0
                            {
                                ScannedProductsDataSet.ScannedBarcodesRow s = null;
                                if (scannedList.Count > 0)
                                    s = scannedList[0];
                                else
                                    s = ActionsClass.Action.AddScannedRow(
                                            lbi.Barcode,
                                            _docType,
                                            _docId,
                                            diff,
                                            0);

                                ActionsClass.Action.ChangeQtyPosition(docRow, s, diff);


                                
                            }
                            //ActionsClass.Action.ChangeQtyPosition(scannedList, diff);
                            RefreshData();
                        }



                    }

                }

                
                e.Handled = true;
                return;
            }
        }

        
        public class ListBoxItem
        {
            public long Barcode;
            public string ProductName;
            public int FactQuantity;
            public int PlanQuantity;
            public Color ForeColor = Color.Black;

            public ListBoxItem(string listBoxItemString)
            {
                string[] lbi = listBoxItemString.Split('|');
                if (lbi.Length == 4)
                {
                    Barcode = long.Parse(lbi[0].Trim());
                    ProductName = lbi[1].Trim();
                    FactQuantity = int.Parse(lbi[2].Trim());
                    PlanQuantity = int.Parse(lbi[3].Trim());
                }
            }
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
                        "Фт",
                        "Пн");
            }
        }

        public class EndListBoxItem : ListBoxItem
        {

            public EndListBoxItem(int factQ, int planQ) :
                base(0, "Итого:", factQ, planQ)
            {

            }

        }
    
    }
}