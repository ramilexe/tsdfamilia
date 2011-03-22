using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Security.Permissions;
using System.IO;
using log4net;
using log4net.Config;


namespace SendMailAttach
{
    [FileIOPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
    public class SendMailClass
    {
        //public static System.Threading.Mutex mut = new System.Threading.Mutex();
        public static System.Threading.ManualResetEvent mEvt = new System.Threading.ManualResetEvent(true);
        public static readonly ILog log = LogManager.GetLogger(typeof(SendMailClass));
        private string _From;
        private string _nameFrom;
        bool _sendCopyToSender = false;
        string _smtpAdress;
        /// <summary>
        /// Конструктор должен содержать следущие поля
        /// </summary>
        /// <param name="from">отправитель по умолчанию</param>
        /// <param name="nameFrom">Понятное название</param>
        /// <param name="sendCopyToSender">True если посылать копию отправителю</param>
        /// <param name="smtpAdress">"адрес почтового сервера"</param>
        public SendMailClass(string from, string nameFrom, bool sendCopyToSender, string smtpAdress)
        {
            _From = from;
            _nameFrom = nameFrom;
            _sendCopyToSender = sendCopyToSender;
            _smtpAdress = smtpAdress;
        }
        private void Log(string text)
        {
            try
            {
                string s = string.Format("Process: {0} Info: {1}", System.Diagnostics.Process.GetCurrentProcess().Id.ToString(), text);
                log.Info(s);
                Console.WriteLine(s);

                using (System.IO.StreamWriter wr = new System.IO.StreamWriter(
                           Path.Combine(TSDServer.Properties.Settings.Default.LocalFilePath,"Log.txt"), true))
                {
                    wr.WriteLine(s);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                Console.WriteLine(err.Message);
                Console.WriteLine(err.Source);
                Console.WriteLine(err.StackTrace);
                Console.WriteLine(err.InnerException.ToString());
            }
        }
        public void SendMail(string Subject ,string Body, string To , string Attachment_FileName)
        {
            //SendMail(new string[]{Subject,Body,To,Attachment_FileName});

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                if (_sendCopyToSender)
                    message.To.Add(new MailAddress(_From, _nameFrom));

                message.From = new MailAddress(_From, _nameFrom);
                message.IsBodyHtml = false;

                message.BodyEncoding = Encoding.GetEncoding("KOI8-R");
                //message.AlternateViews.Add(new AlternateView(


                message.Subject = Subject;
                message.Bcc.Add(To);

                SmtpClient client = new SmtpClient(_smtpAdress);
                client.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;


                message.Body = Body;
                
                System.Net.Mime.ContentType cType = new System.Net.Mime.ContentType("application/vnd.ms-excel");


                if (!String.IsNullOrEmpty(Attachment_FileName))
                {

                    cType.Name = System.IO.Path.GetFileName(Attachment_FileName.Trim());
                    Attachment data = new Attachment(Attachment_FileName.Trim(), cType);
                    System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
                    disposition.FileName = cType.Name;
                    disposition.DispositionType = System.Net.Mime.DispositionTypeNames.Attachment;


                    message.Attachments.Add(data);

                }

                try
                {
                    client.Send(message);
                    Log("Mail succesully sended to " + To);
                }
                catch (Exception err)
                {
                    Log(String.Format("err={0},Subject={1},Body = {2}, To={3}, Attach={4}"
                        ,err.ToString(),Subject,Body,To,Attachment_FileName));
                    throw;
                }
            
        }

        public void SendMail(string[] args)
        {
            int cnt = -1;
            System.IO.FileInfo fi = new System.IO.FileInfo(Path.Combine(TSDServer.Properties.Settings.Default.LocalFilePath,"log4netconfig.xml"));
            XmlConfigurator.Configure(fi);


            Log("START process");
            if (mEvt.WaitOne(5000, false) == false)
            {

            }
            mEvt.Reset();

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: | SendMailAttach | Subject | Body | To | Attachment_FileName ");
                    return;
                }

                //using (System.IO.StreamWriter wrt =
                //    new System.IO.StreamWriter("c:\\sendmail.log", true))
                //{
                try
                {
                    string newTempStr = String.Join(" ", args);
                    //string[] newArgs = newTempStr.Split('|');
                    Log(newTempStr);
                    string[] adress = args[3].Split(';');
                    Log(adress[0].Trim());
                    if (!string.IsNullOrEmpty(adress[0].Trim()))
                    {
                        System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                        if (_sendCopyToSender)
                            message.To.Add(new MailAddress(_From, _nameFrom));
                        
                        message.From = new MailAddress(_From, _nameFrom);
                        message.IsBodyHtml = false;

                        message.BodyEncoding = Encoding.GetEncoding("KOI8-R");
                        //message.AlternateViews.Add(new AlternateView(

                        Log(args[1].Replace("\r\n", " ").Trim());
                        message.Subject = args[1].Replace("\r\n", " ").Trim();

                        for (int i = 0; i < adress.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(adress[i].Trim()))
                            {
                                Log(adress[i].Trim());
                                message.Bcc.Add(adress[i].Trim());
                                //message.Bcc.Add("abanaev@enkatc.ru");
                                //message.Bcc.Add("ftuna@enkatc.ru");

                            }
                        }
                        //SmtpClient client = new SmtpClient("exchange.ramenka.ru");
                        SmtpClient client = new SmtpClient(_smtpAdress);
                        client.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;


                        message.Body = args[2].Trim();
                        Log(args[2].Trim());


                        if (args.Length >= 5 && (!String.IsNullOrEmpty(args[4].Trim())))
                        {
                            Log(args[4].Trim());
                            System.Net.Mime.ContentType cType = new System.Net.Mime.ContentType("application/vnd.ms-excel");
                            cType.Name = System.IO.Path.GetFileName(args[4].Trim());
                            Attachment data = new Attachment(args[4].Trim(), cType);
                            System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
                            disposition.FileName = cType.Name;
                            disposition.DispositionType = System.Net.Mime.DispositionTypeNames.Attachment;


                            message.Attachments.Add(data);

                        }

                        try
                        {
                            client.Send(message);
                            Log("Mail succesully sended to " + args[3]);
                        }
                        catch (Exception err)
                        {
                            Log(err.ToString());

                        }
                    }
                }
                catch (Exception err)
                {
                    Log(err.ToString());

                }
                finally
                {

                    //wrt.Flush();
                    //wrt.Close();
                }
                // }

            }
            catch { }

            finally
            {
                mEvt.Set();
                //   mut.ReleaseMutex();
                Log("END process");
            }

        }
    }

}

