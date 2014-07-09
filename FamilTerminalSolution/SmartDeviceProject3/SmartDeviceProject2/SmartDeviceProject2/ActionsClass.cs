﻿using System;

using System.Collections.Generic;
using System.Text;

namespace TSDServer
{
    public class ActionsClass
    {

        //byte[] buff = new byte[] { 0xD, 0xA };

        private int _timeShift = 0;
        ScannedProductsDataSet _scannedProducts = new ScannedProductsDataSet();
        TSDServer.ProductsDataSet _products
            = new TSDServer.ProductsDataSet();
        private string DatabaseFile = string.Empty;
        private static Random randovValueGenerator = new Random();

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
        ProductsDataSet.DocsTblRow ActualBoxReturnsDocRow = null;

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
        System.IO.FileStream writer = null;

        ScannedRetnBoxWQty _scannedRetnBoxWQty = new ScannedRetnBoxWQty();

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
            dateFormat.FullDateTimePattern = "dd.MM.yyyy HH:mm:ss";
            dateFormat.DateSeparator = ".";
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = "";

            actionsDict.Add(TSDUtils.ActionCode.NoAction, new ActOnProduct(NoActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Reprice, new ActOnProduct(RepriceActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Returns, new ActOnProduct(ReturnActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Remove, new ActOnProduct(RemoveActionProc));
            actionsDict.Add(TSDUtils.ActionCode.QuickHelp, new ActOnProduct(RemoveActionProc));
            actionsDict.Add(TSDUtils.ActionCode.InventoryGlobal, new ActOnProduct(InventoryGlobalActionProc));
            actionsDict.Add(TSDUtils.ActionCode.SimpleIncome, new ActOnProduct(SimpleIncomeActionProc));
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

            DatabaseFile =
                System.IO.Path.Combine(
                         Program.Default.DatabaseStoragePath,
                         "scannedbarcodes.txt");

            onRowChanged = new System.Data.DataRowChangeEventHandler(ScannedBarcodes_RowChanged);
            onColChanged = new System.Data.DataColumnChangeEventHandler(ScannedBarcodes_ColumnChanged);
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

                 if (!System.IO.File.Exists(DatabaseFile) 
                     )
                     //System.IO.Path.Combine(Program.Default.DatabaseStoragePath,"scannedbarcodes.txt")))
                     return;

                 //List<String> openedDocs = new List<string>();
                 //найти все открытые и закрытые инв-ции

                 using (System.IO.FileStream fs =
                     new System.IO.FileStream(DatabaseFile,
                         System.IO.FileMode.Open,
                         System.IO.FileAccess.ReadWrite,
                         System.IO.FileShare.ReadWrite))
                 {
                   
                     if (fs.Length == 0)
                         return;

                     byte[] arrayOfBytes =
                       new byte[fs.Length];

                     fs.Read(arrayOfBytes, 0, arrayOfBytes.Length);
                     string allFile = System.Text.Encoding.UTF8.GetString(arrayOfBytes, 0, arrayOfBytes.Length);

                     string[] allLinesOfFile = allFile.Split('\n');
                     //string[] allLinesOfFile = allFile.Split(
                     //    System.Text.Encoding.UTF8.GetChars(buff));

                     

                     foreach (string s in allLinesOfFile)
                     {
                         if (String.IsNullOrEmpty(s))
                             continue;

                         string[] strAr = s.Split('|');

                         try
                         {


                             //if (strAr.Length > 0 && strAr.Length == 8)
                             if (strAr.Length == 8)
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
                         catch (Exception err)
                         {
                             BTPrintClass.PrintClass.SetErrorEvent(s + '\n' + err.ToString());

                         }
                     }
                     fs.Close();
                 }
                     
                 /*
                 using (System.IO.StreamReader wr =
                 new System.IO.StreamReader(
                     //System.IO.Path.Combine(Program.Default.DatabaseStoragePath,"scannedbarcodes.txt")
                     DatabaseFile, true))
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
                 }*/
             }
             finally
             {
                 ScannedProducts.ScannedBarcodes.AcceptChanges();
                 DoScanEvents = true;

                 if (writer == null)
                 {
                     writer =
                     new System.IO.FileStream(DatabaseFile,System.IO.FileMode.OpenOrCreate,System.IO.FileAccess.ReadWrite,
                         System.IO.FileShare.ReadWrite);
                     writer.Position = writer.Length;
                         //DatabaseFile, true);
                     //writer.AutoFlush = true;
                    
                 }

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
            writer.Flush();
            writer.Close();
            writer = null;
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

            
            _scannedProducts.ScannedBarcodes.RowChanged += onRowChanged;
            //                new System.Data.DataRowChangeEventHandler(ScannedBarcodes_RowChanged); 

            _scannedProducts.ScannedBarcodes.ColumnChanging += onColChanged;
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
                object oldColVal = e.Row[e.Column.ColumnName];
                if (e.Row[e.Column.ColumnName] != System.DBNull.Value)
                //&& e.ProposedValue != System.DBNull.Value)
                {
                    int oldVal = (int)e.Row[e.Column.ColumnName];
                    int newVal = 0;
                    if (e.ProposedValue != System.DBNull.Value)
                        newVal = (int)e.ProposedValue;
                    else
                        WriteDbTxt((ScannedProductsDataSet.ScannedBarcodesRow)e.Row, 0 - oldVal);

                    if (newVal != oldVal)
                        WriteDbTxt((ScannedProductsDataSet.ScannedBarcodesRow)e.Row, newVal - oldVal);
                    
                }
                else
                    WriteDbTxt((ScannedProductsDataSet.ScannedBarcodesRow)e.Row, 1);
                
                
            }
        }
        
        void ScannedBarcodes_RowChanged(object sender, System.Data.DataRowChangeEventArgs e)
        {
            if (e.Action == System.Data.DataRowAction.Add &&
                DoScanEvents)
            {
                WriteDbTxt((ScannedProductsDataSet.ScannedBarcodesRow)e.Row, ((ScannedProductsDataSet.ScannedBarcodesRow)e.Row).FactQuantity);
            }
        }
        void WriteDbTxt(ScannedProductsDataSet.ScannedBarcodesRow row, int quantity)
        {
            if (row.RowState == System.Data.DataRowState.Detached ||
                row.RowState == System.Data.DataRowState.Deleted ||
                row["FactQuantity"] == System.DBNull.Value ||
                //row.FactQuantity <= 0 ||
                //quantity <=0 ||
                DoScanEvents == false)
                return;
            
            /*using (System.IO.StreamWriter wr =
                new System.IO.StreamWriter(
                    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {*/
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                {
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
                    byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                    //byte[] buff = new byte[] { 0xD, 0xA };

                    writer.Write(buff, 0, buff.Length);
                    writer.Flush();
                    //byte[] buff = System.Text.Encoding.UTF8.GetBytes(s);
                    //writer.Write(buff, 0, buff.Length);
                    //writer.WriteLine(System.
                    //    s);
                }

            //}
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
            //WriteDbTxt(row, row.FactQuantity);
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
                int i = fileContent.IndexOf("MAGICSTRING");
                if (i >= 0)
                {
                    string s1 = fileContent.Substring(0, i);
                    byte[] bArray2 = ReplaceAttr(TSDUtils.CustomEncodingClass.Encoding.GetBytes(s1), datarow, docRow);
                    //fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
                    //btPrint.SetStatusEvent(s1);
                    //return;
                    bool result1 = Print(bArray2);

                    string s2 = fileContent.Substring(i + "MAGICSTRING".Length+2);
                    byte[] bArray3 = ReplaceAttr(TSDUtils.CustomEncodingClass.Encoding.GetBytes(s2), datarow, docRow);
                    //fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
                    //btPrint.SetStatusEvent(s2);
                    //return;

                    bool result2 = Print(bArray3);
                    bArray2 = null;
                    bArray3 = null;
                    return result1 & result2;

                }
                else
                {


                    byte[] bArray2 = ReplaceAttr(bArray, datarow, docRow);
                    //fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
                    //btPrint.SetStatusEvent(fileContent);
                    //return;
                    bool result2 = Print(bArray2);
                    return result2;
                }
                /*
                try
                {
                    if (btPrint.Connected)
                    {
                        btPrint.Print(bArray2);
                        return true; //success print
                    }
                    else
                    {

                        btPrint.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                        if (btPrint.Connected)
                        {
                            btPrint.Print(bArray2);
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
                    btPrint.Print(bArray2);
                    return true; //success print
                }*/
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
        private bool Print(byte[] bArray2)
        {
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
                    else
                        return false;

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
                        ((DateTime)value).ToString("dd.MM.yyyy", dateFormat));
                }
                else
                    if (valueType == typeof(Single))
                    {
                        Single v1 = (Single)value;
                        int v2 = (Int32)((Single)value);

                        if (v1 == v2)
                        {
                            bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(v2.ToString("######"));
                        }
                        else
                            bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(v1.ToString("######.00"));
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

        public void SimpleIncomeActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
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

        public void ReturnBoxWProductsActionProc(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
        {
            //ScannedProductsDataSet.ScannedBarcodesRow[] r =
            //    _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                               ActionsClass.Action.AddScannedRow(
                               datarow.Barcode,
                               ((byte)TSDUtils.ActionCode.ReturnBoxWProducts),
                               docsRow.DocId,
                               docsRow.Quantity,
                               docsRow.Priority);


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
            //for (int i = 0; i < quantityFoef; i++)
            //{
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

                scannedRow.FactQuantity += quantityFoef;
            //}
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

            //BeginScan();
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
                    scannedRow.FactQuantity += docrow.Quantity;
                    /*
                    if (docrow.Quantity > 0)
                    {
                        for (int i = 0; i < docrow.Quantity; i++)
                        {
                            scannedRow.FactQuantity += 1;
                        }
                    }
                    else
                        scannedRow.FactQuantity += 1;
                    */


                }
            }
            finally
            {
                //EndScan();
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
           // using (System.IO.StreamWriter wr =
           //new System.IO.StreamWriter(
           //    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
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
                byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s,'\n'));
                //byte[] buff = new byte[] { 0xD, 0xA };
                writer.Write(buff, 0, buff.Length);
                writer.Flush();
                //writer.WriteLine(s);
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

           // using (System.IO.StreamWriter wr =
           //     new System.IO.StreamWriter(
           //         System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
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
                byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                //byte[] buff = new byte[] { 0xD, 0xA };
                //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                writer.Write(buff, 0, buff.Length);
                writer.Flush();
                //writer.WriteLine(s);
                //}

            }
        }

