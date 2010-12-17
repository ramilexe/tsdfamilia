using System;
using System.Collections.Generic;
using System.Text;

namespace FamilTsdDB
{

    public static class Comparator<T> where T : IComparable
    {
        public static int Compare(T t1, T t2)
        {
            int res = t1.CompareTo(t2);
            return res;
        }

    }

    public enum DataColumnTypes
    {
        String,
        Real,
        Byte,
        Short,
        Int,
        BigInt,
        Date,
        ByteArray
    }

    public class DataColumnCollection : IEnumerable<DataColumn>
    {

        private System.Collections.Generic.Dictionary<int, DataColumn> _intCollection =
            new Dictionary<int, DataColumn>();
        private System.Collections.Generic.Dictionary<string, int> _intNames =
            new Dictionary<string, int>();
        int counter = 0;
        public int Count
        {
            get
            {
                return counter;
            }
        }
        public void Add(DataColumn col)
        {
            _intCollection.Add(counter, col);
            _intNames.Add(col.ColumnName, counter);
            counter += 1;
        }

        public DataColumn this[int colId]
        {
            get
            {
                return _intCollection[colId];
            }
        }

        public DataColumn this[string colName]
        {
            get
            {
                return _intCollection[_intNames[colName]];
            }
        }

        public void Clear()
        {
            _intCollection.Clear();
            _intNames.Clear();
        }

        #region IEnumerable<DataColumn> Members

        public IEnumerator<DataColumn> GetEnumerator()
        {
            return _intCollection.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _intCollection.Values.GetEnumerator();
        }

        #endregion
    }

    public class DataRow
    {
        public System.UInt32 RowId = 0;
        public System.UInt16 TotalRowLength = 4; //начальная длина TotalRowLength (2байт)+NullColumnsMask(2байт)
        public System.UInt16 NullColumnsMask = 0;
        public byte[] RowData;
        public DataRowItem[] Items;
        public DataTable Table;
        public DataRow(DataTable table)
        {
            Table = table;


            /*Items = new DataRowItem[Table.Columns.Count];
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new DataRowItem();
                Items[i].

            }*/

        }

        public void AddRange(object[] values)
        {
            TotalRowLength = 4;
            Items = new DataRowItem[values.Length];
            System.Collections.Generic.List<byte> rowDataList =
                new List<byte>();
            rowDataList.AddRange(new byte[] { 0, 0, 0, 0 });

            for (int i = 0; i < values.Length; i++)
            {
                byte isnotnull = (byte)
                    Convert.ToByte(
                    values[i] != null &&
                    values[i] != System.DBNull.Value &&
                    values[i].ToString() != string.Empty);

                NullColumnsMask = (UInt16)(NullColumnsMask | (isnotnull << i));

                Items[i] = new DataRowItem(Table.Columns[i], values[i]);
                Items[i].Column = Table.Columns[i];
                Items[i].ItemId = (ushort)i;
                Items[i].Row = this;

                if (isnotnull == 0)
                {
                    if (Table.Columns[i].AllowDBNull)
                        Items[i].RawData = null;
                    else
                        throw new System.Data.NoNullAllowedException(Table.Columns[i].ColumnName);
                }
                else
                {

                    //Items[i] = new DataRowItem(Table.Columns[i], values[i]);

                    /*if (Table.Columns[i].DataType == DataColumnTypes.String)
                    {
                        byte[] str =
                            TSDUtils.CustomEncodingClass.Encoding.GetBytes(values[i].ToString());

                        Items[i].RawData = new byte[str.Length + 1];
                        Items[i].RawData[0] = (byte)str.Length;
                        Array.Copy(str, 0, Items[i].RawData, 1, str.Length);


                    }
                    if (Table.Columns[i].DataType == DataColumnTypes.BigInt)
                    {
                        Items[i].RawData = BitConverter.GetBytes((Int64)values[i]);
                    }
                    if (Table.Columns[i].DataType == DataColumnTypes.Byte)
                    {
                        Items[i].RawData = BitConverter.GetBytes((Byte)values[i]);
                    }
                    if (Table.Columns[i].DataType == DataColumnTypes.Int)
                    {
                        Items[i].RawData = BitConverter.GetBytes((Int32)values[i]);
                    }
                    if (Table.Columns[i].DataType == DataColumnTypes.Short)
                    {
                        Items[i].RawData = BitConverter.GetBytes((Int16)values[i]);
                    }
                    if (Table.Columns[i].DataType == DataColumnTypes.Real)
                    {
                        Items[i].RawData = BitConverter.GetBytes((Single)values[i]);
                    }
                    if (Table.Columns[i].DataType == DataColumnTypes.Date)
                    {
                        UInt16 valDate = (UInt16)((DateTime)values[i]).Subtract(
                            Properties.Settings.Default.BaseDate).Days;

                        Items[i].RawData = BitConverter.GetBytes((UInt16)valDate);
                    }*/
                    TotalRowLength += (ushort)Items[i].RawData.Length;
                    rowDataList.AddRange(Items[i].RawData);
                }


            }
            RowData = rowDataList.ToArray();
            byte[] bLength = BitConverter.GetBytes(TotalRowLength);
            byte[] bMask = BitConverter.GetBytes(NullColumnsMask);
            RowData[0] = bLength[0];
            RowData[1] = bLength[1];
            RowData[2] = bMask[0];
            RowData[3] = bMask[1];

            //RowData[0] 

        }

        public DataRowItem this[int colId]
        {
            get
            {
                return Items[colId];
            }

        }

        public DataRowItem this[string colName]
        {

            get
            {
                return Items[Table.Columns[colName].ID];
            }

        }

