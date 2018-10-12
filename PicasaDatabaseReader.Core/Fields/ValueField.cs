using System;
using System.IO.Abstractions;
using System.Reactive.Linq;
using PicasaDatabaseReader.Core.Extensions;

namespace PicasaDatabaseReader.Core.Fields
{
    public abstract class ValueField<TRecord>: FieldBase
        where TRecord: struct
    {
        public ValueField(string name, string path, uint count, IFileSystem fileSystem) 
            : base(typeof(TRecord), name, path, count, fileSystem)
        {
        }

        public override IObservable<object> GetValues()
        {
            return GetObservable()
                .Chunk(ByteSize)
                .Select<byte[], object>(bytes => FromBytes(bytes));
        }

        protected abstract TRecord FromBytes(byte[] bytes);

        protected abstract int ByteSize { get; }
    }

    public class ByteField : ValueField<byte>
    {
        public ByteField(string name, string path, uint count, IFileSystem fileSystem)
            : base(name, path, count, fileSystem)
        {
        }

        protected override int ByteSize => 1;

        protected override byte FromBytes(byte[] bytes)
        {
            return bytes[0];
        }
    }

    public class UShortField : ValueField<ushort>
    {
        public UShortField(string name, string path, uint count, IFileSystem fileSystem)
            : base(name, path, count, fileSystem)
        {
        }

        protected override int ByteSize => 2;

        protected override ushort FromBytes(byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes, 0);
        }
    }

    public class UIntField : ValueField<uint>
    {
        public UIntField(string name, string path, uint count, IFileSystem fileSystem)
            : base(name, path, count, fileSystem)
        {
        }

        protected override int ByteSize => 4;

        protected override uint FromBytes(byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, 0);
        }
    }

    public class ULongField : ValueField<ulong>
    {
        public ULongField(string name, string path, uint count, IFileSystem fileSystem)
            : base(name, path, count, fileSystem)
        {
        }

        protected override int ByteSize => 8;

        protected override ulong FromBytes(byte[] bytes)
        {
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
