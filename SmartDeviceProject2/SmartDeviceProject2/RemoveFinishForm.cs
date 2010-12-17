using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Familia.TSDClient
{
    public partial class RemoveFinishForm : Form
    {
        ProductsDataSet.ProductsTblRow _datarow;
        ProductsDataSet.DocsTblRow _docsRow;
        public RemoveFinishForm()
        {
            InitializeComponent();
        }
        public RemoveFinishForm(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docsRow)
            :this()
        {
            _datarow = datarow;
            _docsRow = docsRow;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        void button1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == 13 || e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }


        void RemoveFinishForm_Load(object sender, System.EventArgs e)
        {
            this.label1.Text = string.Format("Документ \"{0}\" собран",
                TSDUtils.ActionCodeDescription.ActionDescription[_docsRow.DocType]);
            this.label2.Text = string.Format("Документ № {0} от {1}",_docsRow.DocId,
                (_docsRow["DocumentDate"] == System.DBNull.Value ||
                _docsRow["DocumentDate"] == null) ? string.Empty :
                _docsRow.DocumentDate.ToString("dd.MM.yyyy"));
        }

    }
}