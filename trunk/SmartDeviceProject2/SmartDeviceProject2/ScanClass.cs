using System;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using Calib;

namespace TSDServer
{
    public delegate void Scanned(string scannedBarcode);
    public delegate void ScanError(string error);

    public class ScanClass 
    {
        //private static CasioScanClass _casioscaner = null;
        //private static M3GreenScanClass _m3scaner = null;
        private static IScanClass _scaner = null;
        
        private ScanClass()
        {
            //if (
           
//            _scaner = CasioScanClass.Scaner;
        }
        private static object obj = new object();
        public static IScanClass Scaner
        {
            get
            {
                if (_scaner == null)
                {
                    lock (obj)
                    {
                        if (_scaner == null)
                        {
                            //_scaner = CasioScanClass.Scaner;
                            
                            //OperatingSystem os = System.Environment.OSVersion;
                            string oemInfo = NativeClass.GetOemInfo();
                            BTPrintClass.PrintClass.SetStatusEvent(oemInfo);
                            if (oemInfo.ToUpper().IndexOf("PY055") >= 0 ||
                              oemInfo.ToUpper().IndexOf("HP101") >= 0 ||
                                oemInfo.ToUpper().IndexOf("EMULATOR")>=0 
                              )
                            {
                                BTPrintClass.PrintClass.SetStatusEvent("Это ТСД КАСИО");
                                _scaner = CasioScanClass.Scaner;
                            }
                            else
                            {
                                if (oemInfo.ToUpper().IndexOf("M3MOBILE") >= 0)
                                {
                                    BTPrintClass.PrintClass.SetStatusEvent("Это ТСД M3Green");
                                    _scaner = (IScanClass) M3GreenScanClass.Scaner;
                                }
                            }
                        }
                    }
                    //string devId = os.Version.ToString();
                    //BTPrintClass.PrintClass.SetStatusEvent(devId);

                }
                
                return _scaner;
            }
        }

    }
    public class CasioScanClass : IScanClass
    {
        private static CasioScanClass _scaner = new CasioScanClass();
        public static CasioScanClass Scaner
        {
            get
            {
                return _scaner;
            }
        }

