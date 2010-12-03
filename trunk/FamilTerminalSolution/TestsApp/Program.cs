using System;
using System.Collections.Generic;
using System.Text;

namespace TestsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] bArray = null;
            int fileLength = 0;
            if (System.IO.File.Exists( @"E:\TSD\3\fam_pr.lbl"))
            {
                using (System.IO.FileStream fs = System.IO.File.OpenRead(@"E:\TSD\3\fam_pr.lbl"))
                {
                    fileLength = (int)fs.Length;
                    bArray = new byte[fileLength];
                    fs.Read(bArray, 0, fileLength);
                    fs.Close();
                }


            }

            List<byte> tempList = new List<byte>();
            List<byte> outArray = new List<byte>();
            for (int i = 0; i < bArray.Length; i++)
            {
                if (i < bArray.Length + 2 &&
                    bArray[i] == Convert.ToByte('<') &&
                    bArray[i + 1] == Convert.ToByte('<') &&
                    bArray[i + 2] == Convert.ToByte('<'))
                {
                    while (i < bArray.Length  &&
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

                    atrName = "Test";
                    bArrTmp = TSDUtils.CustomEncodingClass.Encoding.GetBytes(atrName);

                    outArray.AddRange(bArrTmp);
                    i+= 2;
                }
                else
                {
                    outArray.Add(bArray[i]);
                }
            }
            //bArray = new byte[outArray.Count];

            using (System.IO.FileStream fs = System.IO.File.OpenWrite(@"E:\TSD\3\fam_pr1.lbl"))
            {
                //fileLength = (int)fs.Length;
                //bArray = new byte[fileLength];
                fs.Write(outArray.ToArray(), 0, outArray.Count);
                fs.Close();
            }



        }
    }
}