        public static DataRow GetRow(DataTable table, byte[] rawData)
        {

            DataRow r = new DataRow(table);
            byte[] bLength = new byte[2];
            byte[] bMask = new byte[2];
            r.RowData = rawData;
            bLength[0] = r.RowData[0];
            bLength[1] = r.RowData[1];
            bMask[0] = r.RowData[2];
            bMask[1] = r.RowData[3];

            r.TotalRowLength = BitConverter.ToUInt16(bLength, 0);
            r.NullColumnsMask = BitConverter.ToUInt16(bMask, 0);
            //r.TotalRowLength += 2;

            r.Items = new DataRowItem[table.Columns.Count];
            UInt16 offset = 4;

            for (int i = 0; i < r.Items.Length; i++)
            {
                /*r.Items[i] = new DataRowItem();
                r.Items[i].Column = table.Columns[i];
                r.Items[i].ItemId = (UInt16)i;
                r.Items[i].Row = r;
                */

                bool isNotNull = ((r.NullColumnsMask & (1 << i)) != 0);
                if (!isNotNull)

                    r.Items[i] = new DataRowItem(table.Columns[i], null);
                else
                {
                    if (table.Columns[i].DataType == DataColumnTypes.String)
                    {
                        byte strLength = rawData[offset];
                        byte[] str = new byte[strLength + 1];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        //r.Items[i].Data =
                        //TSDUtils.CustomEncodingClass.Encoding.GetString(str);
                        offset += (UInt16)(str.Length);
                        //continue;
                    }
                    if (table.Columns[i].DataType == DataColumnTypes.ByteArray)
                    {
                        byte strLength = rawData[offset];
                        byte[] str = new byte[strLength];
                        Array.Copy(rawData, offset + 1, str, 0, str.Length);
                        //r.Items[i].Data =str;
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        offset += (UInt16)(str.Length + 1);
                        //continue;
                    }

                    if (table.Columns[i].DataType == DataColumnTypes.Date)
                    {
                        byte[] str = new byte[table.Columns[i].ColumnLength];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        //UInt16 valDate = (UInt16)BitConverter.ToUInt16(str, 0);
                        //r.Items[i].Data = DataTable.BaseDate.AddDays(valDate);
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        //TSDServer.Properties.Settings.Default.BaseDate.AddDays(valDate);
                        offset += (UInt16)(str.Length);
                        //continue;
                    }

                    object value = null;

                    if (table.Columns[i].DataType == DataColumnTypes.Byte)
                    {
                        byte[] str = new byte[] { rawData[offset] };
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        //value = rawData[offset];
                        offset += 1;
                    }
                    if (table.Columns[i].DataType == DataColumnTypes.Int)
                    {
                        byte[] str = new byte[table.Columns[i].ColumnLength];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        //value = BitConverter.ToInt32(str,0);
                        offset += (UInt16)(str.Length);
                    }
                    if (table.Columns[i].DataType == DataColumnTypes.BigInt)
                    {
                        byte[] str = new byte[table.Columns[i].ColumnLength];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        //value = BitConverter.ToInt64(str, 0);
                        offset += (UInt16)(str.Length);
                    }
                    if (table.Columns[i].DataType == DataColumnTypes.Short)
                    {
                        byte[] str = new byte[table.Columns[i].ColumnLength];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        //value = BitConverter.ToInt16(str, 0);
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        offset += (UInt16)(str.Length);
                    }
                    if (table.Columns[i].DataType == DataColumnTypes.Real)
                    {
                        byte[] str = new byte[table.Columns[i].ColumnLength];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        //value = BitConverter.ToSingle(str, 0);
                        r.Items[i] = new DataRowItem(table.Columns[i], str);
                        offset += (UInt16)(str.Length);
                    }
                    /*if (table.Columns[i].DataType == DataColumnTypes.Int)
                    {
                        byte[] str = new byte[table.Columns[i].ColumnLength];
                        Array.Copy(rawData, offset, str, 0, str.Length);
                        value = BitConverter.ToInt32(str, 0);
                        offset += (UInt16)(str.Length);
                    }*/
                    //r.Items[i].Data = value;


                }
                r.Items[i].ItemId = (UInt16)i;
                r.Items[i].Row = r;

            }
            return r;


        }
    }

    public class DataRowItem : IComparable
    {
        public DataRow Row;
        public UInt16 ItemId;
        public DataColumn Column;
        public byte[] RawData;
        public bool InPK = false;
        public object Data;

        public DataRowItem()
        {

        }

        public DataRowItem(DataColumn _column, byte[] str)
        {
            Column = _column;
            if (str == null || str.Length == 0)
            {
                return;
            }
            RawData = str;
            switch (_column.DataType)
            {

                case DataColumnTypes.String:
                    {
                        byte[] b = new byte[str.Length - 1];

                        Array.Copy(str, 1, b, 0, b.Length);
                        Data = TSDUtils.CustomEncodingClass.Encoding.GetString(b);
                        break;
                    }
                case DataColumnTypes.ByteArray:
                    {
                        Data = str;
                        break;
                    }

                case DataColumnTypes.Date:
                    {
                        UInt16 valDate = (UInt16)BitConverter.ToUInt16(str, 0);
                        Data = DataTable.BaseDate.AddDays(valDate);
                        break;
                    }



                case DataColumnTypes.Byte:
                    {
                        Data = str[0];
                        break;
                    }
                case DataColumnTypes.Int:
                    {
                        Data = BitConverter.ToInt32(str, 0);
                        break;
                    }
                case DataColumnTypes.BigInt:
                    {
                        Data = BitConverter.ToInt64(str, 0);
                        break;
                    }
                case DataColumnTypes.Short:
                    {
                        Data = BitConverter.ToInt16(str, 0);
                        break;
                    }
                case DataColumnTypes.Real:
                    {
                        Data = BitConverter.ToSingle(str, 0);
                        break;
                    }
            }

        }

