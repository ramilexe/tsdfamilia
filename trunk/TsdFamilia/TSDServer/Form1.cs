using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;



namespace TSDServer
{
    public partial class Form1 : Form
    {
        
        public System.Threading.Mutex mutex;

        OpenNETCF.Desktop.Communication.RAPI terminalRapi =
            new OpenNETCF.Desktop.Communication.RAPI();

        string[] status =
            new string[] { "Подключен", "Не подключен" };
        System.Globalization.DateTimeFormatInfo dateFormat =
                new System.Globalization.DateTimeFormatInfo();

        System.Globalization.NumberFormatInfo nfi =
                new System.Globalization.NumberFormatInfo();

        string[] cols = null;
        int[] colsLength = null;
        void SetFormats()
        {
            cols = new string[productsDataSet1.ProductsTbl.Columns.Count];
            dateFormat.ShortDatePattern = Properties.Settings.Default.ShortDatePattern;
            dateFormat.DateSeparator = Properties.Settings.Default.DateSeparator;
            nfi.NumberDecimalSeparator = Properties.Settings.Default.NumberDecimalSeparator;

            string [] colsLengthStr =
                Properties.Settings.Default.FieldsLength.Split(';');
            
            colsLength = new int[colsLengthStr.Length];

            for (int i = 0; i < colsLengthStr.Length; i++)
            {
                colsLength[i] = int.Parse(colsLengthStr[i]);
            }

        }

        public Form1()
        {
            InitializeComponent();
            SetFormats();

            mutex = new System.Threading.Mutex(false, "FAMILTSDSERVER");
            if (!mutex.WaitOne(0, false))
            {
                mutex.Close();
                mutex = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.productsDataSet1.CleanDatabase();

                    using (System.IO.StreamReader rdr =
                        new System.IO.StreamReader(openFileDialog1.FileName
                            , Encoding.GetEncoding("windows-1251")))
                    {

                        string s = string.Empty;
                        while ((s = rdr.ReadLine()) != null)
                        {
                            AddFixedString(s);
                        }
                    }


                    using (ProductsDataSetTableAdapters.ProductsTblTableAdapter tAdapter =
                        new TSDServer.ProductsDataSetTableAdapters.ProductsTblTableAdapter())
                    {

                        tAdapter.Update(this.productsDataSet1);
                    }
                    this.productsDataSet1.AcceptChanges();
                    MessageBox.Show("Загрузка завершена", "Статус загрузки", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Ошибка: "+err.Message, "Статус загрузки на сервер", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void AddFixedString(string s)
        {

            cols = new string[productsDataSet1.ProductsTbl.Columns.Count];

            ProductsDataSet.ProductsTblRow row =
                this.productsDataSet1.ProductsTbl.NewProductsTblRow();

            cols[0] = s.Substring(0, colsLength[0]);
            int readedLength = colsLength[0];


            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0")
            {
                this.richTextBox1.AppendText(string.Format("Штрихкод пустой - строка пропущена {0}\n",s));
                return;
            }
            row[0] = cols[0].Trim();
            
            for (int i = 1; i <  productsDataSet1.ProductsTbl.Columns.Count; i++)
            {
                try
                {
                    cols[i] = s.Substring(readedLength+1, colsLength[i]);
                    readedLength = readedLength + colsLength[i] + 1;
                    

                    if (productsDataSet1.ProductsTbl.Columns[i].DataType ==
                        typeof(string))
                    {
                        row[i] = cols[i].Trim();
                        continue;
                    }
                    if (!String.IsNullOrEmpty(cols[i].Trim()))
                    {
                        if (productsDataSet1.ProductsTbl.Columns[i].DataType ==
                            typeof(decimal))
                        {
                            row[i] = Decimal.Parse(cols[i].Trim(), nfi);
                            continue;
                        }

                        if (productsDataSet1.ProductsTbl.Columns[i].DataType ==
                            typeof(DateTime))
                        {
                            row[i] = DateTime.Parse(cols[i].Trim(), dateFormat);
                            continue;
                        }
                    }
                }
                catch (Exception err)
                {
                    this.richTextBox1.AppendText(string.Format("Ошибка в строке: {0}: {1}\n", s,err.Message));
                }
            }
            this.productsDataSet1.ProductsTbl.AddProductsTblRow(row);

        }


        private void AddDelimetedString(string s)
        {

            cols = s.Split('|');

            ProductsDataSet.ProductsTblRow row =
                this.productsDataSet1.ProductsTbl.NewProductsTblRow();

            //cols[0] = s.Substring(0, colsLength[0]);
            //int readedLength = colsLength[0];


            if (String.IsNullOrEmpty(cols[0].Trim()) ||
                cols[0].Trim() == "0" ||
                cols[0].Trim() == "9999999999999")
            {
                this.richTextBox1.AppendText(string.Format("Штрихкод пустой - строка пропущена {0}\n", s));
                return;
            }
            row[0] = cols[0].Trim();

            for (int i = 1; i < productsDataSet1.ProductsTbl.Columns.Count; i++)
            {
                try
                {
                    //cols[i] = s.Substring(readedLength + 1, colsLength[i]);
                    //readedLength = readedLength + colsLength[i] + 1;


                    if (productsDataSet1.ProductsTbl.Columns[i].DataType ==
                        typeof(string))
                    {
                        row[i] = cols[i].Trim();
                        continue;
                    }
                    if (!String.IsNullOrEmpty(cols[i].Trim()))
                    {
                        if (productsDataSet1.ProductsTbl.Columns[i].DataType ==
                            typeof(decimal))
                        {
                            row[i] = Decimal.Parse(cols[i].Trim(), nfi);
                            continue;
                        }

                        if (productsDataSet1.ProductsTbl.Columns[i].DataType ==
                            typeof(DateTime))
                        {
                            row[i] = DateTime.Parse(cols[i].Trim(), dateFormat);
                            continue;
                        }
                    }
                }
                catch (Exception err)
                {
                    this.richTextBox1.AppendText(string.Format("Ошибка в строке: {0}: {1}\n", s, err.Message));
                }
            }
            this.productsDataSet1.ProductsTbl.AddProductsTblRow(row);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                terminalRapi.Connect();
                if (terminalRapi.Connected)
                {
                    terminalRapi.CopyFileToDevice(Application.StartupPath+"\\products.sdf",
                        Properties.Settings.Default.TSDDBPAth + "products.sdf", true);
                    MessageBox.Show("Загрузка завершена", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    MessageBox.Show("Терминал не подключен. Проверьте подключение.", "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Ошибка загрузки на терминал: "+err.Message, "Статус загрузки на терминал", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (terminalRapi.Connected)
            {
                toolStripStatusLabel1.Text = status[0];
                toolStripStatusLabel1.Image =
                    Properties.Resources.OK;

            }
            else
            {
                toolStripStatusLabel1.Text = status[1];
                toolStripStatusLabel1.Image =
                    Properties.Resources.CriticalError;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }
    }
}
