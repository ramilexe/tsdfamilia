using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace TSDUtils
{
    public enum ActionCode:byte
    {
        [Description("Нет действия")]
        NoAction =0,
        
        [Description("Возврат")]
        Returns = 1,

        [Description("Переоценка")]
        Reprice = 2,

        [Description("Переброска")]
        Remove = 3,

        [Description("Скорая помощь")]
        QuickHelp = 4,

        [Description("Инвент-ция Глобальная")]
        InventoryGlobal = 5,

        [Description("Инвент-ция Точечная")]
        InventoryLocal = 6,

        [Description("Действие7")]
        Action7 = 7,

        [Description("Действие8")]
        Action8 = 8

    }

    public class ActionCodeDescription
    {
        static Dictionary<ActionCode, string> dict =
            new Dictionary<ActionCode, string>();

        private ActionCodeDescription()
        {


            foreach (ActionCode ac in Enum.GetValues(typeof(ActionCode)))
            {
                System.Reflection.MemberInfo[] memInfo = ac.GetType().GetMember(ac.ToString());
                String paramDescription = "";
                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(
                        typeof(DescriptionAttribute), false);

                    if (attrs != null && attrs.Length > 0)
                        paramDescription = ((DescriptionAttribute)attrs[0]).Description;
                    else
                        paramDescription = ac.ToString();

                    dict.Add(ac, paramDescription);
                }


            }
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

    }
    

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

    public enum WorkMode : byte
    {
        Always = 0,
        ByPriority = 1
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

}
