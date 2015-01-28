using System;

namespace PicasaDatabaseReader.Fields
{
    interface IField
    {
        string Name { get; }
        Type Type { get; }
        uint Count { get; }
        
        object ReadValue();

        void Close();

    }
}
