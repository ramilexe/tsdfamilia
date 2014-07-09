using System;

using System.Collections.Generic;
using System.Text;

namespace TSDServer
{
    public class Boxes
    {

        bool _accepted = false;
        string _dateLabel;

        public string DateLabel
        {
            get { return _dateLabel; }
            set { _dateLabel = value; }
        }
        string _navCodeText;

        public string NavCodeText
        {
            get { return _navCodeText; }
            set { _navCodeText = value; }
        }
        public bool Accepted
        {
            get { return _accepted; }
            set { _accepted = value; }
        }


        string _barcode;

        public string Barcode
        {
            get { return _barcode; }
            set { _barcode = value; }
        }
        string _navCode;

        public string NavCode
        {
            get { return _navCode; }
            set { _navCode = value; }
        }
        List<string> _productsList;

        public List<string> ProductsList
        {
            get { return _productsList; }
            set { _productsList = value; }
        }

        public Boxes()
        {


        }

        public Boxes(string barcode, string navCode,
            string dateLabel, string navCodeText)
        {
            _barcode = barcode;
            _navCode = navCode;
            _dateLabel = dateLabel;
            _navCodeText = navCodeText;

        }


    }

}
