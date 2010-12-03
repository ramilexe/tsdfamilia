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
        private byte[] m_soundBytes;
        private string m_fileName;

        private enum Flags
        {
            SND_SYNC = 0x0000,  /* play synchronously (default) */
            SND_ASYNC = 0x0001,  /* play asynchronously */
            SND_NODEFAULT = 0x0002,  /* silence (!default) if sound not found */
            SND_MEMORY = 0x0004,  /* pszSound points to a memory file */
            SND_LOOP = 0x0008,  /* loop the sound until next sndPlaySound */
            SND_NOSTOP = 0x0010,  /* don't stop any currently playing sound */
            SND_NOWAIT = 0x00002000, /* don't wait if the driver is busy */
            SND_ALIAS = 0x00010000, /* name is a registry alias */
            SND_ALIAS_ID = 0x00110000, /* alias is a predefined ID */
            SND_FILENAME = 0x00020000, /* name is file name */
            SND_RESOURCE = 0x00040004  /* name is resource name or atom */
        }

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

        /* /// Construct the Sound object to play sound data from the specified file.
        /// </summary>
        public Sound (string fileName) {
            m_fileName = fileName;
        }

        /// <summary>
        /// Construct the Sound object to play sound data from the specified stream.
        /// </summary>
        public Sound(Stream stream)    {
            // read the data from the stream
            m_soundBytes = new byte [stream.Length];
            stream.Read(m_soundBytes, 0,(int)stream.Length);
        }*/

        /// <summary>
        /// Play the sound
        /// </summary>
        public static void Play(string m_fileName)
        {
            // if a file name has been registered, call WCE_PlaySound,
            //  otherwise call WCE_PlaySoundBytes
            if (m_fileName != null)
                WCE_PlaySound(m_fileName, IntPtr.Zero, (int) (Flags.SND_ASYNC | Flags.SND_FILENAME));
            //else
             //   WCE_PlaySoundBytes (m_soundBytes, IntPtr.Zero, (int) (Flags.SND_ASYNC | Flags.SND_MEMORY));
        }

        public static void Play(System.IO.Stream stream)
        {
            byte [] m_soundBytes = new byte[stream.Length];
            stream.Read(m_soundBytes, 0, (int)stream.Length);
            // if a file name has been registered, call WCE_PlaySound,
            //  otherwise call WCE_PlaySoundBytes
            //if (m_fileName != null)
            //    WCE_PlaySound(m_fileName, IntPtr.Zero, (int)(Flags.SND_ASYNC | Flags.SND_FILENAME));
            //else
            WCE_PlaySoundBytes(m_soundBytes, IntPtr.Zero, (int)(Flags.SND_ASYNC | Flags.SND_MEMORY));
        }

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern bool GetSystemMemoryDivision(
                                       ref int lpdwStorePages,
                                       ref int lpdwRamPages,
                                       ref int lpdwPageSize);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern int SetSystemMemoryDivision(
                                       int dwStorePages);

        [DllImport("CoreDll.DLL", EntryPoint = "PlaySound", SetLastError = true)]
        private extern static int WCE_PlaySound(string szSound, IntPtr hMod, int flags);

        [DllImport("CoreDll.DLL", EntryPoint = "PlaySound", SetLastError = true)]
        private extern static int WCE_PlaySoundBytes(byte[] szSound, IntPtr hMod, int flags);

    }
}
