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
        string[] waitStr = new string[] { "\\", "|", "/", "-" };
        int currentItem = 0;

        public static System.Threading.ManualResetEvent _mEvt =
             new System.Threading.ManualResetEvent(false);

        //Familia.TSDClient.ProductsDataSet _products;
        //ScannedProductsDataSet _scannedProducts;
        Color lastColor;
        Font boldFont;
        Font normalFont;
        //ScanClass scaner = new ScanClass();
        ProductsDataSet.ProductsTblRow currentProductRow = null;
        ProductsDataSet.DocsTblRow currentDocRow = null;
        ProductsDataSet.DocsTblRow[] currentdocRows = null;
        

        delegate void PrepareConnectionDelegate();
        //ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scanned_ta = null;
                //= new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
        //ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter move_ta = null;
                //= new Familia.TSDClient.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();

        //Familia.TSDClient.ProductsDataSetTableAdapters.ProductsTblTableAdapter productsTa;
        //Familia.TSDClient.ProductsDataSetTableAdapters.DocsTblTableAdapter docsTa;
        //Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA;

        Dictionary<byte, ProductsDataSet.DocsTblRow> actionDict = new Dictionary<byte, ProductsDataSet.DocsTblRow>();

        //System.Threading.Timer tmr = null;

        public ViewProductForm()
        {
            InitializeComponent();

            //tmr = new System.Threading.Timer(
            // new System.Threading.TimerCallback(OnTimer)
            // , null,
            // System.Threading.Timeout.Infinite,
            // System.Threading.Timeout.Infinite);
        }


        //public ViewProductForm(ProductsDataSet products, ScannedProductsDataSet scannedProducts):this()
        //{
        //    _products = products;
        //    _scannedProducts = scannedProducts;
           
        //   // BeginConnection(new AsyncCallback(OnFinishConnect));
        //}

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
                //tmr.Change(0, 200);
                //if (docsForm != null)
                //{
                //    docsForm.Close();
                //    docsForm.Dispose();
                //    docsForm = null;
                //}
                SearchBarcode(barcode);
            }
        }
        private void SearchBarcode(string barcode)
        {
            label17.Text = barcode;
            DoAction(ActionsClass.Action.GetProductRow(barcode));
        }
        private void DoAction(ProductsDataSet.ProductsTblRow row)
        {
            if (_mEvt.WaitOne(5000,false) == false)
            {
                this.Close();
            }

            //tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";
            label20.Text = "";
            label21.Text = "";
            //label17.Text = "";
            actionLabel.Text = "";
            navCodeTB.Text = "";
            currentProductRow = row;

            if (row != null)
            {
                label17.Text = row.Barcode.ToString("0000000000000");
                if (row.ProductName.Length > 15)
                {
                    label5.Text = row.ProductName.Substring(0, 15);
                    label6.Text = row.ProductName.Substring(15);
                }
                else
                    label5.Text = row.ProductName;
                navCodeTB.Text = row.NavCode;
                
                label7.Text = (row["NewPrice"] == System.DBNull.Value ||
                    row["NewPrice"] == null) ? string.Empty : row.NewPrice.ToString("### ###.00");
                label8.Text = (row["OldPrice"] == System.DBNull.Value ||
                    row["OldPrice"] == null) ? string.Empty : row.OldPrice.ToString("### ###.00");
                if (label8.Text != label7.Text)
                    label7.Font = boldFont;
                else
                    label7.Font = normalFont;
                
                
                label9.Text = (row["Article"] == System.DBNull.Value ||
                    row["Article"] == null)?string.Empty:row.Article;
                
                ProductsDataSet.DocsTblRow[] docRows = 
                    ActionsClass.Action.GetDataByNavCode(row.NavCode);
                actionDict.Clear();
                currentdocRows = docRows;

                if (docRows != null)
                {

                    foreach (ProductsDataSet.DocsTblRow docRow in docRows)
                    {
                        ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                            ActionsClass.Action.AddScannedRow(
                            row.Barcode,
                            docRow.DocType,
                            docRow.DocId,
                            docRow.Quantity,
                            docRow.Priority);
                            /*_scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(
                            row.Barcode,
                            docRow.DocType,
                            docRow.DocId);
                        if (scannedRow == null)
                        {
                            scannedRow =
                                _scannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                            scannedRow.Barcode = row.Barcode;
                            scannedRow.DocId = docRow.DocId;
                            scannedRow.DocType = docRow.DocType;
                            scannedRow.PlanQuanity = docRow.Quantity;
                            scannedRow.Priority = docRow.Priority;
                            scannedRow.ScannedDate = DateTime.Today;
                            scannedRow.TerminalId = Program.TerminalId;
                            _scannedProducts.ScannedBarcodes.AddScannedBarcodesRow(scannedRow);
                        }*/
                        byte actionCodes = docRow.DocType;
                        if (!actionDict.ContainsKey(actionCodes))
                            actionDict.Add(actionCodes,docRow);
                    }
                    foreach (byte acode in actionDict.Keys)
                    {
                        ActionsClass.Action.InvokeAction((TSDUtils.ActionCode)acode, row, actionDict[acode]);
                        this.Refresh();
                    }
                }
                else
                {
                    ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.DocNotFound, row, null);
                }
                    
                
                
            }
            else
            {
                currentdocRows = null;
                currentProductRow = null;
                ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.NotFound,null,null );
                label5.Text = "...Товар не ";
                label6.Text = "   найден...";
                label17.Text = "";
                //NativeClass.Play("ding.wav");
            }
            navCodeTB.SelectAll();
        }
        private void ViewProductForm_Load(object sender, EventArgs e)
        {
           
            /*
            foreach (Control c in this.Controls)
            {
                c.GotFocus += new EventHandler(c_GotFocus);
                c.LostFocus += new EventHandler(c_LostFocus);
            }*/
            /*this.textBox1.Focus();*/
            System.Threading.Thread t = new System.Threading.Thread(
                new System.Threading.ThreadStart(Init));
            t.Start();
            this.SuspendLayout();
            int w = this.Width / 4;
            this.label13.Width = w;
            this.label14.Width = w;
            this.label15.Width = w;
            this.navCodeTB.Focus();

           

            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";
            label17.Text = "";
            label20.Text = "";
            label21.Text = "";
            actionLabel.Text = "";
            navCodeTB.Text = "";
            Font f = this.actionLabel.Font;
            normalFont = f;
            System.Drawing.Font f1 =
                new Font(f.Name, f.Size, FontStyle.Bold);
            boldFont = f1;
            
            this.actionLabel.Font = boldFont;
            this.ResumeLayout(true);

            ActionsClass.Action.OnActionCompleted += new ActionsClass.ActionCompleted(Action_OnActionCompleted);
            ScanClass.Scaner.InitScan();
            ScanClass.Scaner.OnScanned += new Scanned(Scanned);
            BTPrintClass.PrintClass.OnSetStatus += new SetStatus(PrintClass_OnSetStatus);
            BTPrintClass.PrintClass.OnSetError += new SetError(PrintClass_OnSetError);
            BTPrintClass.PrintClass.OnConnectionError += new ConnectionError(PrintClass_OnConnectionError);
            
            this.Refresh();

        }

        void PrintClass_OnConnectionError()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    TSDClient.ConnectionError del = new ConnectionError(PrintClass_OnConnectionError);
                    this.Invoke(del);
                }
                else
                {
                    using (BTConnectionErrorForm frm =
                        new BTConnectionErrorForm())
                    {
                        if (frm.ShowDialog() == DialogResult.Yes)
                        {
                            BTPrintClass.PrintClass.Reconnect();
                        }

                    }
                }
            }
            catch (ObjectDisposedException)
            { }
        }

        void PrintClass_OnSetError(string text)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    TSDClient.SetError del = new SetError(PrintClass_OnSetError);
                    this.Invoke(del, text);
                }
                else
                {
                    label18.ForeColor = Color.Red;
                    label18.Text = text;
                }
            }
            catch (ObjectDisposedException)
            { }
        }

        void PrintClass_OnSetStatus(string text)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    TSDClient.SetStatus del = new SetStatus(PrintClass_OnSetStatus);
                    this.Invoke(del, text);
                }
                else
                {
                    label18.ForeColor = Color.Black;
                    label18.Text = text;
                }
            }
            catch (ObjectDisposedException)
            { }
        }
        /*
        void c_LostFocus(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = lastColor;
        }

        void c_GotFocus(object sender, EventArgs e)
        {
            //lastColor = this.textBox1.BackColor;
            ((Control)sender).BackColor = Color.Plum;
        }
        */
        private void ViewProductForm_Closing(object sender, CancelEventArgs e)
        {
            //tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            ActionsClass.Action.EndScan();
            try
            {
                
                BTPrintClass.PrintClass.OnSetStatus -= (PrintClass_OnSetStatus);
                BTPrintClass.PrintClass.OnSetError -= (PrintClass_OnSetError);
                BTPrintClass.PrintClass.Disconnect();

                //scannedTA.Update(this._scannedProducts);
            }
            catch { };
            //productsTa.Dispose();
            //docsTa.Dispose();
            //productsTa = null;
            //docsTa = null;
            ScanClass.Scaner.OnScanned = null;
            ScanClass.Scaner.StopScan();
            ActionsClass.Action.OnActionCompleted -=Action_OnActionCompleted;


        }

        /*private ProductsDataSet.ProductsTblRow GetProductRow(string barcode)
        {
            ProductsDataSet.ProductsTblRow row = productsTa.GetDataByBarcode(long.Parse(barcode));
            return row;

        }

        private ProductsDataSet.ProductsTblRow GetProductRowByNavCode(string navCode)
        {

            ProductsDataSet.ProductsTblRow[] Rows = ActionsClass.Action.GetDataByNavcode(navCode);
                    //ta.GetDataByNavcode(TSDUtils.CustomEncodingClass.Encoding.GetBytes(navCode));

                if (Rows != null && Rows.Length > 0)
                {
                    return Rows[0]; 
                }
                else
                    return null;

        }*/


        private void navCodeTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DoAction(ActionsClass.Action.GetProductRowByNavCode(navCodeTB.Text));
                navCodeTB.SelectAll();
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                return;
            }
            if (e.KeyValue == 18)
            {
                if (currentProductRow != null)
                {
                    ActionsClass.Action.PrintLabel(currentProductRow, currentDocRow, Program.Default.DefaultRepriceShablon);
                }
                return;
            }
            if (e.KeyValue == 16)
            {
                if (currentProductRow != null)
                {
                    ActionsClass.Action.PrintLabel(currentProductRow, currentDocRow, Program.Default.BlueButtonShablon);
                }
                return;
            }
            if (e.KeyValue == 115)
            {
                if (currentProductRow != null)
                {
                    try
                    {

                        ScanClass.Scaner.PauseScan();
                        //if (currentdocRows != null && currentdocRows.Length > 0)
                        //{
                            using (
                                ViewDocsForm docsForm =
                                    new ViewDocsForm(currentProductRow, currentdocRows, ActionsClass.Action.ScannedProducts))
                            {
                                docsForm.ShowDialog();
                            }
                        //}
                        //else
                        //{
                        //    using (
                        //        ViewDocsForm docsForm =
                        //            new ViewDocsForm(currentProductRow))
                        //    {
                        //        docsForm.ShowDialog();
                        //    }
                        //}


                    }
                    catch { }
                    finally
                    {
                        ScanClass.Scaner.ResumeScan();

                        //this.BringToFront();
                        //this.Focus();
                    }
                }
                return;
            }
        }

        private void Init()
        {
           // _mutex.WaitOne();
            /*
            scanned_ta = 
                new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
            move_ta = 
                new Familia.TSDClient.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();
            */
            try
            {

                //scanned_ta.ClearBeforeFill = true;
                //scanned_ta.Fill(this._scannedProducts.ScannedBarcodes);
                

                
                //move_ta.ClearBeforeFill = true;
                //move_ta.Fill(this._scannedProducts.MoveResultsTbl);
                //productsTa = new ProductsDataSetTableAdapters.ProductsTblTableAdapter(this._products);
                //docsTa = new ProductsDataSetTableAdapters.DocsTblTableAdapter(this._products);
                //scannedTA.Update
                

                //scanned_ta.Connection.Close();
                //move_ta.Connection.Close();
                //BTPrintClass.PrintClass.BTPrinterInit();
                //BTPrintClass.PrintClass.SearchDevices();

                BTPrintClass.PrintClass.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                ActionsClass.Action.BeginScan();
                
            }
            catch (Exception err)
            { BTPrintClass.PrintClass.SetErrorEvent(err.ToString()); }
            finally
            {

                _mEvt.Set();
                //tmr.Change(1000, 60000);
            }
        }

        void  Action_OnActionCompleted(ProductsDataSet.DocsTblRow docsRow, ScannedProductsDataSet.ScannedBarcodesRow scannedRow)
        {
            if (this.InvokeRequired)
            {
                ActionsClass.ActionCompleted del = 
                    new ActionsClass.ActionCompleted(Action_OnActionCompleted);
                this.Invoke(del,
                    docsRow,
                    scannedRow);
            }
            else
            {
                currentDocRow = docsRow;
                this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[docsRow.DocType];

                label20.Text = (scannedRow["PlanQuanity"] == System.DBNull.Value ||
                                scannedRow["PlanQuanity"] == null) ? string.Empty : scannedRow.PlanQuanity.ToString();

                label21.Text = (scannedRow["FactQuantity"] == System.DBNull.Value ||
                                scannedRow["FactQuantity"] == null) ? string.Empty : scannedRow.FactQuantity.ToString();

                this.Refresh();
            }
        }

        
    }
}