using System;

using System.Collections.Generic;
using System.Text;

namespace TSDServer
{
    public class ActionsClass
    {
        private int _timeShift = 0;
        ScannedProductsDataSet _scannedProducts = new ScannedProductsDataSet();
        TSDServer.ProductsDataSet _products
            = new TSDServer.ProductsDataSet();

        public TSDServer.ProductsDataSet Products
        {
            get { return _products; }
            set { _products = value; }
        }
        
        public ScannedProductsDataSet ScannedProducts
        {
            get { return _scannedProducts; }
            set { _scannedProducts = value; }
        }

        TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA;
        TSDServer.ProductsDataSetTableAdapters.ProductsTblTableAdapter productsTa;
        TSDServer.ProductsDataSetTableAdapters.DocsTblTableAdapter docsTa;

        public delegate void ActOnProduct(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow);
        public delegate void ActionCompleted(ProductsDataSet.DocsTblRow docsRow, ScannedProductsDataSet.ScannedBarcodesRow scannedRow);

        public event ActionCompleted OnActionCompleted;

        Dictionary<byte, List<TSDUtils.SoundDef>>
            Sounds = new Dictionary<byte, List<TSDUtils.SoundDef>>();

        Dictionary<byte, List<TSDUtils.VibroDef>>
            Vibros = new Dictionary<byte, List<TSDUtils.VibroDef>>();

         BTPrintClass btPrint;
        System.Collections.Generic.Dictionary<uint, byte[]> labelCollection =
            new Dictionary<uint, byte[]>();
        System.Globalization.DateTimeFormatInfo dateFormat =
        new System.Globalization.DateTimeFormatInfo();

        System.Globalization.NumberFormatInfo nfi =
                new System.Globalization.NumberFormatInfo();

        public Dictionary<TSDUtils.ActionCode, ActOnProduct> actionsDict =
            new Dictionary<TSDUtils.ActionCode, ActOnProduct>();

        System.Threading.Timer tmr = null;
        static ActionsClass _actionClass = new ActionsClass();
        public static ActionsClass Action
        {
            get
            {
                return _actionClass;
            }
        }

        private ActionsClass()
        {
            int shift = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
            if (shift != 4)
                _timeShift = 11;

            FamilTsdDB.DataTable.BaseDate =
                Program.Default.BaseDate;
            FamilTsdDB.DataTable.StartupPath =
                Program.Default.DatabaseStoragePath;

            btPrint = BTPrintClass.PrintClass;
            dateFormat.ShortDatePattern = "dd.MM.yyyy";
            dateFormat.DateSeparator = ".";
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = "";

            actionsDict.Add(TSDUtils.ActionCode.NoAction, new ActOnProduct(NoActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Reprice, new ActOnProduct(RepriceActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Returns, new ActOnProduct(ReturnActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Remove, new ActOnProduct(RemoveActionProc));
            actionsDict.Add(TSDUtils.ActionCode.QuickHelp, new ActOnProduct(RemoveActionProc));
            actionsDict.Add(TSDUtils.ActionCode.InventoryGlobal, new ActOnProduct(InventoryGlobalActionProc));
            actionsDict.Add(TSDUtils.ActionCode.InventoryLocal, new ActOnProduct(InventoryLocalActionProc));
            actionsDict.Add(TSDUtils.ActionCode.NotFound, new ActOnProduct(NotFoundActionProc));
            actionsDict.Add(TSDUtils.ActionCode.DocNotFound, new ActOnProduct(DocNotFoundActionProc));
            //actionsDict.Add(TSDUtils.ActionCode.BoxWProducts, new ActOnProduct(BoxWProductsActionProc));
            
            /*
            tmr = new System.Threading.Timer(
            new System.Threading.TimerCallback(OnTimer)
            , null,
            System.Threading.Timeout.Infinite,
            System.Threading.Timeout.Infinite);
            
            scannedTA =
                    new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(_scannedProducts);
*/
            productsTa = new ProductsDataSetTableAdapters.ProductsTblTableAdapter(this._products);
            docsTa = new ProductsDataSetTableAdapters.DocsTblTableAdapter(this._products);

            
                
                
        }
        System.Data.DataRowChangeEventHandler onRowChanged;
        //    new System.Data.DataRowChangeEventHandler(ScannedBarcodes_RowChanged);

        System.Data.DataColumnChangeEventHandler onColChanged;

        private bool DoScanEvents = true;
        //    new System.Data.DataColumnChangeEventHandler(ScannedBarcodes_ColumnChanged);


        public void OpenProducts()
        {
            if (!productsTa.Opened)
                productsTa.Open();
            if (!docsTa.Opened)
            {
                docsTa.Open();
            }
            //docsTa.InitPkIndex();

            //docsTa.Fill(this._products);
        }
        public void OpenScanned()
        {
            //scannedTA.Open();

             DoScanEvents = false;
             try
             {
                 if (_scannedProducts.ScannedBarcodes.Rows.Count > 0)
                     return;

                 if (!System.IO.File.Exists(System.IO.Path.Combine(
                             Program.Default.DatabaseStoragePath,
                             "scannedbarcodes.txt")))
                     return;

                 //List<String> openedDocs = new List<string>();
                 //найти все открытые и закрытые инв-ции
                 using (System.IO.StreamReader wr =
                 new System.IO.StreamReader(
                     System.IO.Path.Combine(
                         Program.Default.DatabaseStoragePath,
                         "scannedbarcodes.txt"), true))
                 {
                     string s = string.Empty;
                     while ((s = wr.ReadLine()) != null)
                     {
                         try
                         {
                             string[] strAr = s.Split('|');

                             if (strAr.Length > 0)
                             {
                                 //if (strAr[2] == ((byte)_docType).ToString() &&
                                 //    strAr[1] == _docId)
                                 {
                                     ScannedProductsDataSet.ScannedBarcodesRow row =
                                             ScannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                                     row.Barcode = long.Parse(strAr[0]);
                                     row.DocId = strAr[1];
                                     row.DocType = byte.Parse(strAr[2]);
                                     row.FactQuantity = int.Parse(strAr[3]);
                                     row.ScannedDate = DateTime.Parse(strAr[4], dateFormat);
                                     row.TerminalId = int.Parse(strAr[5]);
                                     row.Priority = byte.Parse(strAr[6]);
                                     row.PlanQuanity = int.Parse(strAr[7]);

                                     ScannedProductsDataSet.ScannedBarcodesRow row1 =
                                        ScannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(
                                            row.Barcode, row.DocType, row.DocId);
                                     if (row1 != null)
                                     {
                                         row1.FactQuantity += row.FactQuantity;
                                         row1.Priority = row.Priority;
                                     }
                                     else
                                         ScannedProducts.ScannedBarcodes.AddScannedBarcodesRow(row);
                                     
                                 }
                             }
                         }
                         catch (FormatException fexc)
                         {
                             BTPrintClass.PrintClass.SetErrorEvent(fexc.ToString() + "\n" + s);
                         }
                     }
                 }
             }
             finally
             {
                 DoScanEvents = true;
             }
        }
        public void CloseProducts()
        {
            //BTPrintClass.PrintClass.SetStatusEvent("Begin close prodducts");
            productsTa.Close();
            //BTPrintClass.PrintClass.SetStatusEvent("Begin close docs");
            docsTa.Close();
            _products.ProductsTbl.Clear();
            _products.DocsTbl.Clear();
            //BTPrintClass.PrintClass.SetStatusEvent("end close prodducts");
        }
        public void ClosedScanned()
        {
            //BTPrintClass.PrintClass.SetStatusEvent("Begin close scanned");
            try
            {
                //scannedTA.Update(this._scannedProducts);
                //BTPrintClass.PrintClass.SetStatusEvent("Update scanned finished");
            }
            catch { }
            //scannedTA.Close();
            //BTPrintClass.PrintClass.SetStatusEvent("end close scanned");
        }
        public void CloseDB()
        {
            CloseProducts();
            ClosedScanned(); 

        }
        public void BeginScan()
        {
           
            //OpenProducts();
            //OpenScanned();

            onRowChanged = new System.Data.DataRowChangeEventHandler(ScannedBarcodes_RowChanged);
            onColChanged = new System.Data.DataColumnChangeEventHandler(ScannedBarcodes_ColumnChanged);
            _scannedProducts.ScannedBarcodes.RowChanged += onRowChanged;
            //                new System.Data.DataRowChangeEventHandler(ScannedBarcodes_RowChanged); 

            _scannedProducts.ScannedBarcodes.ColumnChanged += onColChanged;
            //new System.Data.DataColumnChangeEventHandler(ScannedBarcodes_ColumnChanged);
            //tmr.Change(5000, 60000);
        }

        void ScannedBarcodes_ColumnChanged(object sender, 
            System.Data.DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName == 
                _scannedProducts.ScannedBarcodes.FactQuantityColumn.ColumnName/* ||
                e.Column.ColumnName == 
                _scannedProducts.ScannedBarcodes.PriorityColumn.ColumnName*/ 
                && DoScanEvents)
            {

                WriteDbTxt((ScannedProductsDataSet.ScannedBarcodesRow)e.Row,1);
                
            }
        }
        
        void ScannedBarcodes_RowChanged(object sender, System.Data.DataRowChangeEventArgs e)
        {
            if (e.Action == System.Data.DataRowAction.Add &&
                DoScanEvents)
            {
                WriteDbTxt((ScannedProductsDataSet.ScannedBarcodesRow)e.Row,1);
            }
        }
        void WriteDbTxt(ScannedProductsDataSet.ScannedBarcodesRow row, int quantity)
        {
            if (row.RowState == System.Data.DataRowState.Detached ||
                row.RowState == System.Data.DataRowState.Deleted ||
                row["FactQuantity"] == System.DBNull.Value ||
                row.FactQuantity <= 0 ||
                DoScanEvents == false)
                return;

            using (System.IO.StreamWriter wr =
                new System.IO.StreamWriter(
                    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                    string s =
                            string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                row.Barcode,
                                row.DocId,
                                row.DocType,
                                quantity,
                                (row["ScannedDate"] == System.DBNull.Value) ?
                                  DateTime.Today.AddHours(_timeShift).ToString("dd.MM.yyyy")
                                  : row.ScannedDate.AddHours(_timeShift).ToString("dd.MM.yyyy"),

                                (row["TerminalId"] == System.DBNull.Value) ?
                                   string.Empty : row.TerminalId.ToString(),
                                row.Priority,
                                row.PlanQuanity
                                );
                    wr.WriteLine(s);
                //}

            }
            if (row.DocType == (byte)TSDUtils.ActionCode.Reprice)
            {
                using (System.IO.StreamWriter wr =
                   new System.IO.StreamWriter(
                       System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "register.txt"), true))
                {
                    string s =
                                string.Format("{0},{1,11:D}, {2,7:D}",
                                row.Barcode,
                                1,
                                quantity);
                    wr.WriteLine(s);


                }
            }
        }

        void WriteDbTxt(ScannedProductsDataSet.ScannedBarcodesRow row)
        {
            WriteDbTxt(row, row.FactQuantity);
        }


        public void EndScan()
        {

//            _scannedProducts.ScannedBarcodes.RowChanged -= onRowChanged;
//            _scannedProducts.ScannedBarcodes.ColumnChanged -= onColChanged;

            _scannedProducts.ScannedBarcodes.ColumnChanged -= onColChanged;

            _scannedProducts.ScannedBarcodes.RowChanged -= onRowChanged;
            


            //BTPrintClass.PrintClass.SetStatusEvent("Begin end scan");
            //tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            //CloseProducts();
            //ClosedScanned();
            //BTPrintClass.PrintClass.SetStatusEvent("end scan");
        }
        public void PlaySoundAsync(byte soundCode)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(PlaySound), soundCode);
             
        }

