using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core;
using System.IO;


namespace TSDServer
{
    public partial class Form1 : Form
    {
        
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


        System.IO.Compression.DeflateStream stream = null;

        CustomEncodingClass MyEncoder = new CustomEncodingClass();

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loadThread != null)
            {
                MessageBox.Show("Идет процесс загрузки!");
                return;
            }
            toolStripStatusLabel2.Text = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                OnProcessImport += new ProcessImport(Form1_OnProcessImport);
                OnFinishImport += new FinishImport(Form1_OnFinishImport);
                OnFailedImport += new FailedImport(Form1_OnFailedImport);
                loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                loadThread.Start(openFileDialog1.FileName);
                button4.Enabled = true;
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
                try
                {
                    //loadThread.Join();
                    if ((int)(loadThread.ThreadState&System.Threading.ThreadState.Running) != 0)
                        loadThread.Abort();
                }
                catch { }
                loadThread = null;
                button4.Enabled = false;
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
                this.productsDataSet1.CleanDatabase();
                bool IsFileFixed = Properties.Settings.Default.ImportFileTypeIsFixed;
                AddStringDelegate del = null;
                if (IsFileFixed)
                    del = new AddStringDelegate(AddFixedString);
                else
                    del = new AddStringDelegate(AddDelimetedString);


