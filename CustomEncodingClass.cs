using System;
using System.Collections.Generic;
using System.Text;

namespace TSDUtils
{
    /// <summary>
    /// собственный класс для перекодирования из (в) кодировки Windows1251 в байтовый массив
    /// На терминале эта кодировка платформой не поддерживается
    /// </summary>
    public class CustomEncodingClass
    {
        //строка всех используемых символов в русской кодировке
        string sourceString = 
"ЂЃ‚ѓ„…†‡€‰Љ‹ЊЌЋЏђ‘’“”•–—™љ›њќћџ ЎўЈ¤Ґ¦§Ё©Є«¬­®Ї°±Ііґµ¶·ё№є»јЅѕїАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюя";
            //"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯабвгдеёжзийклмнопрстуфхцчшщьыъэюя№";
        //byte[] sourceArray;
        //исходный массив всех символов по порядку, включая латинские и спецсимволы.
        char[] sourceCharArray;
        //словарь для получения по символу его кода
        Dictionary<char, byte> symPos = new Dictionary<char, byte>();
        
        private CustomEncodingClass()
        {
            Prepare();
        }
        private static CustomEncodingClass _custEnc = new CustomEncodingClass();
        public static CustomEncodingClass Encoding
        {
            get
            {
                return _custEnc;
            }
        }



    
        /// <summary>
        /// инициализирует все массивы для начала работы с кодировкой
        /// </summary>
        private void Prepare()
        {
            //максимум 255 символов так как преобразуем в байтовый массив
            sourceCharArray = new char[256];
            
            int lastSymbol = 0;//будет последний латинский символ перед кирилицей в Win1251
            //инициализируем массив латинскими символами
            for (int i = 0; i < 256; i++)
            {
                char c = Convert.ToChar(i);
                sourceCharArray[i] = c;
                symPos.Add(c, (byte)i);
                if (c == '~') //последний символ перед кирилицей в Win1251
                {
                    lastSymbol = i;
                    break;
                }

            }
            //инициализируем массив русскими символами
            for (int i = lastSymbol+1; i < lastSymbol+sourceString.Length+1; i++)
            {
                char c = sourceString[i - lastSymbol-1];
                sourceCharArray[i] = c;
                symPos.Add(c, (byte)i);
            }
            //замена этих символов нужными
            //symPos.Add(Convert.ToChar(160), 32);//какой-то символ похож на пробел
            //symPos.Add(Convert.ToChar(166), 124);//какой-то символ похож на |

        }
        /// <summary>
        /// Получить байтовой массив по строке
        /// </summary>
        /// <param name="decodeString">Исходная строка в Unicode</param>
        /// <returns>возвращает байтовый массив в нашей пользовательской кодировке.Длина массива = длине строки</returns>
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
                    //если попадает неизвестный символ будет заменятся пробелом.
                    array[i] = 32;
                }
            }

            return array;

        }
        /// <summary>
        /// Преобразование из массива байт в пользовательской кодировке в строку unicode
        /// </summary>
        /// <param name="array">массив байт в пользовательской кодировке</param>
        /// <returns>строка в unicode</returns>
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