        public DataRowItem(DataColumn _column, object value)
        {
            Column = _column;
            if (value == null ||
                value == System.DBNull.Value ||
                value.ToString() == string.Empty)
            {
                RawData = null;
                return;
            }
            Data = value;
            if (_column.DataType == DataColumnTypes.String)
            {
                byte[] str =
                    TSDUtils.CustomEncodingClass.Encoding.GetBytes(value.ToString());

                RawData = new byte[str.Length + 1];
                RawData[0] = (byte)str.Length;
                Array.Copy(str, 0, RawData, 1, str.Length);


            }
            if (_column.DataType == DataColumnTypes.ByteArray)
            {
                byte[] str = (byte[])value;

                RawData = new byte[str.Length + 1];
                RawData[0] = (byte)str.Length;
                Array.Copy(str, 0, RawData, 1, str.Length);

            }
            if (_column.DataType == DataColumnTypes.BigInt)
            {
                RawData = BitConverter.GetBytes((Int64)value);
            }
            if (_column.DataType == DataColumnTypes.Byte)
            {
                RawData = new byte[1];
                RawData[0] = (byte)value;
            }
            if (_column.DataType == DataColumnTypes.Int)
            {
                RawData = BitConverter.GetBytes((Int32)value);
            }
            if (_column.DataType == DataColumnTypes.Short)
            {
                RawData = BitConverter.GetBytes((Int16)value);
            }
            if (_column.DataType == DataColumnTypes.Real)
            {
                RawData = BitConverter.GetBytes((Single)value);
            }
            if (_column.DataType == DataColumnTypes.Date)
            {
                UInt16 valDate = (UInt16)((DateTime)value).Subtract(DataTable.BaseDate).Days;
                //    TSDServer.Properties.Settings.Default.BaseDate).Days;

                RawData = BitConverter.GetBytes((UInt16)valDate);
            }
        }

        public int CompareTo(object obj)
        {
            if (Data == null ||
                obj == null)
                return -1;

            switch (Column.DataType)
            {
                case DataColumnTypes.String:
                    return Comparator<string>.Compare((string)Data, (string)((DataRowItem)obj).Data);
                case DataColumnTypes.BigInt:
                    return Comparator<Int64>.Compare((Int64)Data, (Int64)((DataRowItem)obj).Data);
                case DataColumnTypes.Byte:
                    return Comparator<Byte>.Compare((Byte)Data, (Byte)((DataRowItem)obj).Data);
                case DataColumnTypes.Date:
                    return Comparator<Int16>.Compare((Int16)Data, (Int16)((DataRowItem)obj).Data);
                case DataColumnTypes.Int:
                    return Comparator<Int32>.Compare((Int32)Data, (Int32)((DataRowItem)obj).Data);
                case DataColumnTypes.Short:
                    return Comparator<Int16>.Compare((Int16)Data, (Int16)((DataRowItem)obj).Data);
                case DataColumnTypes.Real:
                    return Comparator<Single>.Compare((Single)Data, (Single)((DataRowItem)obj).Data);
                default:
                    {
                        return Comparator<string>.Compare((string)Data, (string)obj);
                    }
            }

        }
    }

    public class DataColumn
    {
        public string ColumnName;
        public DataColumnTypes DataType;
        public bool AllowDBNull;
        public bool Unique;
        public System.Byte ColumnLength;
        public bool PrimaryKey = false;
        public int ID;
        public override string ToString()
        {
            return String.Format("{0}|{1}|{2}|{3}|{4}|{5}", ColumnName, DataType,
                AllowDBNull, Unique, ColumnLength, PrimaryKey);
        }

        public static DataColumn Parse(string columnDef)
        {
            string[] cols = columnDef.Split('|');
            DataColumn col = new DataColumn();
            col.ColumnName = cols[0];

            col.DataType = (DataColumnTypes)Enum.Parse(typeof(DataColumnTypes), cols[1], true);
            //( ((object)dbt).GetType(), cols[1], true);
            col.AllowDBNull = bool.Parse(cols[2]);
            col.Unique = bool.Parse(cols[3]);
            col.ColumnLength = Byte.Parse(cols[4]);
            col.PrimaryKey = bool.Parse(cols[5]);
            return col;
        }
    }

    public class IndexItem : IDisposable, IComparable
    {
        DataRowItem[] currentIndexItem;
        public DataRowItem this[int i]
        {
            get
            {
                return currentIndexItem[i];
            }
        }
        int _offset = -1;
        public int Offset
        {
            get
            {
                return _offset;
            }
        }
        int length = -1;
        public int Length
        {
            get
            {
                return length;
            }
        }
        private bool _disposed = false;
        byte[] bArray;

        public IndexItem(DataRowItem[] ri, int offset)
        {
            currentIndexItem = ri;
            _offset = offset;

            length = 0;
            for (int i = 0; i < currentIndexItem.Length; i++)
            {
                length += (byte)currentIndexItem[i].Column.ColumnLength;
                if (currentIndexItem[i].Column.DataType == DataColumnTypes.String)
                    length += 1;
            }
            length = length + 4;

        }

        public IndexItem(DataRowItem[] ri)
        {
            currentIndexItem = ri;
            //_offset = offset;

            length = 0;
            for (int i = 0; i < currentIndexItem.Length; i++)
            {
                length += (byte)currentIndexItem[i].Column.ColumnLength;
                if (currentIndexItem[i].Column.DataType == DataColumnTypes.String)
                    length += 1;
            }
            length = length + 4;

        }
        public IndexItem(List<DataColumn> col, byte[] foundedArray) :
            this(col.ToArray(), foundedArray)
        {

        }
        public IndexItem(DataColumn[] col, byte[] foundedArray)
        {
            currentIndexItem = new DataRowItem[col.Length];

            byte pos = 0;
            for (int i = 0; i < col.Length; i++)
            {
                int l = col[i].ColumnLength;
                if (col[i].DataType == DataColumnTypes.String)
                    l += 1;
                byte[] b = new byte[l];

                Array.Copy(foundedArray, pos, b, 0, b.Length);
                currentIndexItem[i] = new DataRowItem(col[i], b);
                pos = (byte)(pos + l);
            }
            byte[] b1 = new byte[4];

            Array.Copy(foundedArray, foundedArray.Length - 4, b1, 0, b1.Length);
            _offset = BitConverter.ToInt32(b1, 0);
            length = foundedArray.Length;



        }
        public byte[] GetBytes()
        {
            bArray = new byte[length];
            byte pos = 0;
            for (int i = 0; i < currentIndexItem.Length; i++)
            {

                if (currentIndexItem[i].RawData != null)
                    Array.Copy(currentIndexItem[i].RawData, 0, bArray, pos, currentIndexItem[i].RawData.Length);

                pos = (byte)(pos + currentIndexItem[i].Column.ColumnLength);
            }
            byte[] a = BitConverter.GetBytes(_offset);

            Array.Copy(a
                , 0
                , bArray
                , length - 4
                , 4);

            return bArray;

        }

