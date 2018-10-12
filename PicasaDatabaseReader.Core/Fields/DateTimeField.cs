using System;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using PicasaDatabaseReader.Core.Extensions;

namespace PicasaDatabaseReader.Core.Fields
{
    public class DateTimeField : FieldBase
    {
        public DateTimeField(string name, string filepath, uint count, IFileSystem fileSystem)
            : base(typeof(DateTime), name, filepath, count, fileSystem)
        {
        }

        public override IObservable<object> GetValues()
        {
            return GetObservable()
                .Chunk(8)
                .Select<byte[], object>(bytes =>
                {
                    var d = BitConverter.ToDouble(bytes, 0);
                    return DateTime.FromOADate(d);
                });
        }
    }
}
