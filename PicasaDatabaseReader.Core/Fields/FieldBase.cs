using System;
using System.IO;
using System.IO.Abstractions;
using PicasaDatabaseReader.Core.Extensions;

namespace PicasaDatabaseReader.Core.Fields
{
    public abstract class FieldBase : IField
    {
        protected readonly IFileSystem FileSystem;

        protected FieldBase(Type type, string name, string path, uint count, IFileSystem fileSystem)
        {
            Path = path;
            Type = type;
            Count = count;
            FileSystem = fileSystem;
            Name = name;
        }

        protected IObservable<byte> GetObservable()
        {
            return FileSystem.ReadBytesObservable(Path, 1024, startAt: 20);
        }

        public string Name { get; }
        public string Path { get; }
        public Type Type { get; }
        public uint Count { get; }
        public abstract IObservable<object> GetValues();
    }
}