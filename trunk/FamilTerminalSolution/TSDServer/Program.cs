using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
using log4net;
using log4net.Config;
using Microsoft.Win32;

namespace TSDServer
{
    static class Program
    {
        enum ERRORLEVELS : byte
        {
            NoError =0,
            SomeException =1,
            TimeOut = 2,
            ServerAlreadeyStarted=3,
            IncorrectParametrs =4,
            NotInstalledActiveSync=5
        }
#region old
        /*
        class MyApplicationContext : ApplicationContext
        {
            Form1 mainForm = null;
            System.Threading.Timer tmr = null;

            public MyApplicationContext(string[] args)
            {
                if (args.Length == 4 && args[0].ToLower() == "/c")
                {
                    Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
                    tmr = 
                       new System.Threading.Timer(
                           new System.Threading.TimerCallback(OnTimer),
                           null, System.Threading.Timeout.Infinite,
                           System.Threading.Timeout.Infinite);
                   

                    //c:\111\famil.txtx c:\111\famil_doc.txt  c:\222\
                    InProductName = args[1];
                    InDocName = args[2];
                    Properties.Settings.Default.LocalFilePath = args[3];

                    mainForm = new Form1();
                    mainForm.Closed += new EventHandler(OnFormClosed);
                    //mainForm.Load += new EventHandler(mainForm_Load);
                    mainForm.Shown += new EventHandler(mainForm_Shown);
                   // this.MainForm = mainForm;
                    
                    //mainForm.Closing += new CancelEventHandler(OnFormClosing);  
                    //mainForm.Show();
                    
                }
            }



            void mainForm_Shown(object sender, EventArgs e)
            {
                mainForm.OnFinishImport += new Form1.FinishImport(mainForm_OnFinishImport);
                mEvt.Reset();
                tmr.Change(0, 500);
                mainForm.AutoLoadProduct(InProductName);

                if (mEvt.WaitOne(1000 * 15 * 60, false) == false)
                {
                    Console.WriteLine("ERROR LOAD PRODUCT - TIMEOUT");
                }
                mEvt.Reset();
                mainForm.AutoLoadDoc(InDocName);

                if (mEvt.WaitOne(1000 * 15 * 60, false) == false)
                {
                    Console.WriteLine("ERROR LOAD DOCS - TIMEOUT");
                }
                mainForm.Close();
            }

            
            private void OnApplicationExit(object sender, EventArgs e)
            {
            
            }


            //private void OnFormClosing(object sender, CancelEventArgs e)
            //{
            //    // When a form is closing, remember the form position so it
            //    // can be saved in the user data file.
            //    //if (sender is AppForm1)
            //    //    form1Position = ((Form)sender).Bounds;
            //    //else if (sender is AppForm2)
            //    //    form2Position = ((Form)sender).Bounds;
            //}


            private void OnFormClosed(object sender, EventArgs e)
            {
                 ExitThread();
            }

            void OnTimer(object state)
            {

                //this.MainForm.Invalidate();
                Application.DoEvents();
            }

            void mainForm_OnFinishImport(string fileName)
            {
                mEvt.Set();
            }
        }
        */
#endregion
        public static readonly ILog log = LogManager.GetLogger(typeof(Program));

        //главное окно программы
        static Form1 mainForm = null;
        public static SendMailAttach.SendMailClass sendmail;
        public static bool AutoMode = false;
        public static string InProductName = "";
        public static string InDocName = "";

        //класс сервера удаленного управления
        static RemoteObject ro = new RemoteObject();
        public static string CurrentPath
        {
            get
            {
                return Application.StartupPath;
            }
        }
        static System.Threading.ManualResetEvent mEvt = new System.Threading.ManualResetEvent(false);

