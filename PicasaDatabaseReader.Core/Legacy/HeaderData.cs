using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicasaDatabaseReader
{
    public class HeaderData
    {
        public HeaderData(UInt16 fieldType, UInt32 recordsCount)
        {
            FieldType = fieldType;
            RecordsCount = recordsCount;
        }

        public UInt16 FieldType { get; private set; }
        public UInt32 RecordsCount { get; private set; }
    }
}
