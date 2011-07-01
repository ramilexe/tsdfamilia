using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace WatchDog
{
    public partial class Form1 : Form
    {
         string               _startupPath;// = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
        System.Threading.Timer tmr = null;
        System.Threading.Timer closing_tmr = null;

        bool updating = false;
        bool started = false;

        static object locker =
            new object();

        private System.Diagnostics.Process process = null;
        DateTime lastFileDate = DateTime.Now;

        //System.Reflection.Assembly asm = null;
        //Form main_frm = null;
        //System.Type pType = null;

        

        public Form1()
        {
            InitializeComponent();
            tmr = new System.Threading.Timer(
               new TimerCallback(this.OnTimerCallback),
               null, Timeout.Infinite, Timeout.Infinite);

            closing_tmr = new System.Threading.Timer(
              new TimerCallback(this.OnClosingTimerCallback),
              null, Timeout.Infinite, Timeout.Infinite);

            _startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 235;
            this.Height = 295;
            process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(_startupPath,"TSDClient.exe"), string.Empty);
            process.Exited += new EventHandler(process_Exited);
            tmr.Change(100, 60000);
            closing_tmr.Change(10000, 500);
            this.Closing += new CancelEventHandler(Form1_Closing);
            
        }

        void Form1_Closing(object sender, CancelEventArgs e)
        {
            
        }

        void process_Exited(object sender, EventArgs e)
        {
            //tmr.Change(Timeout.Infinite, Timeout.Infinite);
            //Application.Exit();
        }

        private void OnClosingTimerCallback(object state)
        {
            lock (locker)
            {
                if (!updating)
                    return;

                if (started)
                    if (process.HasExited)
                        Application.Exit();
            }


        }

        private void OnTimerCallback(object state)
        {
            if (this.InvokeRequired)
            {
                TimerCallback del = new TimerCallback(OnTimerCallback);
                this.Invoke(del, state);
            }
            else
            {
                lock (locker)
                {
                    try
                    {
                        updating = true;

                        System.IO.FileInfo fi = new System.IO.FileInfo(System.IO.Path.Combine(_startupPath, "TSDClient.ex_"));
                        fi.Refresh();
                        if (fi.Exists)
                        {
                            if (lastFileDate != fi.LastWriteTime)
                            {
                                this.listBox1.Items.Add("Файлы отличаются");
                                process.Refresh();
                                try
                                {
                                    if (/*main_frm != null)//*/
                                        process.Id != 0 && 
                                        process.MainWindowHandle != IntPtr.Zero)
                                    {
                                        this.listBox1.Items.Add("Ожидание закрытия программы");
                                        //main_frm.Close();
                                        //process.CloseMainWindow();
                                        process.Kill();
                                        process.WaitForExit();
                                        started = false;
                                        RefreshFile();
                                        lastFileDate = fi.LastWriteTime;
                                        this.listBox1.Items.Add("Файл обновлен");

                                    }
                                    else
                                    {
                                        RefreshFile();
                                        lastFileDate = fi.LastWriteTime;
                                        this.listBox1.Items.Add("Файл обновлен");
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    RefreshFile();
                                    lastFileDate = fi.LastWriteTime;
                                    this.listBox1.Items.Add("File changed");
                                }
                            }
                        }
                    }
                    finally
                    {
                        updating = false;
                    }
                }
            }
        }

        public void RenameFile(string sourceName, string targetName)
        {
            System.IO.File.Delete(targetName);
            System.IO.File.Copy(sourceName, targetName);

            /*using (System.IO.FileStream fs =
                    System.IO.File.OpenRead(sourceName))
            {

                using (System.IO.FileStream fs1 =
                    System.IO.File.OpenWrite(targetName))
                {
                    long fLength = fs.Length;
                    int offset = 0;
                    byte[] bArray = new byte[4096];
                    int readed = 0;

                    while ((readed = fs.Read(bArray, 0, bArray.Length)) > 0)
                    {
                        fs1.Write(bArray, 0, readed);
                        offset += readed;
                    }
                    fs1.Flush();
                    fs1.Close();

                }
                fs.Close();
            }*/
        }
        void RefreshFile()
        {
            //lock (locker)
            //{
                
                //this.listBox1.Items.Add("Delete old file");
                
                RenameFile(System.IO.Path.Combine(_startupPath, "TSDClient.ex_"),
                    System.IO.Path.Combine(_startupPath, "TSDClient.exe"));

                //System.IO.File.Delete("TSDClient.exe");
                /*using (System.IO.FileStream fs =
                        System.IO.File.OpenRead(System.IO.Path.Combine(_startupPath, "TSDClient.ex_")))
                {

                    using (System.IO.FileStream fs1 =
                        System.IO.File.OpenWrite(System.IO.Path.Combine(_startupPath, "TSDClient.exe")))
                    {
                        long fLength = fs.Length;
                        int offset = 0;
                        byte[] bArray = new byte[4096];
                        int readed = 0;

                        while ((readed = fs.Read(bArray, 0, bArray.Length)) > 0)
                        {
                            fs1.Write(bArray, 0, readed);
                            offset += readed;
                        }
                        fs1.Flush();
                        fs1.Close();

                    }
                    fs.Close();
                }*/

                //asm = System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(_startupPath, "TSDClient.exe"));
                //object program = asm.CreateInstance("Program");
                //System.Reflection.Module[] modules =
                //    asm.GetModules();
                //System.Type[] t = asm.GetTypes();

                //pType = asm.GetType("TSDServer.MainForm");

                //System.Reflection.ConstructorInfo ci = pType.GetConstructor(new Type[]{});
                //object frm = ci.Invoke(null);
                //main_frm = (Form)frm;
                //main_frm.Closed += new EventHandler(main_frm_Closed);
                //main_frm.Show();
                process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(_startupPath, "TSDClient.exe"), string.Empty);
                //process.Exited += new EventHandler(process_Exited);
                process.Start();
                started = true;
                this.listBox1.Items.Clear();
           // }
        }

        //void main_frm_Closed(object sender, EventArgs e)
        //{
        //    main_frm = null;
        //    //throw new NotImplementedException();
        //}
    }
}