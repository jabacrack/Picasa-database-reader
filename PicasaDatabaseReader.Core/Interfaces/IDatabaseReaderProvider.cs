namespace PicasaDatabaseReader.Core.Interfaces
{
    public interface IDatabaseReaderProvider
    {
        DatabaseReader GetDatabaseReader(string pathToDatabase);
    }
}