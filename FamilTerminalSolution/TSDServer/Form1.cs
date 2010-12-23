using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace TSDServer
{
    public partial class Form1 : Form
    {
        //private delegate void AddDataString(string sourceString);

        public System.Threading.Mutex mutex;

        OpenNETCF.Desktop.Communication.RAPI terminalRapi =
            new OpenNETCF.Desktop.Communication.RAPI();

        string[] status =
            new string[] { "Подключен", "Не подключен" };
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
        FileCopyProgressForm frm = new FileCopyProgressForm();
        enum ImportModeEnum { Undefined = 0, Products = 1, Documents = 2 };
        ImportModeEnum currentImportMode = ImportModeEnum.Undefined;
        ProductsDataSetTableAdapters.ProductsTblTableAdapter productAdapter;
        ProductsDataSetTableAdapters.DocsTblTableAdapter docsAdapter;
 
        //TSDUtils.CustomEncodingClass CustomEncodingClass MyEncoder = new CustomEncodingClass();

        void SetFormats()
        {
            cols = new string[productsDataSet1.ProductsBinTbl.Columns.Count];
            dateFormat.ShortDatePattern = Properties.Settings.Default.ShortDatePattern;
            dateFormat.DateSeparator = Properties.Settings.Default.DateSeparator;
            nfi.NumberDecimalSeparator = Properties.Settings.Default.NumberDecimalSeparator;

            string [] colsLengthStr =
                Properties.Settings.Default.FieldsLength.Split(';');
            
            colsLength = new int[colsLengthStr.Length];

            for (int i = 0; i < colsLengthStr.Length; i++)
            {
                colsLength[i] = int.Parse(colsLengthStr[i]);
            }

        }

        public Form1()
        {
            InitializeComponent();
            SetFormats();

            mutex = new System.Threading.Mutex(false, "FAMILTSDSERVER");
            if (!mutex.WaitOne(0, false))
            {
                mutex.Close();
                mutex = null;
            }
            productAdapter =
                      new TSDServer.ProductsDataSetTableAdapters.ProductsTblTableAdapter(this.productsDataSet1);

            docsAdapter =
                new TSDServer.ProductsDataSetTableAdapters.DocsTblTableAdapter(this.productsDataSet1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loadThread != null)
            {
                MessageBox.Show("Идет процесс загрузки!");
                return;
            }
            Cancelled = false;
            richTextBox1.Text = "";
            toolStripStatusLabel2.Text = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                currentImportMode = ImportModeEnum.Products;
                OnProcessImport += new ProcessImport(Form1_OnProcessImport);
                OnFinishImport += new FinishImport(Form1_OnFinishImport);
                OnFailedImport += new FailedImport(Form1_OnFailedImport);
                loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                loadThread.Start(openFileDialog1.FileName);
                stopGoodBtn.Enabled = true;
                stopDocsBtn.Enabled = false;
                this.importGoodBtn.Enabled = false;
                this.importDocBtn.Enabled = false;
                this.uploadBtn.Enabled = false;
                this.downloadBtn.Enabled = false;
                this.settingsBtn.Enabled = false;

            }
        }

        void Form1_OnFailedImport(string message)
        {
            if (this.InvokeRequired)
            {
                FailedImport del = new FailedImport(Form1_OnFailedImport);
                this.Invoke(del, message);
            }
            else
            {
                MessageBox.Show(message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void Form1_OnFinishImport(string fileName)
        {
            if (this.InvokeRequired)
            {
                FinishImport del = new FinishImport(Form1_OnFinishImport);
                this.Invoke(del, fileName);
            }
            else
            {
                //MessageBox.Show(message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show(string.Format(fileName), "Статус загрузки", MessageBoxButtons.OK, MessageBoxIcon.Information);

                OnProcessImport = null;
                OnFinishImport = null;
                OnFailedImport = null;
                /*try
                {
                    //loadThread.Join();
                    if ((int)(loadThread.ThreadState&System.Threading.ThreadState.Running) != 0)
                        loadThread.Abort();
                }
                catch { }*/
                //loadThread.Join();
                loadThread = null;
                
                //stopGoodBtn.Enabled = false;
                //this.importGoodBtn.Enabled = true;
                //this.uploadBtn.Enabled = true;
                //this.settingsBtn.Enabled = true;

                stopGoodBtn.Enabled = false;
                stopDocsBtn.Enabled = false;
                this.importGoodBtn.Enabled = true;
                this.importDocBtn.Enabled = true;
                this.uploadBtn.Enabled = true;
                this.downloadBtn.Enabled = true;
                this.settingsBtn.Enabled = true;

                richTextBox1.AppendText("Загрузка завершена...\n");
            }
        }

        void Form1_OnProcessImport(string Message, bool hasError)
        {
            if (this.InvokeRequired)
            {
                ProcessImport del = new ProcessImport(Form1_OnProcessImport);
                this.Invoke(del,Message,hasError);
            }
            else
            {
                if (hasError)
                    this.richTextBox1.AppendText(Message);
                else
                    toolStripStatusLabel2.Text = string.Format("Загружено {0} строк",Message);
                //MessageBox.Show(message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
                    while ((s = rdr.ReadLine()) != null )
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
                catch{}

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
                    row[i] = System.Single.Parse(cols[i].Trim(),nfi);
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


            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }
           
            for (int i = 0; i <  productsDataSet1.ProductsTbl.Columns.Count; i++)
            {
                try
                {
                    cols[i] = s.Substring(readedLength+1, colsLength[i]);
                    readedLength = readedLength + colsLength[i] + 1;

                    ParseColumn(i, row);

                   
                }
                catch (Exception err)
                {
                   if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s,err.Message), true);
                    
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




            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
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
        
        System.Random r = new Random();
        private void Test(ProductsDataSet.ProductsBinTblRow row)
        {
            return;
            int r1 = rowCounter % 10;//отработаем одну из 10 строк
            if (r1 != 0)
                return;


            int docQuantity = r.Next(5)+1;
            for (int i = 0; i < docQuantity; i++)
            {

                byte docType = 0;
                do
                {
                    docType = (byte)r.Next(5);
                }
                while (docType==0);

                int docs = 1;
                if (docType == (byte)TSDUtils.ActionCode.Remove)
                {
                    //для перемещения сделаем несколько документов
                    docs = r.Next(5)+1;
                }
                for (int j = 0; j < docs; j++)
                {
                    ProductsDataSet.DocsBinTblRow docRow =
                        this.productsDataSet1.DocsBinTbl.NewDocsBinTblRow();

                    docRow.Barcode = row.Barcode;
                    //docRow.DocType = docType;
                    docRow.DocId = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                       docType.ToString("00")+ (i + 1).ToString()+"-"+j.ToString()//docid
                        );

                    byte LabelCode = (byte)r.Next(4);
                    byte MusicCode = (byte)r.Next(4);
                    byte VibroCode = (byte)r.Next(4);

                    int shablon = LabelCode ;
                    shablon = shablon | (MusicCode << 3);
                    shablon = shablon | (VibroCode << 6);
                    docRow.Shablon = shablon;

                    if (docType == (byte)TSDUtils.ActionCode.Remove)
                    {
                        docRow.Priority = (System.Int16)(j | ((byte)TSDUtils.WorkMode.ByPriority << 14));
                        //docRow.WorkMode = (byte)TSDUtils.WorkMode.ByPriority;
                        //docRow.Priority = j;
                        docRow.Quantity = r.Next(100) + 1;
                    }
                    else
                    {
                        //docRow.WorkMode = (byte)TSDUtils.WorkMode.Always;
                        docRow.Priority = 0; //Always=0 и proirity=0
                    }
                    if (docType == (byte)TSDUtils.ActionCode.Reprice)
                    {
                        docRow.RePriceDate = (short)DateTime.Today.Subtract(BaseDate).Days;
                    }

                    if (docType == (byte)TSDUtils.ActionCode.Returns)
                    {
                        docRow.ReturnDate = (short)DateTime.Today.Subtract(BaseDate).Days;
                    }
                    docRow.Text1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes("text1");
                    docRow.Text2 = TSDUtils.CustomEncodingClass.Encoding.GetBytes("text1");
                    docRow.Text3 = TSDUtils.CustomEncodingClass.Encoding.GetBytes("text1");

                    this.productsDataSet1.DocsBinTbl.AddDocsBinTblRow(docRow);

                }
                
            }
            /*
            Array vals = Enum.GetValues(typeof(TSDUtils.ActionCode));
            Array vals1 = Enum.GetValues(typeof(TSDUtils.ShablonCode));
            byte c = 0;


            for (int k = 0; k < 5; k++)
            {
                int b = 0;
                Double d = Math.Round(r.NextDouble());//произвольное число от 0 до 1
                //при округлении получаем случайное значение 0 или 1
                b = (byte)((byte)vals.GetValue(k) * ((byte)d));//Если d=0, то указанный k-й код действия не используется,
                //иначе, если 1 - то используется
                c = (byte)(b | c);//суммируем все биты
            }
            uint sum = 0;
            //по каждому биту действия
            for (byte k = 0; k < 8; k++)
            {
                //определить произвольный код шаблона
                byte d1 = (byte)r.Next(8);
                //код действия
                byte b1 = (byte)(1 << k);//Math.Pow(2, k);

                byte b = (byte)(c & b1);
                if (b != 0)//если код действия продукта содержит необходимый код действия 
                {
                    uint b2 = (uint)(d1 << (3 * k));//сдвигаем кажый код шаблона (3 бит)
                    //на 3k разрядов влево (код шаблона 0,1,3,4...n умножить на (2^3*k)
                    //k=0=>2^0 = 1, код =0,1,2,3...
                    //k=1=>2^3 = 8,код = 0,8,16,24,...
                    //k=2=>2^6 = 16, код = 0,64,128,192...
                    sum = sum | b2;
                    //sum += b2;//суммируем - складываем полученные биты
                }
                //c = (byte)(b*Math.Pow( | c);

            }

            uint res = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(c, sum);
            row.ActionCode = c;
            row.SoundCode = (int)sum;
            row.Shablon = (int)sum;*/
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                terminalRapi.Connect(true,5000);
                if (terminalRapi.Connected)
                {
                    this.importGoodBtn.Enabled = false;
                    this.uploadBtn.Enabled = false;
                    this.settingsBtn.Enabled = false;
                    richTextBox1.Text = "";

                    OpenNETCF.Desktop.Communication.RAPICopingHandler onCopyDelegate = 
                        new OpenNETCF.Desktop.Communication.RAPICopingHandler(terminalRapi_RAPIFileCoping);
                    terminalRapi.RAPIFileCoping += onCopyDelegate;
                    foreach (string fileName in productAdapter.FileList)
                    {
                        if (System.IO.File.Exists(fileName))
                        {
                            IAsyncResult ar =
                                terminalRapi.BeginCopyFileToDevice(fileName,
                                    Properties.Settings.Default.TSDDBPAth + System.IO.Path.GetFileName(fileName), true,
                                    new AsyncCallback(OnEndCopyFile), null);


                            if (frm.ShowDialog() == DialogResult.Abort)
                            {
                                richTextBox1.AppendText("Отмена копирования...\n");
                                richTextBox1.AppendText("Дождитесь завершения... \n");
                                terminalRapi.RAPIFileCoping -= onCopyDelegate;
                                terminalRapi.CancelCopyFileToDevice();
                            }
                        }
                    }
                    foreach (string fileName in docsAdapter.FileList)
                    {
                        if (System.IO.File.Exists(fileName))
                        {
                            IAsyncResult ar =
                                terminalRapi.BeginCopyFileToDevice(fileName,
                                    Properties.Settings.Default.TSDDBPAth + System.IO.Path.GetFileName(fileName), true,
                                    new AsyncCallback(OnEndCopyFile), null);


                            if (frm.ShowDialog() == DialogResult.Abort)
                            {
                                richTextBox1.AppendText("Отмена копирования...\n");
                                richTextBox1.AppendText("Дождитесь завершения... \n");
                                terminalRapi.RAPIFileCoping -= onCopyDelegate;
                                terminalRapi.CancelCopyFileToDevice();
                            }
                        }
                    }
                    MessageBox.Show("Копирование завершено", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.importGoodBtn.Enabled = true;
                    this.uploadBtn.Enabled = true;
                    this.settingsBtn.Enabled = true;
                    richTextBox1.AppendText("Копирование завершено...\n");
                }
                else
                {
                    MessageBox.Show("Терминал не подключен. Проверьте подключение.", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Ошибка загрузки на терминал: "+err.Message, "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void terminalRapi_RAPIFileCoping(long totalSize, long completed, Exception e)
        {
            if (this.InvokeRequired)
            {

                OpenNETCF.Desktop.Communication.RAPICopingHandler del =
                    new OpenNETCF.Desktop.Communication.RAPICopingHandler(terminalRapi_RAPIFileCoping);
                this.Invoke(del, totalSize, completed, e);

            }
            else
            {
                if (e == null)
                    frm.SetProgress(totalSize, completed);
                else
                {
                    frm.SetError(totalSize, completed,e);
                }
            }
        }
        void OnEndCopyFile(IAsyncResult res)
        {
            if (this.InvokeRequired)
            {
                AsyncCallback del = new AsyncCallback(OnEndCopyFile);
                this.Invoke(del,res);
                //Invoke((Delegate)OnEndCopyFile);
            }
            else
            {
                frm.Hide();
                

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            stopGoodBtn.Enabled = false;
            stopDocsBtn.Enabled = false;
            
            terminalRapi.ActiveSync.Active += new OpenNETCF.Desktop.Communication.ActiveHandler(ActiveSync_Active);
            terminalRapi.ActiveSync.IPChange += new OpenNETCF.Desktop.Communication.IPAddrHandler(ActiveSync_IPChange);
            terminalRapi.ActiveSync.Answer += new OpenNETCF.Desktop.Communication.AnswerHandler(ActiveSync_Answer);
            terminalRapi.ActiveSync.Disconnect += new OpenNETCF.Desktop.Communication.DisconnectHandler(ActiveSync_Disconnect);
            terminalRapi.ActiveSync.Inactive += new OpenNETCF.Desktop.Communication.InactiveHandler(ActiveSync_Inactive);
            
            
            
        }

        void ActiveSync_IPChange(int IP)
        {
            if (this.InvokeRequired)
            {
                OpenNETCF.Desktop.Communication.IPAddrHandler del
                     =
                     new OpenNETCF.Desktop.Communication.IPAddrHandler(ActiveSync_IPChange);
                this.Invoke(del, IP);
            }
            else
            {
                richTextBox1.AppendText("IP Change " +
                    OpenNETCF.Desktop.Communication.ActiveSync.IntToDottedIP(IP)+"\n");
            }
        }

        void ActiveSync_Inactive()
        {
            if (this.InvokeRequired)
            {
                OpenNETCF.Desktop.Communication.InactiveHandler del
                     =
                     new OpenNETCF.Desktop.Communication.InactiveHandler(ActiveSync_Inactive);
                this.Invoke(del);
            }
            else
            {
                richTextBox1.AppendText("Inactive\n");
            }
        }

        void ActiveSync_Disconnect()
        {
            if (this.InvokeRequired)
            {
                OpenNETCF.Desktop.Communication.DisconnectHandler del
                     =
                     new OpenNETCF.Desktop.Communication.DisconnectHandler(ActiveSync_Disconnect);
                this.Invoke(del);
            }
            else
            {
                richTextBox1.AppendText("Disconnect\n");
                toolStripStatusLabel1.Text = status[1];
                toolStripStatusLabel1.Image =
                    Properties.Resources.CriticalError;
            }
        }

        void ActiveSync_Answer()
        {
            if (this.InvokeRequired)
            {
                OpenNETCF.Desktop.Communication.AnswerHandler del
                     =
                     new OpenNETCF.Desktop.Communication.AnswerHandler(ActiveSync_Answer);
                this.Invoke(del);
            }
            else
            {
                richTextBox1.AppendText("Answer\n");
            }
        }

        void ActiveSync_Active()
        {
            if (this.InvokeRequired)
            {
                OpenNETCF.Desktop.Communication.ActiveHandler del
                     =
                     new OpenNETCF.Desktop.Communication.ActiveHandler(ActiveSync_Active);
                this.Invoke(del);
            }
            else
            {
                richTextBox1.AppendText("Active\n");
                toolStripStatusLabel1.Text = status[0];
                toolStripStatusLabel1.Image =
                    Properties.Resources.OK;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           /* try
            {
                if (!terminalRapi.Connected)
                    terminalRapi.Connect(true, 500);
            }
            catch { }*/

            if (terminalRapi.DevicePresent)
            {
                toolStripStatusLabel1.Text = status[0];
                toolStripStatusLabel1.Image =
                    Properties.Resources.OK;

            }
            else
            {

                toolStripStatusLabel1.Text = status[1];
                toolStripStatusLabel1.Image =
                    Properties.Resources.CriticalError;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                productAdapter.Dispose();
                docsAdapter.Dispose();

                mutex.ReleaseMutex();
                mutex = null;
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (loadThread != null)
            {
                if (MessageBox.Show("Вы хотите остановить загрузку ?", "Загрузка данных", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    try
                    {
                        Cancelled = true;
                        richTextBox1.AppendText("Отмена загрузки...\n");
                        richTextBox1.AppendText("Дождитесь завершения... \n");
                        //if ((int)(loadThread.ThreadState&System.Threading.ThreadState.Running) != 0)
                        //loadThread.Abort();
                    }
                    catch 
                    {

                    }
                }
                    

            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void importDocBtn_Click(object sender, EventArgs e)
        {
            if (loadThread != null)
            {
                MessageBox.Show("Идет процесс загрузки!");
                return;
            }
            Cancelled = false;
            richTextBox1.Text = "";
            toolStripStatusLabel2.Text = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                currentImportMode = ImportModeEnum.Documents;
                OnProcessImport += new ProcessImport(Form1_OnProcessImport);
                OnFinishImport += new FinishImport(Form1_OnFinishImport);
                OnFailedImport += new FailedImport(Form1_OnFailedImport);
                loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                loadThread.Start(openFileDialog1.FileName);
                
                stopGoodBtn.Enabled = false;
                stopDocsBtn.Enabled = true;
                this.importGoodBtn.Enabled = false;
                this.importDocBtn.Enabled = false;
                this.uploadBtn.Enabled = false;
                this.downloadBtn.Enabled = false;
                this.settingsBtn.Enabled = false;

            }
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Начало загрузки ...\n");
            richTextBox1.AppendText("Подключите терминал и не отключайте до окончания загрузки\n");
            try
            {
                //OpenNETCF.Desktop.Communication.FileList fl = terminalRapi.EnumFiles(Properties.Settings.Default.TSDDBPAth + "ScannedBarcodes.db");
                TSDServer.ScannedProductsDataSet scannedDs =
                    new TSDServer.ScannedProductsDataSet();
                TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter scannedTA =
                    new TSDServer.ScannedProductsDataSetTableAdapters.ScannedBarcodesTableAdapter(scannedDs);

                terminalRapi.Connect(true, 5000);

                foreach (string s in scannedTA.FileList)
                {
                    string ext = Path.GetExtension(s).ToUpper();
                    terminalRapi.CopyFileFromDevice(s,
                        "\\Program Files\\tsdfamilia\\" + Path.GetFileName(s));
                }
                scannedTA.Fill(scannedDs);
                using (System.IO.StreamWriter wr = new StreamWriter("scannedbarcodes.txt", false))
                {
                    foreach (System.Data.DataRow row1 in
                        scannedDs.ScannedBarcodes.Rows)
                    {
                        TSDServer.ScannedProductsDataSet.ScannedBarcodesRow row =
                            (TSDServer.ScannedProductsDataSet.ScannedBarcodesRow)row1;

                        string s =
                            string.Format("{0}|{1}|{2}|{3}|{4}|", row.Barcode, row.DocId, row.DocType, row.FactQuantity, row.ScannedDate);
                        wr.WriteLine(s);
                    }
                    wr.Flush();
                    wr.Close();
                }
            }
            catch (Exception err)
            {
                richTextBox1.AppendText(err.ToString());
            }
            finally
            {
                richTextBox1.AppendText("Загрузка завершена...\n");
            }
        }
    }
}
