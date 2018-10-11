using System;

namespace PicasaDatabaseReader.Core.Fields
{
    public class DateTimeField : FieldBase
    {
        public DateTimeField(string name, string filepath, uint count)
            : base(typeof(DateTime), name, filepath, count)
        {
        }
    }
}
