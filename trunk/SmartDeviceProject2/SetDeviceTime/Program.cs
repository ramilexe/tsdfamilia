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
            TIME_ZONE_INFORMATION tz = new TIME_ZONE_INFORMATION();
            GetTimeZoneInformation(ref tz);

            string s = args[0];
            System.Globalization.DateTimeFormatInfo dtfi =
                new System.Globalization.DateTimeFormatInfo();
            dtfi.FullDateTimePattern = "dd/MM/yyyy HH:mm:ss zz";
            dtfi.DateSeparator = "/";
            dtfi.TimeSeparator = ":";
            DateTime dt =
                DateTime.Parse(s, dtfi);

            
            SYSTEMTIME time = new SYSTEMTIME(dt);
            //time.wDay = (ushort)dt.Day;
            //time.wHour = (ushort)dt.Hour;
            //time.wDayOfWeek = (ushort)dt.DayOfWeek;
            //time.wMilliseconds = (ushort)dt.Millisecond;
            //time.wMinute = (ushort)dt.Minute;
            //time.wMonth = (ushort)dt.Month;
            //time.wSecond = (ushort)dt.Second;
            //time.wYear = (ushort)dt.Year;

            SetSystemTime(ref time);
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
    
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct TIME_ZONE_INFORMATION 
        {
            [MarshalAs(UnmanagedType.I4)]            
            public Int32 Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]    
            public string StandardName;
            public SYSTEMTIME StandardDate;
            [MarshalAs(UnmanagedType.I4)]            
            public Int32 StandardBias;  
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]    
            public string DaylightName;
            public SYSTEMTIME DaylightDate;  
            [MarshalAs(UnmanagedType.I4)]            
            public Int32 DaylightBias;
        }

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
