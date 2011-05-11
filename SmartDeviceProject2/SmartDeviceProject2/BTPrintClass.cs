using System;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Calib;

namespace Familia.TSDClient
{
    public delegate void SetStatus(string text);
    public delegate void SetError(string text);
    public delegate void ConnectionError();

    public class BTPrintClass
    {
        static int WaitPrintTimeDefault = 0;

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
            WaitPrintTimeDefault = Program.Default.WaitPrintTimeDefault;
            for (int iCnt = 0; iCnt < BTDEF_MAX_INQUIRY_NUM; iCnt++)
            {
                bt_di[iCnt] = new Calib.BluetoothLibNet.BTST_DEVICEINFO();
            }
            BTPrinterInit();
            
        }

        public void Reconnect()
        {
            SetErrorEvent("Выполняется отключение BlueTooth. Ждите...");
            
            try
            {
                doEvents = false;
                Disconnect();
                Thread.Sleep(1000);
                SetErrorEvent("Выполняется подключение BlueTooth. Ждите...");
                BTPrinterInit();
                ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
            }
            catch (Exception err)
            {
                BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                _connected = false;
                SetErrorEvent(err.ToString());
                SetErrorEvent("Ошибка подключения. Переподключите принтер и попробуйте еще раз.");
            }
            finally
            {
                doEvents = true;
            }
        }

        public void PartialReconnect()
        {
            Thread.Sleep(1000);
            try
            {
                doEvents = false;
                //Disconnect();
                //BTPrinterInit();
                ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
            }
            catch (Exception err)
            {
                BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                _connected = false;
                SetErrorEvent(err.ToString());
                SetErrorEvent("Ошибка подключения. Переподключите принтер и попробуйте еще раз.");
            }
            finally
            {
                doEvents = true;
            }
        }

        public event SetStatus OnSetStatus;
        public event ConnectionError OnConnectionError;

        public void SetStatusEvent(string text)
        {
            if (OnSetStatus != null)
                OnSetStatus(text);

            try
            {
                //if (System.IO.File.Exists("BTLog.txt"))
                //    System.IO.FileInfo fi = 
                //        System.IO.File.g
                using (System.IO.StreamWriter wr = new System.IO.StreamWriter(
                           System.IO.Path.Combine(Program.StartupPath, "BTLog.txt"), true))
                {
                   // wr.WriteLine(text);
                    wr.WriteLine(string.Format("{0} {1}", DateTime.Now, text));
                }
            }
            catch { }
        }

        public void SetStatusEvent(string format, params object[] obj)
        {
            string text = string.Format(format, obj);
            SetStatusEvent(text);
            
        }
        public event SetError OnSetError;
        public void SetErrorEvent(string text)
        {
            if (OnSetError != null)
                OnSetError(text);

            try
            {
                using (System.IO.StreamWriter wr = new System.IO.StreamWriter(
                           System.IO.Path.Combine(Program.StartupPath, "BTLog.txt"), true))
                {
                    wr.WriteLine(string.Format("{0} {1}",DateTime.Now,text));
                }
            }
            catch { }
        }

        public void SetErrorEvent(string format, params object[] obj)
        {
            string text = string.Format(format, obj);
            SetErrorEvent(text);
        }

        private bool _connected = false;
        public bool Connected
        {
            get
            {
                return _connected;
            }
        }

        System.Collections.Specialized.StringDictionary foundedDevices =
               new System.Collections.Specialized.StringDictionary();
        public System.Threading.ManualResetEvent mEvt = new ManualResetEvent(false);
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
        
        #region extern Dlls
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
        #endregion

        //---------------------------------------------
        // Define
        //---------------------------------------------

        #region Defins
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
        #endregion

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
        private bool doEvents = true;
        BluetoothLibNet.BTST_DEVICEINFO btDefaultdevice = new BluetoothLibNet.BTST_DEVICEINFO();

        BluetoothLibNet.BTST_LOCALINFO bt_li = new Calib.BluetoothLibNet.BTST_LOCALINFO();
        BluetoothLibNet.BTST_DEVICEINFO[] bt_di = new Calib.BluetoothLibNet.BTST_DEVICEINFO[BTDEF_MAX_INQUIRY_NUM];

