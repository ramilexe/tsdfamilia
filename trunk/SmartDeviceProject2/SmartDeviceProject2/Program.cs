using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public delegate void DatabaseChanged();
    static class Program
    {
        
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            int storePages=0;
            int ramPages=0;
            int pageSize=0;
            bool v = NativeClass.GetSystemMemoryDivision(ref storePages,ref ramPages,ref pageSize);

            int i = NativeClass.SystemStorageMemory;
            NativeClass.SystemStorageMemory = i / 2;
            SystemMemoryChangeStatusEnum s =
                NativeClass.ChangeStatus;

            /*
            using (System.IO.StreamWriter w = new System.IO.StreamWriter("test.txt"))
            {
                
                for (int i = 0; i < 29000; i++)
                {
                    try
                    {
                        byte[] b1 = System.Text.Encoding.GetEncoding(i).GetBytes("ПЕНА Д/БРИТЬЯ АРКО 200");
                        string t = System.Text.Encoding.GetEncoding(i).GetString(b1, 0, b1.Length);
                        w.WriteLine("length = {0}, codepage {1}, text={2}", b1.Length,i, t);
                    }
                    catch { }
                }
                w.Flush();
                w.Close();
            }
            return;*/


            try
            {
                

                Application.Run(new Form1());
            }
            finally
            {
                
            }
        }
    }
}