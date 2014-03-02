namespace TSDServer
{


    public partial class ProductsDataSet
    {
        /*
        public void CleanDatabase()
        {
            using (System.Data.SqlServerCe.SqlCeConnection conn =
                new System.Data.SqlServerCe.SqlCeConnection(
                    Properties.Settings.Default.ProductsConnectionString))
            {
                conn.Open();

                using (System.Data.SqlServerCe.SqlCeCommand cmd
                     = new System.Data.SqlServerCe.SqlCeCommand())
                {
                    cmd.CommandText = "delete from  productsBinTbl";
                    cmd.Connection = conn;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from  productsTbl";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from  DocsBinTbl";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from  DocsTbl";
                    cmd.ExecuteNonQuery();

                }
                this.ProductsTbl.Clear();
                this.ProductsBinTbl.Clear();
                this.DocsBinTbl.Clear();
                this.DocsTbl.Clear();
                this.AcceptChanges();
            }

            using (System.Data.SqlServerCe.SqlCeEngine engine
                 = new System.Data.SqlServerCe.SqlCeEngine(Properties.Settings.Default.ProductsConnectionString))
            {
                engine.Shrink();
            }
            

        }


        public void CleanProducts()
        {
            using (System.Data.SqlServerCe.SqlCeConnection conn =
                new System.Data.SqlServerCe.SqlCeConnection(
                    Properties.Settings.Default.ProductsConnectionString))
            {
                conn.Open();

                using (System.Data.SqlServerCe.SqlCeCommand cmd
                     = new System.Data.SqlServerCe.SqlCeCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "delete from  productsBinTbl";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from  productsTbl";
                    cmd.ExecuteNonQuery();

                }
                this.ProductsTbl.Clear();
                this.ProductsBinTbl.Clear();
                this.AcceptChanges();
            }

            using (System.Data.SqlServerCe.SqlCeEngine engine
                 = new System.Data.SqlServerCe.SqlCeEngine(Properties.Settings.Default.ProductsConnectionString))
            {
                engine.Shrink();
            }


        }


        public void CleanDocs()
        {
            using (System.Data.SqlServerCe.SqlCeConnection conn =
                new System.Data.SqlServerCe.SqlCeConnection(
                    Properties.Settings.Default.ProductsConnectionString))
            {
                conn.Open();

                using (System.Data.SqlServerCe.SqlCeCommand cmd
                     = new System.Data.SqlServerCe.SqlCeCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "delete from  DocsBinTbl";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from  DocsTbl";
                    cmd.ExecuteNonQuery();

                }
                this.DocsBinTbl.Clear();
                this.DocsTbl.Clear();
                this.AcceptChanges();
            }

            using (System.Data.SqlServerCe.SqlCeEngine engine
                 = new System.Data.SqlServerCe.SqlCeEngine(Properties.Settings.Default.ProductsConnectionString))
            {
                engine.Shrink();
            }


        }
        */
        /*public DocsBinTblRow ConvertToBin(DocsTblRow row)
        {
            ProductsDataSet.DocsBinTblRow docRow =
               this.DocsBinTbl.NewDocsBinTblRow();
            docRow.Barcode = row.Barcode;
            byte[] b = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.DocId//docid
                );

            docRow.DocId = new byte[b.Length + 1];
            docRow.DocId[0] = row.DocType;
            System.Array.Copy(b, 0, docRow.DocId, 1, b.Length);
           

                //TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                //row.DocType.ToString("00")+ row.DocId//docid
                //        );
            if (row["Priority"] != System.DBNull.Value && row["WorkMode"] != System.DBNull.Value)
                docRow.Priority = (System.Int16)(row.Priority | (row.WorkMode << 14));

            docRow["Quantity"] = row["Quantity"];
            if (row["RePriceDate"] != System.DBNull.Value)
                docRow.RePriceDate = (short)row.RePriceDate.Subtract(Properties.Settings.Default.BaseDate).Days;
            if (row["ReturnDate"] != System.DBNull.Value)
                docRow.ReturnDate = (short)row.ReturnDate.Subtract(Properties.Settings.Default.BaseDate).Days;
            //int shablon = row.LabelCode ;
            //        shablon = row.LabelCode| (row.MusicCode << 3);
            //        shablon = (row.LabelCode| (row.MusicCode << 3)) | (row.VibroCode << 6);
            docRow.Shablon =
                (row.LabelCode | (row.MusicCode << 3)) | (row.VibroCode << 6);
            if (row["Text1"] != System.DBNull.Value)
                docRow.Text1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.Text1);
            if (row["Text2"] != System.DBNull.Value)
                docRow.Text2 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.Text2);
            if (row["Text3"] != System.DBNull.Value)
                docRow.Text3 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(row.Text3);

            return docRow;
        }

        public DocsTblRow ConvertFromBin(DocsBinTblRow row)
        {
            ProductsDataSet.DocsTblRow docRow =
               this.DocsTbl.NewDocsTblRow();
            docRow.Barcode = row.Barcode;

            byte[] b = new byte[row.DocId.Length - 1];
            System.Array.Copy(row.DocId, 1, b, 0, b.Length);
            
            string s =
                TSDUtils.CustomEncodingClass.Encoding.GetString(b);
            docRow.DocId = TSDUtils.CustomEncodingClass.Encoding.GetString(b);

            docRow.DocType = row.DocId[0];//byte.Parse(new string(
                //new char[]{System.Convert.ToChar(row.DocId[0]),
                //    System.Convert.ToChar(row.DocId[1])}));
                //TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                //row.DocType.ToString("00")+ row.DocId//docid
                //        );
            if (row["Priority"] != System.DBNull.Value)
            {
                docRow.Priority = row.Priority & 8191; //(System.Int16)(row.Priority | (row.WorkMode << 14));
                docRow.WorkMode = (byte)(row.Priority >> 14);
            }
            else
            {
                docRow.Priority = 0; //(System.Int16)(row.Priority | (row.WorkMode << 14));
                docRow.WorkMode = 0;
            }
            docRow.Quantity = (row["Quantity"] == System.DBNull.Value) ? 0 : row.Quantity;
            if (row["RePriceDate"] != System.DBNull.Value)
                docRow.RePriceDate = Properties.Settings.Default.BaseDate.AddDays(row.RePriceDate);
            if (row["ReturnDate"] != System.DBNull.Value)
                docRow.ReturnDate = Properties.Settings.Default.BaseDate.AddDays(row.ReturnDate);

            //    (short)row.RePriceDate.Subtract().Days;
            //docRow.ReturnDate = (short)row.ReturnDate.Subtract(Properties.Settings.Default.BaseDate).Days;
            //int shablon = row.LabelCode ;
            //        shablon = row.LabelCode| (row.MusicCode << 3);
            //        shablon = (row.LabelCode| (row.MusicCode << 3)) | (row.VibroCode << 6);
            int Shablon = (row["Shablon"] == System.DBNull.Value)?0:row.Shablon;
            docRow.VibroCode = (byte)(Shablon >> 6);
            docRow.MusicCode = (byte)((Shablon >> 3) & 7);
            docRow.LabelCode = (byte)(Shablon & 7);

                //(row.LabelCode | (row.MusicCode << 3)) | (row.VibroCode << 6);
            docRow.Text1 = (row["Text1"] == System.DBNull.Value) ? (string.Empty) :
    TSDUtils.CustomEncodingClass.Encoding.GetString(row.Text1);
            //docRow.Text1 = TSDUtils.CustomEncodingClass.Encoding.GetString(row.Text1);
            docRow.Text2 = (row["Text2"] == System.DBNull.Value) ? (string.Empty) :
    TSDUtils.CustomEncodingClass.Encoding.GetString(row.Text2);
            docRow.Text3 = (row["Text3"] == System.DBNull.Value) ? (string.Empty) :
    TSDUtils.CustomEncodingClass.Encoding.GetString(row.Text3);

            return docRow;
        }
    }*/

        public partial class DocsTblDataTable
        {
            public ProductsDataSet.DocsTblRow[] FindByNavCode(string NavCode)
            {

                return (ProductsDataSet.DocsTblRow[])(this.Select(string.Format("NavCode = '{0}'", NavCode)));

            }

            public ProductsDataSet.DocsTblRow[] FindByDocIdAndType(string DocId, byte docType)
            {

                return (ProductsDataSet.DocsTblRow[])(this.Select(string.Format("DocId = '{0}' and DocType={1}", DocId, docType)));

            }
        }

        public partial class ProductsTblDataTable
        {
            public ProductsDataSet.ProductsTblRow[] FindByNavcode(string NavCode)
            {

                return (ProductsDataSet.ProductsTblRow[])(this.Select(string.Format("NavCode = '{0}'", NavCode)));

            }
        }
    }



}
namespace TSDServer.ProductsDataSetTableAdapters
{
    public class DocsTblTableAdapter : BaseTableAdapter
    {

