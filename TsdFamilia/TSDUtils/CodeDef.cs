using System;
using System.Collections.Generic;
using System.Text;

namespace TSDUtils
{
    [Flags]
    public enum ActionCode:byte
    {
        NoAction =0,
        Returns = 1,
        Reprice = 2,
        Remove = 4,
        Action4 = 8,
        Action5 = 16,
        Action6 = 32,
        Action7 = 64,
        Action8 = 128,

    }
    [Flags]
    public enum ShablonCode : byte
    {
        NoShablon = 0,
        Shablon1 = 1,
        Shablon2 = 2,
        Shablon3 = 4,
        Shablon4 = 8,
        Shablon5 = 16,
        Shablon6 = 32,
        Shablon7 = 64,
        Shablon8 = 128
    }

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


}
