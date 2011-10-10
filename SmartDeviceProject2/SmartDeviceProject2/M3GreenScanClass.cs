using System;

using System.Collections.Generic;
using System.Text;
using MCSSLibNet;
using System.Threading;
using System.Windows.Forms;

namespace TSDServer
{
    public class M3GreenScanClass:IScanClass
    {
        //public Scanned OnScanned;
        //public ScanError OnScanError;

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

        private static M3GreenScanClass _scaner = null;//new ScanClass();
        public static M3GreenScanClass Scaner
        {
            get
            {
                if (_scaner == null)
                    _scaner = new M3GreenScanClass();
                return _scaner;
            }
        }


        private MCSSLibNet.ScannerControl ScanCtrl;
        private MCSSLibNet.MCBarCodeType M3BarCodeType;
        private MCSSLibNet.MCModuleOption M3ModuleOption;

        private MCSSLibNet.MCReadOption M3ReadOption;

        // Flag
        public bool m_bReading;
        public bool m_bKeyFlag;
        public bool m_bSyncMode;
        public bool m_bResult;
        public int m_nResult;

        bool aborted = false;
        bool paused = true;

        private M3GreenScanClass()
        {
            ScanCtrl = new ScannerControl();
            M3BarCodeType = new MCBarCodeType();
            M3ModuleOption = new MCModuleOption();
            M3ReadOption = new MCReadOption();

            ScanCtrl.ScannerDataEvent += new ScannerDataDelegate(OnScanRead);

           

        }


        public void ScanRead()
        {
            if (m_bReading == true)
            {
                ScanCtrl.ScanReadCancel();
                m_bReading = false;
                return;
            }

            m_bReading = false;

            ScanCtrl.ScanRead();
        }
        private void SetScanError(string errorText)
        {
            if (OnScanError != null)
                OnScanError(errorText);

        }

        public void OnScanRead(object sender, ScannerDataArgs e)
        {
            try
            {
                if (e.ScanData != "")
                {
                    //string Text = e.ScanType;
                    if (!paused && OnScanned != null && !String.IsNullOrEmpty(e.ScanData))
                        OnScanned(e.ScanData);
                }
                
                
                
            }
            catch (Exception err)
            {
                SetScanError(err.Message);
            }
            

            m_bReading = false;
        }

        private void Tab_M3Scanner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.F22)
            {
                if (m_bKeyFlag == false)
                {
                    m_bKeyFlag = true;
                    ScanRead();
                    Thread.Sleep(10);
                }
            }
        }

        private void Tab_M3Scanner_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.F22)
            {
                if (m_bKeyFlag == true)
                {
                    m_bKeyFlag = false;
                    if (m_bSyncMode == false)
                    {
                        ScanCtrl.ScanReadCancel();
                        Thread.Sleep(10);
                        m_bReading = false;
                    }

                }
            }
        }

        private void M3Scanner_Closing(object sender)
        {
            ScanCtrl.ScanClose();
        }





        #region IScanClass Members

        public int InitScan()
        {
            m_nResult = ScanCtrl.ScanInit();
            ScanCtrl.Default_Setting();
            paused = false;

            m_bReading = false;
            m_bKeyFlag = false;
            m_bSyncMode = false;
            m_bResult = false;
            return m_nResult;
        }

        public bool Paused
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void PauseScan()
        {
            //throw new NotImplementedException();
            ScanCtrl.ScanReadCancel();
            paused = true;
        }

        public void ResumeScan()
        {
            ScanCtrl.ScanRead();
            paused = false;
            //throw new NotImplementedException();
        }

        public void StopScan()
        {
            ScanCtrl.ScanClose();
            paused = true;
        }

        #endregion
    }

    
}
