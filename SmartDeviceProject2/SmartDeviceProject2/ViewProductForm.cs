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

        Familia.TSDClient.ProductsDataSet _products;
        ScannedProductsDataSet _scannedProducts;
        Color lastColor;
        //ScanClass scaner = new ScanClass();

        delegate void PrepareConnectionDelegate();
        //ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scanned_ta = null;
                //= new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
        //ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter move_ta = null;
                //= new Familia.TSDClient.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();

        Familia.TSDClient.ProductsDataSetTableAdapters.ProductsTblTableAdapter productsTa;
        Familia.TSDClient.ProductsDataSetTableAdapters.DocsTblTableAdapter docsTa;
        Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA;

        Dictionary<byte, string> actionDict = new Dictionary<byte, string>();

        System.Threading.Timer tmr = null;

        public ViewProductForm()
        {
            InitializeComponent();

            tmr = new System.Threading.Timer(
             new System.Threading.TimerCallback(OnTimer)
             , null,
             System.Threading.Timeout.Infinite,
             System.Threading.Timeout.Infinite);
        }

        private void AddScannedProduct(Int64 barcode, TSDUtils.ActionCode ac)
        {
            /*try
            {
                ScannedProductsDataSet.ScannedBarcodesRow row = null;
                row = _scannedProducts.ScannedBarcodes.FindByBarcodeActionCodeScannedDate(
                    barcode, (byte)ac, DateTime.Today);
                if (row == null)
                {
                    row = _scannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                    row.Barcode = barcode;
                    row.ActionCode = (byte)ac;
                    row.ScannedDate = DateTime.Today;
                    row.ScannedQuantity = 1;
                    _scannedProducts.ScannedBarcodes.AddScannedBarcodesRow(row);
                }
                else
                {
                    row.ScannedQuantity = row.ScannedQuantity + 1;
                }
            }
            finally
            {

            }*/
        }

        public ViewProductForm(ProductsDataSet products, ScannedProductsDataSet scannedProducts):this()
        {
            _products = products;
            _scannedProducts = scannedProducts;
           
           // BeginConnection(new AsyncCallback(OnFinishConnect));
        }
        /*IAsyncResult BeginConnection(AsyncCallback requestCallback)
        {
            PrepareConnectionDelegate del =
                new PrepareConnectionDelegate(InitConnections);
            IAsyncResult ar = del.BeginInvoke(requestCallback,null);
            return ar;
        }
        private void InitConnections()
        {
            scaner.InitScan();
            scaner.OnScanned += new Scanned(Scanned);
            //_products.OpenConnection();
        }
        private void OnFinishConnect(IAsyncResult res)
        {

        }*/
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
                SearchBarcode(barcode);
            }
        }
        private void SearchBarcode(string barcode)
        {
            label17.Text = barcode;
            DoAction(GetProductRow(barcode));
        }
        private void DoAction(ProductsDataSet.ProductsTblRow row)
        {
            _mEvt.WaitOne();

            tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

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
                label9.Text = (row["Article"] == System.DBNull.Value ||
                    row["Article"] == null)?string.Empty:row.Article;
                
                ProductsDataSet.DocsTblRow[] docRows = docsTa.GetDataByBarcode(row.Barcode);
                actionDict.Clear();

                if (docRows != null)
                {

                    foreach (ProductsDataSet.DocsTblRow docRow in docRows)
                    {
                        ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                            _scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(
                            docRow.Barcode,
                            docRow.DocType,
                            docRow.DocId);
                        if (scannedRow == null)
                        {
                            scannedRow =
                                _scannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                            scannedRow.Barcode = docRow.Barcode;
                            scannedRow.DocId = docRow.DocId;
                            scannedRow.DocType = docRow.DocType;
                            scannedRow.PlanQuanity = docRow.Quantity;
                            scannedRow.Priority = docRow.Priority;
                            scannedRow.ScannedDate = DateTime.Today;
                            _scannedProducts.ScannedBarcodes.AddScannedBarcodesRow(scannedRow);
                        }

                            

                        byte actionCodes = docRow.DocType;
                        if (!actionDict.ContainsKey(actionCodes))
                            actionDict.Add(actionCodes,docRow.DocId);
                        /*
                        TSDUtils.ActionCode ac = (TSDUtils.ActionCode)actionCodes;

                        this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[ac];
                        label20.Text = (docRow["Quantity"] == System.DBNull.Value ||
                            docRow["Quantity"] == null) ? string.Empty : docRow.Quantity.ToString();

                        this.Refresh();

                        ActionsClass.Action.PlaySound(docRow.MusicCode);
                        ActionsClass.Action.PlayVibro(docRow.VibroCode);
                        ActionsClass.Action.PrintLabel(row, docRow);
                        ActionsClass.Action.InvokeAction(ac, row.Barcode);*/
                    }
                    foreach (byte acode in actionDict.Keys)
                    {
                        ScannedProductsDataSet.ScannedBarcodesRow r = _scannedProducts.ScannedBarcodes.UpdateQuantity(
                            row.Barcode, acode, 1);
                        if (r != null)
                        {
                            TSDUtils.ActionCode ac = (TSDUtils.ActionCode)acode;

                            this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[ac];

                            label20.Text = (r["PlanQuanity"] == System.DBNull.Value ||
                                r["PlanQuanity"] == null) ? string.Empty : r.PlanQuanity.ToString();

                            label21.Text = (r["FactQuantity"] == System.DBNull.Value ||
                                r["FactQuantity"] == null) ? string.Empty : r.FactQuantity.ToString();

                            this.Refresh();

                            ProductsDataSet.DocsTblRow docRow =
                                _products.DocsTbl.FindByBarcodeDocIdDocType(r.Barcode, r.DocId, r.DocType);

                            ActionsClass.Action.PlaySoundAsync(docRow.MusicCode);
                            ActionsClass.Action.PlayVibroAsync(docRow.VibroCode);
                            ActionsClass.Action.PrintLabelAsync(row, docRow);
                            ActionsClass.Action.InvokeAction(ac, row.Barcode);
                            this.Refresh();
                        }
                    }
                }
                else
                {
                    ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.NoAction, row.Barcode);
                }
                    
                
                
            }
            else
            {
                label5.Text = "...Товар не ";
                label6.Text = "   найден...";
                NativeClass.Play("ding.wav");
            }
            navCodeTB.Text = "";
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

            int w = this.Width / 4;
            this.label13.Width = w;
            this.label14.Width = w;
            this.label15.Width = w;
            this.navCodeTB.Focus();

            System.Threading.Thread t = new System.Threading.Thread(
                new System.Threading.ThreadStart(Init));
            t.Start();

            BTPrintClass.PrintClass.OnSetStatus += new SetStatus(PrintClass_OnSetStatus);
            BTPrintClass.PrintClass.OnSetError += new SetError(PrintClass_OnSetError);

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
            System.Drawing.Font f1 =
                new Font(f.Name, f.Size, FontStyle.Bold);
            //f.Style = FontStyle.Bold;
            this.actionLabel.Font = f1;

            this.Refresh();

        }

        void PrintClass_OnSetError(string text)
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

        void PrintClass_OnSetStatus(string text)
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
            tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            try
            {
                
                BTPrintClass.PrintClass.OnSetStatus -= (PrintClass_OnSetStatus);
                BTPrintClass.PrintClass.OnSetError -= (PrintClass_OnSetError);
                BTPrintClass.PrintClass.Disconnect();

                scannedTA.Update(this._scannedProducts);
            }
            catch { };
            productsTa.Dispose();
            docsTa.Dispose();
            productsTa = null;
            docsTa = null;
            ScanClass.Scaner.OnScanned = null;
            ScanClass.Scaner.StopScan();
        }

        private ProductsDataSet.ProductsTblRow GetProductRow(string barcode)
        {
            ProductsDataSet.ProductsTblRow row = productsTa.GetDataByBarcode(long.Parse(barcode));
            return row;

        }

        private ProductsDataSet.ProductsTblRow GetProductRowByNavCode(string navCode)
        {

            ProductsDataSet.ProductsTblRow[] Rows = productsTa.GetDataByNavcode(navCode);
                    //ta.GetDataByNavcode(TSDUtils.CustomEncodingClass.Encoding.GetBytes(navCode));

                if (Rows != null && Rows.Length > 0)
                {
                    return Rows[0]; 
                }
                else
                    return null;

        }

        /*
        private ProductsDataSet.ProductsTblRow ParseRow(ProductsDataSet.ProductsBinTblRow binRow)
        {
            ProductsDataSet.ProductsTblRow row = 
                _products.ProductsTbl.NewProductsTblRow();
            for (int i = 0; i < _products.ProductsTbl.Columns.Count; i++)
            {
                if (binRow[i] != null &&
                    binRow[i] != System.DBNull.Value)
                {
                    if (_products.ProductsBinTbl.Columns[i].DataType == typeof(byte[]))
                    {
                        row[i] = TSDUtils.CustomEncodingClass.Encoding.GetString((byte[])binRow[i]); //System.Text.Encoding.Unicode.GetString(buffer, 0, buffer.Length);

                    }
                    else
                    {
                        row[i] = binRow[i];
                    }
                }

            }
            _products.ProductsTbl.AddProductsTblRow(row);
            _products.AcceptChanges();

            //product founded - fill all documents
            using (ProductsDataSetTableAdapters.DocsBinTblTableAdapter docta
                = new Familia.TSDClient.ProductsDataSetTableAdapters.DocsBinTblTableAdapter())
            {
                docta.FillByBarcode(_products.DocsBinTbl, row.Barcode);
            }
            foreach (ProductsDataSet.DocsBinTblRow docRow in _products.DocsBinTbl)
            {
                _products.DocsTbl.AddDocsTblRow(_products.ConvertFromBin(docRow));
            }
            _products.DocsTbl.AcceptChanges();
            
            return row;

        }*/

        private void navCodeTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DoAction(GetProductRowByNavCode(navCodeTB.Text));
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
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
                productsTa = new ProductsDataSetTableAdapters.ProductsTblTableAdapter(this._products);
                docsTa = new ProductsDataSetTableAdapters.DocsTblTableAdapter(this._products);
                scannedTA =
                    new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(_scannedProducts);
                //scannedTA.Update
                ScanClass.Scaner.InitScan();
                ScanClass.Scaner.OnScanned += new Scanned(Scanned);

                //scanned_ta.Connection.Close();
                //move_ta.Connection.Close();
                //BTPrintClass.PrintClass.BTPrinterInit();
                //BTPrintClass.PrintClass.SearchDevices();

                BTPrintClass.PrintClass.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);


            }
            catch (Exception err)
            { BTPrintClass.PrintClass.SetErrorEvent(err.ToString()); }
            finally
            {

                _mEvt.Set();
                tmr.Change(1000, 60000);
            }
        }

        private void OnTimer(object state)
        {
            if (this.InvokeRequired)
            {
                System.Threading.TimerCallback del =
                    new System.Threading.TimerCallback(OnTimer);
                this.Invoke(del, state);
            }
            else
            {
                try
                {
                    _mEvt.Reset();
                    scannedTA.Update(this._scannedProducts);
                }
                finally
                {
                    _mEvt.Set();
                }
            //    if (currentItem < waitStr.Length - 1)
            //    {
            //        currentItem += 1;
            //    }
            //    else
            //        currentItem = 0;
            //    label5.Text = string.Format("Поиск {0}", waitStr[currentItem]);
            //    this.Refresh();
            //    Application.DoEvents();
            }

                
        }
        
    }
}