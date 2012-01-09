using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSDServer
{
    public partial class ViewTtnForm : Form
    {

        /*
        /// <summary>
        /// Список накладных и коробов
        /// </summary>
        System.Collections.Generic.List<ProductsDataSet.DocsTblRow> IncomerowsList =
                    new List<ProductsDataSet.DocsTblRow>();
        /// <summary>
        /// Список всех коробов машины
        /// </summary>
        System.Collections.Generic.List<ProductsDataSet.DocsTblRow> Boxrows = null;
        */

        System.Collections.Generic.Dictionary<string,//car
            System.Collections.Generic.Dictionary<string, //TORG12
                System.Collections.Generic.Dictionary<string,//Box navcode
                    Boxes>>> TtnStruct = null;



        public ViewTtnForm()
        {
            InitializeComponent();
        }

        public ViewTtnForm(System.Collections.Generic.Dictionary<string,//car
            System.Collections.Generic.Dictionary<string, //TORG12
                System.Collections.Generic.Dictionary<string,//Box navcode
                    Boxes>>> ttnStruct)
        {
            InitializeComponent();

            TtnStruct = ttnStruct;
           
        }

        private void ViewTtnForm_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ViewTtnForm_Load(object sender, EventArgs e)
        {
            //this.treeView1.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            this.lblAcceptedBox.Text = "Принято коробов: 0";
            this.lblTotalBox.Text = "Всего коробов: 0";

            int totalBox = 0;
            int totalAccepted = 0;

            this.Width = 240;
            this.Height = 295;

            if (TtnStruct == null ||
                TtnStruct.Count == 0)
            {
                TreeNode zeroNode = this.treeView1.Nodes.Add("Нет данных по ТТН!");
                zeroNode.BackColor = System.Drawing.Color.Plum;
            }
            else
            {
                foreach (string car in TtnStruct.Keys)
                {
                    TreeNode carNode = this.treeView1.Nodes.Add(car);
                    carNode.Text = car;
                    
                    
                    

                    foreach (string incomes in TtnStruct[car].Keys)
                    {
                        bool fullAccepted = true;
                        int acceptedCount = 0;
                        TreeNode incomeNode = carNode.Nodes.Add(incomes);
                        
                        

                        foreach (string box in TtnStruct[car][incomes].Keys)
                        {

                            TreeNode boxNode = incomeNode.Nodes.Add(TtnStruct[car][incomes][box].Barcode);
                            totalBox++;

                            /*
                             Сканируем короб, проверяем свой-чужой.
По таблице документов ищем тип= 7, DocId = ШК короба.
Нашли - Ок.
Записываем в БД
ШК = код короба, DocId  = номер ТТН, кол – во факт =  план = 1, тип 12, Priority = 0. 
                             */

                            if (!TtnStruct[car][incomes][box].Accepted)
                            {
                                ScannedProductsDataSet.ScannedBarcodesRow r =
                                ActionsClass.Action.FindByBarcodeDocTypeDocId(
                                    long.Parse(TtnStruct[car][incomes][box].Barcode),
                                    (byte)TSDUtils.ActionCode.CarsBoxes,
                                    car);


                                if (r == null)
                                {
                                    boxNode.BackColor = System.Drawing.Color.White;
                                    fullAccepted = fullAccepted & false;
                                }
                                else
                                {
                                    totalAccepted++;
                                    boxNode.BackColor = System.Drawing.Color.PaleGreen;
                                    TtnStruct[car][incomes][box].Accepted = true;
                                    fullAccepted = fullAccepted & true;
                                    acceptedCount++;
                                }
                            }
                            else
                            {
                                totalAccepted++;
                                acceptedCount++;
                                boxNode.BackColor = System.Drawing.Color.PaleGreen;
                                fullAccepted = fullAccepted & true;
                            }
                        }

                        if (fullAccepted)
                        {
                            incomeNode.BackColor = System.Drawing.Color.PaleGreen;
                            incomeNode.ForeColor = System.Drawing.Color.Black;

                        }
                        else
                        {
                            if (acceptedCount > 0)
                            {
                                incomeNode.BackColor = System.Drawing.Color.White;
                                incomeNode.ForeColor = System.Drawing.Color.Red;
                            }
                            else
                            {
                                incomeNode.BackColor = System.Drawing.Color.White;
                                incomeNode.ForeColor = System.Drawing.Color.Black;
                            }
                        }


                    }
                    carNode.Expand();


                    
                }
            }

            this.lblAcceptedBox.Text = string.Format("Принято коробов: {0}", totalAccepted);
            this.lblTotalBox.Text = string.Format("Всего коробов: {0}",totalBox);
            this.Refresh();
        }

        /*
        void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.BackColor == System.Drawing.Color.PaleGreen)
                e.Node.BackColor = System.Drawing.Color.PaleGreen;
            

        }*/

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyValue == FunctionButtons.YellowBtn)
            {
                this.Close();
                return;
            }
        }
    }

}