                using (System.IO.StreamReader rdr =
                    new System.IO.StreamReader(fileName, Encoding.GetEncoding("windows-1251")))
                {

                    string s = string.Empty;
                    while ((s = rdr.ReadLine()) != null )
                    {
                        del.Invoke(s);
                        
                    }
                }


              

                
                //MessageBox.Show("Загрузка завершена", "Статус загрузки", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception err)
            {
                if (OnFailedImport != null)
                    OnFailedImport("Ошибка: " + err.Message);
                //MessageBox.Show("Ошибка: " + err.Message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                try
                {
                    using (ProductsDataSetTableAdapters.ProductsBinTblTableAdapter tAdapter =
                      new TSDServer.ProductsDataSetTableAdapters.ProductsBinTblTableAdapter())
                    {

                        tAdapter.Update(this.productsDataSet1);
                    }
                    this.productsDataSet1.AcceptChanges();
                }
                catch{}

                if (OnFinishImport != null)
                    OnFinishImport("Загрузка завершена");
            }
        }

        private void ParseColumn(int i, DataRow row)
        {
            if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
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
                    row[i] = MyEncoder.GetBytes(cols[i].Trim());//buffer;
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
                if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                    typeof(System.Int64))
                {
                    row[i] = System.Int64.Parse(cols[i].Trim());
                    return;
                    //continue;
                }

                if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                   typeof(System.Single))
                {
                    row[i] = System.Single.Parse(cols[i].Trim(),nfi);
                    return;
                    //continue;
                }

                if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                    typeof(DateTime))
                {
                    row[i] = DateTime.Parse(cols[i].Trim(), dateFormat);
                    return;
                    //continue;
                }

                if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                   typeof(decimal))
                {
                    row[i] = Decimal.Parse(cols[i].Trim(), nfi);
                    return;
                    //continue;
                }
            }
        }
        private void AddFixedString(string s)
        {

            cols = new string[productsDataSet1.ProductsBinTbl.Columns.Count];

            ProductsDataSet.ProductsBinTblRow row =
                this.productsDataSet1.ProductsBinTbl.NewProductsBinTblRow();

            cols[0] = s.Substring(0, colsLength[0]);
            int readedLength = -1;//;colsLength[0];


            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод пустой - строка пропущена {0}\n", s), true);
                //this.richTextBox1.AppendText(string.Format("Штрихкод пустой - строка пропущена {0}\n",s));
                return;
            }
            //row[0] = cols[0].Trim();
            //ParseColumn(0, row);
            
            for (int i = 0; i <  productsDataSet1.ProductsBinTbl.Columns.Count; i++)
            {
                try
                {
                    cols[i] = s.Substring(readedLength+1, colsLength[i]);
                    readedLength = readedLength + colsLength[i] + 1;

                    ParseColumn(i, row);
                    /*if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                        typeof(string))
                    {
                        row[i] = cols[i].Trim();
                        continue;
                    }
                    if (!String.IsNullOrEmpty(cols[i].Trim()))
                    {
                        if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                            typeof(decimal))
                        {
                            row[i] = Decimal.Parse(cols[i].Trim(), nfi);
                            continue;
                        }

                        if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                            typeof(DateTime))
                        {
                            row[i] = DateTime.Parse(cols[i].Trim(), dateFormat);
                            continue;
                        }
                    }
                    */

                }
                catch (Exception err)
                {
                    //this.richTextBox1.AppendText(string.Format("Ошибка в строке: {0}: {1}\n", s,err.Message));
                    if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s,err.Message), true);
                    
                }
            }
            this.productsDataSet1.ProductsBinTbl.AddProductsBinTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);

        }

        private void AddDelimetedString(string s)
        {

            cols = s.Split(fieldDelimeter);
            /*byte[] buffer = System.Text.Encoding.Unicode.GetBytes(s);
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(buffer, 0, buffer.Length);
                ms.Seek(0, SeekOrigin.Begin);
                using (MemoryStream ms1 = new MemoryStream())
                {
                    using (GZipOutputStream zipStream = new GZipOutputStream(ms1))
                    {
                        byte[] tmpBuffer = new byte[4096];

                        StreamUtils.Copy(ms, zipStream, tmpBuffer);
                        zipStream.Finish();

                        buffer = new byte[(int)ms1.Length];
                        //zipStream.Seek(0, SeekOrigin.Begin);
                        ms1.Seek(0, SeekOrigin.Begin);
                        ms1.Read(buffer, 0, (int)ms1.Length);
                        //zipStream.Read(tableBuffer, 0, tableBuffer.Length);
                        //System.Threading.Thread.Sleep(500);
                        
                    }
                }
            }*/

            ProductsDataSet.ProductsBinTblRow row =
                this.productsDataSet1.ProductsBinTbl.NewProductsBinTblRow();

            //cols[0] = s.Substring(0, colsLength[0]);
            //int readedLength = colsLength[0];


            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                //this.richTextBox1.AppendText(string.Format("Штрихкод пустой - строка пропущена {0}\n", s));
                if (OnProcessImport != null)
                    OnProcessImport(string.Format("Штрихкод пустой - строка пропущена {0}\n", s), true);

                return;
            }
            //row[0] = cols[0].Trim();
            //ParseColumn(0, row);

            for (int i = 0; i < productsDataSet1.ProductsBinTbl.Columns.Count; i++)
            {
                try
                {
                    //cols[i] = s.Substring(readedLength + 1, colsLength[i]);
                    //readedLength = readedLength + colsLength[i] + 1;

                    ParseColumn(i, row);
                    /*if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                        typeof(string))
                    {
                        row[i] = cols[i].Trim();
                        continue;
                    }
                    if (!String.IsNullOrEmpty(cols[i].Trim()))
                    {
                        if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                            typeof(decimal))
                        {
                            row[i] = Decimal.Parse(cols[i].Trim(), nfi);
                            continue;
                        }

                        if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                            typeof(DateTime))
                        {
                            row[i] = DateTime.Parse(cols[i].Trim(), dateFormat);
                            continue;
                        }
                    }*/
                    
                    

                }
                catch (Exception err)
                {
                    //this.richTextBox1.AppendText(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message));
                    if (OnProcessImport != null)
                        OnProcessImport(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message), true);
                    
                }
            }
            this.productsDataSet1.ProductsBinTbl.AddProductsBinTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);
            

        }

        FileCopyProgressForm frm = new FileCopyProgressForm();
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                terminalRapi.Connect(true,5000);
                if (terminalRapi.Connected)
                {
                    //terminalRapi.CopyFileToDevice(Application.StartupPath+"\\products.sdf",
                    //    Properties.Settings.Default.TSDDBPAth + "products.sdf", true);
                    terminalRapi.RAPIFileCoping += new OpenNETCF.Desktop.Communication.RAPICopingHandler(terminalRapi_RAPIFileCoping);
                    terminalRapi.BeginCopyFileToDevice(Application.StartupPath + "\\products.sdf",
                        Properties.Settings.Default.TSDDBPAth + "products.sdf", true,
                        new AsyncCallback(OnEndCopyFile), null);
                    frm.Show();
                    //MessageBox.Show("Загрузка завершена", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                MessageBox.Show("Загрузка завершена", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            button4.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           /* try
            {
                if (!terminalRapi.Connected)
                    terminalRapi.Connect(true, 500);
            }
            catch { }*/

            if (terminalRapi.Connected)
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
                        //if ((int)(loadThread.ThreadState&System.Threading.ThreadState.Running) != 0)
                        loadThread.Abort();
                    }
                    catch 
                    {

                    }
                }
                    

            }
        }
    }
}
