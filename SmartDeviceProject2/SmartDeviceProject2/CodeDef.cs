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
