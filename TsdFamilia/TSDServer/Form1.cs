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
        private bool Cancelled = false;
        FileCopyProgressForm frm = new FileCopyProgressForm();

 
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
            Cancelled = false;
            richTextBox1.Text = "";
            toolStripStatusLabel2.Text = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                OnProcessImport += new ProcessImport(Form1_OnProcessImport);
                OnFinishImport += new FinishImport(Form1_OnFinishImport);
                OnFailedImport += new FailedImport(Form1_OnFailedImport);
                loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                loadThread.Start(openFileDialog1.FileName);
                button4.Enabled = true; this.button1.Enabled = false;
                this.button2.Enabled = false;
                this.button3.Enabled = false;

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
                button4.Enabled = false;
                this.button1.Enabled = true;
                this.button2.Enabled = true;
                this.button3.Enabled = true;
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
                        if (Cancelled)
                            return;
                        del.Invoke(s);
                        
                    }
                }


              

                
                //MessageBox.Show("Загрузка завершена", "Статус загрузки", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    using (ProductsDataSetTableAdapters.ProductsBinTblTableAdapter tAdapter =
                      new TSDServer.ProductsDataSetTableAdapters.ProductsBinTblTableAdapter())
                    {

                        tAdapter.Update(this.productsDataSet1);
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
                   typeof(System.Byte))
                {
                    row[i] = System.Byte.Parse(cols[i].Trim());
                    return;
                    //continue;
                }
                if (productsDataSet1.ProductsBinTbl.Columns[i].DataType ==
                   typeof(System.Int16))
                {
                    row[i] = System.Int16.Parse(cols[i].Trim());
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
                    OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }
           
            for (int i = 0; i <  productsDataSet1.ProductsBinTbl.Columns.Count; i++)
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
            this.productsDataSet1.ProductsBinTbl.AddProductsBinTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);

        }

        private void AddDelimetedString(string s)
        {

            cols = s.Split(fieldDelimeter);
            
            ProductsDataSet.ProductsBinTblRow row =
                this.productsDataSet1.ProductsBinTbl.NewProductsBinTblRow();




            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                 if (OnProcessImport != null)
                     OnProcessImport(string.Format("Штрихкод неверный - строка пропущена {0}\n", s), true);
                return;
            }


            for (int i = 0; i < productsDataSet1.ProductsBinTbl.Columns.Count; i++)
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
            this.productsDataSet1.ProductsBinTbl.AddProductsBinTblRow(row);
            rowCounter++;
            if (OnProcessImport != null)
                OnProcessImport(rowCounter.ToString(), false);
            

        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                terminalRapi.Connect(true,5000);
                if (terminalRapi.Connected)
                {
                    this.button1.Enabled = false;
                    this.button2.Enabled = false;
                    this.button3.Enabled = false;
                    richTextBox1.Text = "";

                    OpenNETCF.Desktop.Communication.RAPICopingHandler onCopyDelegate = 
                        new OpenNETCF.Desktop.Communication.RAPICopingHandler(terminalRapi_RAPIFileCoping);
                    terminalRapi.RAPIFileCoping += onCopyDelegate;
                    
                    IAsyncResult ar = 
                        terminalRapi.BeginCopyFileToDevice(Application.StartupPath + "\\products.sdf",
                            Properties.Settings.Default.TSDDBPAth + "products.sdf", true,
                            new AsyncCallback(OnEndCopyFile), null);
                    
                    if (frm.ShowDialog() == DialogResult.Abort)
                    {
                        richTextBox1.AppendText("Отмена копирования...\n");
                        richTextBox1.AppendText("Дождитесь завершения... \n");
                        terminalRapi.RAPIFileCoping -= onCopyDelegate;
                        terminalRapi.CancelCopyFileToDevice();
                    }
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
                MessageBox.Show("Копирование завершено", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.button1.Enabled = true;
                this.button2.Enabled = true;
                this.button3.Enabled = true;
                richTextBox1.AppendText("Копирование завершено...\n");

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            button4.Enabled = false;
            
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
    }
}
