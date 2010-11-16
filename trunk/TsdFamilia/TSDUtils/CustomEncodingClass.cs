using System;
using System.Collections.Generic;
using System.Text;

namespace TSDServer
{
    public class CustomEncodingClass
    {
        string sourceString =
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯабвгдеёжзийклмнопрстуфхцчшщьыъэюя№";
        byte[] sourceArray;
        char[] sourceCharArray;
        Dictionary<char, byte> symPos = new Dictionary<char, byte>();
        public CustomEncodingClass()
        {
            Prepare();
        }
        public void Prepare()
        {
            sourceArray = new byte[255];
            //sourceCharArray = sourceString.ToCharArray();
            sourceCharArray = new char[255];
            //string test = "";
            int lastSymbol = 0;
            for (int i = 0; i < 255; i++)
            {
                sourceArray[i ] = (byte)i;
                char c = Convert.ToChar(i);
                sourceCharArray[i] = c;
                //test = test + c;
                symPos.Add(c, (byte)i);
                if (c == '~')
                {
                    lastSymbol = i;
                    break;
                }

            }

            for (int i = lastSymbol+1; i < lastSymbol+sourceString.Length+1; i++)
            {
                sourceArray[i] = (byte)i;
                char c = sourceString[i - lastSymbol-1];
                sourceCharArray[i] = c;
                symPos.Add(c, (byte)i);
               // test = test + c;
               

            }
            symPos.Add(Convert.ToChar(160), 32);
            symPos.Add(Convert.ToChar(166), 124);

            
            
           /* for (int i=0;i<sourceCharArray.Length;i++)
            {
                sourceArray[i + lastSymbol + 1] = (byte)(i + lastSymbol + 1);
                
            }*/
        }

        public byte[] GetBytes(string decodeString)
        {
            
            byte[] array = new byte[decodeString.Length];
            
            for (int i = 0; i < decodeString.Length; i++)
            {
                char c = ' ';
                try
                {
                    c = decodeString[i];
                    array[i] = symPos[c];
                }
                catch
                {
                    array[i] = 32;
                }
            }

            return array;

        }

        public string GetString(byte[] array)
        {

            string s = string.Empty;
            char[] cArray = new char[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                cArray[i] = sourceCharArray[array[i]];
                
            }

            return new string(cArray);

        }


    }
}
