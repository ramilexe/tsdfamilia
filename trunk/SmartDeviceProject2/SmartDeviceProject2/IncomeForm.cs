using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class IncomeForm : Form
    {
        private bool enableInvent = false;
        public IncomeForm()
        {
            InitializeComponent();
        }


        private void InventarForm_Load(object sender, EventArgs e)
        {
            this.Width = 235;
            this.Height = 295;
            textBox1.Focus();

           // string invId = ActionsClass.Action.FindOpenInventar();

            ActionsClass.Action.BeginScan();
            //ActionsClass.Action.OnActionCompleted += new ActionsClass.ActionCompleted(Action_OnActionCompleted);
            ScanClass.Scaner.InitScan();
            ScanClass.Scaner.OnScanned += new Scanned(Scanned);
            
            this.Refresh();


            
            
        }
        void InventarForm_Closed(object sender, System.EventArgs e)
        {
            ActionsClass.Action.EndScan();
            ScanClass.Scaner.StopScan();
        }

        void Scanned(string barcode)
        {
            if (this.InvokeRequired)
            {
                TSDServer.Scanned del = new Scanned(Scanned);
                this.Invoke(del, barcode);
            }
            else
            {
                this.bkLabel.Visible = false;
                this.docLabel.Visible = false;
                this.errLabel.Visible = false;
                this.txtLabel.Visible = false;
                this.dateLabel.Visible = false;
                this.textBox1.Text = barcode;
                
                if (barcode.StartsWith("3"))
                {
                    ProductsDataSet.ProductsTblRow row =
                        ActionsClass.Action.GetProductRow(barcode);

                    if (row != null)
                    {
                        this.bkLabel.Visible = true;
                        this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                        this.docLabel.Visible = true;
                        this.docLabel.Text = string.Format
                            ("Накладная № {0}", row.Article);

                        this.txtLabel.Visible = true;

                        this.dateLabel.Visible = true;
                        this.dateLabel.Text = row.Message;
                            //string.Format(
                            //"Дата: {0}", DateTime.Today.ToString("dd.MM.yyyy"));

                        ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                               ActionsClass.Action.AddScannedRow(
                               row.Barcode,
                               (byte)TSDUtils.ActionCode.IncomeBox,
                               row.Article,
                               0,
                               0);

                    }
                    else
                    {
                        this.errLabel.Text = string.Format("ШК {0} чужой короб!",
                            barcode);

                        this.errLabel.Visible = true;
                    }


                }
                else
                {
                    this.errLabel.Text = string.Format("ШК {0} неверный № короба!",
                            barcode);

                    this.errLabel.Visible = true;
                }
                
            }
        }

       

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //this.textBox1.Text = e.KeyValue.ToString();
            //this.textBox1.Text = e.KeyCode.ToString();
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text != string.Empty)
                    Scanned(textBox1.Text);
  
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                return;
            }
            if (e.KeyValue == (int)SpecialButton.RedBtn)
            {

                return;
            }
            if (e.KeyValue == (int)SpecialButton.BlueBtn)
            {

                return;
            }
            if (e.KeyValue == (int)SpecialButton.YellowBtn)
            {
                int totals=0;
                int totalBk=0;
                ActionsClass.Action.CalculateTotals(
                    TSDUtils.ActionCode.IncomeBox,
                    out totalBk,
                    out totals);

                DialogForm dlgfrm =
                            new DialogForm(
                                string.Format("За {0} отсканировано", 
                                DateTime.Today.ToString("dd.MM.yyyy"))
                                , string.Format(" {0} правильных коробов", totalBk)
                                , ""
                                , "Подсчет коробов");

                return;
            }
            if (e.KeyValue == (int)SpecialButton.GreenBtn) //GreenBtn
            {
                
                return;
            }
             
        }
            

    }
}