using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class InventarForm : Form
    {
        string lastBk = string.Empty;
        private bool enableInvent = false;
        private bool enableScan = false;
        Scanned scannedDelegate = null;
        public InventarForm()
        {
            InitializeComponent();

            scannedDelegate = new Scanned(OnScanned);
        }


        private void InventarForm_Load(object sender, EventArgs e)
        {
            this.Width = 235;
            this.Height = 295;
            textBox1.Focus();

            //string invId = ActionsClass.Action.FindOpenInventar();

            ActionsClass.Action.BeginScan();
            //ActionsClass.Action.OnActionCompleted += new ActionsClass.ActionCompleted(Action_OnActionCompleted);
            ScanClass.Scaner.InitScan();
            //ScanClass.Scaner.OnScanned += new Scanned(Scanned);
            this.Refresh();
            ScanClass.Scaner.OnScanned += scannedDelegate;
            enableScan = true;
            if (
                Program.СurrentInvId != string.Empty ||
               (Program.СurrentInvId = ActionsClass.Action.FindOpenInventar()) != string.Empty
               )
            {
                OnScanned(Program.СurrentInvId);
            }
            
        }
        void InventarForm_Closed(object sender, System.EventArgs e)
        {
            ScanClass.Scaner.OnScanned -= scannedDelegate; ;
            ActionsClass.Action.EndScan();
            ScanClass.Scaner.StopScan();
            enableScan = false;
        }

        void OnScanned(string barcode)
        {
            if (this.InvokeRequired )
            {
                TSDServer.Scanned del = new Scanned(OnScanned);
                this.Invoke(del, barcode);
            }
            else
            {
                if (!enableScan)
                    return;
                
                this.label3.Visible = false;
                this.label4.Visible = false;
                this.label2.Visible = false;
                this.textBox1.Text = barcode;
                this.textBox1.SelectAll();
                if (barcode.StartsWith("3") && barcode.Length == 13)
                {
                    lastBk = barcode;

                    ScannedProductsDataSet.ScannedBarcodesRow [] row =
                    ActionsClass.Action.ScannedProducts.
                        ScannedBarcodes.FindByDocIdAndDocType
                        (
                        barcode,
                        (byte)TSDUtils.ActionCode.InventoryGlobal);

                    if (row == null ||
                        row.Length==0)
                    {
                        this.label3.Visible = true;
                        this.label4.Text = string.Format("№ {0}?",
                            barcode);
                        this.label4.Visible = true;
                        //lastBk = barcode;
                        using (DialogForm frm =
                              new DialogForm("Вы хотите начать просчет?",
                                  string.Format(" № {0}", barcode),
                                   "ДА – продолжить. Нет – выйти",
                                   "Новый просчет"))
                        {
                            if (frm.ShowDialog() == DialogResult.Yes)
                            {
                                try
                                {
                                    enableScan = false;
                                    //ScanClass.Scaner.StopScan();
                                    //ScanClass.Scaner.OnScanned -= scannedDelegate;
                                    //ScanClass.Scaner.OnScanned =null;
                                    //this.Visible = false;
                                    //this.Enabled = false;
                                    using (ViewProductForm prodfrm =
                                        new ViewProductForm(
                                            WorkMode.InventarScan,
                                            barcode))
                                    {
                                        prodfrm.ShowDialog();
                                    }
                                }
                                finally
                                {
                                    enableScan = true;
                                    //this.Visible = true;
                                    //this.Enabled = true;
                                    //ScanClass.Scaner.InitScan();
                                    //ScanClass.Scaner.OnScanned += new Scanned(OnScanned);
                                }
                            
                                //this.Close();
                            }

                        }
                        this.Refresh();
                        
                        


                        enableInvent = true;
                    }
                    else
                    {
                        if (row[0].Priority != byte.MaxValue)
                        {
                            using (DialogForm frm =
                                new DialogForm(
                                    string.Format("Идет просчет № {0}", barcode),
                                    "Продолжить просчет?",
                                     "ДА – продолжить. Нет – выйти",
                                     "Просчет уже существует!"))
                            {
                                
                                if (frm.ShowDialog() == DialogResult.Yes)
                                {
                                    enableScan = false;
                                    enableInvent = true;
                                    try
                                    {
                                        using (ViewProductForm prodfrm =
                                            new ViewProductForm(
                                                WorkMode.InventarScan,
                                                barcode))
                                        {
                                            prodfrm.ShowDialog();
                                        }
                                    }
                                    finally
                                    {
                                        enableScan = true;
                                    }
                                    

                                }
                                //this.Refresh();
                            }
                            this.Close();

                            
                        }
                        else
                        {
                            label2.Text = string.Format("Просчет {0} завершен!",barcode);
                            label2.Visible = true;
                            enableInvent = false;
                            //this.Refresh();
                        }

                    }

                }
                else
                {
                    ActionsClass.Action.InvokeAction(TSDUtils.ActionCode.NotFound, null, null);
                    enableInvent = false;
                    label2.Text = string.Format("Штрихкод {0} неверный адрес!", barcode);
                    label2.Visible = true;
                }
                
                this.Refresh();
                //tmr.Change(0, 200);
                //if (docsForm != null)
                //{
                //    docsForm.Close();
                //    docsForm.Dispose();
                //    docsForm = null;
                //}
                
            }
        }

       

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //this.textBox1.Text = e.KeyValue.ToString();
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
                if (!String.IsNullOrEmpty(lastBk))
                {
                    
                    try
                    {
                        enableScan = false;
                        //ScanClass.Scaner.OnScanned = null;
                        using (ViewInventarForm prod =
                            new ViewInventarForm(lastBk,
                                (byte)TSDUtils.ActionCode.InventoryGlobal))
                        {
                            prod.ShowDialog();
                        }
                    }
                    finally
                    {
                        enableScan = true;
                        //ScanClass.Scaner.OnScanned += scannedDelegate;
                    }
                }

               return;
            }
            if (e.KeyCode == Keys.Tab)
            {
                if (enableInvent)
                {
                    OnScanned(lastBk);
                }
                return;
            }
        }


    }
}