        public void ClearScannedData()
        {
            //scannedTA = new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(
             //   this.ScannedProducts);
            //if (scannedTA != null)
            //    scannedTA.Clean();

            //string [] str= new string[scannedTA.FileList.Length];
            //Array.Copy(scannedTA.FileList,str,str.Length);

            
            //scannedTA.Close();
            //scannedTA.Dispose();
            if (writer != null)
            {
                try
                {
                    //writer.Flush()
                    writer.Close();
                    writer = null;
                    System.Threading.Thread.Sleep(500);
                }
                catch { }
            }

            this.ScannedProducts.Clear();

            if (System.IO.File.Exists(
                System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt")))
                System.IO.File.Delete(System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"));

            if (System.IO.File.Exists(
                System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "register.txt")))
                System.IO.File.Delete(System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "register.txt"));

            /*foreach (string s in str)
            {
                if (System.IO.File.Exists(s))
                    System.IO.File.Delete(s);
            }*/
        }

        public void LoadScannedData()
        {
            OpenScanned();
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
            #region

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
            #endregion
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

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            {
                OpenScanned();
            }
            List<long> scannedList =
                    new List<long>();

            foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
                _scannedProducts.ScannedBarcodes)
            {
                if (//r.DocId == docId &&
                    r.DocType == (byte)docType &&
                    //r.Barcode == long.Parse(barcode)
                    r.Priority == 255
                    )
                {
                    total += r.FactQuantity;//int.Parse(strAr[3]);
                    if (!scannedList.Contains(r.Barcode))
                    {
                        scannedList.Add(r.Barcode);
                        totalBk += 1;
                    }
                }

            }
            return;
            #region oldaction
            /*if (!System.IO.File.Exists(System.IO.Path.Combine(
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
            }*/
            #endregion
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

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            {
                OpenScanned();
            }
            List<long> scannedList =
                    new List<long>();

            foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
                _scannedProducts.ScannedBarcodes)
            {
                if (//r.DocId == docId &&
                    r.DocType == (byte)docType //&&
                    //r.Barcode == long.Parse(barcode)
                    //r.Priority == 255
                    )
                {
                    total += r.FactQuantity;//int.Parse(strAr[3]);
                    if (!scannedList.Contains(r.Barcode))
                    {
                        scannedList.Add(r.Barcode);
                        totalBk += 1;
                    }
                }

            }
            return;
            #region old action
            /*if (!System.IO.File.Exists(System.IO.Path.Combine(
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
            }*/
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
            #endregion
        }

        /// <summary>
        /// Подсчет ШК в документах с указанным типом
        /// </summary>
        /// <param name="docType">Тип документа</param>
        /// <param name="totalBk">Возврат всего уникальных ШК</param>
        /// <param name="total">Возврат всего записей</param>
        public void CalculateTotalsWOPriority(TSDUtils.ActionCode docType,
            string docId,
            string barcode,
            out int totalBk,
            out int total)
        {

            total = 0;
            totalBk = 0;

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            {
                OpenScanned();
            }
            List<long> scannedList =
                    new List<long>();

            foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
                _scannedProducts.ScannedBarcodes)
            {
                if (r.DocId == docId &&
                    r.DocType == (byte)docType &&
                    r.Barcode == long.Parse(barcode))
                {
                    total += r.FactQuantity;//int.Parse(strAr[3]);
                    if (!scannedList.Contains(r.Barcode))
                    {
                        scannedList.Add(r.Barcode);
                        totalBk += 1;
                    }
                }

            }
            return;

            #region old action
            if (!System.IO.File.Exists(System.IO.Path.Combine(
            Program.Default.DatabaseStoragePath,
            "scannedbarcodes.txt")))
                return;

           /* using (System.IO.StreamReader wr =
                new System.IO.StreamReader(
                    System.IO.Path.Combine(
                        Program.Default.DatabaseStoragePath,
                        "scannedbarcodes.txt"), true))
            {
                string s = string.Empty;
                //List<string> scannedList =
                //    new List<string>();
                while ((s = wr.ReadLine()) != null)
                {
                    string[] strAr = s.Split('|');
                    if (strAr.Length > 0)
                    {
                        if (strAr[2] == ((byte)docType).ToString() &&
                            strAr[0] == barcode &&
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
            }*/
            #endregion
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

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            {
                OpenScanned();
            }
            List<long> scannedList =
                    new List<long>();

            foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
                _scannedProducts.ScannedBarcodes)
            {
                if (//r.DocId == docId &&
                    r.DocType == (byte)docType &&
                    //r.Barcode == long.Parse(barcode)
                    r.ScannedDate == date
                    )
                {
                    total += r.FactQuantity;//int.Parse(strAr[3]);
                    if (!scannedList.Contains(r.Barcode))
                    {
                        scannedList.Add(r.Barcode);
                        totalBk += 1;
                    }
                }

            }
            return;
            #region old action
            /*using (System.IO.StreamReader wr =
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

            }*/
            #endregion
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
            CloseInv(docId, docType, InventarFormMode.DefaultInventar);
        }
        public void CloseInv(string docId, TSDUtils.ActionCode docType, InventarFormMode mode)
        {

            byte docCloseType = (mode == InventarFormMode.DefaultInventar) ?
                (byte)TSDUtils.ActionCode.CloseInventar :
                (byte)TSDUtils.ActionCode.CloseIncome;

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
            //using (System.IO.StreamWriter wr =
            //    new System.IO.StreamWriter(
            //        System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{

                DoScanEvents = false;
                try
                {
                    ScannedProductsDataSet.ScannedBarcodesRow r1 =
                        _scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(
                        long.Parse(docId), docCloseType//((byte)TSDUtils.ActionCode.CloseInventar)
                        , docId);
                    if (r1 != null)
                        r1.Priority = 255;
                    else
                        r1 = AddScannedRow(long.Parse(docId), ((byte)TSDUtils.ActionCode.CloseInventar),
                            docId, 0, 255);

                    string s =
                            string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                docId,
                                docId,
                                docCloseType,//((byte)TSDUtils.ActionCode.CloseInventar),
                                0,
                                DateTime.Today.ToString("dd.MM.yyyy"),
                                Program.Default.TerminalID,
                                255,
                                0
                                );
                    byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                    //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                    //byte[] buff = new byte[] { 0xD, 0xA };
                    writer.Write(buff, 0, buff.Length);
                    writer.Flush();
                }
                finally
                {
                    DoScanEvents = true;
                }
                //writer.WriteLine(s);
                //}

            }
            //текущий открытый просчет теперь пуст

            

            if (mode == InventarFormMode.DefaultInventar)
                Program.СurrentInvId = string.Empty;
            else
                Program.СurrentIncomeId = string.Empty;
            




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
            //using (System.IO.StreamWriter wr =
            //    new System.IO.StreamWriter(
            //        System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
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
                byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                //byte[] buff = new byte[] { 0xD, 0xA };
                writer.Write(buff, 0, buff.Length);
                writer.Flush();
                //writer.WriteLine(s);
                //}

            }
            //текущий открытый просчет теперь пуст
            //Program.СurrentInvId = string.Empty;





        }


        public void CloseDoc(string docId, TSDUtils.ActionCode docType, int totalFactQty)
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
            //using (System.IO.StreamWriter wr =
            //    new System.IO.StreamWriter(
            //        System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            docId,
                            docId,
                            ((byte)docType),
                            totalFactQty,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            255,
                            0
                            );
                byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                //byte[] buff = new byte[] { 0xD, 0xA };
                writer.Write(buff, 0, buff.Length);
                writer.Flush();
                //writer.WriteLine(s);
                //}

            }
            //текущий открытый просчет теперь пуст
            //Program.СurrentInvId = string.Empty;





        }
        public bool CheckInv(string docId)
        {
            return CheckInv(docId, InventarFormMode.DefaultInventar);
        }

        public bool CheckInv(string docId, InventarFormMode mode)
        {
            byte docType = (mode == InventarFormMode.DefaultInventar)?
                (byte)TSDUtils.ActionCode.CloseInventar:
                (byte)TSDUtils.ActionCode.CloseIncome;

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            {
                OpenScanned();
            }
            List<long> openedDocs = new List<long>();

            foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
                _scannedProducts.ScannedBarcodes)
            {
                if (r.DocId == docId &&
                    r.DocType == docType//((byte)TSDUtils.ActionCode.CloseInventar) //&&r.Priority == 255
                    //r.Barcode == long.Parse(barcode)
                    //r.ScannedDate == date
                    )
                {
                    if (r.Priority == 255)
                    {
                        if (openedDocs.Contains(r.Barcode))
                        {
                            openedDocs.Remove(r.Barcode);
                            return false;
                        }
                    }
                    else
                        openedDocs.Add(r.Barcode);
                        /*
                    if (r.DocType == ((byte)TSDUtils.ActionCode.CloseInventar) &&
                        r.Priority == 0 &&
                        r.DocId == docId)
                        openedDocs.Add(r.Barcode);
                         * */
                }

            }
            if (openedDocs.Count > 0)
                return true;
            else
                return false;
            #region old action
            /*
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
            */
            #endregion
        }
        public void OpenInv(string docId, TSDUtils.ActionCode docType)
        {
            OpenInv(docId, docType, InventarFormMode.DefaultInventar);
        }

        public void OpenInv(string docId, TSDUtils.ActionCode docType, InventarFormMode mode )
        {
            byte docCloseType = (mode == InventarFormMode.DefaultInventar) ?
             (byte)TSDUtils.ActionCode.CloseInventar :
             (byte)TSDUtils.ActionCode.CloseIncome;
            string textToThrow = (mode == InventarFormMode.DefaultInventar) ?
                "Инв-ция" :
                "Накл-я";

            ScannedProductsDataSet.ScannedBarcodesRow[] r = FindByDocIdAndDocType(docId, docCloseType);
                //(byte)TSDUtils.ActionCode.CloseInventar);

            if (r != null &&
                r.Length > 0)
                throw new ApplicationException(string.Format("{0} {1} уже просчитана", textToThrow, docId));

            //using (System.IO.StreamWriter wr =
            //    new System.IO.StreamWriter(
            //        System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
            {
                //if (row["FactQuantity"] != System.DBNull.Value
                //    && row.FactQuantity > 0)
                //{
                DoScanEvents = false;
                try
                {
                    ScannedProductsDataSet.ScannedBarcodesRow r1 =
                        AddScannedRow(long.Parse(docId),
                        docCloseType//((byte)TSDUtils.ActionCode.CloseInventar)
                        , docId,
                        0, 0);

                    string s =
                            string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                docId,
                                docId,
                                docCloseType,//((byte)TSDUtils.ActionCode.CloseInventar),
                                0,
                                DateTime.Today.ToString("dd.MM.yyyy"),
                                Program.Default.TerminalID,
                                0,
                                0
                                );

                    byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                    //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                    //byte[] buff = new byte[] { 0xD, 0xA };
                    writer.Write(buff, 0, buff.Length);
                    writer.Flush();
                }
                finally
                {
                    DoScanEvents = true;
                }
                //writer.WriteLine(s);
                //}

            }
            //Program.СurrentInvId = docId;

            if (mode == InventarFormMode.DefaultInventar)
                Program.СurrentInvId = docId;
            else
                Program.СurrentIncomeId = docId;




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
            //DoScanEvents = false;
            scannedRow.FactQuantity -= 1;
            if (OnActionCompleted != null)
                OnActionCompleted(docsRow, scannedRow);
            return;

            #region old action
            try
                {
                    scannedRow.FactQuantity -= 1;


                    //using (System.IO.StreamWriter wr =
                    //new System.IO.StreamWriter(
                    //    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
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
                        //writer.WriteLine(s);
                        byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                        //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                        //byte[] buff = new byte[] { 0xD, 0xA };
                        writer.Write(buff, 0, buff.Length);
                        writer.Flush();
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
#endregion
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
            //DoScanEvents = false;
            scannedRow.FactQuantity = scannedRow.FactQuantity + diffQty;

            if (OnActionCompleted != null && docsRow != null)
                OnActionCompleted(docsRow, scannedRow);

            return;
            #region old action
            if (diffQty < 0)
            {
                //scannedRow.FactQuantity = 1;
                for (int i = diffQty; i < 0; i++)
                {
                    
                    scannedRow.FactQuantity -= 1;
                    //using (System.IO.StreamWriter wr =
                    //new System.IO.StreamWriter(
                    //    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
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
                        //writer.WriteLine(s);
                        //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                        byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));

                        writer.Write(buff, 0, buff.Length);
                        writer.Flush();
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
                        //using (System.IO.StreamWriter wr =
                       // new System.IO.StreamWriter(
                        //    System.IO.Path.Combine(Program.Default.DatabaseStoragePath, "scannedbarcodes.txt"), true))
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
                            //writer.WriteLine(s);
                            byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                            //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                            writer.Write(buff, 0, buff.Length);
                            writer.Flush();
                        }
                    }
                }
            }
            #endregion

            //DoScanEvents = true;
            //if (OnActionCompleted != null && docsRow != null)
            //    OnActionCompleted(docsRow, scannedRow);
            
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
            try
            {
                DoScanEvents = false;
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
                    _scannedProducts.ScannedBarcodes.AcceptChanges();

                }
                return scannedRow;

            }
            finally
            {
                DoScanEvents = true;
            }
            //scannedRow.FactQuantity += 1;
            
        }

        public ScannedProductsDataSet.ScannedBarcodesRow[] 
             FindByDocIdAndDocType(string _docId,byte _docType)
             {
                 List<ScannedProductsDataSet.ScannedBarcodesRow> rows =
                     new List<ScannedProductsDataSet.ScannedBarcodesRow>();
                 //DoScanEvents = false;

                 if (ScannedProducts.ScannedBarcodes.Rows.Count > 0)
                 {

                     foreach (ScannedProductsDataSet.ScannedBarcodesRow row in ScannedProducts.ScannedBarcodes)
                     {
                         if (row.DocType == _docType &&
                             row.DocId == _docId)
                             rows.Add(row);
                     }
                     return rows.ToArray();
                 }
                 else
                 {
                     OpenScanned();
                     //проверить что данные были загружены
                     if (ScannedProducts.ScannedBarcodes.Rows.Count > 0)
                         return FindByDocIdAndDocType(_docId, _docType);
                     else
                         return rows.ToArray();
                 }
                 //return rows.ToArray();

                 //DoScanEvents = true;
                 #region old action
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
                 #endregion

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
            if (ScannedProducts.ScannedBarcodes.Rows.Count > 0)
            {
                foreach (ScannedProductsDataSet.ScannedBarcodesRow row in ScannedProducts.ScannedBarcodes)
                {
                    if (row.DocType == DocType &&
                        row.DocId == DocId &&
                        row.Barcode == Barcode)
                        return row;
                }
            }
            else
            {
                OpenScanned();
                if (ScannedProducts.ScannedBarcodes.Rows.Count > 0)
                    return FindByBarcodeDocTypeDocId(Barcode, DocType, DocId);
                else
                    return null;
            }
            return null;

            #region old action
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
            #endregion
        }

        public ScannedProductsDataSet.ScannedBarcodesRow[]
            FindByBarcodeDocType(long Barcode, byte DocType)
        {
            System.Collections.Generic.List<ScannedProductsDataSet.ScannedBarcodesRow> foundedRow =
                new List<ScannedProductsDataSet.ScannedBarcodesRow>();

            if (ScannedProducts.ScannedBarcodes.Rows.Count > 0)
            {
                foreach (ScannedProductsDataSet.ScannedBarcodesRow row in ScannedProducts.ScannedBarcodes)
                {
                    if (row.DocType == DocType &&
                        row.Barcode == Barcode)
                        foundedRow.Add(row);
                }
            }
            else
            {
                OpenScanned();
                if (ScannedProducts.ScannedBarcodes.Rows.Count > 0)
                    return FindByBarcodeDocType(Barcode, DocType);
                
            }
            return foundedRow.ToArray();
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
            return FindOpenInventar(InventarFormMode.DefaultInventar);
        }


        public string FindOpenInventar(InventarFormMode mode)
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
            //DoScanEvents = false;
            //try
            //{
              //  if (!System.IO.File.Exists(DatabaseFile))
                    //System.IO.Path.Combine(Program.Default.DatabaseStoragePath,"scannedbarcodes.txt")))
              //      return string.Empty;
            byte docCloseType = (mode == InventarFormMode.DefaultInventar) ?
              (byte)TSDUtils.ActionCode.CloseInventar :
              (byte)TSDUtils.ActionCode.CloseIncome;

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                OpenScanned();


                List<long> openedDocs = new List<long>();

            foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
                _scannedProducts.ScannedBarcodes)
            {
                if (//r.DocId == docId &&
                    r.DocType == docCloseType//((byte)TSDUtils.ActionCode.CloseInventar) //&&r.Priority == 255
                    //r.Barcode == long.Parse(barcode)
                    //r.ScannedDate == date
                    )
                {
                    if (r.Priority == 255)
                    {
                        if (openedDocs.Contains(r.Barcode))
                        {
                            openedDocs.Remove(r.Barcode);
                            //return false;
                        }
                    }
                    else
                        openedDocs.Add(r.Barcode);
                        /*
                    if (r.DocType == ((byte)TSDUtils.ActionCode.CloseInventar) &&
                        r.Priority == 0 &&
                        r.DocId == docId)
                        openedDocs.Add(r.Barcode);
                         * */
                }


            }

            if (openedDocs.Count > 0)
                return openedDocs[0].ToString();
            else
                return string.Empty;

                /*
                using (System.IO.FileStream fs =
    new System.IO.FileStream(DatabaseFile,
        System.IO.FileMode.Open,
        System.IO.FileAccess.ReadWrite,
        System.IO.FileShare.ReadWrite))
                {
                    byte[] arrayOfBytes =
                        new byte[fs.Length];
                    if (fs.Length == 0)
                        return string.Empty;

                    fs.Read(arrayOfBytes, 0, arrayOfBytes.Length);
                    string allFile = System.Text.Encoding.UTF8.GetString(arrayOfBytes, 0, arrayOfBytes.Length);

                    string[] allLinesOfFile = allFile.Split('\n');



                    foreach (string s in allLinesOfFile)
                    {
                        if (String.IsNullOrEmpty(s))
                            continue;

                        try
                        {
                            string[] strAr = s.Split('|');

                            if (strAr.Length > 0)
                            {
                                if (strAr[0].StartsWith("660"))
                                {
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
                                }
                            }
                        }
                        catch (FormatException fexc)
                        {
                            BTPrintClass.PrintClass.SetErrorEvent(fexc.ToString() + "\n" + s);
                        }
                    }
                */
                //}

                #region old action

                //найти все открытые и закрытые инв-ции
                //using (System.IO.StreamReader wr =
                //new System.IO.StreamReader(
                //    System.IO.Path.Combine(
                //        Program.Default.DatabaseStoragePath,
                //        "scannedbarcodes.txt"), true))
                //{
                //    string s = string.Empty;
                //    while ((s = wr.ReadLine()) != null)
                //    {
                //        try
                //        {
                //            string[] strAr = s.Split('|');

                //            if (strAr.Length > 0)
                //            {
                //                /*
                //                ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                //                ActionsClass.Action.AddScannedRow(
                //                long.Parse(strAr[0]),
                //                byte.Parse(strAr[2]),
                //                strAr[1],
                //                int.Parse(strAr[3]),
                //                byte.Parse(strAr[6]));

                //                scannedRow.FactQuantity += int.Parse(strAr[3]);
                //                */
                //                //string openedDocId=string.Empty;

                //                if (strAr[0].StartsWith("660"))
                //                {


                //                    //if (scannedRow == null)
                //                    //{
                //                    //    return 
                //                    //    //r = new ScannedProductsDataSet.ScannedBarcodesRow[1];
                //                    //    //r[0] = scannedRow;
                //                    //}

                //                    //for (int i = 0; i < r.Length; i++)
                //                    //{

                //                    if (strAr[2] == ((byte)TSDUtils.ActionCode.CloseInventar).ToString() &&
                //                        strAr[6] == "255")
                //                    {
                //                        if (openedDocs.Contains(strAr[0]))
                //                        {
                //                            openedDocs.Remove(strAr[0]);
                //                        }
                //                    }

                //                    if (strAr[2] == ((byte)TSDUtils.ActionCode.CloseInventar).ToString() &&
                //                        strAr[6] == "0")
                //                        openedDocs.Add(strAr[0]);
                //                    //openedDocId = strAr[0];

                //                    //return strAr[0];
                //                }
                //            }
                //        }
                //        catch (FormatException fexc)
                //        {
                //            BTPrintClass.PrintClass.SetErrorEvent(fexc.ToString() + "\n" + s);
                //        }
                //    }
                //    if (openedDocs.Count > 0)
                //        return openedDocs[0];
                //    else
                //        return string.Empty;
                    
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
                //}
