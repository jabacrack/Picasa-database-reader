using System;
using System.Data;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PicasaDatabaseReader.Core.Extensions;
using PicasaDatabaseReader.Core.Fields;
using PicasaDatabaseReader.Core.Interfaces;
using PicasaDatabaseReader.Core.Scheduling;

namespace PicasaDatabaseReader.Core
{
    public class DatabaseReaderProvider : IDatabaseReaderProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISchedulerProvider _scheduler;

        public DatabaseReaderProvider(IFileSystem fileSystem, ILoggerFactory loggerFactory, ISchedulerProvider scheduler = null)
        {
            _fileSystem = fileSystem;
            _loggerFactory = loggerFactory;
            _scheduler = scheduler;
        }

        public DatabaseReader GetDatabaseReader(string pathToDatabase)
        {
            return new DatabaseReader(_fileSystem, pathToDatabase, _loggerFactory.CreateLogger<DatabaseReader>(), _scheduler);
        }
    }
}
