namespace TSDServer
{
    
    
    public partial class ScannedProductsDataSet {

        public partial class ScannedBarcodesDataTable
        {
            public ScannedProductsDataSet.ScannedBarcodesRow[] FindByBarcodeAndDoc(long Barcode, string docId)
            {
                return (ScannedProductsDataSet.ScannedBarcodesRow[])(this.Select(string.Format("Barcode = {0} and DocId = \"{1}\"", Barcode,docId)));
            }
            /*
            public ScannedProductsDataSet.ScannedBarcodesRow FindByBarcodeAndDoc(long Barcode, string docId, byte docType)
            {
                return (ScannedProductsDataSet.ScannedBarcodesRow)(this.Select(string.Format("Barcode = {0} and DocId = \"{1}\" and DocType = {2}", Barcode, docId, docType),"Priority ASC"));
            }*/


            public ScannedProductsDataSet.ScannedBarcodesRow UpdateQuantity(long Barcode, byte docType, int quanity)
            {
                ScannedProductsDataSet.ScannedBarcodesRow[] scanRows = 
                    (ScannedProductsDataSet.ScannedBarcodesRow[])
                        this.Select(
                        string.Format("Barcode = {0} and DocType = {1}", Barcode, docType), 
                        "Priority ASC");


                ScannedProductsDataSet.ScannedBarcodesRow return_row=null;
                foreach (ScannedProductsDataSet.ScannedBarcodesRow r in scanRows)
                {
                    return_row = r;
                    if (r.PlanQuanity > 0) 
                    {
                        if (r.PlanQuanity > r.FactQuantity)
                        {
                            r.FactQuantity += quanity;
                        }
                    }
                }
                return return_row;




            }
        
        }


    }
}

namespace TSDServer.ScannedProductsDataSetTableAdapters
{


    public class ScannedBarcodesTableAdapter : System.IDisposable
    {
        bool _disposed = false;
        public string[] FileList
        {
            get
            {
                return table.FileList.ToArray();
            }

        }
        FamilTsdDB.DataTable table = null;
        TSDServer.ScannedProductsDataSet _scannedproductsDataset;

        public ScannedBarcodesTableAdapter(TSDServer.ScannedProductsDataSet scannedproductsDataset)
        {
            FamilTsdDB.DataTable.BaseDate = TSDServer.Properties.Settings.Default.BaseDate;
            FamilTsdDB.DataTable.StartupPath = Properties.Settings.Default.LocalFilePath;
            _scannedproductsDataset = scannedproductsDataset;
            table = new FamilTsdDB.DataTable(_scannedproductsDataset.ScannedBarcodes);
            table.AddIndex(
                new System.Data.DataColumn[] {
                _scannedproductsDataset.ScannedBarcodes.Columns["Barcode"],
                _scannedproductsDataset.ScannedBarcodes.Columns["DocId"]
                });

            //table.ReadTableDef();

        }
        public void Fill(TSDServer.ScannedProductsDataSet scannedproductsDataset)
        {
            scannedproductsDataset.ScannedBarcodes.Clear();
            table.ReadTableDef();
            table.Fill(scannedproductsDataset.ScannedBarcodes);
        }
        public void Update(TSDServer.ScannedProductsDataSet scannedproductsDataset)
        {
            if (table == null)
            {
                table = new FamilTsdDB.DataTable(scannedproductsDataset.ScannedBarcodes);
                table.AddIndex(
                    new System.Data.DataColumn[] {
                        _scannedproductsDataset.ScannedBarcodes.Columns["Barcode"],
                        _scannedproductsDataset.ScannedBarcodes.Columns["DocId"]
                });
            }
            table.Write();
        }

        public TSDServer.ScannedProductsDataSet.ScannedBarcodesRow[] GetDataByBarcode(System.Int64 barcode, string docId)
        {
            ScannedProductsDataSet.ScannedBarcodesRow[] r = _scannedproductsDataset.ScannedBarcodes.FindByBarcodeAndDoc(barcode, docId);
            if (r != null &&
                r.Length > 0)
                return r;
            else
            {
                System.Data.DataRow[] rows = table.FindByIndexes(1, new object[] { barcode });
                if (rows != null && rows.Length > 0)
                {
                    r = new ScannedProductsDataSet.ScannedBarcodesRow[rows.Length];
                    for (int i = 0; i < r.Length; i++)
                    {
                        r[i] = (ScannedProductsDataSet.ScannedBarcodesRow)rows[i];
                        try
                        {
                            _scannedproductsDataset.ScannedBarcodes.AddScannedBarcodesRow(r[i]);
                        }
                        catch (System.Data.ConstraintException)
                        {
                        }
                    }
                    return r;
                }
                else
                    return null;
            }
        }

        public void Clean()
        {
            this._scannedproductsDataset.ScannedBarcodes.Clear();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_disposed)
            {
                table.Dispose();
                table = null;
                _disposed = true;
            }
        }

        #endregion
    }

}