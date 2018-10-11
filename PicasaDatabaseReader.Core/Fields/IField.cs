using System;

namespace PicasaDatabaseReader.Core.Fields
{
    public interface IField
    {
        string Name { get; }
        string Path { get; }
        Type Type { get; }
        uint Count { get; }
    }
}
