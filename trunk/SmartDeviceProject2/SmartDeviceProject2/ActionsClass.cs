using System;

using System.Collections.Generic;
using System.Text;

namespace Familia.TSDClient
{
    public class ActionsClass
    {
        ScannedProductsDataSet _scannedProducts = new ScannedProductsDataSet();

        public ScannedProductsDataSet ScannedProducts
        {
            get { return _scannedProducts; }
            set { _scannedProducts = value; }
        }
        Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA;
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
            btPrint = BTPrintClass.PrintClass;
            dateFormat.ShortDatePattern = "dd.MM.yyyy";
            dateFormat.DateSeparator = ".";
            nfi.NumberDecimalSeparator = ".";

            actionsDict.Add(TSDUtils.ActionCode.NoAction, new ActOnProduct(NoActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Reprice, new ActOnProduct(RepriceActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Returns, new ActOnProduct(ReturnActionProc));
            actionsDict.Add(TSDUtils.ActionCode.Remove, new ActOnProduct(RemoveActionProc));
            actionsDict.Add(TSDUtils.ActionCode.QuickHelp, new ActOnProduct(RemoveActionProc));
            actionsDict.Add(TSDUtils.ActionCode.InventoryGlobal, new ActOnProduct(InventoryGlobalActionProc));
            actionsDict.Add(TSDUtils.ActionCode.InventoryLocal, new ActOnProduct(InventoryLocalActionProc));
            actionsDict.Add(TSDUtils.ActionCode.NotFound, new ActOnProduct(NotFoundActionProc));
            actionsDict.Add(TSDUtils.ActionCode.DocNotFound, new ActOnProduct(DocNotFoundActionProc));
            

            tmr = new System.Threading.Timer(
            new System.Threading.TimerCallback(OnTimer)
            , null,
            System.Threading.Timeout.Infinite,
            System.Threading.Timeout.Infinite);

            scannedTA =
                    new Familia.TSDClient.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(_scannedProducts);

            
                
        }

        public void BeginScan()
        {
            tmr.Change(1000, 60000);
        }
        public void EndScan()
        {
             tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            scannedTA.Update(this._scannedProducts);
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
                    }
                    catch { }
                }
                PlaySound(soundCode);
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
                    }
                    catch { }
                }
                PlayVibro(vibroCode);
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

        public void PrintLabel(ProductsDataSet.ProductsTblRow datarow,  ProductsDataSet.DocsTblRow docRow, uint shablonCode)
        {
            try
            {
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
                if (bArray == null)
                    return;

                fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);
                btPrint.SetStatusEvent(fileContent);

                byte[] bArray2 = ReplaceAttr(bArray, datarow, docRow);
                fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
                btPrint.SetStatusEvent(fileContent);
                //return;


                if (btPrint.Connected)
                {
                    btPrint.Print(/*fileContent*/bArray2);
                }
                else
                {

                    btPrint.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                    if (btPrint.Connected)
                    {
                        btPrint.Print(/*fileContent*/bArray2);
                    }

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
                        }

                    }
                }
                catch { }
            }
            catch (Exception err)
            {
                BTPrintClass.PrintClass.SetErrorEvent(err.ToString());
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
                                    _scannedProducts.ScannedBarcodes.FindByBarcodeDocTypeDocId(productsRow.Barcode, docsRow.DocType, docsRow.DocId); 
                            }
                    string atrCode = atrName.Replace(attrString, "");
                    int colId = -1;
                    //try
                    //{
                        colId = int.Parse(atrCode) - 1;
                        if (colId >= datarow.Table.Columns.Count)
                            continue;
                        //atrName = "Test";
                        if (datarow.Table.Columns[colId].DataType == typeof(string) ||
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
                                    ((DateTime)datarow[colId]).ToString(dateFormat));
                            }
                            else
                                if (datarow.Table.Columns[colId].DataType == typeof(Single))
                                {
                                    bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                        ((Single)datarow[colId]).ToString("### ###.00"));
                                }
                                else
                                bArrTmp =TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                    datarow[colId].ToString());

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
            PrintLabelAsync(datarow, docsRow);
            PlayVibroAsyncAction(docsRow);
            PlaySoundAsyncAction(docsRow);
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
                    if (r[i].FactQuantity < r[i].PlanQuanity)
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
                        r[i].FactQuantity += 1;
                        PlayVibroAsyncAction(docsRow);
                        PlaySoundAsyncAction(docsRow);
                        PrintLabelAsync(datarow, docsRow);
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
            ScannedProductsDataSet.ScannedBarcodesRow[] r =
                _scannedProducts.ScannedBarcodes.FindByBarcodeAndDocType(datarow.Barcode, docsRow.DocType);
            if (r != null)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    
                        r[i].FactQuantity += 1;
                        PlayVibroAsyncAction(docsRow);
                        PlaySoundAsyncAction(docsRow);
                        PrintLabelAsync(datarow, docsRow);
                        if (OnActionCompleted != null)
                            OnActionCompleted(docsRow, r[i]);
                        break;
                    

                }
            }
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
                        r[i].FactQuantity += 1;
                        PlayVibroAsyncAction(docsRow);
                        PlaySoundAsyncAction(docsRow);
                        PrintLabelAsync(datarow, docsRow);
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
            try
            {
                ViewProductForm._mEvt.Reset();
                scannedTA.Update(this._scannedProducts);
            }
            finally
            {
                ViewProductForm._mEvt.Set();
            }
        }
    }
}
