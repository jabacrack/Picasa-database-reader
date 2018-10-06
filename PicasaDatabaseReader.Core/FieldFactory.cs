using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PicasaDatabaseReader.Core.Fields;

namespace PicasaDatabaseReader.Core
{
    public class FieldFactory
    {
        public static IField CreateField(string fieldFilepath)
        {
            using (var reader = new BinaryReader(File.OpenRead(fieldFilepath)))
            {
                var header = ReadHeader(reader);

                switch (header.FieldType)
                {
                    case 0x0:
                        return new StringField(fieldFilepath, header);
                    case 0x1:
                        return new ValueField<uint>(fieldFilepath, header);
                    case 0x2:
                        return new DateTimeField(fieldFilepath, header);
                    case 0x3:
                        return new ValueField<byte>(fieldFilepath, header);
                    case 0x4:
                        return new ValueField<ulong>(fieldFilepath, header);
                    case 0x5:
                        return new ValueField<ushort>(fieldFilepath, header);
                    case 0x6:
                        return new String2Field(fieldFilepath, header);
                    case 0x7:
                        return new ValueField<uint>(fieldFilepath, header);
                    default:
                        throw new Exception("Unknown field type.");

                }
            }
        }

        private static HeaderData ReadHeader(BinaryReader reader)
        {
            ConstatnsChecker<uint>(reader.ReadUInt32(), 0x3fcccccd); // constant 0x3fcccccd
            var fieldType = reader.ReadUInt16(); //field type
            ConstatnsChecker(reader.ReadUInt16(), 0x1332); // constant 0x1332
            ConstatnsChecker<uint>(reader.ReadUInt32(), 0x00000002); // constant 0x00000002 
            ConstatnsChecker(reader.ReadUInt16(), fieldType); // copy of field type
            ConstatnsChecker(reader.ReadUInt16(), 0x1332); // constant 0x1332
            var count = reader.ReadUInt32(); //number of entries
            return new HeaderData(fieldType, count);
        }

        private static void ConstatnsChecker<T>(T x, T y)
            where T : struct
        {
            if (!EqualityComparer<T>.Default.Equals(x, y))
                throw new Exception("Bad file header.");

        }
    }

    public class HeaderData
    {
        public HeaderData(ushort fieldType, uint recordsCount)
        {
            FieldType = fieldType;
            RecordsCount = recordsCount;
        }

        public ushort FieldType { get; private set; }
        public uint RecordsCount { get; private set; }
    }
}
