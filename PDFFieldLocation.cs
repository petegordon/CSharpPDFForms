using System;


    /// <summary> 
    /// Summary description for PDFFieldLocation 
    /// </summary> 
    public class PDFFieldLocation
    {

        public PDFFieldLocation()
        {
        }

        private string _fieldName;

        public string fieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        private int _page;
        public int page
        {
            get { return _page; }
            set { _page = value; }
        }

        private float _x1;
        public float x1
        {
            get { return _x1; }
            set { _x1 = value; }
        }

        private float _x2;
        public float x2
        {
            get { return _x2; }
            set { _x2 = value; }
        }

        private float _y1;
        public float y1
        {
            get { return _y1; }
            set { _y1 = value; }
        }

        private float _y2;
        public float y2
        {
            get { return _y2; }
            set { _y2 = value; }
        }
    }
