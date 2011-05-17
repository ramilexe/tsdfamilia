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
        public InventarForm()
        {
            InitializeComponent();
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
            ScanClass.Scaner.OnScanned += new Scanned(Scanned);
            this.Refresh();


            if (
                Program.СurrentInvId != string.Empty ||
               (Program.СurrentInvId = ActionsClass.Action.FindOpenInventar()) != string.Empty
               )
            {
                Scanned(Program.СurrentInvId);
            }
            
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
                this.label3.Visible = false;
                this.label4.Visible = false;
                this.label2.Visible = false;
                this.textBox1.Text = barcode;
                if (barcode.StartsWith("3"))
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
                                using (ViewProductForm prodfrm =
                                    new ViewProductForm(
                                        WorkMode.InventarScan,
                                        barcode))
                                {
                                    prodfrm.ShowDialog();
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
                                    enableInvent = true;
                                    using (ViewProductForm prodfrm =
                                        new ViewProductForm(
                                            WorkMode.InventarScan,
                                            barcode))
                                    {
                                        prodfrm.ShowDialog();
                                    }
                                    

                                }
                                this.Refresh();
                            }
                            this.Close();

                            
                        }
                        else
                        {
                            label2.Text = string.Format("Просчет {0} завершен!",barcode);
                            label2.Visible = true;
                            enableInvent = false;
                            this.Refresh();
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
                if (!String.IsNullOrEmpty(lastBk))
                {
                    using (ViewInventarForm prod =
                        new ViewInventarForm(lastBk,
                            (byte)TSDUtils.ActionCode.InventoryGlobal))
                    {
                        prod.ShowDialog();
                    }
                }

               return;
            }
            if (e.KeyCode == Keys.Tab)
            {
                if (enableInvent)
                {
                    Scanned(lastBk);
                }
                return;
            }
        }


    }
}