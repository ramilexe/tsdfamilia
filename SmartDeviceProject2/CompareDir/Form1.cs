﻿using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CompareDir
{
    public partial class Form1 : Form
    {
        List<String> fileDiffList =
            new List<String>();
        string[] sourcefiles = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 235;
            this.Height = 295;
            textBox1.Focus();
            this.Text = "Сравнение каталогов";

            if (System.IO.Directory.Exists(Program.Default.Catalog1))
                sourcefiles =
                    System.IO.Directory.GetFiles(Program.Default.Catalog1);
            else
            {
                this.textBox1.Text = "Каталог не найден: "+Program.Default.Catalog1;
                return;
            }

            foreach (string file1 in sourcefiles)
            {
                //DateTime dt1 = System.IO.File.GetCreationTime(file1);
                System.IO.FileInfo fi1 = new System.IO.FileInfo(file1);
                long fSize1 = fi1.Length;

                string fileName = System.IO.Path.GetFileName(file1);
                string file2 = System.IO.Path.Combine(Program.Default.Catalog2, fileName);

                if (System.IO.File.Exists(file2))
                {
                    System.IO.FileInfo fi2 = new System.IO.FileInfo(file2);
                    long fSize2 = fi2.Length;
                    //DateTime dt2 = System.IO.File.GetCreationTime(file2);
                    if (fSize1 != fSize2)
                    {
                        fileDiffList.Add(fileName);
                    }
                }
                else
                    fileDiffList.Add(fileName);
            }

            if (fileDiffList.Count > 0)
                this.textBox1.Text = "Есть новая версия. Обновиться?";
            else
                this.textBox1.Text = "У вас последняя версия программы";



        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    return;
                }

                if (e.KeyCode == Keys.Enter)
                {
                    if (System.IO.Directory.Exists(Program.Default.Catalog2))
                    {
                        if (sourcefiles != null &&
                            sourcefiles.Length > 0 &&
                            fileDiffList.Count > 0)
                        {
                            textBox1.Text = "Копирование...";
                            this.Refresh();
                            foreach (string file1 in sourcefiles)
                            {
                                string fileName = System.IO.Path.GetFileName(file1);
                                string file2 = System.IO.Path.Combine(Program.Default.Catalog2, fileName);

                                try
                                {
                                    System.IO.File.Copy(file1, file2, true);
                                }
                                catch (Exception err)
                                {
                                    textBox2.Visible = true;
                                    textBox2.Enabled = true;
                                    textBox2.Text = textBox2.Text + "\r\n" + err.ToString();
                                }
                            }
                            textBox1.Focus();
                            //this.Close();
                            //e.Handled = true;
                            return;
                        }
                    }
                    else
                    {
                        textBox1.Text = "Каталог не найден: " + Program.Default.Catalog2;
                    }
                }
            }
            finally
            {
                System.Diagnostics.Process p =
                    new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(
                    Program.Default.ExeProgramPath, "");
                p.Start();
                
                System.Threading.Thread.Sleep(2000);

                this.Close();
                e.Handled = true;
                
            }
        }
    }
}