namespace ScanPlus2.MobilePrinters
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ToshibaTECBT : CustomPrinter
    {
        public override bool Connect()
        {
            return ConnectToPrinter(base.Port, base.BaudRate);
        }

        [DllImport("ToshibaTecBTWin32.dll")]
        private static extern bool ConnectToPrinter(int aComNumber, int aBaudRate);
        public override string DecryptError(int aError)
        {
            switch (aError)
            {
                case 0:
                    return "Операция завершена успешно";

                case 1:
                    return "Ошибка соединения";

                case 2:
                    return "Порт не открыт";

                case 3:
                    return "Ошибка записи в порт";

                case 4:
                    return "Ошибка чтения из порта";

                case 5:
                    return "Открыта крышка принтера";

                case 6:
                    return "Принтер в процессе печати";

                case 7:
                    return "Неверный формат команды";

                case 8:
                    return "Заело бумагу";

                case 9:
                    return "Конец этикетки";

                case 11:
                    return "Поломка печатающей головки";

                case 12:
                    return "Плишком высокая или слишком низкая температура";

                case 13:
                    return "Батарея разряжена";

                case 14:
                    return "Ошибка энергонезависимой памяти";
            }
            return "Неизвестная ошибка";
        }

        public override void Disconnect()
        {
            FreePrinter();
        }

        [DllImport("ToshibaTecBTWin32.dll")]
        private static extern void FreePrinter();
        [DllImport("ToshibaTecBTWin32.dll")]
        private static extern int ReceiveResponse(int aLabelCount);
        [DllImport("ToshibaTecBTWin32.dll")]
        private static extern int SendBuffer(byte[] aBuffer, int aBufLen);

        public override int SendBytes(byte[] aBytes)
        {
            return SendBuffer(aBytes, aBytes.Length);
        }

        public override int SendString(string aString)
        {
            byte[] array = new byte[aString.Length + 3];
            array[0] = 0x1b;
            array[array.Length - 1] = 0;
            array[array.Length - 2] = 10;
            Encoding.GetEncoding(0x4e3).GetBytes(aString).CopyTo(array, 1);
            return this.SendBytes(array);
        }

        public override int SendStrings(string[] aStrings)
        {
            int num = 0;
            foreach (string str in aStrings)
            {
                num += str.Length;
            }
            byte[] destinationArray = new byte[num + (3 * aStrings.Length)];
            int index = 0;
            foreach (string str2 in aStrings)
            {
                destinationArray[index] = 0x1b;
                destinationArray[(index + str2.Length) + 2] = 0;
                destinationArray[(index + str2.Length) + 1] = 10;
                Array.Copy(Encoding.GetEncoding(0x4e3).GetBytes(str2), 0, destinationArray, index + 1, str2.Length);
                index += str2.Length + 3;
            }
            return this.SendBytes(destinationArray);
        }

        public override int WaitCompletion(int aLabelCount)
        {
            return ReceiveResponse(aLabelCount);
        }
    }
}

