using System;
using System.Data;

namespace PicasaDatabaseReader.Core.Interfaces
{
    public interface IDatabaseReader
    {
        IObservable<string> GetTableNames();
        IObservable<DataTable> GetDataTable(string tableName);
    }
}
