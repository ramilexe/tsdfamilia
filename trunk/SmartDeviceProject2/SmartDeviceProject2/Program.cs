using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    static class Program
    {
        public static ScanClass scaner = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {

            try
            {
                scaner = new ScanClass();

                Application.Run(new Form1());
            }
            finally
            {
            }
        }
    }
}