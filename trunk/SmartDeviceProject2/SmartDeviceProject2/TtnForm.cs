using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class TtnForm : Form
    {
        public event EndLoadDelegate onEndLoad;

        private bool closedCar = false;
        //private bool enableInvent = false;
        Scanned scannedDelegate = null;
        /// <summary>
        /// загрузить список всех накладных машин
        /// </summary>
        System.Collections.Generic.List<ProductsDataSet.DocsTblRow> IncomerowsList =
                            new List<ProductsDataSet.DocsTblRow>();
        /// <summary>
        /// Загрузить список всех коробов машины
        /// </summary>
        System.Collections.Generic.List<ProductsDataSet.DocsTblRow> Boxrows =
            new List<ProductsDataSet.DocsTblRow>();

        //структура машины
        public System.Collections.Generic.Dictionary<string,//car
            System.Collections.Generic.Dictionary<string, //TORG12
                System.Collections.Generic.Dictionary<string,//Box navcode
                    Boxes>>> TtnStruct =
                    new Dictionary<string, Dictionary<string, Dictionary<string, Boxes>>>();

        string currentTtnBarcode = string.Empty;

        public TtnForm()
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

            //READ FIRST ANY VALUE - CASHE INDEX
            ActionsClass.Action.GetDataByDocIdAndType("3000000000000",
                            (byte)TSDUtils.ActionCode.Cars);

            this.Refresh();

            if (onEndLoad != null)
                onEndLoad();


            
            
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

                Boxrows.Clear();
                IncomerowsList.Clear();

                if (barcode.StartsWith("320") && barcode.Length == 13)
                {
                    currentTtnBarcode = barcode;
                    //загрузить список машин - должна быть 1 запись т.к. машины уникальны.

                    ProductsDataSet.DocsTblRow[] rows =
                            ActionsClass.Action.GetDataByDocIdAndType(barcode,
                            (byte)TSDUtils.ActionCode.CarsBoxes);

                   

                    

                    if (rows != null && rows.Length > 0)
                    {
                        Boxrows.AddRange(rows);
                        //записи машины найдены
                        ProductsDataSet.DocsTblRow row = rows[0];


                        this.bkLabel.Visible = true;
                        this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                        this.docLabel.Visible = true;
                        this.docLabel.Text = string.Format
                            ("ТТН № {0}", row.Text2);

                        this.txtLabel.Visible = true;

                        this.dateLabel.Visible = true;
                        this.dateLabel.Text = row.Text3;
                        //string.Format(
                        //"Дата: {0}", DateTime.Today.ToString("dd.MM.yyyy"));


                        CheckStatus(barcode);

                        if (Boxrows != null && Boxrows.Count > 0)
                        {

                            //загрузить список всех накладных машин
                            foreach (ProductsDataSet.DocsTblRow docsRow in Boxrows)
                            {

                                //ProductsDataSet.DocsTblRow[] Incomerows =
                                //ActionsClass.Action.GetDataByNavCodeAndType(docsRow.NavCode,
                                //(byte)TSDUtils.ActionCode.BoxIncomes);

                                IncomerowsList.AddRange(ActionsClass.Action.GetDataByNavCodeAndType(docsRow.NavCode,
                                (byte)TSDUtils.ActionCode.BoxIncomes));
                            }
                        }

                        currentTtnBarcode = barcode;
                        //Загрузить список всех коробов машины
                        FillCar(IncomerowsList, Boxrows);

                    }
                    else
                    {
                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        this.errLabel.Text = string.Format("ТТН {0} не для этого магазина!",
                            barcode);

                        this.errLabel.Visible = true;
                    }
                    this.Refresh();
                }
                else
                {
                    currentTtnBarcode = string.Empty;
                    ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                    ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                     this.errLabel.Text = string.Format("Это не ШК ТТН!",
                            barcode);

                     this.errLabel.Visible = true;
                    
                }
               // this.textBox1.SelectAll();
            }
            
            
        }

        private void CheckStatus(string barcode)
        {
            ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                        ActionsClass.Action.FindByBarcodeDocTypeDocId(
                            long.Parse(barcode),
                            (byte)TSDUtils.ActionCode.Cars,
                            barcode);
            this.label2.Visible = true;
            this.errLabel.Visible = false;
            this.txtLabel.Text = "ТТН ПРИНЯТЬ";

            if (scannedRow == null)
            {
                scannedRow =
                   ActionsClass.Action.AddScannedRow(
                   long.Parse(barcode),
                   (byte)TSDUtils.ActionCode.Cars,
                   barcode,
                   0,
                   0);
                scannedRow.FactQuantity += 1;

                ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.Cars);
                ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.Cars);
            }
            else
            {
                switch (scannedRow.Priority)
                {
                    case 0:
                        {
                            this.txtLabel.Text = "ТТН В ПРИЕМКЕ";
                            //ok scanning
                            break;
                        }
                    case byte.MaxValue:
                        {
                            //closed
                            closedCar = true;
                            this.label2.Visible = false;
                            this.txtLabel.Text = "ТТН ПРИНЯТО";
                            ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                            ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                            this.errLabel.Text = string.Format("ТТН уже закрыто!",
                                barcode);

                            this.errLabel.Visible = true;

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }
        }

        private void FillCar(System.Collections.Generic.List<ProductsDataSet.DocsTblRow> _IncomerowsList,
            System.Collections.Generic.List<ProductsDataSet.DocsTblRow> _Boxrows)
        {
            TtnStruct =
                   new Dictionary<string, Dictionary<string, Dictionary<string, Boxes>>>();

            //System.Collections.Generic.
            TtnStruct.Add(_Boxrows[0].DocId,
                new Dictionary<string, Dictionary<string, Boxes>>());

            //кикл по машинам и коробам
            foreach (ProductsDataSet.DocsTblRow boxRow in _Boxrows)
            {
                //цикл по коробам и накладным
                foreach (ProductsDataSet.DocsTblRow incRow in _IncomerowsList)
                {
                    //если есть накладная
                    if (TtnStruct[boxRow.DocId].ContainsKey(incRow.DocId))
                    {
                        //проверить короб
                        if (TtnStruct[boxRow.DocId][incRow.DocId].ContainsKey(incRow.NavCode))
                            continue;//короб есть - продолжим
                        else
                        {
                            //короба нет - добавить

                            ProductsDataSet.DocsTblRow[] d =
                            ActionsClass.Action.GetDataByNavCodeAndType(
                                incRow.NavCode,
                                (byte)TSDUtils.ActionCode.IncomeBox);

                            if (d != null && d.Length > 0)
                            {
                                TtnStruct[boxRow.DocId][incRow.DocId].Add(incRow.NavCode,
                                    new Boxes(d[0].DocId, incRow.NavCode, d[0].Text3, d[0].Text2));
                            }
                        }


                    }
                    else
                    {
                        //нкладной нет - добавить
                        TtnStruct[boxRow.DocId].Add(incRow.DocId, new Dictionary<string, Boxes>());
                        //добавить текущий короб
                        ProductsDataSet.DocsTblRow[] d =
                           ActionsClass.Action.GetDataByNavCodeAndType(
                               incRow.NavCode,
                               (byte)TSDUtils.ActionCode.IncomeBox);

                        if (d != null && d.Length > 0)
                        {
                            TtnStruct[boxRow.DocId][incRow.DocId].Add(
                                incRow.NavCode,
                                new Boxes(d[0].DocId, incRow.NavCode,d[0].Text3,d[0].Text2));
                        }


                    }


                }

            }
        }




        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
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
                    if (!closedCar && currentTtnBarcode != string.Empty)
                    {

                        using (IncomeForm income =
                            new IncomeForm(TtnStruct, currentTtnBarcode))
                        {

                            income.ShowDialog();
                        }
                        CheckStatus(currentTtnBarcode);
                    }
                    return;
                }
                if (e.KeyValue == (int)SpecialButton.BlueBtn)
                {

                    return;
                }
                if (e.KeyValue == (int)SpecialButton.YellowBtn)
                {
                    if (currentTtnBarcode != string.Empty &&
                        Boxrows != null &&
                        Boxrows.Count > 0)
                    {

                        using (ViewTtnForm vFrm = new ViewTtnForm(TtnStruct))
                        {
                            vFrm.ShowDialog();
                        }
                    }


                    /*int totals=0;
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
                     * */

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
        

            

    }


    
   
   
}