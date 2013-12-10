using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace TSDServer
{
    public partial class Form1 : Form
    {
        //private delegate void AddDataString(string sourceString);
        private List<string> copiedFileList =
            new List<string>();

        public System.Threading.Mutex mutex;
        //List<System.Threading.ManualResetEvent> mEvt =
        //    new List<System.Threading.ManualResetEvent>();
        

        //int fileCounter = 0;
        //int finishCount = 0;
        DataLoaderClass loader = new DataLoaderClass();

        OpenNETCF.Desktop.Communication.RAPI terminalRapi =
            new OpenNETCF.Desktop.Communication.RAPI();

        OpenNETCF.Desktop.Communication.RAPICopingHandler onCopyDelegate = null;
           


        string[] status =
            new string[] { "Подключен", "Не подключен" };
       
        FileCopyProgressForm frm = new FileCopyProgressForm();
        Dictionary<string, FileCopyProgressForm> progressForms =
            new Dictionary<string, FileCopyProgressForm>();

        //bool EraseTerminalDB = false;
        

        public Form1()
        {
            InitializeComponent();
            //SetFormats();

            mutex = new System.Threading.Mutex(false, "FAMILTSDSERVER");
            if (!mutex.WaitOne(0, false))
            {
                mutex.Close();
                mutex = null;
            }
            loader.OnProcessImport += new DataLoaderClass.ProcessImport(Form1_OnProcessImport);
            loader.OnFinishImport += new DataLoaderClass.FinishImport(Form1_OnFinishImport);
            loader.OnFailedImport += new DataLoaderClass.FailedImport(Form1_OnFailedImport);

            //productAdapter =
            //          new TSDServer.ProductsDataSetTableAdapters.ProductsTblTableAdapter(this.productsDataSet1);

            //docsAdapter =
            //    new TSDServer.ProductsDataSetTableAdapters.DocsTblTableAdapter(this.productsDataSet1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loader.Processing)
            {
                MessageBox.Show("Идет процесс загрузки!");
                return;
            }
            //Cancelled = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.Text = "";
                toolStripStatusLabel2.Text = "";
                loader.AutoLoadProduct(openFileDialog1.FileName);
                this.importGoodBtn.Enabled = false;
                this.importDocBtn.Enabled = false;
                this.uploadBtn.Enabled = false;
                this.downloadBtn.Enabled = false;
                this.settingsBtn.Enabled = false;
                stopGoodBtn.Enabled = true;
                stopDocsBtn.Enabled = false;

                //currentImportMode = ImportModeEnum.Products;
                //OnProcessImport += new ProcessImport(Form1_OnProcessImport);
                //OnFinishImport += new FinishImport(Form1_OnFinishImport);
                //OnFailedImport += new FailedImport(Form1_OnFailedImport);
                //loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                //loadThread.Start(openFileDialog1.FileName);
                //stopGoodBtn.Enabled = true;
                //stopDocsBtn.Enabled = false;
                //this.importGoodBtn.Enabled = false;
                //this.importDocBtn.Enabled = false;
                //this.uploadBtn.Enabled = false;
                //this.downloadBtn.Enabled = false;
                //this.settingsBtn.Enabled = false;

            }
        }

       

        void Form1_OnFailedImport(string message)
        {
            if (this.InvokeRequired)
            {
                DataLoaderClass.FailedImport del = new DataLoaderClass.FailedImport(Form1_OnFailedImport);
                this.Invoke(del, message);
            }
            else
            {
                if (!Program.AutoMode)
                    MessageBox.Show(message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void Form1_OnFinishImport(string fileName)
        {
            if (this.InvokeRequired)
            {
                DataLoaderClass.FinishImport del = new DataLoaderClass.FinishImport(Form1_OnFinishImport);
                this.Invoke(del, fileName);
            }
            else
            {
                if (!Program.AutoMode)
                //MessageBox.Show(message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MessageBox.Show(string.Format(fileName), "Статус загрузки", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //DataLoaderClass.OnProcessImport = null;
                //DataLoaderClass.OnFinishImport -= Form1_OnFinishImport;
                //DataLoaderClass.OnFailedImport = null;
                /*try
                {
                    //loadThread.Join();
                    if ((int)(loadThread.ThreadState&System.Threading.ThreadState.Running) != 0)
                        loadThread.Abort();
                }
                catch { }*/
                //loadThread.Join();
               
                
                //stopGoodBtn.Enabled = false;
                //this.importGoodBtn.Enabled = true;
                //this.uploadBtn.Enabled = true;
                //this.settingsBtn.Enabled = true;

                stopGoodBtn.Enabled = false;
                stopDocsBtn.Enabled = false;
                this.importGoodBtn.Enabled = true & Properties.Settings.Default.ImportProductsEnabled;
                this.importDocBtn.Enabled = true & Properties.Settings.Default.ImportDocsEnabled;
                this.uploadBtn.Enabled = true;
                this.downloadBtn.Enabled = true;
                this.settingsBtn.Enabled = true & Properties.Settings.Default.SettingsEnabled;

                richTextBox1.AppendText("Загрузка завершена...\n");
            }
        }

        void Form1_OnProcessImport(string Message, bool hasError)
        {
            if (this.InvokeRequired)
            {
                DataLoaderClass.ProcessImport del = new DataLoaderClass.ProcessImport(Form1_OnProcessImport);
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
        StringBuilder copyStatesb = new StringBuilder();

        #region test old
        //System.Random r = new Random();
        //private void Test(ProductsDataSet.ProductsBinTblRow row)
        //{
        //    return;
        //    int r1 = rowCounter % 10;//отработаем одну из 10 строк
        //    if (r1 != 0)
        //        return;


        //    int docQuantity = r.Next(5)+1;
        //    for (int i = 0; i < docQuantity; i++)
        //    {

        //        byte docType = 0;
        //        do
        //        {
        //            docType = (byte)r.Next(5);
        //        }
        //        while (docType==0);

        //        int docs = 1;
        //        if (docType == (byte)TSDUtils.ActionCode.Remove)
        //        {
        //            //для перемещения сделаем несколько документов
        //            docs = r.Next(5)+1;
        //        }
        //        for (int j = 0; j < docs; j++)
        //        {
        //            ProductsDataSet.DocsBinTblRow docRow =
        //                this.productsDataSet1.DocsBinTbl.NewDocsBinTblRow();

        //            docRow.Barcode = row.Barcode;
        //            //docRow.DocType = docType;
        //            docRow.DocId = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
        //               docType.ToString("00")+ (i + 1).ToString()+"-"+j.ToString()//docid
        //                );

        //            byte LabelCode = (byte)r.Next(4);
        //            byte MusicCode = (byte)r.Next(4);
        //            byte VibroCode = (byte)r.Next(4);

        //            int shablon = LabelCode ;
        //            shablon = shablon | (MusicCode << 3);
        //            shablon = shablon | (VibroCode << 6);
        //            docRow.Shablon = shablon;

        //            if (docType == (byte)TSDUtils.ActionCode.Remove)
        //            {
        //                docRow.Priority = (System.Int16)(j | ((byte)TSDUtils.WorkMode.ByPriority << 14));
        //                //docRow.WorkMode = (byte)TSDUtils.WorkMode.ByPriority;
        //                //docRow.Priority = j;
        //                docRow.Quantity = r.Next(100) + 1;
        //            }
        //            else
        //            {
        //                //docRow.WorkMode = (byte)TSDUtils.WorkMode.Always;
        //                docRow.Priority = 0; //Always=0 и proirity=0
        //            }
        //            if (docType == (byte)TSDUtils.ActionCode.Reprice)
        //            {
        //                docRow.RePriceDate = (short)DateTime.Today.Subtract(BaseDate).Days;
        //            }

        //            if (docType == (byte)TSDUtils.ActionCode.Returns)
        //            {
        //                docRow.ReturnDate = (short)DateTime.Today.Subtract(BaseDate).Days;
        //            }
        //            docRow.Text1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes("text1");
        //            docRow.Text2 = TSDUtils.CustomEncodingClass.Encoding.GetBytes("text1");
        //            docRow.Text3 = TSDUtils.CustomEncodingClass.Encoding.GetBytes("text1");

        //            this.productsDataSet1.DocsBinTbl.AddDocsBinTblRow(docRow);

        //        }
                
        //    }
        //    /*
        //    Array vals = Enum.GetValues(typeof(TSDUtils.ActionCode));
        //    Array vals1 = Enum.GetValues(typeof(TSDUtils.ShablonCode));
        //    byte c = 0;


        //    for (int k = 0; k < 5; k++)
        //    {
        //        int b = 0;
        //        Double d = Math.Round(r.NextDouble());//произвольное число от 0 до 1
        //        //при округлении получаем случайное значение 0 или 1
        //        b = (byte)((byte)vals.GetValue(k) * ((byte)d));//Если d=0, то указанный k-й код действия не используется,
        //        //иначе, если 1 - то используется
        //        c = (byte)(b | c);//суммируем все биты
        //    }
        //    uint sum = 0;
        //    //по каждому биту действия
        //    for (byte k = 0; k < 8; k++)
        //    {
        //        //определить произвольный код шаблона
        //        byte d1 = (byte)r.Next(8);
        //        //код действия
        //        byte b1 = (byte)(1 << k);//Math.Pow(2, k);

        //        byte b = (byte)(c & b1);
        //        if (b != 0)//если код действия продукта содержит необходимый код действия 
        //        {
        //            uint b2 = (uint)(d1 << (3 * k));//сдвигаем кажый код шаблона (3 бит)
        //            //на 3k разрядов влево (код шаблона 0,1,3,4...n умножить на (2^3*k)
        //            //k=0=>2^0 = 1, код =0,1,2,3...
        //            //k=1=>2^3 = 8,код = 0,8,16,24,...
        //            //k=2=>2^6 = 16, код = 0,64,128,192...
        //            sum = sum | b2;
        //            //sum += b2;//суммируем - складываем полученные биты
        //        }
        //        //c = (byte)(b*Math.Pow( | c);

        //    }

        //    uint res = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(c, sum);
        //    row.ActionCode = c;
        //    row.SoundCode = (int)sum;
        //    row.Shablon = (int)sum;*/
        //}
        #endregion

        private string LoadToDevice(string[] fileList)
        {
           return LoadToDevice(fileList,
                Properties.Settings.Default.TSDDBPAth);
        }

        private string LoadToDevice(string[] fileList, string pathToCopy)
        {
            //copiedFileList.Clear();
            try
            {
                copyStatesb.Length = 0;

                terminalRapi.Connect(true, 5000);
                if (terminalRapi.Connected)
                {
                    this.importGoodBtn.Enabled = false;
                    this.uploadBtn.Enabled = false;
                    this.settingsBtn.Enabled = false;
                    richTextBox1.Text = "";


                    foreach (string fileName in fileList)
                    {
                        //copiedFileList.Add(fileName);
                        if (terminalRapi.DeviceFileExists(Path.Combine(pathToCopy, System.IO.Path.GetFileName(fileName))))
                        {
                            terminalRapi.DeleteDeviceFile(Path.Combine(pathToCopy, System.IO.Path.GetFileName(fileName)));
                        }
                    }

                    terminalRapi.RAPIFileCoping += onCopyDelegate;


                    foreach (string fileName in fileList)
                    {
                        if (System.IO.File.Exists(fileName))
                        {
                            Program.log.Info("Начало копирования " + fileName);
                            System.Threading.Thread.Sleep(500);

                            if (progressForms.ContainsKey(fileName))
                                progressForms.Remove(fileName);

                            progressForms.Add(fileName,
                                new FileCopyProgressForm());

                            IAsyncResult ar =
                                terminalRapi.BeginCopyFileToDevice(fileName,
                                    Path.Combine(pathToCopy,
                                        System.IO.Path.GetFileName(fileName)), true,
                                    new AsyncCallback(OnEndCopyFile), fileName);

                            progressForms[fileName].ShowDialog();
                        }
                        else
                        {
                            Program.log.Info("Файл не найден " + fileName);
                            copiedFileList.Remove(fileName);
                        }
                    }
                    while (copiedFileList.Count > 0)
                    {
                        System.Threading.Thread.Sleep(100);
                        Application.DoEvents();
                    }

                    System.Threading.Thread.Sleep(1000);
                    /*
                     if (copyStatesb.Length == 0)
                        MessageBox.Show("Копирование завершено успешно",
                            "Статус загрузки на терминал",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    else
                        MessageBox.Show(string.Format("Копирование завершено с ошибкой: {0}", copyStatesb.ToString())
                            , "Статус загрузки на терминал",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                    */
                    this.importGoodBtn.Enabled = true & Properties.Settings.Default.ImportProductsEnabled;
                    this.uploadBtn.Enabled = true;
                    this.settingsBtn.Enabled = true & Properties.Settings.Default.SettingsEnabled;
                    richTextBox1.AppendText("Копирование завершено...\n");
                    terminalRapi.RAPIFileCoping -= onCopyDelegate;

                    progressForms.Clear();
                    return copyStatesb.ToString();    
                }
                else
                {
                    MessageBox.Show("Терминал не подключен. Проверьте подключение.", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return string.Empty;
                }
                
            }
            catch (Exception err)
            {
                MessageBox.Show("Ошибка загрузки на терминал: " + err.Message, "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
            finally
            {
                copyStatesb.Length = 0;
            }
        }

        private void LoadAll()
        {
            copiedFileList.Clear();
            try
            {
                DateTime dt = loader.GetDBDate();
                if (DateTime.Now.Subtract(dt) >= new TimeSpan(1, 0, 0, 0))
                {
                    DialogResult dr = MessageBox.Show(
                        string.Format("Внимание, справочник который вы хотите загрузить на ТСД имеет дату {0}! \nВы уверены, что хотите загрузить старые данные?"
                            , dt.ToString("dd.MM.yyyy"))
                        , "Старые данные в справочнике!"
                        , MessageBoxButtons.YesNo
                        , MessageBoxIcon.Warning);

                    if (dr != DialogResult.Yes)
                    {
                        richTextBox1.AppendText("Копирование отменено...\n");
                        return;
                    }
                }

            }
            catch (Exception err)
            {
                richTextBox1.AppendText(err.Message + "\n");
                return;
            }
            StringBuilder sb = new StringBuilder();


            //foreach (string fileName in loader.ProductsFileList)
            //{
            //    copiedFileList.Add(fileName);
            //}
            copiedFileList.AddRange(loader.ProductsFileList);
            //foreach (string fileName in loader.DocsFileList)
            //{
            //    copiedFileList.Add(fileName);
            //}
            copiedFileList.AddRange(loader.DocsFileList);

           
            //foreach (string fileName in loader.ProgramFileList)
            //{
            //    copiedFileList.Add(fileName);
            //}
            sb.Append(LoadToDevice(copiedFileList.ToArray()));
            copiedFileList.Clear();

            copiedFileList.AddRange(loader.ProgramFileList);

            Program.log.Info("Файлы добавлены для копирования");
            sb.Append(LoadToDevice(copiedFileList.ToArray(),
                Properties.Settings.Default.TSDProgramPath));

            if (sb.Length == 0)
                MessageBox.Show("Копирование завершено",
                    "Статус загрузки на терминал",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            else
                MessageBox.Show(string.Format("Копирование завершено с ошибкой: {0}", sb.ToString())
                    , "Статус загрузки на терминал",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

        }
        private void LoadDocs()
        {
            copiedFileList.Clear();

            foreach (string fileName in loader.DocsFileList)
            {
                copiedFileList.Add(fileName);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(
                LoadToDevice(copiedFileList.ToArray()));

            if (sb.Length == 0)
                MessageBox.Show("Копирование завершено",
                    "Статус загрузки на терминал",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            else
                MessageBox.Show(string.Format("Копирование завершено с ошибкой: {0}", sb.ToString())
                    , "Статус загрузки на терминал",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);


        }


        private void button2_Click(object sender, EventArgs e)
        {

            LoadAll();

            #region oldVar
            /*
            try
            {
                DateTime dt = loader.GetDBDate();
                if (DateTime.Now.Subtract(dt)>= new TimeSpan(1,0,0,0))
                {
                    DialogResult dr = MessageBox.Show(
                        string.Format("Внимание, справочник который вы хотите загрузить на ТСД имеет дату {0}! \nВы уверены, что хотите загрузить старые данные?"
                            ,dt.ToString("dd.MM.yyyy"))
                        , "Старые данные в справочнике!"
                        , MessageBoxButtons.YesNo
                        , MessageBoxIcon.Warning);

                    if (dr != DialogResult.Yes)
                    {
                        richTextBox1.AppendText("Копирование отменено...\n");
                        return;
                    }
                }

            }
            catch (Exception err)
            {
                richTextBox1.AppendText(err.Message+"\n");
                return;
            }

            copiedFileList.Clear();
            try
            {
                copyStatesb.Length = 0;

                terminalRapi.Connect(true,5000);
                if (terminalRapi.Connected)
                {
                    this.importGoodBtn.Enabled = false;
                    this.uploadBtn.Enabled = false;
                    this.settingsBtn.Enabled = false;
                    richTextBox1.Text = "";


                    

                    foreach (string fileName in loader.ProductsFileList)
                    {
                        copiedFileList.Add(fileName);
                        if (terminalRapi.DeviceFileExists(Path.Combine(Properties.Settings.Default.TSDDBPAth ,System.IO.Path.GetFileName(fileName))))
                        {
                            terminalRapi.DeleteDeviceFile(Path.Combine(Properties.Settings.Default.TSDDBPAth ,System.IO.Path.GetFileName(fileName)));
                        }
                    }
                    foreach (string fileName in loader.DocsFileList)
                    {
                        copiedFileList.Add(fileName);
                        if (terminalRapi.DeviceFileExists(Path.Combine(Properties.Settings.Default.TSDDBPAth, System.IO.Path.GetFileName(fileName))))
                        {
                            terminalRapi.DeleteDeviceFile(Path.Combine(
                                Properties.Settings.Default.TSDDBPAth, 
                                System.IO.Path.GetFileName(fileName)));
                        }
                    }

                    terminalRapi.RAPIFileCoping += onCopyDelegate;
                    string[] filelist = copiedFileList.ToArray();
                    
                    foreach (string fileName in filelist)//loader.ProductsFileList)
                    {
                        if (System.IO.File.Exists(fileName))
                        {

                            System.Threading.Thread.Sleep(500);

                            progressForms.Add(fileName,
                                new FileCopyProgressForm());
                            IAsyncResult ar =
                                terminalRapi.BeginCopyFileToDevice(fileName,
                                    Path.Combine(Properties.Settings.Default.TSDDBPAth ,System.IO.Path.GetFileName(fileName)), true,
                                    new AsyncCallback(OnEndCopyFile), fileName);

                            progressForms[fileName].ShowDialog();

                        }
                        else
                           copiedFileList.Remove(fileName);
                    }
                    while (copiedFileList.Count > 0)
                    {
                        System.Threading.Thread.Sleep(100);
                        Application.DoEvents();
                    }

                    System.Threading.Thread.Sleep(1000);
                    if (copyStatesb.Length ==0)
                        MessageBox.Show("Копирование завершено успешно", 
                            "Статус загрузки на терминал", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);
                    else
                        MessageBox.Show(string.Format("Копирование завершено с ошибкой: {0}",copyStatesb.ToString())
                            , "Статус загрузки на терминал", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Exclamation);

                    this.importGoodBtn.Enabled = true & Properties.Settings.Default.ImportProductsEnabled;
                    this.uploadBtn.Enabled = true;
                    this.settingsBtn.Enabled = true & Properties.Settings.Default.SettingsEnabled;
                    richTextBox1.AppendText("Копирование завершено...\n");
                    terminalRapi.RAPIFileCoping -= onCopyDelegate;

                    progressForms.Clear();
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
            copyStatesb.Length = 0;

            */
#endregion
        }

        void terminalRapi_RAPIFileCoping(string name, long totalSize, long completed, Exception e)
        {
            if (this.InvokeRequired)
            {

                OpenNETCF.Desktop.Communication.RAPICopingHandler del =
                    new OpenNETCF.Desktop.Communication.RAPICopingHandler(terminalRapi_RAPIFileCoping);
                this.Invoke(del,name, totalSize, completed, e);

            }
            else
            {
                progressForms[name].FormCaption = string.Format("Копирование {0}", System.IO.Path.GetFileName(name));
                if (e == null)
                {
                    progressForms[name].SetProgress(totalSize, completed);
                    

                    //if (totalSize == completed)
                    //{
                    //    copiedFileList.Remove(name);
                    //    if (copiedFileList.Count == 0)
                    //        mEvt.Set();
                        
                    //}
                }
                else
                {
                    frm.SetError(totalSize, completed, e);
                    copyStatesb.AppendFormat("Ошибка копирования {0}.\n", name);
                    richTextBox1.AppendText(string.Format("Ошибка копирования {0}. \n",name));
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
                lock (this)
                {
                    copiedFileList.Remove(res.AsyncState.ToString());
                    //terminalRapi.RAPIFileCoping -= onCopyDelegate;
                //    mEvt[finishCount++].Set();
                }
                progressForms[res.AsyncState.ToString()].Close();
                progressForms[res.AsyncState.ToString()].Dispose();
                //frm.Hide();
                
                //copiedFileList.Add(fileName, false);

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName an = a.GetName();
            //an.Version.ToString();
            this.Text = string.Format("ТСД Сервер. Версия {0}", an.Version);


            timer1.Enabled = true;
            stopGoodBtn.Enabled = false;
            stopDocsBtn.Enabled = false;
            
            terminalRapi.ActiveSync.Active += new OpenNETCF.Desktop.Communication.ActiveHandler(ActiveSync_Active);
            terminalRapi.ActiveSync.IPChange += new OpenNETCF.Desktop.Communication.IPAddrHandler(ActiveSync_IPChange);
            terminalRapi.ActiveSync.Answer += new OpenNETCF.Desktop.Communication.AnswerHandler(ActiveSync_Answer);
            terminalRapi.ActiveSync.Disconnect += new OpenNETCF.Desktop.Communication.DisconnectHandler(ActiveSync_Disconnect);
            terminalRapi.ActiveSync.Inactive += new OpenNETCF.Desktop.Communication.InactiveHandler(ActiveSync_Inactive);
            terminalRapi.RAPIConnected += new OpenNETCF.Desktop.Communication.RAPIConnectedHandler(terminalRapi_RAPIConnected);
            onCopyDelegate = new OpenNETCF.Desktop.Communication.RAPICopingHandler(terminalRapi_RAPIFileCoping);

            this.importDocBtn.Enabled = Properties.Settings.Default.ImportDocsEnabled;
            this.importGoodBtn.Enabled = Properties.Settings.Default.ImportProductsEnabled;
            this.settingsBtn.Enabled = Properties.Settings.Default.SettingsEnabled;

            cbEraseTerminalDB.Checked = Properties.Settings.Default.EraseTerminalDB;
        }

        void terminalRapi_RAPIConnected()
        {
            
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
                terminalRapi.SetDeviceTime(Properties.Settings.Default.TSDDBPAth);

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

                terminalRapi.SetDeviceTime(Properties.Settings.Default.TSDDBPAth);

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
                terminalRapi.SetDeviceTime(Properties.Settings.Default.TSDDBPAth);

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
            Application.DoEvents();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (e.CloseReason == CloseReason.UserClosing && 
            //    !Program.AutoMode)
            //{
            //    e.Cancel = true;
            //    this.Hide();
            //}
            //else
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

        void notifyIcon1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.Visible)
            {
                this.Show();
            }
            else
                this.BringToFront();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (loader.Processing)
            {
                if (MessageBox.Show("Вы хотите остановить загрузку ?", "Загрузка данных", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    try
                    {
                        loader.Cancel();
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
            cbEraseTerminalDB.Checked = Properties.Settings.Default.EraseTerminalDB; 
        }

        private void importDocBtn_Click(object sender, EventArgs e)
        {
            if (loader.Processing)
            {
                MessageBox.Show("Идет процесс загрузки!");
                return;
            }
            //Cancelled = false;
            //richTextBox1.Text = "";
            //toolStripStatusLabel2.Text = "";
            
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.Text = "";
                toolStripStatusLabel2.Text = "";
                //currentImportMode = ImportModeEnum.Documents;
                loader.AutoLoadDoc(openFileDialog1.FileName);
                stopGoodBtn.Enabled = false;
                stopDocsBtn.Enabled = true;
                this.importGoodBtn.Enabled = false;
                this.importDocBtn.Enabled = false;
                this.uploadBtn.Enabled = false;
                this.downloadBtn.Enabled = false;
                this.settingsBtn.Enabled = false;
                //OnProcessImport += new ProcessImport(Form1_OnProcessImport);
                //OnFinishImport += new FinishImport(Form1_OnFinishImport);
                //OnFailedImport += new FailedImport(Form1_OnFailedImport);
                //loadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginImport));
                //loadThread.Start(openFileDialog1.FileName);
                
                //stopGoodBtn.Enabled = false;
                //stopDocsBtn.Enabled = true;
                //this.importGoodBtn.Enabled = false;
                //this.importDocBtn.Enabled = false;
                //this.uploadBtn.Enabled = false;
                //this.downloadBtn.Enabled = false;
                //this.settingsBtn.Enabled = false;

            }
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Начало загрузки ...\n");
            richTextBox1.AppendText("Подключите терминал и не отключайте до окончания загрузки\n");
            //string loadState = string.Empty;
            //StringBuilder sb = new StringBuilder();
            copyStatesb.Length = 0;
            bool success_upload = true;

            try
            {
                //OpenNETCF.Desktop.Communication.FileList fl = terminalRapi.EnumFiles(Properties.Settings.Default.TSDDBPAth + "ScannedBarcodes.db");
               
                terminalRapi.Connect(true, 5000);
                /*
                foreach (string s in loader.OldScannedFileList)
                {
                    try
                    {
                        if (System.IO.File.Exists(s))
                            System.IO.File.Delete(s);

                        string ext = Path.GetExtension(s).ToUpper();
                        terminalRapi.CopyFileFromDevice(s,
                            Properties.Settings.Default.TSDDBPAth +
                            //"\\Program Files\\tsdfamilia\\" + 
                            Path.GetFileName(s));
                        success_upload = true;
                        //copyStatesb.AppendFormat("Файл {0} скопирован.\n", s);
                        //loadState = "Успешно";
                    }
                    catch (Exception err)
                    {
                        richTextBox1.AppendText(string.Format("Ошибка: Файл {0} не скопирован. \n",s));
                        copyStatesb.AppendFormat("Ошибка: Файл {0} не скопирован.\n", s);
                        success_upload = false;
                        //loadState = "Ошибка";
                    }
                }*/
                //if (copyStatesb.Length > 0)
                {//have error
                    copyStatesb.Length = 0;
                    //loadState = string.Empty;
                    foreach (string s in loader.NewScannedFileList)
                    {
                        try
                        {
                            if (!terminalRapi.DevicePresent)
                            {
                                copyStatesb.AppendFormat("Терминал не подключен.\n");
                                copyStatesb.AppendFormat("Файл {0} не скопирован.\n", s);
                                //continue;

                                throw new ApplicationException(copyStatesb.ToString());
                            }

                            if (!terminalRapi.DeviceFileExists(
                                Properties.Settings.Default.TSDDBPAth +
                                Path.GetFileName(s)))
                            {
                                copyStatesb.AppendFormat("Файл {0} не скопирован - нет данных на терминале.\n", s);
                                throw new ApplicationException(copyStatesb.ToString());
                                //continue;
                            }

                            if (System.IO.File.Exists(s))
                                System.IO.File.Delete(s);

                            string ext = Path.GetExtension(s).ToUpper();
                            

                            terminalRapi.CopyFileFromDevice(s,
                                Properties.Settings.Default.TSDDBPAth +
                                //"\\Program Files\\tsdfamilia\\" + 
                                //@"Computer\WindowsCE\Storage Card\tsdfamilia\"+
                                Path.GetFileName(s));
                            

                            /*ShellLib.ShellFileOperation fo = new ShellLib.ShellFileOperation();

                            String[] source = new String[1];
                            String[] dest = new String[1];

                            source[0] = Properties.Settings.Default.TSDDBPAth +
                                //"\\Program Files\\tsdfamilia\\" + 
                                Path.GetFileName(s);

                                //Environment.SystemDirectory + @"\winmine.exe";
                            //source[1] = Environment.SystemDirectory + @"\freecell.exe";
                            //source[2] = Environment.SystemDirectory + @"\mshearts.exe";
                            dest[0] = s;
                                //Environment.SystemDirectory.Substring(0, 2) + @"\winmine.exe";
                            //dest[1] = Environment.SystemDirectory.Substring(0, 2) + @"\freecell.exe";
                            //dest[2] = Environment.SystemDirectory.Substring(0, 2) + @"\mshearts.exe";

                            fo.Operation = ShellLib.ShellFileOperation.FileOperations.FO_COPY;
                            fo.OwnerWindow = this.Handle;
                            fo.SourceFiles = source;
                            fo.DestFiles = dest;

                            bool RetVal = fo.DoOperation();
                            if (RetVal)
                                MessageBox.Show("Copy Complete without errors!");
                            else
                                MessageBox.Show("Copy Complete with errors!");

                            */

                            success_upload = success_upload & true;
                            //copyStatesb.AppendFormat("Файл {0} скопирован.\n", s);
                            //loadState = "Успешно";
                        }
                        catch (Exception err)
                        {
                            richTextBox1.AppendText(err.ToString());
                            richTextBox1.AppendText(string.Format("Ошибка: Файл {0} не скопирован. \n", s));
                            copyStatesb.AppendFormat("Файл {0} не скопирован.\n", s);
                            success_upload = false;
                            //loadState = "Ошибка";
                        }
                    }
                }
                if (success_upload)
                {
                    loader.UploadResults();

                    richTextBox1.AppendText("Загрузка завершена...\n");
                    richTextBox1.AppendText(copyStatesb.ToString());
                    success_upload = true;
                }
                //terminalRapi.CopyFileToDevice("register.txt", System.IO.Path.Combine(
                //        Properties.Settings.Default.TSDDBPAth, "register.txt"));
            }
            catch (Exception err)
            {
                richTextBox1.AppendText(err.Message.ToString());
                copyStatesb.Append(err.Message.ToString());
                success_upload = false;
            }
            finally
            {
                if (copyStatesb.Length == 0 && success_upload)
                {
                    MessageBox.Show("Загрузка завершена успешно.",
                        "Успех загрузки", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    DeleteOldDB();
                }
                else
                    MessageBox.Show(string.Format("Загрузка завершена с ошибкой: \n{0}",
                        copyStatesb.ToString()),
                        "Ошибка загрузки", MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);

                copyStatesb.Length = 0;
            }
        }

        private void DeleteOldDB()
        {
            try
            {
                //bool EraseTerminalDB = Properties.Settings.Default.EraseTerminalDB;
                //if (EraseTerminalDB)
                /*
                 *Нужна галка «автоматически очистить данные с ТСД». 
                 *Начальное значение галки при открытии формы брать из настроек программы (config  file). 
                 *Пользователь может пометь значение галки (on/off).
                 *Если галка стоит, то стираешь данные после копирования на удаленный ресурс как сейчас при опции 
                 *Измененное значение галки в CONFIG не записывать
                 * 
                 */
                if (cbEraseTerminalDB.Checked)
                {
                    foreach (string s in loader.NewScannedFileList)
                    {
                        try
                        {
                            //if (System.IO.File.Exists(s))
                            //    System.IO.File.Delete(s);

                            //string ext = Path.GetExtension(s).ToUpper();
                            terminalRapi.DeleteDeviceFile(
                                Properties.Settings.Default.TSDDBPAth +
                                Path.GetFileName(s));
                        }
                        catch
                        {
                            richTextBox1.AppendText(
                                string.Format("Ошибка стирания сканированных результатов на терминале - файла [0}\n",s));
                        }
 
                    }
                    foreach (string s in loader.OldScannedFileList)
                    {
                        try
                        {
                            //if (System.IO.File.Exists(s))
                            //    System.IO.File.Delete(s);

                            //string ext = Path.GetExtension(s).ToUpper();
                            terminalRapi.DeleteDeviceFile(
                                Properties.Settings.Default.TSDDBPAth +
                                Path.GetFileName(s));
                        }
                        catch
                        {
                            richTextBox1.AppendText(
                                string.Format("Ошибка стирания сканированных результатов на терминале - файла [0}\n", s));
                        }

                    }
                }
            }
            catch {
               // richTextBox1.AppendText("Ошибка стирания сканированных результатов на терминале\n");
            }
                
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            LoadDocs();
        }


    }


}
