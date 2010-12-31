using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class SettingsForm : Form
    {
        string[] columnsWidth;
        List<String> columnsWidthList = new List<string>();
        List<NumericUpDown> nADArray = new List<NumericUpDown>();
        
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            columnsWidthList.Add("0");
            int length = 0;
            for (int i = 0; i < nADArray.Count; i++)
            {
                /*NumericUpDown nUD = new NumericUpDown();
                nUD.Name = "numericUpDown" + i;
                nUD.Size = new System.Drawing.Size(40, 20);
                nUD.Location = new System.Drawing.Point(0, 8 + length);
                length += nUD.Width;
                this.groupBox2.Controls.Add(nUD);*/
                length += nADArray[i].Width;
            }
            NumericUpDown nUD = new NumericUpDown();
            nUD.Name = "numericUpDown" + nADArray.Count;
            nUD.Size = new System.Drawing.Size(40, 20);
            nUD.Location = new System.Drawing.Point(length, 8);
            length += nUD.Width;
            this.groupBox2.Controls.Add(nUD);
            nADArray.Add(nUD);
            this.Update();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            columnsWidth = Properties.Settings.Default.FieldsLength.Split(';');
            columnsWidthList.AddRange(columnsWidth);
            

            int length = 0;
            for (int i = 0; i < columnsWidth.Length; i++)
            {
                NumericUpDown nUD = new NumericUpDown();
                nUD.Name = "numericUpDown" + i;
                nUD.Size = new System.Drawing.Size(40, 20);
                nUD.Location = new System.Drawing.Point(length, 8);
                length += nUD.Width;
                this.groupBox2.Controls.Add(nUD);
                nUD.Value = int.Parse(columnsWidth[i]);
                nADArray.Add(nUD);
            }
            if (Properties.Settings.Default.ImportFileTypeIsFixed)
            {
                fixedRB.Checked = true;
                delimetedRB.Checked = false;
                groupBox2.Enabled = true;
            }
            else
            {
                fixedRB.Checked = false;
                delimetedRB.Checked = true;
                groupBox2.Enabled = false;
            }
            fieldSeparatorTB.Text = Properties.Settings.Default.FieldDelimeter;
            dbPathTB.Text = Properties.Settings.Default.LocalFilePath;//Properties.Settings.Default.ProductsConnectionString.Replace("Data Source=", "");
            this.folderBrowserDialog1.SelectedPath = Properties.Settings.Default.LocalFilePath;//Properties.Settings.Default.ProductsConnectionString.Replace("Data Source=", "");
            terminalPathTB.Text = Properties.Settings.Default.TSDDBPAth;
            this.folderBrowserDialog1.SelectedPath = Properties.Settings.Default.TSDDBPAth;

            for (int i = 0; i < decimalSeparatorCB.Items.Count; i++)
            {
                if (decimalSeparatorCB.Items[i].ToString() ==
                    Properties.Settings.Default.NumberDecimalSeparator)
                {
                    decimalSeparatorCB.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < dateSeparatorCB.Items.Count; i++)
            {
                if (dateSeparatorCB.Items[i].ToString() ==
                    Properties.Settings.Default.DateSeparator)
                {
                    dateSeparatorCB.SelectedIndex = i;
                    break;
                }
            }
            dateFormatTB.Text = Properties.Settings.Default.ShortDatePattern;


        }

        private void button4_Click(object sender, EventArgs e)
        {
            NumericUpDown nAD =nADArray[nADArray.Count - 1];
            this.groupBox2.Controls.Remove(nAD);
            nADArray.RemoveAt(nADArray.Count - 1);
            this.Update();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (fixedRB.Checked)
            {
                //fixedRB.Checked = true;
                delimetedRB.Checked = false;
                groupBox2.Enabled = true;
            }
            else
            {
                //fixedRB.Checked = false;
                delimetedRB.Checked = true;
                groupBox2.Enabled = false;
            }
        }

        private void fixedRB_CheckedChanged(object sender, EventArgs e)
        {
            if (fixedRB.Checked)
            {
                //fixedRB.Checked = true;
                delimetedRB.Checked = false;
                groupBox2.Enabled = true;
            }
            else
            {
                //fixedRB.Checked = false;
                delimetedRB.Checked = true;
                groupBox2.Enabled = false;
            }
        }

        private void openTerminalBtn_Click(object sender, EventArgs e)
        {
            //OpenNETCF.Desktop.Communication.OpenDeviceFileDialog dlg
            //     = new OpenNETCF.Desktop.Communication.OpenDeviceFileDialog();
            //DialogResult dlgResult = dlg.ShowDialog();
            //if (dlgResult == DialogResult.OK)
            //{
            //    //terminalPathTB.Text = dlg.
            //}
        }

        private void openDatabseBtn_Click(object sender, EventArgs e)
        {

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                dbPathTB.Text = folderBrowserDialog1.SelectedPath;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.FieldDelimeter = fieldSeparatorTB.Text;
            //Properties.Settings.Default.ProductsConnectionString = "Data Source=" + dbPathTB.Text;
            Properties.Settings.Default.TSDDBPAth = terminalPathTB.Text;
            Properties.Settings.Default.LocalFilePath = dbPathTB.Text;
            Properties.Settings.Default.NumberDecimalSeparator = decimalSeparatorCB.SelectedItem.ToString();
            Properties.Settings.Default.DateSeparator = dateSeparatorCB.SelectedItem.ToString();
            Properties.Settings.Default.ShortDatePattern = dateFormatTB.Text;
            string[] s = new string[nADArray.Count];
            for (int i = 0; i < nADArray.Count; i++)
            {
                s[i] = ((int)nADArray[i].Value).ToString();
            }
            Properties.Settings.Default.FieldsLength = String.Join(";", s);
            Properties.Settings.Default.ImportFileTypeIsFixed = fixedRB.Checked;
            
            
            Properties.Settings.Default.Save();
            //Properties.Settings.Default.Upgrade();
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
