namespace ScanPlus2.MobilePrinters
{
    using System;

    public abstract class CustomPrinter
    {
        private int _BaudRate;
        private int _Port;

        protected CustomPrinter()
        {
        }

        public abstract bool Connect();
        public abstract string DecryptError(int aError);
        public abstract void Disconnect();
        public abstract int SendBytes(byte[] aBytes);
        public abstract int SendString(string aString);
        public abstract int SendStrings(string[] aStrings);
        public abstract int WaitCompletion(int aLabelCount);

        public int BaudRate
        {
            get
            {
                return this._BaudRate;
            }
            set
            {
                this._BaudRate = value;
            }
        }

        public int Port
        {
            get
            {
                return this._Port;
            }
            set
            {
                this._Port = value;
            }
        }
    }
}

