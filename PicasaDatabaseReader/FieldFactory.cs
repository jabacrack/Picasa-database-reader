using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PicasaDatabaseReader.Fields;

namespace PicasaDatabaseReader
{
    class FieldFactory
    {
        public static IField CreateField(string fieldFilepath)
        {
            using (var reader = new BinaryReader(File.OpenRead(fieldFilepath)))
            {
                var header = DbUtils.ReadHeader(reader);

                switch (header.FieldType)
                {
                    case 0x0:
                        return new StringField(fieldFilepath);
                    case 0x1:
                        return new ValueField<UInt32>(fieldFilepath);
                    case 0x2:
                        return new DateTimeField(fieldFilepath);
                    case 0x3:
                        return new ValueField<Byte>(fieldFilepath);
                    case 0x4:
                        return new ValueField<UInt64>(fieldFilepath);
                    case 0x5:
                        return new ValueField<UInt16>(fieldFilepath);
                    case 0x6:
                        return new String2Field(fieldFilepath);
                    case 0x7:
                        return new ValueField<UInt32>(fieldFilepath);
                    default:
                        throw new Exception("Unknown field type.");

                }
            }
        }

       
    }
}