        ProductsDataSet _productsDataset;


        public DocsTblTableAdapter(ProductsDataSet productsDataset)
            : base(productsDataset.DocsTbl)
        {
            _productsDataset = productsDataset;
            Init();
        }
        protected override void Init()
        {
            base.Init();
            //table = new FamilTsdDB.DataTable(_productsDataset.DocsTbl);
            //table.ReadTableDef();
            table.AddIndex(new System.Data.DataColumn[] { _productsDataset.DocsTbl.NavCodeColumn });
            //table.AddIndex(new System.Data.DataColumn[] { _productsDataset.DocsTbl.NavCodeColumn });
            _fileList = table.FileList.ToArray();
        }

        public void Fill(ProductsDataSet productsDataset)
        {
            base.Fill(productsDataset.DocsTbl);
        }

        public void Update(TSDServer.ProductsDataSet productsDataset)
        {
            base.Update(productsDataset.DocsTbl);
        }
        //    if (table == null)
        //        table = new FamilTsdDB.DataTable(productsDataset.DocsTbl);
        //    table.Write();
        //}

        public ProductsDataSet.DocsTblRow[] GetDataByNavCode(string NavCode)
        {
            ProductsDataSet.DocsTblRow[] r = _productsDataset.DocsTbl.FindByNavCode(NavCode);
            if (r != null &&
                r.Length > 0)
                return r;
            else
            {
                System.Data.DataRow[] rows = table.FindByIndexes(1, new object[] { NavCode });
                if (rows != null && rows.Length > 0)
                {
                    r = new ProductsDataSet.DocsTblRow[rows.Length];
                    for (int i = 0; i < r.Length; i++)
                    {
                        r[i] = (ProductsDataSet.DocsTblRow)rows[i];
                        try
                        {
                            _productsDataset.DocsTbl.AddDocsTblRow(r[i]);
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

        public ProductsDataSet.DocsTblRow[] GetDataByNavCodeAndType(string NavCode, byte type)
        {
            System.Collections.Generic.List<ProductsDataSet.DocsTblRow> r1 =
                    new System.Collections.Generic.List<ProductsDataSet.DocsTblRow>();

            ProductsDataSet.DocsTblRow[] r = _productsDataset.DocsTbl.FindByNavCode(NavCode);
            if (r != null &&
                r.Length > 0)
            {
                
                //найдено в кэше
                foreach (ProductsDataSet.DocsTblRow docRow in r)
                {
                    if (docRow.DocType == type)
                        r1.Add(docRow);
                }
                if (r1.Count>0)
                    return r1.ToArray();
            }
             
            
            {
                System.Data.DataRow[] rows = table.FindByIndexes(1, new object[] { NavCode });
                if (rows != null && rows.Length > 0)
                {
                    r = new ProductsDataSet.DocsTblRow[rows.Length];
                    for (int i = 0; i < r.Length; i++)
                    {
                        r[i] = (ProductsDataSet.DocsTblRow)rows[i];
                        try
                        {
                            _productsDataset.DocsTbl.AddDocsTblRow(r[i]);
                        }
                        catch (System.Data.ConstraintException)
                        {
                        }
                    }
                    foreach (ProductsDataSet.DocsTblRow docRow in r)
                    {
                        if (docRow.DocType == type)
                            r1.Add(docRow);
                    }
                    return r1.ToArray();
                }
                else
                    return null;
            }
            return r1.ToArray();
        }

        public ProductsDataSet.DocsTblRow[] GetAllDataByDocIdAndType(string DocId, byte docType)
        {
            ProductsDataSet.DocsTblRow[] r = _productsDataset.DocsTbl.FindByDocIdAndType(DocId, docType);
            if (r != null &&
                r.Length > 0)
                return r;
            else
            {
                System.Data.DataRow[] rows = table.FindAllByPartIndexes(0, //0 индекс - первичный ключ
                    new int[] { 1, 2 }, //индекс состоит из 3х колонок: NavCode|6, DocId|20, DocType|1
                    //ищем по DocId|20, DocType|1
                    new object[] { DocId, docType }, //значения которые ищем
                    _productsDataset.DocsTbl); //таблица куда записывать
                if (rows != null && rows.Length > 0)
                {
                    r = new ProductsDataSet.DocsTblRow[rows.Length];
                    for (int i = 0; i < r.Length; i++)
                    {
                        r[i] = (ProductsDataSet.DocsTblRow)rows[i];
                        try
                        {
                            _productsDataset.DocsTbl.AddDocsTblRow(r[i]);
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
        public ProductsDataSet.DocsTblRow[] GetDataByDocIdAndType(string DocId, byte docType)
        {
            ProductsDataSet.DocsTblRow[] r = _productsDataset.DocsTbl.FindByDocIdAndType(DocId, docType);
            if (r != null &&
                r.Length > 0)
                return r;
            else
            {
                System.Data.DataRow[] rows = table.FindFirstByPartIndexes(0, //0 индекс - первичный ключ
                    new int[] { 1, 2 }, //индекс состоит из 3х колонок: NavCode|6, DocId|20, DocType|1
                    //ищем по DocId|20, DocType|1
                    new object[] { DocId, docType }, //значения которые ищем
                    _productsDataset.DocsTbl); //таблица куда записывать
                if (rows != null && rows.Length > 0)
                {
                    r = new ProductsDataSet.DocsTblRow[rows.Length];
                    for (int i = 0; i < r.Length; i++)
                    {
                        r[i] = (ProductsDataSet.DocsTblRow)rows[i];
                        try
                        {
                            _productsDataset.DocsTbl.AddDocsTblRow(r[i]);
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

        public ProductsDataSet.DocsTblRow GetDataByDocIdNavcodeAndType(string Navcode, string DocId, byte docType)
        {
            ProductsDataSet.DocsTblRow r = _productsDataset.DocsTbl.FindByNavCodeDocIdDocType(Navcode, DocId, docType);
            if (r != null)
                return r;
            else
            {
                System.Data.DataRow rows = table.FindByIndex(0, //0 индекс - первичный ключ
                    //new int[] { 1, 2 }, //индекс состоит из 3х колонок: NavCode|6, DocId|20, DocType|1
                    //ищем по DocId|20, DocType|1
                    new object[] {Navcode, DocId, docType }); //таблица куда записывать
                if (rows != null )
                {
                    try
                    {
                        _productsDataset.DocsTbl.AddDocsTblRow((ProductsDataSet.DocsTblRow)rows);
                        
                    }
                    catch (System.Data.ConstraintException)
                    {
                    }
                    return (ProductsDataSet.DocsTblRow)rows;
                }
                else
                    return null;
            }
        }

        /*
        public void InitPkIndex()
        {
            this.table.ReadAllPKIndex();
        }*/
    }

    public class ProductsTblTableAdapter : BaseTableAdapter
    {

        ProductsDataSet _productsDataset;

        public ProductsTblTableAdapter(ProductsDataSet productsDataset)
            : base(productsDataset.ProductsTbl)
        {
            _productsDataset = productsDataset;
            Init();
        }

        /*        public void Update(ProductsDataSet productsDataset)
                {
                    if (table == null)
                        table = new FamilTsdDB.DataTable(productsDataset.ProductsTbl);
                    table.Write();
                }*/
        public void Update(TSDServer.ProductsDataSet productsDataset)
        {
            base.Update(productsDataset.ProductsTbl);
        }

        protected override void Init()
        {
            base.Init();
            table.AddIndex(new System.Data.DataColumn[] { _productsDataset.ProductsTbl.NavCodeColumn });
            _fileList = table.FileList.ToArray();
        }
        public ProductsDataSet.ProductsTblRow GetDataByBarcode(System.Int64 barcode)
        {
            try
            {
                ProductsDataSet.ProductsTblRow r = _productsDataset.ProductsTbl.FindByBarcode(barcode);
                if (r != null)
                    return r;
                else
                {
                    System.Data.DataRow row = table.FindByPk(new object[] { barcode });
                    if (row != null)
                    {
                        try
                        {
                            _productsDataset.ProductsTbl.AddProductsTblRow((ProductsDataSet.ProductsTblRow)row);
                        }
                        catch (System.Data.ConstraintException)
                        {
                        }
                        return (ProductsDataSet.ProductsTblRow)row;
                    }
                    else
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public ProductsDataSet.ProductsTblRow[] GetDataByNavcode(string navcode)
        {
            try
            {
                ProductsDataSet.ProductsTblRow[] r = _productsDataset.ProductsTbl.FindByNavcode(navcode);
                if (r != null &&
                    r.Length > 0)
                    return r;
                else
                {
                    System.Data.DataRow[] rows = table.FindByIndexes(1, new object[] { navcode });
                    if (rows != null && rows.Length > 0)
                    {
                        r = new ProductsDataSet.ProductsTblRow[rows.Length];
                        for (int i = 0; i < r.Length; i++)
                        {
                            r[i] = (ProductsDataSet.ProductsTblRow)rows[i];
                            try
                            {
                                _productsDataset.ProductsTbl.AddProductsTblRow(r[i]);
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
            catch
            {
                return null;
            }
        }

    }

}