        /// <summary>
        /// статический метод для вызова метода Show главного окна программы
        /// </summary>
        public static void Show()
        {
            mainForm.Show();
            mainForm.Activate();
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand)]
        [STAThread]
        static int Main(string[] args)
        {

            System.IO.FileInfo fi = new System.IO.FileInfo(
                            System.IO.Path.Combine(Application.StartupPath,
                            "log4netconfig.xml"));
            XmlConfigurator.Configure(fi);
            
            using (RegistryKey reg =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows CE Services"))
            {
                if (reg == null)
                {
                    MessageBox.Show("Не установлен Active Sync или Windows Mobile Device Center (для Vista/Windows 7)",
                        "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return (int)ERRORLEVELS.NotInstalledActiveSync;
                }
                //int majorVersion = (int)reg.GetValue("MajorVersion", 0);
                //int minorVersion = (int)reg.GetValue("MinorVersion", 0);
                //int buildNumber = (int)reg.GetValue("BuildNumber", 0);
                //syncVersion = new Version(majorVersion, minorVersion, buildNumber);
            }
            

            sendmail = new SendMailAttach.SendMailClass(Properties.Settings.Default.AddressFrom,
                Properties.Settings.Default.UserNameFrom,
                false,
                Properties.Settings.Default.SmtpClient);


            if (args.Length > 0 && args[0].ToLower() == "/c")
            {
                if (args.Length == 4 && args[0].ToLower() == "/c")
                {
                    try
                    {
                        InProductName = args[1];
                        InDocName = args[2];
                        Properties.Settings.Default.LocalFilePath = args[3];
                        
                        

                        DataLoaderClass loader = new DataLoaderClass();
                        loader.OnFinishImport += new DataLoaderClass.FinishImport(loader_OnFinishImport);
                        mEvt.Reset();
                        loader.AutoLoadProduct(InProductName);

                        if (mEvt.WaitOne(1000 * 15 * 60, false) == false)
                        {
                            Console.WriteLine("ERROR LOAD PRODUCT - TIMEOUT");
                            return (int)ERRORLEVELS.TimeOut;
                        }

                        mEvt.Reset();
                        loader.AutoLoadDoc(InDocName);

                        if (mEvt.WaitOne(1000 * 15 * 60, false) == false)
                        {
                            Console.WriteLine("ERROR LOAD DOCS - TIMEOUT");
                            return (int)ERRORLEVELS.TimeOut;
                        }
                        return (int)ERRORLEVELS.NoError;
                    }
                    catch (Exception err)
                    {
                        log.Fatal(err);
                        
                        sendmail.SendMail(
                            new string[] { "", "Ошибка загрузки данных", err.ToString(), 
                                Properties.Settings.Default.AddressToList, "", "" });
                        MessageBox.Show(err.ToString());
                        return (int)ERRORLEVELS.SomeException;
                    }

                }
                else
                    return (int)ERRORLEVELS.IncorrectParametrs;
            }
            #region oldtest
            /*ProductsDataSet ds = new ProductsDataSet();
            
            using (System.IO.StreamWriter wr = new System.IO.StreamWriter("documents1.txt"))
            {
                using (ProductsDataSetTableAdapters.DocsBinTblTableAdapter ta =
                    new TSDServer.ProductsDataSetTableAdapters.DocsBinTblTableAdapter())
                {

                    ta.Fill(ds.DocsBinTbl);
                }
                string[] s = new string [ds.DocsTbl.Columns.Count];
                string stringToWrite = string.Empty;
                foreach (ProductsDataSet.DocsBinTblRow row in ds.DocsBinTbl)
                {
                    ProductsDataSet.DocsTblRow docRow = ds.ConvertFromBin(row);
                    for (int i = 0; i < docRow.Table.Columns.Count; i++)
                    {
                        s[i] = docRow[i].ToString();
                    }
                    stringToWrite = String.Join("|", s);
                    wr.WriteLine(stringToWrite);
                }
                wr.Flush();
                wr.Close();
            }
            return;*/
            //}
            /*
            byte [] ba = new byte[155];
            for (byte b = 0; b < 155; b++)
            {
                ba[b] = (byte)(b+100);
            }
            string s = System.Text.Encoding.GetEncoding("windows-1251").GetString(ba);
            TSDServer.CustomEncodingClass custEnc = new CustomEncodingClass();
            byte [] ba = custEnc.GetBytes("ХОЗЯЙСТВЕННЫЙ САНТИМЕТР|В АСС.|Китай|357792|ПЛАСТИК");
            
            string s = System.Text.Encoding.GetEncoding("windows-1251").GetString(ba);

            return;*/
            
            /*
            using (System.Data.SqlServerCe.SqlCeConnection conn =
                new System.Data.SqlServerCe.SqlCeConnection(Properties.Settings.Default.ProductsConnectionString))
            {
                conn.Open();
                using (System.Data.SqlServerCe.SqlCeCommand cmd =
                    new System.Data.SqlServerCe.SqlCeCommand("select * from productsbintbl", conn))
                {
                    using (System.Data.SqlServerCe.SqlCeCommand cmdUpd =
                    new System.Data.SqlServerCe.SqlCeCommand(@"UPDATE    ProductsBinTbl SET ActionCode = @ac
, Shablon = @sc
, SoundCode = @sndc
WHERE     (ProductsBinTbl.Barcode = @b)", conn))
                    {
                        
                        cmdUpd.Parameters.Add("@ac", typeof(byte));
                        cmdUpd.Parameters.Add("@sc", typeof(int));
                        cmdUpd.Parameters.Add("@sndc", typeof(int));
                        cmdUpd.Parameters.Add("@b", typeof(Int64));
                        System.Random r = new Random();
                        Array vals = Enum.GetValues(typeof(TSDUtils.ActionCode));
                        Array vals1 = Enum.GetValues(typeof(TSDUtils.ShablonCode));

                        using (System.Data.SqlServerCe.SqlCeDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                
                                Int64 bc =  (Int64)rdr[0];
                                cmdUpd.Parameters[3].Value = bc;
                                byte c = 0;
                                
                                
                                for (int k=0;k<5;k++)
                                
                                {
                                    int b = 0;
                                    Double d = Math.Round(r.NextDouble());//произвольное число от 0 до 1
                                    //при округлении получаем случайное значение 0 или 1
                                    b = (byte)((byte)vals.GetValue(k) * ((byte)d));//Если d=0, то указанный k-й код действия не используется,
                                    //иначе, если 1 - то используется
                                    c = (byte)(b|c);//суммируем все биты
                                }
                                uint sum = 0;
                                //по каждому биту действия
                                for (byte k = 0; k < 8; k++)
                                {
                                    //определить произвольный код шаблона
                                    byte d1 = (byte)r.Next( 8);
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

                                cmdUpd.Parameters[0].Value = c;
                                cmdUpd.Parameters[1].Value = sum;
                                cmdUpd.Parameters[2].Value = sum;
                                //cmdUpd.ExecuteNonQuery();

                            }


                        }
                    }

                }


            }
            return;*/

            //TSDUtils.ActionCode a = TSDUtils.ActionCode.Remove | TSDUtils.ActionCode.Reprice;
            //TSDUtils.ActionCode b = TSDUtils.ActionCode.Remove | TSDUtils.ActionCode.Returns;
            //TSDUtils.ActionCode c = TSDUtils.ActionCode.Remove | TSDUtils.ActionCode.Returns | TSDUtils.ActionCode.Reprice;

            /*Array e = Enum.GetValues(typeof(TSDUtils.ActionCode));
            int counter = 0;
            byte[] bArray = new byte[e.Length];
            string s = string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (TSDUtils.ActionCode i in e)
            {
                byte b = (byte)i;
                bArray[counter++] = b;
                sb.AppendFormat("{0} = {1} \n", i, b);
            }
            TSDUtils.ActionCode tmp = TSDUtils.ActionCode.NoAction;
            

            for (int i = 0; i < bArray.Length; i++)
            {
                tmp = TSDUtils.ActionCode.NoAction;
                for (int j = 0; j < bArray.Length; j++)
                {
                    if (bArray[i] == bArray[j])
                        continue;

                    tmp = tmp |(TSDUtils.ActionCode) bArray[j];
                    byte b = (byte)tmp;
                    sb.AppendFormat("{0} = {1} \n", tmp, b);

                }
            }
            */
            #endregion

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                mainForm = new Form1();
                //проверка на наличие второй запущеной копии программы
                //если Null значит уже запущена другая копия
                if (mainForm.mutex != null)
                {
                    //другая копия программы не запущена
                    //инициализируем IPC сервер, который может принимать сообщения
                    //(в данном случае нужно для получения сообщения от второй копии показать главное окно
                    IpcChannel serverChannel =
                        new IpcChannel("localhost:9090");

                    System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(
                        serverChannel, false);

                    System.Runtime.Remoting.WellKnownServiceTypeEntry WKSTE =
                   new System.Runtime.Remoting.WellKnownServiceTypeEntry(
                       typeof(RemoteObject), "RemoteObject.rem", System.Runtime.Remoting.WellKnownObjectMode.Singleton);
                    System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownServiceType(WKSTE);

                    Application.Run(mainForm);//запуск главного экранного потока
                    return (int)ERRORLEVELS.NoError;
                }
                else
                {
                    //есть уже запущенная копия программы
                    IpcChannel channel = new IpcChannel();
                    System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);
                    //получаем адрес сервера программы
                    RemoteObject service = (RemoteObject)Activator.GetObject(
                                   typeof(RemoteObject), "ipc://localhost:9090/RemoteObject.rem");

                    //отправляем сообщение показать главное окно
                    service.Show();
                    return (int)ERRORLEVELS.ServerAlreadeyStarted;
                    //mainForm.Activate();
                    //выходим из программы

                }
            }
            catch
            {
                return (int)ERRORLEVELS.SomeException;
            }
            
        }

        static void loader_OnFinishImport(string fileName)
        {
            mEvt.Set();
        }

    }


// Remote object.
    /// <summary>
    /// класс для сервера управления
    /// </summary>
    public class RemoteObject :System.MarshalByRefObject
    {
        /// <summary>
        /// вызывает у главного класса програмы (Program) статический метод показать главное окно прораммы
        /// </summary>
    public void Show()
    {
        Program.Show();
    }
}



}
