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
        delegate void ActOnProduct(Int64 barcode);
        public static System.Threading.ManualResetEvent _mEvt =
             new System.Threading.ManualResetEvent(false);

        ProductsDataSet _products;
        ScannedProductsDataSet _scannedProducts;
        Color lastColor;
        ScanClass scaner = new ScanClass();
        
        delegate void PrepareConnectionDelegate();
        ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scanned_ta = null;
                //= new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
        ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter move_ta = null;
                //= new Familia.TSDClient.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();


        Dictionary<TSDUtils.ActionCode, ActOnProduct> actionsDict =
            new Dictionary<TSDUtils.ActionCode, ActOnProduct>();
        
        public ViewProductForm()
        {
            InitializeComponent();

            actionsDict.Add(TSDUtils.ActionCode.NoAction, new ActOnProduct(NoActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Reprice, new ActOnProduct(RepriceActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Returns, new ActOnProduct(ReturnActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Remove, new ActOnProduct(RemoveActionProc));
            //try
            //{
            //    BTPrintClass.PrintClass.Disconnect();
            //}
            //catch { }

            //BTPrintClass.PrintClass.BTPrinterInit();
            //BTPrintClass.PrintClass.SearchDevices();

            //BTPrintClass.PrintClass.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
        }

        private void AddScannedProduct(Int64 barcode, TSDUtils.ActionCode ac)
        {
            try
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
                scanned_ta.Connection.Open();
                scanned_ta.Update(row);
            }
            finally
            {
                if ((int)(scanned_ta.Connection.State & ConnectionState.Open) == 1)
                    scanned_ta.Connection.Close();
            }
        }
        void NoActionProc(Int64 barcode)
        {
            this.Refresh();
            System.Threading.Thread.Sleep(1000);
        }
        void RepriceActionProc(Int64 barcode)
        {
            this.Refresh();
            System.Threading.Thread.Sleep(1000);
        }
        void ReturnActionProc(Int64 barcode)
        {
            this.Refresh();
            System.Threading.Thread.Sleep(1000);
        }
        void RemoveActionProc(Int64 barcode)
        {
            this.Refresh();
            System.Threading.Thread.Sleep(1000);
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
                DoAction(GetProductRow(barcode));
            }
        }
        private void DoAction(ProductsDataSet.ProductsTblRow row)
        {
            _mEvt.WaitOne();

            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";
            label17.Text = "";
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
                label7.Text = row.NewPrice.ToString();
                label8.Text = row.OldPrice.ToString();
                label9.Text = row.Article;

                foreach (ProductsDataSet.DocsTblRow docRow in
                    _products.FindDocsByBarcode(row.Barcode))
                {
                    byte actionCodes = docRow.DocType;
                    TSDUtils.ActionCode ac = (TSDUtils.ActionCode)actionCodes;
                    if (actionsDict.ContainsKey(ac))
                    {
                        if (actionCodes == 0)
                        {
                            this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[TSDUtils.ActionCode.NoAction];
                            actionsDict[TSDUtils.ActionCode.NoAction].Invoke(row.Barcode);
                        }
                        else
                        {

                            this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[ac];
                            this.Refresh();

                            AddScannedProduct(row.Barcode, ac);
                            PlaySound(/*row, */docRow.MusicCode);
                            PrintLabelClass.Print.PrintLabel(row, docRow);
                            actionsDict[ac].Invoke(row.Barcode);

                        }
                    }
                }
                    
                
                
            }
            else
            {
                label5.Text = "...Товар не ";
                label6.Text = "   найден...";
                NativeClass.Play("ding.wav");
            }
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
            try
            {
                BTPrintClass.PrintClass.Disconnect();
                scaner.OnScanned = null;
                scaner.StopScan();
            }
            catch { };
        }

        private ProductsDataSet.ProductsTblRow GetProductRow(string barcode)
        {
            ProductsDataSet.ProductsTblRow row = _products.ProductsTbl.FindByBarcode(long.Parse(barcode));
            if (row == null)
            {
                using (ProductsDataSetTableAdapters.ProductsBinTblTableAdapter ta
                    = new Familia.TSDClient.ProductsDataSetTableAdapters.ProductsBinTblTableAdapter())
                {
                   // ta.Connection.Open();
                    ProductsDataSet.ProductsBinTblDataTable table =
                        ta.GetDataByBarcode(long.Parse(barcode));
                    if (table.Rows.Count > 0)
                    {
                        return ParseRow(table[0]); 
                    }
                    else
                        return null;
                }
            }
            else
                return row;

        }

        private ProductsDataSet.ProductsTblRow GetProductRowByNavCode(string navCode)
        {
            foreach (ProductsDataSet.ProductsTblRow row in this._products.ProductsTbl)
            {
                if (row.NavCode == navCode.Trim())
                {
                    return row;
                }
            }
            using (ProductsDataSetTableAdapters.ProductsBinTblTableAdapter ta
                = new Familia.TSDClient.ProductsDataSetTableAdapters.ProductsBinTblTableAdapter())
            {
                //ta.Connection.Open();

                ProductsDataSet.ProductsBinTblDataTable table =
                    ta.GetDataByNavcode(TSDUtils.CustomEncodingClass.Encoding.GetBytes(navCode));

                if (table.Rows.Count > 0)
                {
                    return ParseRow(table[0]); 
                }
                else
                    return null;
            }

        }


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

        }

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
            
            scanned_ta = 
                new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
            move_ta = 
                new Familia.TSDClient.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();

            try
            {

                scanned_ta.ClearBeforeFill = true;
                scanned_ta.Fill(this._scannedProducts.ScannedBarcodes);
                

                
                move_ta.ClearBeforeFill = true;
                move_ta.Fill(this._scannedProducts.MoveResultsTbl);

                scaner.InitScan();
                scaner.OnScanned += new Scanned(Scanned);

                scanned_ta.Connection.Close();
                move_ta.Connection.Close();
                //BTPrintClass.PrintClass.BTPrinterInit();
                //BTPrintClass.PrintClass.SearchDevices();

                BTPrintClass.PrintClass.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                
                
            }
            catch { }
            finally
            {

                _mEvt.Set();
            }
        }

        private void PlaySound(/*ProductsDataSet.ProductsTblRow row, */byte soundCode)
        {
            Calib.SystemLibNet.Api.SysPlayVibrator(
                Calib.SystemLibNet.Def.B_USERDEF, (int)soundCode , 500, 1000);

            Calib.SystemLibNet.Api.SysPlayBuzzer(Calib.SystemLibNet.Def.B_USERDEF,
                2 * ((int)soundCode) * 500, 1000);
            /*try
            {
                uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(ac, (uint)row.SoundCode);
                string soundFileName = System.IO.Path.Combine(Program.StartupPath, string.Format("Sound{0}", shablonCode));
                if (System.IO.File.Exists(soundFileName))
                {
                    NativeClass.Play(soundFileName);

                }
                else
                {
                    NativeClass.Play("ding.wav");
                }
            }
            catch
            {
            }*/

        }

    }
}