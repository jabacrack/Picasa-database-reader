using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace PicasaDatabaseReader
{
    internal class StringField : FieldBase, IField
    {
        public StringField(string fieldFilepath) : base(fieldFilepath)
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