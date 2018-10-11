using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PicasaDatabaseReader.Core.Fields;

namespace PicasaDatabaseReader.Core
{
    public class FieldFactory
    {
        public static IObservable<byte> ReadBytes(string path, int bufferSize = 0x1000)
        {
            return Observable.Using(
                () =>
                {
                    Trace.WriteLine("Blah");
                    return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                }, 
                fileStream =>
                {
                    return Observable
                        .Generate(0, i => true, i => i++, i => i)
                        .Select(i =>
                        {
                            var bytes1 = new byte[bufferSize];
                            return Observable.FromAsync(async () =>
                            {
                                var count1 = await fileStream.ReadAsync(bytes1, i * bufferSize, bufferSize);
                                return new {count = count1, bytes = bytes1};
                            });
                        })
                        .Concat()
                        .TakeWhile(arg => arg.count > 0)
                        .SelectMany(arg => arg.bytes);
                });
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
