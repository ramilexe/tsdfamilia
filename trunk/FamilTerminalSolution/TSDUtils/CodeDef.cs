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
}