        public void Dispose()
        {
            if (!_disposed)
            {

            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            IndexItem ri = (obj as IndexItem);
            if (ri == null)
                return 1;
            if (ri.Length != this.Length)
                return currentIndexItem.Length.CompareTo(ri.Length);

            int res = 0;
            for (int i = 0; i < currentIndexItem.Length; i++)
            {
                res = currentIndexItem[i].CompareTo(ri[i]);
                if (res != 0)
                    return res;

            }
            return res;
        }
    }
    public class Index : IDisposable, IComparable
    {

        public DataTable Table;
        public System.Collections.Generic.Dictionary<DataRowItem[], Int32> keyItems =
            new Dictionary<DataRowItem[], Int32>();

        public System.Collections.Generic.List<IndexItem> keyItems1 =
            new List<IndexItem>();

        public List<DataColumn> IndexColumns = new List<DataColumn>();
        DataRowItem[] currentIndexItem;
        public int indexId = 0;
        string indexFileName = string.Empty;
        System.IO.FileStream fsPk;
        System.IO.BinaryWriter wrt;
        System.IO.BinaryReader rdr;
        public byte IndexLength = 0;
        public int TotalIndexLength = 0;
        private bool _disposed = false;
        private bool _opened = false;
        public static int operationCounter = 0;
        public List<String> FileList = new List<string>();
        int totalIndexes = 0;//(int)(fsPk.Length/item.Length);

        byte[] srchtemplate;
        int indexCounter = 0;
        int maxCounter = 0;
        int minCounter = 0;
        int diff = 0;

        public Index(string fileName, DataTable t)
        {
            Table = t;
            //List<DataColumn> idxCol = new List<DataColumn>();
            using (System.IO.StreamReader rdr =
            new System.IO.StreamReader(fileName))
            {
                string column = string.Empty;
                while ((column = rdr.ReadLine()) != null)
                {
                    string[] c = column.Split('|');
                    IndexColumns.Add(t.Columns[c[0]]);
                }
            }


            currentIndexItem = new DataRowItem[IndexColumns.Count];
            //indexId = Table.indexes.Count;
            indexFileName = fileName.Replace(".idf", ".idx");
            indexId =
                int.Parse(System.IO.Path.GetFileNameWithoutExtension(fileName).Replace(
                    string.Format("{0}_", Table.TableName), ""));
            //string.Format("{0}\\{1}_{2}.idx", DataTable.StartupPath, Table.TableName, indexId);

            for (int i = 0; i < IndexColumns.Count; i++)
            {
                IndexLength += IndexColumns[i].ColumnLength;
                if (IndexColumns[i].DataType == DataColumnTypes.String)
                    IndexLength += 1;
            }
            IndexLength += 4;
            srchtemplate = new byte[IndexLength];
            FileList.Add(fileName);
            FileList.Add(indexFileName);
        }

        public Index(DataTable t, List<DataColumn> indexColumns)
        {
            Table = t;
            IndexColumns = indexColumns;
            currentIndexItem = new DataRowItem[IndexColumns.Count];
            indexId = Table.indexes.Count;
            indexFileName = string.Format("{0}\\{1}_{2}.idx", DataTable.StartupPath, Table.TableName, indexId);

            for (int i = 0; i < IndexColumns.Count; i++)
            {
                IndexLength += IndexColumns[i].ColumnLength;
                if (IndexColumns[i].DataType == DataColumnTypes.String)
                    IndexLength += 1;
            }
            IndexLength += 4;
            srchtemplate = new byte[IndexLength];
            FileList.Add(indexFileName.Replace(".idx", ".idf"));
            FileList.Add(indexFileName);
        }


        public void AddIndexItem(Int32 offset, DataRow row)
        {
            currentIndexItem = new DataRowItem[IndexColumns.Count];
            for (int i = 0; i < IndexColumns.Count; i++)
            {
                currentIndexItem[i] = row[IndexColumns[i].ID];

            }
            //keyItems.Add(currentIndexItem, offset);
            keyItems1.Add(new IndexItem(currentIndexItem, offset));
        }

        public void Write()
        {

            keyItems1.Sort();

            using (System.IO.StreamWriter wrt = new System.IO.StreamWriter
            (indexFileName.Replace(".idx", ".idf"), false))
            {
                for (int i = 0; i < IndexColumns.Count; i++)
                {
                    wrt.WriteLine(string.Format("{0}|{1}", IndexColumns[i].ColumnName, IndexColumns[i].ColumnLength));
                }
                wrt.Flush();
                wrt.Close();
            }


            using (fsPk =
                   new System.IO.FileStream(
                       indexFileName,
                       System.IO.FileMode.Create))
            {
                using (wrt =
                    new System.IO.BinaryWriter(fsPk))
                {

                    /*foreach (DataRowItem[] item in keyItems.Keys)
                    {
                       System.Collections.Generic.List<byte> itemData =
                       new List<byte>();

                        for (int i = 0; i < item.Length; i++)
                        {
                            itemData.Add((byte)item[i].RawData.Length);
                            itemData.AddRange(item[i].RawData);

                        }
                        //offset
                        itemData.AddRange(BitConverter.GetBytes(keyItems[item]));//always 8 byte
                        itemData.Add(0x0A);
                        byte[] a = itemData.ToArray();
                        //byte[] b = new byte[a.Length + 1];
                        wrt.Write((byte)a.Length);
                        wrt.Write(a);
                    }*/
                    foreach (IndexItem i in keyItems1)
                    {
                        wrt.Write(i.GetBytes());
                    }
                    wrt.Flush();
                    wrt.Close();
                }
            }
        }

        public void OpenIndex()
        {
            if (!_opened)
            {
                fsPk =
                       new System.IO.FileStream(
                           indexFileName,
                           System.IO.FileMode.Open);
                rdr = new System.IO.BinaryReader(fsPk);
                _opened = true;
                TotalIndexLength = (int)(fsPk.Length);
                totalIndexes = (int)(fsPk.Length / IndexLength);
                indexCounter = 0;
                maxCounter = totalIndexes;

            }
        }
        public void CloseIndex()
        {
            if (rdr != null && _opened)
            {
                rdr.Close();
                rdr = null;
            }

            if (fsPk != null && _opened)
            {
                fsPk.Close();
                fsPk = null;

            }


            _opened = false;
        }

