using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace PicasaDatabaseReader.Core
{
    public class DatabaseReader
    {
        public const int TableFileHeader = 0x3fcccccd;

        private readonly IFileSystem _fileSystem;
        private readonly string _pathToDatabase;
        private readonly ILogger<DatabaseReader> _logger;

        public DatabaseReader(IFileSystem fileSystem, string pathToDatabase, ILogger<DatabaseReader> logger = null)
        {
            _fileSystem = fileSystem;
            _pathToDatabase = pathToDatabase;
            _logger = logger;

            logger?.LogInformation("Constructed");
        }

        public IObservable<string> GetTableNames()
        {
            EnsureDatabaseExists();

            return _fileSystem.Directory.GetFiles(_pathToDatabase, "*_0")
                .Where(IsTableFile)
                .Select(_fileSystem.Path.GetFileNameWithoutExtension)
                .Select(str => str.Substring(0, str.IndexOf("_0", StringComparison.InvariantCulture)))
                .ToArray()
                .ToObservable();
        }

        private void EnsureDatabaseExists()
        {
            if (!_fileSystem.Directory.Exists(_pathToDatabase))
            {
                throw new InvalidOperationException($"Path to database '{_pathToDatabase}' does not exist.");
            }
        }

        private bool IsTableFile(string filepath)
        {
            using (var reader = new BinaryReader(_fileSystem.File.OpenRead(filepath)))
            {
                try
                {
                    return reader.ReadUInt32() == TableFileHeader;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public IObservable<string> GetFieldFilePaths(string tableName)
        {
            return _fileSystem.Directory
                .GetFiles(_pathToDatabase, $"{tableName}_*.pmp")
                .ToObservable();
        }
    }
}
