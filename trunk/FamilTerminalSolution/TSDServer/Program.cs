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
            /*ProductsDataSet ds = new ProductsDataSet();
            
            using (System.IO.StreamWriter wr = new System.IO.StreamWriter("documents1.txt"))
            {
                using (ProductsDataSetTableAdapters.DocsBinTblTableAdapter ta =
                    new TSDServer.ProductsDataSetTableAdapters.DocsBinTblTableAdapter())
                {

                    ta.Fill(ds.DocsBinTbl);
                }
                string[] s = new string [ds.DocsTbl.Columns.Count];
                string stringToWrite = string.Empty;
                foreach (ProductsDataSet.DocsBinTblRow row in ds.DocsBinTbl)
                {
                    ProductsDataSet.DocsTblRow docRow = ds.ConvertFromBin(row);
                    for (int i = 0; i < docRow.Table.Columns.Count; i++)
                    {
                        s[i] = docRow[i].ToString();
                    }
                    stringToWrite = String.Join("|", s);
                    wr.WriteLine(stringToWrite);
                }
                wr.Flush();
                wr.Close();
            }
            return;*/
            //}
            /*
            byte [] ba = new byte[155];
            for (byte b = 0; b < 155; b++)
            {
                ba[b] = (byte)(b+100);
            }
            string s = System.Text.Encoding.GetEncoding("windows-1251").GetString(ba);
            TSDServer.CustomEncodingClass custEnc = new CustomEncodingClass();
            byte [] ba = custEnc.GetBytes("ХОЗЯЙСТВЕННЫЙ САНТИМЕТР|В АСС.|Китай|357792|ПЛАСТИК");
            
            string s = System.Text.Encoding.GetEncoding("windows-1251").GetString(ba);

            return;*/
            
            /*
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
                        System.Random r = new Random();
                        Array vals = Enum.GetValues(typeof(TSDUtils.ActionCode));
                        Array vals1 = Enum.GetValues(typeof(TSDUtils.ShablonCode));

                        using (System.Data.SqlServerCe.SqlCeDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                
                                Int64 bc =  (Int64)rdr[0];
                                cmdUpd.Parameters[3].Value = bc;
                                byte c = 0;
                                
                                
                                for (int k=0;k<5;k++)
                                
                                {
                                    int b = 0;
                                    Double d = Math.Round(r.NextDouble());//произвольное число от 0 до 1
                                    //при округлении получаем случайное значение 0 или 1
                                    b = (byte)((byte)vals.GetValue(k) * ((byte)d));//Если d=0, то указанный k-й код действия не используется,
                                    //иначе, если 1 - то используется
                                    c = (byte)(b|c);//суммируем все биты
                                }
                                uint sum = 0;
                                //по каждому биту действия
                                for (byte k = 0; k < 8; k++)
                                {
                                    //определить произвольный код шаблона
                                    byte d1 = (byte)r.Next( 8);
                                    //код действия
                                    byte b1 = (byte)(1 << k);//Math.Pow(2, k);
                                    
                                    byte b = (byte)(c & b1);
                                    if (b != 0)//если код действия продукта содержит необходимый код действия 
                                    {
                                        uint b2 = (uint)(d1 << (3 * k));//сдвигаем кажый код шаблона (3 бит)
                                        //на 3k разрядов влево (код шаблона 0,1,3,4...n умножить на (2^3*k)
                                        //k=0=>2^0 = 1, код =0,1,2,3...
                                        //k=1=>2^3 = 8,код = 0,8,16,24,...
                                        //k=2=>2^6 = 16, код = 0,64,128,192...
                                        sum = sum | b2;
                                        //sum += b2;//суммируем - складываем полученные биты
                                    }
                                    //c = (byte)(b*Math.Pow( | c);

                                }
                                
                                uint res = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(c, sum);

                                cmdUpd.Parameters[0].Value = c;
                                cmdUpd.Parameters[1].Value = sum;
                                cmdUpd.Parameters[2].Value = sum;
                                //cmdUpd.ExecuteNonQuery();

                            }


                        }
                    }

                }


            }
            return;*/

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