        public Int32 FindIndex(IndexItem item)
        {
            //totalIndexes = (int)(fsPk.Length / item.Length);
            int position = IndexLength * totalIndexes / 2;

            return FindIndex(item, ref position);
        }

        public Int32 FindIndex(IndexItem item, ref int indexPosition)
        {
            operationCounter = 0;
            indexCounter = totalIndexes / 2;
            indexPosition = IndexLength * indexCounter;

            if (fsPk.Position != indexPosition)
                indexPosition = (int)fsPk.Seek(indexPosition - fsPk.Position, System.IO.SeekOrigin.Current);
            maxCounter = totalIndexes;
            minCounter = 0;

            //indexCounter = indexPosition / IndexLength;            
            while (true)
            {
                operationCounter++;
                int readed = rdr.Read(srchtemplate, 0, srchtemplate.Length);
                indexPosition += readed;
                //indexPosition = (int)fsPk.Position;
                //indexCounter = indexPosition / IndexLength;
                if (readed == 0)
                    return -1;

                if (readed >= IndexLength)
                {
                    IndexItem itemI = new IndexItem(IndexColumns.ToArray(), srchtemplate);

                    int res = item.CompareTo(itemI);


                    if (res == 0)
                    {
                        return itemI.Offset;
                    }
                    else
                    {
                        if (minCounter == maxCounter)//not found
                            return -1;
                        if (maxCounter <= 3)
                            return FindIndexReverse(item, ref indexPosition);

                        if (minCounter >= totalIndexes - 3)
                            return FindIndexDirect(item, ref indexPosition);


                        if (res < 0)
                        {
                            maxCounter = indexCounter;
                            /*if ((maxCounter - minCounter) == 1)
                            {
                                indexCounter = indexCounter - 1;
                            }
                            else*/
                            indexCounter = indexCounter - (maxCounter - minCounter) / 2;
                        }
                        else
                        {
                            minCounter = indexCounter;
                            /*if ((maxCounter - minCounter)==1)
                                indexCounter = indexCounter + 1;
                            else*/
                            indexCounter = indexCounter + (maxCounter - minCounter) / 2;
                        }


                    }

                }
                //diff = indexPosition;
                indexPosition = indexCounter * item.Length;
                diff = indexPosition - (int)fsPk.Position;
                if (diff == -1 * readed)
                    return -1;
                indexPosition = (int)fsPk.Seek(diff, System.IO.SeekOrigin.Current);
            }
        }

        public Int32 FindIndex(DataRowItem[] dataitem)
        {
            IndexItem item = new IndexItem(dataitem);
            return FindIndex(item);
        }

        public Int32 FindIndex(DataRowItem[] item, ref int beginOffset)
        {
            return FindIndex(new IndexItem(item), ref beginOffset);
        }

        public Int32 FindIndexDirect(IndexItem item, ref int indexPosition)
        {
            operationCounter = 0;
            if (fsPk.Position != indexPosition)
                indexPosition = (int)fsPk.Seek(indexPosition - fsPk.Position, System.IO.SeekOrigin.Current);


            indexCounter = indexPosition / IndexLength;

            while (indexPosition <= fsPk.Length)
            {
                operationCounter++;
                int readed = rdr.Read(srchtemplate, 0, srchtemplate.Length);
                indexPosition += readed;

                if (readed == 0)
                    return -1;

                if (readed >= IndexLength)
                {
                    IndexItem itemI = new IndexItem(IndexColumns.ToArray(), srchtemplate);
                    int res = item.CompareTo(itemI);
                    if (res == 0)
                    {
                        return itemI.Offset;
                    }
                }
                else
                    return -1;

            }
            return -1;
        }

        public Int32 FindIndexReverse(IndexItem item, ref int indexPosition)
        {
            operationCounter = 0;
            if (fsPk.Position != indexPosition)
                indexPosition = (int)fsPk.Seek(indexPosition - fsPk.Position, System.IO.SeekOrigin.Current);


            indexCounter = indexPosition / IndexLength;
            while (indexPosition >= 0)
            {
                operationCounter++;
                int readed = rdr.Read(srchtemplate, 0, srchtemplate.Length);



                if (readed == 0)
                    return -1;

                if (readed >= IndexLength)
                {
                    IndexItem itemI = new IndexItem(IndexColumns.ToArray(), srchtemplate);
                    int res = item.CompareTo(itemI);
                    if (res == 0)
                    {
                        indexPosition = indexPosition - readed;
                        return itemI.Offset;
                    }
                    else
                        indexPosition = indexPosition - 2 * readed;
                }
                else
                    return -1;

                //indexPosition = indexPosition - 2 * readed;
                if (fsPk.Position > srchtemplate.Length)
                    indexPosition = (int)fsPk.Seek(-2 * readed, System.IO.SeekOrigin.Current);
                else
                    return -1;
                //if (fsPk.Position > srchtemplate.Length)
                //    indexPosition = (int)fsPk.Seek(-2 * readed, System.IO.SeekOrigin.Current);
                //else
                //    return -1;

            }
            return -1;
        }