        IntPtr[] bt_hdev = new IntPtr[BTDEF_MAX_INQUIRY_NUM + 1];

        System.IO.Ports.SerialPort sp =
                new System.IO.Ports.SerialPort(string.Format("COM6:"),
                    19200,
                    System.IO.Ports.Parity.None,
                    8,
                    System.IO.Ports.StopBits.One);

        #region Old native func

        [System.Obsolete("Use standart .Net class System.IO.Ports.SerialPort")]
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

        [System.Obsolete("Use standart .Net class System.IO.Ports.SerialPort")]
        private int PortWrite(byte[] buffer, int noBytes, IntPtr hPort)
        {
            int iRet = 0;
            if (WriteFile(hPort, buffer, noBytes, ref iRet, IntPtr.Zero) == false)
                iRet = 1;
            return (iRet);
        }

        [System.Obsolete("Use standart .Net class System.IO.Ports.SerialPort")]
        private void PortClose(IntPtr hPort)
        {
            try
            {
                if ((int)hPort != INVALID_HANDLE_VALUE)
                    CloseHandle(hPort);
            }
            catch { }
        }
        #endregion

        private int BTPrinterInit()
        {
            int status = 0;
            BtRet = BluetoothLibNet.Api.BTGetLibraryStatus(ref status);

            if (status == BluetoothLibNet.Def.BTSTATUS_NOT_INITIALIZED)
            {

                //Status.Text = 
                SetStatusEvent("Инициализация модуля Bluetooth...");

                //Cursor.Current = Cursors.WaitCursor;
                // initialize bluetooth device (power on)
                BtRet = BluetoothLibNet.Api.BTInitialize();

                //Cursor.Current = Cursors.Default;

                if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                {
                    //Status.Text = "";
                    //Result = MessageBox.Show(
                    SetErrorEvent("BT ошибка инициализации " + BtRet.ToString());
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
                SetStatusEvent("Получение локальных настроек...");
                BtRet = BluetoothLibNet.Api.BTGetLocalInfo(bt_li);

                if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                {
                    //Status.Text = "";
                    //Result = MessageBox.Show(
                    SetErrorEvent("Получение локальных настроек - Ошибка {0}", BtRet);
                    BluetoothLibNet.Api.BTDeInitialize();
                    return BtRet;
                }


                // set fixed values for our local bt device
                // (can be skiped if already done before)
                bt_li.LocalDeviceMode = BluetoothLibNet.Def.BTMODE_BOTH_ENABLED;
                bt_li.Authentication = false;
                bt_li.Encryption = false;
                //Status.Text = 
                SetStatusEvent("Установка новых локальных настроек...");
                BtRet = BluetoothLibNet.Api.BTSetLocalInfo(bt_li);
                if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                {
                    //Status.Text = "";
                    //Result = MessageBox.Show
                    SetErrorEvent("Установка новых локальных настроек - Ошибка {0}", BtRet);
                    BluetoothLibNet.Api.BTDeInitialize();
                    return BtRet;
                }

                BtRet = BluetoothLibNet.Api.BTRegisterLocalInfo();
                if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                {
                    //Status.Text = "";
                    //Result = MessageBox.Show
                    SetErrorEvent("BT Регистрация локальных настроек - Ошибка");
                    BluetoothLibNet.Api.BTDeInitialize();
                    return BtRet;
                }
                
                
            }
            mEvt.Set();
            return BtRet;
        }

        public int SearchDevices()
        {
            int status = 0;
            BtRet = BluetoothLibNet.Api.BTGetLibraryStatus(ref status);

            if (status == BluetoothLibNet.Def.BTSTATUS_NOT_INITIALIZED)
            {
                BTPrinterInit();
            }

            /************BEGIN SERACH PRINTER*************************************/
            // search for availible bluetooth devices
            bt_dmax = BTDEF_MAX_INQUIRY_NUM;
            SetStatusEvent("Поиск Bluetooth устройств...");
            BtRet = BluetoothLibNet.Api.BTInquiry(IntPtr.Zero, ref bt_dmax, 5000);
            
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                SetErrorEvent("BT Ошибка поиска устройст");
                BluetoothLibNet.Api.BTDeInitialize();
                return BtRet;
            }

            SetStatusEvent("Найдено {0} bluetooth устройств!", bt_dmax);
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

            foreach (BluetoothLibNet.BTST_DEVICEINFO btd in bt_di)
            {
                try
                {
                    string xxx = "";
                    for (int ia = 0; ia < 17; ia++)
                    {
                        xxx = xxx + btd.DeviceAddress[ia].ToString().ToUpper();
                    }
                    //SetStatusEvent(xxx);

                    if (!foundedDevices.ContainsKey(xxx.ToUpper()))
                        foundedDevices.Add(xxx.ToUpper(), new string(btd.DeviceName).Replace("\0", "").ToUpper());
                }
                catch { }
            }
            return foundedDevices;
        }

