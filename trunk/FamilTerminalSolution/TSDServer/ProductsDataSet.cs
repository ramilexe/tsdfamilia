namespace TSDServer
{
    
    
    public partial class ProductsDataSet {
        
        public void CleanDatabase()
        {
           /* using (System.Data.SqlServerCe.SqlCeConnection conn =
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

                }*/
                this.ProductsTbl.Clear();
               // this.ProductsBinTbl.Clear();
                //this.DocsBinTbl.Clear();
                this.DocsTbl.Clear();
                this.AcceptChanges();
           /* }

            using (System.Data.SqlServerCe.SqlCeEngine engine
                 = new System.Data.SqlServerCe.SqlCeEngine(Properties.Settings.Default.ProductsConnectionString))
            {
                engine.Shrink();
            }*/
            

        }


        public void CleanProducts()
        {
            /*using (System.Data.SqlServerCe.SqlCeConnection conn =
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

                }*/
                this.ProductsTbl.Clear();
                //this.ProductsBinTbl.Clear();
                this.AcceptChanges();
           /* }

            using (System.Data.SqlServerCe.SqlCeEngine engine
                 = new System.Data.SqlServerCe.SqlCeEngine(Properties.Settings.Default.ProductsConnectionString))
            {
                engine.Shrink();
            }*/


        }


        public void CleanDocs()
        {
            /*using (System.Data.SqlServerCe.SqlCeConnection conn =
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

                }*/
               // this.DocsBinTbl.Clear();
                this.DocsTbl.Clear();
                this.AcceptChanges();
            //}
            /*
            using (System.Data.SqlServerCe.SqlCeEngine engine
                 = new System.Data.SqlServerCe.SqlCeEngine(Properties.Settings.Default.ProductsConnectionString))
            {
                engine.Shrink();
            }*/


        }
        /*
        public DocsBinTblRow ConvertToBin(DocsTblRow row)
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
         * 
         */ 
    }
}

namespace TSDServer.ProductsDataSetTableAdapters
{
    
    
    public class DocsTblTableAdapter:System.IDisposable
    {
        public string[] FileList
        {
            get
            {
                return table.FileList.ToArray();
            }

        }
        FamilTsdDB.DataTable table = null;
        public DocsTblTableAdapter(TSDServer.ProductsDataSet productsDataset)
        {
            FamilTsdDB.DataTable.BaseDate = TSDServer.Properties.Settings.Default.BaseDate;
            FamilTsdDB.DataTable.StartupPath = TSDServer.Program.CurrentPath;


            table = new FamilTsdDB.DataTable(productsDataset.DocsTbl);

            table.AddIndex(new System.Data.DataColumn[] { productsDataset.DocsTbl.NavCodeColumn }
                );
            //table.ReadTableDef();
        }
        
        public void Update(TSDServer.ProductsDataSet productsDataset)
        {
            table.AddIndex(new System.Data.DataColumn[] { productsDataset.DocsTbl.NavCodeColumn });

            if (table == null)
                table = new FamilTsdDB.DataTable(productsDataset.DocsTbl);
            table.Write();
        }

        #region IDisposable Members

        public void Dispose()
        {
            table.Dispose();
        }

        #endregion
    }

    public class ProductsTblTableAdapter : System.IDisposable
    {
        public string[] FileList
        {
            get
            {
                return table.FileList.ToArray();
            }

        }
        FamilTsdDB.DataTable table = null;
        public ProductsTblTableAdapter(TSDServer.ProductsDataSet productsDataset)
        {
            FamilTsdDB.DataTable.BaseDate = TSDServer.Properties.Settings.Default.BaseDate;
            FamilTsdDB.DataTable.StartupPath =TSDServer.Program.CurrentPath;

            table = new FamilTsdDB.DataTable(productsDataset.ProductsTbl);
            table.AddIndex(new System.Data.DataColumn[] { productsDataset.ProductsTbl.NavCodeColumn }
                );
            //table.ReadTableDef();
            

        }
        public void Update(TSDServer.ProductsDataSet productsDataset)
        {
            table.AddIndex(new System.Data.DataColumn[] { productsDataset.ProductsTbl.Columns["NavCode"] });

            if (table == null)
                table = new FamilTsdDB.DataTable(productsDataset.ProductsTbl);
            table.Write();
        }

        #region IDisposable Members

        public void Dispose()
        {
            table.Dispose();
        }

        #endregion
    }
}
