using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace TSDServer
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

        private static Int32 FILE_DEVICE_HAL = 0x00000101;
        private static Int32 FILE_ANY_ACCESS = 0x0;
        private static Int32 METHOD_BUFFERED = 0x0;

        private static Int32 IOCTL_HAL_GET_DEVICEID =
    ((FILE_DEVICE_HAL) << 16) | ((FILE_ANY_ACCESS) << 14)
     | ((21) << 2) | (METHOD_BUFFERED);

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

        public enum ReaderUnitTypes
        {
            None,
            Dolphin,
            Symbol,
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

        [DllImport("coredll.dll")]
        private static extern bool KernelIoControl(Int32 IoControlCode, IntPtr
          InputBuffer, Int32 InputBufferSize, byte[] OutputBuffer, Int32
          OutputBufferSize, ref Int32 BytesReturned);
        
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        private static extern int SystemParametersInfo(int uiAction, int
        uiParam, string pvParam, int fWinIni);

        public static string GetDeviceID()
{
    byte[] OutputBuffer = new byte[256];
    Int32 OutputBufferSize, BytesReturned;
    OutputBufferSize = OutputBuffer.Length;
    BytesReturned = 0;

    // Call KernelIoControl passing the previously defined
    // IOCTL_HAL_GET_DEVICEID parameter
    // We don’t need to pass any input buffers to this call
    // so InputBuffer and InputBufferSize are set to their null
    // values
    bool retVal = KernelIoControl(IOCTL_HAL_GET_DEVICEID, 
            IntPtr.Zero,
            0,
            OutputBuffer,
            OutputBufferSize,
            ref BytesReturned);

    // If the request failed, exit the method now
    if (retVal == false)
    {
        return null;
    }
    
    // Examine the OutputBuffer byte array to find the start of the 
    // Preset ID and Platform ID, as well as the size of the
    // PlatformID. 
    // PresetIDOffset – The number of bytes the preset ID is offset
    //                  from the beginning of the structure
    // PlatformIDOffset - The number of bytes the platform ID is
    //                    offset from the beginning of the structure
    // PlatformIDSize - The number of bytes used to store the
    //                  platform ID
    // Use BitConverter.ToInt32() to convert from byte[] to int
    Int32 PresetIDOffset = BitConverter.ToInt32(OutputBuffer, 4); 
    Int32 PlatformIDOffset = BitConverter.ToInt32(OutputBuffer, 0xc);
    Int32 PlatformIDSize = BitConverter.ToInt32(OutputBuffer, 0x10);

    // Convert the Preset ID segments into a string so they can be 
    // displayed easily.
    StringBuilder sb = new StringBuilder();
    sb.Append(String.Format("{0:X8}-{1:X4}-{2:X4}-{3:X4}-", 
         BitConverter.ToInt32(OutputBuffer, PresetIDOffset), 
         BitConverter.ToInt16(OutputBuffer, PresetIDOffset +4), 
         BitConverter.ToInt16(OutputBuffer, PresetIDOffset +6), 
         BitConverter.ToInt16(OutputBuffer, PresetIDOffset +8))); 

    // Break the Platform ID down into 2-digit hexadecimal numbers
    // and append them to the Preset ID. This will result in a 
    // string-formatted Device ID
    for (int i = PlatformIDOffset; 
         i < PlatformIDOffset +   PlatformIDSize; 
         i ++ )  
    {
        sb.Append(String.Format("{0:X2}", OutputBuffer[i]));
    }
    
    // return the Device ID string
    return sb.ToString();
}

        public static string GetOemInfo()
        {
            const int SPI_GETOEMINFO = 258;
            string oemInfo = new string(' ', 50);
            int result = SystemParametersInfo(SPI_GETOEMINFO, 50, oemInfo,
            0);
            oemInfo = oemInfo.Substring(0,
            oemInfo.IndexOf('\0')).ToUpper();

            /*ReaderUnitTypes readerType = ReaderUnitTypes.None;

            // Be sure to use upper case text when doing an IndexOf.
            if (oemInfo.IndexOf("HHP") > -1)
            {
                readerType = ReaderUnitTypes.Dolphin;
            }
            else if (oemInfo.IndexOf("SYMBOL") > -1)
            {
                readerType = ReaderUnitTypes.Symbol;
            }

            return readerType;
             * */
            return oemInfo;
        }
    }
}
