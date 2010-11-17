using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Familia.TSDClient
{
    /*
     0 - memory division changed successfully
1 - need to reboot to accept memory division changes
2 - cannot change memory division because the system should be rebooted after the previous call before
3 - change failed.
     * */
    public enum SystemMemoryChangeStatusEnum { SYSMEM_CHANGED, SYSMEM_NEEDREBOOT, SYSMEM_MUSTREBOOT, SYSMEM_FAILED };
    public class NativeClass
    {
        const int m_memoryPageSize = 0x00000004;
        static SystemMemoryChangeStatusEnum _status;
        static int storePages = 0;
        static int ramPages = 0;
        static int pageSize = 0;
        public static SystemMemoryChangeStatusEnum ChangeStatus
        {
            get
            {
                return _status;
            }
        }
        /// <summary>
        /// Gets or sets the amount of allocated memory for Storage,
        /// in kilobytes
        /// </summary>
        public static int SystemStorageMemory
        {
            get
            {
               /* int storePages = 0;
                int ramPages = 0;
                int pageSize = 0;*/

                GetSystemMemoryDivision(ref storePages,
                                                      ref ramPages,
                                                      ref pageSize);

                return (storePages * (pageSize >> 10));
            }
            set
            {
                if (pageSize == 0)
                    pageSize = m_memoryPageSize;
                

              _status = (SystemMemoryChangeStatusEnum)  SetSystemMemoryDivision(
                       (value << 10) / pageSize/*m_memoryPageSize*/);
            }
        }
        [DllImport("coredll.dll", SetLastError = true)]
        public static extern bool GetSystemMemoryDivision(
                                       ref int lpdwStorePages,
                                       ref int lpdwRamPages,
                                       ref int lpdwPageSize);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern int SetSystemMemoryDivision(
                                       int dwStorePages);
    }
}
