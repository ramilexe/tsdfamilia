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
            this.Width = 240;
            this.Height = 295;

            if (TtnStruct == null ||
                TtnStruct.Count == 0)
            {
                /*
                Label lbl = new Label();
                lbl.Text = "Нет данных по ТТН!";
                lbl.Size = new System.Drawing.Size(229, 20);
                lbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                lbl.BackColor = System.Drawing.Color.Red;
                lbl.ForeColor = System.Drawing.Color.Black;
                lbl.Location = new System.Drawing.Point(3, 131);
                this.panel1.Controls.Add(lbl);
                this.treeView1.Visible = false;
                 */
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
                        TreeNode incomeNode = carNode.Nodes.Add(incomes);

                        foreach (string box in TtnStruct[car][incomes].Keys)
                        {
                            TreeNode boxNode = incomeNode.Nodes.Add(box);
                            /*
                             Сканируем короб, проверяем свой-чужой.
По таблице документов ищем тип= 7, DocId = ШК короба.
Нашли - Ок.
Записываем в БД
ШК = код короба, DocId  = номер ТТН, кол – во факт =  план = 1, тип 12, Priority = 0. 
                             */

                            ScannedProductsDataSet.ScannedBarcodesRow r = 
                            ActionsClass.Action.FindByBarcodeDocTypeDocId(
                                long.Parse(TtnStruct[car][incomes][box].Barcode),
                                (byte)TSDUtils.ActionCode.CarsBoxes,
                                car);


                            if (r == null)
                            {
                                boxNode.BackColor = System.Drawing.Color.Plum;
                                boxNode.Parent.BackColor = System.Drawing.Color.Plum;
                            }
                            else
                            {
                                boxNode.BackColor = System.Drawing.Color.PaleGreen;
                                TtnStruct[car][incomes][box].Accepted = true;
                            }




                        }


                    }

                    
                }
            }
           


        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyValue == (int)SpecialButton.YellowBtn)
            {
                this.Close();
                return;
            }
        }
    }

}