        private CasioScanClass()
        {

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
        static int[] DecodeNum = {
									  OBReadLibNet.Def.OBR_NONDT,
									  OBReadLibNet.Def.OBR_CD39,
									  OBReadLibNet.Def.OBR_NW_7,
									  OBReadLibNet.Def.OBR_WPCA,
									  OBReadLibNet.Def.OBR_WPC,
									  OBReadLibNet.Def.OBR_UPEA,
									  OBReadLibNet.Def.OBR_UPE,
									  OBReadLibNet.Def.OBR_IDF,
									  OBReadLibNet.Def.OBR_ITF,
									  OBReadLibNet.Def.OBR_CD93,
									  OBReadLibNet.Def.OBR_CD128,
									  OBReadLibNet.Def.OBR_MSI,
									  OBReadLibNet.Def.OBR_IATA,
                                      OBReadLibNet.Def.OBR_EA13GTIN
								  };

        static string[] DecodeName = {
										 "          ",
										 "OBR_CD39  ",
										 "OBR_NW_7  ",
										 "OBR_WPCA  ",
										 "OBR_WPC   ",
										 "OBR_UPEA  ",
										 "OBR_UPE   ",
										 "OBR_IDF   ",
										 "OBR_ITF   ",
										 "OBR_CD93  ",
										 "OBR_CD128 ",
										 "OBR_MSI   ",
										 "OBR_IATA  ",
                                         "OBR_EA13GTIN "
									 };
        /// <summary>
        /// init scan driver and start listen thread
        /// </summary>
        /// <returns></returns>
        public int InitScan()
        {
            int iRet=0;

            //HWND = GetForegroundWindow();
            iRet = OBReadLibNet.Api.OBRLoadConfigFile();		//ini File read default value set
            iRet = OBReadLibNet.Api.OBRSetDefaultSymbology();	//1D(OBR) driver mode will be ini File vallue
            iRet = OBReadLibNet.Api.OBRSetScanningKey(OBReadLibNet.Def.OBR_TRIGGERKEY_L | OBReadLibNet.Def.OBR_TRIGGERKEY_R | OBReadLibNet.Def.OBR_CENTERTRIGGER);
            iRet = OBReadLibNet.Api.OBRSetScanningCode(OBReadLibNet.Def.OBR_ALL);
            iRet = OBReadLibNet.Api.OBRSetBuffType(OBReadLibNet.Def.OBR_BUFOBR);	//1D(OBR) driver mode will be OBR_BUFOBR
            iRet = OBReadLibNet.Api.OBRSetScanningNotification(OBReadLibNet.Def.OBR_EVENT, IntPtr.Zero);	//1D(OBR) driver mode will be OBR_EVENT
            iRet = OBReadLibNet.Api.OBRSetBuzzer(OBReadLibNet.Def.OBR_BUZOFF);//disable sound notification
            iRet = OBReadLibNet.Api.OBRSetVibrator(OBReadLibNet.Def.OBR_VIBOFF);//disable sound notification
            iRet = OBReadLibNet.Api.OBROpen(HWND, 0);			//OBRDRV open

            if (iRet == OBReadLibNet.Def.OBR_ERROR_INVALID_ACCESS)
            {
                //MessageBox.Show("Failed to connect to the scanner. Please exit a scanner application.", "OBRLibSampleCS");
                SetScanError("Failed to connect to the scanner. Please exit a scanner application.");
                return iRet;
            }
            else if (iRet == OBReadLibNet.Def.OBR_ERROR_HOTKEY)
            {
                //MessageBox.Show("Trigger keys are being used. Please quit the program which is using the trigger keys.", "OBRLibSampleCS");
                SetScanError("Trigger keys are being used. Please quit the program which is using the trigger keys.");
                return iRet;
            }
            else if (iRet != OBReadLibNet.Def.OBR_OK)
            {
                //MessageBox.Show("Failed to connect to the scanner.", "OBRLibSampleCS");
                SetScanError("Failed to connect to the scanner.");
                return iRet;
            }
            aborted = false;
            Paused = false;
            iRet = OBReadLibNet.Api.OBRClearBuff();
            thread = new Thread(new ThreadStart(start));
            thread.Start();     //Start start thread
            OBReadLibNet.Api.OBRSaveConfigFile();
            
            return iRet;
        }
        /// <summary>
        /// main listen thread procedure
        /// </summary>
        private void start()
        {
            try
            {
                while (true && !aborted)
                {
                    //mevt.WaitOne();
                    try
                    {
                        SystemLibNet.Api.SysWaitForEvent(IntPtr.Zero, OBReadLibNet.Def.OBR_NAME_EVENT, 2000/*timeout SystemLibNet.Def.INFINITE*/);  //Wait event
                        string str = GetText();
                        if (!paused && OnScanned != null && !String.IsNullOrEmpty(str))
                            OnScanned(str);
                    }
                    catch (Exception err)
                    {
                        SetScanError(err.Message);
                    }
                    

                    //Invoke(new SetLabelText(SetText));      //Display OBRBuffer data
                }
            }
            catch (ThreadAbortException)
            {
            }
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
        /// get scanned text from scan buffer
        /// </summary>
        /// <returns>scanned barcode</returns>
        private string GetText()
        {
            int leng = new int();	//digit number
            byte leng2 = new byte();//digit number
            int dwrcd = new int();	//barcode type
            int ret;
            byte lcnt = new byte();
            byte[] buff = new byte[1024];
            string str = string.Empty;
            int i;

            //if (HWND != IntPtr.Zero)
            //{	// check OBRBuffer state
            ret = OBReadLibNet.Api.OBRGetStatus(ref leng, ref lcnt);
            if (leng != 0)
            {
                // get OBRBuffer data
                ret = OBReadLibNet.Api.OBRGets(buff, ref dwrcd, ref leng2);
                Encoding ASCII = Encoding.GetEncoding("ascii");
                //textBox1.Text = ASCII.GetString(buff, 0, leng2);	//scan barcode type display

                str = ASCII.GetString(buff, 0, leng2);	
                /*str = "----------";
                for (i = 0; i < 13; i++)
                {
                    if (DecodeNum[i] == dwrcd)
                    {
                        str = DecodeName[i];
                        break;
                    }
                }*/
                //textBox2.Text = str;				//scan barcode type display
                //textBox3.Text = leng2.ToString();	//digit number display
                //textBox4.Text = ret.ToString();		//end information display
            }
            OBReadLibNet.Api.OBRClearBuff();
                
            //}
            return str;
        }

        private void SetText()
        {
            int leng = new int();	//digit number
            byte leng2 = new byte();//digit number
            int dwrcd = new int();	//barcode type
            int ret;
            byte lcnt = new byte();
            byte[] buff = new byte[1024];
            string str;
            int i;

            if (HWND != IntPtr.Zero)
            {	// check OBRBuffer state
                ret = OBReadLibNet.Api.OBRGetStatus(ref leng, ref lcnt);
                if (leng != 0)
                {
                    // get OBRBuffer data
                    ret = OBReadLibNet.Api.OBRGets(buff, ref dwrcd, ref leng2);
                    Encoding ASCII = Encoding.GetEncoding("ascii");
                    //textBox1.Text = ASCII.GetString(buff, 0, leng2);	//scan barcode type display

                    str = "----------";
                    for (i = 0; i < 13; i++)
                    {
                        if (DecodeNum[i] == dwrcd)
                        {
                            str = DecodeName[i];
                            break;
                        }
                    }
                    //textBox2.Text = str;				//scan barcode type display
                    //textBox3.Text = leng2.ToString();	//digit number display
                    //textBox4.Text = ret.ToString();		//end information display
                }
                OBReadLibNet.Api.OBRClearBuff();
            }
        }

        /// <summary>
        /// Close scan driver and stop listen thread
        /// </summary>
        public void StopScan()
        {
            OnScanned = null;
            paused = false;
            aborted = true;
            OBReadLibNet.Api.OBRClose();				//OBRDRV Close
            SystemLibNet.Api.SysTerminateWaitEvent();	//End SysWaitForEvent function
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

        /*public void ScanBtnPressed()
        {
            SystemLibNet.Api.SysWaitForEvent(IntPtr.Zero, OBReadLibNet.Def.OBR_NAME_EVENT, 2000);  //Wait event
            string str = GetText();
            if (!paused && OnScanned != null && !String.IsNullOrEmpty(str))
                OnScanned(str);
        }
    */
        private void SetScanError(string errorText)
        {
            if (OnScanError != null)
                OnScanError(errorText);

        }
    }

    
}