        public IEnumerable<Int32> FindIndexes(IndexItem item)
        {
            Int32 indexPosition = IndexLength * totalIndexes / 2; ;
            Int32 basePosition = 0;
            int offset = -1;

            offset = FindIndex(item, ref indexPosition);
            basePosition = indexPosition;
            yield return offset;
            if (offset != -1)
            {
                indexCounter = indexPosition / IndexLength;

                while (indexPosition < fsPk.Length && offset >= 0)
                {
                    operationCounter++;
                    int readed = rdr.Read(srchtemplate, 0, srchtemplate.Length);
                    indexPosition += readed;

                    if (readed == 0)
                        break;

                    if (readed >= IndexLength)
                    {
                        IndexItem itemI = new IndexItem(IndexColumns.ToArray(), srchtemplate);
                        int res = item.CompareTo(itemI);
                        if (res == 0)
                        {
                            offset = itemI.Offset;
                            yield return offset;
                        }
                        else
                        {
                            offset = -1;
                            break;
                        }
                    }
                    else
                    {
                        offset = -1;
                        break;
                    }
                }
                offset = 0;
                if (basePosition > 0)
                {
                    indexPosition = (int)fsPk.Seek(basePosition - indexPosition - 2 * IndexLength, System.IO.SeekOrigin.Current); ;

                    while (indexPosition > 0 && offset >= 0)
                    {
                        operationCounter++;
                        int readed = rdr.Read(srchtemplate, 0, srchtemplate.Length);
                        //position -= readed;

                        indexPosition = (int)fsPk.Seek(-2 * readed, System.IO.SeekOrigin.Current);

                        if (readed == 0)
                        {
                            offset = -1;
                            break;
                        }

                        if (readed >= IndexLength)
                        {
                            IndexItem itemI = new IndexItem(IndexColumns.ToArray(), srchtemplate);
                            int res = item.CompareTo(itemI);
                            if (res == 0)
                            {

                                offset = itemI.Offset;
                                yield return offset;
                            }
                            else
                            {
                                offset = -1;
                                break;
                            }
                        }
                        else
                        {
                            offset = -1;
                            break;
                        }
                    }
                }
            }
        }

        public IEnumerable<Int32> FindIndexes()
        {
            Int32 indexPosition = 0;
            operationCounter = 0;
            indexPosition = (int)fsPk.Seek(0, System.IO.SeekOrigin.Begin);

            indexCounter = indexPosition / IndexLength;

            while (indexPosition <= fsPk.Length)
            {
                operationCounter++;
                int readed = rdr.Read(srchtemplate, 0, srchtemplate.Length);
                indexPosition += readed;
                indexCounter++;

                if (readed == 0)
                    break;

                if (readed >= IndexLength)
                {
                    IndexItem itemI = new IndexItem(IndexColumns.ToArray(), srchtemplate);
                    yield return itemI.Offset;
                }
                else
                    break;

            }

        }

        public void Clear()
        {
            keyItems.Clear();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
                return;
            else
            {
                IndexColumns.Clear();
                if (_opened)
                    CloseIndex();
#if  WindowsCE

#else
                if (fsPk != null)
                    fsPk.Dispose();
#endif

                _disposed = true;
            }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            Index idx = null;
            if ((idx = (obj as Index)) != null)
            {
                if (IndexColumns.Count != idx.IndexColumns.Count)
                    return -1;
                for (int i = 0; i < IndexColumns.Count; i++)
                {
                    if (idx.IndexColumns[i].ColumnName != this.IndexColumns[i].ColumnName)
                        return -1;
                }
                return 0;

            }
            else
            {
                return -1;
            }
        }

        #endregion
    }

    public class DataTable : System.IDisposable
    {
        public static DateTime BaseDate;
        public static string StartupPath;
        public DataColumnCollection Columns =
            new DataColumnCollection();

        public System.Collections.Generic.List<DataRow>
            RowsCollection = new List<DataRow>();

        public System.Collections.Generic.List<DataColumn> PrimaryKey =
            new List<DataColumn>();
        public System.Collections.Generic.List<Index> indexes =
            new List<Index>();

        System.Data.DataTable _data;

        public string TableName;
        public List<string> FileList = new List<string>();
        private bool _Disposed = false;

        public DataTable(System.Data.DataTable data)
        {
            _data = data;
            TableName = data.TableName;
            int colCounter = 0;
            #region Columns Mapping
            foreach (System.Data.DataColumn col in data.Columns)
            {
                DataColumn myCol =
                    new DataColumn();
                myCol.ID = colCounter++;

                if (col.DataType == typeof(string))
                {
                    myCol.DataType = DataColumnTypes.String;
                    myCol.ColumnLength = (byte)(col.MaxLength);
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(byte[]))
                {
                    myCol.DataType = DataColumnTypes.ByteArray;
                    myCol.ColumnLength = (byte)255;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(Int64))
                {
                    myCol.DataType = DataColumnTypes.BigInt;
                    myCol.ColumnLength = 8;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(Int32))
                {
                    myCol.DataType = DataColumnTypes.Int;
                    myCol.ColumnLength = 4;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(Int16))
                {
                    myCol.DataType = DataColumnTypes.Short;
                    myCol.ColumnLength = 2;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(Byte))
                {
                    myCol.DataType = DataColumnTypes.Byte;
                    myCol.ColumnLength = 1;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(DateTime))
                {
                    myCol.DataType = DataColumnTypes.Date;
                    myCol.ColumnLength = 2;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                if (col.DataType == typeof(Decimal) ||
                    col.DataType == typeof(Double) ||
                    col.DataType == typeof(Single)
                    )
                {
                    myCol.DataType = DataColumnTypes.Real;
                    myCol.ColumnLength = 4;
                    myCol.ColumnName = col.ColumnName;
                    myCol.AllowDBNull = col.AllowDBNull;
                    myCol.Unique = col.Unique;
                }
                Columns.Add(myCol);

            }
            #endregion



            foreach (System.Data.DataColumn col in data.PrimaryKey)
            {
                PrimaryKey.Add(Columns[col.ColumnName]);
                Columns[col.ColumnName].PrimaryKey = true;
            }

            FileList.Add(string.Format("{0}\\{1}.col", DataTable.StartupPath, this.TableName));
            FileList.Add(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName));
            Index idx = new Index(this, PrimaryKey);
            idx.indexId = indexes.Count;
            indexes.Add(idx);
            FileList.AddRange(idx.FileList.ToArray());
        }
        public DataTable(string tableName)
        {
            TableName = tableName;
            FileList.Add(string.Format("{0}\\{1}.col", DataTable.StartupPath, this.TableName));
            FileList.Add(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName));
        }

