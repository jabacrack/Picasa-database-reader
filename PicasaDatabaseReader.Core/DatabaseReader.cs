using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PicasaDatabaseReader.Core.Extensions;
using PicasaDatabaseReader.Core.Fields;
using PicasaDatabaseReader.Core.Interfaces;
using PicasaDatabaseReader.Core.Scheduling;

namespace PicasaDatabaseReader.Core
{
    public class DatabaseReader : IDatabaseReader
    {
        public const int TableFileHeader = 0x3FCCCCCD;
        private const string ThumbindexTableName = "thumbindex";

        private readonly IFileSystem _fileSystem;
        private readonly string _pathToDatabase;
        private readonly ILogger<DatabaseReader> _logger;
        private readonly ISchedulerProvider _scheduler;

        public DatabaseReader(IFileSystem fileSystem, string pathToDatabase, ILogger<DatabaseReader> logger = null, ISchedulerProvider scheduler = null)
        {
            _fileSystem = fileSystem;
            _pathToDatabase = pathToDatabase;
            _logger = logger ?? NullLogger<DatabaseReader>.Instance;
            _scheduler = scheduler ?? new SchedulerProvider();

            _logger.LogDebug("Constructed pathToDatabase:{pathToDatabase}");
        }

        public IObservable<string> GetTableNames()
        {
            return Observable.Defer(() =>
            {
                _logger.LogTrace("GetTableNames");

                EnsureDatabaseExists();

                return _fileSystem.Directory.GetFiles(_pathToDatabase, "*_0")
                    .ToObservable()
                    .Where(IsTableFile)
                    .Select(_fileSystem.Path.GetFileNameWithoutExtension)
                    .Select(str => str.Substring(0, str.IndexOf("_0", StringComparison.InvariantCulture)))
                    .Append(ThumbindexTableName);
            }).SubscribeOn(_scheduler.ThreadPool);
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
                    // 0x3FCCCCCD
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

        public IObservable<IField> GetFields(string tableName)
        {
            return GetFieldFilePaths(tableName)
                .Select<string, Task<IField>>(async fieldPath =>
                {
                    var fieldStream = _fileSystem.ReadBytesObservable(fieldPath, 1024, 20);

                    byte[] typeResultBytes = null;
                    byte[] typeResultConfirmBytes = null;
                    byte[] countBytes = null;

                    await fieldStream
                        .MatchNextItems<byte>("constant 0x3fcccccd", 0xCD, 0xCC, 0xCC, 0x3F)
                        .CaptureNextItems(2, bytes => typeResultBytes = bytes)
                        .MatchNextItems<byte>("constant 0x1332", 0x32, 0x13)
                        .MatchNextItems<byte>("constant 0x00000002", 0x02, 0x00, 0x00, 0x00)
                        .CaptureNextItems(2, bytes => typeResultConfirmBytes = bytes)
                        .MatchNextItems<byte>("constant 0x1332", 0x32, 0x13)
                        .CaptureNextItems(4, bytes => countBytes = bytes)
                        .LastOrDefaultAsync();

                    var typeResult = BitConverter.ToUInt16(typeResultBytes, 0);
                    var typeResultConfirm = BitConverter.ToUInt16(typeResultConfirmBytes, 0);
                    var count = BitConverter.ToUInt32(countBytes, 0);

                    if (typeResult != typeResultConfirm)
                    {
                        throw new Exception();
                    }

                    var name = _fileSystem.Path.GetFileNameWithoutExtension(fieldPath);
                    name = name.Substring(name.IndexOf("_", StringComparison.InvariantCultureIgnoreCase) + 1);

                    switch (typeResult)
                    {
                        case 0x0:
                            return new StringField(name, fieldPath, count, _fileSystem);
                        case 0x1:
                            return new UIntField(name, fieldPath, count, _fileSystem);
                        case 0x2:
                            return new DateTimeField(name, fieldPath, count, _fileSystem);
                        case 0x3:
                            return new ByteField(name, fieldPath, count, _fileSystem);
                        case 0x4:
                            return new ULongField(name, fieldPath, count, _fileSystem);
                        case 0x5:
                            return new UShortField(name, fieldPath, count, _fileSystem);
                        case 0x6:
                            return new String2Field(name, fieldPath, count, _fileSystem);
                        case 0x7:
                            return new UIntField(name, fieldPath, count, _fileSystem);
                        default:
                            throw new Exception("Unknown field type.");
                    }
                })
                .Concat();
        }

        public IObservable<DataTable> GetDataTable(string tableName)
        {
            if (tableName == ThumbindexTableName)
            {
                return GetThumbIndexDataTable(tableName);
            }

            return Observable.FromAsync(async () =>
            {
                var fields = await GetFields(tableName)
                    .ToArray()
                    .FirstAsync();

                var dataTable = new DataTable(tableName);

                var dataColumns = fields
                    .Select(field => new DataColumn(field.Name, field.Type) { AllowDBNull = true })
                    .ToArray();

                dataTable.Columns.AddRange(dataColumns);

                var maxFieldCount = fields.Select(field => field.Count).Max();

                var observables = fields
                    .Select(field => field
                        .GetValues()
                        .Select(o => o ?? DBNull.Value)
                        .Concat(Observable.Repeat(DBNull.Value, (int)(maxFieldCount - field.Count))))
                    .ToArray();

                await Observable.Zip(observables)
                    .Select((list, i) => new { index = i, list })
                    .Do((arg) =>
                    {
                        var dataRow = dataTable.NewRow();
                        dataRow.ItemArray = arg.list.ToArray();
                        dataTable.Rows.Add(dataRow);
                    })
                    .LastOrDefaultAsync();

                return dataTable;
            });
        }

        private IObservable<DataTable> GetThumbIndexDataTable(string tableName)
        {
            return GetThumbIndex().ToArray()
                .Select(datas =>
                {
                    var dataTable = new DataTable(tableName);

                    dataTable.Columns.Add(new DataColumn("Index", typeof(uint)));
                    dataTable.Columns.Add(new DataColumn("Content", typeof(string)));

                    foreach (var data in datas)
                    {
                        dataTable.Rows.Add(data.Index, data.Content);
                    }

                    return dataTable;
                });
        }

        public IObservable<IndexData> GetThumbIndex()
        {
            return Observable.Create<IndexData>(async observer =>
            {
                var indexFilePath = _fileSystem.Path.Combine(_pathToDatabase, "thumbindex.db");
                var headerStream = _fileSystem.ReadBytesObservable(indexFilePath, 1024, 8);

                byte[] countBytes = null;

                await headerStream
                    .MatchNextItems<byte>("constant 0x66664640", 0x66, 0x66, 0x46, 0x40)
                    .CaptureNextItems(4, bytes => countBytes = bytes)
                    .LastOrDefaultAsync();

                var recordCount = BitConverter.ToUInt32(countBytes, 0);
                uint recordIndex = 0;

                var fileStream = _fileSystem.ReadBytesObservable(indexFilePath, 1024, null, 8);

                var hasCapturedContent = false;
                var contentStringBytes = new List<byte>();

                const int uselessCountdownMax = 26;
                var uselessCountdown = uselessCountdownMax;

                var indexBytes = new List<byte>();

                return fileStream.Subscribe(Observer.Create<byte>(
                    onNext: b =>
                    {
                        if (!hasCapturedContent)
                        {
                            if (b == 0x00)
                            {
                                hasCapturedContent = true;
                            }
                            else
                            {
                                contentStringBytes.Add(b);
                            }
                        }
                        else if (uselessCountdown != 0)
                        {
                            uselessCountdown--;
                        }
                        else
                        {
                            indexBytes.Add(b);

                            if (indexBytes.Count == 4)
                            {
                                var indexValue = BitConverter.ToUInt32(indexBytes.ToArray(), 0);
                                indexBytes.Clear();

                                var contentString = Encoding.ASCII.GetString(contentStringBytes.ToArray());
                                contentStringBytes.Clear();

                                hasCapturedContent = false;
                                uselessCountdown = uselessCountdownMax;
                                
                                observer.OnNext(new IndexData(recordIndex++, contentString, indexValue));

                                if (recordIndex == recordCount)
                                {
                                    observer.OnCompleted();
                                }
                            }
                        }
                    }, 
                    onError: observer.OnError, 
                    onCompleted: () =>
                    {
                        if (recordIndex != recordCount)
                        {
                            observer.OnError(new InvalidOperationException("Unexpected source completion"));
                        }
                    }));
            });
        }
    }

    public class IndexData
    {
        public uint Position { get; }
        public string Content { get; }
        public uint Index { get; }

        public IndexData(uint position, string content, uint index)
        {
            Position = position;
            Content = content;
            Index = index;
        }
    }
}
