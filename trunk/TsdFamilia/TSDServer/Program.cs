using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;

namespace TSDServer
{
    static class Program
    {
        static Form1 mainForm = null;
        static RemoteObject ro = new RemoteObject();
        public static void Show()
        {
            mainForm.Show();
            mainForm.Activate();
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand)]
        [STAThread]
        static void Main()
        {
            /*using (System.Data.SqlServerCe.SqlCeConnection conn =
                new System.Data.SqlServerCe.SqlCeConnection(
                    "Data Source=Копия Products.sdf"))
            {
                conn.Open();
                using (System.IO.StreamWriter writer =
                    new System.IO.StreamWriter("test_goods.txt",true,System.Text.Encoding.GetEncoding("windows-1251")))
                    {
                using (System.Data.SqlServerCe.SqlCeCommand cmd
                     = new System.Data.SqlServerCe.SqlCeCommand())
                {
                    cmd.CommandText = "select * from  productsTbl";
                    cmd.Connection = conn;
                    System.Data.SqlServerCe.SqlCeDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            writer.Write("{0}|",rdr[i].ToString());

                        }
                        writer.Write("\n");
                    }
                    writer.Flush();
                }
                }
            }
            return;*/
                
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1();
            if (mainForm.mutex != null)
            {
                IpcChannel serverChannel =
                    new IpcChannel("localhost:9090");

                System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(
                    serverChannel);

                System.Runtime.Remoting.WellKnownServiceTypeEntry WKSTE =
               new System.Runtime.Remoting.WellKnownServiceTypeEntry(
                   typeof(RemoteObject), "RemoteObject.rem", System.Runtime.Remoting.WellKnownObjectMode.Singleton);
                System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownServiceType(WKSTE);
                Application.Run(mainForm);
            }
            else
            {
                IpcChannel channel = new IpcChannel();
                System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel);

                RemoteObject service = (RemoteObject)Activator.GetObject(
                               typeof(RemoteObject), "ipc://localhost:9090/RemoteObject.rem");

                service.Show();

                mainForm.Activate();
                
            }
        }
    }


// Remote object.
    
    public class RemoteObject :System.MarshalByRefObject
    {
    public void Show()
    {
        Program.Show();
        //Mainform.Show();
    }
}



}
