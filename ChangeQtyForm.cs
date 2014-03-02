using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{

    public partial class ChangeQtyForm : Form
    {
        public string ProductName
        {
            get
            {
                return this.label2.Text.Replace("Название:","").Trim();
            }
            set
            {
                this.label2.Text = string.Format("Название: {0}",value);
            }
        }
        public string Barcode
        {
            get
            {
                return this.label1.Text.Replace("Штрихкод:","").Trim();
            }
            set
            {
                this.label1.Text = string.Format("Штрихкод: {0}",value);
            }
        }

        public int NewQuantity
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

        public int FactQuantity
        {
            get
            {
                return Int32.Parse(this.label6.Text.Replace("Кол-во Факт:","").Trim());
            }
            set
            {
                this.label6.Text = string.Format("Кол-во Факт: {0}",value.ToString());
            }
        }
        public int PlanQuantity
        {
            get
            {
                return Int32.Parse(this.label5.Text.Replace("Кол-во план:","").Trim());
            }
            set
            {
                this.label5.Text = string.Format("Кол-во план: {0}",value.ToString());
            }
        }

        public ChangeQtyForm()
        {
            InitializeComponent();
        }

        private void MultiplyForm_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = FactQuantity.ToString();
            this.ClientSize = new System.Drawing.Size(225, 164); 
            this.Location = new Point(5, 65);
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
                        if (outq < 0 || outq > 1000)
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

    public class FChangeQtyForm
    {
        private FChangeQtyForm()
        {

        }
        private static ChangeQtyForm _mForm = null;

        public static ChangeQtyForm MForm
        {
            get
            {
                if (_mForm == null)
                    _mForm = new ChangeQtyForm();
                return _mForm;
            }
        }
    }
}