        public void ReadTableDef()
        {
            try
            {
                /*if (System.IO.File.Exists(string.Format("{0}\\{1}.col", DataTable.StartupPath, this.TableName)))
                {
                    using (System.IO.StreamReader rdr =
                       new System.IO.StreamReader(string.Format("{0}\\{1}.col", DataTable.StartupPath, this.TableName)))
                    {
                        string column = string.Empty;
                        while ((column = rdr.ReadLine()) != null)
                        {
                            Columns.Add(DataColumn.Parse(column));
                        }
                    }

                    foreach (DataColumn col in Columns)
                    {
                        if (col.PrimaryKey)
                            this.PrimaryKey.Add(col);
                    }
                }*/

                string indexesNames = string.Format("{0}_*.idf", this.TableName);
                string[] indexFiles = System.IO.Directory.GetFiles(DataTable.StartupPath, indexesNames);
                foreach (string s in indexFiles)
                {
                    /* List<DataColumn> idxCol = new List<DataColumn>();
                     using (System.IO.StreamReader rdr =
                     new System.IO.StreamReader(s))
                     {
                         string column = string.Empty;
                         while ((column = rdr.ReadLine()) != null)
                         {
                             string[] c = column.Split('|');
                             idxCol.Add(Columns[c[0]]);
                         }
                     }*/
                    //indexes.Clear();
                    Index idx = new Index(s, this);

                    bool fouded = false;
                    foreach (Index i in this.indexes)
                    {
                        if (i.CompareTo(idx) == 0)
                        {
                            fouded = true; //contains index
                            break;
                        }
                    }
                    if (!fouded)
                        indexes.Add(idx);
                }
            }
            catch { }
            foreach (Index idx in indexes)
            {
                FileList.AddRange(idx.FileList.ToArray());
            }

        }

        public void Write()
        {

            using (System.IO.StreamWriter wrt =
                new System.IO.StreamWriter(string.Format("{0}\\{1}.col", DataTable.StartupPath, this.TableName)))
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    wrt.WriteLine(Columns[i].ToString());
                }
                wrt.Flush();
                wrt.Close();

            }

            Int32 offset = 0;

