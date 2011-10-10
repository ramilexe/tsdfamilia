using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace TSDServer
{
    public delegate void DatabaseChanged();
    public static class Program
    {
        
        /// <summary>
        /// Номер Текущего открытого просчета
        /// </summary>
        public static string СurrentInvId = string.Empty;
        public static SystemMemoryChangeStatusEnum SystemMemoryChangeStatus;
        public static SettingsDataSet Settings = null;
        static string _startupPath = string.Empty;
        static Int32 _terminal_id;
        public static Int32 TerminalId
        {
            get
            {
                return _terminal_id;
            }

        }
        public static SettingsDataSet.TypedSettingsRow Default
        {
            get
            {
                return Settings.TypedSettings[0];
            }
        }
        public static string StartupPath
        {
            get
            {
                return _startupPath;//return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
            }

        }
        //static System.Threading.ManualResetEvent mEvt =
        //    new System.Threading.ManualResetEvent(false);
        //static int WaitPrintTimeDefault = 1000;

        //static System.Threading.ManualResetEvent mEvt = new System.Threading.ManualResetEvent(true);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        public static void Main()
        {
           
            //mEvt.Reset();

            //bool result = mEvt.WaitOne((int)(WaitPrintTimeDefault + WaitPrintTimeDefault / 2), false);
            //mEvt.Reset();
            //MessageBox.Show("test");
            //mEvt.WaitOne((int)(WaitPrintTimeDefault + WaitPrintTimeDefault / 2), false);
            //mEvt.Reset();

            //System.Threading.Thread.Sleep(1000);
            //mEvt.Set();
            try
            {
                //BTPrintClass.PrintClass.SetErrorEvent("Started");

                SystemMemoryChangeStatus = SystemMemoryChangeStatusEnum.SYSMEM_NEEDREBOOT;
                Settings = new SettingsDataSet();

                string settingFilePath = string.Empty;
                //try
                //{
                _startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
                settingFilePath = System.IO.Path.Combine(StartupPath, "Settings.xml");

                //if (System.IO.File.Exists(System.IO.Path.Combine(_startupPath, "BTLog.txt")))
                //    System.IO.File.Delete(System.IO.Path.Combine(_startupPath, "BTLog.txt"));
                //}
                //catch (Exception err)
                //{
                //    BTPrintClass.PrintClass.SetErrorEvent("Global error: {0}, Stack: {1}", err.Message, err.StackTrace);

                //}
                try
                {


                    if (System.IO.File.Exists(settingFilePath))
                    {
                        Settings.ReadXml(settingFilePath);
                        if (Settings.TypedSettings[0]["TerminalID"] != System.DBNull.Value)
                            _terminal_id = Settings.TypedSettings[0].TerminalID;
                        else
                            _terminal_id = 0;
                    }
                    else
                    {
                        SettingsDataSet.TypedSettingsRow r = Settings.TypedSettings.NewTypedSettingsRow();
                        r.BTComPort = 9;
                        r.BTPrinterAddress = "00:03:7a:32:4c:55";
                        r.DatabaseFileName = "products.sdf";
                        r.DatabaseStoragePath = _startupPath;
                        r.StorageMemorySize = 15000;
                        r.BaseDate = new DateTime(2000, 1, 1);
                        r.ProductsConnectionString = @"Data Source=|DataDirectory|\Products.sdf";
                        r.DefaultRepriceShablon = 1;
                        r.TerminalID = 0;
                        r.BlueButtonShablon = 1;
                        r.EnableExit = 1;
                        r.EnableWorkWOPrinter = 1;
                        r.WaitPrintTimeDefault = 1000;
                        Settings.TypedSettings.AddTypedSettingsRow(r);
                        Settings.TypedSettings.AcceptChanges();

                        _terminal_id = 0;

                        Settings.WriteXml(settingFilePath);
                    }

                    int storePages = 0;
                    int ramPages = 0;
                    int pageSize = 0;
                    bool v = NativeClass.GetSystemMemoryDivision(ref storePages, ref ramPages, ref pageSize);

                    int currentMem = NativeClass.SystemStorageMemory;
                    int p = Math.Abs((100 * (currentMem - Settings.TypedSettings[0].StorageMemorySize)) /
                        Settings.TypedSettings[0].StorageMemorySize);

                    if (p > 5)
                    {
                        NativeClass.SystemStorageMemory = Settings.TypedSettings[0].StorageMemorySize;
                        SystemMemoryChangeStatus =
                            NativeClass.ChangeStatus;
                    }
                    //try
                    //{
                    //    BTPrintClass.PrintClass.Disconnect();
                    //}
                    //catch { }

                    //string oemInfo = NativeClass.GetOemInfo();
                    //BTPrintClass.PrintClass.SetStatusEvent(oemInfo);
                    Application.Run(new MainForm());
                }
                catch (Exception err)
                {
                    //MessageBox.Show(err.Message);
                    BTPrintClass.PrintClass.SetErrorEvent("Global error: {0}, Stack: {1}", err.Message, err.StackTrace);
                }
                finally
                {
                    ScanClass.Scaner.StopScan();
                    //try
                    //{

                    Settings.WriteXml(settingFilePath);
                    BTPrintClass.PrintClass.Disconnect();
                    //}
                    //catch { }
                }
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.ToString());
            }
        }
    }
    public enum SpecialButton:int
    {

        EscBtn = 9,
        EnterBtn=13,

        BlueBtn=16,
        RedBtn=18,
        GreenBtn=9,
        YellowBtn = 115
    }
}