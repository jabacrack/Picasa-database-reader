using System;

namespace PicasaDatabaseReader.Fields
{
    class DateTimeField : FieldBase
    {
        public DateTimeField(string filepath) : base(filepath)
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
