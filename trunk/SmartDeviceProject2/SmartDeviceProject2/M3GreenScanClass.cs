using System;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TSDServer
{
    public class M3GreenScanClass : IScanClass
    {
        private static MC6500s.MC6500s _scaner = MC6500s.MC6500s.GetInstance();
        private static M3GreenScanClass _scn = new M3GreenScanClass();

        private M3GreenScanClass()
        {

        }
        public static M3GreenScanClass Scaner
        {
            get
            {
                return _scn;
            }
        }

        public Scanned OnScanned
        {
            get
            {
                return _OnScanned;
            }
            set
            {
                _OnScanned = value;
            }
        }
        private Scanned _OnScanned;
        
        private ScanError _OnScanError;
        public ScanError OnScanError
        {
            get
            {
                return _OnScanError;
            }
            set
            {
                _OnScanError = value;


            }
        }
        bool aborted = false;
        bool paused = true;
        ManualResetEvent mevt = new ManualResetEvent(false);
        public bool Paused
        {
            get { return paused; }
            set { 
                paused = value;
                //if (paused)
                //    mevt.Reset();
                //else
                //    mevt.Set();
            }
        }


        //[DllImport("coredll.dll")]
        //public static extern IntPtr GetForegroundWindow();

        public static IntPtr HWND;
        private System.Threading.Thread thread;
        //Barcode type
       
        /// <summary>
        /// init scan driver and start listen thread
        /// </summary>
        /// <returns></returns>
        public int InitScan()
        {

            if (!_scaner.Opened)
                _scaner.Open();
            _scaner.NsdUPCE = true;
            _scaner.UseRFID = false;
            _scaner.ExtendUPCA = true;
            _scaner.BarcodeRead += new MC6500s.BarcodeReadEventHandler(_scaner_BarcodeRead);
            _scaner.Init(
                new MC6500s.BarcodeType[]
                {MC6500s.BarcodeType.btEAN13,
                    MC6500s.BarcodeType.btEAN8,
                    MC6500s.BarcodeType.btUPCA,
                    MC6500s.BarcodeType.btUPCE});
            paused = false;
            return 0;
        }

        void _scaner_BarcodeRead(object sender, MC6500s.BarcodeReadEventArgs e)
        {
            if (!paused && OnScanned != null && !String.IsNullOrEmpty(e.Barcode))
                OnScanned(e.Barcode);
        }
        
        /// <summary>
        /// abort listen scan thread
        /// </summary>
        public void PauseScan()
        {
            Paused = true;
            //aborted = true;
            //try
            //{
            //    thread.Abort();
            //}
            //catch { }
            //thread = null;
        }

        /// <summary>
        /// restart listen scan thread
        /// </summary>
        public void ResumeScan()
        {
            Paused = false;
            //aborted = false;
            //thread = new Thread(new ThreadStart(start));
            //thread.Start();     //Start start thread
        }


        /// <summary>
        /// Close scan driver and stop listen thread
        /// </summary>
        public void StopScan()
        {
            OnScanned = null;
            paused = false;
            aborted = true;
            _scaner.Stop();
            _scaner.Close();
            
            //HWND = IntPtr.Zero;
            //textBox1.Text = "";
            //textBox2.Text = "";
            //textBox3.Text = "";
            //textBox4.Text = "";
            try
            {
                if (thread != null)
                    thread.Abort();								//Abort start thread
            }
            catch { };

        }


        private void SetScanError(string errorText)
        {
            if (OnScanError != null)
                OnScanError(errorText);

        }

    }

    
}
