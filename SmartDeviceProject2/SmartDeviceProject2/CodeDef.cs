using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace TSDUtils
{
    /// <summary>
    /// Типы документов
    /// </summary>
    public enum ActionCode : byte
    {
        //[Description("Нет действия")]
        /// <summary>
        /// Нет действия
        /// </summary>
        NoAction = 0,

        //[Description("Возврат")]
        /// <summary>
        /// Возврат
        /// </summary>
        Returns = 1,

        //[Description("Переоценка")]

        /// <summary>
        /// Переоценка
        /// </summary>
        Reprice = 2,

        //[Description("Переброска")]
        /// <summary>
        /// Переброска
        /// </summary>
        Remove = 3,

        //[Description("Скорая помощь")]
        /// <summary>
        /// Скорая помощь
        /// </summary>
        QuickHelp = 4,

        //[Description("")]
        /// <summary>
        /// Инвентаризация
        /// </summary>
        InventoryGlobal = 5,

        //[Description("Действие6")]
        /// <summary>
        /// Зарезервировано для дальнейшего использования
        /// </summary>
        InventoryLocal = 6,

        //[Description("Действие7")]
        /// <summary>
        /// Зарезервировано для дальнейшего использования
        /// </summary>
        Action7 = 7,

        //[Description("Действие8")]
        /// <summary>
        /// Зарезервировано для дальнейшего использования
        /// </summary>
        Action8 = 8

    }
    /// <summary>
    /// Типы шаблонов-этикеток (а также звуков, вибро)
    /// </summary>
    public enum ShablonCode : byte
    {
        NoShablon = 0,
        Shablon1 = 1,
        Shablon2 = 2,
        Shablon3 = 3,
        Shablon4 = 4,
        Shablon5 = 5,
        Shablon6 = 6,
        Shablon7 = 7
    }
    /// <summary>
    /// Режим работы
    /// </summary>
    public enum WorkMode : byte
    {
        /// <summary>
        /// Режим работы ВСЕГДА
        /// </summary>
        Always = 0,

        /// <summary>
        /// режим работы по ПРИОРИТЕТУ
        /// </summary>
        ByPriority = 1
    }

    public class ActionCodeDescription
    {
        static Dictionary<ActionCode, string> dict =
            new Dictionary<ActionCode, string>();
        
        private ActionCodeDescription()
        {
            dict.Add(ActionCode.NoAction, "НЕТ ДЕЙСТВИЯ");
            dict.Add(ActionCode.Returns, "ВОЗВРАТ");
            dict.Add(ActionCode.Reprice, "ПЕРЕОЦЕНКА");
            dict.Add(ActionCode.Remove, "ПЕРЕБРОСКА");
            dict.Add(ActionCode.InventoryGlobal, "Инвент-ция Глобальная");
            dict.Add(ActionCode.QuickHelp, "СКОРАЯ ПОМОЩЬ");
            dict.Add(ActionCode.InventoryLocal, "Инвент-ция Точечная");
            dict.Add(ActionCode.Action7, "Действие7");
            dict.Add(ActionCode.Action8, "Действие8");


            
        }

        static ActionCodeDescription acd = new ActionCodeDescription();
        public static ActionCodeDescription ActionDescription
        {
            get
            {
                return acd;
            }
        }
        public string this[ActionCode i]
        {
            get
            {
                return dict[i];
            }
        }
        public string this[byte i]
        {
            get
            {
                return dict[(ActionCode)i];
            }
        }
        public uint GetShablon(byte ac, uint productshablonCode)
        {
            return productshablonCode;
           /* byte i = (byte)(Math.Log(ac) / Math.Log(2));

            if (((1 << i) & ac) != 0)
            {
                uint b = productshablonCode >> (3 * i);
                uint c = (uint)(7 << (3 * i));
                uint a = (uint)(c & productshablonCode);

                return (uint)(a >> (3*i));
            }

            return 0;*/
        }
        public uint GetShablon(ActionCode ac, uint productshablonCode)
        {
            return GetShablon((byte)ac, productshablonCode);
        }
    }

    /// <summary>
    /// Init and save/parse string for PlayVibro function
    /// </summary>
    public class VibroDef
    {
        VibroCodes vibroType;
        /// <summary>
        /// This parameter is for specifying a vibration pattern selecting one of the listed patterns in the table below.
        /// B_ALARM Alarm 1 1000 1000
        ///B_WARNING Warning 1 1000 1000
        ///B_SCANEND Scanning complete 1 1000 1000
        ///B_CARDREAD Card read/write 1 1000 1000
        ///B_WIREREAD Wireless in call 1 1000 1000
        ///B_USERDEF User defined -
        /// </summary>
        public VibroCodes VibroType
        {
            get { return vibroType; }
            set { vibroType = value; }
        }


        int count;
///        Count
///This parameter is for specifying the number of times in the range of 1 to 20 (effect only for
///the user defined mode) to turn on the vibrator. VIBRATOR_DEFAULT is used in other
///cases than B_USERDEF.
        public int Count
        {
            get { return count; }
            set { count = value; }
        }


        int ontime;

        /// <summary>
        /// This parameter is for specifying a time period in the range of 0 to 16,000 milliseconds for
        ///tuning on the vibrator (effect only for the user defined mode). VIBRATOR_DEFAULT is
        ///used in cases other than B_USERDEF.
        /// </summary>
        public int OnTime
        {
            get { return ontime; }
            set { ontime = value; }
        }
        int offtime;

        /// <summary>
        /// This parameter is specifying for a time period in the range of 0 to 1,000 milliseconds for
        ///turning off the vibrator (effect only for the user defined mode). VIBRATOR_DEFAULT is
        ///used in cases other than B_USERDEF.
        /// </summary>
        public int OFFTime
        {
            get { return offtime; }
            set { offtime = value; }
        }
        public VibroDef()
        {

        }
        public VibroDef(string str)
        {
            string[] s = str.Split('|');
            vibroType = (VibroCodes)Enum.Parse(typeof(VibroCodes), s[0], true);
            count = int.Parse(s[1]);
            ontime = int.Parse(s[2]);
            offtime = int.Parse(s[3]);
        }

        public override string ToString()
        {
            string[] s = new string[4];
            s[0] = vibroType.ToString();
            s[1] = count.ToString();
            s[2] = ontime.ToString();
            s[3] = offtime.ToString();
            return string.Join("|", s);
        }

        public static VibroDef Parse(string str)
        {
            return new VibroDef(str);
            /*string[] s = str.Split('|');
            SoundCodes sndCode = (SoundCodes)Enum.Parse(typeof(SoundCodes), s[0], true);
            int frequency = int.Parse(s[1]);
            int time = int.Parse(s[2]);*/
        }
    }

    /// <summary>
    /// Init and save/parse string for PlayVibro function
    /// </summary>
    public class SoundDef
    {
        SoundCodes sndType;
        /// <summary>
        /// This parameter is for specifying a buzzer sound selecting one of the values in the table below.
        /// Setting Value Sound
        ///          Sound                                          DT-X11 IT-600 DT-X7 DT-X30 IT-3100 IT-800
        ///B_USERDEF User defined                                   Y       Y       Y   Y       --      Y
        ///B_TAP Tap (2600Hz for 25 milliseconds)                   Y       Y       --  Y       --      Y
        ///B_CLICK Key click (2800Hz for 50 milliseconds)           Y       Y       Y   Y       --      Y
        ///B_ALARM Alarm (3500Hz for 150 milliseconds)              Y       Y       Y   Y       --      Y
        /// B_WARNING Warning (3000Hz for 100 milliseconds)         Y       Y       Y   Y       --      Y
        ///B_SCANEND Scan complete (3300Hz for 75 milliseconds)     Y       Y       Y   Y       --      Y
        ///B_CARDRED Card read/write (2900Hz for 100 milliseconds) -- -- -- -- -- --
        ///B_WIREREAD Wireless in call (3200Hz for 100 milliseconds) -- -- -- -- -- --
        /// </summary>
        public SoundCodes SoundType
        {
            get { return sndType; }
            set { sndType = value; }
        }
        int frequency;
        /// <summary>
        /// This parameter is for specifying frequency of buzzer sound which is effect only when user
        ///defined sound is specified. BUZ_DEFAULT is used in cases other than BUZ_USERDEF.
        /// </summary>
        public int Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }
        int time;

        ///This parameter is for specifying a period of ON time for sounding the buzzer which is
        ///effect only when user defined sound is specified. BUZ_DEFAULT is used in cases other
        ///than BUZ_USERDEF.
        public int Time
        {
            get { return time; }
            set { time = value; }
        }

        public SoundDef()
        {

        }
        public SoundDef(string str)
        {
            string[] s = str.Split('|');
            sndType = (SoundCodes)Enum.Parse(typeof(SoundCodes), s[0], true);
            frequency = int.Parse(s[1]);
            time = int.Parse(s[2]);
        }

        public override string ToString()
        {
            string[] s = new string[3];
            s[0] = sndType.ToString();
            s[1] = frequency.ToString();
            s[2] = time.ToString();
            return string.Join("|", s);
        }

        public static SoundDef Parse(string str)
        {
            return new SoundDef(str);
            /*string[] s = str.Split('|');
            SoundCodes sndCode = (SoundCodes)Enum.Parse(typeof(SoundCodes), s[0], true);
            int frequency = int.Parse(s[1]);
            int time = int.Parse(s[2]);*/
        }
    }

    public enum SoundCodes : int
    {

        B_USERDEF = Calib.SystemLibNet.Def.B_USERDEF,
        B_TAP = Calib.SystemLibNet.Def.B_TAP,
        B_CLICK = Calib.SystemLibNet.Def.B_CLICK,
        B_ALARM = Calib.SystemLibNet.Def.B_ALARM,
        B_WARNING = Calib.SystemLibNet.Def.B_WARNING,
        B_SCANEND = Calib.SystemLibNet.Def.B_SCANEND,
        B_CARDREAD = Calib.SystemLibNet.Def.B_CARDREAD,
        B_WIREREAD = Calib.SystemLibNet.Def.B_WIREREAD
    }

    public enum VibroCodes : int
    {
        B_USERDEF = Calib.SystemLibNet.Def.B_USERDEF,
        B_ALARM = Calib.SystemLibNet.Def.B_ALARM,
        B_WARNING = Calib.SystemLibNet.Def.B_WARNING,
        B_SCANEND = Calib.SystemLibNet.Def.B_SCANEND,
        B_CARDREAD = Calib.SystemLibNet.Def.B_CARDREAD,
        B_WIREREAD = Calib.SystemLibNet.Def.B_WIREREAD

    }

    /*
    [Flags]
    public enum ReturnsShablonCode : uint
    {
        NoShablon = 0,
        Shablon1 = 1,
        Shablon2 = 2,
        Shablon3 = 3,
        Shablon4 = 4,
        Shablon5 = 5,
        Shablon6 = 6,
        Shablon7 = 7
    }

    [Flags]
    public enum RepriceShablonCode : uint
    {
        NoShablon = 0,
        Shablon1 = 8,
        Shablon2 = 16,
        Shablon3 = 24,
        Shablon4 = 32,
        Shablon5 = 40,
        Shablon6 = 48,
        Shablon7 = 56
    }

    [Flags]
    public enum RemoveShablonCode : uint
    {
        NoShablon = 0,
        Shablon1 = 64,
        Shablon2 = 128,
        Shablon3 = 192,
        Shablon4 = 256,
        Shablon5 = 320,
        Shablon6 = 384,
        Shablon7 = 448
    }

    [Flags]
    public enum InventoryShablonCode : uint
    {
        NoShablon = 0,
        Shablon1 = 512,
        Shablon2 = 1024,
        Shablon3 = 1536,
        Shablon4 = 2048,
        Shablon5 = 2560,
        Shablon6 = 3072,
        Shablon7 = 3584
    }
    */
}
