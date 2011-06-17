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
        //private bool enableInvent = false;
        Scanned scannedDelegate = null;
        public IncomeForm()
        {
            InitializeComponent();

            scannedDelegate = new Scanned(OnScanned);
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
            ScanClass.Scaner.OnScanned += scannedDelegate;
            
            this.Refresh();


            
            
        }
        void InventarForm_Closed(object sender, System.EventArgs e)
        {
            //BTPrintClass.PrintClass.SetStatusEvent("Begin closing");
            ScanClass.Scaner.OnScanned -= scannedDelegate;
            ActionsClass.Action.EndScan();
            ScanClass.Scaner.StopScan();
            //BTPrintClass.PrintClass.SetStatusEvent("End closing");
        }

        void OnScanned(string barcode)
        {
            if (this.InvokeRequired)
            {
                TSDServer.Scanned del = new Scanned(OnScanned);
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
                this.textBox1.SelectAll();

                if (barcode.StartsWith("3") && barcode.Length == 13)
                {
                    ProductsDataSet.DocsTblRow[] rows =
                            ActionsClass.Action.GetDataByDocIdAndType(barcode,
                            (byte)TSDUtils.ActionCode.IncomeBox);


                    if (rows != null && rows.Length > 0)
                    {
                        ProductsDataSet.DocsTblRow row = rows[0];

                        this.bkLabel.Visible = true;
                        this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                        this.docLabel.Visible = true;
                        this.docLabel.Text = string.Format
                            ("Накладная № {0}", row.Text2);

                        this.txtLabel.Visible = true;

                        this.dateLabel.Visible = true;
                        this.dateLabel.Text = row.Text3;
                        //string.Format(
                        //"Дата: {0}", DateTime.Today.ToString("dd.MM.yyyy"));

                        ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                               ActionsClass.Action.AddScannedRow(
                               long.Parse(barcode),
                               (byte)TSDUtils.ActionCode.IncomeBox,
                               row.DocId,
                               row.Quantity,
                               row.Priority);
                        scannedRow.FactQuantity += 1;

                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.IncomeBox);
                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.IncomeBox);

                    }
                    else
                    {
                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        this.errLabel.Text = string.Format("ШК {0} чужой короб!",
                            barcode);

                        this.errLabel.Visible = true;
                    }
                    this.Refresh();
                }
                else
                {
                    ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                    ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                     this.errLabel.Text = string.Format("Это не ШК короба!",
                            barcode);

                     this.errLabel.Visible = true;
                    
                }
               // this.textBox1.SelectAll();
            }
            
            
        }

       

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //BTPrintClass.PrintClass.SetStatusEvent("KeyDown pressed");
            //this.textBox1.Text = e.KeyValue.ToString();
            //this.textBox1.Text = e.KeyCode.ToString();
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text != string.Empty)
                    OnScanned(textBox1.Text);
  
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
                ActionsClass.Action.CalculateTotalsWOPriority(
                    TSDUtils.ActionCode.IncomeBox,
                    DateTime.Today,
                    out totalBk,
                    out totals);
                try
                {
                    ScanClass.Scaner.OnScanned -= scannedDelegate;
                    using (DialogForm dlgfrm =
                                new DialogForm(
                                    string.Format("За {0} отсканировано",
                                    DateTime.Today.ToString("dd.MM.yyyy"))
                                    , string.Format(" {0} правильных коробов", totalBk)
                                    , ""
                                    , "Подсчет коробов"))
                    {
                        dlgfrm.ShowDialog();
                    }
                }
                finally
                {
                    ScanClass.Scaner.OnScanned += scannedDelegate;

                }
                this.Refresh();

                return;
            }
            if (e.KeyValue == (int)SpecialButton.GreenBtn) //GreenBtn
            {
                
                return;
            }
             
        }
            

    }
}