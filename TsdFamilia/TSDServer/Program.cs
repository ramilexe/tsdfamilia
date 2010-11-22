using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;

namespace TSDServer
{
    static class Program
    {
        //главное окно программы
        static Form1 mainForm = null;
        //класс сервера удаленного управления
        static RemoteObject ro = new RemoteObject();
        
        /// <summary>
        /// статический метод для вызова метода Show главного окна программы
        /// </summary>
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
            using (System.Data.SqlServerCe.SqlCeConnection conn =
                new System.Data.SqlServerCe.SqlCeConnection(Properties.Settings.Default.ProductsConnectionString))
            {
                conn.Open();
                using (System.Data.SqlServerCe.SqlCeCommand cmd =
                    new System.Data.SqlServerCe.SqlCeCommand("select * from productsbintbl", conn))
                {
                    using (System.Data.SqlServerCe.SqlCeCommand cmdUpd =
                    new System.Data.SqlServerCe.SqlCeCommand(@"UPDATE    ProductsBinTbl SET ActionCode = @ac
, Shablon = @sc
, SoundCode = @sndc
WHERE     (ProductsBinTbl.Barcode = @b)", conn))
                    {
                        
                        cmdUpd.Parameters.Add("@ac", typeof(byte));
                        cmdUpd.Parameters.Add("@sc", typeof(int));
                        cmdUpd.Parameters.Add("@sndc", typeof(int));
                        cmdUpd.Parameters.Add("@b", typeof(Int64));

                        using (System.Data.SqlServerCe.SqlCeDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                
                                Int64 bc =  (Int64)rdr[0];
                                cmdUpd.Parameters[3].Value = bc;
                                byte c = 0;
                                System.Random r = new Random();
                                foreach (TSDUtils.ActionCode i in Enum.GetValues(typeof(TSDUtils.ActionCode)))
                                {
                                    byte b = (byte)i;
                                    //c = (((byte)r.NextDouble()) & b)|((byte)c);



                                    
                                }
                            }


                        }
                    }

                }


            }
            //TSDUtils.ActionCode a = TSDUtils.ActionCode.Remove | TSDUtils.ActionCode.Reprice;
            //TSDUtils.ActionCode b = TSDUtils.ActionCode.Remove | TSDUtils.ActionCode.Returns;
            //TSDUtils.ActionCode c = TSDUtils.ActionCode.Remove | TSDUtils.ActionCode.Returns | TSDUtils.ActionCode.Reprice;
            /*Array e = Enum.GetValues(typeof(TSDUtils.ActionCode));
            int counter = 0;
            byte[] bArray = new byte[e.Length];
            string s = string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (TSDUtils.ActionCode i in e)
            {
                byte b = (byte)i;
                bArray[counter++] = b;
                sb.AppendFormat("{0} = {1} \n", i, b);
            }
            TSDUtils.ActionCode tmp = TSDUtils.ActionCode.NoAction;
            

            for (int i = 0; i < bArray.Length; i++)
            {
                tmp = TSDUtils.ActionCode.NoAction;
                for (int j = 0; j < bArray.Length; j++)
                {
                    if (bArray[i] == bArray[j])
                        continue;

                    tmp = tmp |(TSDUtils.ActionCode) bArray[j];
                    byte b = (byte)tmp;
                    sb.AppendFormat("{0} = {1} \n", tmp, b);

                }
            }
            */

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1();
            //проверка на наличие второй запущеной копии программы
            //если Null значит уже запущена другая копия
            if (mainForm.mutex != null)
            {
                //другая копия программы не запущена
                //инициализируем IPC сервер, который может принимать сообщения
                //(в данном случае нужно для получения сообщения от второй копии показать главное окно
                IpcChannel serverChannel =
                    new IpcChannel("localhost:9090");

                System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(
                    serverChannel,false);

                System.Runtime.Remoting.WellKnownServiceTypeEntry WKSTE =
               new System.Runtime.Remoting.WellKnownServiceTypeEntry(
                   typeof(RemoteObject), "RemoteObject.rem", System.Runtime.Remoting.WellKnownObjectMode.Singleton);
                System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownServiceType(WKSTE);

                Application.Run(mainForm);//запуск главного экранного потока
            }
            else
            {
                //есть уже запущенная копия программы
                IpcChannel channel = new IpcChannel();
                System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel,false);
                //получаем адрес сервера программы
                RemoteObject service = (RemoteObject)Activator.GetObject(
                               typeof(RemoteObject), "ipc://localhost:9090/RemoteObject.rem");
                
                //отправляем сообщение показать главное окно
                service.Show();

                //mainForm.Activate();
                //выходим из программы
                
            }
        }
    }


// Remote object.
    /// <summary>
    /// класс для сервера управления
    /// </summary>
    public class RemoteObject :System.MarshalByRefObject
    {
        /// <summary>
        /// вызывает у главного класса програмы (Program) статический метод показать главное окно прораммы
        /// </summary>
    public void Show()
    {
        Program.Show();
    }
}



}
