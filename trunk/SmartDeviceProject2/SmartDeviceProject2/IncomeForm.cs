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
        CarScanMode _mode;

        string _currentTTNBarcode;
        System.Collections.Generic.Dictionary<string,//car
            System.Collections.Generic.Dictionary<string, //TORG12
                System.Collections.Generic.Dictionary<string,//Box navcode
                    Boxes>>> TtnStruct = null;

        public IncomeForm()
        {
            InitializeComponent();
            _mode = CarScanMode.BoxScan;

            scannedDelegate = new Scanned(OnScanned);
        }

        

        public IncomeForm(System.Collections.Generic.Dictionary<string,//car
            System.Collections.Generic.Dictionary<string, //TORG12
                System.Collections.Generic.Dictionary<string,//Box navcode
                    Boxes>>> ttnStruct,
            string barCode)
        {
            InitializeComponent();

            scannedDelegate = new Scanned(OnScanned);
            TtnStruct = ttnStruct;
            _currentTTNBarcode = barCode;
            _mode = CarScanMode.CarsScan;
        }

        private void InventarForm_Load(object sender, EventArgs e)
        {
            if (_mode == CarScanMode.CarsScan)
                label4.Text = "F3-Завершить";
            else
                label4.Text = "F3-Прием Товаров";

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
                            (byte)TSDUtils.ActionCode.IncomeBox);

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
            switch (_mode)
            {
                case CarScanMode.CarsScan: {            
                        CarScanModeScaneed(barcode);
                        break;
                }
                case CarScanMode.BoxScan: {
                    BoxScanModeScanned(barcode);
                    break;
                }
                default:
                    return;
            }
            
        }

        void CarScanModeScaneed(string barcode)
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

                    bool found = false;
                    foreach (string incomeDoc in TtnStruct[_currentTTNBarcode].Keys)
                    {//цикл по всем накладным
                        foreach (string navCodeBox in TtnStruct[_currentTTNBarcode][incomeDoc].Keys)
                        {//цикл по всем коробам этой накладной
                            if (
                                TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox].Barcode ==
                                barcode)
                            {
                                this.bkLabel.Visible = true;
                                this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                                this.docLabel.Visible = true;
                                this.docLabel.Text = string.Format
                                    ("Накладная № {0}", incomeDoc);//row.Text2);

                                this.txtLabel.Visible = true;

                                this.dateLabel.Visible = true;
                                this.dateLabel.Text = TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox].DateLabel;
                                TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox].Accepted = true;

                                //записываем в БД
                                ActionsClass.Action.IncomeCarBoxAction(barcode, TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox]);
                                found = true;

                            }
                        }
                    }
                    if (!found)
                    {
                        this.errLabel.Text = string.Format("Короб {0} не относится к данной ТТН!",
                                barcode);

                        this.errLabel.Visible = true;
                    }

                        /*try
                        {
                            //сравнение ШК данного и штрихкода поставки 
                            //накладная и коробка должны соответствовать
                            if (barcode ==
                                TtnStruct[_currentTTNBarcode][incomeDoc][row.NavCode].Barcode)
                            {
                                //если ок - ставим признак "принят"
                                TtnStruct[_currentTTNBarcode][incomeDoc][row.NavCode].Accepted = true;

                                this.bkLabel.Visible = true;
                                this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                                this.docLabel.Visible = true;
                                this.docLabel.Text = string.Format
                                    ("Накладная № {0}", incomeDoc);//row.Text2);

                                this.txtLabel.Visible = true;

                                this.dateLabel.Visible = true;
                                this.dateLabel.Text = row.Text3;
                                //записываем в БД
                                ActionsClass.Action.IncomeBoxAction(barcode, row);
                                found = true;

                                break;
                            }
                            else
                            {
                                //не соответствует
                                throw new ApplicationException("Короб не относится к данной ТТН!");
                            }
                        }
                        catch
                        //не найдена запись
                        {
                            this.errLabel.Text = string.Format("Короб {0} не относится к данной ТТН!",
                                barcode);

                            this.errLabel.Visible = true;
                        }

                    }*/
                    //НЕ НАЙДЕНО - СТРАННО
                   
                    /*


                    //Проверка по всем коробам данного магазина
                    ProductsDataSet.DocsTblRow[] rows =
                            ActionsClass.Action.GetDataByDocIdAndType(barcode,
                            (byte)TSDUtils.ActionCode.IncomeBox);


                    if (rows != null && rows.Length > 0)
                    {
                        //если запись найдена - то в данный магазин должен припыбит этот короб
                        ProductsDataSet.DocsTblRow row = rows[0];

                        //проверка, чтобы короб был в данной поставке (машине)
                        bool found = false;
                        foreach (string incomeDoc in TtnStruct[_currentTTNBarcode].Keys)
                        {//цикл по всем накладным
                            try
                            {
                                //сравнение ШК данного и штрихкода поставки 
                                //накладная и коробка должны соответствовать
                                if (barcode ==
                                    TtnStruct[_currentTTNBarcode][incomeDoc][row.NavCode].Barcode)
                                {
                                    //если ок - ставим признак "принят"
                                    TtnStruct[_currentTTNBarcode][incomeDoc][row.NavCode].Accepted = true;

                                    this.bkLabel.Visible = true;
                                    this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                                    this.docLabel.Visible = true;
                                    this.docLabel.Text = string.Format
                                        ("Накладная № {0}", incomeDoc);//row.Text2);

                                    this.txtLabel.Visible = true;

                                    this.dateLabel.Visible = true;
                                    this.dateLabel.Text = row.Text3;
                                    //записываем в БД
                                    ActionsClass.Action.IncomeBoxAction(barcode, row);
                                    found = true;

                                    break;
                                }
                                else
                                {
                                    //не соответствует
                                    throw new ApplicationException("Короб не относится к данной ТТН!");
                                }
                            }
                            catch
                            //не найдена запись
                            {
                                this.errLabel.Text = string.Format("Короб {0} не относится к данной ТТН!",
                                    barcode);

                                this.errLabel.Visible = true;
                            }

                        }
                        //НЕ НАЙДЕНО - СТРАННО
                        if (!found)
                        {
                            this.errLabel.Text = string.Format("Короб {0} не относится к данной ТТН!",
                                    barcode);

                            this.errLabel.Visible = true;
                        }
                        */

                        //string.Format(
                        //"Дата: {0}", DateTime.Today.ToString("dd.MM.yyyy"));
                        /*
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
                         * 
                         */
                    /*
                    }
                    else
                    {
                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        this.errLabel.Text = string.Format("ШК {0} чужой короб!",
                            barcode);

                        this.errLabel.Visible = true;
                    }*/
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

        void BoxScanModeScanned(string barcode)
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
                        this.docLabel.Text = 
                            string.Format("Накладная № {0}", row.Text2);

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

                        ActionsClass.Action.IncomeBoxAction(scannedRow);

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
                /*
                if (textBox1.Text != string.Empty)
                    OnScanned(textBox1.Text);
                */
  
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
                if (_mode == CarScanMode.BoxScan)
                {
                    int totals = 0;
                    int totalBk = 0;
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
                }
                else
                {
                    using (ViewTtnForm vFrm = new ViewTtnForm(TtnStruct))
                    {
                        vFrm.ShowDialog();
                    }
                }
                this.Refresh();

                return;
            }
            if (e.KeyValue == (int)SpecialButton.GreenBtn) //GreenBtn
            {
                if (_mode == CarScanMode.CarsScan)
                {
                    int total = 0;
                    
                    foreach (string incomeDoc in TtnStruct[_currentTTNBarcode].Keys)
                    {//цикл по всем накладным
                        foreach (string navCodeBox in TtnStruct[_currentTTNBarcode][incomeDoc].Keys)
                        {
                            if (TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox].Accepted)
                                total++;
                        }
                    }


                    using (DialogForm dlgfrm =
                            new DialogForm(
                                "Вы хотите закончить просчет ТТН?"
                                , ""//string.Format("Посчитано: {0} кодов", totalBk)
                                , string.Format("Итого: {0} коробов", total)
                                , "Закрытие ТТН"))
                    {
                        if (dlgfrm.ShowDialog() == DialogResult.Yes)
                        {
                            /*ActionsClass.Action.CloseInv(
                                _documentId,
                                TSDUtils.ActionCode.InventoryGlobal);
                            */
                            this.Close();
                        }
                    }

                }
                return;
            }
             
        }
            

    }

    public enum CarScanMode : byte
    {
        CarsScan,
        BoxScan
    }
}