        public int ConnToPrinter(string PrinterAdr)
        {
            if (bt_dmax == 0)
            {
                BTPrinterInit();
                Int32 res = BluetoothLibNet.Api.BTSelectDevice(IntPtr.Zero, BluetoothLibNet.Def.BTPORT_SERIAL);
                return ConnToPrinter3();
            }

            for (int i = 0; i < bt_dmax; i++)
            {
                //string xxx = "";
                //string xxx = new string(bt_di[i].DeviceAddress).Replace("\0", "").ToUpper();
                /*for (int ia = 0; ia < 17; ia++)
                {
                    xxx = xxx + bt_di[i].DeviceAddress[ia].ToString();
                }*/

                string xxx = "";
                for (int ia = 0; ia < 17; ia++)
                {
                    xxx = xxx + bt_di[i].DeviceAddress[ia].ToString().ToUpper();
                }

                SetErrorEvent(xxx);
                SetErrorEvent(PrinterAdr);
                if (xxx == PrinterAdr.ToUpper())
                {	// we found the printer and try to get service informations
                    // (can be skiped because we know printer capabilities)

                    /************CONNECT TO PRINTER********************/
                    BtRet = ConnToPrinter3(bt_di[i]);
                    if (BtRet == BluetoothLibNet.Def.BTERR_CONNECTED)
                        _connected = true;
                    return BtRet;
                }
            }
            return BtRet;
        }

        public int SetDefaultDevice(string PrinterAdr)
        {
            for (int i = 0; i < bt_dmax; i++)
            {
                string xxx = "";
                for (int ia = 0; ia < 17; ia++)
                {
                    xxx = xxx + bt_di[i].DeviceAddress[ia].ToString().ToUpper();
                }

                //SetErrorEvent(xxx);
                //SetErrorEvent(PrinterAdr);
                if (xxx == PrinterAdr.ToUpper())
                {
                    btDefaultdevice = bt_di[i];
                    BtRet = BluetoothLibNet.Api.BTGetServiceInfo(btDefaultdevice);
                    
                    // register this device in registry
                    // (can be skiped if already done before)
                    BtRet = BluetoothLibNet.Api.BTRegisterDeviceInfo(btDefaultdevice);
                    if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                    {
                        SetErrorEvent("BT регистрация принтера {0} - ошибка", xxx);
                        BluetoothLibNet.Api.BTDeInitialize();
                        PrinterFound = false;
                        _connected = false;
                        return BtRet;
                    }
                    SetStatusEvent("BT регистрация принтера {0} - успешно", xxx);

                    PrinterFound = true;
                    BtRet = BluetoothLibNet.Api.BTSetDefaultDevice(btDefaultdevice, BluetoothLibNet.Def.BTPORT_SERIAL);
                    if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                    {
                        SetErrorEvent("BT установка принтера стандартным {0} - ошибка", xxx);
                        BluetoothLibNet.Api.BTDeInitialize();
                        _connected = false;
                        return BtRet;
                    }
                    SetStatusEvent("BT установка принтера стандартным {0} - успешно", xxx);
                    Program.Settings.TypedSettings[0].BTPrinterAddress = xxx;
                    return BtRet;//BluetoothLibNet.Def.BTERR_SUCCESS
                }
            }
            return BluetoothLibNet.Def.BTERR_NOT_FOUND;
        }
        
