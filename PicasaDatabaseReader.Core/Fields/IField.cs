using System;
using System.IO.Abstractions;

namespace PicasaDatabaseReader.Core.Fields
{
    public interface IField
    {
        string Name { get; }
        string Path { get; }
        Type Type { get; }
        uint Count { get; }
        IObservable<object> GetValues();
    }
}
