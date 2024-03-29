﻿using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace TSDServer
{
    public partial class MainForm : Form
    {
        System.Threading.Thread thread = null;
        private WaitForm waitfrm =null;// new WaitForm();

        
        //string settingFilePath = string.Empty;
        //public static string СurrentInvId = string.Empty;
        //public static SystemMemoryChangeStatusEnum SystemMemoryChangeStatus;
        //public static SettingsDataSet Settings = null;
        //static string _startupPath = string.Empty;
        //static Int32 _terminal_id;
        //public static Int32 TerminalId
        //{
        //    get
        //    {
        //        return _terminal_id;
        //    }

        //}
        //public static SettingsDataSet.TypedSettingsRow Default
        //{
        //    get
        //    {
        //        return Settings.TypedSettings[0];
        //    }
        //}
        //public static string StartupPath
        //{
        //    get
        //    {
        //        return _startupPath;//return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
        //    }

        //}

        private enum MenuItems : int
        {
            ExitMenu = 48,
            ViewProductMenu = 49,
            InventarMenu = 50,
            IncomeMenu = 51,
            TTNForm = 52,
            ViewSettingMenu = 53,
            SimpleIncome = 54,
            ReturnBoxes = 55

            
        }

        public static event DatabaseChanged OnDatabaseChaned;
        //ProductsDataSet _products = ActionsClass.Action.Products;
        //ScannedProductsDataSet _scannedProducts = ActionsClass.Action.ScannedProducts;

        System.Threading.Timer tmr =
            new System.Threading.Timer(new System.Threading.TimerCallback(OnTimer)
                , null
                , System.Threading.Timeout.Infinite
                , System.Threading.Timeout.Infinite);
        private static DateTime lastCreationDatabaseTime = DateTime.Now;
        
        private static void OnTimer(object state)
        {
            /*DateTime dt = System.IO.File.GetCreationTime("Products.sdf");
            if (dt != lastCreationDatabaseTime)
            {
                if (OnDatabaseChaned != null)
                    OnDatabaseChaned();
            }*/
                
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //InitProgram();
            //List<Button > btnList = new List<Button>();
            //for (int i=0;i<this.Controls.Count;i++)
            //{
            //    if (this.Controls[i] is Button)
            //        btnList.Add(this.Controls[i] as Button);
            //}
            //foreach (Button btn in btnList)
            //{
            //    btn.Height = this.ClientSize.Height / btnList.Count;
            //}

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName an = a.GetName();
            //an.Version.ToString();
            this.Text = string.Format("{0} - {1}", an.Version, Program.Default.TerminalID);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            for (int i = 0;  i < this.Controls.Count; i++)
            {
                if (this.Controls[i].Name.IndexOf("button")>=0)
                {
                    System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)this.Controls[i];
                    b.Click += new EventHandler(button_Click);
                    b.KeyDown += new KeyEventHandler(b_KeyDown);
                    b.GotFocus += new EventHandler(b_GotFocus);
                    b.LostFocus += new EventHandler(b_LostFocus);
                    b.KeyPress += new KeyPressEventHandler(b_KeyPress);
                    System.Drawing.Font f =
                        new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);

                    b.Font = f;
                    b.Height = 30;
                }
            }
            if (Program.Default.EnableExit != 1)
                button0.Enabled = false;
            else
                button0.Enabled = true;

            //ActionsClass.Action.LoadScannedData();
            ActionsClass.Action.BeginScan();

            //string oemInfo = NativeClass.GetOemInfo();

            //if (oemInfo.ToUpper().IndexOf("PY055") >= 0 ||
            //  oemInfo.ToUpper().IndexOf("HP101") >= 0 ||
            //    oemInfo.ToUpper().IndexOf("EMULATOR") >= 0
            //  )
            //{
                 
            //    FunctionButtons.RedBtn = 18;
            //    FunctionButtons.BlueBtn = 16;
            //    FunctionButtons.GreenBtn = 9;
            //    FunctionButtons.YellowBtn = 115;

            //}
            //else
            //{
            //    if (oemInfo.ToUpper().IndexOf("M3MOBILE") >= 0)
            //    {
            //        FunctionButtons.RedBtn = 193;
            //        FunctionButtons.BlueBtn = 194;
            //        FunctionButtons.GreenBtn = 195;
            //        FunctionButtons.YellowBtn = 196;

            //    }
            //}
            //onEndLoad += new EndLoadDelegate(MainForm_onEndLoad);
        }

        void MainForm_onEndLoad()
        {
                //EndLoadDelegate del = new EndLoadDelegate(MainForm_onEndLoad);
                //if (waitfrm != null)
                //    waitfrm.Invoke(del);
            
        }

        void b_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        void b_LostFocus(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)sender;

            b.BackColor = Color.PaleGreen;
            System.Drawing.Font f =
                new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);

            b.Font = f;
            
        }

        void b_GotFocus(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b
             = (System.Windows.Forms.Button)sender;

            b.BackColor = Color.Plum;

            System.Drawing.Font f =
                new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold|FontStyle.Underline);

            b.Font = f;
            
        }

        void b_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is System.Windows.Forms.Button)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    MenuEvents((int)MenuItems.ExitMenu);
                    return;
                }

                if (e.KeyValue == 13 || e.KeyCode == Keys.Enter)
                {
                    System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)sender;

                    if (b.Name.IndexOf("button") >= 0)
                    {
                        string id = b.Name.Replace("button", "");
                        MenuEvents(id[0]);
                    }
                    return;
                }
                else
                {
                    MenuEvents(e.KeyValue);
                }
            }
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void button_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b
                         = (System.Windows.Forms.Button)sender;

            if (b.Name.IndexOf("button") >= 0)
            {
                string id = b.Name.Replace("button", "");
                MenuEvents(id[0]);

            }
        }

        void PrintClass_OnSetError(string text)
        {
            //throw new NotImplementedException();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            MenuEvents(e.KeyChar);
        }

        void MenuEvents(char menuId)
        {
            MenuEvents(Convert.ToInt32(menuId));
        }

        

        void MenuEvents(int menuId)
        {
            try
            {
                switch (menuId)
                {

                    case (int)MenuItems.ExitMenu:
                        {
                            if (Program.Default.EnableExit == 1)
                                Application.Exit(); 
                            break;
                        }
                    case (int)MenuItems.ViewProductMenu:
                        {
                            
                            BTPrintClass.PrintClass.CheckForClear();
                            ActionsClass.Action.OpenProducts();
                            ActionsClass.Action.OpenScanned();
                            using (ViewProductForm frm = new ViewProductForm())
                            {
                                //this.Visible = false;
                                frm.ShowDialog();
                            }
                            ActionsClass.Action.CloseProducts();
                            ActionsClass.Action.ClosedScanned();
                            ActionsClass.Action.ClearCache();
                           // this.Visible = true;
                            //this.Activate();
                            break;
                        }
                    case (int)MenuItems.InventarMenu:
                        {
                            
                            BTPrintClass.PrintClass.CheckForClear();
                            ActionsClass.Action.OpenProducts();
                            ActionsClass.Action.OpenScanned();
                            using (InventarForm frm = new InventarForm())
                            {
                                //this.Visible = false;
                                frm.ShowDialog();
                            }
                            ActionsClass.Action.CloseProducts();
                            ActionsClass.Action.ClosedScanned();
                            ActionsClass.Action.ClearCache();
                            //this.Visible = true;
                            //this.Activate();
                            break;
                        }
                    case (int)MenuItems.SimpleIncome:
                        {

                            BTPrintClass.PrintClass.CheckForClear();
                            ActionsClass.Action.OpenProducts();
                            ActionsClass.Action.OpenScanned();
                            using (InventarForm frm = new InventarForm(InventarFormMode.SimpleIncome))
                            {
                                //this.Visible = false;
                                frm.ShowDialog();
                            }
                            ActionsClass.Action.CloseProducts();
                            ActionsClass.Action.ClosedScanned();
                            ActionsClass.Action.ClearCache();
                            //this.Visible = true;
                            //this.Activate();
                            break;
                        }
                    case (int)MenuItems.IncomeMenu:
                        {
                            //this.Visible = false;
                            BTPrintClass.PrintClass.CheckForClear();
                            ActionsClass.Action.OpenProducts();
                            ActionsClass.Action.OpenScanned();
                            using (
                                IncomeForm frm = new IncomeForm())
                            {
                                frm.ShowDialog();
                            }
                            ActionsClass.Action.CloseProducts();
                            ActionsClass.Action.ClosedScanned();
                            ActionsClass.Action.ClearCache();
                            //this.Visible = true;
                            //this.Activate();
                            //BTPrintClass.PrintClass.SetStatusEvent("Form Closed");
                            break;
                        }
                    case (int)MenuItems.TTNForm:
                        {

                            //this.Visible = false;


                            //thread = new System.Threading.Thread(
                            //    new System.Threading.ThreadStart(
                            //        frm_onBeginLoad));
                            //thread.IsBackground = false;


                            //thread.Start();

                            using (TtnForm frm = new TtnForm())
                            {
                                frm.onEndLoad += new EndLoadDelegate(frm_onEndLoad);
                                BTPrintClass.PrintClass.CheckForClear();
                                ActionsClass.Action.OpenProducts();
                                ActionsClass.Action.OpenScanned();
                                //this.Visible = false;
                                frm.ShowDialog();
                               //this.Visible = true;
                            }
                            ActionsClass.Action.CloseProducts();
                            ActionsClass.Action.ClosedScanned();
                            ActionsClass.Action.ClearCache();
                            //this.Visible = true;
                            //this.Activate();
                            break;

                        }
                    case (int)MenuItems.ReturnBoxes:
                        {

                            //this.Visible = false;


                            //thread = new System.Threading.Thread(
                            //    new System.Threading.ThreadStart(
                            //        frm_onBeginLoad));
                            //thread.IsBackground = false;


                            //thread.Start();

                            using (ReturnForm frm = new ReturnForm())
                            {
                                frm.onEndLoad += new EndLoadDelegate(frm_onEndLoad);
                                BTPrintClass.PrintClass.CheckForClear();
                                ActionsClass.Action.OpenProducts();
                                ActionsClass.Action.OpenScanned();
                                //this.Visible = false;
                                frm.ShowDialog();
                                //this.Visible = true;
                            }
                            ActionsClass.Action.CloseProducts();
                            ActionsClass.Action.ClosedScanned();
                            ActionsClass.Action.ClearCache();
                            //this.Visible = true;
                            //this.Activate();
                            break;

                        }
                    case (int)MenuItems.ViewSettingMenu:
                        {
                            BTPrintClass.PrintClass.CheckForClear();
                            using (SettingsForm frm = new SettingsForm())
                            {
                                //this.Visible = false;
                                frm.ShowDialog();
                            }
                           // this.Visible = true;
                            //this.Activate();
                            break;
                        }


                    default:
                        {
                            break;
                        }

                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message
                    , "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
            this.Refresh();
            BTPrintClass.PrintClass.SetStatusEvent("Main Form Refreshed");
        }
        System.Threading.Timer refreshTimer = null;

        void frm_onBeginLoad()
        {

            
            waitfrm = new WaitForm();

            refreshTimer = new System.Threading.Timer(
                new System.Threading.TimerCallback(onRefreshWaitForm),
                waitfrm,
                0,
                100);

            waitfrm.Show();

            waitfrm.Refresh();


        }

        void onRefreshWaitForm(object target)
        {
            //if (this.InvokeRequired)
            //{
            //    System.Threading.TimerCallback del =
            //        new System.Threading.TimerCallback(onRefreshWaitForm);
            //    this.Invoke(del, target);
            //}
            //else
            //{
            //if (((WaitForm)target).OutRefresh != null)
             ((WaitForm)target).WaitForm_OutRefresh();

            //}
        }
        void frm_onEndLoad()
        {
            try
            {
                refreshTimer.Change(System.Threading.Timeout.Infinite,
                    System.Threading.Timeout.Infinite);

                //waitfrm.Close();
                //waitfrm.Dispose();
                //waitfrm = null;
                thread.Abort();

            }
            catch { }
            
        }

        private void FillData()
        {


        }
        /*
        private void InitProgram()
        {
            try
            {
                //BTPrintClass.PrintClass.SetErrorEvent("Started");

                SystemMemoryChangeStatus = SystemMemoryChangeStatusEnum.SYSMEM_NEEDREBOOT;
                Settings = new SettingsDataSet();

                
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


                    //Application.Run(new Form1());
                }
                catch (Exception err)
                {
                    //MessageBox.Show(err.Message);
                    BTPrintClass.PrintClass.SetErrorEvent("Global error: {0}, Stack: {1}", err.Message, err.StackTrace);
                }
                finally
                {
                    //ScanClass.Scaner.StopScan();
                    ////try
                    ////{

                    //Settings.WriteXml(settingFilePath);
                    //BTPrintClass.PrintClass.Disconnect();
                    //}
                    //catch { }
                }
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.ToString());
            }
        }
*/
        void Form1_Closed(object sender, System.EventArgs e)
        {
            //ScanClass.Scaner.StopScan();
            //Settings.WriteXml(settingFilePath);
            //BTPrintClass.PrintClass.Disconnect();
        }

    }

    public delegate void EndLoadDelegate();
}