            using (System.IO.FileStream fs =
                new System.IO.FileStream(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName), System.IO.FileMode.Create))
            {



                using (System.IO.BinaryWriter wrt =
                    new System.IO.BinaryWriter(fs))
                {
                    DataRow r = null;
                    for (int i = 0; i < _data.Rows.Count; i++)
                    {
                        r = new DataRow(this);
                        r.AddRange(_data.Rows[i].ItemArray);
                        RowsCollection.Add(r);
                        wrt.Write(r.RowData);
                        foreach (Index idx in indexes)
                        {
                            idx.AddIndexItem(offset, r);
                        }
                        offset += r.RowData.Length;
                    }
                    wrt.Flush();


                }




            }

            foreach (Index idx in indexes)
            {
                idx.Write();

            }


        }

        public void AddIndex(System.Data.DataColumn[] columns)
        {
            List<DataColumn> c = new List<DataColumn>();
            foreach (System.Data.DataColumn col in columns)
            {
                c.Add(Columns[col.ColumnName]);
            }
            Index idx = new Index(this, c);
            idx.indexId = indexes.Count;
            foreach (Index i in this.indexes)
            {
                if (i.CompareTo(idx) == 0)
                    return; //contains index
            }
            indexes.Add(idx);
            FileList.AddRange(idx.FileList.ToArray());

        }

        public System.Data.DataRow
            FindByPk(Object[] pkValues)
        {

            return FindByIndex(0, pkValues);
        }

        public System.Data.DataRow
            FindByPkDirectScan(int indexId, Object[] pkValues)
        {
            return FindByIndexDirectScan(0, pkValues);
        }

        public System.Data.DataRow
            FindByIndexDirectScan(int indexId, Object[] pkValues)
        {
            if (pkValues.Length != indexes[indexId].IndexColumns.Count)
                throw new System.Data.DataException();

            System.Data.DataRow out_row = _data.NewRow();


            DataRowItem[] item =
                new DataRowItem[indexes[indexId].IndexColumns.Count];

            for (int i = 0; i < pkValues.Length; i++)
            {

                item[i] = new DataRowItem(indexes[indexId].IndexColumns[i], pkValues[i]);
            }

            indexes[indexId].OpenIndex();
            int position = 0;
            Int32 offset = indexes[indexId].FindIndexDirect(new IndexItem(item), ref position);
            try
            {
                if (offset >= 0)
                {
                    using (System.IO.FileStream fs =
                        new System.IO.FileStream(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName), System.IO.FileMode.Open))
                    {
                        fs.Seek(offset, System.IO.SeekOrigin.Begin);

                        int readed = 0;

                        byte[] bLength = new byte[2];
                        fs.Read(bLength, 0, 2);
                        UInt16 length = BitConverter.ToUInt16(bLength, 0);
                        byte[] rawData = new byte[length];
                        fs.Seek(-2, System.IO.SeekOrigin.Current);
                        fs.Read(rawData, 0, rawData.Length);

                        DataRow r = DataRow.GetRow(this, rawData);

                        for (int i = 0; i < _data.Columns.Count; i++)
                        {
                            if (r[i].Data != null)
                                out_row[i] = r[i].Data;
                            else
                                out_row[i] = System.DBNull.Value;
                        }
                        return out_row;
                    }
                }
                else
                    return null;
            }
            finally
            {
                //indexes[0].CloseIndex();
            }

        }

        public System.Data.DataRow
                FindByPkReverseScan(int indexId, Object[] pkValues)
        {
            if (pkValues.Length != indexes[indexId].IndexColumns.Count)
                throw new System.Data.DataException();

            System.Data.DataRow out_row = _data.NewRow();


            DataRowItem[] item =
                new DataRowItem[indexes[indexId].IndexColumns.Count];

            for (int i = 0; i < pkValues.Length; i++)
            {

                item[i] = new DataRowItem(indexes[indexId].IndexColumns[i], pkValues[i]);
            }

            indexes[indexId].OpenIndex();
            int position = 1000 * indexes[indexId].TotalIndexLength;
            Int32 offset = indexes[indexId].FindIndexReverse(new IndexItem(item), ref position);
            try
            {
                if (offset >= 0)
                {
                    using (System.IO.FileStream fs =
                        new System.IO.FileStream(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName), System.IO.FileMode.Open))
                    {
                        fs.Seek(offset, System.IO.SeekOrigin.Begin);

                        int readed = 0;

                        byte[] bLength = new byte[2];
                        fs.Read(bLength, 0, 2);
                        UInt16 length = BitConverter.ToUInt16(bLength, 0);
                        byte[] rawData = new byte[length];
                        fs.Seek(-2, System.IO.SeekOrigin.Current);
                        fs.Read(rawData, 0, rawData.Length);

                        DataRow r = DataRow.GetRow(this, rawData);

                        for (int i = 0; i < _data.Columns.Count; i++)
                        {
                            if (r[i].Data != null)
                                out_row[i] = r[i].Data;
                            else
                                out_row[i] = System.DBNull.Value;
                        }
                        return out_row;
                    }
                }
                else
                    return null;
            }
            finally
            {
                //indexes[0].CloseIndex();
            }

        }

        public System.Data.DataRow
            FindByIndex(int indexId, Object[] pkValues)
        {
            if (pkValues.Length != indexes[indexId].IndexColumns.Count)
                throw new System.Data.DataException();

            System.Data.DataRow out_row = _data.NewRow();


            DataRowItem[] item =
                new DataRowItem[indexes[indexId].IndexColumns.Count];

            for (int i = 0; i < pkValues.Length; i++)
            {

                item[i] = new DataRowItem(indexes[indexId].IndexColumns[i], pkValues[i]);
            }

            indexes[indexId].OpenIndex();
            Int32 offset = indexes[indexId].FindIndex(new IndexItem(item));
            try
            {
                if (offset >= 0)
                {
                    using (System.IO.FileStream fs =
                        new System.IO.FileStream(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName), System.IO.FileMode.Open))
                    {
                        fs.Seek(offset, System.IO.SeekOrigin.Begin);

                        int readed = 0;

                        byte[] bLength = new byte[2];
                        fs.Read(bLength, 0, 2);
                        UInt16 length = BitConverter.ToUInt16(bLength, 0);
                        byte[] rawData = new byte[length];
                        fs.Seek(-2, System.IO.SeekOrigin.Current);
                        fs.Read(rawData, 0, rawData.Length);

                        DataRow r = DataRow.GetRow(this, rawData);

                        for (int i = 0; i < _data.Columns.Count; i++)
                        {
                            if (r[i].Data != null)
                                out_row[i] = r[i].Data;
                            else
                                out_row[i] = System.DBNull.Value;
                        }
                        return out_row;
                    }
                }
                else
                    return null;
            }
            finally
            {
                //indexes[0].CloseIndex();
            }


        }

        public System.Data.DataRow[]
            FindByIndexes(int indexId, Object[] pkValues)
        {
            if (pkValues.Length != indexes[indexId].IndexColumns.Count)
                throw new System.Data.DataException();

            List<System.Data.DataRow> Rows =
                new List<System.Data.DataRow>();



            DataRowItem[] item =
                new DataRowItem[indexes[indexId].IndexColumns.Count];

            for (int i = 0; i < pkValues.Length; i++)
            {

                item[i] = new DataRowItem(indexes[indexId].IndexColumns[i], pkValues[i]);
            }

            indexes[indexId].OpenIndex();
            //Int32 offset = 0;
            foreach (Int32 offset in indexes[indexId].FindIndexes(new IndexItem(item)))
            {
                try
                {
                    if (offset >= 0)
                    {
                        System.Data.DataRow out_row = _data.NewRow();

                        using (System.IO.FileStream fs =
                            new System.IO.FileStream(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName), System.IO.FileMode.Open))
                        {
                            fs.Seek(offset, System.IO.SeekOrigin.Begin);

                            int readed = 0;

                            byte[] bLength = new byte[2];
                            fs.Read(bLength, 0, 2);
                            UInt16 length = BitConverter.ToUInt16(bLength, 0);
                            byte[] rawData = new byte[length];
                            fs.Seek(-2, System.IO.SeekOrigin.Current);
                            fs.Read(rawData, 0, rawData.Length);

                            DataRow r = DataRow.GetRow(this, rawData);

                            for (int i = 0; i < _data.Columns.Count; i++)
                            {
                                if (r[i].Data != null)
                                    out_row[i] = r[i].Data;
                                else
                                    out_row[i] = System.DBNull.Value;
                            }
                            Rows.Add(out_row);
                        }
                    }
                }
                finally
                {
                    //indexes[0].CloseIndex();
                }
            }
            return Rows.ToArray();


        }

        public void Fill(System.Data.DataTable data)
        {
            ReadTableDef();
            foreach (Int32 offset in indexes[0].FindIndexes())
            {
                try
                {
                    if (offset >= 0)
                    {
                        System.Data.DataRow out_row = data.NewRow();

                        using (System.IO.FileStream fs =
                            new System.IO.FileStream(string.Format("{0}\\{1}.db", DataTable.StartupPath, this.TableName), System.IO.FileMode.Open))
                        {
                            fs.Seek(offset, System.IO.SeekOrigin.Begin);

                            int readed = 0;

                            byte[] bLength = new byte[2];
                            fs.Read(bLength, 0, 2);
                            UInt16 length = BitConverter.ToUInt16(bLength, 0);
                            byte[] rawData = new byte[length];
                            fs.Seek(-2, System.IO.SeekOrigin.Current);
                            fs.Read(rawData, 0, rawData.Length);

                            DataRow r = DataRow.GetRow(this, rawData);

                            for (int i = 0; i < _data.Columns.Count; i++)
                            {
                                if (r[i].Data != null)
                                    out_row[i] = r[i].Data;
                                else
                                    out_row[i] = System.DBNull.Value;
                            }
                            data.Rows.Add(out_row);
                        }
                    }
                }
                finally
                {
                    //indexes[0].CloseIndex();
                }
            }

        }
        #region IDisposable Members

        public void Dispose()
        {
            if (_Disposed)
                return;
            else
            {
                Columns.Clear();
                RowsCollection.Clear();
                PrimaryKey.Clear();
                foreach (Index idx in indexes)
                {
                    idx.Dispose();
                }
                indexes.Clear();

                _Disposed = true;
            }
        }

        #endregion

    }
    
    
}
