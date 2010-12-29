using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SetDeviceTime
{
    class Program
    {
        static void Main(string[] args)
        {
           
            string s = args[0];
            System.Globalization.DateTimeFormatInfo dtfi =
                new System.Globalization.DateTimeFormatInfo();
            dtfi.FullDateTimePattern = "dd/MM/yyyy HH:mm:ss";
            dtfi.ShortDatePattern = "dd/MM/yyyy";
            dtfi.ShortTimePattern = "HH:mm:ss";
            dtfi.DateSeparator = "/";
            dtfi.TimeSeparator = ":";
            

            string s1 = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss",dtfi);
            string s2 = s.Remove(s.Length - 4,4);
            string s3 = s.Substring(s.Length - 4, 4);

            DateTime dt =
                DateTime.Parse(s2, dtfi);


            SYSTEMTIME time = new SYSTEMTIME(dt);
            //time.wDay = (ushort)dt.Day;
            //time.wHour = (ushort)dt.Hour;
            //time.wDayOfWeek = (ushort)dt.DayOfWeek;
            //time.wMilliseconds = (ushort)dt.Millisecond;
            //time.wMinute = (ushort)dt.Minute;
            //time.wMonth = (ushort)dt.Month;
            //time.wSecond = (ushort)dt.Second;
            //time.wYear = (ushort)dt.Year;

            SetLocalTime(ref time);

            TIME_ZONE_INFORMATION tzI = new TIME_ZONE_INFORMATION();
            GetTimeZoneInformation(ref tzI);
            tzI.Bias = int.Parse(s3)*60;
            tzI.StandardBias = 0;
            //tzI.DaylightBias = 0;
            //tzI.StandardName = "Moscow";
            //tzI.DaylightName = "Moscow";
            //tzI.DaylightDate = time;
            //tzI.StandardDate = time;


            SetTimeZoneInformation(ref tzI);
            
            Microsoft.Win32.Registry.LocalMachine.Flush();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)]
            public Int32 Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;
            public SYSTEMTIME StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;
            public SYSTEMTIME DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 DaylightBias;
        }
        [DllImport("coredll.dll")]
        public static extern int RegFlushKey(IntPtr hKey);

        [DllImport("coredll.dll", CharSet = CharSet.Unicode)]
        public extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll", CharSet = CharSet.Unicode)]
        public extern static uint SetTimeZoneInformation(
            ref TIME_ZONE_INFORMATION lpTimeZoneInformation);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern uint SetLocalTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern uint GetTimeZoneInformation(
            ref TIME_ZONE_INFORMATION lpTimeZoneInformation);

        [DllImport("coredll.dll")]
        public extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        //[DllImport("coredll.dll")]
        //private extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);
    }

}