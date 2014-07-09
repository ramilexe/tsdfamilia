using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ReturnForm : Form
    {
        public event EndLoadDelegate onEndLoad;

        private bool enableScan = false;
        private bool closedCar = false;
        //private bool enableInvent = false;
        Scanned scannedDelegate = null;
        Dictionary<string, string> docsList = new Dictionary<string, string>();

        ///// <summary>
        ///// загрузить список всех накладных машин
        ///// </summary>
        //System.Collections.Generic.List<ProductsDataSet.DocsTblRow> IncomerowsList =
        //                    new List<ProductsDataSet.DocsTblRow>();
        ///// <summary>
        ///// Загрузить список всех коробов машины
        ///// </summary>
        //System.Collections.Generic.List<ProductsDataSet.DocsTblRow> Boxrows =
        //    new List<ProductsDataSet.DocsTblRow>();

        ////структура машины
        //public System.Collections.Generic.Dictionary<string,//car
        //    System.Collections.Generic.Dictionary<string, //TORG12
        //        System.Collections.Generic.Dictionary<string,//Box navcode
        //            Boxes>>> TtnStruct =
        //            new Dictionary<string, Dictionary<string, Dictionary<string, Boxes>>>();

        string currentTtnBarcode = string.Empty;
        

        public ReturnForm()
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

            //ActionsClass.Action.BeginScan();
            //ActionsClass.Action.OnActionCompleted += new ActionsClass.ActionCompleted(Action_OnActionCompleted);
            ScanClass.Scaner.InitScan();
            ScanClass.Scaner.OnScanned += scannedDelegate;
            enableScan = true;

            //READ FIRST ANY VALUE - CASHE INDEX
            //ActionsClass.Action.GetDataByDocIdAndType("4000000000000",
            //                (byte)TSDUtils.ActionCode.ReturnsTTN);

            this.Refresh();

            if (onEndLoad != null)
                onEndLoad();




        }
        void InventarForm_Closed(object sender, System.EventArgs e)
        {
            //BTPrintClass.PrintClass.SetStatusEvent("Begin closing");
            ScanClass.Scaner.OnScanned -= scannedDelegate;
            //ActionsClass.Action.EndScan();
            ScanClass.Scaner.StopScan();
            enableScan = false;
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
                if (!enableScan)
                    return;


                this.bkLabel.Visible = false;
                this.docLabel.Visible = false;
                this.errLabel.Visible = false;
                
                this.dateLabel.Visible = false;
                this.textBox1.Text = barcode;
                this.textBox1.SelectAll();
                this.listBox1.Items.Clear();

                //Boxrows.Clear();
                //IncomerowsList.Clear();
                //изменено на просто 4, было 400 - 09092013
                //новый ШК машины теперь будет начинаться с 4
                //if (barcode.StartsWith("4") && barcode.Length == 13)
                //{
                    //currentTtnBarcode = barcode;
                    //загрузить список машин - должна быть 1 запись т.к. машины уникальны.
                try
                {
                    ProductsDataSet.DocsTblRow[] rows =
                            ActionsClass.Action.FindAllReturnsByProduct(barcode);

                    if (rows != null && rows.Length > 0)
                    {
                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.ReturnsTTN);
                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.ReturnsTTN);

                        
                        docsList.Add(rows[0].DocId, rows[0].Text2);

                        if (rows.Length > 1)
                        {
                            for (int i = 1; i < rows.Length; i++)
                            {
                                if (docsList.ContainsKey(rows[0].DocId))
                                    continue;
                                else
                                    docsList.Add(rows[i].DocId, rows[i].Text2);

                            }
                        }
                    }
                    else
                        throw new ApplicationException("Не найдено возвратов");

                    foreach (string key in docsList.Keys)
                    {

                        this.listBox1.Items.Add(string.Concat(key, "|", docsList[key]));
                        this.listBox1.Focus();
                        this.listBox1.SelectedIndex = 0;
                        

                    }

                }
                catch (Exception err)
                {
                    this.errLabel.Visible = true;
                    string errmsg = err.Message;
                    int l = errmsg.Length;
                    int qty = l / 30;
                    
                    for (int i = 0; i < qty; i++)
                    {
                        if ( l - i*30 <30)
                            this.listBox1.Items.Add(errmsg.Substring(i*30));
                        else
                            this.listBox1.Items.Add(errmsg.Substring(i * 30,30));
                    }
                    this.textBox1.Focus();
                    this.textBox1.SelectAll();
                    return;
                }
                this.Refresh();
            }


        }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeyDoun(e);   
        }

        private void ProcessKeyDoun(KeyEventArgs e)
        {
            try
            {
                //BTPrintClass.PrintClass.SetStatusEvent("KeyDown pressed");
                //this.textBox1.Text = e.KeyValue.ToString();
                //this.textBox1.Text = e.KeyCode.ToString();
                if (e.KeyCode == Keys.Enter)
                {
                    if (textBox1.Focused)
                    {
                        if (textBox1.Text != string.Empty)
                            OnScanned(textBox1.Text);
                    }
                    else
                    {
                        

                        try
                        {

                            enableScan = false;

                            //int selected = listBox1.SelectedIndex;
                            string[] selectObj = listBox1.Items[listBox1.SelectedIndex].ToString().Split('|');

                            ScanClass.Scaner.OnScanned -= scannedDelegate;
                            ScanClass.Scaner.StopScan();
                            //ActionsClass.Action.CreateNewReturnBox(
                           
                            //проверить есть ли открытый короб
                            //если есть - перейти в него
                            //если нет - создать новый
                            string returnBox = ActionsClass.Action.CheckOpenedReturnBoxAndGo(selectObj[0]);

                            using (ViewProductForm prodfrm =
                                new ViewProductForm(
                                    WorkMode.ReturnBoxWProducts,
                                    returnBox,
                                    //selectObj[0],
                                    textBox1.Text))
                            {
                                prodfrm.ShowDialog();
                            }
                        }
                        catch (Exception err)
                        {
                            BTPrintClass.PrintClass.SetErrorEvent(err.Message);
                        }
                        finally
                        {

                            ScanClass.Scaner.InitScan();
                            ScanClass.Scaner.OnScanned += scannedDelegate;
                            enableScan = true;
                        }


                    }



                    return;
                }
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                    return;
                }
                if (e.KeyValue == (int)SpecialButton.RedBtn)
                {

                    if (listBox1.Items[listBox1.SelectedIndex] != null)
                    {
                        string[] selectObj = listBox1.Items[listBox1.SelectedIndex].ToString().Split('|');
                        //string[] selectObj = listBox1.SelectedValue.ToString().Split('|');

                        BTPrintClass.PrintClass.SetStatusEvent(selectObj[0]);
                        BTPrintClass.PrintClass.SetStatusEvent(textBox1.Text);
                        enableScan = false;

                        try
                        {
                            ScanClass.Scaner.OnScanned -= scannedDelegate;
                            ScanClass.Scaner.StopScan();
                            using (ViewProductForm prodfrm =
                                new ViewProductForm(
                                    WorkMode.ReturnBoxWProducts,
                                    selectObj[0],
                                    textBox1.Text))
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



                    //if (!closedCar && currentTtnBarcode != string.Empty)
                    //{
                    //    enableScan = false;
                    //    ScanClass.Scaner.OnScanned -= scannedDelegate;
                    //    try
                    //    {
                    //        using (IncomeForm income =
                    //            new IncomeForm(TtnStruct, currentTtnBarcode))
                    //        {

                    //            income.ShowDialog();
                    //        }
                    //    }
                    //    finally
                    //    {
                    //        enableScan = true;
                    //        ScanClass.Scaner.InitScan();
                    //        ScanClass.Scaner.OnScanned += scannedDelegate;
                    //    }
                    //    CheckStatus(currentTtnBarcode);
                    //}
                    return;
                }
                if (e.KeyValue == (int)SpecialButton.BlueBtn)
                {

                    return;
                }
                if (e.KeyValue == (int)SpecialButton.YellowBtn)
                {
                    //enableScan = false;
                    //try
                    //{
                    //    if (currentTtnBarcode != string.Empty &&
                    //        Boxrows != null &&
                    //        Boxrows.Count > 0)
                    //    {

                    //        using (ViewTtnForm vFrm = new ViewTtnForm(TtnStruct))
                    //        {
                    //            vFrm.ShowDialog();
                    //        }
                    //    }

                    //}
                    //finally
                    //{
                    //    enableScan = true;
                    //}

                    return;
                }
                if (e.KeyValue == (int)SpecialButton.GreenBtn) //GreenBtn
                {

                    return;
                }

            }
            finally
            {
                this.Refresh();
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeyDoun(e);  
        }


    }





}