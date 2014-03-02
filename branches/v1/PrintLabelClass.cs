using System;

using System.Collections.Generic;
using System.Text;

namespace Familia.TSDClient
{
    public class PrintLabelClass
    {
        BTPrintClass btPrint;
        System.Collections.Generic.Dictionary<uint, byte[]> labelCollection =
            new Dictionary<uint, byte[]>();
        System.Globalization.DateTimeFormatInfo dateFormat =
        new System.Globalization.DateTimeFormatInfo();

        System.Globalization.NumberFormatInfo nfi =
                new System.Globalization.NumberFormatInfo();
        private PrintLabelClass()
        {
            btPrint = BTPrintClass.PrintClass;
            dateFormat.ShortDatePattern = "dd.MM.yyyy";
            dateFormat.DateSeparator = ".";
            nfi.NumberDecimalSeparator = ".";

        }

        static PrintLabelClass _printClass = new PrintLabelClass();
        public static PrintLabelClass Print
        {
            get
            {
                return _printClass;
            }
        }

        /// <summary>
        /// Печать этикетки. На вход - строка данных и код действия
        /// Код этикетки определяется динамически на основе поля в строке данных и кода действия.
        /// </summary>
        /// <param name="datarow">Строка с данными</param>
        /// <param name="code">Код действия</param>
        public void PrintLabel(ProductsDataSet.ProductsTblRow datarow, ProductsDataSet.DocsTblRow docRow)
        {
            string fileContent = string.Empty;
            int fileLength = 0;
            byte[] bArray = null;
            uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(docRow.DocType, (uint)docRow.LabelCode);
            if (labelCollection.ContainsKey(shablonCode))
            {
                bArray = labelCollection[shablonCode];
                fileLength = bArray.Length;
                fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);
            }
            else
            {
                string labelName = string.Format("LABEL_{0}.DEF", shablonCode);
                if (System.IO.File.Exists(System.IO.Path.Combine(Program.StartupPath, "fam_pr.lbl")))
                {
                    using (System.IO.FileStream fs = System.IO.File.OpenRead(System.IO.Path.Combine(Program.StartupPath, "fam_pr.lbl")))
                    {
                        fileLength = (int)fs.Length;
                        bArray = new byte[fileLength];
                        fs.Read(bArray, 0, fileLength);
                        fs.Close();
                    }
                    labelCollection.Add(shablonCode, bArray);
                }

            }
            if (bArray == null)
                return;

            fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);
            btPrint.SetStatusEvent(fileContent);
            /*
            byte[] bArray1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(fileContent);
            int pos = bArray1.Length;

            string barcode = datarow.Barcode.ToString("00000000000000");
            if (fileContent.IndexOf("<<<GOODS_ATTRIBUTE_001>>>") > -1)
                fileContent = fileContent.Replace("<<<GOODS_ATTRIBUTE_001>>>", barcode);

            for (int i = 1; i < datarow.Table.Columns.Count; i++)
            {
                string strAttr = string.Format("<<<GOODS_ATTRIBUTE_{0}>>>", i.ToString("000"));
                if (fileContent.IndexOf(strAttr) > -1)
                    fileContent = fileContent.Replace(strAttr, datarow[i].ToString());
            }
            bArray1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(fileContent);

            byte[] bArray2 = new byte[bArray.Length - pos + bArray1.Length];
            System.Array.Copy(bArray1, 0, bArray2, 0, bArray1.Length);
            System.Array.Copy(bArray, pos, bArray2, bArray1.Length, bArray.Length - pos);
            
            fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);*/
            byte[] bArray2 = ReplaceAttr(bArray, datarow);
            fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
            btPrint.SetStatusEvent(fileContent);
            return;
            if (btPrint.Connected)
            {
                btPrint.Print(/*fileContent*/bArray2);
            }
            {
                
                btPrint.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                if (btPrint.Connected)
                {
                    btPrint.Print(/*fileContent*/bArray2);
                }
                
            }
        }

        /// <summary>
        /// для теста
        /// </summary>
        public void PrintLabel()
        {
            string fileContent = string.Empty;
            int fileLength = 0;
            byte[] bArray = null;
            //uint shablonCode = TSDUtils.ActionCodeDescription.ActionDescription.GetShablon(code, (uint)datarow.Shablon);
            /*if (labelCollection.ContainsKey(shablonCode))
            {
                bArray = labelCollection[shablonCode];
                fileLength = bArray.Length;
                fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);
            }
            else*/
            {
               // string labelName = string.Format("LABEL{0}.lbl", shablonCode);
                if (System.IO.File.Exists(System.IO.Path.Combine(Program.StartupPath, "familia.lbl")))
                {
                    using (System.IO.FileStream fs = System.IO.File.OpenRead(System.IO.Path.Combine(Program.StartupPath, "familia.lbl")))
                    {
                        fileLength = (int)fs.Length;
                        bArray = new byte[fileLength];
                        fs.Read(bArray, 0, fileLength);
                        fs.Close();
                    }
                    //labelCollection.Add(shablonCode, bArray);
                }

            }
            if (bArray == null)
                return;

            fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray);

            byte[] bArray1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(fileContent);
            int pos = bArray1.Length;

            
            bArray1 = TSDUtils.CustomEncodingClass.Encoding.GetBytes(fileContent);

            byte[] bArray2 = new byte[bArray.Length - pos + bArray1.Length];
            System.Array.Copy(bArray1, 0, bArray2, 0, bArray1.Length);
            System.Array.Copy(bArray, pos, bArray2, bArray1.Length, bArray.Length - pos);
            fileContent = TSDUtils.CustomEncodingClass.Encoding.GetString(bArray2);
            if (btPrint.Connected)
            {
                btPrint.Print(fileContent);
            }
            {

                //if (btPrint.IsPrinterFound)

                btPrint.ConnToPrinter(Program.Settings.TypedSettings[0].BTPrinterAddress);
                if (btPrint.Connected)
                {
                    btPrint.Print(fileContent);
                }

            }
        }

        private byte[] ReplaceAttr(byte[] bArray, ProductsDataSet.ProductsTblRow datarow)
        {
            List<byte> tempList = new List<byte>();
            List<byte> outArray = new List<byte>();
            for (int i = 0; i < bArray.Length; i++)
            {
                if (i < bArray.Length + 2 &&
                    bArray[i] == Convert.ToByte('<') &&
                    bArray[i + 1] == Convert.ToByte('<') &&
                    bArray[i + 2] == Convert.ToByte('<'))
                {
                    while (i < bArray.Length &&
                    bArray[i] != Convert.ToByte('>') &&
                    bArray[i - 1] != Convert.ToByte('>') &&
                    bArray[i - 2] != Convert.ToByte('>')

                        )
                    {
                        tempList.Add(bArray[i]);
                        i++;
                    }
                    Byte[] bArrTmp = new byte[tempList.Count];

                    tempList.CopyTo(bArrTmp);
                    tempList.Clear();
                    string atrName = TSDUtils.CustomEncodingClass.Encoding.GetString(bArrTmp).Replace("<","");
                    string atrCode = atrName.Replace("GOODS_ATTRIBUTE_", "");
                    int colId = -1;
                    try
                    {
                        colId = int.Parse(atrCode)+1;
                        //atrName = "Test";
                        if (datarow.Table.Columns[colId].DataType == typeof(string) ||
                            datarow.Table.Columns[colId].DataType == typeof(long) ||
                            datarow.Table.Columns[colId].DataType == typeof(int) ||
                            datarow.Table.Columns[colId].DataType == typeof(byte))
                        {
                            bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                datarow[colId].ToString());
                        }

                        if (datarow.Table.Columns[colId].DataType == typeof(DateTime))
                        {
                            bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                ((DateTime)datarow[colId]).ToString(dateFormat));
                        }
                        else
                            bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(
                                datarow[colId].ToString());

                    }
                    catch { 
                    }

                    

                    outArray.AddRange(bArrTmp);
                    i += 2;
                }
                else
                {
                    outArray.Add(bArray[i]);
                }
            }
            return outArray.ToArray();
        }

    

    }
}
