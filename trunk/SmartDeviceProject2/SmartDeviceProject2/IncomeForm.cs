﻿using System;

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
        private bool enableScan = false;
        private bool fullAcceptEnabled = false;
        private bool fullAccepted = false;
        private int totalIncomeCar = 0;
        private int totalQtyCar = 0;
 
        Scanned scannedDelegate = null;
        CarScanMode _mode;

        string _currentTTNBarcode;
        string _currentBoxBarcode;
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
            foreach (string incomeDoc in TtnStruct[_currentTTNBarcode].Keys)
            {//цикл по всем накладным
                foreach (string navCodeBox in TtnStruct[_currentTTNBarcode][incomeDoc].Keys)
                {//цикл по всем коробам этой накладной
                    totalQtyCar += 1;
                }
            }
            
        }

        private void InventarForm_Load(object sender, EventArgs e)
        {
            if (_mode == CarScanMode.CarsScan)
            {
                lblF3.Text = "F3-Завершить";
                lblF1.Visible = false;
                lblQtySku.Visible = false;
                lblQtyTotal.Visible = false;
                lblQtySkuScanned.Visible = false;
                lblQtyTotalScanned.Visible = false;
                ScannedProductsDataSet.ScannedBarcodesRow [] scannedRows = 
                    ActionsClass.Action.FindByDocIdAndDocType(_currentTTNBarcode,
                        (byte)TSDUtils.ActionCode.CarsBoxes);
                totalIncomeCar = scannedRows.Length;
             
                lblQtySku.Font = new Font(FontFamily.GenericSansSerif,
                    lblQtyTotal.Font.Size,
                    FontStyle.Bold);
            }
            else
            {
                //ActionsClass.Action.BeginScan();
                lblF3.Text = "F3-Прием Товаров";
                lblQtySku.Visible = true;
                lblQtyTotal.Visible = true;
                lblQtySkuScanned.Visible = true;
                lblQtyTotalScanned.Visible = true;

                lblQtySku.Text = string.Format("Всего артикулов: {0}",
                   0);
                lblQtyTotal.Text = string.Format("Всего штук: {0}", 0);
                lblQtySkuScanned.Text = string.Format("Всего принято артикулов: {0}",
                   0);
                lblQtyTotalScanned.Text = string.Format("Всего штук принятно: {0}",
                    0);

                lblQtyTotal.Font = new Font(FontFamily.GenericSansSerif,
                    lblQtyTotal.Font.Size,
                    FontStyle.Bold);

                ActionsClass.Action.GetDataByDocIdAndType("3000000000000",
                (byte)TSDUtils.ActionCode.IncomeBox);
            }

            this.Width = 235;
            this.Height = 295;
            textBox1.Focus();

           // string invId = ActionsClass.Action.FindOpenInventar();
            
            
            //ActionsClass.Action.OnActionCompleted += new ActionsClass.ActionCompleted(Action_OnActionCompleted);
            ScanClass.Scaner.InitScan();
            ScanClass.Scaner.OnScanned += scannedDelegate;
            enableScan = true;

            //READ FIRST ANY VALUE - CASHE INDEX


            this.Refresh();
            

            
            
        }
        void InventarForm_Closed(object sender, System.EventArgs e)
        {
            //BTPrintClass.PrintClass.SetStatusEvent("Begin closing");
            ScanClass.Scaner.OnScanned -= scannedDelegate;
            if (_mode == CarScanMode.CarsScan)
            {
                
            }
            else
            {
               // ActionsClass.Action.EndScan();
                
            }
            //ActionsClass.Action.EndScan();
            ScanClass.Scaner.StopScan();
            //BTPrintClass.PrintClass.SetStatusEvent("End closing");
        }

        void OnScanned(string barcode)
        {
            if (enableScan)
            {
                _currentBoxBarcode = barcode;
                switch (_mode)
                {
                    case CarScanMode.CarsScan:
                        {
                            CarScanModeScaneed(barcode);
                            break;
                        }
                    case CarScanMode.BoxScan:
                        {
                            BoxScanModeScanned(barcode);
                            break;
                        }
                    default:
                        return;
                }
                
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

                if (barcode.StartsWith("300") && barcode.Length == 13)
                {
                    lblQtySku.Visible = true;

                    //int totalQty = TtnStruct[_currentTTNBarcode].Keys.Count;

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

                                ScannedProductsDataSet.ScannedBarcodesRow scannedRow = 
                                    ActionsClass.Action.FindByBarcodeDocTypeDocId(
                                     TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox].Barcode,
                                     (byte)TSDUtils.ActionCode.CarsBoxes,
                                     _currentTTNBarcode);
                                if (scannedRow == null)
                                {
                                    totalIncomeCar++;

                                    //записываем в БД
                                    ActionsClass.Action.IncomeCarBoxAction(_currentTTNBarcode,
                                    TtnStruct[_currentTTNBarcode][incomeDoc][navCodeBox]);
                                    
                                }
                                else
                                {
                                    this.errLabel.Text = 
                                        string.Format("Короб уже принят!");

                                    this.errLabel.Visible = true;
                                }
                                found = true;

                            }
                        }
                    }
                    lblQtySku.Text = string.Format("Принято {0} из {1} коробов",
                        totalIncomeCar, totalQtyCar);

                    if (!found)
                    {
                        this.errLabel.Text = string.Format("Короб не в данной ТТН!");

                        this.errLabel.Visible = true;
                    }

                    #region oldcode
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
                    #endregion
                    this.Refresh();
                }
                else
                {
                    _currentBoxBarcode = string.Empty;
                    ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                    ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                    this.errLabel.Text = string.Format("Это не ШК короба!");

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

                if (barcode.StartsWith("300") && barcode.Length == 13)
                {
                    ProductsDataSet.DocsTblRow[] rows =
                            ActionsClass.Action.GetDataByDocIdAndType(barcode,
                            (byte)TSDUtils.ActionCode.IncomeBox);


                    if (rows != null && rows.Length > 0)
                    {
                        //919051|002352151|11|1|76|2011-09-07|7|7|7|002352151||07.09.2011
                        //919051|3001012060898|7|1|76|2011-09-07|7|7|7|919051|002352151|07.09.2011

                        try
                        {
                            ProductsDataSet.DocsTblRow[] boxrows =
                                ActionsClass.Action.GetDataByDocIdAndType(barcode,
                                    (byte)TSDUtils.ActionCode.IncomeBox);

                            if (boxrows.Length > 0)
                            {

                                ProductsDataSet.DocsTblRow[] naklrows =
                                    ActionsClass.Action.GetDataByNavCodeAndType(boxrows[0].NavCode,
                                        (byte)TSDUtils.ActionCode.BoxIncomes);

                                ProductsDataSet.DocsTblRow[] naklrows1 =
                                    ActionsClass.Action.GetDataByDocIdAndType(naklrows[0].DocId,
                                        (byte)TSDUtils.ActionCode.BoxIncomes);

                                if (naklrows.Length > 0)
                                {
                                    this.docLabel.Text =
                                        string.Format("По накладной №{0} {1} коробов", naklrows[0].DocId,
                                        naklrows1.Length);

                                }
                                else
                                    throw new ApplicationException("Накладных не найдено!");
                            }
                            else
                                throw new ApplicationException("Номер короба не определен!");
                        }
                        catch (Exception err)
                        {
                            this.docLabel.Text = err.Message;
                        }

                        ProductsDataSet.DocsTblRow row = rows[0];

                        this.bkLabel.Visible = true;
                        this.bkLabel.Text = string.Format("ШК: {0}", barcode);
                        this.docLabel.Visible = true;

                        this.txtLabel.Visible = true;

                        this.dateLabel.Visible = true;
                        this.dateLabel.Text = row.Text3;

                        RefreshData(barcode);
                        //string.Format(
                        //"Дата: {0}", DateTime.Today.ToString("dd.MM.yyyy"));
                        
                        

                        /*
                        ScannedProductsDataSet.ScannedBarcodesRow scannedRow =
                               ActionsClass.Action.AddScannedRow(
                               long.Parse(barcode),
                               (byte)TSDUtils.ActionCode.IncomeBox,
                               barcode,
                               0,
                               0);
                        scannedRow.FactQuantity += 1;
                        */
                        //ActionsClass.Action.IncomeBoxAction(scannedRow);

                        ActionsClass.Action.PlaySoundAsync(row.MusicCode);
                        ActionsClass.Action.PlayVibroAsync(row.VibroCode);

                    }
                    else
                    {
                        ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                        this.errLabel.Text = string.Format("Это чужой короб!");

                        this.errLabel.Visible = true;
                    }
                    this.Refresh();
                }
                else
                {
                    _currentBoxBarcode = string.Empty;
                    ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                    ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                    this.errLabel.Text = string.Format("Это не ШК короба!",
                           barcode);

                    this.errLabel.Visible = true;

                }
                // this.textBox1.SelectAll();
            }
        }

        private void RefreshData(string barcode)
        {
            this.errLabel.Visible = false;
            ProductsDataSet.DocsTblRow[] product_rows =
                             ActionsClass.Action.GetDataByDocIdAndType(barcode,
                            (byte)TSDUtils.ActionCode.BoxWProducts);

            if (product_rows != null &&
                product_rows.Length > 0)
            {
                
                List<string> uniqCodes = new List<string>();

                
                int totalQty = 0;
                int totalSku = 0;
                foreach (ProductsDataSet.DocsTblRow r in product_rows)
                {
                    totalQty += r.Quantity;

                    if (!uniqCodes.Contains(r.NavCode))
                    {
                        uniqCodes.Add(r.NavCode);
                        totalSku++;
                    }
                }
                lblQtySku.Text = string.Format("Всего артикулов: {0}",
                   totalSku);
                lblQtyTotal.Text = string.Format("Всего штук: {0}", totalQty);
                

                ScannedProductsDataSet.ScannedBarcodesRow[] scannedProdRow =
                    ActionsClass.Action.FindByDocIdAndDocType(
                        barcode,
                        (byte)TSDUtils.ActionCode.BoxWProducts);

                totalQty = 0;
                totalSku = 0;
                uniqCodes.Clear();
                fullAccepted = false;

                if (scannedProdRow != null &&
                    scannedProdRow.Length > 0)
                {
                    foreach (ScannedProductsDataSet.ScannedBarcodesRow r in scannedProdRow)
                    {
                        if (r.Priority == Byte.MaxValue)
                            fullAccepted = true;

                        if (!uniqCodes.Contains(r.Barcode.ToString()))
                        {
                            uniqCodes.Add(r.Barcode.ToString());
                            totalSku++;
                        }

                        totalQty += r.FactQuantity;
                    }
                    
                }

                lblQtySkuScanned.Text = string.Format("Всего принято артикулов: {0}",
                        totalSku);
                lblQtyTotalScanned.Text = string.Format("Всего штук принятно: {0}",
                    totalQty);


                if (totalQty > 0)
                {
                    lblF1.Visible = false;
                    fullAcceptEnabled = false;

                }
                else
                {
                    fullAcceptEnabled = true;
                    lblF1.Visible = true;
                }

                if (fullAccepted)
                {
                    txtLabel.Text = "КОРОБ ПРИНЯТ ПОЛНОСТЬЮ";
                    lblF3.Visible = false;
                    lblF1.Visible = false;
                }
                else
                {
                    txtLabel.Text = "Короб ПРИНЯТЬ";
                    lblF3.Visible = true;
                    //lblF1.Visible = true;
                }

            }
            else
            {
                
                ActionsClass.Action.PlaySoundAsync((byte)TSDUtils.ActionCode.StrangeBox);
                ActionsClass.Action.PlayVibroAsync((byte)TSDUtils.ActionCode.StrangeBox);
                this.errLabel.Text = string.Format("Товаров не найдено!");

                this.errLabel.Visible = true;

                lblQtySku.Text = string.Format("Всего артикулов: {0}",
                   0);
                lblQtyTotal.Text = string.Format("Всего штук: {0}", 0);
                lblQtySku.Text = string.Format("Всего принято артикулов: {0}",
                   0);
                lblQtyTotal.Text = string.Format("Всего штук принятно: {0}",
                    0);

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

                    if (_mode == CarScanMode.BoxScan && fullAcceptEnabled && !fullAccepted)
                    {
                        
                        using (DialogForm dlgfrm =
                                new DialogForm(
                                   lblQtySku.Text,
                                    lblQtyTotal.Text,
                                     "Вы уверены?",
                                     "Принять полностью"))
                        {
                            if (dlgfrm.ShowDialog() == DialogResult.Yes)
                            {
                                ActionsClass.Action.AcceptFullBoxWProductsActionProc(_currentBoxBarcode);
                                RefreshData(_currentBoxBarcode);
    
                            }
                        }

                    }

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
                        /*int totals = 0;
                        int totalBk = 0;
                        ActionsClass.Action.CalculateTotalsWOPriority(
                            TSDUtils.ActionCode.IncomeBox,
                            DateTime.Today,
                            out totalBk,
                            out totals);*/
                        try
                        {
                            /*ScanClass.Scaner.OnScanned -= scannedDelegate;
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
                            */
                            using (ViewBoxForm boxFrm =
                                new ViewBoxForm(_currentBoxBarcode, (byte)TSDUtils.ActionCode.BoxWProducts))
                            {
                                boxFrm.ShowDialog();
                            }

                        }
                        finally
                        {
                            //ScanClass.Scaner.OnScanned += scannedDelegate;

                        }
                    }
                    else
                    {
                        using (ViewTtnForm vFrm = new ViewTtnForm(TtnStruct))
                        {
                            vFrm.ShowDialog();
                        }
                    }
                    

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

                                ActionsClass.Action.CloseDoc(
                                    _currentTTNBarcode,
                                    TSDUtils.ActionCode.Cars);

                                this.Close();
                            }
                        }

                    }
                    else
                    {
                        if (!fullAccepted)
                        {
                            try
                            {


                                enableScan = false;
                                ScanClass.Scaner.StopScan();
                                ScanClass.Scaner.OnScanned -= scannedDelegate;
                                //ScanClass.Scaner.OnScanned =null;
                                //this.Visible = false;
                                //this.Enabled = false;
                                ScanClass.Scaner.StopScan();
                                using (ViewProductForm prodfrm =
                                    new ViewProductForm(
                                        WorkMode.BoxScan,
                                        _currentBoxBarcode))
                                {
                                    prodfrm.ShowDialog();
                                }
                                RefreshData(_currentBoxBarcode);

                            }
                            catch (Exception err)
                            {
                                //using (DialogForm frmErr =
                                //    new DialogForm("Ошибка инв-ции",
                                //        err.Message,
                                //        "ДА – продолжить. Нет – выйти",
                                //         "Ошибка"))
                                //{
                                //    if (frmErr.ShowDialog() == DialogResult.No)
                                //        return;
                                //}
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
                        }
                    }
                    return;
                }
            }
            finally
            {
                this.Refresh();
            }
             
        }

        private void docLabel_ParentChanged(object sender, EventArgs e)
        {

        }
            

    }

    public enum CarScanMode : byte
    {
        CarsScan,
        BoxScan
    }
}