using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{

    public partial class MultiplyForm : Form
    {
        /*public string ProductName
        {
            get
            {
                return this.label1.Text;
            }
            set
            {
                this.label1.Text = value;
            }
        }
        public string Barcode
        {
            get
            {
                return this.label2.Text;
            }
            set
            {
                this.label2.Text = value;
            }
        }*/
        public int Quantity
        {
            get
            {
                return Int32.Parse(this.textBox1.Text);
            }
            set
            {
                this.textBox1.Text = value.ToString();
            }
        }

        public MultiplyForm()
        {
            InitializeComponent();
        }

        private void MultiplyForm_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = "1";
            this.ClientSize = new System.Drawing.Size(224, 70);
            this.Location = new Point(5, 110);
            this.textBox1.SelectAll();
        }

        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                //this.Close();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                int outq=0;
                try
                {
                    try
                    {
                        outq = int.Parse(this.textBox1.Text);
                        if (outq <= 0 || outq > 1000)
                        {
                            throw new OverflowException("Кол-во вне диапазона");
                        }
                    }
                    catch (OverflowException ovExc)
                    {
                        this.textBox1.Text = "Кол-во вне диапазона";
                        throw;
                    }
                    catch (ArgumentException)
                    {
                        this.textBox1.Text = "Необходимо ввести кол-во";
                        throw;
                    }
                    catch (FormatException)
                    {
                        this.textBox1.Text = "Неверный формат";
                        throw;
                    }
                }
                catch
                {
                    this.textBox1.SelectAll();
                    e.Handled = true;
                    return;
                }

                this.DialogResult = DialogResult.OK;
                //this.Close();
                e.Handled = true;
                return;
            }
        }
    }

    public class FMultiplyForm
    {
        private FMultiplyForm()
        {

        }
        private static MultiplyForm _mForm = null;

        public static MultiplyForm MForm
        {
            get
            {
                if (_mForm == null)
                    _mForm = new MultiplyForm();
                return _mForm;
            }
        }
    }
}