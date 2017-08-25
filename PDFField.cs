using System;


    /// <summary> 
    /// Summary description for PDFField 
    /// </summary> 
    public class PDFField
    {
        public PDFField(string name, string val)
        {
            this._Name = name;
            this._Value = val;
            this._Type = PDFFieldType.TEXT;
        }
        public PDFField(string name, byte[] imageValue)
        {
            this._Name = name;
            this._imageValue = imageValue;
            this._Type = PDFFieldType.IMAGE;
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Value;

        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
        private byte[] _imageValue;
        public byte[] ImageValue
        {
            get 
            {
                if (this.Type == PDFFieldType.IMAGE)
                    return _imageValue;
                else
                    throw new Exception("PDFField is not of Type IMAGE");
            }
            set { _imageValue = value; } 
        }

        private string _Type;

        public string Type
        {
            get { return _Type; }
            set { _Type = value.ToUpper(); }
        }
    }

public class PDFFieldType
{
    public static string IMAGE = "IMAGE";
    public static string TEXT = "TEXT";
}