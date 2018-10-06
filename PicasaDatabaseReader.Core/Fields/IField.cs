using System;

namespace PicasaDatabaseReader.Core.Fields
{
    public interface IField
    {
        string Name { get; }
        Type Type { get; }
        uint Count { get; }
        
        object ReadValue();

        void Close();
    }
}
