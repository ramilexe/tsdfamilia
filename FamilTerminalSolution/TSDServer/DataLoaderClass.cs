using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace TSDServer
{
    public class DataLoaderClass:IDisposable
    {
        private ProductsDataSet productsDataSet1;
        TSDServer.ScannedProductsDataSet scannedDs;
        bool _disposed = false;

        public bool Processing
        {
            get
            {
                if (loadThread != null)
                    return true;
                else
                    return false;
            }
        }
        System.Globalization.DateTimeFormatInfo dateFormat =
               new System.Globalization.DateTimeFormatInfo();

        System.Globalization.NumberFormatInfo nfi =
                new System.Globalization.NumberFormatInfo();
        int rowCounter = 0;
        string[] cols = null;
        int[] colsLength = null;
        DateTime BaseDate = Properties.Settings.Default.BaseDate;

        char fieldDelimeter = Properties.Settings.Default.FieldDelimeter[0];
        public delegate void AddStringDelegate(string source);

        public delegate void StartImport(string fileName);
        public delegate void ProcessImport(string Message, bool hasError);
        public delegate void FinishImport(string fileName);
        public delegate void FailedImport(string message);

        public event ProcessImport OnProcessImport;
        public event FinishImport OnFinishImport;
        public event FailedImport OnFailedImport;
        private System.Threading.Thread loadThread = null;
        private bool Cancelled = false;

        public enum ImportModeEnum { Undefined = 0, Products = 1, Documents = 2 };
        ImportModeEnum currentImportMode = ImportModeEnum.Undefined;
        ProductsDataSetTableAdapters.ProductsTblTableAdapter productAdapter;
        ProductsDataSetTableAdapters.DocsTblTableAdapter docsAdapter;
        TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA;


        //TSDUtils.CustomEncodingClass CustomEncodingClass MyEncoder = new CustomEncodingClass();
        public DataLoaderClass()
        {
            
            productsDataSet1 = new ProductsDataSet();
            productAdapter = new TSDServer.ProductsDataSetTableAdapters.ProductsTblTableAdapter(productsDataSet1);
            docsAdapter = new TSDServer.ProductsDataSetTableAdapters.DocsTblTableAdapter(productsDataSet1);
            scannedDs = new TSDServer.ScannedProductsDataSet();
            scannedTA = new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(scannedDs);
            SetFormats();
        }
        void SetFormats()
        {
            cols = new string[productsDataSet1.ProductsBinTbl.Columns.Count];
            dateFormat.ShortDatePattern = Properties.Settings.Default.ShortDatePattern;
            dateFormat.DateSeparator = Properties.Settings.Default.DateSeparator;
            nfi.NumberDecimalSeparator = Properties.Settings.Default.NumberDecimalSeparator;

            string[] colsLengthStr =
                Properties.Settings.Default.FieldsLength.Split(';');

            colsLength = new int[colsLengthStr.Length];

            for (int i = 0; i < colsLengthStr.Length; i++)
            {
                colsLength[i] = int.Parse(colsLengthStr[i]);
            }

        }

        public void AutoLoadDoc(string fileName)
        {
            LoadFile(fileName, ImportModeEnum.Documents);
        }
        public void Cancel()
        {
            this.Cancelled = true;
        }

        private void LoadFile(string fileName, ImportModeEnum _currentImportMode)
        {
            if (loadThread == null)
            {

                Cancelled = false;
                currentImportMode = _currentImportMode;

                //richTextBox1.Text = "";
                //toolStripStatusLabel2.Text = "";
                //currentImportMode = ImportModeEnum.Products;
                OnProcessImport += new ProcessImport(DataLoaderClass_OnProcessImport);
                OnFinishImport += new FinishImport(DataLoaderClass_OnFinishImport);
                OnFailedImport += new FailedImport(DataLoaderClass_OnFailedImport);
                loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                loadThread.Start(fileName);
            }
            else
                throw new ApplicationException("Processing");
            //if (currentImportMode == ImportModeEnum.Documents)
            //{
            //    stopGoodBtn.Enabled = true;
            //    stopDocsBtn.Enabled = false;
            //}
            //else
            //    if (currentImportMode == ImportModeEnum.Products)
            //    {                this.importGoodBtn.Enabled = false;
                //this.importDocBtn.Enabled = false;
                //this.uploadBtn.Enabled = false;
                //this.downloadBtn.Enabled = false;
                //this.settingsBtn.Enabled = false;
            //        stopGoodBtn.Enabled = false;
            //        stopDocsBtn.Enabled = true;
            //    }
            //this.importGoodBtn.Enabled = false;
            //this.importDocBtn.Enabled = false;
            //this.uploadBtn.Enabled = false;
            //this.downloadBtn.Enabled = false;
            //this.settingsBtn.Enabled = false;
        }

        void DataLoaderClass_OnFailedImport(string message)
        {
            
        }

        void DataLoaderClass_OnFinishImport(string fileName)
        {
            //Cancelled = false;
            loadThread = null;
        }

        void DataLoaderClass_OnProcessImport(string Message, bool hasError)
        {
            
        }
        public void AutoLoadProduct(string fileName)
        {
            LoadFile(fileName, ImportModeEnum.Products);
        }

        private void BeginImport(object _fileName)
        {

            string fileName = _fileName.ToString();
            try
            {
                bool IsFileFixed = Properties.Settings.Default.ImportFileTypeIsFixed;
                AddStringDelegate del = null;
                if (currentImportMode == ImportModeEnum.Products)
                {
                    this.productsDataSet1.CleanProducts();
                    if (IsFileFixed)
                        del = new AddStringDelegate(AddFixedStringProducts);
                    else
                        del = new AddStringDelegate(AddDelimetedStringProducts);
                }
                else
                {
                    if (currentImportMode == ImportModeEnum.Documents)
                    {
                        this.productsDataSet1.CleanDocs();
                        if (IsFileFixed)
                            del = new AddStringDelegate(AddFixedStringDocs);
                        else
                            del = new AddStringDelegate(AddDelimetedStringDocs);
                    }
                    else
                        return;
                }

                rowCounter = 0;
                using (System.IO.StreamReader rdr =
                    new System.IO.StreamReader(fileName, Encoding.GetEncoding("windows-1251")))
                {

                    string s = string.Empty;
                    while ((s = rdr.ReadLine()) != null)
                    {
                        if (Cancelled)
                            return;
                        del.Invoke(s);

                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                if (OnFailedImport != null)
                    OnFailedImport("Загрузка отменена... ");
            }
            catch (Exception err)
            {
                if (OnFailedImport != null)
                    OnFailedImport("Ошибка: " + err.Message);
                //MessageBox.Show("Ошибка: " + err.Message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                Cancelled = false;
                try
                {
                    if (currentImportMode == ImportModeEnum.Products)
                    {
                        productAdapter.Update(this.productsDataSet1);
                    }

                    if (currentImportMode == ImportModeEnum.Documents)
                    {
                        docsAdapter.Update(this.productsDataSet1);
                    }

                    this.productsDataSet1.AcceptChanges();
                }
                catch { }

                if (OnFinishImport != null)
                    OnFinishImport("Загрузка завершена...");
            }
        }

        private void ParseColumn(int i, DataRow row)
        {
            if (row.Table.Columns[i].DataType ==
                        typeof(System.Byte[]))
            {
                if (cols[i].Trim() != string.Empty)
                {
                    //using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    //{
                    // Use the newly created memory stream for the compressed data.
                    //using (stream = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress))
                    //{


                    //byte[] buffer /*= System.Text.Encoding.GetBytes(cols[i].Trim());
                    //buffer */= System.Text.Encoding.Unicode.GetBytes(cols[i].Trim());
                    row[i] = TSDUtils.CustomEncodingClass.Encoding.GetBytes(cols[i].Trim());//buffer;
                    //byte[] newBuff = Encoding.Convert(Encoding.Default, Encoding.ASCII, buffer);
                    /*stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    stream.Close();
                    byte[] outBuffer = ms.ToArray();*/
                    //row[i] = Compressor.Compress(buffer);



                    //}
                    //}


                }
                return;
            }
            if (!String.IsNullOrEmpty(cols[i].Trim()))
            {
                if (row.Table.Columns[i].DataType ==
                    typeof(string))
                {
                    row[i] = cols[i].Trim();
                    return;
                    //continue;
                }
                if (row.Table.Columns[i].DataType ==
                    typeof(System.Int64)
                    )
                {
                    row[i] = System.Int64.Parse(cols[i].Trim());
                    return;
                    //continue;
                }
                if (row.Table.Columns[i].DataType ==
                   typeof(System.Byte))
                {
                    row[i] = System.Byte.Parse(cols[i].Trim());
                    return;
                    //continue;
                }
                if (row.Table.Columns[i].DataType ==
                   typeof(System.Int16))
                {
                    row[i] = System.Int16.Parse(cols[i].Trim());
                    return;
                    //continue;
                }
                if (row.Table.Columns[i].DataType ==
                   typeof(System.Int32))
                {
                    row[i] = System.Int32.Parse(cols[i].Trim());
                    return;
                    //continue;
                }
                if (row.Table.Columns[i].DataType ==
                   typeof(System.Single))
                {
                    row[i] = System.Single.Parse(cols[i].Trim(), nfi);
                    return;
                    //continue;
                }


                if (row.Table.Columns[i].DataType ==
                    typeof(DateTime))
                {
                    row[i] = DateTime.Parse(cols[i].Trim(), dateFormat);
                    return;
                    //continue;
                }

                if (row.Table.Columns[i].DataType ==
                   typeof(decimal))
                {
                    row[i] = Decimal.Parse(cols[i].Trim(), nfi);
                    return;
                    //continue;
                }
            }
        }
        private void AddFixedStringProducts(string s)
        {

            cols = new string[productsDataSet1.ProductsTbl.Columns.Count];

            ProductsDataSet.ProductsTblRow row =
                this.productsDataSet1.ProductsTbl.NewProductsTblRow();

            cols[0] = s.Substring(0, colsLength[0]);
            int readedLength = -1;//;colsLength[0];


            if (String.IsNullOrEmpty(cols[0].Trim())
                //|| cols[0].Trim() == "0" 
                //|| cols[0].Trim() == "9999999999999"
                )
            {
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }

            for (int i = 0; i < productsDataSet1.ProductsTbl.Columns.Count; i++)
            {
                try
                {
                    cols[i] = s.Substring(readedLength + 1, colsLength[i]);
                    readedLength = readedLength + colsLength[i] + 1;

                    ParseColumn(i, row);


                }
                catch (Exception err)
                {
                    if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message), true);

                }
            }
            //Test(row);
            this.productsDataSet1.ProductsTbl.AddProductsTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);

        }

        private void AddDelimetedStringProducts(string s)
        {

            cols = s.Split(fieldDelimeter);

            ProductsDataSet.ProductsTblRow row =
                this.productsDataSet1.ProductsTbl.NewProductsTblRow();




            if (String.IsNullOrEmpty(cols[0].Trim())
                //||    cols[0].Trim() == "0" ||
                //    cols[0].Trim() == "9999999999999"
            )
            {
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }


            for (int i = 0; i < productsDataSet1.ProductsTbl.Columns.Count; i++)
            {
                try
                {


                    ParseColumn(i, row);

                }
                catch (Exception err)
                {
                    if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message), true);

                }
            }
            //Test(row);
            this.productsDataSet1.ProductsTbl.AddProductsTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);


        }

        private void AddDelimetedStringDocs(string s)
        {

            cols = s.Split(fieldDelimeter);

            ProductsDataSet.DocsTblRow row =
                this.productsDataSet1.DocsTbl.NewDocsTblRow();

            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }


            for (int i = 0; i < this.productsDataSet1.DocsTbl.Columns.Count; i++)
            {
                try
                {
                    ParseColumn(i, row);

                }
                catch (Exception err)
                {
                    if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message), true);

                }
            }

            //ProductsDataSet.DocsBinTblRow docRow = 
            //    this.productsDataSet1.DocsBinTbl.NewDocsBinTblRow();
            //docRow.Barcode = row.Barcode;
            //docRow.DocId = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
            //    row.DocType.ToString("00")/*тип документа*/+ row.DocId//docid
            //            );
            //docRow.Priority = (System.Int16)(row.Priority | (row.WorkMode << 14));
            //docRow.Quantity = row.Quantity;
            //docRow.RePriceDate = (short)row.RePriceDate.Subtract(BaseDate).Days;
            //docRow.ReturnDate = (short)row.ReturnDate.Subtract(BaseDate).Days;
            ////int shablon = row.LabelCode ;
            ////        shablon = row.LabelCode| (row.MusicCode << 3);
            ////        shablon = (row.LabelCode| (row.MusicCode << 3)) | (row.VibroCode << 6);
            //docRow.Shablon =
            //    (row.LabelCode | (row.MusicCode << 3)) | (row.VibroCode << 6);
            //docRow.Text1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.Text1);
            //docRow.Text2 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.Text2);
            //docRow.Text3 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.Text3);

            this.productsDataSet1.DocsTbl.AddDocsTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);


        }

        private void AddFixedStringDocs(string s)
        {
            cols = new string[productsDataSet1.DocsTbl.Columns.Count];

            ProductsDataSet.DocsTblRow row =
                this.productsDataSet1.DocsTbl.NewDocsTblRow();

            cols[0] = s.Substring(0, colsLength[0]);
            int readedLength = -1;//;colsLength[0];


            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }

            for (int i = 0; i < productsDataSet1.DocsTbl.Columns.Count; i++)
            {
                try
                {
                    cols[i] = s.Substring(readedLength + 1, colsLength[i]);
                    readedLength = readedLength + colsLength[i] + 1;

                    ParseColumn(i, row);


                }
                catch (Exception err)
                {
                    if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message), true);

                }
            }

            //ProductsDataSet.DocsBinTblRow docRow =
            //  this.productsDataSet1.DocsBinTbl.NewDocsBinTblRow();
            this.productsDataSet1.DocsTbl.AddDocsTblRow(row);

            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);
        }

        public string[] ScannedFileList
        {
            get
            {
                return this.scannedTA.FileList;
            }
        }

        public string[] ProductsFileList
        {
            get
            {
                return this.productAdapter.FileList;
            }
        }
        public string[] DocsFileList
        {
            get
            {
                return this.docsAdapter.FileList;
            }
        }
        public void UploadResults()
        {
            
            scannedTA.Fill(scannedDs);

            using (System.IO.StreamWriter wr = new StreamWriter(
                    Path.Combine(
                    Properties.Settings.Default.LocalFilePath,
                    "scannedbarcodes.txt"), false))
            {
                foreach (System.Data.DataRow row1 in
                    scannedDs.ScannedBarcodes.Rows)
                {
                    TSDServer.ScannedProductsDataSet.ScannedBarcodesRow row =
                        (TSDServer.ScannedProductsDataSet.ScannedBarcodesRow)row1;

                    string s =
                        string.Format("{0}|{1}|{2}|{3}|{4}|{5}|", row.Barcode, row.DocId, row.DocType, row.FactQuantity, row.ScannedDate, row.TerminalId);
                    wr.WriteLine(s);
                }
                wr.Flush();
                wr.Close();
            }
            using (System.IO.StreamWriter wr = new StreamWriter(
                Path.Combine(
                Properties.Settings.Default.LocalFilePath,
                "register.txt"), false))
            {
                foreach (System.Data.DataRow row1 in
                    scannedDs.ScannedBarcodes.Rows)
                {
                    TSDServer.ScannedProductsDataSet.ScannedBarcodesRow row =
                        (TSDServer.ScannedProductsDataSet.ScannedBarcodesRow)row1;

                    if (row.DocType == (byte)TSDUtils.ActionCode.Reprice)
                    {
                        //ProductsDataSet.ProductsTblRow prodRow = 
                        //    productsDataSet1.ProductsTbl.FindByBarcode(row.Barcode);
                        //if (prodRow != null)
                        //{
                        string s =
                            string.Format("{0},{1,11:D}, {1,7:D}",
                            row.Barcode,
                            1,
                            //prodRow.NewPrice,
                            row.FactQuantity);
                        wr.WriteLine(s);
                        //}
                    }
                }


                wr.Flush();
                wr.Close();
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_disposed)
            {
                productAdapter.Dispose();
                docsAdapter.Dispose();
                productsDataSet1.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}
