using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ViewProductForm : Form
    {
        WorkMode _mode;
        string _documentId;
        string[] waitStr = new string[] { "\\", "|", "/", "-" };
        int currentItem = 0;
        int quantityKoeff;
        ProductsDataSet.DocsTblRow inventRow;

        public static System.Threading.ManualResetEvent _mEvt =
             new System.Threading.ManualResetEvent(false);

        //TSDServer.ProductsDataSet _products;
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
                //= new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
        //ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter move_ta = null;
                //= new TSDServer.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();

        //TSDServer.ProductsDataSetTableAdapters.ProductsTblTableAdapter productsTa;
        //TSDServer.ProductsDataSetTableAdapters.DocsTblTableAdapter docsTa;
        //TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA;

        Dictionary<byte, ProductsDataSet.DocsTblRow> actionDict = new Dictionary<byte, ProductsDataSet.DocsTblRow>();

        //System.Threading.Timer tmr = null;

        public ViewProductForm()
        {
            quantityKoeff =1;
            
            _mode = WorkMode.ProductsScan;
            BTPrintClass.PrintClass.SetStatusEvent("Open Products form");
            InitializeComponent();

            quantityLabel.Visible = false;
            quantityLabel.Text = string.Empty;

            //tmr = new System.Threading.Timer(
            // new System.Threading.TimerCallback(OnTimer)
            // , null,
            // System.Threading.Timeout.Infinite,
            // System.Threading.Timeout.Infinite);
        }

        public ViewProductForm(WorkMode mode, string docId):this()
        {
            _mode = mode;
            _documentId = docId;
            switch (_mode)
            {
                case WorkMode.InventarScan:
                    {
                        this.label15.Text = "F3-Инв.Закрыть";
                        Program.СurrentInvId = docId;

                        inventRow = ActionsClass.Action.Products.DocsTbl.NewDocsTblRow();
                        inventRow.DocId = _documentId;
                        inventRow.DocType = (byte)(TSDUtils.ActionCode.InventoryGlobal);
                        inventRow.DocumentDate = DateTime.Today;
                        inventRow.LabelCode = (byte)TSDUtils.ShablonCode.NoShablon;
                        inventRow.MusicCode = 5;
                        //            docRows.NavCode = row.NavCode;
                        inventRow.Priority = 0;
                        inventRow.Quantity = 0;
                        inventRow.VibroCode = 5;
                        inventRow.Text1 = "";
                        inventRow.Text2 = "";
                        inventRow.Text3 = "";
                        label14.Text = "";
                        break;
                    }
                case WorkMode.BoxScan:
                    {
                        this.label15.Text = "";
                        if (Program.Default.EnableChgMlt == 1)
                            this.label14.Text = "F2-Колво";
                        else
                            this.label14.Text = "";

                        inventRow = ActionsClass.Action.Products.DocsTbl.NewDocsTblRow();
                        inventRow.DocId = _documentId;
                        inventRow.DocType = (byte)(TSDUtils.ActionCode.BoxWProducts);
                        inventRow.DocumentDate = DateTime.Today;
                        inventRow.LabelCode = (byte)TSDUtils.ShablonCode.NoShablon;
                        inventRow.MusicCode = 5;
                        //            docRows.NavCode = row.NavCode;
                        inventRow.Priority = 0;
                        inventRow.Quantity = 0;
                        inventRow.VibroCode = 5;
                        inventRow.Text1 = "";
                        inventRow.Text2 = "";
                        inventRow.Text3 = "";
                        break;

                    }
                default:
                    {
                        break;
                    }
            }



            currentdocRows = new ProductsDataSet.DocsTblRow[1];
            currentdocRows[0] = inventRow;
            
            label13.Text = "";
            this.label13.Text = "Уменьш.КОЛВО";


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
            //ActionsClass.Action.ClearCache();
            this.Close();
        }
        void Scanned(string barcode)
        {
            if (this.InvokeRequired)
            {
                TSDServer.Scanned del = new Scanned(Scanned);
                this.Invoke(del, barcode);
            }
            else
            {
                this.label18.Text = "";
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
            try
            {

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
                        row["NewPrice"] == null) ? string.Empty : row.NewPrice.ToString("######.00");
                    label8.Text = (row["OldPrice"] == System.DBNull.Value ||
                        row["OldPrice"] == null) ? string.Empty : row.OldPrice.ToString("######.00");
                    if (label8.Text != label7.Text)
                        label7.Font = boldFont;
                    else
                        label7.Font = normalFont;


                    label9.Text = (row["Article"] == System.DBNull.Value ||
                        row["Article"] == null) ? string.Empty : row.Article;

                    if (_mode == WorkMode.ProductsScan)
                    {
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
                                    actionDict.Add(actionCodes, docRow);
                            }
                            foreach (byte acode in actionDict.Keys)
                            {
                                ActionsClass.Action.InvokeAction((TSDUtils.ActionCode)acode, row, actionDict[acode]);
                                //this.Refresh();
                            }
                        }
                        else
                        {
                            ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.DocNotFound, row, null);
                        }
                    }
                    else
                        if (_mode == WorkMode.InventarScan)
                        {

                            inventRow.NavCode = row.NavCode;
                            ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.InventoryGlobal,
                                row,
                                inventRow
                                );

                            //this.Refresh();
                            //ActionsClass.Action.InventoryGlobalActionProc(
                            //    row,
                            //    inventRow);


                        }
                        else
                        {//BoxScan
                            //_mode == WorkMode.BoxScan

                            ProductsDataSet.DocsTblRow docRow =
                                ActionsClass.Action.GetDataByNavcodeDocIdAndType(row.NavCode,
                                    inventRow.DocId,
                                    (byte)TSDUtils.ActionCode.BoxWProducts
                                );
                            if (docRow != null)
                            {
                                ScannedProductsDataSet.ScannedBarcodesRow srows
                                    = ActionsClass.Action.FindByBarcodeDocTypeDocId(row.Barcode.ToString(),
                                    (byte)TSDUtils.ActionCode.BoxWProducts,
                                    inventRow.DocId
                                    );

                                if (srows != null)
                                {
                                    if (srows.PlanQuanity == srows.FactQuantity)
                                    {
                                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.AlreadyAccepted);
                                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.AlreadyAccepted);

                                        using (DialogForm dlgfrm =
                                new DialogForm(
                                    string.Format("Товар уже принят {0} из {1}",
                                    srows.FactQuantity,
                                    srows.PlanQuanity)
                                    , string.Format("Принять этот товар {0}", row.Barcode)//string.Format("Посчитано: {0} кодов", totalBk)
                                    , row.ProductName
                                    , "Прием товара"))
                                        {
                                            if (dlgfrm.ShowDialog() == DialogResult.Yes)
                                            {
                                                //inventRow.NavCode = row.NavCode;
                                                ActionsClass.Action.BoxWProductsActionProc(
                                                    row,
                                                    docRow,
                                                    quantityKoeff
                                                    );
                                                return;//приняли
                                            }
                                            else //не хотим принимать
                                                return;
                                        }
                                    }
                                }
                                //если не сработали условия - то принимаем
                                //inventRow.NavCode = row.NavCode;
                                ActionsClass.Action.BoxWProductsActionProc(
                                                     row,
                                                     docRow,
                                                     quantityKoeff
                                                     );
                                /*ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.BoxWProducts,
                                    row,
                                    docRow
                                    );*/


                            }
                            else
                            {
                                ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                                ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);

                                using (DialogForm dlgfrm =
                                new DialogForm(
                                    "Товар не входит в короб!"
                                    , string.Format("Принять этот товар {0}", row.Barcode)//string.Format("Посчитано: {0} кодов", totalBk)
                                    , row.ProductName
                                    , "Прием товара"))
                                {
                                    if (dlgfrm.ShowDialog() == DialogResult.Yes)
                                    {
                                        inventRow.NavCode = row.NavCode;
                                        ActionsClass.Action.BoxWProductsActionProc(
                                                    row,
                                                    inventRow,
                                                    quantityKoeff
                                                    );
                                        return;
                                    }
                                }

                            }
                            quantityLabel.Visible = false;
                            quantityKoeff = 1;
                            //this.Refresh();

                        }



                }
                else
                {
                    currentdocRows = null;
                    currentProductRow = null;
                    ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.NotFound, null, null);
                    label5.Text = "...Товар не ";
                    label6.Text = "   найден...";
                    label17.Text = "";
                    //NativeClass.Play("ding.wav");
                }
            }
            finally
            {
                navCodeTB.SelectAll();
                this.Refresh();
            }
        }
        private void ViewProductForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (WorkMode.ProductsScan == _mode)
                {
                    if (BTPrintClass.PrintClass.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress)
                        != Calib.BluetoothLibNet.Def.BTERR_CONNECTED)
                    {

                        if (Program.Default.EnableWorkWOPrinter == 1)
                        {

                            using (BTConnectionErrorForm frm =
                                new BTConnectionErrorForm())
                            {
                                if (frm.ShowDialog() == DialogResult.Yes)
                                {
                                    BTPrintClass.PrintClass.Reconnect();
                                }
                                else
                                {
                                    this.Close();
                                    return;
                                }

                            }
                        }
                        else
                        {
                            this.Close();
                            return;
                        }
                    }
                    //ActionsClass.Action.BeginScan();
                    

                }
                ActionsClass.Action.BeginScan();
                ScanClass.Scaner.InitScan();

                /*
                foreach (Control c in this.Controls)
                {
                    c.GotFocus += new EventHandler(c_GotFocus);
                    c.LostFocus += new EventHandler(c_LostFocus);
                }*/
                /*this.textBox1.Focus();*/
                //System.Threading.Thread t = new System.Threading.Thread(
                //    new System.Threading.ThreadStart(Init));
                //t.Start();
                this.SuspendLayout();
                int w = this.Width / 4;
                this.label13.Width = w;
                this.label14.Width = w;
                this.label15.Width = w;
                this.navCodeTB.Focus();


                ScanClass.Scaner.OnScanned += new Scanned(Scanned);

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


                BTPrintClass.PrintClass.OnSetStatus += new SetStatus(PrintClass_OnSetStatus);
                BTPrintClass.PrintClass.OnSetError += new SetError(PrintClass_OnSetError);
                BTPrintClass.PrintClass.OnConnectionError += new ConnectionError(PrintClass_OnConnectionError);

                this.Refresh();
            }
            finally
            {
                _mEvt.Set();
            }

        }

        void PrintClass_OnConnectionError()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    ConnectionError del = new ConnectionError(PrintClass_OnConnectionError);
                    this.Invoke(del);
                }
                else
                {
                    if (WorkMode.ProductsScan == _mode)
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
                    SetError del = new SetError(PrintClass_OnSetError);
                    this.Invoke(del, text);
                }
                else
                {
                    label18.ForeColor = Color.Red;
                    label18.Text = text;
                    this.Refresh();
                    System.Threading.Thread.Sleep(500);
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
                    SetStatus del = new SetStatus(PrintClass_OnSetStatus);
                    this.Invoke(del, text);
                }
                else
                {
                    label18.ForeColor = Color.Black;
                    label18.Text = text;
                    this.Refresh();
                    System.Threading.Thread.Sleep(500);
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
           
            if (_mode != WorkMode.InventarScan)
            {
                 
                try
                {

                    BTPrintClass.PrintClass.OnSetStatus -= (PrintClass_OnSetStatus);
                    BTPrintClass.PrintClass.OnSetError -= (PrintClass_OnSetError);
                    BTPrintClass.PrintClass.Disconnect();
                }
                catch { };
                ScanClass.Scaner.OnScanned = null;
                
            }
            ActionsClass.Action.EndScan();
            ScanClass.Scaner.StopScan();
            
            ActionsClass.Action.OnActionCompleted -=Action_OnActionCompleted;
            BTPrintClass.PrintClass.SetStatusEvent("Close Products form");
            //ActionsClass.Action.ClearCache();
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
            this.label18.Text = "";
            //label11.Text = e.KeyValue.ToString();
            if (e.KeyCode == Keys.Enter)
            {
                DoAction(ActionsClass.Action.GetProductRowByNavCode(navCodeTB.Text));
                navCodeTB.SelectAll();
                e.Handled = true;
                return;
            }
            
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Tab)
            {
                if (_mode == WorkMode.InventarScan)
                {
                    ScanClass.Scaner.PauseScan();
                    try
                    {
                        int total = 0;
                        int totalBk = 0;

                        ActionsClass.Action.CalculateTotals(
                            _documentId,
                            TSDUtils.ActionCode.InventoryGlobal,
                            out totalBk,
                            out total);

                        using (DialogForm dlgfrm =
                            new DialogForm(
                                "Вы хотите закрыть просчет?"
                                , ""//string.Format("Посчитано: {0} кодов", totalBk)
                                , string.Format("Итого: {0} штук",total)
                                , "Закрытие просчета"))
                        {
                            if (dlgfrm.ShowDialog() == DialogResult.Yes)
                            {
                                ActionsClass.Action.CloseInv(
                                    _documentId,
                                    TSDUtils.ActionCode.InventoryGlobal);

                                this.Close();
                            }
                        }

                    }
                    catch (Exception err)
                    {
                        BTPrintClass.PrintClass.SetErrorEvent(err.ToString());
                        using (DialogForm dlgfrm =
                            new DialogForm(
                                err.Message
                                , err.StackTrace
                                , ""
                                , "Ошибка"))
                        {
                            dlgfrm.ShowDialog();
                        }
                    }
                    finally
                    {
                        ScanClass.Scaner.ResumeScan();
                        this.Refresh();
                        e.Handled = true;
                    }
                }
                return;
            }
            if (e.KeyValue == 18)//RedBtn
            {
                #region redbutton
                if (currentProductRow != null && WorkMode.ProductsScan == _mode)
                {
                    ActionsClass.Action.PrintLabel(currentProductRow, currentDocRow, Program.Default.DefaultRepriceShablon);
                }
                if (currentProductRow != null && 
                    (WorkMode.InventarScan == _mode ||
                    WorkMode.BoxScan == _mode
                    ))
                {
                    ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                             ActionsClass.Action.AddScannedRow(
                             currentProductRow.Barcode,
                             inventRow.DocType,
                             inventRow.DocId,
                             inventRow.Quantity,
                             inventRow.Priority);

                    int total = 0;
                    int totalBk = 0;
                    ActionsClass.Action.CalculateTotalsWOPriority(
                        (TSDUtils.ActionCode)scannedRow.DocType,
                        scannedRow.DocId,
                        scannedRow.Barcode.ToString(),
                        out totalBk,
                        out total);

                    if (scannedRow != null && //existing row
                        scannedRow.Priority == 0 //not closed
                        && scannedRow["FactQuantity"] != System.DBNull.Value
                        && total > 0 //scanned already
                        && 
                        (
                           (Program.СurrentInvId != string.Empty && _mode == WorkMode.InventarScan) ||
                           true
                        )
                    )
                    {
                        


                        using (DialogForm dlgfrm =
                                new DialogForm(
                                    string.Format("Уменьшить по коду {0} ", currentProductRow.NavCode),
                                    string.Format("название {0} ", currentProductRow.ProductName),
                                     string.Format("с количества {0} до количества {1} ?",
                                      total,
                                      total - 1),
                                     "Отмена последнего сканирования"))
                        {
                            if (dlgfrm.ShowDialog() == DialogResult.Yes)
                            {
                                inventRow.NavCode = currentProductRow.NavCode;
                                ActionsClass.Action.UndoLastScannedPosition(
                                    currentProductRow,
                                    inventRow,
                                    scannedRow
                                    );

                            }
                        }
                    }
                    else
                    {
                        using (DialogForm dlgfrm =
                                new DialogForm(
                                    "Дальше уменьшить нельзя!",
                                    "",
                                     "",
                                     "Отмена последнего сканирования"))
                        {
                            dlgfrm.ShowDialog();
                        }
                    }
                    

                }
                e.Handled = true;
                return;
                #endregion
            }
            if (e.KeyValue == 16)//BluBtn
            {
                #region ProductsScan
                if (currentProductRow != null && WorkMode.ProductsScan == _mode)
                {
                    ActionsClass.Action.PrintLabel(currentProductRow, currentDocRow, Program.Default.BlueButtonShablon);
                }
                #endregion
                #region BoxScan
                
                //multyply
                //if (currentProductRow != null && 
                 if (_mode == WorkMode.BoxScan && Program.Default.EnableChgMlt ==1)
                {

                        DialogResult dr = FMultiplyForm.MForm.ShowDialog();

                        if (dr == DialogResult.OK)
                        {
                            quantityKoeff = FMultiplyForm.MForm.Quantity;
                            quantityLabel.Text = string.Format("Добавляется {0} шт", quantityKoeff);
                            quantityLabel.Visible = true;
                            this.navCodeTB.SelectAll();
                            this.navCodeTB.Focus();
                            this.Refresh();
                            
                        }
                }
                #endregion
                e.Handled = true;
                return;
            }
            if (e.KeyValue == 115)//YellowBtn
            {
                try
                {
                    ScanClass.Scaner.PauseScan();
                    if (currentProductRow != null )
                    {
                        if (WorkMode.ProductsScan == _mode)
                        {
                            using (
                                ViewDocsForm docsForm =
                                    new ViewDocsForm(currentProductRow, currentdocRows, ActionsClass.Action.ScannedProducts))
                            {
                                docsForm.ShowDialog();
                            }

                        }
                        else
                        {
                            if (WorkMode.InventarScan == _mode)
                            {
                                if (!String.IsNullOrEmpty(_documentId))
                                {
                                    using (ViewInventarForm prod =
                                        new ViewInventarForm(_documentId,
                                            (byte)TSDUtils.ActionCode.InventoryGlobal))
                                    {
                                        prod.ShowDialog();

                                    }
                                }
                            }
                            else
                                if (WorkMode.BoxScan == _mode)
                                {
                                    if (!String.IsNullOrEmpty(_documentId))
                                    {
                                        using (ViewBoxForm prod =
                                            new ViewBoxForm(_documentId,
                                                (byte)TSDUtils.ActionCode.BoxWProducts))
                                        {
                                            prod.ShowDialog();

                                        }
                                    }
                                }
                        }
                    }
                }
                catch (Exception err)
                {
                    BTPrintClass.PrintClass.SetErrorEvent(err.ToString());
                }
                finally
                {
                    ScanClass.Scaner.ResumeScan();
                    this.Refresh();
                    e.Handled = true;
                }

                        //this.BringToFront();
                        //this.Focus();
                    
                return;
            }
            //if (e.KeyValue == 9)
            //{
            //    try
            //    {
            //        BTPrintClass.PrintClass.PartialReconnect();
            //    }
            //    catch (Exception err)
            //    { BTPrintClass.PrintClass.SetErrorEvent(err.ToString()); }
            //    return;
            //}
            
        }

        private void Init()
        {
           // _mutex.WaitOne();
            /*
            scanned_ta = 
                new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter();
            move_ta = 
                new TSDServer.ScannedProductsDataSetTableAdapters.MoveResultsTblTableAdapter();
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
                this.label18.Text = "";
                currentDocRow = docsRow;
                this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[docsRow.DocType];

                label20.Text = (scannedRow["PlanQuanity"] == System.DBNull.Value ||
                                scannedRow["PlanQuanity"] == null) ? string.Empty : scannedRow.PlanQuanity.ToString();

                label21.Text = (scannedRow["FactQuantity"] == System.DBNull.Value ||
                                scannedRow["FactQuantity"] == null) ? string.Empty : scannedRow.FactQuantity.ToString();

                

                if (docsRow.DocType == (byte)TSDUtils.ActionCode.BoxWProducts)
                {
                    //label21.Text = (scannedRow["FactQuantity"] == System.DBNull.Value ||
                    //            scannedRow["FactQuantity"] == null) ? string.Empty : scannedRow.FactQuantity.ToString();

                    ScannedProductsDataSet.ScannedBarcodesRow[] rowsS
                           = ActionsClass.Action.FindByDocIdAndDocType(scannedRow.DocId,
                           scannedRow.DocType);
                    int fQty = 0;
                    for (int i = 0; i < rowsS.Length; i++)
                    {
                        if (rowsS[i].Barcode == currentProductRow.Barcode)
                            fQty += rowsS[i].FactQuantity;
                    }

                    label21.Text = fQty.ToString();

                    if (fQty == scannedRow.PlanQuanity)
                    {
                       

                        ProductsDataSet.DocsTblRow[] docsRows =
                            ActionsClass.Action.GetDataByDocIdAndType(scannedRow.DocId,
                            scannedRow.DocType);

                        int pQuantity = 0;
                        int fQuantity = 0;
                        for (int i = 0; i < docsRows.Length; i++)
                            pQuantity += docsRows[i].Quantity;

                        for (int i = 0; i < rowsS.Length; i++)
                            fQuantity += rowsS[i].FactQuantity;
                        if (pQuantity == fQuantity)
                        {
                            //907218|3001011908672|7|1|8|2011-09-03|7|7|7|907218|002355438|03.09.2011
                            ProductsDataSet.DocsTblRow[] docsNaklRows =
                                ActionsClass.Action.GetDataByDocIdAndType(scannedRow.DocId,
                                (byte)TSDUtils.ActionCode.IncomeBox);


                            using (DialogForm dlgfrm =
                                new DialogForm(
                                    "Короб по накладной"
                                    , docsNaklRows[0].Text2
                                    , "принят полностью!"
                                    , "Прием товара"))
                            {
                                if (dlgfrm.ShowDialog() == DialogResult.Yes)
                                {
                                    //inventRow.NavCode = row.NavCode;
                                    //ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.BoxWProducts,
                                    //    row,
                                    //    inventRow
                                    //    );
                                }
                            }
                        }
                    }
                }

                this.Refresh();
            }
        }

        
    }

    public enum WorkMode
    {
        ProductsScan,
        InventarScan,
        BoxScan
    }
}