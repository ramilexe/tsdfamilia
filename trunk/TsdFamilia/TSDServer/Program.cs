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