        public void PlaySound(object soundCode)
        {
            PlaySound((byte)soundCode);
        }

        public void PlaySound(byte soundCode)
        {
            if (Sounds.ContainsKey(soundCode))
            {
                foreach (TSDUtils.SoundDef sd in Sounds[soundCode])
                {
                    Calib.SystemLibNet.Api.SysPlayBuzzer((int)sd.SoundType, sd.Frequency, sd.Time);
                }
            }
            else
            {
                lock (Sounds)
                {
                    List<TSDUtils.SoundDef> sndDef =
                        new List<TSDUtils.SoundDef>();
                    try
                    {
                        using (System.IO.StreamReader rdr =
                            new System.IO.StreamReader(System.IO.Path.Combine(
                                Program.StartupPath, string.Format("SOUND_{0}.def", soundCode))))
                        {
                            string str = String.Empty;
                            while (
                                (str = rdr.ReadLine()) != null)
                            {
                                if (str.Trim()[0] == ';')
                                    continue;
                                else
                                    sndDef.Add(TSDUtils.SoundDef.Parse(str));

                            }
                        }
                        Sounds.Add(soundCode, sndDef);
                        PlaySound(soundCode);
                    }
                    catch { }
                }
                
            }
        }

        public void PlayVibroAsync(byte vibroCode)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(
               new System.Threading.WaitCallback(PlayVibro), vibroCode);
        }

        public void PlayVibro(object vibroCode)
        {
            PlayVibro((byte)vibroCode);
        }

        public void PlayVibro(byte vibroCode)
        {
            if (Vibros.ContainsKey(vibroCode))
            {
                foreach (TSDUtils.VibroDef sd in Vibros[vibroCode])
                {
                    Calib.SystemLibNet.Api.SysPlayVibrator((int)sd.VibroType, sd.Count, sd.OnTime, sd.OFFTime);
                }
            }
            else
            {
                lock (Vibros)
                {
                    List<TSDUtils.VibroDef> sndDef =
                        new List<TSDUtils.VibroDef>();
                    try
                    {
                        using (System.IO.StreamReader rdr =
                            new System.IO.StreamReader(System.IO.Path.Combine(
                                Program.StartupPath, string.Format("VIBRO_{0}.def", vibroCode))))
                        {
                            string str = String.Empty;
                            while (
                                (str = rdr.ReadLine()) != null)
                            {
                                if (str.Trim()[0] == ';')
                                    continue;
                                else
                                    sndDef.Add(TSDUtils.VibroDef.Parse(str));

                            }
                        }
                        Vibros.Add(vibroCode, sndDef);
                        PlayVibro(vibroCode);
                    }
                    catch { }
                }
                
            }


        }

        /// <summary>
        /// Печать этикетки. На вход - строка данных и код действия
        /// Код этикетки определяется динамически на основе поля в строке данных и кода действия.
        /// </summary>
        /// <param name="datarow">Строка с данными</param>
        /// <param name="code">Код действия</param>
        public void PrintLabel(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docRow)
        {
            uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docRow.DocType, (uint)docRow.LabelCode);
            PrintLabel(datarow,docRow,shablonCode);
        }

        public bool PrintLabel(ProductsDataSet.ProductsTblRow datarow,  ProductsDataSet.DocsTblRow docRow, uint shablonCode)
        {
            int counter = 0;
            try
            {
                tryagain:
                if (btPrint.mEvt.WaitOne(Program.Default.WaitPrintTimeDefault, false) == false)
                {
                    counter++;
                    btPrint.SetStatusEvent("Ожидание очереди печати {0} попытка...",counter);
                    if (counter < 5)
                        goto tryagain;
                }

                string fileContent = string.Empty;
                int fileLength = 0;
                byte[] bArray = null;
                //uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docRow.DocType, (uint)docRow.LabelCode);
                if (labelCollection.ContainsKey(shablonCode))
                {
                    bArray = labelCollection[shablonCode];
                    fileLength = bArray.Length;
                    fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);
                }
                else
                {
                    string labelName = System.IO.Path.Combine(Program.StartupPath, string.Format("LABEL_{0}.DEF", shablonCode));
                    if (System.IO.File.Exists(labelName))
                    {
                        using (System.IO.FileStream fs = System.IO.File.OpenRead(labelName))
                        {
                            fileLength = (int)fs.Length;
                            bArray = new byte[fileLength];
                            fs.Read(bArray, 0, fileLength);
                            fs.Close();
                        }
                        labelCollection.Add(shablonCode, bArray);
                    }

                }
                //no print shablon - error
                if (bArray == null)
                    return false;

                fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);
                //btPrint.SetStatusEvent(fileContent);

                byte[] bArray2 = ReplaceAttr(bArray, datarow, docRow);
                //fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
                //btPrint.SetStatusEvent(fileContent);
                //return;

                try
                {
                    if (btPrint.Connected)
                    {
                        btPrint.Print(/*fileContent*/bArray2);
                        return true; //success print
                    }
                    else
                    {

                        btPrint.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                        if (btPrint.Connected)
                        {
                            btPrint.Print(/*fileContent*/bArray2);
                            return true; //success print
                        }

                    }
                }
                catch
                {
                    btPrint.SetErrorEvent("Ошибка связи BlueTooth. Ждите 5 сек...");
                    //BTPrintClass.PrintClass.Reconnect();
                    System.Threading.Thread.Sleep(5000);
                    //btPrint.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                    btPrint.Print(/*fileContent*/bArray2);
                    return true; //success print
                }
            }
            catch (BTConnectionFailedException)
            {
                try
                {
                    using (BTConnectionErrorForm frm =
                            new BTConnectionErrorForm())
                    {
                        if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                        {
                            BTPrintClass.PrintClass.Reconnect();
                            PrintLabel(datarow, docRow, shablonCode);
                            return true; //success print
                        }

                    }
                }
                catch (Exception err) 
                { 
                    BTPrintClass.PrintClass.SetErrorEvent(err.ToString());
                    BTPrintClass.PrintClass.SetErrorEvent("Ошибка! Отключите принтер и подключите заново");
                    return false;
                }
            }
            catch (Exception err)
            {
                BTPrintClass.PrintClass.SetErrorEvent(err.ToString());
                BTPrintClass.PrintClass.SetErrorEvent("Отключите принтер и подключите заново");
                return false;
            }
            return false;

        }

        public void PrintLabel(object state)
        {
            PrintLabelParam prm = (PrintLabelParam)state;
            PrintLabel(prm.datarow, prm.docRow);
            
        }

        public void PrintLabelAsync(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docRow)
        {
            PrintLabelParam prm = new PrintLabelParam();
            prm.datarow = datarow;
            prm.docRow = docRow;

            System.Threading.ThreadPool.QueueUserWorkItem(
               new System.Threading.WaitCallback(PrintLabel), prm);
        }



        private byte[] ReplaceAttr(byte[] bArray, ProductsDataSet.ProductsTblRow productsRow, ProductsDataSet.DocsTblRow docsRow)
        {
            string attrString = string.Empty;
             List<byte> tempList = new List<byte>();
            List<byte> outArray = new List<byte>();
            for (int i = 0; i < bArray.Length; i++)
            {
                if (i < bArray.Length + 2 &&
                    bArray[i] == Convert.ToByte('<') &&
                    bArray[i + 1] == Convert.ToByte('<') &&
                    bArray[i + 2] == Convert.ToByte('<'))
                {
                    while (i < bArray.Length &&
                    bArray[i] != Convert.ToByte('>') &&
                    bArray[i - 1] != Convert.ToByte('>') &&
                    bArray[i - 2] != Convert.ToByte('>')

                        )
                    {
                        tempList.Add(bArray[i]);
                        i++;
                    }
                    Byte[] bArrTmp = new byte[tempList.Count];

                    tempList.CopyTo(bArrTmp);
                    tempList.Clear();
                    string atrName = TSDUtils.CustomEncodingClass.Encoding.GetString(bArrTmp).Replace("<", "");
                    System.Data.DataRow datarow = null;
                    if (atrName.IndexOf("GOODS") >= 0 ||
                        atrName.IndexOf("DOCS") >= 0 ||
                        atrName.IndexOf("SCAN") >= 0)
                    {
                        if (atrName.IndexOf("GOODS") >= 0)
                        {
                            attrString = "GOODS_ATTRIBUTE_";
                            datarow = productsRow;
                        }
                        else
                            if (atrName.IndexOf("DOCS") >= 0)
                            {
                                attrString = "DOCS_ATTRIBUTE_";
                                datarow = docsRow;
                            }
                            else
                                if (atrName.IndexOf("SCAN") >= 0)
                                {
                                    attrString = "SCAN_ATTRIBUTE_";
                                    datarow = 
                                        _scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(
                                        productsRow.Barcode, docsRow.DocType, docsRow.DocId); 
                                }
                        string atrCode = atrName.Replace(attrString, "");
                        int colId = -1;

                        //try
                        //{
                        colId = int.Parse(atrCode) - 1;
                        if (colId >= datarow.Table.Columns.Count)
                            continue;
                        bArrTmp = GetAttrValue(datarow[colId], datarow.Table.Columns[colId].DataType);
                    }
                    else
                    {
                        if (atrName.IndexOf("SYSTEMDATE") >= 0)
                        {
                            bArrTmp = GetAttrValue(DateTime.Today, typeof(DateTime));
                        }
                    }
                        //atrName = "Test";
                        /*if (datarow.Table.Columns[colId].DataType == typeof(string) ||
                            datarow.Table.Columns[colId].DataType == typeof(long) ||
                            datarow.Table.Columns[colId].DataType == typeof(int) ||
                            datarow.Table.Columns[colId].DataType == typeof(byte))
                        {
                            bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                datarow[colId].ToString());
                        }
                        else
                            if (datarow.Table.Columns[colId].DataType == typeof(DateTime))
                            {
                                bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                    ((DateTime)datarow[colId]).ToShortDateString());
                            }
                            else
                                if (datarow.Table.Columns[colId].DataType == typeof(Single))
                                {
                                    bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                        ((Single)datarow[colId]).ToString("######.00"));
                                }
                                else
                                bArrTmp =TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                    datarow[colId].ToString());
                    */
                    //}
                    //catch
                    //{
                    //}



                    outArray.AddRange(bArrTmp);
                    i += 2;//skip 2 >>
                }
                else
                {
                    outArray.Add(bArray[i]);
                }
            }
            return outArray.ToArray();
        }
        private byte[] GetAttrValue(object value, Type valueType)
        {
            byte[] bArrTmp;
            if (valueType == typeof(string) ||
                            valueType == typeof(long) ||
                            valueType == typeof(int) ||
                            valueType == typeof(byte))
            {
                bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                    value.ToString());
            }
            else
                if (valueType == typeof(DateTime))
                {
                    bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                        ((DateTime)value).ToString("dd.MM.yyyy",dateFormat));
                }
                else
                    if (valueType == typeof(Single))
                    {
                        bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                            ((Single)value).ToString("######.00"));
                    }
                    else
                        bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                            value.ToString());
            return bArrTmp;
        }

        public void NoActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            PlayVibro((byte)TSDUtils.ActionCode.NotFound);
            PlaySound((byte)TSDUtils.ActionCode.NotFound);
            //PlayVibroAsyncAction(docsRow);
            //PlaySoundAsyncAction(docsRow);
            
            //System.Threading.Thread.Sleep(1000);
        }
        public void RepriceActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            

             ScannedProductsDataSet.ScannedBarcodesRow r =
                _scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(datarow.Barcode, docsRow.DocType, docsRow.DocId);
             if (r != null)
             {
                 
                 try
                 {
                     PlayVibroAsyncAction(docsRow);
                     PlaySoundAsyncAction(docsRow);
                     //PrintLabelAsync(datarow, docsRow);
                     uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docsRow.DocType, (uint)docsRow.LabelCode);
                     if (PrintLabel(datarow, docsRow, shablonCode))
                     {
                         r.FactQuantity += 1;
                         
                     }
                     else
                     {
                         PlayVibroAsync((byte)TSDUtils.ActionCode.NotFound);
                         PlaySoundAsync((byte)TSDUtils.ActionCode.NotFound);
                     }

                 }
                 finally
                 {
                     if (OnActionCompleted != null)
                         OnActionCompleted(docsRow, r);
                 }
             } 
            

            ////System.Threading.Thread.Sleep(1000);
            //ScannedProductsDataSet.ScannedBarcodesRow r = _scannedProducts.ScannedBarcodes.UpdateQuantity(
            //docsRow.Barcode, docsRow.DocType, 1);
            //        if (r != null)
            //        {
            //            TSDUtils.ActionCode ac = (TSDUtils.ActionCode)docsRow.DocType;

            //            //this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[ac];
            //            if (OnActionCompleted != null)
            //                OnActionCompleted(docsRow, r);
            //        }

        }
        public void ReturnActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] r =
                _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            if (r != null)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    
                    try
                    {
                       
                        PlayVibroAsyncAction(docsRow);
                        PlaySoundAsyncAction(docsRow);
                         /*PrintLabelAsync(datarow, docsRow);
                        r[i].FactQuantity += 1;
                         */
                        uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docsRow.DocType, (uint)docsRow.LabelCode);
                        if (PrintLabel(datarow, docsRow, shablonCode))
                        //if (PrintLabel(datarow, docsRow, docsRow.LabelCode))
                        {
                            r[i].FactQuantity += 1;
                            //PlayVibroAsyncAction(docsRow);
                            //PlaySoundAsyncAction(docsRow);
                        }
                        else
                        {
                            PlayVibroAsync((byte)TSDUtils.ActionCode.NotFound);
                            PlaySoundAsync((byte)TSDUtils.ActionCode.NotFound);
                        }

                    }
                    finally
                    {
                        if (OnActionCompleted != null)
                            OnActionCompleted(docsRow, r[i]);
                    }

                    /*if (r[i].FactQuantity < r[i].PlanQuanity)
                    {
                        r[i].FactQuantity += 1;
                        PlayVibroAsyncAction(docsRow);
                        PlaySoundAsyncAction(docsRow);
                        PrintLabelAsync(datarow, docsRow);
                        if (r[i].FactQuantity == r[i].PlanQuanity)
                        {

                            using (RemoveFinishForm frm = new RemoveFinishForm(datarow,docsRow))
                            {
                                frm.ShowDialog();
                                
                            }

                        }
                        if (OnActionCompleted != null)
                            OnActionCompleted(docsRow, r[i]);
                        break;
                    }
                    */
                }
            }
            System.Threading.Thread.Sleep(1000);
        }
        public void RemoveActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] r =
                _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            if (r != null)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    if (r[i].FactQuantity < r[i].PlanQuanity)
                    {
                        //r[i].FactQuantity += 1;
                        try
                        {
                            PlayVibroAsyncAction(docsRow);
                            PlaySoundAsyncAction(docsRow);
                            /*PrintLabelAsync(datarow, docsRow);
                            r[i].FactQuantity += 1;
                            */
                            uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docsRow.DocType, (uint)docsRow.LabelCode);
                            if (PrintLabel(datarow, docsRow, shablonCode))

                            //if (PrintLabel(datarow, docsRow, docsRow.LabelCode))
                            {
                                r[i].FactQuantity += 1;
                                //PlayVibroAsyncAction(docsRow);
                                //PlaySoundAsyncAction(docsRow);
                            }
                            else
                            {
                                PlayVibroAsync((byte)TSDUtils.ActionCode.NotFound);
                                PlaySoundAsync((byte)TSDUtils.ActionCode.NotFound);
                            }


                            if (r[i].FactQuantity == r[i].PlanQuanity)
                            {

                                using (RemoveFinishForm frm = new RemoveFinishForm(datarow, docsRow))
                                {
                                    frm.ShowDialog();

                                }
                            }
                        }
                        finally
                        {
                            if (OnActionCompleted != null)
                                OnActionCompleted(docsRow, r[i]);
                        }
                        break;
                    }
                    
                }
                //this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[ac];
 

            }
            /*ScannedProductsDataSet.ScannedBarcodesRow r = _scannedProducts.ScannedBarcodes.UpdateQuantity(
            docsRow.Barcode, docsRow.DocType, 1);
            if (r != null)
            {
                //TSDUtils.ActionCode ac = (TSDUtils.ActionCode)docsRow.DocType;

                if (r.FactQuantity == r.PlanQuanity)
                {
                    using (RemoveFinishForm frm = new RemoveFinishForm())
                    {
                        frm.ShowDialog();
                    }
                }
                //this.actionLabel.Text = TSDUtils.ActionCodeDescription.ActionDescription[ac];
                if (OnActionCompleted != null)
                    OnActionCompleted(docsRow, r);
            }*/

        }

        public void InventoryGlobalActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            //ScannedProductsDataSet.ScannedBarcodesRow[] r =
            //    _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                               ActionsClass.Action.AddScannedRow(
                               datarow.Barcode,
                               docsRow.DocType,
                               docsRow.DocId,
                               docsRow.Quantity,
                               docsRow.Priority);

            //if (scannedRow == null)
            //{
            //    return 
            //    //r = new ScannedProductsDataSet.ScannedBarcodesRow[1];
            //    //r[0] = scannedRow;
            //}

            //for (int i = 0; i < r.Length; i++)
            //{

                
                PlayVibroAsyncAction(docsRow);
                PlaySoundAsyncAction(docsRow);
                scannedRow.FactQuantity += 1;
                //PrintLabelAsync(datarow, docsRow);
                if (OnActionCompleted != null)
                    OnActionCompleted(docsRow, scannedRow);
                //break;


            //}
        }

        public void BoxWProductsActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow, int quantityFoef)
        {
            //ScannedProductsDataSet.ScannedBarcodesRow[] r =
            //    _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow = null;
            for (int i = 0; i < quantityFoef; i++)
            {
                scannedRow = ActionsClass.Action.AddScannedRow(
                                   datarow.Barcode,
                                   docsRow.DocType,
                                   docsRow.DocId,
                                   docsRow.Quantity,
                                   0);

                //if (scannedRow == null)
                //{
                //    return 
                //    //r = new ScannedProductsDataSet.ScannedBarcodesRow[1];
                //    //r[0] = scannedRow;
                //}

                //for (int i = 0; i < r.Length; i++)
                //{

                scannedRow.FactQuantity += 1;
            }
            PlayVibroAsyncAction(docsRow);
            PlaySoundAsyncAction(docsRow);
            //PrintLabelAsync(datarow, docsRow);
            if (OnActionCompleted != null)
                OnActionCompleted(docsRow, scannedRow);
            //break;


            //}
        }

        public void AcceptFullBoxWProductsActionProc(string docId)
        {
            //ScannedProductsDataSet.ScannedBarcodesRow[] r =
            //    _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);

            BeginScan();
            try
            {
                ScannedProductsDataSet.ScannedBarcodesRow scannedRow = null;

                ProductsDataSet.DocsTblRow[] docs =
                    docsTa.GetAllDataByDocIdAndType(docId, (byte)TSDUtils.ActionCode.BoxWProducts);

                foreach (ProductsDataSet.DocsTblRow docrow in docs)
                {
                    ProductsDataSet.ProductsTblRow productRow =
                        GetProductRowByNavCode(docrow.NavCode);

                    scannedRow = ActionsClass.Action.AddScannedRow(
                                       productRow.Barcode,
                                       docrow.DocType,
                                       docrow.DocId,
                                       docrow.Quantity,
                                       Byte.MaxValue);
                    if (docrow.Quantity > 0)
                    {
                        for (int i = 0; i < docrow.Quantity; i++)
                        {
                            scannedRow.FactQuantity += 1;
                        }
                    }
                    else
                        scannedRow.FactQuantity += 1;



                }
            }
            finally
            {
                EndScan();
            }

            //if (scannedRow == null)
            //{
            //    return 
            //    //r = new ScannedProductsDataSet.ScannedBarcodesRow[1];
            //    //r[0] = scannedRow;
            //}

            //for (int i = 0; i < r.Length; i++)
            //{


            //PlayVibroAsyncAction(docsRow);
            //PlaySoundAsyncAction(docsRow);
            
            //PrintLabelAsync(datarow, docsRow);
            //if (OnActionCompleted != null)
            //    OnActionCompleted(docsRow, scannedRow);
            //break;


            //}
        }

        public void InventoryLocalActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] r =
                _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            if (r != null)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    if (r[i].FactQuantity < r[i].PlanQuanity)
                    {
                        
                        PlayVibroAsyncAction(docsRow);
                        PlaySoundAsyncAction(docsRow);
                        //PrintLabelAsync(datarow, docsRow);
                        //r[i].FactQuantity += 1;
                        uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docsRow.DocType, (uint)docsRow.LabelCode);
                        if (PrintLabel(datarow, docsRow, shablonCode))
                        //if (PrintLabel(datarow, docsRow, docsRow.LabelCode))
                        {
                            r[i].FactQuantity += 1;
                            //PlayVibroAsyncAction(docsRow);
                            //PlaySoundAsyncAction(docsRow);
                        }
                        else
                        {
                            PlayVibroAsync((byte)TSDUtils.ActionCode.NotFound);
                            PlaySoundAsync((byte)TSDUtils.ActionCode.NotFound);
                        }

                        if (r[i].FactQuantity == r[i].PlanQuanity)
                        {

                            using (RemoveFinishForm frm = new RemoveFinishForm(datarow, docsRow))
                            {
                                frm.ShowDialog();
                            }
                        }
                        if (OnActionCompleted != null)
                            OnActionCompleted(docsRow, r[i]);
                        break;
                    }

                }
            }
        }

        public void NotFoundActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            PlayVibroAsync((byte)TSDUtils.ActionCode.NotFound);
            PlaySoundAsync((byte)TSDUtils.ActionCode.NotFound);
        }

        public void DocNotFoundActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            PlayVibroAsync((byte)TSDUtils.ActionCode.DocNotFound);
            PlaySoundAsync((byte)TSDUtils.ActionCode.DocNotFound);
        }

        public void IncomeCarBoxAction(string TTNBarcode, Boxes b)
        {
            //ProductsDataSet.DocsTblRow[] rows =
            //                ActionsClass.Action.GetDataByDocIdAndType(barcode,
            //                (byte)TSDUtils.ActionCode.IncomeBox);

            //if (rows != null && rows.Length > 0)
            //{
                //ProductsDataSet.DocsTblRow row = rows[0];
                ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                                  ActionsClass.Action.AddScannedRow(
                                  long.Parse(b.Barcode),
                                  (byte)TSDUtils.ActionCode.CarsBoxes,
                                  TTNBarcode,
                                  0,
                                  0);
                scannedRow.FactQuantity += 1;
            /*
                using (System.IO.StreamWriter wr =
               new System.IO.StreamWriter(
                   System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
                {
                    //if (row["FactQuantity"] != System.DBNull.Value
                    //    && row.FactQuantity > 0)
                    //{
                    string s =
                            string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                                b.Barcode,
                                TTNBarcode,//docId,
                                ((byte)TSDUtils.ActionCode.CarsBoxes),
                                1,
                                DateTime.Today.ToString("dd.MM.yyyy"),
                                Program.Default.TerminalID,
                                0
                                );
                    wr.WriteLine(s);
                    //}

                }*/
                ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.IncomeBox);
                ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.IncomeBox);

                //return true;
            //}
            //else
            //{
            //    ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
            //    ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                
            //    return false;
            //}

        }
        
        /// <summary>
        /// Добавление записи о сканированном коробе, при режиме проверки коробов свой - чужой
        ///ScannedProductsDataSet.ScannedBarcodesRow
        ///long.Parse(barcode),
        ///(byte)TSDUtils.ActionCode.IncomeBox,
        ///row.DocId,
        ///row.Quantity,
        ///row.Priority);
        ///scannedRow.FactQuantity += 1;
        /// </summary>
        /// <param name="row"> ScannedProductsDataSet.ScannedBarcodesRow</param>
        public void IncomeBoxAction(ScannedProductsDataSet.ScannedBarcodesRow scannedRow)
        {
            using (System.IO.StreamWriter wr =
           new System.IO.StreamWriter(
               System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                            scannedRow.Barcode,
                            scannedRow.DocId,
                            scannedRow.DocType,
                            1,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            0
                            );
                wr.WriteLine(s);
            }
        }

        public void CloseCarAction(string ttnBarcode)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
           _scannedProducts.ScannedBarcodes.FindByDocIdAndDocType(ttnBarcode,
               (byte)TSDUtils.ActionCode.Cars);

            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                /*throw new ApplicationException(string.Format("Документ {0} с типом {1} не найден!",
                    docId,
                    docType.ToString())
                    );*/
            }
            else
            {
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    scannedrow[i].Priority = byte.MaxValue;
                    //WriteDbTxt(scannedrow[i]);
                }

            }

            using (System.IO.StreamWriter wr =
                new System.IO.StreamWriter(
                    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            ttnBarcode,
                            ttnBarcode,
                            ((byte)TSDUtils.ActionCode.Cars),
                            0,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            byte.MaxValue,
                            0
                            );
                wr.WriteLine(s);
                //}

            }
        }
        public void ClearScannedData()
        {
            scannedTA = new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(
                this.ScannedProducts);
            
            scannedTA.Clean();

            string [] str= new string[scannedTA.FileList.Length];
            Array.Copy(scannedTA.FileList,str,str.Length);

            
            scannedTA.Close();
            scannedTA.Dispose();

            this.ScannedProducts.Clear();

            if (System.IO.File.Exists(
                System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt")))
                System.IO.File.Delete(System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"));

            if (System.IO.File.Exists(
                System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "register.txt")))
                System.IO.File.Delete(System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "register.txt"));

            foreach (string s in str)
            {
                if (System.IO.File.Exists(s))
                    System.IO.File.Delete(s);
            }
        }

        public void LoadScannedData()
        {
            /*
            if (!scannedTA.Opened)
                scannedTA.Open();

            scannedTA.Fill(this._scannedProducts);
            scannedTA.Close();*/
        }
        public void PlaySoundAsyncAction(ProductsDataSet.DocsTblRow docsRow)
        {
            //System.Threading.Thread.Sleep(1000);
            ActionsClass.Action.PlaySoundAsync(docsRow.MusicCode);
        }
        public void PlayVibroAsyncAction(ProductsDataSet.DocsTblRow docsRow)
        {
            //System.Threading.Thread.Sleep(1000);
            ActionsClass.Action.PlayVibroAsync(docsRow.VibroCode);
        }

        public void InvokeAction(TSDUtils.ActionCode ac, ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            if (actionsDict.ContainsKey(ac))
            {
                actionsDict[ac].Invoke(datarow, docsRow);
            }
                    
        }

        public class PrintLabelParam
        {
            public ProductsDataSet.ProductsTblRow datarow;
            public ProductsDataSet.DocsTblRow docRow;
        }

        private void OnTimer(object state)
        {
            /*
            try
            {
                ViewProductForm._mEvt.Reset();
                if (!scannedTA.Opened)
                    scannedTA.Open();
                scannedTA.Update(this._scannedProducts);
            }
            finally
            {
                ViewProductForm._mEvt.Set();
            }
             * */
        }
        /// <summary>
        /// Посчитать общее кол-во ШК в документе
        /// и кол-во уникальных ШК
        /// </summary>
        /// <param name="docId">№ док-та</param>
        /// <param name="docType">Тип документа</param>
        /// <param name="totalBk">уникальное кол-во ШК</param>
        /// <param name="total">Всего записей</param>
        
        public void CalculateTotals(string docId, TSDUtils.ActionCode docType, out int totalBk, out int total)
        {

            total = 0;
            totalBk = 0;


            foreach (ScannedProductsDataSet.ScannedBarcodesRow row in
                ScannedProducts.ScannedBarcodes)
            {
                if (row.DocId == docId &&
                    row.DocType == (byte)docType)
                {
                    total += row.FactQuantity;
                    totalBk++;

                }
            }
            return;

            if (!System.IO.File.Exists(System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt")))
                return;

             using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
            {
                string s = string.Empty;
                List<string> scannedList =
                    new List<string>();
                while ((s = wr.ReadLine()) != null)
                {
                    string[] strAr = s.Split('|');
                    if (strAr.Length > 0)
                    {
                        if (strAr[2] == ((byte)docType).ToString() &&
                            strAr[1] == docId)
                        {
                            //if (strAr[4] == date.ToString("dd.MM.yyyy"))
                            //{
                                total += int.Parse(strAr[3]);
                                if (!scannedList.Contains(strAr[0]))
                                {
                                    scannedList.Add(strAr[0]);
                                    totalBk += 1;
                                }
                            //}
                        }

                            
                    }
                }
            }

            /*ScannedProductsDataSet.ScannedBarcodesRow [] scannedrow  =
            _scannedProducts.ScannedBarcodes.FindByDocIdAndDocType(docId,
                (byte)docType);
            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                return;
            }
            else
            {
                total = scannedrow.Length;
                List<long> scannedList =
                    new List<long>();
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    if (!scannedList.Contains(scannedrow[i].Barcode))
                    {
                        scannedList.Add(scannedrow[i].Barcode);
                        totalBk += scannedrow[i].FactQuantity;
                    }
                }
            }*/
        }

        /// <summary>
        /// Подсчет ШК в документах с указанным типом
        /// и Приоритетом = 0 
        /// </summary>
        /// <param name="docType">Тип документа</param>
        /// <param name="totalBk">Возврат всего уникальных ШК</param>
        /// <param name="total">Возврат всего записей</param>
        public void CalculateTotals(TSDUtils.ActionCode docType, 
            out int totalBk, 
            out int total)
        {

            total = 0;
            totalBk = 0;
            if (!System.IO.File.Exists(System.IO.Path.Combine(
            Program.Default.DatabaseStoragePath,
            "scannedbarcodes.txt")))
                return;
            using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
            {
                string s = string.Empty;
                //List<long> scannedList =
                //    new List<long>();
                List<string> scannedList =
                    new List<string>();
                while ((s = wr.ReadLine()) != null)
                {
                    string[] strAr = s.Split('|');
                    if (strAr.Length > 0)
                    {
                        if (strAr[2] == ((byte)docType).ToString() &&
                            strAr[6] == "255")
                        {
                            //if (strAr[4] == date.ToString("dd.MM.yyyy"))
                            //{
                                //total += 1;
                            total += int.Parse(strAr[3]);
                            if (!scannedList.Contains(strAr[0]))
                            {
                                scannedList.Add(strAr[0]);
                                totalBk += 1;
                            }
                            //}
                        }

                            
                    }
                }
            }
            /*ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
            _scannedProducts.ScannedBarcodes.FindByDocTypeAndPriority(
                (byte)docType,
                0);
            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                return;
            }
            else
            {
                total = scannedrow.Length;
                List<long> scannedList =
                    new List<long>();
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    if (!scannedList.Contains(scannedrow[i].Barcode))
                    {
                        scannedList.Add(scannedrow[i].Barcode);
                        totalBk += 1;
                    }
                }
            }*/
        }

        /// <summary>
        /// Подсчет ШК в документах с указанным типом
        /// </summary>
        /// <param name="docType">Тип документа</param>
        /// <param name="totalBk">Возврат всего уникальных ШК</param>
        /// <param name="total">Возврат всего записей</param>
        public void CalculateTotalsWOPriority(TSDUtils.ActionCode docType,
            out int totalBk,
            out int total)
        {

            total = 0;
            totalBk = 0;
            if (!System.IO.File.Exists(System.IO.Path.Combine(
            Program.Default.DatabaseStoragePath,
            "scannedbarcodes.txt")))
                return;
            using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
            {
                string s = string.Empty;
                List<string> scannedList =
                    new List<string>();
                while ((s = wr.ReadLine()) != null)
                {
                    string[] strAr = s.Split('|');
                    if (strAr.Length > 0)
                    {
                        if (strAr[2] == ((byte)docType).ToString())
                        {
                            //if (strAr[4] == date.ToString("dd.MM.yyyy"))
                            //{
                            total += 1;
                            if (!scannedList.Contains(strAr[0]))
                            {
                                scannedList.Add(strAr[0]);
                                totalBk += 1;
                            }
                            //}
                        }


                    }
                }
            }
            /*ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
            _scannedProducts.ScannedBarcodes.FindByDocType(
                (byte)docType);

            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                return;
            }
            else
            {
                total = scannedrow.Length;
                List<long> scannedList =
                    new List<long>();
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    if (!scannedList.Contains(scannedrow[i].Barcode))
                    {
                        scannedList.Add(scannedrow[i].Barcode);
                        totalBk += 1;
                    }
                }
            }*/
        }

        /// <summary>
        /// Подсчет ШК в документах с указанным типом
        /// </summary>
        /// <param name="docType">Тип документа</param>
        /// <param name="totalBk">Возврат всего уникальных ШК</param>
        /// <param name="total">Возврат всего записей</param>
        public void CalculateTotalsWOPriority(TSDUtils.ActionCode docType,
            DateTime date,
            out int totalBk,
            out int total)
        {

            total = 0;
            totalBk = 0;

            using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
            {
                string s = string.Empty;
                List<long> scannedList =
                    new List<long>();
                while ((s = wr.ReadLine()) != null)
                {
                    string[] strAr = s.Split('|');
                    if (strAr.Length > 0)
                    {
                        if (strAr[2] == ((byte)docType).ToString())
                        {
                            if (strAr[4] == date.ToString("dd.MM.yyyy"))
                            {
                                total += 1;
                                if (!scannedList.Contains(long.Parse(strAr[0])))
                                {
                                    scannedList.Add(long.Parse(strAr[0]));
                                    totalBk += 1;
                                }
                            }
                        }

                            
                    }
                }
                //string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                //    row.Barcode,
                //    row.DocId,
                //    row.DocType,
                //    quantity,
                //    (row["ScannedDate"] == System.DBNull.Value) ?
                //      DateTime.Today : row.ScannedDate,

                //    (row["TerminalId"] == System.DBNull.Value) ?
                //       string.Empty : row.TerminalId.ToString(),
                //    row.Priority
                //    );
                //wr.WriteLine(s);
                //}

            }
            /*
            ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
            _scannedProducts.ScannedBarcodes.FindByDocType(
                (byte)docType);

            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                return;
            }
            else
            {
                total = scannedrow.Length;
                List<long> scannedList =
                    new List<long>();
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    if (scannedrow[i].ScannedDate == date)
                    {
                        if (!scannedList.Contains(scannedrow[i].Barcode))
                        {
                            scannedList.Add(scannedrow[i].Barcode);
                            totalBk += 1;
                        }
                    }
                }
            }*/
        }
        /// <summary>
        /// Закрыть просчет инвентаризации
        /// Просчет закрыт обозначает Priority=Byte.MaxValue
        /// </summary>
        /// <param name="docId">Номер док-та = адрес просчета</param>
        /// <param name="docType">Тип документа</param>
        
        public void CloseInv(string docId, TSDUtils.ActionCode docType)
        {


            ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
            _scannedProducts.ScannedBarcodes.FindByDocIdAndDocType(docId,
                (byte)docType);
            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                /*throw new ApplicationException(string.Format("Документ {0} с типом {1} не найден!",
                    docId,
                    docType.ToString())
                    );*/
            }
            else
            {
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    scannedrow[i].Priority = byte.MaxValue;
                    //WriteDbTxt(scannedrow[i]);
                }

            }
            using (System.IO.StreamWriter wr =
                new System.IO.StreamWriter(
                    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            docId,
                            docId,
                            ((byte)TSDUtils.ActionCode.CloseInventar),
                            0,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            255,
                            0
                            );
                wr.WriteLine(s);
                //}

            }
            //текущий открытый просчет теперь пуст
            Program.СurrentInvId = string.Empty;
                
            



        }

        public void CloseDoc(string docId, TSDUtils.ActionCode docType)
        {


            ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
            _scannedProducts.ScannedBarcodes.FindByDocIdAndDocType(docId,
                (byte)docType);
            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                /*throw new ApplicationException(string.Format("Документ {0} с типом {1} не найден!",
                    docId,
                    docType.ToString())
                    );*/
            }
            else
            {
                for (int i = 0; i < scannedrow.Length; i++)
                {
                    scannedrow[i].Priority = byte.MaxValue;
                    //WriteDbTxt(scannedrow[i]);
                }

            }
            using (System.IO.StreamWriter wr =
                new System.IO.StreamWriter(
                    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            docId,
                            docId,
                            ((byte)docType),
                            0,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            255,
                            0
                            );
                wr.WriteLine(s);
                //}

            }
            //текущий открытый просчет теперь пуст
            //Program.СurrentInvId = string.Empty;





        }
        
        public bool CheckInv(string docId)
        {
                if (!System.IO.File.Exists(System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt")))
                return false;

                List<String> openedDocs = new List<string>();
            //найти все открытые и закрытые инв-ции
                using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
                {
                    string s = string.Empty;
                    while ((s = wr.ReadLine()) != null)
                    {
                        string[] strAr = s.Split('|');
                        //string openedDocId=string.Empty;
                        if (strAr.Length > 0)
                        {
                            if (strAr[2] == ((byte)TSDUtils.ActionCode.CloseInventar).ToString() &&
                                strAr[6] == "255" &&
                                strAr[1] == docId)
                            {
                                if (openedDocs.Contains(strAr[0]))
                                {
                                    openedDocs.Remove(strAr[0]);
                                    return false;
                                }
                            }

                            if (strAr[2] == ((byte)TSDUtils.ActionCode.CloseInventar).ToString() &&
                                strAr[6] == "0" &&
                                strAr[1] == docId)
                                openedDocs.Add(strAr[0]);
                            //openedDocId = strAr[0];

                            //return strAr[0];
                        }
                    }
                    if (openedDocs.Count > 0)
                        return true;
                    else
                        return false;
                }
        }
        public void OpenInv(string docId, TSDUtils.ActionCode docType)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] r = FindByDocIdAndDocType(docId,
                (byte)TSDUtils.ActionCode.CloseInventar);

            if (r != null &&
                r.Length > 0)
                throw new ApplicationException(string.Format("Инв-ция {0} уже просчитана",docId));

            using (System.IO.StreamWriter wr =
                new System.IO.StreamWriter(
                    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            docId,
                            docId,
                            ((byte)TSDUtils.ActionCode.CloseInventar),
                            0,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            0,
                            0
                            );
                wr.WriteLine(s);
                //}

            }
            Program.СurrentInvId = docId;




        }

        /// <summary>
        /// Уменьшить кол-во отсканированного на 1
        /// </summary>
        /// <param name="datarow"></param>
        /// <param name="docsRow"></param>
        public void UndoLastScannedPosition(ProductsDataSet.ProductsTblRow datarow,
            ProductsDataSet.DocsTblRow docsRow,
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow)
        {
            //ScannedProductsDataSet.ScannedBarcodesRow[] r =
            //_scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);

           

            //if (scannedRow != null && //existing row
            //    scannedRow.Priority == 0 //not closed
            //    && scannedRow["FactQuantity"] != System.DBNull.Value
            //    && scannedRow.FactQuantity > 0 //scanned already
            //    && Program.СurrentInvId != string.Empty
            //    )
            //{
                DoScanEvents = false;

                try
                {
                    scannedRow.FactQuantity -= 1;


                    using (System.IO.StreamWriter wr =
                    new System.IO.StreamWriter(
                        System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
                    {
                        //if (row["FactQuantity"] != System.DBNull.Value
                        //    && row.FactQuantity > 0)
                        //{
                        string s =
                                string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                    scannedRow.Barcode,
                                    scannedRow.DocId,
                                    scannedRow.DocType,
                                    -1,
                                    (scannedRow["ScannedDate"] == System.DBNull.Value) ?
                                      DateTime.Today.AddHours(_timeShift).ToString("dd.MM.yyyy")
                                      : scannedRow.ScannedDate.AddHours(_timeShift).ToString("dd.MM.yyyy"),

                                    (scannedRow["TerminalId"] == System.DBNull.Value) ?
                                       string.Empty : scannedRow.TerminalId.ToString(),
                                    scannedRow.Priority,
                                    scannedRow.PlanQuanity
                                    );
                        wr.WriteLine(s);
                    }
                    if (OnActionCompleted != null)
                        OnActionCompleted(docsRow, scannedRow);
                }

                catch
                {

                }
                finally
                {
                    DoScanEvents = true;

                }
                //}

            //}
        }

        public void ChangeQtyPosition(
            ProductsDataSet.DocsTblRow docsRow,
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow,
            int diffQty)
        {
            //ScannedProductsDataSet.ScannedBarcodesRow[] r =
            //_scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);



            //if (scannedRow != null && //existing row
            //    scannedRow.Priority == 0 //not closed
            //    && scannedRow["FactQuantity"] != System.DBNull.Value
            //    && scannedRow.FactQuantity > 0 //scanned already
            //    && Program.СurrentInvId != string.Empty
            //    )
            //{
            DoScanEvents = false;

            if (diffQty < 0)
            {
                for (int i = diffQty; i < 0; i++)
                {
                    
                    scannedRow.FactQuantity -= 1;
                    using (System.IO.StreamWriter wr =
                    new System.IO.StreamWriter(
                        System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
                    {
                        //if (row["FactQuantity"] != System.DBNull.Value
                        //    && row.FactQuantity > 0)
                        //{
                        string s =
                                string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                    scannedRow.Barcode,
                                    scannedRow.DocId,
                                    scannedRow.DocType,
                                    -1,
                                    (scannedRow["ScannedDate"] == System.DBNull.Value) ?
                                      DateTime.Today.AddHours(_timeShift).ToString("dd.MM.yyyy")
                                      : scannedRow.ScannedDate.AddHours(_timeShift).ToString("dd.MM.yyyy"),

                                    (scannedRow["TerminalId"] == System.DBNull.Value) ?
                                       string.Empty : scannedRow.TerminalId.ToString(),
                                    scannedRow.Priority,
                                    scannedRow.PlanQuanity
                                    );
                        wr.WriteLine(s);
                    }

                }
            }
            else
            {
                if (diffQty > 0)
                {
                    for (int i = 0; i < diffQty; i++)
                    {
                        scannedRow.FactQuantity += 1;
                        using (System.IO.StreamWriter wr =
                        new System.IO.StreamWriter(
                            System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
                        {
                            //if (row["FactQuantity"] != System.DBNull.Value
                            //    && row.FactQuantity > 0)
                            //{
                            string s =
                                    string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                        scannedRow.Barcode,
                                        scannedRow.DocId,
                                        scannedRow.DocType,
                                        1,
                                        (scannedRow["ScannedDate"] == System.DBNull.Value) ?
                                          DateTime.Today.AddHours(_timeShift).ToString("dd.MM.yyyy")
                                          : scannedRow.ScannedDate.AddHours(_timeShift).ToString("dd.MM.yyyy"),

                                        (scannedRow["TerminalId"] == System.DBNull.Value) ?
                                           string.Empty : scannedRow.TerminalId.ToString(),
                                        scannedRow.Priority,
                                        scannedRow.PlanQuanity
                                        );
                            wr.WriteLine(s);
                        }
                    }
                }
            }
            if (OnActionCompleted != null && docsRow != null)
                OnActionCompleted(docsRow, scannedRow);
            DoScanEvents = true;

            
            //}

            //}
        }
            
    

        

        public ProductsDataSet.ProductsTblRow GetProductRow(string barcode)
        {
            try
            {
                return productsTa.GetDataByBarcode(long.Parse(barcode));
            }
            catch
            {
                return null;
            }
            //return row;

        }

        public ProductsDataSet.ProductsTblRow GetProductRowByNavCode(string navCode)
        {
            try
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
            catch
            {
                return null;
            }

        }

        public ProductsDataSet.DocsTblRow[] GetDataByNavCode(string navCode)
        {
            try
            {
                return docsTa.GetDataByNavCode(navCode);
            }
            catch
            {
                return null;
            }
        }

        public ProductsDataSet.DocsTblRow[] GetDataByNavCodeAndType(string navCode, byte type)
        {
            try
            {
                return docsTa.GetDataByNavCodeAndType(navCode,type);
            }
            catch
            {
                return null;
            }
        }

        public ProductsDataSet.DocsTblRow[] GetDataByDocIdAndType(string DocId, byte docType)
        {
            //try
            //{
                //return this._products.DocsTbl.FindByDocIdAndType(DocId, docType);

                    return docsTa.GetAllDataByDocIdAndType(DocId, docType);
            //}
            //catch
            //{
            //    return null;
            //}
        }

        public ProductsDataSet.DocsTblRow GetDataByNavcodeDocIdAndType(string Navcode, string DocId, byte docType)
        {
            //try
            //{
            //return this._products.DocsTbl.FindByDocIdAndType(DocId, docType);

            return docsTa.GetDataByDocIdNavcodeAndType(Navcode, DocId, docType);
            //}
            //catch
            //{
            //    return null;
            //}
        }


        /// <summary>
        /// Добавить строку в сканированные документы. Кол-во Факт = 0
        /// </summary>
        /// <param name="barcode">штрихкод</param>
        /// <param name="docType">тип документа</param>
        /// <param name="docId">№документа</param>
        /// <param name="quantity">Кол-во План </param>
        /// <param name="priority">Приоритет</param>
        /// <returns>Сформированная строка</returns>
        public ScannedProductsDataSet.ScannedBarcodesRow AddScannedRow(
            long barcode,
            byte docType,
            string docId,
            int quantity,
            byte priority
            )
        {
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                            _scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(
                            barcode,
                            docType,
                            docId);
            if (scannedRow == null)
            {
                scannedRow =
                    _scannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                scannedRow.Barcode = barcode;
                scannedRow.DocId = docId;
                scannedRow.DocType = docType;
                scannedRow.PlanQuanity = quantity;
                scannedRow.Priority = priority;
                scannedRow.ScannedDate = DateTime.Today;
                scannedRow.FactQuantity = 0;
                scannedRow.TerminalId = Program.TerminalId;

                _scannedProducts.ScannedBarcodes.AddScannedBarcodesRow(scannedRow);
            }
            
            //scannedRow.FactQuantity += 1;
            return scannedRow;
        }

        public ScannedProductsDataSet.ScannedBarcodesRow[] 
             FindByDocIdAndDocType(string _docId,byte _docType)
             {
                 List<ScannedProductsDataSet.ScannedBarcodesRow> rows =
                     new List<ScannedProductsDataSet.ScannedBarcodesRow>();
                 DoScanEvents = false;
                 //foreach (ScannedProductsDataSet.ScannedBarcodesRow row in ScannedProducts.ScannedBarcodes)
                 //{
                 //    if (row.DocType == _docType &&
                 //        row.DocId == _docId)
                 //        rows.Add(row);
                 //}
                 //return rows.ToArray();
                 try
                 {
                     if (!System.IO.File.Exists(System.IO.Path.Combine(
                          Program.Default.DatabaseStoragePath,
                          "scannedbarcodes.txt")))
                         return null;

                     using (System.IO.StreamReader wr =
                     new System.IO.StreamReader(
                         System.IO.Path.Combine(
                             Program.Default.DatabaseStoragePath,
                             "scannedbarcodes.txt"), true))
                     {
                         string s = string.Empty;
                         //List<long> scannedList =
                         //    new List<long>();
                         while ((s = wr.ReadLine()) != null)
                         {
                             string[] strAr = s.Split('|');
                             try
                             {
                                 if (strAr.Length > 0)
                                 {
                                     if (strAr[2] == ((byte)_docType).ToString() &&
                                         strAr[1] == _docId)
                                     {
                                         ScannedProductsDataSet.ScannedBarcodesRow row =
                                                 ScannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                                         row.Barcode = long.Parse(strAr[0]);
                                         row.DocId = strAr[1];
                                         row.DocType = byte.Parse(strAr[2]);
                                         row.FactQuantity = int.Parse(strAr[3]);
                                         row.ScannedDate = DateTime.Parse(strAr[4], dateFormat);
                                         row.TerminalId = int.Parse(strAr[5]);
                                         row.Priority = byte.Parse(strAr[6]);
                                         row.PlanQuanity = int.Parse(strAr[7]);
                                         rows.Add(row);
                                     }

                                     /*   string s =
                                   string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                                       row.Barcode,
                                       row.DocId,
                                       row.DocType,
                                       quantity,
                                       (row["ScannedDate"] == System.DBNull.Value) ?
                                         DateTime.Today.ToString("dd.MM.yyyy")
                                         : row.ScannedDate.ToString("dd.MM.yyyy"),

                                       (row["TerminalId"] == System.DBNull.Value) ?
                                          string.Empty : row.TerminalId.ToString(),
                                       row.Priority
                                       );
                                 
                                    }*/


                                 }
                             }
                             catch (FormatException fexc)
                             {
                                 BTPrintClass.PrintClass.SetErrorEvent(fexc.ToString() + "\n" + s);
                             }
                         }
                     }
                 }
                 finally
                 {
                     DoScanEvents = true;
                 }
                 return rows.ToArray();

             }

        public ScannedProductsDataSet.ScannedBarcodesRow
              FindByBarcodeDocTypeDocId(string Barcode,
                                        byte DocType,
                                        string DocId)
        {

            return FindByBarcodeDocTypeDocId(long.Parse(Barcode), DocType, DocId);
        }

        public ScannedProductsDataSet.ScannedBarcodesRow
            FindByBarcodeDocTypeDocId(long Barcode,
                                      byte DocType,
                                      string DocId)
        {
            //if (!System.IO.File.Exists(System.IO.Path.Combine(
            //     Program.Default.DatabaseStoragePath,
            //     "scannedbarcodes.txt")))
            //    return null;

            foreach (ScannedProductsDataSet.ScannedBarcodesRow row in ScannedProducts.ScannedBarcodes)
            {
                if (row.DocType == DocType &&
                    row.DocId == DocId &&
                    row.Barcode == Barcode)
                    return row;
            }
            return null;


            using (System.IO.StreamReader wr =
                 new System.IO.StreamReader(
                     System.IO.Path.Combine(
                         Program.Default.DatabaseStoragePath,
                         "scannedbarcodes.txt"), true))
            {
                string s = string.Empty;
                //List<long> scannedList =
                //    new List<long>();
                while ((s = wr.ReadLine()) != null)
                {
                    string[] strAr = s.Split('|');
                    if (strAr.Length > 0)
                    {
                        if (strAr[2] == DocType.ToString() &&
                            strAr[1] == DocId &&
                            strAr[0] == Barcode.ToString())
                        {
                            ScannedProductsDataSet.ScannedBarcodesRow row =
                                    ScannedProducts.ScannedBarcodes.NewScannedBarcodesRow();
                            row.Barcode = long.Parse(strAr[0]);
                            row.DocId = strAr[1];
                            row.DocType = byte.Parse(strAr[2]);
                            row.FactQuantity = int.Parse(strAr[3]);
                            row.ScannedDate = DateTime.Parse(strAr[4], dateFormat);
                            row.TerminalId = int.Parse(strAr[5]);
                            row.Priority = byte.Parse(strAr[6]);
                            row.PlanQuanity = int.Parse(strAr[7]);
                            ScannedProducts.ScannedBarcodes.AddScannedBarcodesRow(row);

                            return row;
                        }
                    }
                }
            }
            return null;

        }
        public void ClearCache()
        {
            this.Products.ProductsTbl.Clear();
            this.Products.DocsTbl.Clear();
            this.ScannedProducts.ScannedBarcodes.Clear();
            this.ScannedProducts.AcceptChanges();

            this.Products.AcceptChanges();

        }

        public string FindOpenInventar()
        {
            /*ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
           _scannedProducts.ScannedBarcodes.FindByDocTypeAndPriority(
           (byte)TSDUtils.ActionCode.InventoryGlobal,
           0);//openInventar;

            if (scannedrow != null &&
                scannedrow.Length != 0)
            {
                return scannedrow[0].DocId;
            }
            else
            {*/
            DoScanEvents = false;
            try
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(
                            Program.Default.DatabaseStoragePath,
                            "scannedbarcodes.txt")))
                    return string.Empty;

                List<String> openedDocs = new List<string>();
                //найти все открытые и закрытые инв-ции
                using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
                {
                    string s = string.Empty;
                    while ((s = wr.ReadLine()) != null)
                    {
                        try
                        {
                            string[] strAr = s.Split('|');

                            if (strAr.Length > 0)
                            {
                                /*
                                ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                                ActionsClass.Action.AddScannedRow(
                                long.Parse(strAr[0]),
                                byte.Parse(strAr[2]),
                                strAr[1],
                                int.Parse(strAr[3]),
                                byte.Parse(strAr[6]));

                                scannedRow.FactQuantity += int.Parse(strAr[3]);
                                */
                                //string openedDocId=string.Empty;

                                if (strAr[0].StartsWith("660"))
                                {


                                    //if (scannedRow == null)
                                    //{
                                    //    return 
                                    //    //r = new ScannedProductsDataSet.ScannedBarcodesRow[1];
                                    //    //r[0] = scannedRow;
                                    //}

                                    //for (int i = 0; i < r.Length; i++)
                                    //{

                                    if (strAr[2] == ((byte)TSDUtils.ActionCode.CloseInventar).ToString() &&
                                        strAr[6] == "255")
                                    {
                                        if (openedDocs.Contains(strAr[0]))
                                        {
                                            openedDocs.Remove(strAr[0]);
                                        }
                                    }

                                    if (strAr[2] == ((byte)TSDUtils.ActionCode.CloseInventar).ToString() &&
                                        strAr[6] == "0")
                                        openedDocs.Add(strAr[0]);
                                    //openedDocId = strAr[0];

                                    //return strAr[0];
                                }
                            }
                        }
                        catch (FormatException fexc)
                        {
                            BTPrintClass.PrintClass.SetErrorEvent(fexc.ToString() + "\n" + s);
                        }
                    }
                    if (openedDocs.Count > 0)
                        return openedDocs[0];
                    else
                        return string.Empty;

                    //if (row["FactQuantity"] != System.DBNull.Value
                    //    && row.FactQuantity > 0)
                    //{
                    //string s =
                    //        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    //            docId,
                    //            docId,
                    //            docType,
                    //            0,
                    //            DateTime.Today,
                    //            Program.Default.TerminalID,
                    //            255
                    //            );
                    //  string s =
                    //string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    //    row.Barcode,
                    //    row.DocId,
                    //    row.DocType,
                    //    quantity,
                    //    (row["ScannedDate"] == System.DBNull.Value) ?
                    //      DateTime.Today : row.ScannedDate,

                    //    (row["TerminalId"] == System.DBNull.Value) ?
                    //       string.Empty : row.TerminalId.ToString(),
                    //    row.Priority
                    //    );
                    //wr.WriteLine(s);
                    //}

                    //}
                    //return string.Empty;
                }
            }
            finally
            {
                DoScanEvents = true;
            }
        }
        
        public string TestDB(out bool noerrors)
        {
            noerrors = true;
            TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

            StringBuilder sb = new StringBuilder();
            
            DateTime dtDB = DateTime.Now;
            
            
            sb.Append(string.Format("Текущее время: {0}\n", dtDB.ToString("dd.MM.yyyy HH:mm")));

            //sb.Append("Shift: " + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString());



            string fileDB = 
                string.Format("{0}\\{1}.db", Program.Default.DatabaseStoragePath, 
                this.Products.ProductsTbl.TableName);
            sb.Append(CheckDbFile(ref noerrors, fileDB, out dtDB));

            /*System.IO.FileInfo fi = new System.IO.FileInfo(fileDB);
            fi.Refresh();
            if (fi.Exists && fi.Length>0)
            {
               dtDB = fi.CreationTime;
            }
            else
                return string.Format("Файл Базы {0} отсутствует!", fileDB);
            */
            sb.Append(TestFileArray(ref noerrors, dtDB, this.productsTa.FileList));
            /*
            foreach (string file in this.productsTa.FileList)
            {

                if (System.IO.File.Exists(file))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(
                    file
                    );
                    fi.Refresh();
                    if (Math.Abs(fi.CreationTime.Subtract(dtDB).Minutes)>15)
                        sb.Append(string.Format("Файл БД {0} возможно старый!", file));
                }
                else
                    sb.Append(string.Format("Файл {0} отсутствует!", file));

            }*/

            fileDB =
                string.Format("{0}\\{1}.db", Program.Default.DatabaseStoragePath,
                this.Products.DocsTbl.TableName);
            sb.Append(CheckDbFile(ref noerrors, fileDB, out dtDB));
            /*
            System.IO.FileInfo fi = new System.IO.FileInfo(fileDB);
            fi.Refresh();
            if (fi.Exists && fi.Length > 0)
            {
                dtDB = fi.CreationTime;
            }
            else
                return string.Format("Файл Базы {0} отсутствует!", fileDB);
            */
            sb.Append(TestFileArray(ref noerrors, dtDB, this.docsTa.FileList));

           
            return sb.ToString();
        }

        private string TestFileArray(ref bool noerrors, DateTime dtDB, string[] fileList)
        {
            //TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            StringBuilder sb = new StringBuilder();
            foreach (string file in fileList)
            {

                if (System.IO.File.Exists(file))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(
                    file
                    );
                    fi.Refresh();
                    if (Math.Abs(fi.LastWriteTime.AddHours(_timeShift).Subtract(dtDB).Minutes) > 15)
                    {
                        sb.Append(string.Format("Файл БД\n {0}\n возможно старый!\n", file));
                        noerrors = noerrors & false;
                    }
                }
                else
                {
                    noerrors = noerrors & false;
                    sb.Append(string.Format("Файл\n {0}\n отсутствует!\n", file));
                }

            }
            return sb.ToString();
        }

        private string CheckDbFile(ref bool noerrors, string fileDB, out DateTime dtDB)
        {
            //TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            dtDB = DateTime.Now;
            System.IO.FileInfo fi = new System.IO.FileInfo(fileDB);
            fi.Refresh();
            if (fi.Exists && fi.Length > 0)
            {
                dtDB = fi.LastWriteTime.AddHours(_timeShift);

                return string.Format("Дата файла Базы\n {0}: \n{1} \n", fileDB,
                    dtDB.ToString("dd.MM.yyyy HH:mm"));
            }
            else
            {
                noerrors = noerrors & false;
                return string.Format("Файл Базы \n{0} \nне найден!\n", fileDB);
                
            }
        }
    }
}
