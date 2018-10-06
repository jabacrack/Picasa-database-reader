using System.Collections.Generic;
using System.Text;

namespace PicasaDatabaseReader.Core.Fields
{
    public class StringField : FieldBase, IField
    {
        public StringField(string fieldFilepath, HeaderData header) : base(fieldFilepath, header)
        {
            Type = typeof (string);
        }

        public override object ReadValue()
        {
            byte b;
            var result = new List<byte>();
            try
            {
                while ((b = reader.ReadByte()) != 0x0 )
                {
                    result.Add(b);
                }
            }
            catch{}

            return Encoding.UTF8.GetString(result.ToArray());
        }
    }
}