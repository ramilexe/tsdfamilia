namespace TSDServer {
    
    
    public partial class ProductsDataSet {

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
    }
}
