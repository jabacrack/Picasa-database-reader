using System;

namespace PicasaDatabaseReader.Core.Fields
{
    public class DateTimeField : FieldBase
    {
        public DateTimeField(string filepath, HeaderData header) : base(filepath, header)
        {
            Type = typeof (DateTime);
        }

        public override object ReadValue()
        {
            try
            {
                return DateTime.FromOADate(reader.ReadDouble());
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
