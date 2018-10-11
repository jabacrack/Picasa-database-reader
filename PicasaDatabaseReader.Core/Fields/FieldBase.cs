using System;
using System.IO;

namespace PicasaDatabaseReader.Core.Fields
{
    public abstract class FieldBase: IField
    {
        protected FieldBase(Type type, string name, string path, uint count)
        {
            Path = path;
            Type = type;
            Count = count;
            Name = name;
        }

        public string Name { get; }
        public string Path { get; }
        public Type Type { get; }
        public uint Count { get; }
    }
}