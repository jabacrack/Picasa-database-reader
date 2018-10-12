using System;
using System.IO.Abstractions;

namespace PicasaDatabaseReader.Core.Fields
{
    public class ValueField<TRecord>: FieldBase
        where TRecord: struct
    {
        public ValueField(string name, string path, uint count, IFileSystem fileSystem) 
            : base(typeof(TRecord), name, path, count, fileSystem)
        {
        }

        public override IObservable<object> GetValues()
        {
            throw new NotImplementedException();
        }
    }
}
