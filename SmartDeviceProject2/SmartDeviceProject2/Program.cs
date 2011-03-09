using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public delegate void DatabaseChanged();
    static class Program
    {
        
        public static SystemMemoryChangeStatusEnum SystemMemoryChangeStatus = SystemMemoryChangeStatusEnum.SYSMEM_NEEDREBOOT;
        public static SettingsDataSet Settings = new SettingsDataSet();
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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            if (System.IO.File.Exists("BTLog.txt"))
                System.IO.File.Delete("BTLog.txt");

            _startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
            string settingFilePath = System.IO.Path.Combine(StartupPath, "Settings.xml");
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


                Application.Run(new Form1());
            }
            catch (Exception err)
            {
                //MessageBox.Show(err.Message);
                BTPrintClass.PrintClass.SetErrorEvent("Global error: {0}, Stack: {1}", err.Message, err.StackTrace);
            }
            finally
            {
                ScanClass.Scaner.StopScan();
                try
                {
                    
                    Settings.WriteXml(settingFilePath);
                    BTPrintClass.PrintClass.Disconnect();
                }
                catch { }
            }
        }
    }
}