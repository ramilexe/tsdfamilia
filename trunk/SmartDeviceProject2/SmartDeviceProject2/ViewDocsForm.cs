using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class ViewDocsForm : Form
    {
        ProductsDataSet.ProductsTblRow _productRow;
        ProductsDataSet.DocsTblRow[] _docsRows;
        List<DocumentClass> documents =
            new List<DocumentClass>();
        List<Label> labels = new List<Label>();
        int selectedItem = 0;

        public ViewDocsForm()
        {
            InitializeComponent();
        }

        public ViewDocsForm(ProductsDataSet.ProductsTblRow productRow,ProductsDataSet.DocsTblRow[] docsRows):this()
        {
            _productRow = productRow;
            _docsRows = docsRows;
            //this.listBox1.KeyDown += new KeyEventHandler(listBox1_KeyDown);
            this.Load += new EventHandler(ViewDocsForm_Load);
            this.KeyDown += new KeyEventHandler(ViewDocsForm_KeyDown);
            this.Paint += new PaintEventHandler(ViewDocsForm_Paint);
            if (_productRow != null
                && _docsRows != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(System.Globalization.CultureInfo.CurrentCulture,
                    "Код: {0} \nШтрихкод: {1} \nНазвание: {2}", 
                    _productRow.NavCode, 
                    _productRow.Barcode, 
                    _productRow.ProductName);
                this.label1.Text = sb.ToString();

                foreach (ProductsDataSet.DocsTblRow doc in _docsRows)
                {

                    DocumentClass d = new DocumentClass();
                    d.DocDate = doc.DocumentDate;
                    d.DocId = doc.DocId;
                    d.DocType = doc.DocType;
                        
                    d.PlanQuantity = doc.Quantity;

                    
                    Label l = new Label();
                    l.Size = new System.Drawing.Size(231, 60);
                    l.Name = string.Format("label{0}", documents.Count);
                    l.Left = 0;
                    l.Top = documents.Count * 60;
                    l.Text = d.ToString();
                    l.BackColor = System.Drawing.Color.PaleGreen;
                    documents.Add(d);
                    this.panel2.Controls.Add(l);
                    
                }

            }
        }

        void ViewDocsForm_Paint(object sender, PaintEventArgs e)
        {
            foreach (Control c in this.panel2.Controls)
            {
                if (c.Name == string.Format("label{0}", selectedItem))
                    c.BackColor = System.Drawing.Color.Plum;
                else
                    c.BackColor = System.Drawing.Color.PaleGreen;
            }
        }

        void ViewDocsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            if (e.KeyValue == 40)
            {
                selectedItem++;
                if (selectedItem >= documents.Count-1)
                    selectedItem = 0;
            }
            if (e.KeyValue == 38)
            {
                selectedItem++;
                if (selectedItem < 0)
                    selectedItem = documents.Count-1;
            }
        }

        void ViewDocsForm_Load(object sender, EventArgs e)
        {
            //this.label1.Size = new System.Drawing.Size(231, 93);
            
            
        }

        void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
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

        public override string ToString()
        {
            return String.Format("Документ {0} №{1} \n от {2} \n План:{3}, Факт: {4}",
                TSDUtils.ActionCodeDescription.ActionDescription[DocType],
                DocId,
                DocDate.ToShortDateString(),
                PlanQuantity,
                FactQuantity);
        }
    }
}