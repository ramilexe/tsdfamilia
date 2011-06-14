using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ViewDocsForm : Form
    {
        ProductsDataSet.ProductsTblRow _productRow;
        ProductsDataSet.DocsTblRow[] _docsRows;
        ScannedProductsDataSet _scannedProducts;
        List<DocumentClass> documents =
            new List<DocumentClass>();
        List<Label> labels = new List<Label>();
        int selectedItem = 0;

        public ViewDocsForm()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(ViewDocsForm_KeyDown);

        }

        //public ViewDocsForm(ProductsDataSet.ProductsTblRow productRow):this()
        //{
        //    this.panel2.SuspendLayout();
        //    this.SuspendLayout();
        //    _productRow = productRow;
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat(System.Globalization.CultureInfo.CurrentCulture,
        //        "Код: {0} \nШтрихкод: {1} \nНазвание: {2}",
        //        _productRow.NavCode,
        //        _productRow.Barcode,
        //        _productRow.ProductName);
        //    this.label.Text = sb.ToString();

        //    Label l = new Label();
        //    l.Size = new System.Drawing.Size(231, 90);
        //    l.Name = string.Format("label{0}", 0);
        //    l.Left = 0;
        //    l.Top = 0;
        //    l.Text = "По данному товару \nнет никаких документов, \nнажмите желтую кнопку \nеще раз для выхода";
        //    l.BackColor = System.Drawing.Color.PaleGreen;
            
        //    this.panel2.Controls.Add(l);
        //    this.panel1.ResumeLayout(false);
        //    this.ResumeLayout();
        //}
        public ViewDocsForm(ProductsDataSet.ProductsTblRow productRow,
            ProductsDataSet.DocsTblRow[] docsRows,
            ScannedProductsDataSet scannedProducts):this()
        {
            this.panel2.SuspendLayout();
            this.SuspendLayout();

            _productRow = productRow;
            _scannedProducts = scannedProducts;
            _docsRows = docsRows;
            //this.listBox1.KeyDown += new KeyEventHandler(listBox1_KeyDown);
            this.Load += new EventHandler(ViewDocsForm_Load);
            int docCounter = 0;
            //this.Paint += new PaintEventHandler(ViewDocsForm_Paint);
            if (_productRow != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(System.Globalization.CultureInfo.CurrentCulture,
                    "Код: {0} \nШтрихкод: {1} \nНазвание: {2}",
                    _productRow.NavCode,
                    _productRow.Barcode,
                    _productRow.ProductName);
                this.label.Text = sb.ToString();

                if (_docsRows != null && _docsRows.Length > 0)
                {
                    foreach (ProductsDataSet.DocsTblRow doc in _docsRows)
                    {

                        ScannedProductsDataSet.ScannedBarcodesRow srow =
                        _scannedProducts
                                .ScannedBarcodes
                                .FindByBarcodeDocTypeDocId(_productRow.Barcode,
                                                            doc.DocType,
                                                            doc.DocId);

                        DocumentClass d = new DocumentClass();
                        d.DocDate = doc.DocumentDate;
                        d.DocId = doc.DocId;
                        d.DocType = doc.DocType;
                        d.Text1 = doc.Text1;
                        d.Text2 = doc.Text2;
                        d.Text3 = doc.Text3;

                        d.PlanQuantity = doc.Quantity;
                        if (srow != null &&
                            srow["FactQuantity"] != System.DBNull.Value)
                            d.FactQuantity = srow.FactQuantity;
                        else
                            d.FactQuantity = 0;



                        Label l = new Label();
                        l.Size = new System.Drawing.Size(231, 60);
                        l.Name = string.Format("label{0}", documents.Count);
                        l.Left = 0;
                        l.Top = documents.Count * 60;
                        l.Text = d.ToString();
                        l.BackColor = System.Drawing.Color.PaleGreen;
                        l.TextAlign = ContentAlignment.TopCenter;
                        documents.Add(d);
                        this.panel2.Controls.Add(l);
                        selectedItem = 0;
                        docCounter++;

                    }
                }
               else
                {
                    Label l = new Label();
                    l.Size = new System.Drawing.Size(231, 60);
                    l.Name = string.Format("label{0}", 0);
                    l.Left = 0;
                    l.Top = 0;
                    l.Text = "По данному товару \nнет никаких документов";
                    l.BackColor = System.Drawing.Color.PaleGreen;
                    l.TextAlign = ContentAlignment.TopCenter;
                    this.panel2.Controls.Add(l);
                    docCounter++;
                }
                Label l1 = new Label();
                l1.Size = new System.Drawing.Size(231, 30);
                l1.Name = string.Format("label{0}", docCounter + 1);
                l1.Left = 0;
                l1.Top = (docCounter * 60);
                l1.Text = "Нажмите желтую кнопку или Clr-Fn\nеще раз для выхода";
                l1.BackColor = System.Drawing.Color.PaleGreen;
                l1.TextAlign = ContentAlignment.TopCenter;
                this.panel2.Controls.Add(l1);

            }

            this.panel1.ResumeLayout(false);
            this.ResumeLayout();
        }

        //void ViewDocsForm_Paint(object sender, PaintEventArgs e)
        //{
        //    UpdatePanel();
        //}

        void ViewDocsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyValue == 115)
            {
                this.Close();
                return;
            }
            if (e.KeyValue == 40)
            {
                selectedItem++;
                if (selectedItem >= documents.Count)
                    selectedItem = 0;
                UpdatePanel();
                return;
            }
            if (e.KeyValue == 38)
            {
                selectedItem--;
                if (selectedItem < 0)
                    selectedItem = documents.Count-1;
                
                UpdatePanel();
                return;
            }
        }

        private void UpdatePanel()
        {
            foreach (Control c in this.panel2.Controls)
            {
                if (c.Name == string.Format("label{0}", selectedItem))
                    c.BackColor = System.Drawing.Color.Plum;
                else
                    c.BackColor = System.Drawing.Color.PaleGreen;
            }
            this.panel2.Refresh();
        }

        void ViewDocsForm_Load(object sender, EventArgs e)
        {

            UpdatePanel();
        }
      
    }


    public class DocumentClass
    {
        /*string _productName;

        public string ProductName
        {
            get { return _productName; }
            set { _productName = value; }
        }
        string _barcode;

        public string Barcode
        {
            get { return _barcode; }
            set { _barcode = value; }
        }
        string _navCode;

        public string NavCode
        {
            get { return _navCode; }
            set { _navCode = value; }
        }*/
        string _docId;

        public string DocId
        {
            get { return _docId; }
            set { _docId = value; }
        }
        DateTime _docDate;

        public DateTime DocDate
        {
            get { return _docDate; }
            set { _docDate = value; }
        }
        byte _docType;

        public byte DocType
        {
            get { return _docType; }
            set { _docType = value; }
        }

        int _planQuantity;

        public int PlanQuantity
        {
            get { return _planQuantity; }
            set { _planQuantity = value; }
        }
        int _factQuantity;

        public int FactQuantity
        {
            get { return _factQuantity; }
            set { _factQuantity = value; }
        }
        string _text1;

        public string Text1
        {
            get { return _text1; }
            set { _text1 = value; }
        }
        string _text2;

        public string Text2
        {
            get { return _text2; }
            set { _text2 = value; }
        }
        string _text3;

        public string Text3
        {
            get { return _text3; }
            set { _text3 = value; }
        }

        public override string ToString()
        {
            return String.Format("Документ {0} №{1} \n от {2} \n{3} {4} {5}\n План:{6}, Факт: {7}",
                TSDUtils.ActionCodeDescription.ActionDescription[DocType],
                DocId,
                DocDate.ToString("dd.MM.yyyy"),
                _text1,
                _text2,
                _text3,
                PlanQuantity,
                FactQuantity);
        }
    }
}