#endregion
            //}
            //finally
            //{
                //DoScanEvents = true;
            //}
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

        /// <summary>
        /// Сгенерировать новый шк короба
        /// </summary>
        /// <returns></returns>
        public long GenerateNewBarcodeReturnBoxRnd()
        {
            string bc;
            string dt = (DateTime.Today.Year * 10000 +
                DateTime.Today.Month * 100 +
                DateTime.Today.Day).ToString();


            string randomValue = randovValueGenerator.Next(100).ToString("0#");
            bc = "16" + dt + randomValue;
            byte resultValue = 0;

            for (int i = 0; i < bc.Length; i++)
            {
                resultValue = (byte)(resultValue + 
                    byte.Parse(bc[i].ToString())
                    * ((i % 2 == 0) ? 3 : 1));

            }
            byte resultDigit = (byte)((10 - (resultValue % 10)) % 10);
            bc = bc + resultDigit.ToString();
            
            return (long)(long.Parse(bc) * 10 + resultDigit);

        }

        /// <summary>
        /// Сгенерировать новый шк короба
        /// </summary>
        /// <returns></returns>
        public long GenerateNewBarcodeReturnBox()
        {
            string bc;
            string dt = (DateTime.Today.Year * 10000 +
                DateTime.Today.Month * 100 +
                DateTime.Today.Day).ToString();
            long[] allReturnBox = ActionsClass.Action.FindAllReturnBox();
            int qty = 0;
            
            if (allReturnBox != null ||
                allReturnBox.Length > 0)
            {
                qty = allReturnBox.Length;
            }

            string randomValue = qty.ToString("0#");
            bc = "16" + dt + randomValue;
            byte resultValue = 0;

            for (int i = 0; i < bc.Length; i++)
            {
                resultValue = (byte)(resultValue +
                    byte.Parse(bc[i].ToString())
                    * ((i % 2 == 0) ? 3 : 1));

            }
            byte resultDigit = (byte)((10 - (resultValue % 10)) % 10);
            bc = bc + resultDigit.ToString();

            return (long)(long.Parse(bc) * 10 + resultDigit);

        }


        public ProductsDataSet.DocsTblRow CreateNewReturnBox(string returnNumber)
        {
            string boxBarcode = GenerateNewBarcodeReturnBox().ToString();
            return CreateNewReturnBox(boxBarcode, returnNumber);
        }
        /// <summary>
        /// создать новую запись короба
        /// </summary>
        /// <param name="boxBarcode">ШК короба</param>
        /// <param name="returnNumber">№возврата</param>
        /// <returns>строка нового типа документа</returns>
        public ProductsDataSet.DocsTblRow CreateNewReturnBox(string boxBarcode, string returnNumber)
        {
            try
            {
                DoScanEvents = false;

                ScannedProductsDataSet.ScannedBarcodesRow r1 =
                    AddScannedRow(long.Parse(boxBarcode),
                    ((byte)TSDUtils.ActionCode.BoxReturns)
                    , returnNumber,
                    0, 0);
                
                ActualBoxReturnsDocRow = 
                        Products.DocsTbl.AddDocsTblRow(
                            boxBarcode,
                            boxBarcode,
                            ((byte)TSDUtils.ActionCode.BoxReturns),
                            0,
                            0,
                            DateTime.Now,
                            ((byte)TSDUtils.ActionCode.BoxReturns),
                            ((byte)TSDUtils.ActionCode.BoxReturns),
                            ((byte)TSDUtils.ActionCode.BoxReturns),
                            returnNumber,
                            string.Empty,
                            string.Empty);

                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            boxBarcode,
                            returnNumber,
                            ((byte)TSDUtils.ActionCode.BoxReturns),
                            0,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            0,
                            0
                            );

                byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                //byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, '\n'));
                //byte[] buff = new byte[] { 0xD, 0xA };
                writer.Write(buff, 0, buff.Length);
                writer.Flush();


            }
            finally
            {
                DoScanEvents = true;
            }
            return ActualBoxReturnsDocRow;
            //return GetActualBoxReturnsDocRow(boxBarcode, returnNumber);
        }

        /// <summary>
        /// Закрытыие короба
        /// </summary>
        /// <param name="boxBarcode">ШК короба</param>
        /// <param name="returnNumber">Номер возврата</param>
        public void CloseReturnBoxAction(string boxBarcode, string returnNumber)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] scannedrow =
           _scannedProducts.ScannedBarcodes.FindByDocIdAndDocType(boxBarcode,
               (byte)TSDUtils.ActionCode.CloseBoxReturns);

            if (scannedrow == null ||
                scannedrow.Length == 0)
            {
                ScannedProductsDataSet.ScannedBarcodesRow r1 =
                   AddScannedRow(long.Parse(boxBarcode),
                   ((byte)TSDUtils.ActionCode.CloseBoxReturns)
                   , returnNumber
                   ,0
                   , byte.MaxValue);
            }
            else
            {
                //for (int i = 0; i < scannedrow.Length; i++)
                //{
                //    scannedrow[i].Priority = byte.MaxValue;
                //}
                throw new ApplicationException(string.Format(
                    "Короб {0} уже закрыт",boxBarcode));
            }

            {
                string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            boxBarcode,
                            returnNumber,
                            ((byte)TSDUtils.ActionCode.CloseBoxReturns),
                            0,
                            DateTime.Today.ToString("dd.MM.yyyy"),
                            Program.Default.TerminalID,
                            byte.MaxValue,
                            0
                            );
                byte[] buff = System.Text.Encoding.UTF8.GetBytes(string.Concat(s, "\r\n"));
                writer.Write(buff, 0, buff.Length);
                writer.Flush();

                ActualBoxReturnsDocRow = null;
            }
        }

        public string CheckOpenedReturnBoxAndGo(string returnNumber)
        {
            string FirstOpenedReturnBox = FindFirstOpenedReturnBox(returnNumber);
            if (String.IsNullOrEmpty(FirstOpenedReturnBox))
            {
                int totalScanByBarcode = CalculateTotalShkToScanByRetnNumber(returnNumber);
                int qty_scanned = 0;
                long[] retnBox = FindAllReturnBoxByRetnNumber(returnNumber, out qty_scanned);
                if (totalScanByBarcode > qty_scanned)
                    FirstOpenedReturnBox = GenerateNewBarcodeReturnBox().ToString();
                else
                    throw new ApplicationException("Уже все отканировано");
                //ProductsDataSet.DocsTblRow r = CreateNewReturnBox(returnNumber);
            }
            
            return FirstOpenedReturnBox;

        }
        /// <summary>
        /// Проверка что короб открыт
        /// </summary>
        /// <param name="BoxBarcode">ШК короба</param>
        /// <returns>если есть открытый короб - возвращает его ШЕ, если нет - пустая строка</returns>
        public string CheckOpenedReturnBox(string BoxBarcode)
        {

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                OpenScanned();


            List<long> openedDocs = new List<long>();

            //foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
            //    _scannedProducts.ScannedBarcodes)
            for (int i=0;i<_scannedProducts.ScannedBarcodes.Rows.Count;i++)
            {
                ScannedProductsDataSet.ScannedBarcodesRow r =
                    (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns)
                    && r.Barcode == long.Parse(BoxBarcode)
                    )
                {
                    openedDocs.Add(r.Barcode);
                }
                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.CloseBoxReturns)
                    && r.Barcode == long.Parse(BoxBarcode)
                    )
                {
                    openedDocs.Remove(r.Barcode);
                }


            }

            if (openedDocs.Count > 0)
                return openedDocs[0].ToString();
            else
                return string.Empty;
        }
        
        /// <summary>
        /// Первый открытый короб
        /// </summary>
        /// <param name="returnNumber">Номер возврата</param>
        /// <returns>ШК короба если есть или пустая строка если нет</returns>
        public string FindFirstOpenedReturnBox(string returnNumber)
        {

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                OpenScanned();


            List<long> openedDocs = new List<long>();

            //foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
            //    _scannedProducts.ScannedBarcodes)
            for (int i = 0; i < _scannedProducts.ScannedBarcodes.Rows.Count; i++)
            {
                ScannedProductsDataSet.ScannedBarcodesRow r =
                    (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns)
                    && r.DocId == returnNumber
                    )
                {
                    openedDocs.Add(r.Barcode);
                }
                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.CloseBoxReturns)
                    && r.DocId == returnNumber
                    )
                {
                    openedDocs.Remove(r.Barcode);
                }


            }

            if (openedDocs.Count > 0)
                return openedDocs[0].ToString();
            else
                return string.Empty;
        }

        /// <summary>
        /// Любой Первый открытый короб
        /// </summary>
        /// <returns></returns>
        public string FindFirstOpenedReturnBox()
        {
            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                OpenScanned();


            List<long> openedDocs = new List<long>();

            //foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
            //    _scannedProducts.ScannedBarcodes)
            for (int i = 0; i < _scannedProducts.ScannedBarcodes.Rows.Count; i++)
            {
                ScannedProductsDataSet.ScannedBarcodesRow r =
                    (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns)
                    )
                {
                    openedDocs.Add(r.Barcode);
                }
                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.CloseBoxReturns)
                    )
                {
                    openedDocs.Remove(r.Barcode);
                }


            }

            if (openedDocs.Count > 0)
                return openedDocs[0].ToString();
            else
                return string.Empty;
        }

        /// <summary>
        /// список всех откр коробов
        /// </summary>
        /// <param name="returnNumber">номер возврата</param>
        /// <returns>массив ШК коробов</returns>
        public long[] FindAllOpenedReturnBox(string returnNumber)
        {

            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                OpenScanned();


            List<long> openedDocs = new List<long>();

            //foreach (ScannedProductsDataSet.ScannedBarcodesRow r in
            //    _scannedProducts.ScannedBarcodes)
            for (int i = 0; i < _scannedProducts.ScannedBarcodes.Rows.Count; i++)
            {
                ScannedProductsDataSet.ScannedBarcodesRow r =
                    (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns)
                    && r.DocId == returnNumber
                    )
                {
                    openedDocs.Add(r.Barcode);
                }
                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.CloseBoxReturns)
                    && r.DocId == returnNumber
                    )
                {
                    openedDocs.Remove(r.Barcode);
                }


            }

            return openedDocs.ToArray();
        }

        /// <summary>
        /// Получить список всех коробов по номеру возврата
        /// </summary>
        /// <param name="returnNumber">Номер возврата DocId</param>
        /// <returns>массив ШК коробов</returns>
        public long[] FindAllReturnBox()
    {

        if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            OpenScanned();


        List<long> openedDocs = new List<long>();

        for (int i = 0; i < _scannedProducts.ScannedBarcodes.Rows.Count; i++)
        {
            ScannedProductsDataSet.ScannedBarcodesRow r =
                (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

            if (r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns))
            {
                openedDocs.Add(r.Barcode);
            }
        }

        return openedDocs.ToArray();
    }

        /// <summary>
        /// Получить список всех коробов по номеру возврата
        /// </summary>
        /// <param name="returnNumber">Номер возврата DocId</param>
        /// <returns>массив ШК коробов</returns>
        public long[] FindAllReturnBoxByRetnNumber(string returnNumber, out int qty)
        {
            qty = 0;
            if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                OpenScanned();


            List<long> openedDocs = new List<long>();

            for (int i = 0; i < _scannedProducts.ScannedBarcodes.Rows.Count; i++)
            {
                ScannedProductsDataSet.ScannedBarcodesRow r =
                    (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

                if (
                    r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns)
                    && r.DocId == returnNumber
                    )
                {
                    qty += r.FactQuantity;
                    openedDocs.Add(r.Barcode);
                }
            }

            return openedDocs.ToArray();
        }
        /// <summary>
        /// Получить последний открытый короб
        /// </summary>
        /// <returns></returns>
        public ProductsDataSet.DocsTblRow GetActualBoxReturnsDocRow()
        //string boxBarcode, string returnNumber)
        {
            if (ActualBoxReturnsDocRow == null)
            {

                if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
                    OpenScanned();


                Dictionary<long, ScannedProductsDataSet.ScannedBarcodesRow> openedDocs = new Dictionary<long, ScannedProductsDataSet.ScannedBarcodesRow>();
                //long last_barcode = -1;

                for (int i = 0; i < _scannedProducts.ScannedBarcodes.Rows.Count; i++)
                {
                    ScannedProductsDataSet.ScannedBarcodesRow r =
                        (ScannedProductsDataSet.ScannedBarcodesRow)_scannedProducts.ScannedBarcodes.Rows[i];

                    if (
                        r.DocType == ((byte)TSDUtils.ActionCode.BoxReturns)
                        )
                    {
                        openedDocs.Add(r.Barcode, r);
                        //last_barcode = r.Barcode;
                    }
                    if (
                        r.DocType == ((byte)TSDUtils.ActionCode.CloseBoxReturns)
                        )
                    {
                        openedDocs.Remove(r.Barcode);

                    }


                }

                if (openedDocs.Count > 0)
                {
                    ScannedProductsDataSet.ScannedBarcodesRow[] r_array =
                        new ScannedProductsDataSet.ScannedBarcodesRow[openedDocs.Count];

                    openedDocs.Values.CopyTo(r_array, 0);

                    ActualBoxReturnsDocRow =
                       Products.DocsTbl.AddDocsTblRow(
                           r_array[0].Barcode.ToString(),
                           r_array[0].Barcode.ToString(),
                           ((byte)TSDUtils.ActionCode.BoxReturns),
                           0,
                           0,
                           DateTime.Now,
                           ((byte)TSDUtils.ActionCode.BoxReturns),
                           ((byte)TSDUtils.ActionCode.BoxReturns),
                           ((byte)TSDUtils.ActionCode.BoxReturns),
                           r_array[0].DocId,
                           string.Empty,
                           string.Empty);
                    return ActualBoxReturnsDocRow;
                }
                else
                    return null;


                //    ActualBoxReturnsDocRow = 
                //        GetDataByNavcodeDocIdAndType
                //        (
                //            boxBarcode,
                //            boxBarcode,
                //            (byte)((byte)TSDUtils.ActionCode.BoxReturns));

                //    if (ActualBoxReturnsDocRow != null)
                //        return ActualBoxReturnsDocRow;
                //    else
                //    {
                //        ActualBoxReturnsDocRow = 
                //            Products.DocsTbl.AddDocsTblRow(
                //                boxBarcode,
                //                boxBarcode,
                //                ((byte)TSDUtils.ActionCode.BoxReturns),
                //                0,
                //                0,
                //                DateTime.Now,
                //                ((byte)TSDUtils.ActionCode.BoxReturns),
                //                ((byte)TSDUtils.ActionCode.BoxReturns),
                //                ((byte)TSDUtils.ActionCode.BoxReturns),
                //                returnNumber,
                //                string.Empty,
                //                string.Empty);

                //        return ActualBoxReturnsDocRow;
                //    }
                //}
                //else
                //return ActualBoxReturnsDocRow;

                //string Navcode, string DocId, byte docType)
                //this.columnNavCode.MaxLength = 13;
                //this.columnDocId.AllowDBNull = false;
                //this.columnDocId.MaxLength = 20;
                //this.columnDocType.AllowDBNull = false;
                //this.columnText1.MaxLength = 20;
                //this.columnText2.MaxLength = 20;
                //this.columnText3.MaxLength = 20;
                //ProductsDataSet.DocsTblRow docsRow = 
                //    Products.DocsTbl.AddDocsTblRow(

            }
            else
                return ActualBoxReturnsDocRow;
        }

        /// <summary>
        /// поиск всех возвратов по ШК
        /// </summary>
        /// <param name="barcode">ШК товароа по которому искать возврат</param>
        /// <returns></returns>
        public ProductsDataSet.DocsTblRow[] FindAllReturnsByProduct(string barcode)
        {
            ProductsDataSet.ProductsTblRow productRow = productsTa.GetDataByBarcode(long.Parse(barcode));
            if (productRow != null)
            {
                ProductsDataSet.DocsTblRow[] arrofdocs = ActionsClass.Action.GetDataByNavCodeAndType(productRow.NavCode,
                    (byte)TSDUtils.ActionCode.Returns);
                return arrofdocs;
            }
            else
                throw new ApplicationException("Не найдено возвратов");

        }

        /// <summary>
        /// Считает кол-во ШК для сканирования
        /// </summary>
        /// <param name="retnNumber">Номер возврата</param>
        /// <returns>Возвр кол-во ШК для сканирования</returns>
        public int CalculateTotalShkToScanByRetnNumber(string retnNumber)
        {
            ProductsDataSet.DocsTblRow[] arrofdocs = ActionsClass.Action.GetDataByDocIdAndType(retnNumber,
                    (byte)TSDUtils.ActionCode.Returns);
            //Dictionary<string, int> retnDocsToScan = new Dictionary<string,int>();
            //Dictionary<string, long> retnBoxDocs = new Dictionary<string, long>();
            //Dictionary<long, int> scannedQtyDict = new Dictionary<long, int>();
            int totalScanByBarcode = 0;
            //int qty_scanned = 0;
            //1. Посчитать всего по сканированным
            //2. Посчитать всего по "плану"
            //3. записать (сделать) массив по плану по 


            //считаем всего по плану по №возврата
            if (arrofdocs == null ||
                arrofdocs.Length == 0)
            {
                return 0;
            }
            else
            {
                for (int i = 0; i < arrofdocs.Length; i++)
                {
                    totalScanByBarcode += arrofdocs[i].Quantity;
                    
                }
                //_scannedRetnBoxWQty.SetRetnQty(retnNumber, totalScanByBarcode);
            }
            return totalScanByBarcode;

            ////считаем отсканированные по номеру возврата
            //if (
            //    ScannedProducts == null ||
            //    ScannedProducts.ScannedBarcodes == null ||
            //    ScannedProducts.ScannedBarcodes.Rows.Count == 0)
            //{
                

            //    OpenScanned();
            //}

            //if (_scannedProducts.ScannedBarcodes.Rows.Count == 0)
            //{
            //    return totalScanByBarcode;
            //}
            //else
            //{
            //    //int qty = 0;
            //    long[] retnBox = FindAllReturnBoxByRetnNumber(retnNumber, out qty_scanned);
            //    return totalScanByBarcode - qty_scanned;

            //}



        }
    }

    public class ScannedRetnBoxWQty
    {
        /// <summary>
        /// Номер возврата
        /// </summary>
        string retnDocsToScanNumber;
        
        /// <summary>
        /// Список коробов возврата
        /// </summary>
        List<long> boxes = new List<long>();

        /// <summary>
        /// Кол-во уже отскан коробов
        /// </summary>
        Dictionary<long, int> scannedQtyDict = new Dictionary<long, int>();

        /// <summary>
        /// Кол-во оставшихся сканирований для возврата
        /// </summary>
        int retnDocsDeltaToScan;


        public ScannedRetnBoxWQty()
        {

        }

        public void AddNewRetn(string retnNumber, int qty)
        {
            retnDocsToScanNumber = retnNumber;
            retnDocsDeltaToScan = qty;

        }


        public void SetRetnQty(string retnNumber, int qty)
        {
            AddNewRetn(retnNumber, qty);
        }

        public void AddNewBoxRetn(long box_id)
        {
            if (boxes.Contains(box_id))
                return;
            else boxes.Add(box_id);

        }


        /*public void SetBoxRetn(string retnNumber, long box_id, int qty)
        {
            AddNewBoxRetn(retnNumber, box_id, qty);
        }

        public int GetRetnQty(string retnNumber)
        {
            if (retnDocsToScan.ContainsKey(retnNumber))
                return retnDocsToScan[retnNumber];
            else
                return 0;
        }

        public int GetBoxRetn(string retnNumber, long box_id)
        {
            if (retnBoxScan.ContainsKey(retnNumber))
            {
                if (scannedQtyDict.ContainsKey(box_id))
                {
                    return scannedQtyDict[box_id];
                }
                else
                    return 0; ;
            }
            else
            {
                return 0;
            }
        }*/

    }

}