        #region old func
        [System.Obsolete("Use ConnToPrinter3")]
        public int ConnToPrinter2(BluetoothLibNet.BTST_DEVICEINFO btdevice)
        {
            BtRet = BluetoothLibNet.Api.BTSelectDevice(btdevice, BluetoothLibNet.Def.BTPORT_SERIAL);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                SetErrorEvent("BT register printer info Error");
                BluetoothLibNet.Api.BTDeInitialize();
                PrinterFound = false;
                _connected = false;
                return BtRet;
            }
            byte[] prnout = new byte[1];
            SetStatusEvent("try to connect to printer...");
            hSerial = PortOpen(string.Format("COM{0}:",
                Program.Settings.TypedSettings[0].BTComPort), CBR_19200, 8, NOPARITY, ONESTOPBIT, 3000, 3000);

            if (hSerial.ToInt32() == INVALID_HANDLE_VALUE)
            {
                SetErrorEvent("BT Open Error");
                BluetoothLibNet.Api.BTDeInitialize();
                BtRet = BluetoothLibNet.Def.BTERR_CONNECTION_FAILED;
                _connected = false;
                return BtRet;
            }



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
            _connected = true;
            return BtRet;
        }

        [System.Obsolete("Use ConnToPrinter3")]
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
                _connected = false;
                BTPrinterInit();
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
                _connected = false;
                return BtRet;
            }
            // dummy write to etablish bt connection
            byte[] prnout = new byte[1];

            //BtRet = BluetoothLibNet.Api.BTSetPassKey("111");
            BtRet = BluetoothLibNet.Api.BTConnectSerial(BluetoothLibNet.Def.BTCONNECT_SERIAL_SERVER,
                1000, 1000);
            //BtRet = BluetoothLibNet.Api.BTSendSerialData(ref prnout, 1, 1);
            // open serial communication
            //Status.Text = 
            SetStatusEvent("try to connect to printer...");
            hSerial = PortOpen(string.Format("COM{0}:",
                Program.Settings.TypedSettings[0].BTComPort), CBR_19200, 8, NOPARITY, ONESTOPBIT, 3000, 3000);

            if (hSerial.ToInt32() == INVALID_HANDLE_VALUE)
            {
                //Status.Text = "";
                //MessageBox.Show
                SetErrorEvent("BT Open Error");
                BluetoothLibNet.Api.BTDeInitialize();
                BtRet = BluetoothLibNet.Def.BTERR_CONNECTION_FAILED;
                _connected = false;
                return BtRet;
            }



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
            _connected = true;
            return BtRet;
        }
        #endregion

        public Int32 ConnToPrinter3()
        {
            try
            {
                if (sp.IsOpen)
                {
                    try
                    {
                        sp.Close();
                        Thread.Sleep(500);
                        sp.DiscardOutBuffer();
                        sp.DiscardInBuffer();
                    }
                    catch (Exception err1)
                    {
                        SetErrorEvent(err1.ToString());
                    }
                }
                sp.Open();
                
                if (sp.IsOpen)
                {
                    SetStatusEvent("BT Принтер подключен");
                    BtRet = BluetoothLibNet.Def.BTERR_CONNECTED;
                    _connected = true;
                }
                else
                {
                    BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                    SetErrorEvent("BT сбой подключения принтера");
                    
                    _connected = false;
                    throw new BTConnectionFailedException();
                }
                return BtRet;
            }
            catch (Exception err)
            {
                BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                _connected = false;
                SetErrorEvent(err.ToString());
                SetErrorEvent("Ошибка подключения. Переподключите принтер и попробуйте еще раз.");
                if (OnConnectionError != null && doEvents)
                    OnConnectionError();
                return BtRet;
            }
        }

        public int ConnToPrinter3(BluetoothLibNet.BTST_DEVICEINFO btdevice)
        {
            try
            {
                if (sp.IsOpen)
                    return BluetoothLibNet.Def.BTERR_CONNECTED;
                int status = 0;
                BtRet = BluetoothLibNet.Api.BTGetLibraryStatus(ref status);

                if (status == BluetoothLibNet.Def.BTSTATUS_NOT_INITIALIZED)
                {
                    BTPrinterInit();
                }
                //BtRet = BluetoothLibNet.Api.BTGetDefaultDeviceInfo(btDefaultdevice, BluetoothLibNet.Def.BTPORT_SERIAL);
                BtRet = BluetoothLibNet.Api.BTSelectDevice(IntPtr.Zero, BluetoothLibNet.Def.BTPORT_SERIAL);
                if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                {
                    SetErrorEvent("Стандартный принтер не найден!");
                    if (Program.Settings.TypedSettings[0]["BTPrinterAddress"] != null &&
                        Program.Settings.TypedSettings[0]["BTPrinterAddress"] != DBNull.Value &&
                        Program.Settings.TypedSettings[0]["BTPrinterAddress"].ToString() != string.Empty &&
                        Program.Settings.TypedSettings[0]["BTPrinterAddress"].ToString() != "00:00:00:00:00:00")
                    {

                        SetStatusEvent("Поиск принтера {0}", Program.Settings.TypedSettings[0].BTPrinterAddress.ToUpper());
                        BtRet = SearchDevices();
                        if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
                        {
                            SetErrorEvent("Принтер не найден!");
                            throw new BTConnectionFailedException();
                            //return BtRet;
                        }
                        SetStatusEvent("Установка принтера {0} стандартным", Program.Settings.TypedSettings[0].BTPrinterAddress.ToUpper());

                        SetDefaultDevice(Program.Settings.TypedSettings[0].BTPrinterAddress.ToUpper());

                    }
                    else
                    {
                        SetErrorEvent("Стандартный принтер не установлен");
//                        return BtRet;
                        throw new BTConnectionFailedException();
                    }

                }

                PrinterFound = true;

                SetStatusEvent("{0} BT принтер найден!", Program.Settings.TypedSettings[0].BTPrinterAddress.ToUpper());

                return ConnToPrinter3();
            }
            catch (Exception err)
            {
                BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                _connected = false;
                SetErrorEvent(err.ToString());
                SetErrorEvent("Ошибка подключения. Переподключите принтер и попробуйте еще раз.");
                if (OnConnectionError != null && doEvents)
                    OnConnectionError();
                return BtRet;
            }
            
            /*
            sp.Open();
            if (sp.IsOpen)
            {
                SetStatusEvent("BT Printer connected");
                BtRet = BluetoothLibNet.Def.BTERR_CONNECTED;
                _connected = true;
            }
            else
            {
                BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                SetErrorEvent("BT Printer connection failed");
                _connected = false;
            }
            return BtRet;*/
        }


        public void Print(byte[] prnout)
        {
            int counter = 0;
            try
            {
                tryagain:
                if (mEvt.WaitOne((int)(WaitPrintTimeDefault + WaitPrintTimeDefault / 2), false) == false)
                {
                    SetStatusEvent("Ожидание очереди печати...");
                    counter++;
                    if (counter >= 5)
                    {
                        SetStatusEvent("Принтер занят...");
                        //throw new ApplicationException("Принтер занят...");
                        return;
                    }
                    goto tryagain;
                }
                mEvt.Reset();
                try
                {
                    if (BtRet == BluetoothLibNet.Def.BTERR_CONNECTED)
                    {
                        SetStatusEvent("Отправка на принтер...");
                        sp.DiscardOutBuffer();
                        sp.Write(prnout, 0, prnout.Length);

                        
                        SetStatusEvent("Идет печать...");
                        Thread.Sleep(WaitPrintTimeDefault);
                        SetStatusEvent("BT принтер успешно напечатал");
                        #region
                        /*
            SetStatusEvent("sending datas...");
            if (PortWrite(prnout, prnout.Length, hSerial) == 1)
            {
                //Status.Text = "";
                //MessageBox.Show
                SetErrorEvent("BT Write Error");
            }

            //MessageBox.Show
            SetStatusEvent("BT Printing");
            */
                        #endregion
                    }
                    else
                    {
                        SetErrorEvent("BT Принтер не подключен");
                        throw new BTConnectionFailedException();
                    }
                }
                finally
                {
                    mEvt.Set();
                }
            }
            catch (Exception err)
            {
                BtRet = BluetoothLibNet.Def.BTERR_FAILED;
                _connected = false;
                SetErrorEvent(err.ToString());
                SetErrorEvent("Ошибка печати. Переподключите принтер и попробуйте еще раз.");
                throw new BTConnectionFailedException();
                //if (OnConnectionError != null)
                //    OnConnectionError();
            }
        }

        public void Print(string label1)
        {

            byte[] prnout = TSDUtils.CustomEncodingClass.Encoding.GetBytes(label1);
            Print(prnout);
        }

        public void Disconnect()
        {

            
            try
            {
                SetStatusEvent("Отключение принтера...");
                // deinitialize bt device (power off)
                if (sp.IsOpen)
                    sp.Close();
                _connected = false;

            }
            catch (Exception err)
            {
                SetErrorEvent("Ошибка отключения  BT принтера {0}", err);
            }
            finally
            {
                BluetoothLibNet.Api.BTDeInitialize();
                Calib.BluetoothLibNet.Api.BTWaitForBtReady();
            }

        }

        #region test
        public void TestPrint3(string addr)
        {
            try
            {
                ConnToPrinter(addr);

                string label1 = @"! UTILITIES
GAP-SENSE
SET-TOF 0
TONE 100
SPEED 5
ON-FEED FEED
TIMEOUT 0
PRINT
! 0 200 200 318 1
LABEL
PAGE-WIDTH 463
SETMAG 0 0
T 0 4 20  8 TEST_ATTRIBUTE_007
T 0 4 240 8 TEST_ATTRIBUTE_008
T 93 0 20 60 TEST_ATTRIBUTE_002
T 92 0 240 107 TEST_ATTRIBUTE_003
T 92 0 20 107 TEST_ATTRIBUTE_004
T 92 0 20 137 СОСТАВ:
BT OFF
B EAN13 1 1 79 20 185 0000000000000
T 92 0 20 158 TEST_ATTRIBUTE_006
T 93 0 286 265 TEST_ATTRIBUTE_009
T 93 0 40 265 TEST_ATTRIBUTE_005
FORM
PRINT
";
                Print(label1);
            }
            finally
            {
                Disconnect();
            }

        }

        [System.Obsolete("Use TestPrint3")]
        public void TestPrint(string addr)
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
            string PrinterAdr = addr;//"00:80:37:17:78:DA";

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
            hSerial = PortOpen(string.Format("COM{0}:",
                Program.Settings.TypedSettings[0].BTComPort),CBR_19200, 8, NOPARITY, ONESTOPBIT, 3000, 3000);

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

        [System.Obsolete("Use TestPrint3")]
        void TestPrint2(string addr)
        {


            string PrinterAdr = addr;//"00:80:37:17:78:DA";

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



            /****************END SEARCH***************************************/
            /*if (BtRet == BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                for (i = 0; i < bt_di.Length; i++)
                {
                    
                    string xxx = "";
                    for (int ia = 0; ia < 17; ia++)
                    {
                        xxx = xxx + bt_di[i].DeviceAddress[ia].ToString();
                    }
                    SetStatusEvent(xxx);
                    SetStatusEvent(PrinterAdr);
                    if (xxx == PrinterAdr)

                    {	// we found the printer and try to get service informations
                        // (can be skiped because we know printer capabilities)
                        using (System.IO.StreamWriter wr = new System.IO.StreamWriter(
                            System.IO.Path.Combine(Program.StartupPath, "BTLog.txt"),true))
                        {
                            wr.WriteLine(new string(bt_di[i].DeviceAddress));
                            wr.WriteLine(bt_di[i].DeviceErrorFlag.ToString());
                            wr.WriteLine(bt_di[i].DeviceHandle.ToString());
                            wr.WriteLine(new string(bt_di[i].DeviceName));
                            wr.WriteLine(bt_di[i].length.ToString());
                            wr.WriteLine(bt_di[i].LocalClass1.ToString());
                            wr.WriteLine(bt_di[i].LocalClass2.ToString());
                            wr.WriteLine(bt_di[i].LocalClass3.ToString());
                            wr.WriteLine(bt_di[i].ProfileNumber.ToString());
                            for (int kkk=0;kkk<bt_di[i].ProfileUUID.Length;kkk++)
                                wr.Write(bt_di[i].ProfileUUID[kkk].ToString()+" ");
                            wr.WriteLine(" ");
                            wr.Flush();
                            wr.Close();

                        }
                       
                        //BtRet = BluetoothLibNet.Api.BTGetServiceInfo(bt_di[i]);
                        // register this device in registry
                        // (can be skiped if already done before)
                        
                        PrinterFound = true;
                        ii = i;
                        break;
                    }
                }
            }
            else
            {
                //Result = MessageBox.Show
                SetErrorEvent("BT Get device info error");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }*/
            i = 0;
            ii = i;

            using (System.IO.StreamWriter wr = new System.IO.StreamWriter(
                            System.IO.Path.Combine(Program.StartupPath, "BTLog.txt"), true))
            {
                wr.WriteLine(new string(bt_di[i].DeviceAddress));
                wr.WriteLine(bt_di[i].DeviceErrorFlag.ToString());
                wr.WriteLine(bt_di[i].DeviceHandle.ToString());
                wr.WriteLine(new string(bt_di[i].DeviceName));
                wr.WriteLine(bt_di[i].length.ToString());
                wr.WriteLine(bt_di[i].LocalClass1.ToString());
                wr.WriteLine(bt_di[i].LocalClass2.ToString());
                wr.WriteLine(bt_di[i].LocalClass3.ToString());
                wr.WriteLine(bt_di[i].ProfileNumber.ToString());
                for (int kkk = 0; kkk < bt_di[i].ProfileUUID.Length; kkk++)
                    wr.Write(bt_di[i].ProfileUUID[kkk].ToString() + " ");
                wr.WriteLine(" ");
                wr.Flush();
                wr.Close();

            }
            /*
            if (PrinterFound == false)
            {
                //Status.Text = "";
                //Result = MessageBox.Show
                SetErrorEvent("BT Printer not found!");
                BluetoothLibNet.Api.BTDeInitialize();
                return;
            }*/

            BtRet = BluetoothLibNet.Api.BTSelectDevice(bt_di[ii], BluetoothLibNet.Def.BTPORT_SERIAL);
            if (BtRet != BluetoothLibNet.Def.BTERR_SUCCESS)
            {
                SetErrorEvent("BT register printer info Error" + BtRet.ToString());
                BluetoothLibNet.Api.BTDeInitialize();
                PrinterFound = false;
                _connected = false;
                return;
            }

            string [] coms = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string c in coms)
                SetStatusEvent(string.Format("Com port {0} present",c));

            SetStatusEvent("try to connect to printer...");
           /* hSerial = PortOpen(string.Format("COM{0}:",
                Program.Settings.TypedSettings[0].BTComPort),
                CBR_19200, 8, NOPARITY, ONESTOPBIT, 3000, 3000);
            
            if (hSerial.ToInt32() == INVALID_HANDLE_VALUE)
            {
                SetErrorEvent("BT Port Open Error" + Program.Settings.TypedSettings[0].BTComPort.ToString());
                BluetoothLibNet.Api.BTDeInitialize();
                BtRet = BluetoothLibNet.Def.BTERR_CONNECTION_FAILED;
                _connected = false;
            }*/

            //BtRet = BluetoothLibNet.Api.BTConnectSerial(BluetoothLibNet.Def.BTCONNECT_SERIAL_CLIENT, 3000, 3000);
            //SetStatusEvent("BTConnectSerial: "+BtRet.ToString());

            comNum = Program.Settings.TypedSettings[0].BTComPort;
            TestPrintCom();
            /*for (int k1 = 1; k1 < 10; k1++)
            {
                try
                {
                comNum = k1;
                System.Threading.Thread tr =
                    new Thread(new ThreadStart(TestPrintCom));
                signalled = true;
                //mEvt.Reset();
                tr.Start();

                int counter = 0;
                while (counter < 5)
                {
                    if (!signalled)
                        break;
                    Thread.Sleep(1000);
                    counter++;

                }
                //if (mEvt.WaitOne() == false)
                if (signalled)
                {

                    tr.Abort();
                    signalled = false;
                    //mEvt.Set();

                }
                
                
                    }
                    catch (Exception err){
                        SetStatusEvent(err.ToString());
                    }            }*/
            /*
            PortWrite(prnout, 0, hSerial);
            for (int j1 = 0; j1 < 76; j1++)
            {
                SetStatusEvent(String.Format("wait for answer...{0}", j1.ToString()));

                GetCommModemStatus(hSerial, ref CommStatus);
                if ((CommStatus & (MS_RING_ON | MS_RLSD_ON)) != 0)
                {
                    break;
                }
                Thread.Sleep(200);
            }
            BtRet = BluetoothLibNet.Def.BTERR_CONNECTED;
            _connected = true;

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
            PortClose(hSerial);*/
            BluetoothLibNet.Api.BTDeInitialize();
            //Status.Text = "";

        }
        int comNum = 0;

        
        
        
        private bool signalled = false;

        [System.Obsolete("Use TestPrint3")]
        private void TestPrintCom()
        {
            try
            {
                int k1 = comNum;
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
                /*try
                {
                    hSerial = PortOpen(string.Format("COM{0}:", k1), CBR_9600, 8, NOPARITY, ONESTOPBIT, 3000, 3000);

                    if (hSerial.ToInt32() == INVALID_HANDLE_VALUE)
                    {
                        SetErrorEvent("BT Open Error");
                        BluetoothLibNet.Api.BTDeInitialize();
                        BtRet = BluetoothLibNet.Def.BTERR_CONNECTION_FAILED;
                        _connected = false;
                        
                    }



                    PortWrite(prnout, 0, hSerial);
                    SetStatusEvent("Port written");
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
                    _connected = true;
                    PortClose(hSerial);
                }
                catch (Exception err)
                {
                    SetStatusEvent(err.ToString());
                }
                */
                try
                {
                    System.IO.Ports.SerialPort sp =
                        new System.IO.Ports.SerialPort(string.Format("COM{0}", k1),
                            19200,
                            System.IO.Ports.Parity.None,
                            8,
                            System.IO.Ports.StopBits.One);
                    sp.Open();
                    if (sp.IsOpen)
                    {
                        SetStatusEvent("Port open" + k1);
                        sp.Write(prnout, 0, prnout.Length);
                        Thread.Sleep(5000);
                        sp.Close();
                        SetStatusEvent("Port written" );
                        //return;
                    }
                    else
                        SetStatusEvent("Port closed");
                }
                catch (Exception err)
                {
                    SetStatusEvent(err.ToString());
                }

                try
                {
                    System.IO.Ports.SerialPort sp =
                        new System.IO.Ports.SerialPort(string.Format("COM{0}:", k1),
                            19200,
                            System.IO.Ports.Parity.None,
                            8,
                            System.IO.Ports.StopBits.One);
                    sp.Open();
                    if (sp.IsOpen)
                    {
                        SetStatusEvent("Port open: " + k1);
                        sp.Write(prnout, 0, prnout.Length);
                        SetStatusEvent("Port written");
                        Thread.Sleep(5000);
                        sp.Close();
                        
                    }
                    else
                        SetStatusEvent("Port closed");
                }
                catch (Exception err)
                {
                    SetStatusEvent(err.ToString());
                }


            }
            catch (Exception err)
            {
                SetStatusEvent(err.ToString());
            }
            finally
            {
                // mEvt.Set();
                signalled = false;
            }
        }

        #endregion
    }

    public class BTConnectionFailedException : System.Exception
    {
        /*private int last_error = 0;
        //public BluetoothLibNet.Def
        public BTConnectionFailedException ()
        {
            
        }*/
        /*public string GetLastErrorText()
        {
            int last_error = BluetoothLibNet.Api.BTGetLastError();
            if (BluetoothLibNet.Def.FUNCTION_UNSUPPORT != last_error)
            {
                BluetoothLibNet.Api.
            }
            else
                return "FUNCTION_UNSUPPORT";
        }*/
        
    }
}
