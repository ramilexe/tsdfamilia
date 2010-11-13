using System;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Calib;

namespace Familia.TSDClient
{
    public class BTPrintClass
    {
        static BTPrintClass _PrintClass = new BTPrintClass();
        public static BTPrintClass PrintClass
        {
            get
            {
                return _PrintClass;
            }
        }

        private BTPrintClass()
        {
            for (int iCnt = 0; iCnt < BTDEF_MAX_INQUIRY_NUM; iCnt++)
            {
                bt_di[iCnt] = new Calib.BluetoothLibNet.BTST_DEVICEINFO();
            }
        }
    
        public delegate void SetStatus(string text);
        public event SetStatus OnSetStatus;
        private void SetStatusEvent(string text)
        {
            if (OnSetStatus != null)
                OnSetStatus(text);
        }

        public delegate void SetError(string text);
        public event SetError OnSetError;
        private void SetErrorEvent(string text)
        {
            if (OnSetError != null)
                OnSetError(text);
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        // DCB structure
        [StructLayout(LayoutKind.Sequential)]
        private class DCB
        {
            public Int32 DCBlength;						// sizeofDCB = 28 bytes (WinCE 4.1)
            public Int32 BaudRate;						// Baudrate at which running
            public Int32 Flag32;							// each falgs
            public Int16 wReserved;						// Not currently used
            public Int16 XonLim;							// Transmit X-ON threshold
            public Int16 XoffLim;						// Transmit X-OFF threshold
            public byte ByteSize;						// Number of bits/byte, 4-8
            public byte Parity;							// 0-4=None,Odd,Even,Mark,Space
            public byte StopBits;						// 0,1,2 = 1, 1.5, 2
            public byte XonChar;						// Tx and Rx X-ON character
            public byte XoffChar;						// Tx and Rx X-OFF character
            public byte ErrorChar;						// Error replacement char
            public byte EofChar;						// End of Input character
            public byte EvtChar;						// Received Event character
            public Int16 wReserved1;						// Fill for now.
        }

        // COMMTIMEOUTS structure
        [StructLayout(LayoutKind.Sequential)]
        private class COMMTIMEOUTS
        {
            public Int32 ReadIntervalTimeout;			// Maximum time between read chars.
            public Int32 ReadTotalTimeoutMultiplier;		// Multiplier of characters.
            public Int32 ReadTotalTimeoutConstant;		// Constant in milliseconds.
            public Int32 WriteTotalTimeoutMultiplier;	// Multiplier of characters.
            public Int32 WriteTotalTimeoutConstant;		// Constant in milliseconds.
        }

        //---------------------------------------------
        // Windows API functions
        //---------------------------------------------
        [DllImport("coredll")]
        private static extern IntPtr CreateFile(
                string lpFileName, Int32 dwDesiredAccess, Int32 dwShareMode,
                IntPtr lpSecurityAttributes, Int32 dwCreationDisposition,
                Int32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("coredll")]
        private static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, Int32 nNumberOfBytesToWrite,
                    ref Int32 lpNumberOfBytesWritten, IntPtr lpOverLapped);

