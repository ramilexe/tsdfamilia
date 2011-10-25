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
        private System.Threading.ManualResetEvent _mevt =
            new System.Threading.ManualResetEvent(false);

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

            //ActionsClass.Action.BeginScan();
            //ActionsClass.Action.OnActionCompleted += new ActionsClass.ActionCompleted(Action_OnActionCompleted);
            ScanClass.Scaner.InitScan();
            //ScanClass.Scaner.OnScanned += new Scanned(Scanned);
            this.Refresh();
            ScanClass.Scaner.OnScanned += scannedDelegate;
            TSDServer.Scanned del = new Scanned(OnScanned);

            enableScan = true;
            if (
                //Program.СurrentInvId != string.Empty ||
               (Program.СurrentInvId = ActionsClass.Action.FindOpenInventar()) != string.Empty
               )
            {
                this.BeginInvoke(del, Program.СurrentInvId);
                //OnScanned(Program.СurrentInvId);
            }
            _mevt.Set();
            
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
                if (_mevt.WaitOne(5000, false) == false)
                    return;

                if (!enableScan)
                    return;
                
                this.label3.Visible = false;
                this.label4.Visible = false;
                this.label2.Visible = false;
                this.textBox1.Text = barcode;
                this.textBox1.SelectAll();
                if (barcode.StartsWith("660") && barcode.Length == 13)
                {
                    lastBk = barcode;

                    ScannedProductsDataSet.ScannedBarcodesRow [] row =
                    ActionsClass.Action.FindByDocIdAndDocType
                        (
                        barcode,
                        (byte)TSDUtils.ActionCode.CloseInventar);

                    if (row == null ||
                        row.Length==0 )
                    {
                        this.label3.Visible = true;
                        this.label4.Text = string.Format("№ {0}?",
                            barcode);
                        this.label4.Visible = true;
                        //lastBk = barcode;
                        //using (DialogForm frm =
                        //      new DialogForm(
                        DialogResult dr = DialogFrm.ShowMessage("Вы хотите начать просчет?",
                                  string.Format(" № {0}", barcode),
                                   "ДА – продолжить. Нет – выйти",
                                   "Новый просчет");//)
                        {
                            if (dr == DialogResult.Yes)
                            {
                                try
                                {
                                    ActionsClass.Action.OpenInv(
                                         barcode,
                                         TSDUtils.ActionCode.InventoryGlobal
                                        );

                                    enableScan = false;
                                    //ScanClass.Scaner.StopScan();
                                    ScanClass.Scaner.OnScanned -= scannedDelegate;
                                    //ScanClass.Scaner.OnScanned =null;
                                    //this.Visible = false;
                                    //this.Enabled = false;
                                    ScanClass.Scaner.StopScan();
                                    using (ViewProductForm prodfrm =
                                        new ViewProductForm(
                                            WorkMode.InventarScan,
                                            barcode))
                                    {
                                        prodfrm.ShowDialog();
                                    }
                                }
                                catch (Exception err)
                                {
                                    //using (DialogForm frmErr =
                                    //    new DialogForm(
                                    DialogResult dr1 = DialogFrm.ShowMessage("Ошибка инв-ции",
                                            err.Message,
                                            "ДА – продолжить. Нет – выйти",
                                             "Ошибка");//)
                                    {
                                        if (dr1 == DialogResult.No)
                                            return;
                                    }
                                }
                                finally
                                {
                                    ScanClass.Scaner.OnScanned += scannedDelegate;
                                    ScanClass.Scaner.InitScan();
                                    enableScan = true;
                                    //this.Visible = true;
                                    //this.Enabled = true;
                                    //ScanClass.Scaner.InitScan();
                                    //ScanClass.Scaner.OnScanned += new Scanned(OnScanned);
                                }

                                //this.Close();
                            }
                            else
                                this.Close();

                        }
                        this.Refresh();
                        
                        


                        enableInvent = true;
                    }
                    else
                    {
                        if (ActionsClass.Action.CheckInv(barcode))
                        {
                            //using (DialogForm frm =
                                //new DialogForm(
                            DialogResult dr = DialogFrm.ShowMessage(
                                    string.Format("Идет просчет № {0}", barcode),
                                    "Продолжить просчет?",
                                     "ДА – продолжить. Нет – выйти",
                                     "Просчет уже существует!");//)
                            {
                                
                                if (dr == DialogResult.Yes)
                                {
                                    enableScan = false;
                                    enableInvent = true;
                                    try
                                    {
                                        ScanClass.Scaner.OnScanned -= scannedDelegate;
                                        ScanClass.Scaner.StopScan();
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
                                        
                                        ScanClass.Scaner.InitScan();
                                        ScanClass.Scaner.OnScanned += scannedDelegate;
                                        enableScan = true;
                                    }
                                    

                                }
                                else
                                    this.Close();
                                //this.Refresh();
                            }
                            

                            
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
                e.Handled = true;
                return;
            }
            if (e.KeyValue == (int)SpecialButton.BlueBtn)
            {
                e.Handled = true;
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