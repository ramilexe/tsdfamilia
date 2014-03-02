namespace TSDServer
{
    
    
    public partial class ScannedProductsDataSet {

        public partial class ScannedBarcodesDataTable
        {
            public ScannedProductsDataSet.ScannedBarcodesRow[] FindByBarcodeAndDoc(long Barcode, string docId)
            {
                return (ScannedProductsDataSet.ScannedBarcodesRow[])(this.Select(string.Format("Barcode = {0} and DocId = '{1}'", Barcode,docId)));
            }

            public ScannedProductsDataSet.ScannedBarcodesRow[] FindByBarcodeAndDocType(long Barcode, byte docType)
            {
                
                return  (ScannedProductsDataSet.ScannedBarcodesRow[])
                        this.Select(
                        string.Format("Barcode = {0} and DocType = {1}", Barcode, docType),
                        "Priority ASC");
                



            }

            public ScannedProductsDataSet.ScannedBarcodesRow[] FindByDocIdAndDocType(string docId, byte docType)
            {

                return (ScannedProductsDataSet.ScannedBarcodesRow[])
                        this.Select(
                        string.Format("DocId ='{0}' and DocType = {1}", docId, docType),
                        "Priority ASC");




            }
            public ScannedProductsDataSet.ScannedBarcodesRow[] FindByDocTypeAndPriority(byte docType, byte Priority)
            {

                return (ScannedProductsDataSet.ScannedBarcodesRow[])
                        this.Select(
                        string.Format("DocType = {0} and Priority={1}", docType, Priority),
                        "Priority ASC");




            }
            public ScannedProductsDataSet.ScannedBarcodesRow[] FindByDocType(byte docType)
            {

                return (ScannedProductsDataSet.ScannedBarcodesRow[])
                        this.Select(
                        string.Format("DocType = {0}", docType),
                        "Priority ASC");




            }
            /*
            public ScannedProductsDataSet.ScannedBarcodesRow FindByBarcodeAndDoc(long Barcode, string docId, byte docType)
            {
                return (ScannedProductsDataSet.ScannedBarcodesRow)(this.Select(string.Format("Barcode = {0} and DocId = \"{1}\" and DocType = {2}", Barcode, docId, docType),"Priority ASC"));
            }*/
            public ScannedProductsDataSet.ScannedBarcodesRow FindFirstByBarcodeAndDocType(long Barcode, byte docType)
            {
                ScannedProductsDataSet.ScannedBarcodesRow[] scanRows =
                    (ScannedProductsDataSet.ScannedBarcodesRow[])
                        this.Select(
                        string.Format("Barcode = {0} and DocType = {1}", Barcode, docType),
                        "Priority ASC");
                if (scanRows != null &&
                    scanRows.Length > 0)
                {
                    return scanRows[0];
                }
                else
                    return null;



            }

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


    public class ScannedBarcodesTableAdapter :BaseTableAdapter
    {
        
        ScannedProductsDataSet _scannedproductsDataset;
        


        public ScannedBarcodesTableAdapter(ScannedProductsDataSet scannedproductsDataset)
            :base(scannedproductsDataset.ScannedBarcodes)
        {
            _scannedproductsDataset = scannedproductsDataset;
            Init();
        }

        protected override void Init()
        {
            base.Init();
            
            table.AddIndex(
                new System.Data.DataColumn[] {
                _scannedproductsDataset.ScannedBarcodes.Columns["Barcode"],
                _scannedproductsDataset.ScannedBarcodes.Columns["DocId"]
                });
            _fileList = table.FileList.ToArray();
        }
        public void Fill(ScannedProductsDataSet scannedproductsDataset)
        {
            base.Fill(scannedproductsDataset.ScannedBarcodes);
            //try
            //{
            //    table.ReadTableDef();
            //    this.table.Fill(scannedproductsDataset.ScannedBarcodes);
            //}
            //catch { };
            //_fileList = table.FileList.ToArray();
        }
        public void Update(ScannedProductsDataSet scannedproductsDataset)
        {
            //base.Update(scannedproductsDataset.ScannedBarcodes);
            //table.AddIndex(
            //            new System.Data.DataColumn[] {
            //            _scannedproductsDataset.ScannedBarcodes.Columns["Barcode"],
            //            _scannedproductsDataset.ScannedBarcodes.Columns["DocId"]
            //        });
            //_fileList = table.FileList.ToArray();
            base.Update(scannedproductsDataset.ScannedBarcodes);

            //lock (table)
            //{
            //    if (table == null)
            //    {
            //        table = new FamilTsdDB.DataTable(scannedproductsDataset.ScannedBarcodes);
            //        table.AddIndex(
            //            new System.Data.DataColumn[] {
            //            _scannedproductsDataset.ScannedBarcodes.Columns["Barcode"],
            //            _scannedproductsDataset.ScannedBarcodes.Columns["DocId"]
            //        });
            //    }
            //    table.Write();
            //    _fileList = table.FileList.ToArray();
            //}
        }

        public ScannedProductsDataSet.ScannedBarcodesRow[] GetDataByBarcode(System.Int64 barcode, string docId)
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

        public override void Clean()
        {
            base.Clean();
            try
            {
                table.Dispose();
                foreach (string fileName in _fileList)
                    System.IO.File.Delete(fileName);
            }
            catch { }
            

            Init();
        }
        
    }

}