        [DllImport("coredll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("coredll")]
        private static extern bool GetCommTimeouts(IntPtr hObject, COMMTIMEOUTS pIntersectCTO);

        [DllImport("coredll")]
        private static extern bool SetCommTimeouts(IntPtr hObject, COMMTIMEOUTS pIntersectCTO);

        [DllImport("coredll")]
        private static extern bool GetCommState(IntPtr hObject, DCB pIntersectDCB);

        [DllImport("coredll")]
        private static extern bool SetCommState(IntPtr hObject, DCB pIntersectDCB);

        [DllImport("coredll")]
        private static extern bool GetCommModemStatus(IntPtr hFile, ref Int32 lpModemStat);

        //---------------------------------------------
        // Define
        //---------------------------------------------
        private const int INVALID_HANDLE_VALUE = -1;
        private const Int32 FILE_ATTRIBUTE_NORMAL = 0x80;

        private const Int32 GENERIC_READ = -2147483648;
        private const Int32 GENERIC_WRITE = 0x40000000;
        private const Int32 GENERIC_EXECUTE = 0x20000000;
        private const Int32 GENERIC_ALL = 0x10000000;

        private const Int32 CREATE_NEW = 1;
        private const Int32 CREATE_ALWAYS = 2;
        private const Int32 OPEN_EXISTING = 3;
        private const Int32 OPEN_ALWAYS = 4;
        private const Int32 TRUNCATE_EXISTING = 5;
        private const Int32 OPEN_FOR_LOADER = 6;

        private const Int32 CBR_110 = 110;
        private const Int32 CBR_300 = 300;
        private const Int32 CBR_600 = 600;
        private const Int32 CBR_1200 = 1200;
        private const Int32 CBR_2400 = 2400;
        private const Int32 CBR_4800 = 4800;
        private const Int32 CBR_9600 = 9600;
        private const Int32 CBR_14400 = 14400;
        private const Int32 CBR_19200 = 19200;
        private const Int32 CBR_38400 = 38400;
        private const Int32 CBR_56000 = 56000;
        private const Int32 CBR_57600 = 57600;
        private const Int32 CBR_115200 = 115200;
        private const Int32 CBR_128000 = 128000;
        private const Int32 CBR_256000 = 256000;

        private const byte NOPARITY = 0;
        private const byte ODDPARITY = 1;
        private const byte EVENPARITY = 2;
        private const byte MARKPARITY = 3;
        private const byte SPACEPARITY = 4;

        private const byte ONESTOPBIT = 0;
        private const byte ONE5STOPBITS = 1;
        private const byte TWOSTOPBITS = 2;

        private const Int32 MS_CTS_ON = 0x10;
        private const Int32 MS_DSR_ON = 0x20;
        private const Int32 MS_RING_ON = 0x40;
        private const Int32 MS_RLSD_ON = 0x80;
        private const int BTDEF_MAX_INQUIRY_NUM = 16;
        
        private bool PrinterFound = false;
        public bool IsPrinterFound
        {
            get
            {
                return PrinterFound;
            }
        }

        private int bt_dmax;
        private int BtRet;
        private IntPtr hSerial;
        private int CommStatus = 0;


        BluetoothLibNet.BTST_LOCALINFO bt_li = new Calib.BluetoothLibNet.BTST_LOCALINFO();
        BluetoothLibNet.BTST_DEVICEINFO[] bt_di = new Calib.BluetoothLibNet.BTST_DEVICEINFO[BTDEF_MAX_INQUIRY_NUM];

        IntPtr[] bt_hdev = new IntPtr[BTDEF_MAX_INQUIRY_NUM + 1];

        

        private IntPtr PortOpen(string PortName, int Baudrate, byte Data, byte Parity, byte StopBit, int SendTime, int RecvTime)
        {
            IntPtr ipRet;
            DCB PortDCB = new DCB();
            COMMTIMEOUTS CommTimeouts = new COMMTIMEOUTS();

            ipRet = CreateFile(
                PortName,
                GENERIC_WRITE | GENERIC_READ,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if ((int)ipRet == INVALID_HANDLE_VALUE)
                return (ipRet);

            GetCommTimeouts(ipRet, CommTimeouts);
            CommTimeouts.ReadIntervalTimeout = 0;
            CommTimeouts.ReadTotalTimeoutMultiplier = 0;
            CommTimeouts.WriteTotalTimeoutConstant = RecvTime;
            CommTimeouts.WriteTotalTimeoutMultiplier = 0;
            CommTimeouts.WriteTotalTimeoutConstant = SendTime;
            SetCommTimeouts(ipRet, CommTimeouts);

            GetCommState(ipRet, PortDCB);
            PortDCB.BaudRate = Baudrate;
            PortDCB.Parity = Parity;
            PortDCB.ByteSize = Data;
            PortDCB.StopBits = StopBit;
            SetCommState(ipRet, PortDCB);
            return (ipRet);
        }

        private int PortWrite(byte[] buffer, int noBytes, IntPtr hPort)
        {
            int iRet = 0;
            if (WriteFile(hPort, buffer, noBytes, ref iRet, IntPtr.Zero) == false)
                iRet = 1;
            return (iRet);
        }

        private void PortClose(IntPtr hPort)
        {
            if ((int)hPort != INVALID_HANDLE_VALUE)
                CloseHandle(hPort);
        }

        public int BTPrinterInit()
        {
            //Status.Text = 
            SetStatusEvent("initialize bluetooth modul...");

            //Cursor.Current = Cursors.WaitCursor;
            // initialize bluetooth device (power on)
            BtRet = BluetoothLibNet.Api.BTInitialize();

            //Cursor.Current = Cursors.Default;

            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show(
                SetErrorEvent("BT Init Error");
                return BtRet;
            }

            string swork = new string(' ', 82);

            bt_li.LocalName = swork.ToCharArray();

            bt_li.LocalAddress = "                  ".ToCharArray();

            bt_li.LocalDeviceMode = 0;
            bt_li.LocalClass1 = 0;
            bt_li.LocalClass2 = 0;
            bt_li.LocalClass3 = 0;
            bt_li.Authentication = false;
            bt_li.Encryption = false;


            //Status.Text = 
            SetStatusEvent("get local device info...");
            BtRet = BluetoothLibNet.Api.BTGetLocalInfo(bt_li);

            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show(
                SetErrorEvent("BT get local info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }


            // set fixed values for our local bt device
            // (can be skiped if already done before)
            bt_li.LocalDeviceMode = BluetoothLibNet.Def.BTMODE_BOTH_ENABLED;
            bt_li.Authentication = false;
            bt_li.Encryption = false;
            //Status.Text = 
            SetStatusEvent("set new local device info...");
            BtRet = BluetoothLibNet.Api.BTSetLocalInfo(bt_li);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT set local info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }

            BtRet = BluetoothLibNet.Api.BTRegisterLocalInfo();
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT register local info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }

            return BtRet;
        }

        public int SearchDevices()
        {
            /************BEGIN SERACH PRINTER*************************************/
            // search for availible bluetooth devices
            bt_dmax = BTDEF_MAX_INQUIRY_NUM;
            //Status.Text = 
            SetStatusEvent("searching bluetooth devices...");
            //Cursor.Current = Cursors.WaitCursor;
            BtRet = BluetoothLibNet.Api.BTInquiry(IntPtr.Zero, ref bt_dmax, 5000);
            
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                SetErrorEvent("BT Inquiry Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }

            //Status.Text = "found " + bt_dmax.ToString() + " bluetooth devices!";
            SetStatusEvent(String.Format("Found {0} bluetooth devices!", bt_dmax));
            PrinterFound = false;

            string swork = new string(' ', 82);
            int iii,j,i;

            for (iii = 0; iii < 82; iii++)
                swork = swork + " ";

            for (j = 0; j < bt_dmax; j++)
            {
                bt_di[j].DeviceErrorFlag = 0;
                bt_di[j].DeviceHandle = 0;
                bt_di[j].DeviceName = swork.ToCharArray();
                bt_di[j].DeviceAddress = swork.Substring(1, 18).ToCharArray();
                bt_di[j].LocalClass1 = 0;
                bt_di[j].LocalClass2 = 0;
                bt_di[j].LocalClass3 = 0;
                bt_di[j].ProfileNumber = 0;
                for (i = 0; i < BTDEF_MAX_INQUIRY_NUM; i++)
                {
                    bt_di[j].ProfileUUID[i] = 0;
                }
            }

            BtRet = BluetoothLibNet.Api.BTGetDeviceInfo(bt_di, bt_dmax, 0);
            return BtRet;
        }

        public BluetoothLibNet.BTST_DEVICEINFO[] GetFoundedDevices()
        {
            return bt_di;
        }

        public System.Collections.Specialized.StringDictionary GetFoundedDevicesDict()
        {
            System.Collections.Specialized.StringDictionary dict =
                new System.Collections.Specialized.StringDictionary();

            for (int i = 0; i < bt_di.Length; i++)
            {
                dict.Add(new string(bt_di[i].DeviceAddress),
                    new string(bt_di[i].DeviceName));
            }
            return dict;
        }

        public int ConnToPrinter(string PrinterAdr)
        {
            if (BtRet == BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                for (int i = 0; i < bt_dmax; i++)
                {
                    //string xxx = "";
                    string xxx = new string(bt_di[i].DeviceAddress);
                    /*for (int ia = 0; ia < 17; ia++)
                    {
                        xxx = xxx + bt_di[i].DeviceAddress[ia].ToString();
                    }*/

                    if (xxx == PrinterAdr)
                    {	// we found the printer and try to get service informations
                        // (can be skiped because we know printer capabilities)

                        /************CONNECT TO PRINTER********************/
                        BtRet = ConnToPrinter(bt_di[i]);
                        return BtRet;
                    }
                }
            }
            else
            {
                //Result = MessageBox.Show
                SetErrorEvent("BT Get device info error");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }
            return BtRet;
        }

        public int ConnToPrinter(BluetoothLibNet.BTST_DEVICEINFO btdevice)
        {
            BtRet = BluetoothLibNet.Api.BTGetServiceInfo(btdevice);
            // register this device in registry
            // (can be skiped if already done before)
            BtRet = BluetoothLibNet.Api.BTRegisterDeviceInfo(btdevice);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                SetErrorEvent("BT register printer info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                PrinterFound = false;
                return BtRet;
            }
            PrinterFound = true;

            /*if (PrinterFound == false)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT Printer not found!");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }*/

            // set printer as default device
            // (can be skiped)
            //Status.Text = 
            SetStatusEvent("Extech BT printer found!");
            BtRet = BluetoothLibNet.Api.BTSetDefaultDevice(btdevice, BluetoothLibNet.Def.BTPORT_SERIAL);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT Printer set default device Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }

            // open serial communication
            //Status.Text = 
            SetStatusEvent("try to connect to printer...");
            hSerial = PortOpen("COM6:", CBR_9600, 8, NOPARITY, ONESTOPBIT, 3000, 3000);

            if (hSerial.ToInt32() == INVALID_HANDLE_VALUE)
            {
                //Status.Text = "";
                //MessageBox.Show
                SetErrorEvent("BT Open Error");
                BluetoothLibNet.Api.BTDeInitialize();
                BtRet = BluetoothLibNet.Def.BTERR_CONNECTION_FAILED;
                return BtRet;
            }

            // dummy write to etablish bt connection
            byte[] prnout = new byte[1];

            PortWrite(prnout, 0, hSerial);
            for (int i = 0; i < 76; i++)
            {
                SetStatusEvent(String.Format("wait for answer...{0}", i.ToString()));

                GetCommModemStatus(hSerial, ref CommStatus);
                if ((CommStatus & (MS_RING_ON | MS_RLSD_ON)) != 0)
                {
                    break;
                }
                Thread.Sleep(200);
            }
            BtRet = BluetoothLibNet.Def.BTERR_CONNECTED; 
            return BtRet;
        }

        public void Print(string label1)
        {
            if (BtRet == BluetoothLibNet.Def.BTERR_CONNECTED)
            {
                Encoding ascii = Encoding.ASCII;
                byte[] prnout = ascii.GetBytes(label1);

                // write label to printer
                //Status.Text = 
                SetStatusEvent("sending datas...");
                if (PortWrite(prnout, label1.Length, hSerial) == 1)
                {
                    //Status.Text = "";
                    //MessageBox.Show
                    SetErrorEvent("BT Write Error");
                }

                //MessageBox.Show
                SetStatusEvent("BT Printing");
            }
            else
            {
                SetErrorEvent("BT Printer not connected");
            }
        }

        public void Disconnect()
        {
            //Status.Text = 
            SetStatusEvent("disconnect printer...");
            // deinitialize bt device (power off)
            PortClose(hSerial);
            BluetoothLibNet.Api.BTDeInitialize();
        }
        public void TestPrint()
        {
            const int BTDEF_MAX_INQUIRY_NUM = 16;

            
            for (int iCnt = 0; iCnt < 16; iCnt++)
            {
                bt_di[iCnt] = new Calib.BluetoothLibNet.BTST_DEVICEINFO();
            }


            IntPtr[] bt_hdev;
            bt_hdev = new IntPtr[BTDEF_MAX_INQUIRY_NUM + 1];

            int bt_dmax;
            int BtRet;
            bool PrinterFound;
            //DialogResult Result;
            string PrinterAdr = "00:80:37:17:78:DA";

            //        'Set some variables used by serial
            IntPtr hSerial;
            int i, j, iii, ii = 0;
            int CommStatus = 0;

            string label1 = "***** TEST PRINT CS.NET *****" + "\r" + "\n"
                + "IT-600 or DT-X11" + "\r" + "\n"
                + "CASIO" + "\r" + "\n"
                + "www.casio.co.jp/English/system/" + "\r" + "\n"
                + "\r" + "\n"
                + "CASIO Computer Co., Ltd." + "\r" + "\n"
                + "\r" + "\n"
                + "6-2, Hon-machi 1-chome" + "\r" + "\n"
                + "\r" + "\n"
                + "Shibuya-ku, Tokyo 151-8543, Japan" + "\r" + "\n"
                + "\r" + "\n"
                + "Tel.: +81-3-5334-4771" + "\r" + "\n"
                + "Fax.: +81-3-5334-4656" + "\r" + "\n"
                + "\r" + "\n"
                + "\r" + "\n"
                + "\r" + "\n"
                + "\r" + "\n"
                + "\r" + "\n"
                + "\r" + "\n";

            Encoding ascii = Encoding.ASCII;
            byte[] prnout = ascii.GetBytes(label1);


            //Status.Text = 
            SetStatusEvent("initialize bluetooth modul...");

            //Cursor.Current = Cursors.WaitCursor;
            // initialize bluetooth device (power on)
            BtRet = BluetoothLibNet.Api.BTInitialize();

            //Cursor.Current = Cursors.Default;

            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show(
                SetErrorEvent("BT Init Error");
                return;
            }

            string swork = new string(' ', 82);

            bt_li.LocalName = swork.ToCharArray();

            bt_li.LocalAddress = "                  ".ToCharArray();

            bt_li.LocalDeviceMode = 0;
            bt_li.LocalClass1 = 0;
            bt_li.LocalClass2 = 0;
            bt_li.LocalClass3 = 0;
            bt_li.Authentication = false;
            bt_li.Encryption = false;


            //Status.Text = 
            SetStatusEvent("get local device info...");
            BtRet = BluetoothLibNet.Api.BTGetLocalInfo(bt_li);

            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show(
                SetErrorEvent("BT get local info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }


            // set fixed values for our local bt device
            // (can be skiped if already done before)
            bt_li.LocalDeviceMode = BluetoothLibNet.Def.BTMODE_BOTH_ENABLED;
            bt_li.Authentication = false;
            bt_li.Encryption = false;
            //Status.Text = 
            SetStatusEvent("set new local device info...");
            BtRet = BluetoothLibNet.Api.BTSetLocalInfo(bt_li);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT set local info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }

            BtRet = BluetoothLibNet.Api.BTRegisterLocalInfo();
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT register local info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }
            /**********************************************************************/
            /*******END*  *INIT BT PRINTER*****************************************/
            /**********************************************************************/


            /************BEGIN SERACH PRINTER*************************************/
            // search for availible bluetooth devices
            bt_dmax = BTDEF_MAX_INQUIRY_NUM;
            //Status.Text = 
            SetStatusEvent("searching bluetooth devices...");
            //Cursor.Current = Cursors.WaitCursor;
            BtRet = BluetoothLibNet.Api.BTInquiry(IntPtr.Zero, ref bt_dmax, 5000);

            //Cursor.Current = Cursors.Default;
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT Inquiry Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }

            //Status.Text = "found " + bt_dmax.ToString() + " bluetooth devices!";
            SetStatusEvent(String.Format("Found {0} bluetooth devices!", bt_dmax));
            PrinterFound = false;

            swork = "";
            for (iii = 0; iii < 82; iii++)
                swork = swork + " ";

            for (j = 0; j < bt_dmax; j++)
            {
                bt_di[j].DeviceErrorFlag = 0;
                bt_di[j].DeviceHandle = 0;
                bt_di[j].DeviceName = swork.ToCharArray();
                bt_di[j].DeviceAddress = swork.Substring(1, 18).ToCharArray();
                bt_di[j].LocalClass1 = 0;
                bt_di[j].LocalClass2 = 0;
                bt_di[j].LocalClass3 = 0;
                bt_di[j].ProfileNumber = 0;
                for (i = 0; i < 16; i++)
                {
                    bt_di[j].ProfileUUID[i] = 0;
                }
            }
            
            BtRet = BluetoothLibNet.Api.BTGetDeviceInfo(bt_di, bt_dmax, 0);

            /****************END SEARCH***************************************/
            if (BtRet == BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                for (i = 0; i < bt_dmax; i++)
                {
                    string xxx = "";
                    for (int ia = 0; ia < 17; ia++)
                    {
                        xxx = xxx + bt_di[i].DeviceAddress[ia].ToString();
                    }

                    if (xxx == PrinterAdr)
                    {	// we found the printer and try to get service informations
                        // (can be skiped because we know printer capabilities)
                        
                        /************CONNECT TO PRINTER********************/
                        BtRet = BluetoothLibNet.Api.BTGetServiceInfo(bt_di[i]);
                        // register this device in registry
                        // (can be skiped if already done before)
                        BtRet = BluetoothLibNet.Api.BTRegisterDeviceInfo(bt_di[i]);
                        if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                        {
                            //Status.Text = "";
                            //Result = MessageBox.Show
                            SetErrorEvent("BT register printer info Error");
                            BluetoothLibNet.Api.BTDeInitialize();
                            return;
                        }
                        PrinterFound = true;
                        ii = i;
                    }
                }
            }
            else
            {
                //Result = MessageBox.Show
                SetErrorEvent("BT Get device info error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }


            if (PrinterFound == false)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT Printer not found!");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }

            // set printer as default device
            // (can be skiped)
            //Status.Text = 
            SetStatusEvent("Extech BT printer found!");
            BtRet = BluetoothLibNet.Api.BTSetDefaultDevice(bt_di[ii], BluetoothLibNet.Def.BTPORT_SERIAL);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT Printer set default device Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }

            // open serial communication
            //Status.Text = 
            SetStatusEvent("try to connect to printer...");
            hSerial = PortOpen("COM6:", CBR_9600, 8, NOPARITY, ONESTOPBIT, 3000, 3000);

            if (hSerial.ToInt32() == INVALID_HANDLE_VALUE)
            {
                //Status.Text = "";
                //MessageBox.Show
                SetErrorEvent("BT Open Error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }

            // dummy write to etablish bt connection
            PortWrite(prnout, 0, hSerial);
            for (i = 0; i < 76; i++)
            {
                //Status.Text = 
                SetStatusEvent(String.Format("wait for answer...{0}", i.ToString()));

                GetCommModemStatus(hSerial, ref CommStatus);
                if ((CommStatus & (MS_RING_ON | MS_RLSD_ON)) != 0)
                {
                    break;
                }
                Thread.Sleep(200);
            }

            // write label to printer
            //Status.Text = 
            SetStatusEvent("sending datas...");
            if (PortWrite(prnout, label1.Length, hSerial) == 1)
            {
                //Status.Text = "";
                //MessageBox.Show
                SetErrorEvent("BT Write Error");
            }

            //MessageBox.Show
            SetStatusEvent("BT Printing");

            //Status.Text = 
            SetStatusEvent("disconnect printer...");
            // deinitialize bt device (power off)
            PortClose(hSerial);
            BluetoothLibNet.Api.BTDeInitialize();
            //Status.Text = "";

        }

    }
}
